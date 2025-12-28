using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sumfulla.TankTankBoom
{
    public class Enemies : MonoBehaviour
    {
        [SerializeField] private GameObject _enemyPF = null;
        [SerializeField] private GameObject _blimpPF = null;
        [SerializeField] private GameObject _berserkerDropperPF = null;

        private EnemyTank _enemyTank;
        private bool _berserkerJumpToggle;

        /// <summary>
        /// Triggers the next blimp countdown coroutine
        /// </summary>
        public void StartBlimp()
        {
            StartCoroutine(CountdownToNextBlimp());
        }

        /// <summary>
        /// Triggers the next berserker countdown coroutine
        /// </summary>
        public void StartBerserkerDroppers()
        {
            StartCoroutine(CountdownToNextBerserker());
        }

        /// <summary>
        /// Calculates how long until the next berserker release, waits, and releases when time is ready
        /// </summary>
        private IEnumerator CountdownToNextBerserker()
        {
            // No berserker for first few levels
            if (GameData.CurrentBattle <= 2)
            {
                yield return null;
                yield break;
            }

            // Calculate countdown
            float timeToNewDrop = GameRef.Enemies.GetBerserkerFrequency();
            while (timeToNewDrop > 0)
            {
                if (PlayManager.I.State.Current == RunState.PLAY)
                {
                    timeToNewDrop -= Time.deltaTime;
                }
                yield return null;
            }

            // Calculate drop position
            float xPos = Screen.width * UnityEngine.Random.Range(0.4f, 0.75f);
            Vector3 dropPosition = new Vector3(xPos, Screen.height, 0);
            CreateBerserkerDropper(Camera.main.ScreenToWorldPoint(dropPosition));

            // Start countdown until next drop
            StartBerserkerDroppers();
        }

        /// <summary>
        /// Calculates how long until the next blimp release, waits, and releases when time is ready
        /// </summary>
        private IEnumerator CountdownToNextBlimp()
        {
            // Calculate countdown
            float timeToNewDrop = GameRef.Enemies.GetBlimpFrequency();
            while (timeToNewDrop > 0)
            {
                if (PlayManager.I.State.Current == RunState.PLAY)
                {
                    timeToNewDrop -= Time.deltaTime;
                }
                yield return null;
            }

            // Create blimp object
            CreateEnemyBlimp();
        }

        /// <summary>
        /// Global watch to switch jump noise so it doesn't get super annoying (TODO: implement pitch shift on single track)
        /// </summary>
        public bool ToggleBerserkerJump()
        {
            _berserkerJumpToggle = !_berserkerJumpToggle;
            return _berserkerJumpToggle;
        }

        /// <summary>
        /// Instantiates enemy tank object at given position
        /// </summary>
        public void CreateEnemyTank(Vector3 tankPosition)
        {
            // Destroy any existing tank
            if (_enemyTank != null)
            {
                Destroy(_enemyTank.gameObject);
            }
            GameObject enemyTank = Instantiate(_enemyPF, tankPosition, Quaternion.identity);
            if(enemyTank.TryGetComponent(out EnemyTank tank))
            {
                _enemyTank = tank;
            }
        }

        /// <summary>
        /// Instantiates enemy blimp object at given position
        /// </summary>
        public void CreateEnemyBlimp()
        {
            Vector3 position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width + 1000f, Screen.height * 0.75f, 0));
            position.z = Camera.main.nearClipPlane;
            GameObject blimpObject = Instantiate(_blimpPF, position, Quaternion.identity);
            if (blimpObject.TryGetComponent(out Blimp b))
            {
                b.OnBlimpDestroyedEvent += StartBlimp;
            }
        }

        /// <summary>
        /// Instantiates enemy berserker dropper object at given position
        /// </summary>
        public void CreateBerserkerDropper(Vector3 startDescentPosition)
        {
            Vector3 adjustZ = new Vector3(startDescentPosition.x, startDescentPosition.y, Camera.main.nearClipPlane);
            GameObject berserkerDropper = Instantiate(_berserkerDropperPF, adjustZ, Quaternion.identity);
            if(berserkerDropper.TryGetComponent(out BerserkerDropper bd))
            {
                StartCoroutine(bd.StartDescent());
            }
                
        }

        /// <summary>
        /// Global catch-all to remove existing enemy tank
        /// </summary>
        public void RemoveEnemyTank()
        {
            if (_enemyTank != null)
            {
                Destroy(_enemyTank.gameObject);
            }
        }

        /// <summary>
        /// Global catch-all to remove all existing enemy berserker droppers
        /// </summary>
        public void RemoveBerserkerDroppers()
        {
            foreach (BerserkerDropper dropper in FindObjectsByType<BerserkerDropper>(FindObjectsSortMode.None))
            {
                dropper.RemoveFromScene();
            }
        }

        /// <summary>
        /// Global catch-all to remove all existing enemy blimps
        /// </summary>
        public void RemoveEnemyBlimps()
        {
            foreach (Blimp blimp in FindObjectsByType<Blimp>(FindObjectsSortMode.None))
            {
                blimp.RemoveFromScene();
            }
        }

        /// <summary>
        /// Global catch-all to remove all existing enemy berserker
        /// </summary>
        public void RemoveBerserkers()
        {
            foreach (Berserker berserker in FindObjectsByType<Berserker>(FindObjectsSortMode.None))
            {
                berserker.RemoveFromScene();
            }
        }

    }


}