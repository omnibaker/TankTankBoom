using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Sumfulla.TankTankBoom
{
    public class SkyUpdater : MonoBehaviour
    {
        [SerializeField] private List<Sprite> _skies;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        private int _skyIndex;

        public void UpdateSky()
        {
            _skyIndex = (_skyIndex + 1) % _skies.Count;
            _spriteRenderer.sprite = _skies[_skyIndex];
        }
    }
}
