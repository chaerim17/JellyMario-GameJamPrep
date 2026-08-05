//벌
using UnityEngine;

namespace JellyMario.Enemy
{
    public class FlyEnemy : EnemyBase
    {
        [SerializeField] private float moveRange = 3f;
        [SerializeField] private float moveSpeed = 2f;

        private Vector3 _startPosition;

        protected override void Awake()
        {
            base.Awake();

            _startPosition = transform.position;

            Move();
        }

        protected override void HandleMovement()
        {
            float x = Mathf.Sin(Time.time * moveSpeed) * moveRange;

            transform.position =
                _startPosition + Vector3.right * x;
        }
    }
}