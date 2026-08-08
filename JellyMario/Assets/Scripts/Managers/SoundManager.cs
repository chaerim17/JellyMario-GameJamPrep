using UnityEngine;
using JellyMario.Core;

namespace JellyMario.Managers
{
    // 게임 사운드를 관리하는 매니저
    public class SoundManager : Singleton<SoundManager>
    {
        // 배경음 AudioClip과 AudioSource
        [SerializeField] private AudioClip _mainBGM;
        
        private AudioSource _bgmSource;

        // 효과음 AudioClip과 AudioSource
        [SerializeField] private AudioClip _jumpSFX;
        [SerializeField] private AudioClip _deathSFX;
        
        private AudioSource _sfxSource;

        // SoundManager 초기화
        protected override void Initialize()
        {
            base.Initialize();

            // 배경음 AudioSource 설정
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;

            // 효과음 AudioSource 설정
            _sfxSource = gameObject.AddComponent<AudioSource>();

            // 배경음 재생
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

        // 점프 효과음 재생
        public void PlayJumpSFX()
        {
            if (_jumpSFX != null)
                _sfxSource.PlayOneShot(_jumpSFX);
        }

        // 죽음 효과음 재생
        public void PlayDeathSFX()
        {
            if (_deathSFX != null)
                _sfxSource.PlayOneShot(_deathSFX);
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