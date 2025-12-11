using UnityEngine;

namespace Sumfulla.TankTankBoom
{
    [CreateAssetMenu(fileName = "SoundData", menuName = "Audio/SoundData")]
    public class SoundData : ScriptableObject
    {
        public SoundType SoundType;
        public AudioClip Clip;
        public float DefaultVolume = 1f;

        public bool CheckValidity()
        {
            return Clip != null;
        }
    }
}