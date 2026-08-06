//표면 굴러다니는 적
using UnityEngine;

namespace JellyMario.Enemy
{
    public class SurfaceEnemy : EnemyBase
    {
        [SerializeField] private Transform[] movePoints;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private int direction = 1;

        private int _currentIndex;

        protected override void Awake()
        {
            base.Awake();

            Move();
        }

        protected override void HandleMovement()
        {
            transform.Translate(
                Vector2.left *
                (direction * moveSpeed * Time.deltaTime));
        }
    }
}