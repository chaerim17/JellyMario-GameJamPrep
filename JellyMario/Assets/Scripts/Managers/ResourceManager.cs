using UnityEngine;
using JellyMario.Core;

namespace JellyMario.Managers
{
    public class ResourceManager : Singleton<ResourceManager>
    {
        // ResourceManager 초기화
        public void Initialize()
        {

        }

        // Resources 폴더에서 리소스를 불러옵니다.
        public T Load<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        }

        // 사용하지 않는 리소스를 정리합니다.
        public void UnloadUnusedAssets()
        {
            Resources.UnloadUnusedAssets();
        }

        // ResourceManager를 초기 상태로 되돌립니다.
        public void Clear()
        {

        }
    }
}