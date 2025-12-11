using System.Collections;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace Sumfulla.TankTankBoom
{
    /// <summary>
    /// Base class for individual SFX/Music objects created at runtime
    /// </summary>
    public class AudioObject
    {
        [SerializeField] public AudioClip _clip;

        [Range(0f, 1f)]
        [SerializeField] private float _volume = 1f;

        [Range(-1.5f, 1.5f)]
        [SerializeField] private float _pitch = 1f;

        [Range(0f, 0.5f)]
        [SerializeField] private float _randomVolume = 0.1f;

        [Range(0f, 0.5f)]
        [SerializeField] private float _randomPitch = 0.1f;

        public AudioSource Source { get; set; }

        private float InitialVolume { get; set; }


        /// <summary>
        /// Assign AudioSource and update properties
        /// </summary>
        public void SetSource(AudioSource source)
        {
            Source = source;
            Source.clip = _clip;
            Source.volume = _volume;
            Source.pitch = _pitch;
            Source.playOnAwake = false;
            InitialVolume = Source.volume;
        }

        /// <summary>
        /// Activate play feature in AudioSource
        /// </summary>
        public void Play()
        {
            //Source.Play();
            Source.PlayOneShot(_clip);
        }

        /// <summary>
        /// Activate stop feature in AudioSource
        /// </summary>
        public void Stop()
        {
            Source.Stop();
        }

        /// <summary>
        /// Activate play feature in AudioSource with randomised variance
        /// </summary>
        public void PlayWithVariance()
        {
            Source.volume = _volume * (1 + Random.Range(-_randomVolume / 2f, _randomVolume / 2f));
            Source.pitch = _pitch * (1 + Random.Range(-_randomPitch / 2f, _randomPitch / 2f));
            Play();
        }

        /// <summary>
        /// Activate play feature in AudioSource but with a fade in
        /// </summary>
        public IEnumerator PlayWithFade(float fadeTime)
        {
            Play();

            float t = 0;
            while (t < InitialVolume)
            {
                t += Time.deltaTime / fadeTime;
                Source.volume = Mathf.Lerp(0, InitialVolume, t);
                yield return null;
            }
            Source.volume = InitialVolume;
        }

        /// <summary>
        /// Activate stop feature in AudioSource but with a fade out
        /// </summary>
        public IEnumerator StopWithFade(float fadeTime)
        {
            float t = Mathf.InverseLerp(0, InitialVolume, Source.volume);
            while (t > 0)
            {
                t -= Time.deltaTime / fadeTime;
                Source.volume = Mathf.Lerp(0, InitialVolume, t);
                yield return null;
            }
            Source.volume = 0;
            Stop();
        }
    }
}