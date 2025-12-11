using System.Collections;
using UnityEngine;
using TMPro;

namespace Sumfulla.TankTankBoom
{
    /// <summary>
    /// TODO: Change to 'rising points' from original TMP versin to UITK (currently unused)
    /// </summary>
    public class PointsTextObject : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _text;

        private float _seconds = 1f;

        public void RiseWithFX(int qty, Color color)
        {
            _text.color = color;
            _text.text = qty.ToString();
            _text.gameObject.SetActive(true);

            StartCoroutine(Ascending());
        }

        private IEnumerator Ascending()
        {
            float t = 0;
            float time = 0;
            while (t < 1f)
            {
                transform.position += Vector3.up * t / 70f;
                t += Time.deltaTime / _seconds;
                time += Time.deltaTime;

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}