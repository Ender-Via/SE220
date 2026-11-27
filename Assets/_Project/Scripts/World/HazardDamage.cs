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
        [SerializeField] private bool showDebugLog = false;

        private void OnTriggerStay2D(Collider2D other)
        {
            PlayerHealth player = FindPlayerHealth(other);

            if (player == null || player.IsDead)
                return;

            DamageData damageData = new DamageData(
                damage,
                DamageType.Normal,
                player.transform.position,
                Vector2.zero,
                0f,
                gameObject
            );

            bool damaged = player.TakeDamage(damageData);

            if (showDebugLog && damaged)
            {
                Debug.Log(
                    $"[HazardDamage] Đã gây {damage} sát thương cho Player.",
                    gameObject
                );
            }
        }

        private PlayerHealth FindPlayerHealth(Collider2D other)
        {
            if (other.attachedRigidbody != null)
            {
                PlayerHealth player =
                    other.attachedRigidbody.GetComponent<PlayerHealth>();

                if (player != null)
                    return player;
            }

            PlayerHealth parentPlayer =
                other.GetComponentInParent<PlayerHealth>();

            if (parentPlayer != null)
                return parentPlayer;

            return other.GetComponent<PlayerHealth>();
        }
    }
}