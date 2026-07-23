using UnityEngine;
using JellyMario.Core;

namespace JellyMario.Managers
{
    // 게임 전체를 관리하는 매니저
    // Singleton<GameManager>를 상속받기 때문에
    // GameManager.Instance로 어디서든 접근할 수 있다.
    public class GameManager : Singleton<GameManager>
    {
        // 현재 게임 상태를 저장하는 변수
        private GameState _currentState;

        // 플레이어 이름
        private string _playerName = string.Empty;

        // 다른 스크립트에서는 읽기만 가능하도록 만든다.
        public GameState CurrentState => _currentState;

        // 플레이어 이름은 읽기만 가능하도록 만든다.
        public string PlayerName => _playerName;

        // 게임 시작 시 가장 먼저 호출된다.
        protected override void Awake()
        {
            // 부모(Singleton)의 Awake를 먼저 실행
            base.Awake();

            // 게임 시작 시 타이틀 상태로 초기화
            _currentState = GameState.Title;

            Debug.Log("GameManager Awake 실행");
        }

        // 게임 상태를 변경하는 함수
        public void ChangeState(GameState newState)
        {
            // 같은 상태라면 아무것도 하지 않는다.
            if (_currentState == newState)
                return;

            _currentState = newState;

            Debug.Log($"현재 상태 : {_currentState}");
        }

        // 플레이어 이름을 저장하는 함수
        public void SetPlayerName(string playerName)
        {
            _playerName = playerName;
        }
    }
}