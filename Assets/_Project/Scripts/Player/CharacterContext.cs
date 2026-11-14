using UnityEngine;
using Woodcutter.Core;
using Woodcutter.Data;
using Woodcutter.Player.States;

namespace Woodcutter.Player
{
    /// <summary>
    /// Thực thể Context: Đóng vai trò là Trọng tài điều phối nhân vật.
    /// Nắm giữ các Component vật lý, nạp ScriptableObject thông số, khởi tạo FSM, và nối dây Event từ Broker sang State.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class CharacterContext : MonoBehaviour
    {
        [Header("Components & References")]
        public Rigidbody2D Rigidbody;
        public SpriteRenderer SpriteRenderer;
        public Animator Animator;
        [SerializeField] private PlayerInputBroker inputBroker;
        [SerializeField] private PlayerStatsSO stats;
        [SerializeField] private PlayerCombatStatsSO combatStats;
        [SerializeField] private AbilitiesStatsSO abilitiesStats;

        [Header("Ground Detection")]
        public Transform GroundCheckPoint;
        public Vector2 GroundCheckDimensions = new Vector2(0.8f, 0.15f);
        public LayerMask GroundLayerMask;

        // Dữ liệu thời gian thực
        public PlayerStatsSO Stats => stats;
        public PlayerCombatStatsSO CombatStats => combatStats;
        public AbilitiesStatsSO AbilitiesStats => abilitiesStats;
        public Vector2 CurrentInputVector { get; private set; }
        public bool IsFacingRight { get; private set; } = true;

        // Quản lý tấn công trên không: Trong không trung chỉ được tấn công 1 lần duy nhất
        public bool CanAirAttack { get; set; } = true;

        // Quản lý Jump Buffer (SRS FR-MOV-03: Bộ đệm lệnh nhảy 0.1s)
        public float JumpBufferTimer { get; private set; }

        // Cỗ máy FSM
        public StateMachine StateMachine { get; private set; }

        // BỘ NHỚ ĐỆM TRẠNG THÁI (State Caching - Zero GC Spikes trong suốt vòng lặp game)
        public IdleState IdleState { get; private set; }
        public MoveState MoveState { get; private set; }
        public JumpState JumpState { get; private set; }
        public FallState FallState { get; private set; }
        public AttackState AttackState { get; private set; }

        // Buffer kiểm tra va chạm mặt đất (Không sinh rác bộ nhớ)
        private readonly Collider2D[] groundHitBuffer = new Collider2D[8];

        private void Awake()
        {
            if (Rigidbody == null) Rigidbody = GetComponent<Rigidbody2D>();
            if (SpriteRenderer == null) SpriteRenderer = GetComponent<SpriteRenderer>();
            if (inputBroker == null) inputBroker = GetComponent<PlayerInputBroker>();

            // Bắt lỗi nếu chưa gán ScriptableObject 
            if (stats == null) Debug.LogError($"[CharacterContext] BẮT BUỘC gán PlayerStatsSO trên {gameObject.name}!");
            if (combatStats == null) Debug.LogError($"[CharacterContext] BẮT BUỘC gán PlayerCombatStatsSO trên {gameObject.name}!");
            if (abilitiesStats == null) Debug.LogError($"[CharacterContext] BẮT BUỘC gán AbilitiesStatsSO trên {gameObject.name}!");

            // 1. Khởi tạo State Machine
            StateMachine = new StateMachine();

            // 2. Cấp phát bộ nhớ cho các State ĐÚNG 1 LẦN DUY NHẤT trong Awake
            IdleState = new IdleState(this, StateMachine);
            MoveState = new MoveState(this, StateMachine);
            JumpState = new JumpState(this, StateMachine);
            FallState = new FallState(this, StateMachine);
            AttackState = new AttackState(this, StateMachine);
        }

        private void Start()
        {
            // Bắt đầu vòng đời nhân vật ở trạng thái phù hợp với vị trí lúc xuất hiện
            if (CheckGrounded())
                StateMachine.Initialize(IdleState);
            else
                StateMachine.Initialize(FallState);
        }

        private void OnEnable()
        {
            // 3. ĐĂNG KÝ SỰ KIỆN: Chuyển tiếp Event từ Broker vào State hiện tại
            if (inputBroker != null)
            {
                inputBroker.OnMoveInput += HandleMoveInputReceived;
                inputBroker.OnJumpPressed += ForwardJumpPressedToCurrentState;
                inputBroker.OnJumpReleased += ForwardJumpReleasedToCurrentState;
                inputBroker.OnAttackPressed += ForwardAttackPressedToCurrentState;
            }
        }

        private void OnDisable()
        {
            // 4. HỦY ĐĂNG KÝ: Bảo vệ bộ nhớ, chống rò rỉ (Zero dangling references)
            if (inputBroker != null)
            {
                inputBroker.OnMoveInput -= HandleMoveInputReceived;
                inputBroker.OnJumpPressed -= ForwardJumpPressedToCurrentState;
                inputBroker.OnJumpReleased -= ForwardJumpReleasedToCurrentState;
                inputBroker.OnAttackPressed -= ForwardAttackPressedToCurrentState;
            }
        }

        private void Update()
        {
            // Đếm ngược Jump Buffer (SRS FR-MOV-03)
            if (JumpBufferTimer > 0f)
            {
                JumpBufferTimer -= Time.deltaTime;
            }

            // Hồi lại lượt tấn công trên không khi chạm đất
            if (CheckGrounded())
            {
                CanAirAttack = true;
            }

            // Ủy thác toàn bộ logic cho State hiện tại
            StateMachine.LogicUpdate();
        }

        private void FixedUpdate()
        {
            // Ủy thác toàn bộ tương tác vật lý cho State hiện tại
            StateMachine.PhysicsUpdate();
        }

        // ================================================================
        // QUẢN LÝ JUMP BUFFER
        // ================================================================
        public void RegisterJumpBuffer()
        {
            JumpBufferTimer = stats.jumpBufferTime;
        }

        public bool HasBufferedJump()
        {
            return JumpBufferTimer > 0f;
        }

        public void ConsumeJumpBuffer()
        {
            JumpBufferTimer = 0f;
        }

        // ================================================================
        // TÍNH TOÁN VẬN TỐC THEO GIA TỐC & GIẢM TỐC (SRS 5.1)
        // ================================================================
        public float ComputeHorizontalVelocity(float inputX)
        {
            float targetSpeed = inputX * stats.maxRunSpeed;
            float currentSpeed = Rigidbody.linearVelocityX;

            // Xác định xem đang tăng tốc theo hướng muốn đi hay đang giảm tốc / quay đầu
            bool isAccelerating = Mathf.Abs(targetSpeed) > 0.01f && 
                (Mathf.Abs(currentSpeed) < 0.01f || Mathf.Sign(targetSpeed) == Mathf.Sign(currentSpeed));

            float rate = isAccelerating 
                ? (stats.maxRunSpeed / Mathf.Max(0.001f, stats.accelerationTime))
                : (stats.maxRunSpeed / Mathf.Max(0.001f, stats.decelerationTime));

            return Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.fixedDeltaTime);
        }

        // ================================================================
        // CÁC HÀM CẦU NỐI SỰ KIỆN (EVENT DISPATCHERS)
        // ================================================================
        private void HandleMoveInputReceived(Vector2 input)
        {
            CurrentInputVector = input;
            OrientFacingDirection(input.x);
        }

        private void ForwardJumpPressedToCurrentState()
        {
            if (StateMachine.CurrentState is CharacterBaseState baseState)
                baseState.HandleJumpPressed();
        }

        private void ForwardJumpReleasedToCurrentState()
        {
            if (StateMachine.CurrentState is CharacterBaseState baseState)
                baseState.HandleJumpReleased();
        }

        private void ForwardAttackPressedToCurrentState()
        {
            if (StateMachine.CurrentState is CharacterBaseState baseState)
                baseState.HandleAttackPressed();
        }

        // ================================================================
        // CÁC HÀM TIỆN ÍCH DÙNG CHUNG (SHARED UTILITIES)
        // ================================================================
        public bool CheckGrounded()
        {
            if (GroundCheckPoint == null)
            {
                return false;
            }

            // LayerMask ưu tiên lớp Ground (layer 7) hoặc Default (layer 0)
            LayerMask mask = GroundLayerMask.value == 0 ? LayerMask.GetMask("Ground", "Default") : GroundLayerMask;
            if (mask.value == 0) mask = ~0;

            int hitCount = Physics2D.OverlapBoxNonAlloc(
                GroundCheckPoint.position,
                GroundCheckDimensions,
                0f,
                groundHitBuffer,
                mask
            );

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = groundHitBuffer[i];
                if (hit == null || hit.isTrigger)
                    continue;

                // TUYỆT ĐỐI BỎ QUA chính bản thân nhân vật và mọi collider con
                if (hit.gameObject == gameObject || hit.transform.IsChildOf(transform))
                    continue;

                return true;
            }

            return false;
        }

        private void OrientFacingDirection(float horizontalInput)
        {
            if ((horizontalInput > 0.05f && !IsFacingRight) || (horizontalInput < -0.05f && IsFacingRight))
            {
                IsFacingRight = !IsFacingRight;
                Vector3 currentScale = transform.localScale;
                currentScale.x *= -1f;
                transform.localScale = currentScale;
            }
        }

        public void SetVisualState(string stateName, Color color)
        {
            if (SpriteRenderer != null)
            {
                SpriteRenderer.color = color;
            }

            if (Animator != null)
            {
                Animator.SetBool("isMoving", stateName == "Move");
            }
        }

        private void OnDrawGizmos()
        {
            if (GroundCheckPoint != null)
            {
                Gizmos.color = Application.isPlaying && CheckGrounded() ? Color.green : Color.red;
                Gizmos.DrawWireCube(GroundCheckPoint.position, GroundCheckDimensions);
            }
        }

        public void HandleLandingTransition()
        {
            if (HasBufferedJump())
            {
                ConsumeJumpBuffer();
                StateMachine.ChangeState(JumpState);
            }
            else if (Mathf.Abs(CurrentInputVector.x) > 0.05f)
            {
                StateMachine.ChangeState(MoveState);
            }
            else
            {
                StateMachine.ChangeState(IdleState);
            }
        }
    }
}
