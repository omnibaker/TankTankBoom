using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Sumfulla.TankTankBoom
{
    public class AirStrike : MonoBehaviour
    {
        private const float FLY_SPEED = 10f;
        private const int BOMB_COUNT = 10;
        private const float BOMB_DELAY = 0.05f;
        
        [Header("References")]
        [SerializeField] private GameObject _bombPf;
        [SerializeField] private Transform _bombDispatchPoint;
        private List<GameObject> _spawnedBombs = new List<GameObject>();

        public bool BombsReleased { get { return _isComplete; } }
        private float _endX;
        private bool _hasStartedBombing;
        private bool _isComplete;

        public event Action<bool> FlyoverCompleted;
        public event Action BombingCompleted;

        /// <summary>
        /// Set up callback listeners and initiates flyover
        /// </summary>
        public void Init(float endX, Action<bool> onFlyoverCompleted, Action onBombingCompleted)
        {
            _endX = endX;
            FlyoverCompleted = onFlyoverCompleted;
            BombingCompleted = onBombingCompleted;

            StartCoroutine(Fly());
        }

        /// <summary>
        /// Coroutine that pushes craft across the screen
        /// </summary>
        private IEnumerator Fly()
        {
            while (transform.position.x < _endX)
            {
                transform.position += FLY_SPEED * Time.deltaTime * Vector3.right;
                yield return null;
            }

            FlyoverCompleted.Invoke(_hasStartedBombing);
        }

        /// <summary>
        /// Triggers bombing coroutine
        /// </summary>
        public void ExecuteStrike()
        {
            if (_hasStartedBombing || _isComplete)
            {
                return;
            }

            StartCoroutine(DropBombs());
        }

        /// <summary>
        /// Sequentially drops set number of bombs over selected area
        /// </summary>
        private IEnumerator DropBombs()
        {
            _hasStartedBombing = true;


            int numOfBombs = BOMB_COUNT;
            List<Coroutine> _activeDrops = new List<Coroutine>();

            for (int i = 0; i < numOfBombs; i++)
            {
                // Spawn bombs
                GameObject bomb = Instantiate(_bombPf, _bombDispatchPoint.position, Quaternion.identity);
                _spawnedBombs.Add(bomb);

                if (bomb.TryGetComponent(out StrikeBomb sb))
                {
                    _activeDrops.Add(StartCoroutine(sb.DropBombUntilImpact()));
                }

                // Trigger ground explosion sound
                if (i % 2 == 0)
                {
                    GameAudio.I.Play(SoundType.BombDropping);
                }

                yield return new WaitForSeconds(BOMB_DELAY);
            }

            // Wait until all bombs dropped and signal raid is over
            foreach (Coroutine drop in _activeDrops)
            {
                yield return drop;
            }

            CompleteStrike();
        }

        /// <summary>
        /// Cleans up scene after bomb and removes any strike objects
        /// </summary>
        public void CompleteStrike()
        {
            if (_isComplete)
            {
                return;
            }
            _isComplete = true;

            StopAllCoroutines();

            // Ensure all bombs are indeed removed
            foreach(GameObject bomb in _spawnedBombs)
            {
                if(_spawnedBombs != null)
                {
                    Destroy(bomb);
                }
            }
            _spawnedBombs.Clear();

            // Remove any callbacks and destroy strike object
            BombingCompleted.Invoke();
            BombingCompleted = null;
            FlyoverCompleted = null;
            Destroy(gameObject);
        }
    }
}