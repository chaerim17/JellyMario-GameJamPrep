//튀어나오는 장애물
using System.Collections;
using UnityEngine;

namespace JellyMario.Enemy
{
    public class SpawnEnemy : EnemyBase
    {
        [SerializeField] private float riseHeight = 1f;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float waitTime = 1.5f;

        private Vector3 _hiddenPos;
        private Vector3 _showPos;

        protected override void Awake()
        {
            base.Awake();

            _hiddenPos = transform.position;
            _showPos = _hiddenPos + Vector3.up * riseHeight;
        }

        private void Start()
        {
            StartCoroutine(MoveRoutine());
        }

        private IEnumerator MoveRoutine()
        {
            while (true)
            {
                // 올라오기
                while (Vector3.Distance(transform.position, _showPos) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(
                        transform.position,
                        _showPos,
                        moveSpeed * Time.deltaTime);

                    yield return null;
                }

                yield return new WaitForSeconds(waitTime);

                // 내려가기
                while (Vector3.Distance(transform.position, _hiddenPos) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(
                        transform.position,
                        _hiddenPos,
                        moveSpeed * Time.deltaTime);

                    yield return null;
                }

                yield return new WaitForSeconds(waitTime);
            }
        }
    }
}