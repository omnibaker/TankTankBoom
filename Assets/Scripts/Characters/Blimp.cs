using System.Collections;
using UnityEngine;

namespace Sumfulla.TankTankBoom
{
    public class Blimp : MonoBehaviour, IExplodableEnemy
    {
        [SerializeField] private GameObject _bombPF = null;
        [SerializeField] private Transform _bombDropPoint = null;

        public delegate void OnBlimpDestroyed();
        public event OnBlimpDestroyed OnBlimpDestroyedEvent;

        private TankPlayer _player;

        private const float OFFSCREEN_OFFSET = 2f;
        private Vector3 _startPoint;
        private float _endPointX;
        private float _speed = 1f;
        private bool _bombDropped;
        private float _secondScanner;

        private void Update()
        {
            if (PlayManager.I.State.Current == RunState.PLAY)
            {
                MoveLeft();
                ScanForPlayer();
            }
        }

        private void Start()
        {
            _player = FindFirstObjectByType<TankPlayer>();
            _startPoint = new Vector3(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x + OFFSCREEN_OFFSET, transform.position.y, Camera.main.nearClipPlane);
            _endPointX = Camera.main.ScreenToWorldPoint(Vector3.zero).x - OFFSCREEN_OFFSET;
        }

        /// <summary>
        /// Makes blimp to slowly coast to the left of the screen
        /// </summary>
        private void MoveLeft()
        {
            if (PlayManager.I.State.Current == RunState.PAUSED) return;

            // Reset to start on the right if reached left end point
            if (transform.position.x < _endPointX)
            {
                transform.position = _startPoint;
                _bombDropped = false;
                return;
            }
            
            // Per frame left increment
            transform.position = transform.position + _speed * Time.deltaTime * Vector3.left;
        }
        
        /// <summary>
        /// Uses raycast to look underneath and detect if tank player is directly below, if so drops bomb
        /// </summary>
        private void ScanForPlayer()
        {
            if (PlayManager.I.State.Current == RunState.PAUSED) return;

            // Only scan once every half-second
            if (_secondScanner < 0.5f)
            {
                _secondScanner += Time.deltaTime;
                return;
            }

            // Reset scan timer
            _secondScanner = 0;

            // Attempt to find player tank with racast, drop bomb if found
            int layermask = 1 << LayerMask.NameToLayer(LayerNames.PlayerTarget.ToString());
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 300f, layermask);
            if (hit.collider != null)
            {
                if (!_bombDropped)
                {
                    StartCoroutine(DropBomb());
                }
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
        /// Causes object to hide main image, start explosion animatin and add reward points
        /// </summary>
        public void Explode()
        {
            GameAudio.I.Play(SoundType.MachineExplode);

            PlayManager.I.Score.AddPoints(GameRef.Points.BLIMP_KILL);
            if(TryGetComponent(out Animator animator))
            {
                animator.SetBool(GameRef.AnimationTags.READY_TO_EXLODE, true);
            }
            else
            {
                Die();
            }
        }

        /// <summary>
        /// Interface method to stop exploding animation and remove object
        /// </summary>
        public void Die()
        {
            //ArtilleryManager.I.Points.CreateTextObject(ArtilleryEnemies.POINT_BLIMP, transform.position, Color.green);
            OnBlimpDestroyedEvent?.Invoke();
            Destroy(gameObject);
        }

        /// <summary>
        /// Holds off creating and dropping bomb until at least half way over player
        /// </summary>
        private IEnumerator DropBomb()
        {
            yield return new WaitUntil(() => _player.transform.position.x > transform.position.x);

            if (!_bombDropped)
            {
                Instantiate(_bombPF, new Vector3(_player.transform.position.x, _bombDropPoint.position.y, Camera.main.nearClipPlane), Quaternion.identity); ;

                GameAudio.I.Play(SoundType.BombDropping);
            }
            _bombDropped = true;
        }

        /// <summary>
        /// Ignored interace method - no parts to disable
        /// </summary>
        public void DisableNonExplodingParts()
        {
            // Ignored
        }
    }


}