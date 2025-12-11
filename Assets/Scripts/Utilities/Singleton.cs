using UnityEngine;

namespace Sumfulla.TankTankBoom
{
    /// <summary>
    /// Base class for any script that wants to act as a Singleton
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;
        public static bool IsInitialized { private set; get; } = false;

        public static T I
        {
            get
            {
                if (!IsInitialized)
                {
                    GameLog.Shout($"Singleton<{typeof(T)}> was accessed before initialization.");
                }
                return _instance;
            }
        }

        /// <summary>
        /// Static method for subclass to call on Awake to assign Instance immediately
        /// </summary>
        internal static void CreateInstance(T subclass, GameObject gameObject, bool doNotDestroy = true)
        {
            if (_instance == null)
            {
                _instance = subclass;
                GameLog.Say($"Created new <b>{typeof(T)}</b> singleton instance");
                
                if (doNotDestroy)
                {
                    DontDestroyOnLoad(gameObject);
                }
                IsInitialized = true;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                IsInitialized = false;
            }
        }
    }
}