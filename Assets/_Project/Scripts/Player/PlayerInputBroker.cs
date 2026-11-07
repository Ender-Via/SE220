using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Woodcutter.Player
{
    /// <summary>
    /// Adapter chuyển tín hiệu từ Unity New Input System thành C# Events thuần chủng.
    /// Lắng nghe trực tiếp tầng callback phần cứng, đạt độ trễ cực thấp (Zero polling).
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputBroker : MonoBehaviour, IPlayerInputEvents
    {
        public event Action<Vector2> OnMoveInput;
        public event Action<Vector2> OnLookInput;
        public event Action OnJumpPressed;
        public event Action OnJumpReleased;
        public event Action OnAttackPressed;
        public event Action OnAttackReleased;
        public event Action OnDodgePressed;
        public event Action OnFireAxePressed;
        public event Action OnDashPressed;
        public event Action OnPlantSeedPressed;
        public event Action OnInteractPressed;
        public event Action OnPausePressed;

        private PlayerInput playerInput;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction jumpAction;
        private InputAction attackAction;
        private InputAction dodgeAction;
        private InputAction fireAxeAction;
        private InputAction dashAction;
        private InputAction plantSeedAction;
        private InputAction interactAction;
        private InputAction pauseAction;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            BindActions();
        }

        private void BindActions()
        {
            if (playerInput == null || playerInput.actions == null)
                return;

            var actions = playerInput.actions;
            moveAction = actions.FindAction("Move", false);
            lookAction = actions.FindAction("Look", false);
            jumpAction = actions.FindAction("Jump", false);
            attackAction = actions.FindAction("Attack", false);
            dodgeAction = actions.FindAction("Dodge", false);
            fireAxeAction = actions.FindAction("FireAxe", false);
            dashAction = actions.FindAction("Dash", false);
            plantSeedAction = actions.FindAction("PlantSeed", false);
            interactAction = actions.FindAction("Interact", false);
            pauseAction = actions.FindAction("Pause", false);
        }

        private void OnEnable()
        {
            if (moveAction == null)
                BindActions();

            if (moveAction != null)
            {
                moveAction.performed += HandleMove;
                moveAction.canceled += HandleMove;
            }

            if (lookAction != null)
            {
                lookAction.performed += HandleLook;
                lookAction.canceled += HandleLook;
            }

            if (jumpAction != null)
            {
                jumpAction.performed += HandleJumpPressed;
                jumpAction.canceled += HandleJumpReleased;
            }

            if (attackAction != null)
            {
                attackAction.performed += HandleAttackPressed;
                attackAction.canceled += HandleAttackReleased;
            }

            if (dodgeAction != null) dodgeAction.performed += HandleDodge;
            if (fireAxeAction != null) fireAxeAction.performed += HandleFireAxe;
            if (dashAction != null) dashAction.performed += HandleDash;
            if (plantSeedAction != null) plantSeedAction.performed += HandlePlantSeed;
            if (interactAction != null) interactAction.performed += HandleInteract;
            if (pauseAction != null) pauseAction.performed += HandlePause;
        }

        private void OnDisable()
        {
            // QUY TẮC ĐỐI XỨNG: Hủy đăng ký toàn bộ sự kiện để triệt tiêu triệt để rò rỉ bộ nhớ (Memory Leak)
            if (moveAction != null)
            {
                moveAction.performed -= HandleMove;
                moveAction.canceled -= HandleMove;
            }

            if (lookAction != null)
            {
                lookAction.performed -= HandleLook;
                lookAction.canceled -= HandleLook;
            }

            if (jumpAction != null)
            {
                jumpAction.performed -= HandleJumpPressed;
                jumpAction.canceled -= HandleJumpReleased;
            }

            if (attackAction != null)
            {
                attackAction.performed -= HandleAttackPressed;
                attackAction.canceled -= HandleAttackReleased;
            }

            if (dodgeAction != null) dodgeAction.performed -= HandleDodge;
            if (fireAxeAction != null) fireAxeAction.performed -= HandleFireAxe;
            if (dashAction != null) dashAction.performed -= HandleDash;
            if (plantSeedAction != null) plantSeedAction.performed -= HandlePlantSeed;
            if (interactAction != null) interactAction.performed -= HandleInteract;
            if (pauseAction != null) pauseAction.performed -= HandlePause;
        }

        private void HandleMove(InputAction.CallbackContext ctx) => OnMoveInput?.Invoke(ctx.ReadValue<Vector2>());
        private void HandleLook(InputAction.CallbackContext ctx) => OnLookInput?.Invoke(ctx.ReadValue<Vector2>());
        private void HandleJumpPressed(InputAction.CallbackContext ctx) => OnJumpPressed?.Invoke();
        private void HandleJumpReleased(InputAction.CallbackContext ctx) => OnJumpReleased?.Invoke();
        private void HandleAttackPressed(InputAction.CallbackContext ctx) => OnAttackPressed?.Invoke();
        private void HandleAttackReleased(InputAction.CallbackContext ctx) => OnAttackReleased?.Invoke();
        private void HandleDodge(InputAction.CallbackContext ctx) => OnDodgePressed?.Invoke();
        private void HandleFireAxe(InputAction.CallbackContext ctx) => OnFireAxePressed?.Invoke();
        private void HandleDash(InputAction.CallbackContext ctx) => OnDashPressed?.Invoke();
        private void HandlePlantSeed(InputAction.CallbackContext ctx) => OnPlantSeedPressed?.Invoke();
        private void HandleInteract(InputAction.CallbackContext ctx) => OnInteractPressed?.Invoke();
        private void HandlePause(InputAction.CallbackContext ctx) => OnPausePressed?.Invoke();
    }
}
