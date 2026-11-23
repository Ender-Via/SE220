using UnityEngine;
using Woodcutter.Core;

namespace Woodcutter.Enemies
{
    /// <summary>
    /// Đạn bắn ra từ quái đánh xa (Quạ xám ném đá tuyết, Tinh linh sương bắn đạn băng) theo SRS 6.1.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EnemyProjectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private float speed = 7f;
        [SerializeField] private int damage = 1;
        [SerializeField] private float lifetime = 4f;
        [SerializeField] private float knockbackForce = 5f;
        [SerializeField] private LayerMask obstacleLayer;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Vector2 moveDirection;
        private float lifeTimer;
        private bool hasHit;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;

            if (obstacleLayer.value == 0)
            {
                obstacleLayer = LayerMask.GetMask("Ground", "Default");
            }
        }

        public void Initialize(Vector2 direction, float projSpeed, int projDamage, float projLifetime = 4f)
        {
            moveDirection = direction.normalized;
            speed = projSpeed;
            damage = projDamage;
            lifetime = projLifetime;
            lifeTimer = 0f;
            hasHit = false;

            // Xoay đầu đạn theo hướng bay
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Update()
        {
            if (hasHit) return;

            // Di chuyển đạn
            transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);

            lifeTimer += Time.deltaTime;
            if (lifeTimer >= lifetime)
            {
                DestroySelf();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasHit) return;

            // Bỏ qua nếu chạm chính quái bắn ra hoặc đạn khác
            if (other.GetComponent<EnemyBase>() != null || other.GetComponent<EnemyProjectile>() != null)
                return;

            // 1. Kiểm tra va chạm người chơi
            var damageable = other.GetComponent<IDamageable>() ?? other.GetComponentInParent<IDamageable>();
            if (damageable != null && other.CompareTag("Player"))
            {
                hasHit = true;
                Vector2 knockDir = moveDirection;
                var damageData = new DamageData(damage, DamageType.Normal, transform.position, knockDir, knockbackForce, gameObject);
                damageable.TakeDamage(damageData);
                DestroySelf();
                return;
            }

            // Fallback: nếu Player dùng DemoPlayerHealth trong Sandbox
            var demoHealth = other.GetComponent<Demo.DemoPlayerHealth>() ?? other.GetComponentInParent<Demo.DemoPlayerHealth>();
            if (demoHealth != null)
            {
                hasHit = true;
                demoHealth.TakeDamage(damage, transform.position);
                DestroySelf();
                return;
            }

            // 2. Chạm mặt đất hoặc tường
            if ((obstacleLayer.value & (1 << other.gameObject.layer)) != 0 && !other.isTrigger)
            {
                hasHit = true;
                DestroySelf();
            }
        }

        private void DestroySelf()
        {
            // Có thể spawn VFX vỡ băng tại đây nếu có
            Destroy(gameObject);
        }
    }
}
