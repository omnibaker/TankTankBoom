
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sumfulla.TankTankBoom
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class TankPlayer : MonoBehaviour
    {
        private const int MAX_SHIELD_DAMAGE = 4;

        [Header("ACTION FEATURES")]
        [SerializeField] private GameObject _explosion;

        [Header("PLAYER SPRITE RENDERERS")]
        [SerializeField] private SpriteRenderer _tankStateRenderer;
        [SerializeField] private SpriteRenderer _tankImpactRenderer;
        [SerializeField] private SpriteRenderer _cannonImpactRenderer;

        [Header("HEALTH")]
        [SerializeField] private Sprite[] _stateSprites;
        [SerializeField] private Sprite[] _impactSprites;
        [SerializeField] private Color[] _impactColors;

        [Header("Tank Components")]
        [SerializeField] private GameObject _flag;
        [SerializeField] private int _shieldDamage;

        private Coroutine _hitFading;
        private Animator _animator;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Stops any movement caused by force or impacts
        /// </summary>
        public void StopVelocity()
        {
            _rb.linearVelocity = Vector3.zero;
        }

        /// <summary>
        /// Sets tank up at a new world position
        /// </summary>
        public void UpdateTankPosition(TankPositionsInWorld mp)
        {
            transform.position = mp.Tank;
        }

        /// <summary>
        /// Resets player properties back to default
        /// </summary>
        public void RestorePlayerState()
        {
            RemoveAllDamage();
            _explosion.SetActive(false);
        }

        /// <summary>
        /// Resets player health/damage/displays back to defaults
        /// </summary>
        private void RemoveAllDamage()
        {
            _shieldDamage = 0;
            _tankStateRenderer.sprite = _stateSprites[_shieldDamage];
            _tankImpactRenderer.sprite = _impactSprites[_shieldDamage];
        }

        /// <summary>
        /// Changes health/damage/displays when receiving damage
        /// </summary>
        public void InflictDamage()
        {
            if (PlayManager.I.GodMode) return;

            _shieldDamage++;
            GameLog.Say($"Player Hit! Damage={_shieldDamage} | Max{MAX_SHIELD_DAMAGE}");

            if (_shieldDamage > MAX_SHIELD_DAMAGE)
            {
                GameAudio.I.Play(SoundType.MachineExplode);
                PlayManager.I.State.Pause();
                Explode();
                TriggerDamageGlow(true);
            }
            else
            {
                GameAudio.I.Play(SoundType.TankHitNotDestroyed);
                GameAudio.I.Play(SoundType.GroundExplode01);

                // Update player sprite
                int damageIndex = _shieldDamage <= 2 ? 0 : _shieldDamage - 1;
                _tankStateRenderer.sprite = _stateSprites[damageIndex];

                // Update shield/dmg sprites
                _tankImpactRenderer.sprite = _impactSprites[damageIndex];
                _tankImpactRenderer.color = _impactColors[_shieldDamage - 1];

                TriggerDamageGlow();
            }
        }

        /// <summary>
        /// Triggers coroutine which displays shield sprites, stops any previous coroutine run
        /// </summary>
        private void TriggerDamageGlow(bool isFinal = false)
        {
            // Activate shield objects
            _tankImpactRenderer.gameObject.SetActive(true);
            if (!isFinal) _cannonImpactRenderer.gameObject.SetActive(true);

            // Start fade away
            if (_hitFading != null)
            {
                StopCoroutine(_hitFading);
                _hitFading = null;
            }
            _hitFading = StartCoroutine(FadeHitIndicator(isFinal));
        }

        /// <summary>
        /// Coroutine that displays shield sprites and fades out color property upon impact
        /// </summary>
        private IEnumerator FadeHitIndicator(bool isFinal)
        {
            if(!isFinal) _cannonImpactRenderer.color = _tankImpactRenderer.color;
            Color shieldColor = _tankImpactRenderer.color;
            float t = 0;

            while (t <= 1f)
            {
                shieldColor.a = Mathf.Lerp(1f, 0, t);
                _tankImpactRenderer.color = shieldColor;
                if (!isFinal) _cannonImpactRenderer.color = shieldColor;
                t += Time.deltaTime * 2f;
                yield return null;
            }

            shieldColor.a = 0;
            _tankImpactRenderer.color = shieldColor;
            if (!isFinal) _cannonImpactRenderer.color = shieldColor;

            _hitFading = null; 
            _tankImpactRenderer.gameObject.SetActive(false);
            _cannonImpactRenderer.gameObject.SetActive(false);
        }

        /// <summary>
        /// Hides any additional component, changes sprite to broken image, runs explosion animation
        /// </summary>
        private void Explode()
        {
            // Hide cannon/flag
            _flag.SetActive(false);
            if(TryGetComponent(out TankCannon cannon))
            {
                cannon.HideCannon();
            }

            // Updates player sprite
            _tankStateRenderer.sprite = _stateSprites[_shieldDamage - 1];

            // Deactivate damage sprite
            _tankImpactRenderer.gameObject.SetActive(false);

            // Activates explosion object and trigger animation
            _explosion.SetActive(true);
            _animator.SetBool(GameRef.AnimationTags.READY_TO_EXPLODE, true);
        }

        /// <summary>
        /// Stops animation, removes from scene
        /// </summary>
        public void Die()
        {
            // Deactivates explosion object and end animation
            _animator.SetBool(GameRef.AnimationTags.READY_TO_EXPLODE, false);
            _animator.SetBool(GameRef.AnimationTags.READY_TO_DIE, true);
            _explosion.SetActive(false);
            //ArtilleryManager.I.LevelFailed(ArtilleryFailure.Destroyed);

            PlayManager.I.BattleFailed(FailureReason.Destroyed);
        }
    }


    public readonly struct TankPositionsInWorld
    {
        public Vector3 Tank { get; }
        public Vector3 Enemy { get; }

        public TankPositionsInWorld(Vector3 tank, Vector3 enemy)
        {
            Tank = tank;
            Enemy = enemy;
        }
    }
}

