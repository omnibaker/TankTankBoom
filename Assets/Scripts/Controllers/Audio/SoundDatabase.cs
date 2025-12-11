using System.Collections.Generic;
using UnityEngine;

namespace Sumfulla.TankTankBoom
{
    [CreateAssetMenu(fileName = "SoundDatabase", menuName = "Audio/SoundDatabase")]
    public class SoundDatabase : ScriptableObject
    {
        [SerializeField] List<SoundData> _sounds;

        private Dictionary<SoundType, SoundData> _lookup;

        public void Init()
        {
            _lookup = new Dictionary<SoundType, SoundData>();
            foreach (SoundData data in _sounds)
            {
                if (!_lookup.ContainsKey(data.SoundType))
                {
                    if (data.CheckValidity())
                    {
                        _lookup[data.SoundType] = data;
                    }
                }
            }
        }

        public SoundData Get(SoundType type)
        {
            if (_lookup == null)
            {
                Init();
            }

            _lookup.TryGetValue(type, out SoundData data);

            return data;
        }
    }
}