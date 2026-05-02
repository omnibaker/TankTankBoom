using System.Collections;
using UnityEngine;

namespace Sumfulla.TankTankBoom
{
    public class StrikeBomb : MonoBehaviour
    {
        private float _speed = 2f;
        private bool _isDropping = true;
        public PlayManager PlayMgr { get; set; }

        private void Awake()
        {
            // Play manager failsafe
            if (PlayMgr == null)
            {
                PlayMgr = FindAnyObjectByType<PlayManager>();
                if (PlayMgr == null)
                {
                    GameLog.Warn("PlayManager not assigned in StrikeBomb");
                }
            }
        }
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.CompareTag(TagNames.Enemy.ToString()))
            {
                collider.gameObject.GetComponent<IExplodableEnemy>().InflictDamage();
                PlayMgr.CameraShake();
                _speed = 0.5f;
            }
            else
            {
                TerrainController.Instance.DestroyTerrainSet(new Vector3(transform.position.x, transform.position.y, 0));
                _speed = 0.2f;
            }

            if(TryGetComponent(out Animator anim))
            {
                anim.SetBool(GameRef.AnimationTags.READY_TO_EXPLODE, true);
            }
            else
            {
                Die();
            }
        }

        /// <summary>
        /// Public coroutine which is watched to assess when raid is over
        /// </summary>
        public IEnumerator DropBombUntilImpact()
        {
            Vector3 dropWithRightMomentum = new Vector3(0.1f, -1.0f, 0.0f);
            _isDropping = true;

            while (_isDropping)
            {
                if (PlayMgr.State.Current == RunState.PLAY && _isDropping)
                {
                    transform.position = transform.position + _speed * Time.deltaTime * dropWithRightMomentum;
                }
                yield return null;
            }
        }

        /// <summary>
        /// Stop coroutine and remove object
        /// </summary>
        public void Die()
        {
            _isDropping = false;
            Destroy(gameObject);
        }
    }

}