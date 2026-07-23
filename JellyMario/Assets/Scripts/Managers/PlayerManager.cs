using JellyMario.Core;
using JellyMario.Player;

namespace JellyMario.Managers
{
    // 플레이어를 관리하는 매니저
    public class PlayerManager : Singleton<PlayerManager>
    {
        // 현재 플레이어
        private PlayerBase _player;

        // 현재 플레이어 읽기
        public PlayerBase Player => _player;

        // 플레이어 등록
        public void RegisterPlayer(PlayerBase player)
        {
            _player = player;
        }

        // 플레이어 제거
        public void ClearPlayer()
        {
            _player = null;
        }
    }
}