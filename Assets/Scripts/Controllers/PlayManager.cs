using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


namespace Sumfulla.TankTankBoom
{
    [DefaultExecutionOrder(-1000)]
    public class PlayManager : Singleton<PlayManager>
    {
        [SerializeField] private GameObject _playerPF = null;
        [SerializeField] private Enemies _enemies;
        [SerializeField] private TerrainController _terrainController;
        [SerializeField] private AirSupport AirSupport;
        [SerializeField] private SlowReelClouds _clouds = null;
        [SerializeField] public bool GodMode;

        public event Action<float> OnStandardFireEvent;
        public event Action<float> OnRapidFireEvent;

        public PlayerData Player { get; private set; } = new PlayerData();
        public ProgressData Progress { get; private set; } = new ProgressData();
        public ScoreData Score { get; private set; } = new ScoreData();
        public EnvironmentData Environment { get; private set; } = new EnvironmentData();
        public RunStateData State { get; private set; } = new RunStateData();
        public UI_Play UIPlay { get; private set; }
        public FiringType Firing { get; set; }
        public int RapidFiresLeft { get; set; }

        private TankPlayer _player;
        private CameraShaker _cameraShaker;
        private Coroutine _oscillate;
        private StrikeState _strikeState;
        private bool _selectionAttempted;
        private bool _readyToFire;
        private int _timerValue;
        private float _readyGaugeWaitProgress;
        private float _movementFactor;
        private float _timerIncrement;
        private float _timeLeft;

        private void Awake()
        {
            CreateInstance(this, gameObject);
            State.Pause();
        }

        private void Start()
        {
            _cameraShaker = FindFirstObjectByType<CameraShaker>();
            ResetTimer();
            StartNewGame();
        }

        private void Update()
        {
            if (State.Current == RunState.PLAY)
            {
                TimerCountdown();
                IncrementWaitingGauge();
            }
        }

        private void OnEnable()
        {
            UIPlay = FindFirstObjectByType<UI_Play>();
            UIPlay.OnAccuracyHeldEvent += FirePressed;
            UIPlay.OnAccuracyReleasedEvent += StopOscillateAccuracy;
            UIPlay.OnStrikeEvent += StrikeButtonPressed;
            Progress.StrikeReadyEvent += AllowStrike;
            Progress.RapidFireReadyEvent += EnableRapidFire;
            OnStandardFireEvent += (value) => { ResetReadyToFire(); };
        }

        private void OnDisable()
        {
            UIPlay.OnAccuracyHeldEvent -= FirePressed;
            UIPlay.OnAccuracyReleasedEvent -= StopOscillateAccuracy;
            UIPlay.OnStrikeEvent -= StrikeButtonPressed;
            Progress.StrikeReadyEvent -= AllowStrike;
            Progress.RapidFireReadyEvent -= EnableRapidFire;
            OnStandardFireEvent -= (value) => { ResetReadyToFire(); };
        }


        /* - - - - - GAME FLOW - - - - - */
        /// <summary>
        /// Initial configuring/resets when a brand new game is started
        /// </summary>
        internal void StartNewGame()
        {
            // Pause to start with
            State.Pause();

            // Refresh for new game
            Player.Reset();
            EnableTurnBasedFire();
            DestroyPlayer();
            UIPlay.DisplayStrikeButton(false);

            // Reset UI values
            //GameLog.Warn("TODO[PlayManager]: Reset UI score -> 0");
            //GameLog.Warn("TODO[PlayManager]: Reset UI time -> 02:00");
            //GameLog.Warn("TODO[PlayManager]: Reset UI lives -> x3");

            // Reevaluate scoring algorithm
            Score.CalculateRewardMultiplier();

            // Initial a new battle
            StartCoroutine(StartNewBattle());
        }

        /// <summary>
        /// Initial configuring/resets when a new battle is started
        /// </summary>
        private IEnumerator StartNewBattle(bool newGame = false)
        {
            yield return new WaitForEndOfFrame();

            // Pause while battlefield builds
            State.Pause();

            // Reset objects and values
            RemoveAllAmmo();
            RemoveAllEnemies();
            DestroyPlayer();

            // Update UI
            _timeLeft = GameRef.Time.MIN_2;
            UIPlay.SetTimerLabel($"{GameUtils.GetTimeInFormattedString(_timeLeft)}");
            UIPlay.SetBattleLabel(GameData.CurrentBattle.ToString());
            UIPlay.SetScoreLabel(Score.CurrentScore.ToString());
            UIPlay.SetLivesLabel(Player.CurrentLives.ToString());
            UIPlay.DisplayGaugeChildren(false);

            // Create environment
            SkyUpdater su = FindAnyObjectByType<SkyUpdater>();
            if(su != null)
            {
                su.UpdateSky();
            }
            GameRef.Environment.GenerateNewWind();
            yield return new WaitForSeconds(0.1f);
            _terrainController.CreateNewBattlefieldTerrain();
            yield return new WaitForSeconds(0.1f);

            // Create enemies
            _enemies.CreateEnemyTank(TerrainController.Instance.TankPositions.Enemy);
            _enemies.StartBlimp();
            _enemies.StartBerserkerDroppers();

            // Setup clouds movements
            _clouds.UpdateClouds(TerrainController.Instance.TankPositions.Tank);
            _clouds.UpdatePush();

            // Reset player start state
            PreparePlayer();

            // Set init trajectory
            UIPlay.Angle = GameRef.Trajectory.ANGLE_SLIDER_INIT;
            UIPlay.Power = GameRef.Trajectory.POWER_SLIDER_INIT;

            // Announce game to start
            yield return new WaitForSeconds(1f);
            UIPlay.MessagePlay("READY!");
            yield return new WaitForSeconds(2f);
            UIPlay.MessagePlay("GO!");

            // Unpause
            State.Unpause();

            // Remove display
            yield return new WaitForSeconds(1.5f);
            UIPlay.MessagePlay("");
        }

        /// <summary>
        /// Behaviour when enemy tank is destroyed, completing battle
        /// </summary>
        public void OnEnemyTankDestroyed()
        {
            GameAudio.I.Play(SoundType.DestroyedEnemyTank);

            // Stop timer and player
            State.Pause();

            // Hide ready gauge
            UIPlay.DisplayGaugeChildren(false);

            // End any extra activities
            AirSupport.EndSupport();

            // Battle progress points
            Score.AddPoints(GameRef.Points.BATTLE_WON);

            // Display Message
            PopUp pu = PopUp.InstantiatePopUp();
            pu.UpdateTitle($"BATTLE {GameData.CurrentBattle} COMPLETE!");
            int points = Mathf.RoundToInt(_timeLeft * 5f * Score.RewardMultiplier);
            Coroutine calc = StartCoroutine(CalculateBonusPoints(pu, points));
            pu.AddButton(() =>
            {
                // If user doesn't want to wait for points to finish adding up, they can skip
                if (calc != null) StopCoroutine(calc);

                // Add points to actual data
                Score.AddPoints(points, false);

                // Start new wave
                Progress.NextBattle();
                StartCoroutine(StartNewBattle());
            }, "OK");

            //GameLog.Warn("TODO[PlayManager]: Update high scores if relevant");
        }

        /// <summary>
        /// When battle is lost, lives are checked and failure reason passed on if criteria to keep playing is met
        /// </summary>
        public void BattleFailed(FailureReason reason)
        {
            // Stop timer and player
            State.Pause();

            // Hide ready gauge
            UIPlay.DisplayGaugeChildren(false);

            // Check if game can proceed if lives to spare
            if (Player.CurrentLives > 0)
            {
                FailedButPlayAgain(reason);
            }
            // Game over
            else
            {
                GameOver();
            }
        }

        /// <summary>
        /// Presents UI pop up with option to restart battle or quit 
        /// </summary>
        public void FailedButPlayAgain(FailureReason reason)
        {
            GameAudio.I.Play(SoundType.FailedLevel);

            // Display Message
            PopUp pu = PopUp.InstantiatePopUp();
            pu.UpdateTitle($"TANK DESTROYED!");
            string plural = Player.CurrentLives == 1 ? "" : "s";
            pu.UpdateMessage($"You have {Player.CurrentLives} tank{plural} left.\nDo you want to keep playing?");

            // Remove life icon from display
            Player.CurrentLives--;

            // Confirm button
            pu.AddButton(() =>
            {
                // Remove rapid fire if was active during defeate
                EnableTurnBasedFire();

                // Restart battle
                StartCoroutine(StartNewBattle());

            }, "HELL YEAH!");

            // Decline button
            pu.AddButton(() =>
            {
                // Exit to main menu
                SceneController.I.GoToScene(GameRef.Scenes.MENU);
            }, "GOD, NO");
        }

        /// <summary>
        /// Returns string for header based on failure reason
        /// </summary>
        private string FailedMessage(FailureReason failure)
        {
            // TODO: Reintroduce time-outs, generic 'Battle  Failed' added for now
            //switch (failure)
            //{
            //    case FailureReason.Destroyed: return "DESTROYED!";
            //    case FailureReason.OutOfTime: return "OUT OF TIME!";
            //}
            return "BATTLE FAILED!";
        }

        /// <summary>
        /// Visual cycles through time decrease vs points increase in pop-up display
        /// </summary>
        private IEnumerator CalculateBonusPoints(PopUp popUpInstance, int points)
        {
            yield return new WaitForSeconds(1f);

            popUpInstance.UpdateMessage($"{GameUtils.GetTimeInFormattedString(_timeLeft)} \u279C 0");
            UIPlay.SetScoreLabel(Score.CurrentScore.ToString());
            yield return new WaitForSeconds(1f);

            float pointsSplit = points / _timeLeft;
            float visiblePoints = 0;
            while (visiblePoints < points)
            {
                _timeLeft -= 1f;
                visiblePoints += pointsSplit;
                popUpInstance.UpdateMessage($"{GameUtils.GetTimeInFormattedString(_timeLeft)} \u279C {(int)visiblePoints}");
                UIPlay.SetScoreLabel((Score.CurrentScore + (int)visiblePoints).ToString());
                yield return new WaitForSeconds(0.02f);
            }

            popUpInstance.UpdateMessage($"{GameUtils.GetTimeInFormattedString(0)} \u279C {points}");
            UIPlay.SetScoreLabel((Score.CurrentScore + points).ToString());
        }

        /// <summary>
        /// Presents UI pop up with option to start new game or quit 
        /// </summary>
        public void GameOver()
        {
            GameAudio.I.Play(SoundType.GameOver);

            // Display Message
            PopUp pu = PopUp.InstantiatePopUp();
            pu.UpdateTitle($"TANK DESTROYED!");
            pu.UpdateMessage("You are out of tanks. Would you like to start over again");

            // Confirm button
            pu.AddButton(() =>
            {
                // Remove rapid fire if was active during defeate
                EnableTurnBasedFire();

                // Restart battle
                StartNewGame();

            }, "HELL YEAH!");

            // Decline button
            pu.AddButton(() =>
            {
                // Exit to main menu
                SceneController.I.GoToScene(GameRef.Scenes.MENU);
            }, "GOD NO");

        }


        /* - - - - - ATTACKING - - - - - */
        /// <summary>
        /// Triggers coroutine to start accuracy gauge, and stops any previous coroutine run
        /// </summary>
        public void TriggerOscillateAccuracy()
        {
            if (Firing == FiringType.STD)
            {
                _selectionAttempted = false;
                if (_oscillate != null)
                {
                    StopCoroutine(_oscillate);
                }
                _oscillate = StartCoroutine(OscillateAccuracy());
            }
        }

        /// <summary>
        /// Presents accuracy gauge that oscillates back and forth so user can adjust strength of firing force
        /// </summary>
        public IEnumerator OscillateAccuracy()
        {
            UIPlay.DisplayAccuracySlider(true);

            while (!_selectionAttempted)
            {
                if (GameRef.Trajectory.PERIOD > Mathf.Epsilon)
                {
                    // Time-based cycle (in revolutions per second)
                    float cycles = Time.time / GameRef.Trajectory.PERIOD;

                    // Sine wave oscillation from -1 to +1
                    float sine = Mathf.Sin(cycles * GameRef.Maths.TAU);

                    // Convert to range -0.5 to +0.5
                    _movementFactor = (sine + 1f) * 0.5f - 0.5f;

                    // Update UI slider values
                    UIPlay.SetAccuracySlider(_movementFactor);
                }
                yield return null;
            }

            // Fire trajectory
            OnStandardFireEvent?.Invoke(_movementFactor);

            // Remove slider element
            UIPlay.DisplayAccuracySlider(false);

            // Reset for next use
            _selectionAttempted = false;
        }

        /// <summary>
        /// Triggers while 'out' condition to stop oscillation
        /// </summary>
        public void StopOscillateAccuracy()
        {
            if (_oscillate != null)
            {
                _selectionAttempted = true;
            }
        }

        /// <summary>
        /// Resets properties for next firing stage
        /// </summary>
        private void ResetReadyToFire()
        {
            UIPlay.ActivateFireButton(false);
            UIPlay.DisplayGaugeChildren(false);
            _readyToFire = false;
        }

        /// <summary>
        /// Checks current firing type and invokes appopriate firing behaviour
        /// </summary>
        public void FirePressed()
        {
            if (Firing == FiringType.STD)
            {
                TriggerOscillateAccuracy();
            }
            else if (Firing == FiringType.RAPID)
            {
                OnRapidFireEvent?.Invoke(0);
            }
        }

        /// <summary>
        /// Increments the 'waiting to fire' graphic with the next bar up until full and ready to fire
        /// </summary>
        private void IncrementWaitingGauge()
        {
            if (!_readyToFire)
            {
                // Hold until next time limit met
                if (_readyGaugeWaitProgress < 1f)
                {
                    float waitFactor = Firing == FiringType.STD ? GameRef.Time.READY_GAUGE_WAIT_STD : GameRef.Time.READY_GAUGE_WAIT_RAPID;
                    _readyGaugeWaitProgress += Time.deltaTime / waitFactor;
                }
                // Display next block in guage
                else
                {
                    if (UIPlay.DisplayNextGaugeChild())
                    {
                        _readyToFire = true;
                        UIPlay.ActivateFireButton(true);
                        _readyGaugeWaitProgress = 0;
                    }

                    // Reset wait time, account for any negative offset so time is evened out
                    _readyGaugeWaitProgress--;
                }
            }
        }

        /// <summary>
        /// Triggers method which shakes camera giving the impression of impact vibrations
        /// </summary>
        public void CameraShake()
        {
            if(_cameraShaker != null)
            {
                _cameraShaker.StartShake();
            }
        }

        /// <summary>
        /// When 'rapid fire' milestone is met, updates firing behaviours and UI components
        /// </summary>
        private void EnableRapidFire()
        {
            // No point restarting if already happening
            if (Firing == FiringType.RAPID) return;

            _selectionAttempted = true;
            RapidFiresLeft = 1;
            Firing = FiringType.RAPID;
            UIPlay.ColorGaugeElements(GaugeColor.RAPID);
        }

        /// <summary>
        /// When standard turn-based type is returned to, updates firing behaviours and UI components
        /// </summary>
        public void EnableTurnBasedFire()
        {
            RapidFiresLeft = 0;
            Firing = FiringType.STD;
            UIPlay.ColorGaugeElements(GaugeColor.STANDARD);
        }

        /// <summary>
        /// When 'air strike' milestone is met, updates state properties and UI components
        /// </summary>
        public void AllowStrike()
        {
            if (_strikeState == StrikeState.DORMANT)
            {
                UIPlay.DisplayStrikeButton(true);
                UIPlay.EnableStrikeButton(true);
                _strikeState = StrikeState.READY;
            }
        }

        /// <summary>
        /// When 'air strike' mission is completed, updates state properties and UI components
        /// </summary>
        public void StrikeSuccesful()
        {
            if (_strikeState == StrikeState.DROPPING)
            {
                UIPlay.UpdateStrikeImage(GameUtils.UITK.GetUIIcon(GameRef.Textures.ICON_STRIKE__PLANE));
                UIPlay.EnableStrikeButton(false);
                UIPlay.DisplayStrikeButton(false);
                _strikeState = StrikeState.DORMANT;
            }
        }

        /// <summary>
        /// Updates state of 'air strike' process, moving from 'launch plane' to 'drop bombs'
        /// </summary>
        public void StrikeButtonPressed()
        {
            if (_strikeState == StrikeState.READY)
            {
                AirSupport.LaunchStrikeFlyover();
                UIPlay.UpdateStrikeImage(GameUtils.UITK.GetUIIcon(GameRef.Textures.ICON_STRIKE__BOMB));
                _strikeState = StrikeState.AIRBORNE;
            }
            else if (_strikeState == StrikeState.AIRBORNE)
            {
                _strikeState = StrikeState.DROPPING;
                UIPlay.EnableStrikeButton(false);
                UIPlay.DisplayStrikeButton(false);
                AirSupport.DropBombs();
            }
        }

        /// <summary>
        /// Called when plane reaches end of screen, if bombs have not been released, resets to allow new fly-over
        /// </summary>
        public void StrikeFlyoverEnded(bool bombsDropped)
        {
            if (!bombsDropped)
            {
                UIPlay.UpdateStrikeImage(GameUtils.UITK.GetUIIcon(GameRef.Textures.ICON_STRIKE__PLANE));
                UIPlay.EnableStrikeButton(true);
                _strikeState = StrikeState.READY;
            }
        }


        /* - - - - - MANAGE OBJECTS - - - - - */
        /// <summary>
        /// Sets up player for scene by repositioning and resetting data
        /// </summary>
        private void PreparePlayer()
        {
            if (_player == null)
            {
                _player = CreatePlayer();
            }

            // Set up player for next cave
            _player.UpdateTankPosition(_terrainController.TankPositions);
            _player.RestorePlayerState();
        }

        /// <summary>
        /// Removes player from scene
        /// </summary>
        public void DestroyPlayer()
        {
            if (_player != null)
            {
                Destroy(_player.gameObject);
                _player = null;
            }
        }

        /// <summary>
        /// Stops any coroutines being run by enemies in monitored list, and removes all enemy objects from scene
        /// </summary>
        public void RemoveAllEnemies()
        {
            _enemies.StopAllCoroutines();
            _enemies.RemoveEnemyBlimps();
            _enemies.RemoveBerserkers();
            _enemies.RemoveBerserkerDroppers();
            _enemies.RemoveEnemyTank();
        }

        /// <summary>
        /// Removes and projectiles/bombs from scene
        /// </summary>
        public void RemoveAllAmmo()
        {
            TankProjectile[] projectiles = FindObjectsByType<TankProjectile>(FindObjectsSortMode.None);
            foreach (TankProjectile p in projectiles)
            {
                p.Die();
            }
            StrikeBomb[] bombs = FindObjectsByType<StrikeBomb>(FindObjectsSortMode.None);
            foreach (StrikeBomb b in bombs)
            {
                b.Die();
            }
        }

        /// <summary>
        /// Creates new tank player instance and returns TankPlayer component 
        /// </summary>
        /// <returns></returns>
        private TankPlayer CreatePlayer()
        {
            GameObject player = Instantiate(_playerPF, _terrainController.TankPositions.Tank, Quaternion.identity);
            return player.GetComponent<TankPlayer>();
        }

        /// <summary>
        /// Triggers request for berserker noise variant from 'Enemies' component and returns value
        /// </summary>
        public bool GetBerserkerNoise()
        {
            return _enemies.ToggleBerserkerJump();
        }


        /* - - - - - TIMER  - - - - - */
        /// <summary>
        /// Resets game timer at default time limit countdown (unless specified)
        /// </summary>
        private void ResetTimer(int duration = GameRef.Time.SEC_5)
        {
            _timerValue = duration;
            UIPlay.TimerUpdateLabel(GameUtils.Time.GetTimeInFormattedString(_timerValue));
        }

        /// <summary>
        /// Counts down in frame delta time until a second has passed and then actions increment changes
        /// </summary>
        private void TimerCountdown()
        {
            _timerIncrement -= Time.deltaTime;

            if (_timerIncrement < 0)
            {
                _timerIncrement += 1f;
                TimerIncrementSecond();
            }
        }

        /// <summary>
        /// Increments countdown a second and updates timer UI
        /// </summary>
        private void TimerIncrementSecond()
        {
            _timerValue -= 1;
            UIPlay.TimerUpdateLabel(GameUtils.Time.GetTimeInFormattedString(_timerValue));
        }


        /* - - - - - TODO / UNUSED  - - - - - */
        /// <summary>
        /// TODO: WHAT WAS THIS INITIALLY...?
        /// </summary>
        public void DisableRapidFire()
        {
            RapidFiresLeft = 0;
            Firing = FiringType.STD;
        }

        /// <summary>
        /// TODO: HAS THIS BEEN OVERWRITTEN WITH DIFFERTENT PROCESS?
        /// </summary>
        public void ResetGame(bool hardReset = false)
        {
            Player.Reset();
            Progress.Reset(hardReset);
            Score.Reset();
            Environment.Reset();
        }
    }
}
