using UnityEngine;
using Woodcutter.Player;

namespace Woodcutter.Demo
{
    /// <summary>
    /// CHỈ DÙNG CHO SCENE DEMO. Phát animation tiều phu (AC_Woodcutter) theo state của CharacterContext
    /// và theo đòn mà DemoPlayerCombat báo (nhịp 1 / nhịp 2 / đòn nặng / rìu lửa).
    /// Không đọc màu debug: chỉ trả màu sprite về trắng vì CharacterContext.SetVisualState trên main vẫn tô màu.
    /// </summary>
    [RequireComponent(typeof(CharacterContext))]
    public class DemoPlayerVisuals : MonoBehaviour
    {
        public RuntimeAnimatorController controller;

        CharacterContext ctx;
        DemoPlayerCombat combat;
        Animator anim;
        SpriteRenderer sr;
        string current;

        void Awake()
        {
            ctx = GetComponent<CharacterContext>();
            combat = GetComponent<DemoPlayerCombat>();
            sr = GetComponent<SpriteRenderer>();
            anim = GetComponent<Animator>();
            if (anim == null) anim = gameObject.AddComponent<Animator>();
            if (controller != null) anim.runtimeAnimatorController = controller;
        }

        void Start()
        {
            if (ctx.StateMachine != null) ctx.StateMachine.OnStateChanged += OnStateChanged;
            if (combat != null)
            {
                combat.OnAttackFired += OnAttackFired;
                combat.OnChargeStarted += OnChargeStarted;
            }
            Play("Idle");
        }

        void OnDestroy()
        {
            if (ctx != null && ctx.StateMachine != null) ctx.StateMachine.OnStateChanged -= OnStateChanged;
            if (combat != null)
            {
                combat.OnAttackFired -= OnAttackFired;
                combat.OnChargeStarted -= OnChargeStarted;
            }
        }

        void OnStateChanged(Woodcutter.Core.IState s)
        {
            if (s == ctx.IdleState) Play("Idle");
            else if (s == ctx.MoveState) Play("Move");
            else if (s == ctx.JumpState) Play("Jump");
            else if (s == ctx.FallState) Play("Fall");
            else if (s == ctx.RollState) Play("Roll");
            // AttackState: chờ DemoPlayerCombat báo đòn cụ thể
        }

        // Dưới đất AttackState bắt đầu bằng tư thế lấy đà (giống SetVisualState("HeavyAttack") trong AttackState.Enter)
        void OnChargeStarted() => Play("HeavyAttack", true);

        void OnAttackFired(DemoPlayerCombat.AttackKind kind)
        {
            switch (kind)
            {
                case DemoPlayerCombat.AttackKind.Combo1: Play("Attack", true); break;
                case DemoPlayerCombat.AttackKind.Combo2: Play("Attack2", true); break;
                case DemoPlayerCombat.AttackKind.Heavy: Play("HeavyAttack", true); break;
                case DemoPlayerCombat.AttackKind.FireAxe: Play("Attack", true); break;
            }
        }

        void LateUpdate()
        {
            if (sr == null) return;
            Color c = sr.color;
            // Bỏ màu debug của SetVisualState; giữ nguyên khi đang nháy (alpha 0 lúc nhấp nháy bất tử)
            if (c != Color.white && c.a > 0f) sr.color = Color.white;
        }

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
