using System.Collections;
using UnityEngine;
using Woodcutter.Core;
using Woodcutter.Player;

namespace Woodcutter.World
{
    public class FallToSafePoint : MonoBehaviour
    {
        [Header("Safe Point")]
        [SerializeField] private Transform safePoint;

        [Header("Fall Settings")]
        [SerializeField] private int fallDamage = 1;
        [SerializeField] private float returnDelay = 0.15f;

        private bool isReturning;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isReturning)
                return;

            PlayerHealth player = FindPlayerHealth(other);

            if (player == null || player.IsDead)
                return;

            StartCoroutine(ReturnPlayer(player));
        }

        private IEnumerator ReturnPlayer(PlayerHealth player)
        {
            isReturning = true;

            if (safePoint == null)
            {
                Debug.LogError(
                    "[FallToSafePoint] Chưa gán Safe Point.",
                    this
                );

                isReturning = false;
                yield break;
            }

            DamageData damageData = new DamageData(
                fallDamage,
                DamageType.Normal,
                player.transform.position,
                Vector2.zero,
                0f,
                gameObject
            );

            player.TakeDamage(damageData);

            // Nếu cú rơi làm Player hết máu,
            // để RespawnController đưa Player về checkpoint.
            if (player.IsDead)
            {
                isReturning = false;
                yield break;
            }

            yield return new WaitForSeconds(returnDelay);

            // Player có thể chết bởi nguồn khác trong thời gian chờ.
            if (player == null || player.IsDead)
            {
                isReturning = false;
                yield break;
            }

            Rigidbody2D body = player.GetComponent<Rigidbody2D>();

            if (body != null)
                body.linearVelocity = Vector2.zero;

            player.transform.position = safePoint.position;

            // Không gọi ReviveFull vì sẽ hồi đầy máu.
            isReturning = false;
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

            return other.GetComponentInParent<PlayerHealth>();
        }
    }
}