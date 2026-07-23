using UnityEngine;
using JellyMario.Core;

namespace JellyMario.Managers
{
    // 게임 사운드를 관리하는 매니저
    public class SoundManager : Singleton<SoundManager>
    {
        // SoundManager 초기화
        protected override void Initialize()
        {
            base.Initialize();
        }

        // 배경음 재생
        public virtual void PlayBGM(string bgmName)
        {

        }

        // 효과음 재생
        public virtual void PlaySFX(string sfxName)
        {

        }

        // 배경음 정지
        public virtual void StopBGM()
        {

        }

        // 모든 사운드 정지
        public virtual void StopAllSound()
        {

        }
    }
}