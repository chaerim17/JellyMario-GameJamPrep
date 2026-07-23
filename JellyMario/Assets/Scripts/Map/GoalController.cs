using JellyMario.Player;
using UnityEngine;

namespace JellyMario.Map
{
    public class GoalController : MonoBehaviour
    {
        // 플레이어가 골인했을 때 호출
        public virtual void ClearStage()
        {
            // TODO : 스테이지 클리어 처리
            // 예)
            // ManagersHub.Game.ClearStage();
            // ManagersHub.Scene.LoadScene("NextStage");
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 플레이어인지 확인
            PlayerBase player = collision.GetComponent<PlayerBase>();

            if (player == null)
                return;

            ClearStage();
        }
    }
}