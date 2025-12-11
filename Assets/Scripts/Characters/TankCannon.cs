using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


namespace Sumfulla.TankTankBoom
{
    [RequireComponent(typeof(LineRenderer))]
    public class TankCannon : MonoBehaviour
    {
        private const float LR_FADEOUT_TIME = 3f;
        private const float COLLISION_CHECKRADIUS = 0.1f;
        private const float FORCE_FACTOR = 125f;

        [SerializeField] private GameObject _projectilePF;
        [SerializeField] private Transform _cannon;

        private LineRenderer _lineRenderer;
        private Material _lrMaterial;
        private Color _lrColor;
        private Color _lrColorFaded;
        private Coroutine _fadeOutTrajectory;
        private float _curAngle;
        private float _vel;
        private float _projectileMass;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _projectileMass = _projectilePF.GetComponent<Rigidbody2D>().mass;
        }

        private void Start()
        {
            _lrMaterial = _lineRenderer.material;
            _lrColor = _lineRenderer.material.color;
            _lrColorFaded = new Color(_lrColor.r, _lrColor.g, _lrColor.b, 0);
            UpdateAngle(PlayManager.I.UIPlay.Angle);
        }

        private void OnEnable()
        {
            PlayManager.I.UIPlay.OnAngleChangeEvent += UpdateAngle;
            PlayManager.I.UIPlay.OnAnyChangeEvent += DrawTrajectory;
            PlayManager.I.OnStandardFireEvent += FireProjectile;
            PlayManager.I.OnRapidFireEvent += FireProjectile;
        }

        private void OnDisable()
        {
            if (PlayManager.IsInitialized)
            {
                PlayManager.I.UIPlay.OnAngleChangeEvent -= UpdateAngle;
                PlayManager.I.UIPlay.OnAnyChangeEvent -= DrawTrajectory;
                PlayManager.I.OnStandardFireEvent -= FireProjectile;
                PlayManager.I.OnRapidFireEvent -= FireProjectile;
            }
        }

        /// <summary>
        /// Trigger trajectory visiblity fade out coroutine
        /// </summary>
        private void MakeTrajectoryTemporarilyVisible()
        {
            if (_fadeOutTrajectory != null)
            {
                StopCoroutine(_fadeOutTrajectory);
            }
            _fadeOutTrajectory = StartCoroutine(SlowlyFadeOutTrajectory());
        }

        /// <summary>
        /// Displays line renderer tragectory display, then fades out over given duration
        /// </summary>
        private IEnumerator SlowlyFadeOutTrajectory()
        {
            yield return new WaitUntil(() => _lrMaterial != null);

            float t = 0;
            _lrMaterial.color = _lrColor;

            while (t < 1)
            {
                _lrMaterial.color = Color.Lerp(_lrColor, _lrColorFaded, t);
                t += Time.deltaTime / LR_FADEOUT_TIME;
                yield return null;
            }
        }

        /// <summary>
        /// Disables cannon object in scene
        /// </summary>
        internal void HideCannon()
        {
            _cannon.gameObject.SetActive(false);
        }

        /// <summary>
        /// Create a series of projectiles and that fire close to main trajectory
        /// </summary>
        public IEnumerator RapidFireProjectile(float variation)
        {
            float amount = 5f;
            for (int i = 0; i < amount; i++)
            {
                Vector3 instantiatedPosition = _cannon.position + _cannon.right;
                GameObject projectile = Instantiate(_projectilePF, instantiatedPosition, Quaternion.identity);
                float incrementalVariation = (variation + (i - amount / 2f)) / 10f;
                Vector3 directionalForce = _cannon.right * (PlayManager.I.Player.CurrentPower + incrementalVariation * FORCE_FACTOR);
                projectile.GetComponent<TankProjectile>().Initialize(directionalForce, true);

                // Alternate firing sounds
                if (i % 2f == 0)
                {
                    GameAudio.I.Play(SoundType.PlayerFire01);
                }
                else
                {
                    GameAudio.I.Play(SoundType.PlayerFire02);
                }

                yield return new WaitForSeconds(0.2f);
            }
        }

        /// <summary>
        /// Create projectile and starts process of firing it on current trajectory
        /// </summary>
        public void FireProjectile(float variation)
        {
            if (PlayManager.I.Firing == FiringType.RAPID)
            {
                StartCoroutine(RapidFireProjectile(variation));

                if (--PlayManager.I.RapidFiresLeft <= 0)
                {
                    PlayManager.I.EnableTurnBasedFire();
                }
            }
            else
            {
                Vector3 instantiatedPosition = _cannon.position + _cannon.right;
                GameObject projectile = Instantiate(_projectilePF, instantiatedPosition, Quaternion.identity);
                Vector3 directionalForce = _cannon.right * (PlayManager.I.Player.CurrentPower + variation * FORCE_FACTOR);
                projectile.GetComponent<TankProjectile>().Initialize(directionalForce, true);
                GameAudio.I.Play(SoundType.PlayerFire01);
            }
        }

        /// <summary>
        /// Moves barrel and updates data panel with latest angle
        /// </summary>
        private void UpdateAngle(float newAngle)
        {
            _curAngle = newAngle;
            _cannon.rotation = Quaternion.AngleAxis(90 - _curAngle, Vector3.forward);
        }


        #region CALCULATING AND VISUALISING TRAJECTORY

        // CONCEPT ADAPTED FROM: https://schatzeder.medium.com/basic-trajectory-prediction-in-unity-8537b52e1b34
        /// <summary>
        /// Draws the visual trajectory path
        /// </summary>    
        private void DrawTrajectory()
        {
            MakeTrajectoryTemporarilyVisible();
            _lineRenderer.positionCount = SimulateArc().Count;
            for (int a = 0; a < _lineRenderer.positionCount; a++)
            {
                _lineRenderer.SetPosition(a, SimulateArc()[a]); //Add each Calculated Step to a LineRenderer to display a Trajectory. Look inside LineRenderer in Unity to see exact points and amount of them
            }
        }

        /// <summary>
        ///  Works out the path the projectile will follow and returns a list of points for the line renderer to draw
        /// </summary>
        private List<Vector2> SimulateArc()
        {
            // Segment points along the predicted path
            List<Vector2> points = new List<Vector2>();

            // How long we want to simulate the shot for
            float maxDuration = 5f;

            // How often we check the next position (smaller = smoother curve)
            float timeStep = 0.1f;

            // How many total steps we will simulate
            int maxSteps = (int)(maxDuration / timeStep);

            // The direction the cannon is pointin - Unity gives this to us already normalised
            Vector2 direction = _cannon.right;

            // Where the projectile starts. This uses the cannon tip (cannon position + its facing direction)
            Vector2 startPos = _cannon.position + _cannon.right;

            // Work out the projectile’s launch speed
            _vel = PlayManager.I.Player.CurrentPower / _projectileMass * Time.fixedDeltaTime;

            for (int i = 0; i < maxSteps; ++i)
            {
                float t = i * timeStep;

                // Start position + (velocity * time * direction)
                Vector2 calculatedPosition = startPos + _vel * t * direction;

                // Apply gravity to the y axis, gravity pulls downward more and more over time
                calculatedPosition.y += Physics2D.gravity.y / 2f * (t * t);

                // Add this point to the list
                points.Add(calculatedPosition);

                // Stop early if we hit something.s
                if (CheckForCollision(calculatedPosition)) break;
            }
            return points;
        }

        /// <summary>
        /// Checks if latest path estimate has hit a collider
        /// </summary>
        private bool CheckForCollision(Vector2 position)
        {
            // Measure collision via a small circle at the latest position
            Collider2D[] hits = Physics2D.OverlapCircleAll(position, COLLISION_CHECKRADIUS); 
            if (hits.Length > 0) 
            {
                // Ignore if hits player tank (keep for testing)
                //string hitObject = hits[0].gameObject.name;
                //if (hitObject.Equals("TankGood") || hitObject.Equals("TankGood(Clone)") || hitObject.Equals("Cannon"))
                //{
                //    GameLog.Say($"Trajectory check hit {hitObject} - false alarm");
                //    return false;
                //}

                // Return true if something is hit, stopping arc simulation
                return true;
            }
            return false;
        }

        #endregion

    }

}