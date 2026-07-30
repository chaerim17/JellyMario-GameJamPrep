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
        [SerializeField, Min(0f)] private float horizontalMargin = 4f;

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
            // 플레이어와 카메라 중심 사이의 X축 거리
            float distanceX = target.position.x - transform.position.x;

            float targetX = transform.position.x;

            // 플레이어가 오른쪽 여백을 넘어간 경우
            if (distanceX > horizontalMargin)
                targetX = target.position.x - horizontalMargin;
            // 플레이어가 왼쪽 여백을 넘어간 경우
            else if (distanceX < -horizontalMargin)
                targetX = target.position.x + horizontalMargin;

            float followRatio = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);

            float newX = Mathf.Lerp(transform.position.x, targetX, followRatio);

            // X축만 변경하고 Y축과 Z축은 유지
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }
    }
}