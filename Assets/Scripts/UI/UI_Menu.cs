using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sumfulla.TankTankBoom
{
    public class UI_Menu : MonoBehaviour
    {
        // Visual Elements
        private VisualElement _root;
        private VisualElement _footerContainer;

        // UI Panels
        public UI_Info Info { get; set; }

        // Footer Buttons
        private IconButton _info_btn;
        private IconButton _privacy_btn;
        private IconButton _noAds_btn;


        private void OnEnable()
        {
            // Get central UI document
            UIDocument uid = GetComponent<UIDocument>();

            // Instantiate base Menu UI and configure
            _root = GameUtils.UITK.ConfigureRoot(uid, GameRef.UIRef.UXML_MENU);

            // Get UI panel components
            Info = TryGetComponent(out UI_Info i) ? i : gameObject.AddComponent<UI_Info>();

            Init_Menu();
        }

        /// <summary>
        /// Configures and sets up Menu UI for runtime use
        /// </summary>
        private void Init_Menu()
        {
            // Find menu elements
            _footerContainer = _root.Q(GameRef.UIRef.MENU_FOOTER__BUTTONS);

            // All of these should have been initialised at runtime
            _root.Add(Info.Init());

            // Add button tabs to bottom section
            MakeBottomTabs();

            // Update list buttons
            UpdateStartButton();
        }


        private void MakeBottomTabs()
        {
            // Footer - Add 'Info' button
            _info_btn = new IconButton();
            _info_btn.name = GameRef.UIRef.MENU_FOOTER__INFO;
            _info_btn.UpdateIcon(GameUtils.UITK.GetUIIcon(GameRef.Textures.ICON_HAMBURGER));
            _info_btn.AddAction(() =>
            {
                Info.Display(true);
                Info.Select(InfoTab.SETTINGS);
            });
            _info_btn.style.height = 96;
            _info_btn.style.width = 96;
            _footerContainer.Add(_info_btn);

            // Footer - Add 'Privacy' button
            _privacy_btn = new IconButton();
            _privacy_btn.name = GameRef.UIRef.MENU_FOOTER__PRIVACY;
            _privacy_btn.UpdateIcon(GameUtils.UITK.GetUIIcon(GameRef.Textures.ICON_LEGAL__FULL));
            _privacy_btn.AddAction(() =>
            {
                Info.Display(true);
                Info.Select(InfoTab.LEGAL);
            });
            _privacy_btn.style.height = 96;
            _privacy_btn.style.width = 96;
            _footerContainer.Add(_privacy_btn);

            // Footer - Add 'No Ads' button
            _noAds_btn = new IconButton();
            _noAds_btn.name = GameRef.UIRef.MENU_FOOTER__NO_ADS;
            _noAds_btn.UpdateIcon(GameUtils.UITK.GetUIIcon(GameRef.Textures.ICON_NO_ADS__WHITE));
            _noAds_btn.AddAction(() =>
            {
                Info.Display(true);
                Info.Select(InfoTab.NO_ADS);
            });
            _noAds_btn.style.height = 96;
            _noAds_btn.style.width = 96;
            _footerContainer.Add(_noAds_btn);
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void UpdateStartButton()
        {
            // Create button for SCENE.PLAY, update label, add behaviour, add to list
            Button startButton = _root.Q<Button>("menu-start-button");
            startButton.clicked += (() =>
            {
                GameData.SetCurrentLevel(1);
                SceneController.I.GoToScene(GameRef.Scenes.BATTLEFIELD);
            });
        }
    }
}