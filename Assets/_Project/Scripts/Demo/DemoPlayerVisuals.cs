using UnityEngine;
using Woodcutter.Player;

namespace Woodcutter.Demo
{
    /// <summary>
    /// CHỈ DÙNG CHO SCENE DEMO. Trên main, CharacterContext.SetVisualState vẫn tô màu debug
    /// (xanh khi chạy, vàng khi nhảy...). Component này đổi sang phát animation tiều phu
    /// (AC_Woodcutter) theo state hiện tại và trả màu sprite về trắng, để demo trông như game thật.
    /// </summary>
    [RequireComponent(typeof(CharacterContext))]
    public class DemoPlayerVisuals : MonoBehaviour
    {
        public RuntimeAnimatorController controller;

        CharacterContext ctx;
        Animator anim;
        SpriteRenderer sr;
        string current;

        void Awake()
        {
            ctx = GetComponent<CharacterContext>();
            sr = GetComponent<SpriteRenderer>();
            anim = GetComponent<Animator>();
            if (anim == null) anim = gameObject.AddComponent<Animator>();
            if (controller != null) anim.runtimeAnimatorController = controller;
        }

        void Start()
        {
            if (ctx.StateMachine != null) ctx.StateMachine.OnStateChanged += OnStateChanged;
            Play("Idle");
        }

        void OnDestroy()
        {
            if (ctx != null && ctx.StateMachine != null) ctx.StateMachine.OnStateChanged -= OnStateChanged;
        }

        void OnStateChanged(Woodcutter.Core.IState s)
        {
            if (s == ctx.IdleState) Play("Idle");
            else if (s == ctx.MoveState) Play("Move");
            else if (s == ctx.JumpState) Play("Jump");
            else if (s == ctx.FallState) Play("Fall");
            else if (s == ctx.RollState) Play("Roll");
            // AttackState: chọn theo màu debug ở LateUpdate
        }

        void LateUpdate()
        {
            if (sr == null) return;
            Color c = sr.color;
            if (c.r > 1.01f) return; // đang nháy trắng (hit flash)

            if (ctx.StateMachine != null && ctx.StateMachine.CurrentState == ctx.AttackState)
            {
                // Màu debug AttackState: trắng = đang nạp / đánh trên không, hồng = nhịp 1, đỏ = nhịp 2, tím = đòn nặng
                if (Near(c, new Color(1f, 0.5f, 0.5f))) Play("Attack", true);
                else if (Near(c, new Color(1f, 0f, 0f))) Play("Attack2", true);
                else if (Near(c, new Color(0.5f, 0f, 0.5f))) Play("HeavyAttack", true);
                else if (current != "Attack" && current != "Attack2" && current != "HeavyAttack")
                    Play(ctx.CheckGrounded() ? "HeavyAttack" : "Attack");
            }
            if (c != Color.white && c.a > 0f) sr.color = Color.white;
        }

        static bool Near(Color a, Color b) =>
            Mathf.Abs(a.r - b.r) < 0.02f && Mathf.Abs(a.g - b.g) < 0.02f && Mathf.Abs(a.b - b.b) < 0.02f;

        void Play(string state, bool restart = false)
        {
            if (anim == null || anim.runtimeAnimatorController == null) return;
            if (current == state && !restart) return;
            int h = Animator.StringToHash(state);
            if (!anim.HasState(0, h)) return;
            current = state;
            anim.Play(h, 0, 0f);
        }
    }
}
