using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Sumfulla.TankTankBoom
{
    [RequireComponent(typeof(Animator))]
    public class BerserkerDropper : MonoBehaviour, IExplodableEnemy
    {
        private const float SPEED = 2f;

        [SerializeField] private GameObject _berserkerPF;
        [SerializeField] private GameObject _explosion;

        private Vector3 _thresholds;
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Descends from instantiation point, drops berserker object at calculated height, then flies off
        /// </summary>
        public IEnumerator StartDescent()
        {
            Vector3 screenDropPoint = new Vector3(Screen.width + 50f, Screen.height * 0.65f, 0);
            _thresholds = Camera.main.ScreenToWorldPoint(screenDropPoint);

            while (transform.position.y > _thresholds.y)
            {
                if (PlayManager.I.State.Current == RunState.PLAY)
                {
                    transform.position += SPEED * Time.deltaTime * Vector3.down;
                }
                yield return null;
            }

            Instantiate(_berserkerPF, transform.position + Vector3.down / 2f, Quaternion.identity);
            StartCoroutine(FlyAway());
        }

        /// <summary>
        /// Flies off to top right and then removes itself offscreen
        /// </summary>
        private IEnumerator FlyAway()
        {
            while (transform.position.x < _thresholds.x)
            {
                if (PlayManager.I.State.Current == RunState.PLAY)
                {
                    transform.position += 3f * Time.deltaTime * new Vector3(1f, 1f, 0);
                }
                yield return null;
            }

            Die();
        }

        /// <summary>
        /// Interface method to cause explosion
        /// </summary>
        public void InflictDamage()
        {
            Explode();
        }

        /// <summary>
        /// Causes object to hide main image, start explosion animatin and add reward points
        /// </summary>
        public void Explode()
        {
            GameAudio.I.Play(SoundType.MachineExplode);

            // Update score
            PlayManager.I.Score.AddPoints(GameRef.Points.BDROPPER_KILL);

            // Stop 'run' coroutine and disable physical elements
            StopAllCoroutines();
            DisableNonExplodingParts();
            _explosion.SetActive(true);

            // Hide main image and 
            if(TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.enabled = false;
            }

            // Trigger explosion
            if (_animator != null)
            {
                _animator.SetBool(GameRef.AnimationTags.READY_TO_EXLODE, true);
            }
            else
            {
                Die();
            }
        }

        /// <summary>
        /// Disables and physics related properties
        /// </summary>
        public void DisableNonExplodingParts()
        {
            if (TryGetComponent(out BoxCollider2D bc))
            {
                bc.enabled = false;
            }
        }

        /// <summary>
        /// Interface method to stop exploding animation and remove object
        /// </summary>
        public void Die()
        {
            //ArtilleryManager.I.Points.CreateTextObject(ArtilleryEnemies.POINT_BDROPPER, transform.position, Color.green);
            if (_animator != null)
            {
                _animator.SetBool(GameRef.AnimationTags.READY_TO_EXLODE, false);
            }
            Destroy(gameObject);
        }
    }


}