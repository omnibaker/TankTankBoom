using UnityEngine;

namespace Sumfulla.TankTankBoom
{
    /// <summary>
    /// Bootstrap class that sets up core game components, starts game, then removes itself (needs to run first)
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class GameInit : MonoBehaviour
    {
        private void Awake()
        {
            // Set initial defaults if needed
            PrefRefDefaultCheck();
        }

        private void Start()
        {
            // Display Start Menu
            SceneController.I.GoToScene(GameRef.Scenes.MENU);
            Destroy(gameObject);
        }

        /// <summary>
        /// Used to set iniitial settings values
        /// </summary>
        private void PrefRefDefaultCheck()
        {
            if (PlayerPrefs.GetInt(GameRef.PrefRef.DEFAULT_CHECKED) == 0)
            {
                // Set up PrefRefs if needed
            }
        }
    }
}