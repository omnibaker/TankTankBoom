using UnityEngine;


namespace Sumfulla.TankTankBoom
{
    public class GameAudio : Singleton<GameAudio>
    {
        [SerializeField] public SoundtrackManager Soundtrack;
        [SerializeField] private SoundDatabase _soundBank;

        private AudioSource _sfxAudioSource;

        private bool _audioSourceConfirmed = false;
        public bool SoundOn;

        private void Awake()
        {
            CreateInstance(this, gameObject);
        }

        private void OnEnable()
        {
            CheckAudioSource();
            Soundtrack.Init();
            _soundBank.Init();
        }

        private void Start()
        {
            SoundOn = PlayerPrefs.GetInt(GameRef.PrefRef.PREF_SOUND, 0) == 0;
        }

        /// <summary>
        /// Checks local object for AudioSource and marks boolean
        /// </summary>
        private void CheckAudioSource()
        {
            if (TryGetComponent(out AudioSource s))
            {
                _sfxAudioSource = s;
                _audioSourceConfirmed = true;
            }
        }

        /// <summary>
        /// Checks if enum value has clip pair and invokes PlayOneShot
        /// </summary>
        public void Play(SoundType sound, float? volumeOverride = null)
        {
            if (SoundOn)
            {
                SoundData data = _soundBank.Get(sound);
                if (data != null && _audioSourceConfirmed)
                {
                    float volume = volumeOverride ?? data.DefaultVolume;
                    _sfxAudioSource.PlayOneShot(data.Clip, volume);
                }
                else
                {
                    // Comment out until being actually being implemented in game
                    //GameLog.Warn($"No sound for {sound}");
                }
            }
        }
    }

    public enum SoundType
    {
        PlayerFire01,
        PlayerFire02,
        EnemyFire,
        MachineExplode,
        GroundExplode01,
        GroundExplode02,
        BombDropping,
        BerserkerJump01,
        DestroyedEnemyTank,
        FailedLevel,
        GameOver,
        TankHitNotDestroyed,
        BerserkerJump02,
    }

}