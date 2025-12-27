using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sumfulla.TankTankBoom
{
    public class TankProjectile : MonoBehaviour
    {
        private bool _airborne;
        private bool _windAffected;
        private Rigidbody2D _rb;
        private Coroutine _countdownToExplode;
        private Vector3 _velocityWhenPaused;
        private bool _triggered;

        private void Update()
        {
            if (PlayManager.I.State.Current == RunState.PLAY && _airborne)
            {
                if (_windAffected)
                {
                    AddWindResistance();
                }
                MaintainProperAngle();
            }
        }

        private void OnEnable()
        {
            PlayManager.I.State.OnPause += ProjectilePaused;
            PlayManager.I.State.OnUnpause += ProjectileUnpaused;
        }

        private void OnDisable()
        {
            if (PlayManager.IsInitialized)
            {
                PlayManager.I.State.OnPause -= ProjectilePaused;
                PlayManager.I.State.OnUnpause -= ProjectileUnpaused;
            }
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            // Hits enemy object
            if (collider.CompareTag(TagNames.Enemy.ToString()))
            {
                if (collider.TryGetComponent(out IExplodableEnemy explodable))
                {
                    if(!_triggered)
                    {
                        explodable.InflictDamage();
                        PlayManager.I.CameraShake();
                        _triggered = true;
                    }
                    else
                    {
                        Debug.LogError("Has already been triggered!");
                    }

                }
            }
            // Hits player
            else if (collider.CompareTag(TagNames.PlayerTank.ToString()))
            {
                if (collider.TryGetComponent(out TankPlayer tp))
                {
                    if(!_triggered)
                    {
                        tp.InflictDamage();
                        PlayManager.I.CameraShake();
                        _triggered = true;
                    }
                    else
                    {
                        Debug.LogError("Has already been triggered!");
                    }
                }
            }

            Explode();
        }

        /// <summary>
        /// Set rigidbody and push with force mode in given direction
        /// </summary>
        public void Initialize(Vector3 directionalForce, bool windAffected = false)
        {
            _windAffected = windAffected;
            if(TryGetComponent(out Rigidbody2D rb))
            {
                _rb = rb;
                _rb.AddForce(directionalForce);
                _airborne = true; ;
            }
            else
            {
                GameLog.Warn("Cannot find Rigidbody2D on projectile so removing from scene");
                Die();
            }

        }

        /// <summary>
        /// Changes color layout of projectile red/white. Not used anymore as using Material. 
        /// </summary>
        private void ChangeColor()
        {
            if(TryGetComponent(out TrailRenderer tr))
            {
                Gradient gradient = new Gradient();
                GradientColorKey[] colorKey = new GradientColorKey[2];
                colorKey[0].color = Color.red;
                colorKey[0].time = 0f;
                colorKey[1].color = Color.white;
                colorKey[1].time = 1f;

                GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
                alphaKey[0].alpha = 0.0f;
                alphaKey[0].time = 1f;
                alphaKey[1].alpha = 1f;
                alphaKey[1].time = 0.0f;

                gradient.SetKeys(colorKey, alphaKey);

                tr.colorGradient = gradient;
            }
        }

        /// <summary>
        /// Updates gameobjects rotation so as to match the direction it is currently moving in
        /// </summary>
        private void MaintainProperAngle()
        {
            if (_airborne)
            {
                Vector3 velocity = _rb.linearVelocity;
                float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }

        /// <summary>
        /// Adjusts the velocity to be decreases/inceased by wind force
        /// </summary>
        private void AddWindResistance()
        {
            _rb.linearVelocity += Vector2.right * PlayManager.I.Environment.Wind * Time.deltaTime;
        }

        /// <summary>
        /// Stops failsafe countdown, disables colliders and blows up piece of land if made contact
        /// </summary>
        private void Explode()
        {
            if (_countdownToExplode != null)
            {
                StopCoroutine(_countdownToExplode);
            }
            _airborne = false;
            if (TryGetComponent(out BoxCollider2D bc))
            {
                bc.enabled = false;
            }
            TerrainController.Instance.DestroyTerrainSet(transform.position);
            Die();
        }

        /// <summary>
        /// Removes object from scene
        /// </summary>
        public void Die()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Failsafe explosion if projectile misses target or land, so it doesn't fall forever
        /// </summary>
        public IEnumerator ExplodeAfterTime()
        {
            float t = 0;

            while (t < 7f)
            {
                if (PlayManager.I.State.Current == RunState.PLAY)
                {
                    t += Time.deltaTime;
                }
                yield return null;
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// Behaviour affecting physics stopped/saved when game is paused 
        /// </summary>
        private void ProjectilePaused()
        {
            if (_rb != null)
            {
                // Save velocity value so when unpaused, the momemtum continues
                _velocityWhenPaused = _rb.linearVelocity;
                if (_rb.bodyType != RigidbodyType2D.Static)
                {
                    _rb.linearVelocity = Vector2.zero;
                }
                _rb.constraints = RigidbodyConstraints2D.FreezeAll;
                _rb.gravityScale = 0;
                _rb.bodyType = RigidbodyType2D.Static;
            }
            else
            {
                GameLog.Say("PAUSE - Would normally error here");
            }
        }

        /// <summary>
        /// Behaviour affecting physics returned when game is unpaused 
        /// </summary>
        private void ProjectileUnpaused()
        {
            if (_rb != null)
            {
                _rb.bodyType = RigidbodyType2D.Dynamic;
                _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                _rb.gravityScale = 1;
                _rb.linearVelocity = _velocityWhenPaused;
            }
            else
            {
                GameLog.Say("UNPAUSE - Would normally error here");
            }
        }
    }


}