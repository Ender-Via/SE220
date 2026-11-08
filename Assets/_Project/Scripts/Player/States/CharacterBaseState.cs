using Woodcutter.Core;

namespace Woodcutter.Player.States
{
    /// <summary>
    /// Lớp cơ sở cho toàn bộ các State của nhân vật.
    /// Nắm giữ tham chiếu tới Context, StateMachine và các Event Hook tiếp nhận sự kiện.
    /// </summary>
    public abstract class CharacterBaseState : IState
    {
        protected CharacterContext context;
        protected StateMachine stateMachine;

        public CharacterBaseState(CharacterContext context, StateMachine stateMachine)
        {
            this.context = context;
            this.stateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void LogicUpdate() { }
        public virtual void PhysicsUpdate() { }

        // Các móc sự kiện (Event Hooks):
        // Mặc định rỗng; State con nào quan tâm sự kiện nào thì Override hàm đó.
        public virtual void HandleJumpPressed() { }
        public virtual void HandleJumpReleased() { }
        public virtual void HandleAttackPressed() { }
        public virtual void HandleAttackReleased() { }
        public virtual void HandleDodgePressed() { }
    }
}
