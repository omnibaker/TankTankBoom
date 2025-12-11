using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sumfulla.TankTankBoom
{
    public class PlayerData
    {
        private float _currentPower;
        public float CurrentPower
        {
            get => _currentPower;
            set
            {
                _currentPower = value;
                OnPowerChanged?.Invoke(_currentPower);
            }
        }

        public float CurrentAngle { get; set; }

        private int _currentLives;
        public int CurrentLives
        {
            get => _currentLives;
            set
            {
                _currentLives = value;
                OnLivesChanged?.Invoke(_currentLives);
            }
        }

        // Events for reactive updates
        public event Action<float> OnPowerChanged;
        public event Action<int> OnLivesChanged;

        public void Reset()
        {
            CurrentPower = 0;
            CurrentAngle = 0;
            CurrentLives = 3;
        }
    }

    public class ProgressData
    {
        private int _milestoneStrike;
        private int _milestoneRapidFire;

        public event Action StrikeReadyEvent;
        public event Action RapidFireReadyEvent;

        /// <summary>
        /// Increments battle up one unit update to next level if reach 10 battles
        /// </summary>
        public void NextBattle()
        {
            GameData.CurrentBattle++;
            if (GameData.CurrentBattle % 10 == 0)
            {
                GameData.CurrentLevel++;
            }

            PlayManager.I.Score.CalculateRewardMultiplier();
        }

        /// <summary>
        /// Resets level/milestone values
        /// </summary>
        public void Reset(bool hardReset = false)
        {
            GameData.CurrentLevel = hardReset ? 1 : GameData.SelectedLevel;
            GameData.SelectedLevel = hardReset ? 1 : GameData.SelectedLevel;
            GameData.CurrentBattle = 1;
            _milestoneStrike = 0;
            _milestoneRapidFire = 0;
        }

        /// <summary>
        /// Runs a check on whether any of the RapidFire/AirStrike triggers have met their criteria
        /// </summary>
        public void RunMilestoneCheck(int pointsIncrease)
        {
            _milestoneStrike += pointsIncrease;
            _milestoneRapidFire += pointsIncrease;

            if (_milestoneStrike > GameRef.Milestone.MS_STRIKE)
            {
                StrikeReadyEvent?.Invoke();
                while (_milestoneStrike > GameRef.Milestone.MS_STRIKE)
                {
                    _milestoneStrike -= GameRef.Milestone.MS_STRIKE;
                }
            }

            if (_milestoneRapidFire > GameRef.Milestone.MS_RAPID_FIRE)
            {
                RapidFireReadyEvent?.Invoke();
                while(_milestoneRapidFire > GameRef.Milestone.MS_RAPID_FIRE)
                {
                    _milestoneRapidFire -= GameRef.Milestone.MS_RAPID_FIRE;
                }
            }
        }
    }

    public class ScoreData
    {
        public int CurrentScore { get; private set; }
        public int HighestLevelAchieved { get; set; } = 0;
        public int HighestScore { get; set; } = 0;
        public float RewardMultiplier { get; private set; } = 1f;

        /// <summary>
        /// Updates points multiplier based on current level
        /// </summary>
        public void CalculateRewardMultiplier()
        {
            RewardMultiplier = 1f + GameData.CurrentBattle * 0.01f;
        }

        /// <summary>
        /// Resets score to 0
        /// </summary>
        public void Reset()
        {
            CurrentScore = 0;
        }

        /// <summary>
        /// Updates score data, and various UI score elements
        /// </summary>
        public void AddPoints(int points, bool useRewardMultiplier = true)
        {
            int pointsIncrease = Mathf.RoundToInt(points * (useRewardMultiplier ? RewardMultiplier : 1f));
            CurrentScore += pointsIncrease;
            PlayManager.I.UIPlay.SetScoreLabel(CurrentScore.ToString());
            PlayManager.I.Progress.RunMilestoneCheck(pointsIncrease);
        }

        /// <summary>
        /// Returns true if the given score is higher than the saved highest
        /// </summary>
        public bool CheckForNewHighScore(int current)
        {
            return current > HighestScore;
        }
    }

    public class EnvironmentData
    {
        public float Wind { get; set; }

        /// <summary>
        /// Resets wind value to 0
        /// </summary>
        public void Reset()
        {
            Wind = 0f;
        }
    }

    public class RunStateData
    {
        public RunState Current;

        public event Action OnPause;
        public event Action OnUnpause;

        /// <summary>
        /// Updates state to PAUSED and runs any associated callbacks
        /// </summary>
        public void Pause()
        {
            Current = RunState.PAUSED;
            OnPause?.Invoke();
        }

        /// <summary>
        /// Updates state to PLAY and runs any associated callbacks
        /// </summary>
        public void Unpause()
        {
            Current = RunState.PLAY;
            OnUnpause?.Invoke();
        }
    }

    public class GameData
    {
        public static VisualElement Foreground;
        public static int SelectedLevel = 1;
        public static int CurrentLevel = 3;
        public static int CurrentBattle = 30;

        /// <summary>
        /// Public method to set starting level when selecting is optional
        /// </summary>
        public static void SetCurrentLevel(int level)
        {
            SelectedLevel = level;
            CurrentLevel = level;
            CurrentBattle = (level - 1) * 10 + 1;
        }
    }
}