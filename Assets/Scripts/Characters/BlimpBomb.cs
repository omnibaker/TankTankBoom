using UnityEngine;

namespace Sumfulla.TankTankBoom
{
    public class BlimpBomb : MonoBehaviour
    {
        private float _speed = 2f;

        private void Update()
        {
            DropToGround();
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.CompareTag(TagNames.PlayerTank.ToString()))
            {
                if(collider.gameObject.TryGetComponent(out TankPlayer player))
                {
                    player.InflictDamage();
                }
                PlayManager.I.CameraShake();
                _speed = 0.5f;
            }
            else
            {
                TerrainController.Instance.DestroyTerrainSet(new Vector3(transform.position.x, transform.position.y, 0));
                _speed = 0.2f;
            }

            // Set explosion animation
            if(TryGetComponent(out Animator anim))
            {
                anim.SetBool(GameRef.AnimationTags.READY_TO_EXLODE, true);
            }
            else
            {
                Die();
            }
        }

        /// <summary>
        /// Incrementall drops to ground each frame
        /// </summary>
        private void DropToGround()
        {
            if (PlayManager.I.State.Current == RunState.PAUSED) return;
         
            transform.position = transform.position + _speed * Time.deltaTime * Vector3.down;
        }

        /// <summary>
        /// Remove bomb
        /// </summary>
        public void Die()
        {
            Destroy(gameObject);
        }
    }
}