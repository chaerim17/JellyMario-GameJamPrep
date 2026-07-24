using JellyMario.Player;
using UnityEngine;

namespace JellyMario.Map
{
    public class DeathZone : MonoBehaviour
    {
        // 플레이어 사망 처리
        public virtual void OnPlayerDead()
        {
            // TODO : 플레이어 사망 처리
            // 예)
            // ManagersHub.Player.CurrentPlayer.Die();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 플레이어인지 확인
            PlayerBase player = collision.GetComponent<PlayerBase>();

            if (player == null)
                return;

            OnPlayerDead();
        }
    }
}