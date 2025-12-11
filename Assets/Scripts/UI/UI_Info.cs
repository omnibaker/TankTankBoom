using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sumfulla.TankTankBoom
{
    public class UI_Info : MonoBehaviour
    {
        private VisualElement _root;
        private VisualElement _topics;
        private Label _topicLabel;
        private ScrollView _topicScroll;
        private VisualElement _topicScrollContent;
        private Settings _settings;
        private bool _initialized = false;

        private Dictionary<InfoTab, VisualElement> _tabs = new Dictionary<InfoTab, VisualElement>();

        /// <summary>
        /// Setus and configure Info panel for selection - hides by default
        /// </summary>
        public VisualElement Init(bool displayOnInit = false)
        {
            if (!_initialized)
            {
                _root = GameUtils.UITK.InstantiateUXML(GameRef.UIRef.UXML_INFO, TemplateShape.ABSOLUTE);
                Display(displayOnInit);

                _topics = _root.Q(GameRef.UIRef.INFO_TOPICS);
                _topicLabel = _root.Q<Label>(GameRef.UIRef.INFO_TOPIC__LABEL);
                _topicScroll = _root.Q<ScrollView>(GameRef.UIRef.INFO_TOPIC__SCROLL);
                _topicScrollContent = _topicScroll.Q(GameRef.UIRef.UNITY_CONTENT_CONTAINER);


                ConstructTopics();

                _initialized = true;
            }

            return _root;
        }

        /// <summary>
        /// Updates Info panels based on the selection
        /// </summary>
        public void Select(InfoTab it)
        {
            switch (it)
            {
                case InfoTab.LEGAL:
                    VisualElement legalVE = GameUtils.UITK.GetLongTextVisualElement(GameRef.TextDocs.GAME_LEGAL);
                    UpdateTopicContainer("Legal", legalVE);
                    break;
                case InfoTab.ABOUT:
                VisualElement aboutVE = GameUtils.UITK.GetLongTextVisualElement(GameRef.TextDocs.GAME_ABOUT);
                    UpdateTopicContainer("About", aboutVE);
                    break;
                case InfoTab.NO_ADS:
                    VisualElement adsVE = GameUtils.UITK.GetLongTextVisualElement(GameRef.TextDocs.GAME_ADS);
                    UpdateTopicContainer("Remove Ads", adsVE);
                    break;
                case InfoTab.SETTINGS:
                default:
                    _settings = Settings.InstantiateSettings();
                    UpdateTopicContainer("Settings", _settings.Root);
                    break;
            }

            // Highlight active tab
            foreach(InfoTab i in _tabs.Keys)
            {
                _tabs[i].style.backgroundColor = i == it
                    ? GameRef.Colors.INFO_BTN_ON
                    : GameRef.Colors.INFO_BTN_OFF;
            }
        }

        /// <summary>
        /// Temporary filler for adding selection buttons to main menu
        /// </summary>
        private void ConstructTopics()
        {
            // Back button
            TopicButton tb_back = new TopicButton();
            tb_back.UpdateIcon(GameUtils.UITK.GetUIIcon(GameRef.Textures.ICON_ARROW_GO_BACK));
            tb_back.AddAction(() => Display(false));
            _topics.Add(tb_back);

            // Settings button
            TopicButton tb_settings = new TopicButton();
            tb_settings.UpdateIcon(GameUtils.UITK.GetUIIcon(GameRef.Textures.ICON_SETTINGS__FULL));
            tb_settings.AddAction(() => Select(InfoTab.SETTINGS));
            _tabs.Add(InfoTab.SETTINGS, tb_settings.Btn);
            _topics.Add(tb_settings);

            // Legal button
            TopicButton tb_legal = new TopicButton();
            tb_legal.UpdateIcon(GameUtils.UITK.GetUIIcon(GameRef.Textures.ICON_LEGAL__FULL));
            tb_legal.AddAction(() => Select(InfoTab.LEGAL));
            _tabs.Add(InfoTab.LEGAL, tb_legal.Btn);
            _topics.Add(tb_legal);

            // About button
            TopicButton tb_about = new TopicButton();
            tb_about.UpdateIcon(GameUtils.UITK.GetUIIcon(GameRef.Textures.ICON_ABOUT__FULL));
            tb_about.AddAction(() => Select(InfoTab.ABOUT));
            _tabs.Add(InfoTab.ABOUT, tb_about.Btn);
            _topics.Add(tb_about);

            // Ads button
            TopicButton tb_ads = new TopicButton();
            tb_ads.UpdateIcon(GameUtils.UITK.GetUIIcon(GameRef.Textures.ICON_NO_ADS__WHITE));
            tb_ads.AddAction(() => Select(InfoTab.NO_ADS));
            _tabs.Add(InfoTab.NO_ADS, tb_ads.Btn);
            _topics.Add(tb_ads);
        }

        /// <summary>
        /// Creates topic selection update for header and content
        /// </summary>
        private void UpdateTopicContainer(string headerText, VisualElement containerContent)
        {
            // Rename body header
            _topicLabel.text = headerText;

            // Clear content container
            _topicScrollContent.Clear();

            // Create visual element and add to container
            _topicScrollContent.Add(containerContent);
        }

        /// <summary>
        /// Enabled display on and off
        /// </summary>
        public virtual void Display(bool flex)
        {
            if (_root != null) _root.style.display = flex ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}