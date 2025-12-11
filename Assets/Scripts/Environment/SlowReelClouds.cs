using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;



namespace Sumfulla.TankTankBoom
{
    public class SlowReelClouds : MonoBehaviour
    {
        private const int MIDDLE_INDEX = 1;
        private const float WIDTH = 30f;

        [SerializeField] private Transform _fgC1;
        [SerializeField] private Transform _fgC2;
        [SerializeField] private Transform _fgC3;
        [SerializeField] private Transform _bgC1;
        [SerializeField] private Transform _bgC2;
        [SerializeField] private Transform _bgC3;
        [SerializeField] private Transform _cloudContainer;

        private float _widthHalf;
        private Vector3 _pushFG;
        private Vector3 _pushBG;
        private Transform[] _cloudGroupsFG;
        private Transform[] _cloudGroupsBG;

        private void Start()
        {
            _widthHalf = WIDTH * 0.5f;
            _cloudGroupsFG = new Transform[] { _fgC1, _fgC2, _fgC3 };
            _cloudGroupsBG = new Transform[] { _bgC1, _bgC2, _bgC3 };
        }

        private void Update()
        {
            MoveClouds();
        }

        /// <summary>
        /// Increments foreground and background clouds in precalcualted direction
        /// </summary>
        private void MoveClouds()
        {
            if (PlayManager.I.State.Current == RunState.PAUSED) return;

            float dt = Time.deltaTime;

            // Move
            _fgC1.localPosition += _pushFG * dt;
            _fgC2.localPosition += _pushFG * dt;
            _fgC3.localPosition += _pushFG * dt;

            _bgC1.localPosition += _pushBG * dt;
            _bgC2.localPosition += _pushBG * dt;
            _bgC3.localPosition += _pushBG * dt;

            // Wrap
            HandleWrap(_cloudGroupsFG, _pushFG.x);
            HandleWrap(_cloudGroupsBG, _pushBG.x);
        }

        /// <summary>
        /// Checks if cloud groups have hit wrap limits and requeues if necessary
        /// </summary>
        private void HandleWrap(Transform[] groups, float moveX)
        {
            var mid = groups[MIDDLE_INDEX];
            float x = mid.localPosition.x;

            // Moving right, middle passed +width
            if (moveX > 0 && x > _widthHalf)
            {
                // rotate 2 to 1 to 0
                Transform b = groups[2];
                groups[2] = groups[1];
                groups[1] = groups[0];
                groups[0] = b;

                // mirror and reposition
                Transform g0 = groups[0];
                g0.localPosition = new Vector3(-g0.localPosition.x, g0.localPosition.y, g0.localPosition.z);

                float diff = Mathf.Abs(_widthHalf - x);
                g0.localPosition += Vector3.right * diff;
            }

            // Moving left, middle passed –width
            else if (moveX < 0 && x < -_widthHalf)
            {
                // rotate 0 to 2 to 1
                Transform b = groups[0];
                groups[0] = groups[1];
                groups[1] = groups[2];
                groups[2] = b;

                Transform g2 = groups[2];
                g2.localPosition = new Vector3(-g2.localPosition.x, g2.localPosition.y, g2.localPosition.z);

                float diff = Mathf.Abs(_widthHalf - x);
                g2.localPosition += Vector3.left * diff;
            }
        }

        /// <summary>
        /// Calculates push increment based on current wind value
        /// </summary>
        public void UpdatePush()
        {
            _pushFG = 0.7f * PlayManager.I.Environment.Wind * Vector3.right;
            _pushBG = 0.3f * PlayManager.I.Environment.Wind * Vector3.right;
        }

        /// <summary>
        /// Updates cloud object position based on tank position
        /// </summary>
        internal void UpdateClouds(Vector3 tankPosition)
        {
            transform.position = Vector3.up * (tankPosition.y);
        }
    }

}