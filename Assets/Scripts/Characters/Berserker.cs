using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sumfulla.TankTankBoom
{
    public class Berserker : MonoBehaviour, IExplodableEnemy
    {
        private const float MIN_JUMP = 40f;
        private float _previousX;
        private float _upForce = 2f;
        private float _leftForce = 5f;

        private Rigidbody2D _rb;
        private Coroutine _runningLeft;
        private Animator _animator;
        private Collider2D _collider;

        private Vector2 _velocityWhenPaused = Vector2.zero;
        private bool _canRun;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _collider = GetComponent<Collider2D>();
        }

        private void Start()
        {
            EndBersekerRun();
            _runningLeft = StartCoroutine(RunLeft());
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag(TagNames.PlayerTank.ToString()))
            {
                collision.gameObject.GetComponent<TankPlayer>().StopVelocity();
                collision.gameObject.GetComponent<TankPlayer>().InflictDamage();
                InflictDamage();
                PlayManager.I.CameraShake();
            }
        }
        
        private void OnEnable()
        {
            PlayManager.I.State.OnPause += BerserkerPaused;
            PlayManager.I.State.OnUnpause += BerserkerUnpaused;
        }
        
        private void OnDisable()
        {
            if (PlayManager.IsInitialized)
            {
                PlayManager.I.State.OnPause -= BerserkerPaused;
                PlayManager.I.State.OnUnpause -= BerserkerUnpaused;
            }
        }

        /// <summary>
        /// Drives Berserker left, triggers escalating jumps whenever it hits obstacle
        /// </summary>
        private IEnumerator RunLeft()
        {
            yield return new WaitForSeconds(3f);

            _canRun = true;
            int forcedCount = 0;
            float timeCheck = 0;
            while (_canRun)
            {
                // Only do logic when the game is actually running
                if (PlayManager.I.State.Current != RunState.PLAY)
                {
                    yield return null;
                    continue;
                }

                // Only apply left force if not already at target speed
                if (_rb.linearVelocity.x <= -2.5f)
                {
                    yield return null;
                    continue;
                }

                // Apply leftwards force
                float leftForce = Time.deltaTime * _leftForce * GameRef.Enemies.GetBerserkerSpeed();
                _rb.AddForce(Vector3.left * leftForce, ForceMode2D.Impulse);


                // Check horizontal movement every ~0.3s or once it moves past previous x
                if (transform.position.x <= _previousX && timeCheck <= 0.3f)
                {
                    timeCheck += Time.deltaTime;
                }
                else
                {
                    float diff = Mathf.Abs(transform.position.x - _previousX);
                    if (diff < 0.02f)
                    {

                        // Not moving? Force jump
                        float jumpPower = MIN_JUMP + 2.5f * forcedCount;
                        _rb.AddForce(_upForce * jumpPower * Vector3.up, ForceMode2D.Force);

                        //Trigger ground explosion sound
                        if (forcedCount >= 3)
                        {
                            bool jumpVariant = PlayManager.I.GetBerserkerNoise();
                            GameAudio.I.Play(jumpVariant ? SoundType.BerserkerJump01 : SoundType.BerserkerJump02);
                        }
                        forcedCount++;
                    }
                    else
                    {
                        // Moved? Reset forced jump counter
                        forcedCount = 0;
                    }

                    timeCheck = 0;
                    _previousX = transform.position.x;
                }
            }
        }

        /// <summary>
        /// Kills any current running coroutine
        /// </summary>
        private void EndBersekerRun()
        {
            _canRun = false;
            if (_runningLeft != null)
            {
                StopCoroutine(_runningLeft);
            }
        }

        /// <summary>
        /// Interface method to cause explosion
        /// </summary>
        public void InflictDamage()
        {
            Explode();
        }

        /// <summary>
        /// Causes object to stop, explode and take a chunk of ground with it
        /// </summary>
        public void Explode()
        {
            GameAudio.I.Play(SoundType.MachineExplode);

            // Update score
            PlayManager.I.Score.AddPoints(GameRef.Points.BERSERKER_KILL);

            // Stop 'run' coroutine and disable physical elements
            EndBersekerRun();
            DisableNonExplodingParts();
            _animator.SetBool(GameRef.AnimationTags.READY_TO_EXLODE, true);

            // Destroy any close land tiles
            TerrainController.Instance.DestroyTerrainSet(transform.position, 0);

        }

        /// <summary>
        /// Interface method to stop exploding animation and remove object
        /// </summary>
        public void Die()
        {
            //ArtilleryManager.I.Points.CreateTextObject(ArtilleryEnemies.POINT_BERSERKER, transform.position + Vector3.up * 0.5f, Color.green);
            _animator.SetBool(GameRef.AnimationTags.READY_TO_EXLODE, false);
            RemoveFromScene();
        }


        /// <summary>
        /// Destroys object and any other clean-up tasks when resetting battle
        /// </summary>
        public void RemoveFromScene()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Disables and physics related properties
        /// </summary>
        public void DisableNonExplodingParts()
        {
            _collider.enabled = false;
            Destroy(_rb);
        }

        /// <summary>
        /// Behaviours to stop when game is paused
        /// </summary>
        private void BerserkerPaused()
        {
            if (_rb != null)
            {
                // Record current velocity to momentum contionues when unpaused
                _velocityWhenPaused = _rb.linearVelocity;
                if(_rb.bodyType != RigidbodyType2D.Static)
                {
                    _rb.linearVelocity = Vector2.zero;
                }

                // Halt any other physics related movement
                _rb.constraints = RigidbodyConstraints2D.FreezeAll;
                _rb.gravityScale = 0;
                _rb.bodyType = RigidbodyType2D.Static;
            }
        }

        /// <summary>
        /// Behaviours to restart when game is unpaused
        /// </summary>
        private void BerserkerUnpaused()
        {
            if (_rb != null)
            {
                _rb.bodyType = RigidbodyType2D.Dynamic;
                _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                _rb.gravityScale = 1;
                _rb.linearVelocity = _velocityWhenPaused;
            }
        }
    }
}