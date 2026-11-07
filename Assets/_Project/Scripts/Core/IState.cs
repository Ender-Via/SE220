namespace Woodcutter.Core
{
    /// <summary>
    /// Vòng đời chuẩn của một trạng thái (State) trong hệ thống FSM.
    /// </summary>
    public interface IState
    {
        /// <summary>Gọi 1 lần duy nhất khi bước vào trạng thái.</summary>
        void Enter();

        /// <summary>Gọi 1 lần duy nhất khi rời khỏi trạng thái để dọn dẹp.</summary>
        void Exit();

        /// <summary>Chạy trong Update() - xử lý logic, timer, điều kiện chuyển đổi.</summary>
        void LogicUpdate();

        /// <summary>Chạy trong FixedUpdate() - xử lý tương tác vật lý (Rigidbody2D).</summary>
        void PhysicsUpdate();
    }
}
