using System;
using System.Collections;
using UnityEngine;

namespace Sumfulla.TankTankBoom
{
    public class SoundtrackManager : MonoBehaviour
    {
        [Range(0f, 1f)]
        [SerializeField] private float _volume = 0.3f;
        private AudioSource _source;
        private float _initialVolume;


        /// <summary>
        /// Set up initial ST levels
        /// </summary>
        internal void Init()
        {
            if (TryGetComponent(out AudioSource audioSource))
            {
                _source = audioSource;
                _source.volume = _volume; // 
                _source.pitch = 0.9f; // 
                _source.loop = true;
            }
            else
            {
                GameLog.Warn("Could not find AudioSource");
            }
            _initialVolume = _volume;
        }

        /// <summary>
        /// Fades current track out and new track in
        /// </summary>
        private IEnumerator SoundtrackTransission(AudioClip newClip)
        {
            // Only fade stop if already playing
            if (_source != null && _source.isPlaying)
            {

                yield return StopWithFade(GameRef.Audio.SCENE_TRANSISSION_TIME);
            }

            // Add new clip and fade in
            _source.clip = newClip;
            if (PlayerPrefs.GetInt(GameRef.PrefRef.PREF_MUSIC, 0) == 1)
            {
                yield return PlayWithFade(GameRef.Audio.SCENE_TRANSISSION_TIME);
            }
        }

        /// <summary>
        /// Starts playing given soundtrack
        /// </summary>
        public void UpdateSoundtrack(string ostPath)
        {
            // // Turn off music while debugging
            // if (PlayerPrefs.GetInt(GameRef.PrefRef.PREF_MUSIC, 0) != 1)
            // {
            //     GameLog.Warn($"UpdateSoundtrack(<i>{ostPath}</i>) is currently disabled");
            // }
            // // Standard use
            // else
            {
                AudioClip newTrack = Resources.Load<AudioClip>($"OST/{ostPath}");

                if (newTrack == null)
                {
                    GameLog.Warn($"Soundtrack could not load: {ostPath}");
                }
                else
                {
                    if (_source.clip == newTrack)
                    {
                        GameLog.Warn($"Track request denied, already playing: {ostPath}");
                        return;
                    }
                    else
                    {
                        StartCoroutine(SoundtrackTransission(newTrack));
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Activate play feature in AudioSource but with a fade in
        /// </summary>
        public IEnumerator PlayWithFade(float fadeTime)
        {
            _source.Play();

            float t = 0;
            while (t < _initialVolume)
            {
                t += Time.deltaTime / fadeTime;
                _source.volume = Mathf.Lerp(0, _initialVolume, t);
                yield return null;
            }
            _source.volume = _initialVolume;
        }

        /// <summary>
        /// Activate stop feature in AudioSource but with a fade out
        /// </summary>
        public IEnumerator StopWithFade(float fadeTime = GameRef.Audio.SCENE_TRANSISSION_TIME)
        {
            float t = Mathf.InverseLerp(0, _initialVolume, _source.volume);
            while (t > 0)
            {
                t -= Time.deltaTime / fadeTime;
                _source.volume = Mathf.Lerp(0, _initialVolume, t);
                yield return null;
            }
            _source.volume = 0;
            _source.Stop();
        }

        /// <summary>
        /// Pause/unpause current soundtrack
        /// </summary>
        public void Pause(bool pause)
        {
            if (pause)
            {
                _source.Pause();
            }
            else
            {
                _source.Play();
            }
        }
    }
}