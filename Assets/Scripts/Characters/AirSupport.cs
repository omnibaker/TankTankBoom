using UnityEngine;

namespace Sumfulla.TankTankBoom
{
    public class AirSupport : MonoBehaviour
    {
        [SerializeField] private GameObject _airStrikePF;

        private Vector3 _limitsStart;
        private Vector3 _limitsEnd;

        private AirStrike _strike;

        /// <summary>
        /// Public method to initiate an airstrike from AirStrike prefab
        /// </summary>
        public void LaunchStrikeFlyover()
        {
            // Determine entry position at top left of screen
            _limitsStart = Camera.main.ScreenToWorldPoint(new Vector3(-200f, Screen.height * 0.8f, 0));
            _limitsEnd = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width + 200f, Screen.height * 0.8f, 0));
            Vector3 entryPosition = new Vector3(_limitsStart.x, _limitsStart.y, Camera.main.nearClipPlane);

            // Create AirStrike plane object and initialise behaviour
            GameObject plane = Instantiate(_airStrikePF, entryPosition, Quaternion.identity);
            if (plane.TryGetComponent(out AirStrike strike))
            {
                _strike = strike;
                _strike.Init(_limitsEnd.x, PlayManager.I.StrikeFlyoverEnded, PlayManager.I.StrikeSuccesful);
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
                _strike.BombingCompleted();
            }
        }
    }
}