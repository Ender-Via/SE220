using System.Collections;
using UnityEngine;
using Woodcutter.Player;

namespace Woodcutter.World
{
    public class FallToSafePoint : MonoBehaviour
    {
        [SerializeField] private Transform safePoint;
        [SerializeField] private float returnDelay = 0.15f;

        private bool isReturning;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isReturning)
                return;

            PlayerHealth player = other.attachedRigidbody != null
                ? other.attachedRigidbody.GetComponent<PlayerHealth>()
                : other.GetComponentInParent<PlayerHealth>();

            if (player == null)
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

            yield return new WaitForSeconds(returnDelay);

            Rigidbody2D body = player.GetComponent<Rigidbody2D>();

            if (body != null)
                body.linearVelocity = Vector2.zero;

            player.transform.position = safePoint.position;
            player.ReviveFull();

            isReturning = false;
        }
    }
}