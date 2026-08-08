using UnityEngine;

namespace JellyMario.CameraSystem
{
    // 같은 게임 오브젝트에 여러 개의 FollowCamera 컴포넌트가 붙는 것을 방지
    [DisallowMultipleComponent]
    public class FollowCamera : MonoBehaviour
    {
        [Header("추적 설정")]
        [SerializeField] private Transform target;
        [SerializeField] private float followSpeed = 8f;

        [Header("화면 여백 설정")]
        [SerializeField, Min(0f)] private float horizontalMargin = 1f;
        [SerializeField, Min(0f)] private float verticalMargin = 2f;

        // 초기화
        private void Awake()
        {
            if (target == null)
            {
                Debug.LogWarning("카메라가 따라갈 대상이 등록되지 않았습니다.", this);
                
                enabled = false;
                return;
            }
        }

        private void LateUpdate()
        {
            // 플레이어와 카메라 중심 사이의 거리
            float distanceX = target.position.x - transform.position.x;
            float distanceY = target.position.y - transform.position.y;

            float targetX = transform.position.x;
            float targetY = transform.position.y;

            // 플레이어가 여백을 넘어간 경우
            if (distanceX > horizontalMargin)
                targetX = target.position.x - horizontalMargin;
            else if (distanceX < -horizontalMargin)
                targetX = target.position.x + horizontalMargin;
            if (distanceY > verticalMargin)
                targetY = target.position.y - verticalMargin;
            else if (distanceY < -verticalMargin)
                targetY = target.position.y + verticalMargin;

            float followRatio = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);

            float newX = Mathf.Lerp(transform.position.x, targetX, followRatio);
            float newY = Mathf.Lerp(transform.position.y, targetY, followRatio);

            // X축과 Y축만 변경하고 Z축은 유지
            transform.position = new Vector3(newX, newY, transform.position.z);
        }
    }
}