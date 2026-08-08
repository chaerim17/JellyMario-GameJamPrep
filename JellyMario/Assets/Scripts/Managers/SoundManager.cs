using UnityEngine;
using JellyMario.Core;

namespace JellyMario.Managers
{
    // 게임 사운드를 관리하는 매니저
    public class SoundManager : Singleton<SoundManager>
    {
        [SerializeField] private AudioClip _mainBGM;

        private AudioSource _bgmSource;

        // SoundManager 초기화
        protected override void Initialize()
        {
            base.Initialize();

            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;

            PlayBGM();
        }

        // 배경음 재생
        public void PlayBGM()
        {
            if (_mainBGM == null)
                return;

            if (_bgmSource.clip == _mainBGM && _bgmSource.isPlaying)
                return;

            _bgmSource.clip = _mainBGM;
            _bgmSource.Play();
        }

        // 효과음 재생
        public virtual void PlaySFX(string sfxName)
        {

        }

        // 배경음 정지
        public virtual void StopBGM()
        {
            _bgmSource.Stop();
        }

        // 모든 사운드 정지
        public virtual void StopAllSound()
        {

        }
    }
}