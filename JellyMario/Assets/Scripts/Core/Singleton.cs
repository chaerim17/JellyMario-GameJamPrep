using UnityEngine;

namespace JellyMario.Core
{
    // 제네릭(Generic) 싱글톤 클래스
    // T에는 GameManager, UIManager 같은 클래스가 들어간다.
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        // 어디서든 접근할 수 있는 유일한 객체
        public static T Instance;

        // 싱글톤 객체가 생성된 후 초기화를 수행하는 함수
        protected virtual void Initialize()
        {

        }

        // Unity가 오브젝트를 생성하면 가장 먼저 실행되는 함수
        protected virtual void Awake()
        {
            // 아직 Instance가 없다면
            if (Instance == null)
            {
                // 현재 객체를 Instance로 등록
                Instance = this as T;

                // 씬이 바뀌어도 삭제되지 않도록 설정
                DontDestroyOnLoad(gameObject);

                // 싱글톤 객체 초기화
                Initialize();
            }
            else
            {
                // 이미 같은 객체가 존재하면 중복 생성된 것이므로 삭제
                Destroy(gameObject);
            }
        }
    }
}