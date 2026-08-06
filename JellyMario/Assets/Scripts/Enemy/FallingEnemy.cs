// 밟으면 떨어지는 땅
using UnityEngine;

namespace JellyMario.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class FallingEnemy : EnemyBase
    {
        private Rigidbody2D _rigidbody;

        protected override void Awake()
        {
            base.Awake();

            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
        }

        ////2초 뒤에 떨어지는지 확인용
        //private void Start()
        //{
        //    Invoke(nameof(Hit), 2f);
        //}

        public override void Hit()
        {
            base.Hit();

            _rigidbody.bodyType = RigidbodyType2D.Dynamic;
        }
    }
}