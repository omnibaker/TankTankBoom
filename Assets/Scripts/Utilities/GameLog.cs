using UnityEngine;

namespace Sumfulla.TankTankBoom
{
    public class GameLog : MonoBehaviour
    {
        /// <summary>
        /// Console log output with Game prefix
        /// </summary>
        public static void Say(string msg)
        {
            Debug.Log($"{GameRef.Core.LOG_LABEL}{msg}");
        }

        /// <summary>
        /// Console warning output with Game prefix
        /// </summary>
        public static void Warn(string msg)
        {
            Debug.LogWarning($"{GameRef.Core.LOG_LABEL}{msg}");
        }

        /// <summary>
        /// Console assertion output with Game prefix
        /// </summary>
        public static void Shout(string msg)
        {
            Debug.LogError($"{GameRef.Core.LOG_LABEL}{msg}");
        }
    }
}