using UnityEngine;
using Woodcutter.Core;
using Woodcutter.Player;

namespace Woodcutter.World
{
    public class HazardDamage : MonoBehaviour
    {
        [Header("Damage Settings")]
        [SerializeField] private int damage = 1;

        [Header("Debug")]
        [SerializeField] private bool showDebugLog = true;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (showDebugLog)
            {
                Debug.Log(
                    $"[HazardDamage] Trigger detected: {other.name}",
                    gameObject
                );
            }

            PlayerHealth player = FindPlayerHealth(other);

            if (player == null)
            {
                if (showDebugLog)
                {
                    Debug.LogWarning(
                        $"[HazardDamage] Không tìm thấy PlayerHealth trên {other.name}.",
                        gameObject
                    );
                }

                return;
            }

            if (player.IsDead)
            {
                if (showDebugLog)
                {
                    Debug.Log(
                        "[HazardDamage] Player đã chết, không gây thêm sát thương.",
                        gameObject
                    );
                }

                return;
            }

            DamageData damageData = new DamageData(
                damage,
                DamageType.Normal,
                player.transform.position,
                Vector2.zero,
                0f,
                gameObject
            );

            bool damaged = player.TakeDamage(damageData);

            if (showDebugLog)
            {
                Debug.Log(
                    $"[HazardDamage] Đã gây {damage} sát thương cho Player. Thành công: {damaged}",
                    gameObject
                );
            }
        }

        private PlayerHealth FindPlayerHealth(Collider2D other)
        {
            // Trường hợp Rigidbody2D và PlayerHealth nằm trên cùng object
            if (other.attachedRigidbody != null)
            {
                PlayerHealth player =
                    other.attachedRigidbody.GetComponent<PlayerHealth>();

                if (player != null)
                    return player;
            }

            // Trường hợp Collider nằm trên object con của Player
            PlayerHealth parentPlayer =
                other.GetComponentInParent<PlayerHealth>();

            if (parentPlayer != null)
                return parentPlayer;

            // Trường hợp PlayerHealth nằm trên chính Collider object
            return other.GetComponent<PlayerHealth>();
        }
    }
}