using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sumfulla.TankTankBoom
{
    public class UI_Play : MonoBehaviour
    {
        // UI Panels
        public UI_Info Info { get; set; }

        // Custom Button
        private IconButton _goToMenu;
        private IconButton _openInfo;

        // Messages
        private Coroutine _messageHide;

        // Visual Elements
        private VisualElement _root;
        private Button _fire;
        private Button _strike;
        private VisualElement _fireBase;
        private VisualElement _strikeBase;
        private Label _message;
        private Label _level;
        private Label _score;
        private Label _timer;
        private Label _windLabel;
        private Label _fireLabel;
        private Label _rapidFireLabel;
        private Label _livesLabel;

        private ImageSlider _angleSlider;
        private ImageSlider _powerSlider;
        private VisualElement _readyGauge;
        private VisualElement _strikeIcon;
        private VisualElement _windLeftArrow;
        private VisualElement _windRightArrow;

        public event Action<float> OnAngleChangeEvent;
        public event Action<float> OnPowerChangeEvent;
        public event Action<float> OnAccuracyChangeEvent;
        public event Action OnAccuracyHeldEvent;
        public event Action OnAccuracyReleasedEvent;
        public event Action OnAnyChangeEvent;
        public event Action OnStrikeEvent;

        // Sliders
        private Slider _angle;
        public float Angle
        {
            get => _angle.value;
            set => _angle.value = value;
        }

        private Slider _power;
        public float Power
        {
            get => _power.value;
            set => _power.value = value;
        }

        private Slider _accuracy;
        public float Accuracy
        {
            get => _accuracy.value;
            set => _accuracy.value = value;
        }

        private bool _fireButtonIsPressed;
        private bool _strikeButtonIsPressed;
    
        private void OnEnable()
        {
            // Get central UI document
            UIDocument uid = GetComponent<UIDocument>();

            // Instantiate base Menu UI and configure
            _root = GameUtils.UITK.ConfigureRoot(uid, GameRef.UIRef.UXML_PLAY);

            // Get UI panel components
            Info = TryGetComponent(out UI_Info i) ? i : gameObject.AddComponent<UI_Info>();

            Init_Play();
        }

        /// <summary>
        /// Configures and sets up Menu UI for runtime use
        /// </summary>
        private void Init_Play()
        {
            _message = _root.Q<Label>(GameRef.UIRef.PLAY_MESSAGE);
            _timer = _root.Q<Label>(GameRef.UIRef.PLAY_TIMER);
            _livesLabel = _root.Q<Label>(GameRef.UIRef.PLAY_LIVES_LABEL);
            _readyGauge = _root.Q(GameRef.UIRef.PLAY_READY_GAUGE);
            _fireLabel = _readyGauge.Q<Label>(GameRef.UIRef.PLAY_READY_GAUGE__FIRE);
            _rapidFireLabel = _readyGauge.Q<Label>(GameRef.UIRef.PLAY_READY_GAUGE__FIRE_LABEL);
            _strikeBase = _root.Q<VisualElement>(GameRef.UIRef.PLAY_STRIKE__BASE);
            _strike = _root.Q<Button>(GameRef.UIRef.PLAY_STRIKE__BUTTON);
            _strikeIcon = _root.Q(GameRef.UIRef.PLAY_STRIKE__ICON);
            _level = _root.Q<Label>(GameRef.UIRef.PLAY_LEVEL);
            _score = _root.Q<Label>(GameRef.UIRef.PLAY_SCORE);
            _angleSlider = _root.Q<ImageSlider>(GameRef.UIRef.PLAY_TRAJECTORY_ANGLE);
            _angleSlider.UpdateMinMaxDefault(0, 90, 45);
            _powerSlider = _root.Q<ImageSlider>(GameRef.UIRef.PLAY_TRAJECTORY_POWER);
            _powerSlider.UpdateMinMaxDefault(0, 1, 1);
            _windLabel = _root.Q<Label>(GameRef.UIRef.PLAY_WIND__LABEL);
            _windLeftArrow = _root.Q(GameRef.UIRef.PLAY_WIND__LEFT_ARROW);
            _windRightArrow = _root.Q(GameRef.UIRef.PLAY_WIND__RIGHT_ARROW);
            _fireBase = _root.Q<VisualElement>(GameRef.UIRef.PLAY_FIRE__BASE);

            // FIRE button
            _fire = _root.Q<Button>(GameRef.UIRef.PLAY_FIRE_BUTTON);
            _fire.RegisterCallback<PointerDownEvent>(evt =>
            {
                _fireButtonIsPressed = true;
                OnAccuracyHeldEvent?.Invoke();
                ShowFireAsPressed();
                evt.StopPropagation();
            }, TrickleDown.TrickleDown);
            _fire.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (_fireButtonIsPressed)
                {
                    OnAccuracyReleasedEvent?.Invoke();
                    ShowFireAsReleased();
                    DisplayFireLabel(false);
                }
                _fireButtonIsPressed = false;
            });
            _fire.RegisterCallback<PointerOutEvent>(evt =>
            {
                if (_fireButtonIsPressed)
                {
                    OnAccuracyReleasedEvent?.Invoke();
                    ShowFireAsReleased();
                }
                _fireButtonIsPressed = false;
            });
            _fire.RegisterCallback<PointerLeaveEvent>(evt =>
            {
                if (_fireButtonIsPressed)
                {
                    OnAccuracyReleasedEvent?.Invoke();
                    ShowFireAsReleased();
                }
                _fireButtonIsPressed = false;
            });

            // STRIKE button
            _strike.RegisterCallback<ClickEvent>((evt) =>
            {
                OnStrikeEvent?.Invoke();
                evt.StopPropagation();
            }, TrickleDown.TrickleDown);
            _strike.RegisterCallback<PointerDownEvent>(evt =>
            {
                _strikeButtonIsPressed = true;
                ShowStrikeAsPressed();
                evt.StopPropagation();
            }, TrickleDown.TrickleDown);
            _strike.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (_strikeButtonIsPressed)
                {
                    ShowStrikeAsReleased();
                }
                _strikeButtonIsPressed = false;
            });
            _strike.RegisterCallback<PointerOutEvent>(evt =>
            {
                if (_strikeButtonIsPressed)
                {
                    ShowStrikeAsReleased();
                }
                _strikeButtonIsPressed = false;
            });
            _strike.RegisterCallback<PointerLeaveEvent>(evt =>
            {
                if (_strikeButtonIsPressed)
                {
                    ShowStrikeAsReleased();
                }
                _strikeButtonIsPressed = false;
            });

            // ANGLE slider
            _angle = _angleSlider.Q<Slider>(GameRef.UIRef.PLAY_TRAJECTORY_SLIDER);
            _angle.RegisterValueChangedCallback((evt) =>
            {
                OnAngleChangeEvent?.Invoke(evt.newValue);
                OnAnyChangeEvent?.Invoke();
            });
            OnAngleChangeEvent += ConvertAngleSliderToReadible;

            // POWER slider
            _power = _powerSlider.Q<Slider>(GameRef.UIRef.PLAY_TRAJECTORY_SLIDER);
            _power.RegisterValueChangedCallback((evt) =>
            {
                OnPowerChangeEvent?.Invoke(evt.newValue);
                OnAnyChangeEvent?.Invoke();
            });
            OnPowerChangeEvent += ConvertPowerSliderToReadible;

            // ACCURACY GAUGE slider
            _accuracy = _root.Q<Slider>(GameRef.UIRef.PLAY_ACCURACY_SLIDER);
            _accuracy.RegisterValueChangedCallback((evt) =>
            {
                OnAccuracyChangeEvent?.Invoke(evt.newValue);
            });

            // INFO button
            _openInfo = _root.Q<IconButton>(GameRef.UIRef.PLAY_SETTINGS);
            _openInfo.AddAction(() =>
            {
                Info.Display(true);
                Info.Select(InfoTab.SETTINGS);
            });
            _root.Add(Info.Init());

            // ESCAPE button
            _goToMenu = _root.Q<IconButton>(GameRef.UIRef.PLAY_RETURNS);
            _goToMenu.AddAction(() =>
            {
                GameLog.Say($"Go to 'MENU'");
                SceneController.I.GoToScene(GameRef.Scenes.MENU);
            });

            // Default activations
            ActivateFireButton(false);
            DisplayAccuracySlider(false);
            ColorGaugeElements(GaugeColor.STANDARD);
            DisplayFireLabel(false);
            DisplayGaugeChildren(false);
        }


        /* - - - - - SLIDERS - - - - - */
        /// <summary>
        /// Adjust angle slider values if needed (old version) and update label
        /// </summary>
        private void ConvertAngleSliderToReadible(float newAngle)
        {
            PlayManager.I.Player.CurrentAngle = newAngle;
            _angleSlider.Updatelabel(PlayManager.I.Player.CurrentAngle.ToString());
        }
        /// <summary>
        /// Calculate power slider values if needed and update label
        /// </summary>
        private void ConvertPowerSliderToReadible(float newPower)
        {
            PlayManager.I.Player.CurrentPower = GameRef.Trajectory.POWER_MIN + (newPower * (GameRef.Trajectory.POWER_MAX - GameRef.Trajectory.POWER_MIN));
            _powerSlider.Updatelabel(((int)PlayManager.I.Player.CurrentPower).ToString());
        }

        /// <summary>
        /// Update accuracy slider value
        /// </summary>
        public void SetAccuracySlider(float accuracyValue)
        {
            _accuracy.value = accuracyValue;
        }

        /// <summary>
        /// Hide/display accuracy slider
        /// </summary>
        public void DisplayAccuracySlider(bool display)
        {
            _accuracy.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
        }


        /* - - - - - LABELS - - - - - */
        /// <summary>
        /// Update battle label value
        /// </summary>
        public void SetBattleLabel(string text)
        {
            _level.text = $"BATTLE: {text}";
        }

        /// <summary>
        /// Update score label value
        /// </summary>
        public void SetScoreLabel(string text)
        {
            _score.text = $"SCORE: {text}";
        }

        /// <summary>
        /// Update timer label value
        /// </summary>
        public void SetTimerLabel(string text)
        {
            _level.text = text;
        }

        /// <summary>
        /// Update wind force label value, and updates arrows pointing in direction
        /// </summary>
        public void SetWindLabel(float windValue)
        {
            // Check which directinal arrow to select
            _windLeftArrow.style.display = windValue < 0 ? DisplayStyle.Flex :DisplayStyle.None;
            _windRightArrow.style.display = windValue > 0 ? DisplayStyle.Flex :DisplayStyle.None;

            float factor = 100f / GameRef.Environment.WIND_RANGE;
            _windLabel.text = $"{GameUtils.LimitDP(windValue * factor, 0, false)}";
        }

        /// <summary>
        /// Update lives available label value
        /// </summary>
        public void SetLivesLabel(string livesValue)
        {
            _livesLabel.text = livesValue;
        }


        /* - - - - - MESSAGES - - - - - */
        /// <summary>
        /// Updates current message field and then starts hide countdown
        /// </summary>
        public void MessagePlay(string msg)
        {
            // Display message
            _message.text = msg;

            // Stop any hide in progress and start again
            if (_messageHide != null)
            {
                StopCoroutine(_messageHide);
            }
            _messageHide = StartCoroutine(MessageHoldAndEnd());
        }

        /// <summary>
        /// Waits for period of time then clears message
        /// </summary>
        private IEnumerator MessageHoldAndEnd()
        {
            yield return new WaitForSeconds(GameRef.Time.MESSAGE_HOLD);
            MessageClear();
        }

        /// <summary>
        /// Clears messages display so it is blank
        /// </summary>
        public void MessageClear()
        {
            _message.text = "";
        }


        /* - - - - - TIMER - - - - - */
        /// <summary>
        /// Update time remaining label value
        /// </summary>
        public void TimerUpdateLabel(string timerValue)
        {
            _timer.text = timerValue;
        }

        /// <summary>
        /// Clears time remaining label value completely
        /// </summary>
        public void TimerClear()
        {
            _timer.text = "";
        }

        /// <summary>
        /// Hides/disables 'FIRE!' display when gauge limit is met/reset
        /// </summary>
        public void DisplayFireLabel(bool enable)
        {
            if (enable)
            {
                VisualElement enableLabel = PlayManager.I.Firing == FiringType.STD ? _fireLabel : _rapidFireLabel;
                VisualElement disableLabel = PlayManager.I.Firing == FiringType.STD ? _rapidFireLabel : _fireLabel;
                enableLabel.style.display = DisplayStyle.Flex;
                disableLabel.style.display = DisplayStyle.None;
            }
            else
            {
                _fireLabel.style.display = DisplayStyle.None;
                _rapidFireLabel.style.display = DisplayStyle.None;
            }
        }


        /* - - - - - READY GAUGE - - - - */
        /// <summary>
        /// Recolors the fire-waiting gauge blocks dependent on firing type
        /// </summary>
        public void ColorGaugeElements(GaugeColor gaugeCol)
        {
            // Make purple with increasing lightness
            if(gaugeCol == GaugeColor.RAPID)
            {
                Color col = GameRef.Colors.RAPID_FIRE;
                float lowestGrey = 0.5f;
                float greySplit = (1f - lowestGrey) / (_readyGauge.childCount - 2);
                for (int c = 0; c < _readyGauge.childCount - 2; c++)
                {
                    float greyValue = lowestGrey + (greySplit * (c + 1));
                    Color slotColor = new Color(greyValue, greyValue, greyValue);
                    slotColor = Color.Lerp(slotColor, col, 0.5f);
                    _readyGauge[c].style.backgroundColor = slotColor;
                    _readyGauge[c].style.backgroundColor = col;
                }
            }
            // Update with preset yellow -> red flow
            else
            {
                for (int c = 0; c < _readyGauge.childCount - 2; c++)
                {
                    _readyGauge[c].style.backgroundColor = GameRef.Colors.GAUGE_BARS[c];
                }
            }
        }

        /// <summary>
        /// Hides/displays all 'fire-waiting' guage blocks 
        /// </summary>
        public void DisplayGaugeChildren(bool display, bool includeLabels = true)
        {
            for (int c = 0; c < _readyGauge.childCount; c++)
            {
                // Ignore labels is specified
                if (!includeLabels && c >= _readyGauge.childCount - 2) continue;

                _readyGauge[c].style.visibility = display ? Visibility.Visible :  Visibility.Hidden;
            }
        }

        /// <summary>
        /// Checks next hidden block in 'fire'waiting' gauge and makes visible, returns true and displays label if reaches max
        /// </summary>
        public bool DisplayNextGaugeChild()
        {
            for (int c = 0; c < _readyGauge.childCount; c++)
            {
                if(_readyGauge[c].style.visibility != Visibility.Visible)
                {
                    _readyGauge[c].style.visibility = Visibility.Visible;

                    // If last member, return true (ready)
                    if(c == _readyGauge.childCount - 1)
                    {
                        DisplayFireLabel(true);
                        StartCoroutine(ShakeFireLabel());
                        return true;
                    }

                    return false;
                }
            }

            // If nothing was found, must already all be displaying - therefore ready
            return true;
        }

        /// <summary>
        /// Shakes label by repositioning it rapidly at random points within a circle
        /// </summary>
        public IEnumerator ShakeFireLabel()
        {
            float t = 0;
            float shakeAmount = 10f;

            VisualElement label = PlayManager.I.Firing == FiringType.STD ? _fireLabel: _rapidFireLabel;

            while (t < 1f)
            {
                Vector2 v = UnityEngine.Random.insideUnitCircle * shakeAmount;
                label.style.translate = new Translate(v.x, v.y, 0);

                t += Time.deltaTime;

                yield return null;
            }

            label.style.translate = new Translate(0,0,0);

            yield return new WaitForSeconds(0.5f);

            DisplayFireLabel(false);
        }


        /*- - - - - BUTTONS - - - - - */
        /// <summary>
        /// Hides/displays strike button in UI
        /// </summary>
        public void DisplayStrikeButton(bool enable)
        {
            _strike.style.display = enable ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// Enables/disables strike button in UI
        /// </summary>
        public void EnableStrikeButton(bool enable)
        {
            _strike.SetEnabled(enable);
        }

        /// <summary>
        /// Updates image texture in strike button element
        /// </summary>
        public void UpdateStrikeImage(Texture2D image)
        {
            _strikeIcon.style.backgroundImage = image;
        }

        /// <summary>
        /// Change fire button  with indication of being pressed/active
        /// </summary>
        private void ShowFireAsPressed()
        {
            _fireBase.ToggleInClassList(GameRef.USS.PLAY_BUTTON__PRESSED);
        }

        /// <summary>
        /// Change fire button with indication of being released
        /// </summary>
        public void ShowFireAsReleased()
        {
            _fireBase.ToggleInClassList(GameRef.USS.PLAY_BUTTON__PRESSED);
        }

        /// <summary>
        /// Change strike button  with indication of being pressed/active
        /// </summary>
        private void ShowStrikeAsPressed()
        {
            _strikeBase.ToggleInClassList(GameRef.USS.PLAY_BUTTON__PRESSED);
        }

        /// <summary>
        /// Change strike button with indication of being released
        /// </summary>
        public void ShowStrikeAsReleased()
        {
            _strikeBase.ToggleInClassList(GameRef.USS.PLAY_BUTTON__PRESSED);
        }

        /// <summary>
        /// Enables/disables fire button and adjusts background color based on state
        /// </summary>
        public void ActivateFireButton(bool isReady)
        {
            if (isReady)
            {
                _fire.SetEnabled(true);
                _fireBase.ToggleInClassList(GameRef.USS.PLAY_FIRE__BUTTON__DISABLED);
            }
            else
            {
                _fire.SetEnabled(false);
                _fireBase.ToggleInClassList(GameRef.USS.PLAY_FIRE__BUTTON__DISABLED);
            }
        }
    }
}
