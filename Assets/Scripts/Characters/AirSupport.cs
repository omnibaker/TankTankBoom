using System;
using UnityEngine;

namespace Sumfulla.TankTankBoom
{
    public class AirSupport : MonoBehaviour
    {
        private const float SPAWN_OFFSET = 200f;
        private const float HEIGHT_PERCENT = 0.8f;

        [SerializeField] private GameObject _airStrikePF;

        private AirStrike _strike;

        /// <summary>
        /// Public method to initiate an airstrike from AirStrike prefab
        /// </summary>
        public void LaunchStrikeFlyover(Action<bool> strikeFlyoverEnded, Action strikeSuccessful)
        {
            Camera cam = Camera.main;

            // Determine entry position at top left of screen
            Vector3 limitsStart = cam.ScreenToWorldPoint(new Vector3(-SPAWN_OFFSET, Screen.height * HEIGHT_PERCENT, 0));
            Vector3 limitsEnd = cam.ScreenToWorldPoint(new Vector3(Screen.width + SPAWN_OFFSET, Screen.height * HEIGHT_PERCENT, 0));
            Vector3 entryPosition = new Vector3(limitsStart.x, limitsStart.y, 0);

            // Create AirStrike plane object and initialise behaviour
            GameObject plane = Instantiate(_airStrikePF, entryPosition, Quaternion.identity);
            if (plane.TryGetComponent(out AirStrike strike))
            {
                _strike = strike;
                _strike.Init(limitsEnd.x, strikeFlyoverEnded, strikeSuccessful);
            }
        }

        /// <summary>
        /// Public method to trigger previously instantiated plane to drop bombs
        /// </summary>
        public void DropBombs()
        {
            if(_strike != null)
            {
                _strike.ExecuteStrike();
            }
        }

        /// <summary>
        /// Public method to kill any strike currentluy underway
        /// </summary>
        public void EndSupport()
        {
            if (_strike != null)
            {
                _strike.CompleteStrike();
            }
        }
    }
}