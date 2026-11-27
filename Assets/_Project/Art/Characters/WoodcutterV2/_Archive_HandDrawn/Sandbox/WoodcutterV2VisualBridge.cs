using System.Reflection;
using UnityEngine;
using Woodcutter.Core;
using Woodcutter.Player;

namespace Woodcutter.Sandbox
{
    /// <summary>
    /// CHỈ DÙNG ĐỂ CHƠI THỬ (Sandbox_WoodcutterV2Play). Nối FSM của CharacterContext với Animator của PF_WoodcutterV2
    /// mà không sửa code gameplay: đọc state hiện tại; nhịp đòn trong AttackState đọc từ các biến private
    /// của nó (reflection); bị đánh lấy từ PlayerHealth.OnDamaged.
    /// Khi nhóm nối Animator chính thức vào CharacterContext thì bỏ file này.
    /// </summary>
    public class WoodcutterV2VisualBridge : MonoBehaviour
    {
        [SerializeField] CharacterContext context;
        [SerializeField] Animator animator;

        [Header("Thời gian trộn tư thế khi đổi animation (giây)")]
        [SerializeField] float toLocomotion = 0.12f;
        [SerializeField] float toAttack1 = 0.04f;
        [SerializeField] float toAttack2 = 0.05f;
        [SerializeField] float toCharge = 0.12f;
        [SerializeField] float toHeavy = 0.02f;
        [SerializeField] float toAir = 0.08f;
        [SerializeField] float toRoll = 0.03f;
        [SerializeField] float toHit = 0.02f;
        [SerializeField] float toDie = 0.05f;
        [Tooltip("Đòn đánh chạy hết clip mà code vẫn giữ trạng thái đánh thì trộn về Idle sau bấy nhiêu giây")]
        [SerializeField] float attackEndToIdle = 0.1f;
        [Tooltip("Thời gian giữ animation Hit (giây)")]
        [SerializeField] float hitDuration = 0.25f;

        [Header("Phím thử: H = bị đánh (chỉ hình), G = chết / hồi sinh (gọi code thật)")]
        [SerializeField] bool enableTestKeys = true;

        Woodcutter.VFX.AfterimageTrail trail;
        PlayerHealth health;
        string current;        // state đang yêu cầu Animator phát
        string finishedAttack; // đòn vừa chạy hết clip, đang hiển thị Idle trong lúc chờ code
        float hitTimer;

        static FieldInfo fCharging, fHeavyFired, fComboStep;

        void Awake()
        {
            var t = typeof(Woodcutter.Player.States.AttackState);
            const BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Instance;
            fCharging = t.GetField("isCharging", bf);
            fHeavyFired = t.GetField("hasFiredHeavyAttack", bf);
            fComboStep = t.GetField("currentComboStep", bf);
            if (fCharging == null || fHeavyFired == null || fComboStep == null)
                Debug.LogWarning("[WoodcutterV2VisualBridge] AttackState đã đổi tên biến, không đọc được nhịp đòn.", this);
        }

        void OnEnable()
        {
            if (context != null) health = context.GetComponent<PlayerHealth>();
            if (health != null) health.OnDamaged += HandleDamaged;
        }

        void OnDisable()
        {
            if (health != null) health.OnDamaged -= HandleDamaged;
        }

        void HandleDamaged(DamageData _)
        {
            if (health != null && health.IsDead) return;
            PlayHit();
        }

        void PlayHit()
        {
            hitTimer = hitDuration;
            animator.CrossFadeInFixedTime("Hit", toHit, 0, 0f);
            current = "Hit";
        }

        void Update()
        {
            if (!enableTestKeys || animator == null || context == null || context.StateMachine == null) return;
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null) return;

            if (kb.gKey.wasPressedThisFrame)
            {
                if (context.StateMachine.CurrentState == context.DeathState) context.Revive();
                else context.EnterDeathState();
            }
            if (kb.hKey.wasPressedThisFrame && context.StateMachine.CurrentState != context.DeathState)
                PlayHit();
        }

        void LateUpdate()
        {
            if (context == null || animator == null || context.StateMachine == null) return;
            var state = context.StateMachine.CurrentState;

            if (trail == null) trail = animator.GetComponent<Woodcutter.VFX.AfterimageTrail>();
            if (trail != null) trail.SetEmitting(state == context.RollState);

            if (state == context.DeathState)
            {
                hitTimer = 0f;
                if (current != "Die") { current = "Die"; animator.CrossFadeInFixedTime("Die", toDie, 0, 0f); }
                return;
            }
            if (hitTimer > 0f)
            {
                hitTimer -= Time.deltaTime;
                if (hitTimer > 0f) return;
                current = null;
            }

            string next; float fade;
            bool isAttack = state == context.AttackState;
            if (isAttack)
            {
                object a = context.AttackState;
                bool heavy = fHeavyFired != null && (bool)fHeavyFired.GetValue(a);
                bool charging = fCharging != null && (bool)fCharging.GetValue(a);
                int step = fComboStep != null ? (int)fComboStep.GetValue(a) : 1;
                if (heavy) { next = "HeavyAttack"; fade = toHeavy; }
                else if (charging) { next = "HeavyCharge"; fade = toCharge; }
                else if (step == 2) { next = "Attack2"; fade = toAttack2; }
                else { next = "Attack1"; fade = toAttack1; }
            }
            else if (state == context.MoveState) { next = "Run"; fade = toLocomotion; }
            else if (state == context.JumpState) { next = "Jump"; fade = toAir; }
            else if (state == context.FallState) { next = "Fall"; fade = toAir; }
            else if (state == context.RollState) { next = "Dash"; fade = toRoll; }
            else { next = "Idle"; fade = toLocomotion; }

            if (!isAttack) finishedAttack = null;

            // Đòn đã chạy hết clip mà code vẫn đang ở AttackState: hiển thị Idle, chỉ đổi khi sang đòn khác
            if (isAttack && next == finishedAttack) return;
            if (isAttack && next != "HeavyCharge" && current == next)
            {
                var info = animator.GetCurrentAnimatorStateInfo(0);
                if (info.IsName(next) && !animator.IsInTransition(0) && info.normalizedTime >= 1f)
                {
                    finishedAttack = next;
                    current = "Idle";
                    animator.CrossFadeInFixedTime("Idle", attackEndToIdle, 0, 0f);
                    return;
                }
            }

            if (next == current) return;
            current = next;
            animator.CrossFadeInFixedTime(next, fade, 0, 0f);
        }
    }
}
