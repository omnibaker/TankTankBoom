using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Sumfulla.TankTankBoom
{
    public class AirStrike : MonoBehaviour
    {
        private const float SPEED = 10f;
        private const int NUM_OF_BOMBS = 10;

        [SerializeField] private GameObject _bombPf;
        [SerializeField] private Transform _bombDispatchPoint;
        [SerializeField] private List<GameObject> _bombs;

        public bool BombsReleased { get { return _bombingComplete; } }
        private float _endX;
        private bool _bombingStarted;
        private bool _bombingComplete;

        public Action<bool> OnFlyoverCompleted;
        public Action OnBombingCompleted;

        /// <summary>
        /// Set up callback listeners and initiates flyover
        /// </summary>
        public void Init(float endX, Action<bool> strikeState, Action bombingComplete)
        {
            OnFlyoverCompleted = strikeState;
            OnBombingCompleted = bombingComplete;
            _endX = endX;
            StartCoroutine(Fly());
        }

        /// <summary>
        /// Coroutine that pushes craft across the screen
        /// </summary>
        private IEnumerator Fly()
        {
            while (transform.position.x < _endX)
            {
                transform.position += Vector3.right * Time.deltaTime * SPEED;
                yield return null;
            }

            OnFlyoverCompleted.Invoke(_bombingStarted);
        }

        /// <summary>
        /// Triggers bombing coroutine
        /// </summary>
        public void ExecuteStrike()
        {
            StartCoroutine(DropBombs());
        }

        /// <summary>
        /// Sequentially drops set number of bombs over selected area
        /// </summary>
        private IEnumerator DropBombs()
        {
            _bombingStarted = true;
            int numOfBombs = NUM_OF_BOMBS;
            List<Coroutine> listOfBombDrops = new List<Coroutine>();

            for (int i = 0; i < numOfBombs; i++)
            {
                // Spawn
                GameObject bomb = Instantiate(_bombPf, _bombDispatchPoint.position, Quaternion.identity);
                _bombs.Add(bomb);

                if (bomb.TryGetComponent(out StrikeBomb sb))
                {
                    listOfBombDrops.Add(StartCoroutine(sb.DropBombUntilImpact()));
                }

                // Trigger ground explosion sound
                if (i % 2 == 0)
                {
                    GameAudio.I.Play(SoundType.BombDropping);
                }

                yield return new WaitForSeconds(0.05f);
            }

            // Wait until all bombs dropped and signal raid is over
            foreach (Coroutine c in listOfBombDrops)
            {
                yield return c;
            }

            BombingCompleted();
        }
        
        /// <summary>
        /// Cleans up scene after bomb and removes any strike objects
        /// </summary>
        public void BombingCompleted()
        {
            StopAllCoroutines();

            // Ensure all bombs are indeed removed
            foreach(GameObject bomb in _bombs)
            {
                if(_bombs != null)
                {
                    Destroy(bomb);
                }
            }

            // Remove any callbacks and destroy strike object
            OnBombingCompleted.Invoke();
            OnBombingCompleted = null;
            OnFlyoverCompleted = null;
            Destroy(gameObject);
        }
    }
}