using UnityEngine;

namespace Woodcutter.Demo
{
    /// <summary>
    /// CHỈ DÙNG CHO SCENE DEMO. Camera bám theo người chơi theo trục X (giữ Y cố định),
    /// thay cho hệ phòng + Cinemachine của game thật để demo chạy liền một mạch.
    /// </summary>
    public class DemoCameraFollow : MonoBehaviour
    {
        public Transform target;
        public float smooth = 6f;
        public float minX = 0f, maxX = 200f;
        public float y = 4.5f;
        public float lookAhead = 2f;

        void LateUpdate()
        {
            if (target == null) return;
            float x = Mathf.Clamp(target.position.x + lookAhead, minX, maxX);
            var p = transform.position;
            p.x = Mathf.Lerp(p.x, x, 1f - Mathf.Exp(-smooth * Time.unscaledDeltaTime));
            p.y = y;
            transform.position = p;
        }
    }
}
