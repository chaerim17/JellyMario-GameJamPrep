using UnityEngine;

namespace JellyMario.Enemy
{
    public class BasicEnemy : EnemyBase
    {
        [Header("기본 설정")]
        [SerializeField] private Transform startPoint;
        [SerializeField, Min(0f)] private float speed = 2f;

        protected override void Awake()
        {
            base.Awake();

            ChangeState(EnemyState.Move);
        }

        protected override void HandleMovement()
        {
            Move();
        }

        public override void Move()
        {
            base.Move();

            transform.position += Vector3.left * speed * Time.deltaTime;
        }
    }
}
