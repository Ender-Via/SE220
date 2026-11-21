using System.Collections;
using UnityEngine;
using Woodcutter.Player;

namespace Woodcutter.Demo
{
    /// <summary>
    /// CHỈ DÙNG CHO SCENE DEMO. Vật nhặt được bằng E: Đá Đỏ (mở Rìu lửa, +1 vạch máu, hồi màu vùng xung quanh)
    /// hoặc Mẩu giấy (hiện nội dung, E để đóng).
    /// </summary>
    public class DemoPickup : MonoBehaviour
    {
        public enum Kind { RedStone, Note }
        public Kind kind;
        public float range = 1.6f;
        [TextArea(4, 10)] public string noteText;
        [Tooltip("Vùng hồi màu khi nhặt đá (tùy chọn).")]
        public DemoColorZone colorZone;

        Transform player;
        bool taken;
        Vector3 basePos;

        void Start()
        {
            var input = FindAnyObjectByType<PlayerInputBroker>();
            if (input != null) player = input.transform;
            basePos = transform.position;
            DemoUI.RegisterPickup(this); // phím E do DemoUI xử lý tập trung
        }

        public float DistanceToPlayer => player != null ? Vector2.Distance(player.position + Vector3.up * 0.9f, transform.position) : float.MaxValue;
        bool InRange => DistanceToPlayer < range;
        public bool CanUse => !taken && isActiveAndEnabled && InRange;

        void Update()
        {
            if (taken) return;
            transform.position = basePos + Vector3.up * Mathf.Sin(Time.time * 2.5f) * 0.12f;
            if (InRange) DemoUI.Prompt(transform.position + Vector3.up * 1.2f, kind == Kind.Note ? "E: đọc mẩu giấy" : "E: nhặt viên đá");
        }

        /// <summary>Gọi bởi DemoUI khi người chơi bấm E và vật này gần nhất.</summary>
        public void Use()
        {
            if (!CanUse) return;

            if (kind == Kind.Note)
            {
                DemoUI.ShowNote(noteText);
                return; // mẩu giấy đọc lại được nhiều lần
            }

            taken = true;
            var combat = player.GetComponent<DemoPlayerCombat>();
            if (combat) combat.fireAxeUnlocked = true;
            var hp = player.GetComponent<DemoPlayerHealth>();
            if (hp) hp.AddMaxHp(1);
            DemoUI.HasRedStone = true;
            DemoUI.Banner("Nhận ĐÁ ĐỎ!  Năng lực mới: RÌU LỬA (phím L)  ·  Máu tối đa +1", 4f);
            if (colorZone) colorZone.Restore(transform.position);
            StartCoroutine(Vanish());
        }

        IEnumerator Vanish()
        {
            var sr = GetComponentInChildren<SpriteRenderer>();
            Vector3 s = transform.localScale;
            for (float t = 0; t < 0.5f; t += Time.deltaTime)
            {
                transform.localScale = s * (1f + t * 2f);
                if (sr) { var c = sr.color; c.a = 1f - t * 2f; sr.color = c; }
                yield return null;
            }
            gameObject.SetActive(false);
        }
    }
}
