using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sumfulla.TankTankBoom
{
    public interface IExplodableEnemy
    {
        void InflictDamage();

        void Explode();

        void DisableNonExplodingParts();

        void Die();
    }
}