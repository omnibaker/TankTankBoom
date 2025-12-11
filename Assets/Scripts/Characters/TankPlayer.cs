
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sumfulla.TankTankBoom
{
    public class TankPlayer : MonoBehaviour
    {
        private const int MAX_SHIELD_DAMAGE = 4;

        [Header("ACTION FEATURES")]
        [SerializeField] private float _gravityScale = 0.3f;
        public GameObject Explosion;

        [Header("PLAYER SPRITE RENDERERS")]
        public SpriteRenderer PlayerState;
        public SpriteRenderer ShieldState;
        public SpriteRenderer CannonState;

        [Header("HEALTH")]
        public Sprite[] PlayerSprites;
        public Sprite[] ShieldSprites;
        public Color[] ShieldColors;

        [Header("Tank Components")]
        [SerializeField] private GameObject _flag;

        public Vector3 PausedVelocity { get; set; } = Vector3.zero;
        public float GravityScale { get { return _gravityScale; } }
        public int ShieldDamage { get; set; }
        public float MaxShieldDamage { get { return _maxShieldDamage; } }

        private Collider2D[] _colliders;
        private Coroutine _hitFading;
        private Animator _animator;
        private Rigidbody2D _rb;
        private Vector3 _tankPos;
        private float _maxShieldDamage;

        internal void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _colliders = GetComponents<Collider2D>();
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            _maxShieldDamage = MAX_SHIELD_DAMAGE;
        }

       
        /// <summary>
        /// Stops any movement cause by force or impacts
        /// </summary>
        internal void StopVelocity()
        {
            _rb.linearVelocity = Vector3.zero;
        }

        /// <summary>
        /// Sets tank up at a new world position
        /// </summary>
        public void UpdateTankPosition(TankPositionsInWorld mp)
        {
            _tankPos = mp.Tank;
            transform.position = _tankPos;
        }

        /// <summary>
        /// Resets player properties back to default
        /// </summary>
        internal void RestorePlayerState()
        {
            RemoveAllDamage();
            Explosion.SetActive(false);
            PausedVelocity = Vector3.zero;
        }

        /// <summary>
        /// Resets player health/damage/displays back to defaults
        /// </summary>
        public void RemoveAllDamage()
        {
            ShieldDamage = 0;
            PlayerState.sprite = PlayerSprites[ShieldDamage];
            ShieldState.sprite = ShieldSprites[ShieldDamage];
        }

        /// <summary>
        /// Changes health/damage/displays when receiving damage
        /// </summary>
        internal void InflictDamage()
        {
            if (PlayManager.I.GodMode) return;

            ShieldDamage++;
            GameLog.Say($"Player Hit! Damage={ShieldDamage} | Max{MaxShieldDamage}");

            if (ShieldDamage > MaxShieldDamage)
            {
                GameAudio.I.Play(SoundType.MachineExplode);

                PlayManager.I.State.Pause();
                Explode();
            }
            else
            {
                GameAudio.I.Play(SoundType.TankHitNotDestroyed);
                GameAudio.I.Play(SoundType.GroundExplode01);

                // Update player sprite
                PlayerState.sprite = PlayerSprites[ShieldDamage - 1];

                // Update shield/dmg sprites
                ShieldState.sprite = ShieldSprites[ShieldDamage - 1];
                ShieldState.color = ShieldColors[ShieldDamage - 1];

                TriggerDamageGlow();
            }
        }

        /// <summary>
        /// Triggers coroutine which displays shield sprites, stops any previous coroyutine run
        /// </summary>
        internal void TriggerDamageGlow()
        {
            // Activate shield objects
            ShieldState.gameObject.SetActive(true);
            CannonState.gameObject.SetActive(true);

            // Start fade away
            if (_hitFading != null)
            {
                StopCoroutine(_hitFading);
                _hitFading = null;
            }
            _hitFading = StartCoroutine(FadeHitIndicator());
        }

        /// <summary>
        /// Coroutine that displays shield sprites and fades out color property upon impact
        /// </summary>
        public IEnumerator FadeHitIndicator()
        {
            CannonState.color = ShieldState.color;
            Color shieldColor = ShieldState.color;
            float t = 0;

            while (t <= 1f)
            {
                shieldColor.a = Mathf.Lerp(1f, 0, t);
                ShieldState.color = shieldColor;
                CannonState.color = shieldColor;
                t += Time.deltaTime;
                yield return null;
            }

            shieldColor.a = 0;
            ShieldState.color = shieldColor;
            CannonState.color = shieldColor;

            _hitFading = null; 
            ShieldState.gameObject.SetActive(false);
            CannonState.gameObject.SetActive(false);
        }

        /// <summary>
        /// Hides any additonal component, changes sprite to broken image, runs explosion animation
        /// </summary>
        internal void Explode()
        {
            // Hide cannon/flag
            _flag.SetActive(false);
            if(TryGetComponent(out TankCannon cannon))
            {
                cannon.HideCannon();
            }

            // Updates player sprite
            PlayerState.sprite = PlayerSprites[ShieldDamage - 1];

            // Deactivate damage sprite
            ShieldState.gameObject.SetActive(false);

            // Activates explosion object and trigger animation
            Explosion.SetActive(true);
            _animator.SetBool(GameRef.AnimationTags.READY_TO_EXLODE, true);
        }

        /// <summary>
        /// Stops animation, removes from scene
        /// </summary>
        internal void Die()
        {
            // Dectivates explosion object and end animation
            _animator.SetBool(GameRef.AnimationTags.READY_TO_EXLODE, false);
            _animator.SetBool(GameRef.AnimationTags.READY_TO_DIE, true);
            Explosion.SetActive(false);
            //ArtilleryManager.I.LevelFailed(ArtilleryFailure.Destroyed);

            PlayManager.I.BattleFailed(FailureReason.Destroyed);
        }
    }


    public struct TankPositionsInWorld
    {
        public Vector3 Tank;
        public Vector3 Enemy;

        public TankPositionsInWorld(Vector3 tank, Vector2 enemy)
        {
            Tank = tank;
            Enemy = enemy;
        }
    }
}

