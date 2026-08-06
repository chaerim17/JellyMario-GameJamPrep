//표면 굴러다니는 적
using UnityEngine;

namespace JellyMario.Enemy
{
    public class SurfaceEnemy : EnemyBase
    {
        [SerializeField] private Transform[] movePoints;
        [SerializeField] private float moveSpeed = 3f;

        private int _currentIndex;

        protected override void Awake()
        {
            base.Awake();

            Move();
        }

        protected override void HandleMovement()
        {
            if (movePoints == null || movePoints.Length == 0)
                return;

            Transform target = movePoints[_currentIndex];

            transform.position = Vector3.MoveTowards(
                transform.position,
                target.position,
                moveSpeed * Time.deltaTime);

            if (Vector3.Distance(
                transform.position,
                target.position) < 0.05f)
            {
                _currentIndex++;

                if (_currentIndex >= movePoints.Length)
                    _currentIndex = 0;
            }
        }
    }
}