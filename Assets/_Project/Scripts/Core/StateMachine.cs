using System;

namespace Woodcutter.Core
{
    /// <summary>
    /// Bộ máy trạng thái độc lập (Engine-Agnostic), tái sử dụng cho Player, Enemies, Bosses.
    /// </summary>
    public class StateMachine
    {
        public IState CurrentState { get; private set; }

        /// <summary>Sự kiện bắn ra khi trạng thái thay đổi (phục vụ Debug, UI hoặc Logging).</summary>
        public event Action<IState> OnStateChanged;

        /// <summary>
        /// Khởi động máy trạng thái với trạng thái ban đầu.
        /// </summary>
        public void Initialize(IState startingState)
        {
            if (startingState == null)
                throw new ArgumentNullException(nameof(startingState), "Trạng thái khởi tạo không được null!");

            CurrentState = startingState;
            CurrentState.Enter();
            OnStateChanged?.Invoke(CurrentState);
        }

        /// <summary>
        /// Chuyển đổi sang trạng thái mới theo chu trình: Exit cũ -> Đổi con trỏ -> Enter mới.
        /// </summary>
        public void ChangeState(IState newState)
        {
            if (newState == null || newState == CurrentState)
                return;

            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter();
            OnStateChanged?.Invoke(CurrentState);
        }

        public void LogicUpdate() => CurrentState?.LogicUpdate();
        public void PhysicsUpdate() => CurrentState?.PhysicsUpdate();
    }
}
