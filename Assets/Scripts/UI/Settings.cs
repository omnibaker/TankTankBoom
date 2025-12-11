using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Sumfulla.TankTankBoom
{
    public class Settings
    {
        private VisualElement _root;
        private VisualElement _settingsContainer;

        public event Action OnReset;

        public VisualElement Root { get { return _root; } }


        public Settings()
        {
            GameLog.Say($"Instantiating Settings");

            _root = GameUtils.UITK.InstantiateUXML(GameRef.UIRef.UXML_SETTINGS, TemplateShape.STRETCH);
            _settingsContainer = _root.Q(GameRef.UIRef.SETTINGS_CONTAINER);
            _settingsContainer.Add(ConfigureMusic());
            _settingsContainer.Add(ConfigureSound());
            _settingsContainer.Add(ConfigureReset());
        }

        /// <summary>
        /// Static public getter to create and configure new instance
        /// </summary>
        public static Settings InstantiateSettings()
        {
            Settings newSettings = new Settings();
            return newSettings;
        }

        /// <summary>
        /// Instantiates and retuns ListButton required for triggering reset
        /// </summary>
        private ListButton ConfigureReset()
        {
            // Button action
            void reset()
            {
                // Create pop-up warning first
                PopUp popUp = PopUp.InstantiatePopUp();
                popUp.UpdateTitle("RESET");
                popUp.UpdateMessage("Do you wish to reset?");

                // Pop-up option: YES
                void yes()
                {
                    GameLog.Say("Yes");
                    PlayerPrefs.DeleteAll();
                    OnReset?.Invoke();
                    popUp.KillMe();
                }

                // Pop-up option: NO
                void no()
                {
                    GameLog.Say("No");
                    popUp.KillMe();
                }

                // Add buttons to pop-up
                popUp.AddButton(yes, "YES");
                popUp.AddButton(no, "NO");
            }

            // Instantiate button option
            ListButton ls = AddButtonRow("Reset", reset);

            return ls;
        }

        /// <summary>
        /// Instantiates and retuns ListToggle required for music toggling
        /// </summary>
        private ListToggle ConfigureMusic()
        {
            // Toggle default
            bool musicState = GameUtils.Settings.GetMusicState();

            // Toggle action
            Action<bool> musicToggle = (musicOn) =>
            {
                PlayerPrefs.SetInt(GameRef.PrefRef.MUSIC_STATE, (musicOn ? 1 : 0));
            };

            // Instantiate toggle
            ListToggle lt = AddToggleRow("Music", musicToggle, musicState);

            // Add reset process to event
            OnReset += () => { lt.SetValueWithoutNotify(GameUtils.Settings.GetMusicState()); };

            return lt;
        }

        /// <summary>
        /// Instantiates and retuns ListToggle required for sound toggling
        /// </summary>
        private ListToggle ConfigureSound()
        {
            // Toggle default
            bool soundState = GameUtils.Settings.GetSoundState();

            // Toggle action
            Action<bool> soundToggle = (soundOn) =>
            {
                PlayerPrefs.SetInt(GameRef.PrefRef.SOUND_STATE, (soundOn ? 1 : 0));
            };

            // Instantiate toggle
            ListToggle lt = AddToggleRow("Sound", soundToggle, soundState);

            // Add reset process to event
            OnReset += () => { lt.SetValueWithoutNotify(GameUtils.Settings.GetSoundState()); };

            return lt;
        }

        /// <summary>
        /// Generic public method to add a vertical element with a labelled, single button option
        /// </summary>
        public ListButton AddButtonRow(string label, Action action)
        {
            ListButton lb = new ListButton();
            lb.RegisterOnClickCallback(false);
            lb.UpdateButtonEvent(action);
            lb.ListButtonLabel = label;
            return lb;
        }

        /// <summary>
        /// Generic public method to add a vertical element with a labelled, togglable option
        /// </summary>
        public ListToggle AddToggleRow(string label, Action<bool> action, bool initState)
        {
            ListToggle lt = new ListToggle();
            lt.UpdateToggleEvent(action);
            lt.ListToggleLabel = label;
            lt.value = initState;
            return lt;
        }
    }
}