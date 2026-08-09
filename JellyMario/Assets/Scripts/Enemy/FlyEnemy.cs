//벌
using JellyMario.Jelly;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace JellyMario.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class FlyEnemy : EnemyBase
    {
        [SerializeField, Min(0f)] private float moveSpeed = 2f;
        [SerializeField] private int direction = 1;

        private Rigidbody2D _rigidbody;
        private Collider2D _bodyCollider;
        private Tilemap[] _groundTilemaps;
        private Vector2 _lastSafePosition;
        private float _lastDirectionChangeTime = float.NegativeInfinity;

        protected override void Awake()
        {
            base.Awake();

            _rigidbody = GetComponent<Rigidbody2D>();
            _bodyCollider = GetComponent<Collider2D>();
            _rigidbody.gravityScale = 0f;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            CacheGroundTilemaps();
            _lastSafePosition = _rigidbody.position;

            ApplyMovement();
            Move();
        }

        private void FixedUpdate()
        {
            if (IsInsideGround(_rigidbody.position))
            {
                _rigidbody.position = _lastSafePosition;
                ReverseDirection();
                return;
            }

            _lastSafePosition = _rigidbody.position;
            ApplyMovement();
        }

        private void CacheGroundTilemaps()
        {
            JellySurfaceWave[] surfaces = Object.FindObjectsByType<JellySurfaceWave>(FindObjectsInactive.Include);
            _groundTilemaps = new Tilemap[surfaces.Length];

            for (int index = 0; index < surfaces.Length; index++)
            {
                _groundTilemaps[index] = surfaces[index] != null
                    ? surfaces[index].GetComponent<Tilemap>()
                    : null;
            }
        }

        private bool IsInsideGround(Vector2 bodyPosition)
        {
            if (_bodyCollider == null || _groundTilemaps == null)
                return false;

            Bounds bounds = _bodyCollider.bounds;
            Vector2 offset = bodyPosition - _rigidbody.position;
            Vector2 center = (Vector2)bounds.center + offset;
            Vector2 extent = (Vector2)bounds.extents * 0.8f;
            Vector2[] samplePoints =
            {
                center,
                center + new Vector2(extent.x, 0f),
                center + new Vector2(-extent.x, 0f),
                center + new Vector2(0f, extent.y),
                center + new Vector2(0f, -extent.y),
                center + new Vector2(extent.x, extent.y),
                center + new Vector2(extent.x, -extent.y),
                center + new Vector2(-extent.x, extent.y),
                center + new Vector2(-extent.x, -extent.y)
            };

            foreach (Tilemap groundTilemap in _groundTilemaps)
            {
                if (groundTilemap == null)
                    continue;

                foreach (Vector2 samplePoint in samplePoints)
                {
                    Vector3Int cell = groundTilemap.WorldToCell(samplePoint);

                    if (groundTilemap.HasTile(cell))
                        return true;
                }
            }

            return false;
        }

        private void ApplyMovement()
        {
            Vector2 velocity = _rigidbody.linearVelocity;
            velocity.x = -direction * moveSpeed;
            velocity.y = 0f;
            _rigidbody.linearVelocity = velocity;

            if (CachedSpriteRenderer != null)
                CachedSpriteRenderer.flipX = velocity.x > 0f;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            JellySurfaceWave surfaceWave =
                collision.gameObject.GetComponentInParent<JellySurfaceWave>();

            if (surfaceWave == null)
                return;

            if (collision.contactCount > 0)
            {
                ContactPoint2D contact = collision.GetContact(0);
                float impactSpeed = Mathf.Max(moveSpeed, 
                    Mathf.Abs(Vector2.Dot(collision.relativeVelocity, contact.normal)));

                surfaceWave.PlayRipple(contact.point, contact.normal, impactSpeed);
            }

            ReverseDirection();
        }

        private void ReverseDirection()
        {
            if (Time.time - _lastDirectionChangeTime < 0.1f)
                return;

            _lastDirectionChangeTime = Time.time;
            direction *= -1;
            ApplyMovement();
        }
    }
}