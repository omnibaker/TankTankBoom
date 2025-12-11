using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Sumfulla.TankTankBoom
{
    public class EnemyTank : MonoBehaviour, IExplodableEnemy
    {
        private const int MAX_ENEMY_HEALTH = 4;
        private int _tankHealth = MAX_ENEMY_HEALTH;

#pragma warning disable 0649
        [SerializeField] private GameObject _projectilePF = null;
        [SerializeField] private SpriteRenderer _damageState = null;
        [SerializeField] private SpriteRenderer _tankState = null;
        [SerializeField] private SpriteRenderer _barrelState = null;
        [SerializeField] private GameObject _barrel = null;
        [SerializeField] private Transform _angleVisual = null;
        [SerializeField] private GameObject _flag = null;
        [SerializeField] private GameObject _explosion = null;
        [SerializeField] private float _blastForce = 600f;
        [SerializeField] private bool _userInputAngle = false;
        [SerializeField] private float _gunAngle = 145f;
        [SerializeField] private float _roomForError = 0.03f;
#pragma warning restore 0649

        private float _trackedInputAngle;
        private float _difficulty = 1f;
        private float _projectileMass = 1f;
        private bool _scanningForPlayer;
        private bool _repeatedShooting;

        private float _lrFadeoutTime = 1f;
        private float _vel;
        private Animator _animator;
        private LineRenderer _lineRenderer = null;
        private Coroutine _fadeOutTrajectory;
        private Coroutine _hitFading;
        private Material _lrMaterial;
        private Rigidbody2D _rb;
        private LayerMask _layerMask;
        private Color _lrColor;
        private Color _lrColorFaded;

        private float _timeSinceLastShot = 10f; // TODO: Testing, return to 0/null when done

        [Header("HEALTH")]
        [SerializeField] private Color[] ShieldColors;
        public Sprite[] StateSprites;
        public Sprite[] ShieldSprites;
        public CameraShaker _cameraShake;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _lineRenderer = GetComponent<LineRenderer>();
            _tankState = GetComponent<SpriteRenderer>();
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            _tankHealth = MAX_ENEMY_HEALTH;
            _lrMaterial = _lineRenderer.material;
            _lrColor = _lineRenderer.material.color;
            _lrColorFaded = new Color(_lrColor.r, _lrColor.g, _lrColor.b, 0);
            _damageState.color = ShieldColors[ShieldColors.Length - 1];
            _barrelState.color = ShieldColors[ShieldColors.Length - 1];

            SetLayerMasks();
            StartCoroutine(KeepShooting());
            _gunAngle = 145f;
        }

        private void Update()
        {
            UpdateAngle();
            if (PlayManager.I.State.Current == RunState.PLAY)
            {
                TimedShooting();
            }
        }

        private void OnEnable()
        {
            PlayManager.I.State.OnPause += EnemyTankPaused;
            PlayManager.I.State.OnUnpause += EnemyTankUnpaused;
        }

        private void OnDisable()
        {
            if (PlayManager.IsInitialized)
            {
                PlayManager.I.State.OnPause -= EnemyTankPaused;
                PlayManager.I.State.OnUnpause -= EnemyTankUnpaused;
            }
        }

        /// <summary>
        /// Increments wait time for shot, and fires if limit criteria met (then resets)
        /// </summary>
        private void TimedShooting()
        {
            if (_timeSinceLastShot > GameRef.Enemies.GetFiringFrequency())
            {
                //StartCoroutine(FindValidTrajectoryWithVisualArc());
                FindValidTrajectoryInOneFrame();
                 _timeSinceLastShot = 0;
            }
            else
            {
                _timeSinceLastShot += Time.deltaTime;
            }
        }

        /// <summary>
        /// Calculated trajectory in single frame
        /// </summary>
        private void FindValidTrajectoryInOneFrame()
        {
            float zRotation = 90f;
            float blast = 400;
            float rotationIncrement = 0.1f;
            float blastIncrement = 1f;

            _angleVisual.rotation = Quaternion.Euler(0, 0, zRotation);
            _scanningForPlayer = true;

            int count = 0;
            int limit = Mathf.RoundToInt((180 - zRotation) / rotationIncrement);

            for (int i = 0; i < limit; i++)
            {
                IncrementRotation(rotationIncrement, ref zRotation);
                blast += blastIncrement;
                count++;
                DrawTrajectory(blast);
                if (!_scanningForPlayer)
                {
                    break;
                }

            }
            _blastForce = blast;

            if (!_scanningForPlayer)
            {
                FireProjectile(blast);
            }
        }

        /// <summary>
        /// Calculated trajectory and display progress of scan by running single increment per frame (for testing purposes) 
        /// </summary>
        private IEnumerator FindValidTrajectoryWithVisualArc()

        {
            float zRotation = 90f;
            float blast = _blastForce;
            float rotationIncrement = 0.1f;
            float blastIncrement = 1f;

            _angleVisual.rotation = Quaternion.Euler(0, 0, zRotation);
            _scanningForPlayer = true;

            int count = 0;
            float limit = Mathf.RoundToInt(180 - zRotation) / rotationIncrement;

            while (_scanningForPlayer && count < limit)
            {
                IncrementRotation(rotationIncrement, ref zRotation);
                blast += blastIncrement;
                count++;
                DrawTrajectory(blast);
                yield return null;
            }

            if (!_scanningForPlayer)
            {
                FireProjectile(blast);
            }
        }

        /// <summary>
        /// Assign player layer to layermask target
        /// </summary>
        private void SetLayerMasks()
        {
            _layerMask |= 1 << LayerMask.NameToLayer(LayerNames.PlayerTarget.ToString());
        }

        /// <summary>
        /// Kicks off coroutine to wait, aim, shoot, wait, aim shoot etc...
        /// </summary>
        private IEnumerator KeepShooting()
        {
            float t = 0;
            float rate = 0.1f;
            yield return new WaitForSeconds(3f);

            while (true)
            {
                if (_repeatedShooting)
                {
                    _lineRenderer.positionCount = 0;
                    if (t > rate)
                    {
                        FireProjectile();
                        t = Time.deltaTime;
                    }
                    else
                    {
                        t += Time.deltaTime;
                    }
                }

                yield return null;
            }
        }
        
        /// <summary>
        /// Moves barrel and updates data panel with latest angle
        /// </summary>
        private void UpdateAngle()
        {
            if (_userInputAngle)
            {
                if (_gunAngle != _trackedInputAngle)
                {
                    _angleVisual.rotation = Quaternion.Euler(0, 0, _gunAngle);
                    _trackedInputAngle = _gunAngle;
                }
            }
        }
  
        /// <summary>
        /// Interface method to cause explosion
        /// </summary>
        public void InflictDamage()
        {
            //if (ArtilleryManager.I.ReactiveBehaviour)
            if (true)
            {
                if (_tankHealth > 0)
                {
                    GameAudio.I.Play(SoundType.TankHitNotDestroyed);
                    GameAudio.I.Play(SoundType.GroundExplode01);

                    // Drop current health
                    _tankHealth--;
                    GameLog.Say($"<color='green'>Enemy Tank Hit!</color>: <color='red'>{_tankHealth}</color>");

                    // Shield + Color
                    int newDmgIndex = ShieldColors.Length - 1 - _tankHealth;
                    _damageState.color = ShieldColors[newDmgIndex];
                    _barrelState.color = ShieldColors[newDmgIndex];
                    _damageState.sprite = ShieldSprites[newDmgIndex];

                    // State graphic
                    int newStateIndex = StateSprites.Length - 2 - _tankHealth;
                    _tankState.sprite = StateSprites[newStateIndex];

                    TriggerDamageGlow();

                    PlayManager.I.Score.AddPoints(GameRef.Points.TANK_HIT);
                    //ArtilleryManager.I.AddGamePlayPoints(ArtilleryEnemies.POINT_TANK_HIT);
                    //ArtilleryManager.I.Points.CreateTextObject(ArtilleryEnemies.POINT_TANK_HIT, transform.position, Color.green);
                }
                else
                {
                    GameAudio.I.Play(SoundType.MachineExplode);
                    
                    Explode();
                }
            }
        }

        /// <summary>
        /// Triggers coroutine which displays shield sprites, stops any previous coroyutine run
        /// </summary>
        internal virtual void TriggerDamageGlow()
        {
            _damageState.gameObject.SetActive(true);
            _barrelState.gameObject.SetActive(true);
            if (_hitFading != null)
            {
                StopCoroutine(_hitFading);
                _hitFading = null;
            }
            _hitFading = StartCoroutine(FadeHitIndicator());
        }
        /// <summary>
        /// Coroutine that displays shield sprites and fades out color property upon impact
        /// </summary>
        public IEnumerator FadeHitIndicator()
        {
            Color shieldColor = _damageState.color;
            float t = 0;

            while (t <= 1f)
            {
                _damageState.color = new Color(shieldColor.r, shieldColor.g, shieldColor.b, Mathf.Lerp(1f, 0, t));
                _barrelState.color = new Color(shieldColor.r, shieldColor.g, shieldColor.b, Mathf.Lerp(1f, 0, t));
                t += Time.deltaTime;
                yield return null;
            }

            _hitFading = null; // Needed?
            _damageState.gameObject.SetActive(false);
            _barrelState.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Create projectile and starts process of firing it on current trajectory
        /// </summary>
        public void FireProjectile(float blastForce = 0)
        {
            Vector3 instantiatedPosition = _angleVisual.position + _angleVisual.right;
            GameObject projectile = Instantiate(_projectilePF, instantiatedPosition, _angleVisual.rotation);
            projectile.name = "Enemy Projectile";
            AdjustedBlastValue(ref blastForce);

            // Direction is right as initial position of cannon is 0 degress facing true right;
            projectile.GetComponent<TankProjectile>().Initialize(projectile.transform.right * blastForce);
            GameAudio.I.Play(SoundType.EnemyFire);

            MakeTrajectoryTemporarilyVisible();
        }
        
        /// <summary>
        /// Adjusts accuracy of enemy firing projection so not always direct hit
        /// </summary>
        private void AdjustedBlastValue(ref float force)
        {
            force = force == 0 ? _blastForce : force;
            float errorAdjustment = UnityEngine.Random.Range(-_roomForError, _roomForError);
            force *= _difficulty;
            force *= errorAdjustment + 1;
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
            float t = 0;
            _lrMaterial.color = _lrColor;

            while (t < 1)
            {
                _lrMaterial.color = Color.Lerp(_lrColor, _lrColorFaded, t);
                t += Time.deltaTime / _lrFadeoutTime;
                yield return null;
            }
        }

        /// <summary>
        /// Checks each scan point to see in line raycast is hitting the Tank Player's collider, returns true if hit made
        /// </summary>
        private bool CheckIntersectionForTankCollision(Vector2 startPos, Vector2 endPos, out Vector3 hitPos)
        {
            RaycastHit2D playerHit = Physics2D.Raycast(startPos, (endPos - startPos).normalized, Vector3.Distance(startPos, endPos), _layerMask);

            if (playerHit.collider != null)
            {
                hitPos = playerHit.point;
                return true;
            }

            hitPos = Vector3.zero;
            return false;
        }

        /// <summary>
        /// Updates rotation of cannon direction
        /// </summary>
        private void IncrementRotation(float increment, ref float zRotation)
        {
            zRotation += increment;
            _angleVisual.rotation = Quaternion.Euler(0, 0, zRotation);
            _gunAngle = zRotation;
            UpdateAngle();
        }

        /// <summary>
        /// Disables and physics related properties
        /// </summary>
        public void DisableNonExplodingParts()
        {
            _damageState.enabled = false;
            _tankState.enabled = false;
            _flag.SetActive(false);
            _barrel.SetActive(false);
            if(TryGetComponent(out BoxCollider2D bc))
            {
                bc.enabled = false;
            }
            Destroy(_rb);
        }
        
        /// <summary>
        /// Interface method to cause explosion and adds points for kill
        /// </summary>
        public void Explode()
        {
            PlayManager.I.State.Pause();
            PlayManager.I.Score.AddPoints(GameRef.Points.TANK_KILL);

            StopAllCoroutines();
            DisableNonExplodingParts();

            _explosion.SetActive(true);
            _animator.SetBool(GameRef.AnimationTags.READY_TO_EXLODE, true);

            //ArtilleryManager.I.AddGamePlayPoints(ArtilleryEnemies.POINT_TANK_KILL);
            //ArtilleryManager.I.Points.CreateTextObject(ArtilleryEnemies.POINT_TANK_KILL, transform.position, _killColor);

            // TODO: Should this be delayed...?
        }

        /// <summary>
        /// Interface method to stop exploding animation and remove object
        /// </summary>
        public void Die()
        {
            _animator.SetBool(GameRef.AnimationTags.READY_TO_EXLODE, false);
            _explosion.SetActive(false);
            PlayManager.I.OnEnemyTankDestroyed();
            Destroy(gameObject);
        }
        
        /// <summary>
        /// Behaviours to stop when game is paused
        /// </summary>
        private void EnemyTankPaused()
        {
            if (_rb != null)
            {
                _rb.constraints = RigidbodyConstraints2D.FreezeAll;
                _rb.gravityScale = 0;
                _rb.bodyType = RigidbodyType2D.Static;
            }
            else
            {
                GameLog.Say("PAUSE - Would normally error here");
            }
            GetComponent<BoxCollider2D>().enabled = true;
        }
        
        /// <summary>
        /// Behaviours to restart when game is unpaused
        /// </summary>
        private void EnemyTankUnpaused()
        {
            if (_rb != null)
            {
                _rb.bodyType = RigidbodyType2D.Dynamic;
                _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                _rb.gravityScale = 1;
            }
            else
            {
                GameLog.Say("PAUSE - Would normally error here");
            }
            GetComponent<BoxCollider2D>().enabled = true;
        }


        #region CALCULATING AND VISUALISING TRAJECTORY
        // CONCEPT ADAPTED FROM: https://schatzeder.medium.com/basic-trajectory-prediction-in-unity-8537b52e1b34
        /// <summary>
        /// Draws the visual trajectory path
        /// </summary>
        private void DrawTrajectory(float blastForce)
        {
            List<Vector2> arcCoordinates = SimulateArc(out bool hitTarget, blastForce);
            _lineRenderer.positionCount = arcCoordinates.Count;
            for (int a = 0; a < _lineRenderer.positionCount; a++)
            {
                if (hitTarget)
                {
                    _lineRenderer.SetPosition(a, arcCoordinates[a]); //Add each Calculated Step to a LineRenderer to display a Trajectory. Look inside LineRenderer in Unity to see exact points and amount of them
                    _scanningForPlayer = false;
                    break;
                }
                else
                {
                    _lineRenderer.SetPosition(a, arcCoordinates[a]); //Add each Calculated Step to a LineRenderer to display a Trajectory. Look inside LineRenderer in Unity to see exact points and amount of them
                }
            }
        }

        /// <summary>
        /// Calculates the positions points and path of the trajectory 
        /// </summary>
        private List<Vector2> SimulateArc(out bool hitTarget, float blastForce = 0)
        {
            hitTarget = false;
            blastForce = blastForce == 0 ? _blastForce : blastForce;

            // Segment points along the predicted path
            List<Vector2> points = new List<Vector2>();

            // How long we want to simulate the shot for
            float maxDuration = 5f;

            // How often we check the next position (smaller = smoother curve)
            float timeStep = 0.1f;

            // How many total steps we will simulate
            int maxSteps = (int)(maxDuration / timeStep);

            // The direction the cannon is pointin - Unity gives this to us already normalised
            Vector2 direction = _angleVisual.right;

            // Where the projectile starts. This uses the cannon tip (cannon position + its facing direction)
            Vector2 startPos = _angleVisual.position + _angleVisual.right;

            // Work out the projectile’s launch speed
            _vel = blastForce / _projectileMass * Time.fixedDeltaTime;

            for (int i = 0; i < maxSteps; ++i)
            {
                float t = i * timeStep;

                // Start position + (velocity * time * direction)
                Vector2 calculatedPosition = startPos + _vel * t * direction;

                // Apply gravity to the y axis, gravity pulls downward more and more over time
                calculatedPosition.y += Physics2D.gravity.y / 2f * (t * t);

                // Add this to the next entry on the list
                points.Add(calculatedPosition);

                // If projection encounters collision, stop adding positions
                int hitCount = 0;
                if (i > 0 && CheckIntersectionForTankCollision(points[points.Count - 2], calculatedPosition, out Vector3 hitPos))
                {
                    if (CheckIfAnyGroundCollisionsBeforeTank(hitPos, points, ref hitCount))
                    {
                        GameLog.Say($"Obscured by {hitCount} ground point(s) @ {GameUtils.GetVector3String("", hitPos)}");
                    }
                    else
                    {
                        hitTarget = true;
                        break;
                    }
                }
            }
            return points;
        }

        /// <summary>
        /// If tank fit found, checks to make sure there is no land tiles in the way, returns true is tank obscured
        /// </summary>
        private bool CheckIfAnyGroundCollisionsBeforeTank(Vector3 tankHitPoint, List<Vector2> lrPoints, ref int hitCount)
        {
            for (int i = 1; i < lrPoints.Count; i++)
            {
                Vector3 direction = (lrPoints[i] - lrPoints[i - 1]).normalized;
                float distance = Vector3.Distance(lrPoints[i - 1], lrPoints[i]);
                RaycastHit2D[] groundHits = Physics2D.RaycastAll(lrPoints[i - 1], direction, distance, 1 << LayerMask.NameToLayer(LayerNames.Ground.ToString()));
                if (groundHits.Length > 0)
                {
                    foreach (RaycastHit2D hit in groundHits)
                    {
                        hitCount++;
                        if (hit.point.x > tankHitPoint.x)
                        {
                            GameLog.Say($"Earlier hits found at {hit.point}");
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #endregion
    }

}