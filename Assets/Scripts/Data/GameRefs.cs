using UnityEngine;

namespace Sumfulla.TankTankBoom
{
    public static class GameRef
    {
        public static class AnimationTags
        {
            public const string READY_TO_EXLODE = "IsReadyToExplode";
            public const string READY_TO_DIE = "IsReadyToDie";
        }

        public static class Core
        {
            public const string LOG_LABEL = "TankTankBoom :: ";
            public const string GAME = "Game";
        }

        public static class Audio
        {
            public const float SCENE_TRANSISSION_TIME = 0.3f;
        }

        public static class Colors
        {
            public static readonly Color DESTROYED_TANK = new Color(0.5f, 0, 0, 0.97f);
            public static readonly Color FADE_SHADE_ON = new Color(0, 0, 0, 1f);
            public static readonly Color FADE_SHADE_OFF = new Color(0, 0, 0, 0);
            public static readonly Color INFO_BTN_ON = new Color(0.5f, 0.5f, 0.5f, 0.02f);
            public static readonly Color INFO_BTN_OFF = new Color(0, 0, 0, 0);
            public static readonly Color RAPID_FIRE = new Color(1f, 0, 0.88f, 1f);
            public static readonly Color GAUGE_BAR_1 = new Color(1f, 0.81f, 0f, 1f);
            public static readonly Color GAUGE_BAR_2 = new Color(1f, 0.72f, 0f, 1f);
            public static readonly Color GAUGE_BAR_3 = new Color(1f, 0.63f, 0f, 1f);
            public static readonly Color GAUGE_BAR_4 = new Color(1f, 0.54f, 0f, 1f);
            public static readonly Color GAUGE_BAR_5 = new Color(1f, 0.45f, 0f, 1f);
            public static readonly Color GAUGE_BAR_6 = new Color(1f, 0.36f, 0f, 1f);
            public static readonly Color GAUGE_BAR_7 = new Color(1f, 0.27f, 0f, 1f);
            public static readonly Color GAUGE_BAR_8 = new Color(1f, 0.18f, 0f, 1f);
            public static readonly Color GAUGE_BAR_9 = new Color(1f, 0.09f, 0f, 1f);
            public static readonly Color GAUGE_BAR_10 = new Color(1f, 0f, 0f, 1f);
            public static readonly Color[] GAUGE_BARS = new Color[] {
                GAUGE_BAR_1,
                GAUGE_BAR_2,
                GAUGE_BAR_3,
                GAUGE_BAR_4,
                GAUGE_BAR_5,
                GAUGE_BAR_6,
                GAUGE_BAR_7,
                GAUGE_BAR_8,
                GAUGE_BAR_9,
                GAUGE_BAR_10,
            };
        }

        public static class Default
        {
            public const int START_LIFE_TOTAL = 3;
            public const int DEF_MUSIC = 1;
            public const int DEF_SOUND = 1;
        }

        public static class Enemies
        {
            public static float GetBerserkerFrequency()
            {
                switch (GameData.CurrentLevel)
                {
                    case 1: return 9f;
                    case 2: return 8.5f;
                    case 3: return 8f;
                    case 4: return 7.5f;
                    case 5: return 7f;
                    case 6: return 6.5f;
                    case 7: return 6f;
                    case 8: return 5.5f;
                    case 9: return 4f;
                    default:
                        GameLog.Warn("BerserkerFrequency option not found.");
                        return 0f;
                }
            }

            public static float GetBerserkerSpeed()
            {
                switch (GameData.CurrentLevel)
                {
                    case 1: return 1f;
                    case 2: return 1.5f;
                    case 3: return 2;
                    case 4: return 2.5f;
                    case 5: return 3f;
                    case 6: return 3.5f;
                    case 7: return 4f;
                    case 8: return 5.5f;
                    case 9: return 6f;
                    default:
                        GameLog.Warn("BerserkerSpeed option not found.");
                        return 10f;
                }
            }

            public static float GetBlimpFrequency()
            {
                switch (GameData.CurrentLevel)
                {
                    case 1: return 9f;
                    case 2: return 8.5f;
                    case 3: return 8f;
                    case 4: return 7.5f;
                    case 5: return 7f;
                    case 6: return 6.5f;
                    case 7: return 6f;
                    case 8: return 5.5f;
                    case 9: return 5f;
                    default:
                        GameLog.Warn("BlimpFrequency option not found.");
                        return 10f;
                }
            }

            public static float GetFiringFrequency()
            {
                switch (GameData.CurrentLevel)
                {
                    case 1: return 9f;
                    case 2: return 8.5f;
                    case 3: return 8f;
                    case 4: return 7.5f;
                    case 5: return 7f;
                    case 6: return 6.5f;
                    case 7: return 6f;
                    case 8: return 5.5f;
                    case 9: return 5f;
                    default:
                        GameLog.Warn("FiringFrequency option not found.");
                        return 10f;
                }
            }

            public static float GetEnemyTankStrength()
            {
                switch (GameData.CurrentLevel)
                {
                    case 1: return 0;
                    case 2: return 0;
                    case 3: return 0;
                    case 4: return 0;
                    case 5: return 0;
                    case 6: return 0;
                    case 7: return 0;
                    case 8: return 0;
                    case 9: return 0;
                    default: return 0;
                }
            }
        }

        public static class Environment
        {
            public const float WIND_RANGE = 2f;

            public static void GenerateNewWind()
            {
                float rand = 0;
                float windLimits = WIND_RANGE * GetWindLimit();
                while (rand < 0.1f && rand > -0.1f)
                {
                    rand = Random.Range(-windLimits, windLimits);
                }
                PlayManager.I.Environment.Wind = rand;
                PlayManager.I.UIPlay.SetWindLabel(PlayManager.I.Environment.Wind);
            }

            public static int GetTerrainProfileIndex()
            {
                if (GameData.CurrentBattle >= 0 && GameData.CurrentBattle < 40)
                {
                    return 1;
                }
                else if (GameData.CurrentBattle >= 40 && GameData.CurrentBattle < 80)
                {
                    return 2;
                }
                else if (GameData.CurrentBattle >= 80)
                {
                    return 3;
                }
                else
                {
                    GameLog.Warn("TileWidthRef option not found.");
                    return 0;
                }
            }

            public static TileTerrain GetTileTerain()
            {
                int randomTerrainIndex = Random.Range(1, System.Enum.GetNames(typeof(TileTerrain)).Length);
                return (TileTerrain)randomTerrainIndex;
            }

            public static float GetWindLimit()
            {
                switch (GameData.CurrentLevel)
                {
                    case 1: return 0.5f;
                    case 2: return 0.55f;
                    case 3: return 0.6f;
                    case 4: return 0.65f;
                    case 5: return 0.7f;
                    case 6: return 0.75f;
                    case 7: return 0.8f;
                    case 8: return 0.9f;
                    case 9: return 0.95f;
                    default:
                        GameLog.Warn("WindLimit option not found.");
                        return 10f;
                }
            }
        }

        public static class Files
        {
            public const string GAMESO_GROUND_TILE = "GamesSO/GroundTile";
            public const string GAMESO_GRASS_TILE = "GamesSO/GrassTile";
        }

        public static class Maths
        {
            public const float TAU = Mathf.PI * 2;
        }

        public static class Milestone
        {
            public const int MS_STRIKE = 1200;//1200;
            public const int MS_RAPID_FIRE = 2000;
        }

        public static class Points
        {
            public static int DIRT_HIT = 5;
            public static int TANK_KILL = 125;
            public static int TANK_HIT = 20;
            public static int BLIMP_KILL = 100;
            public static int BDROPPER_KILL = 150;
            public static int BERSERKER_KILL = 200;
            public const int BATTLE_WON = 100;
        }

        public static class PrefRef
        {
            public const string DEFAULT_CHECKED = "PREF_DEFAULT_CHECKED";
            public const string INIT_TRIGGERED = "INIT TRIGGERED";
            public const string UI_SLIDER_VALUE = "UI_SLIDER_VALUE";
            public const string MUSIC_STATE = "MUSIC_STATE";
            public const string SOUND_STATE = "SOUND_STATE";
            public const string PREF_SOUND = "PREF_SOUND";
            public const string PREF_MUSIC = "PREF_MUSIC";
        }

        public static class Scenes
        {
            public const string MAIN = "Main";
            public const string MENU = "Menu";
            public const string BATTLEFIELD = "Battlefield";
        }

        public static class TextDocs
        {
            public const string GAME_ABOUT = "game_about";
            public const string GAME_ADS = "game_ads";
            public const string GAME_LEGAL = "game_legal";
        }

        public static class Textures
        {
            public const string RES_ICONS = "UIIcons/";
            public const string ICON_DEFAULT = ICON_CROSS;
            public const string ICON_CROSS = "icon_cross";
            public const string ICON_ABOUT__FULL = "icon_about--full";
            public const string ICON_ARROW_GO_BACK = "icon_arrow-go-back";
            public const string ICON_ARROW_DOWN = "icon_arrow-down";
            public const string ICON_ARROW_LEFT = "icon_arrow-left";
            public const string ICON_ARROW_RIGHT = "icon_arrow-right";
            public const string ICON_ARROW_UP = "icon_arrow-up";
            public const string ICON_HAMBURGER = "icon_hamburger";
            public const string ICON_LEGAL__FULL = "icon_legal--full";
            public const string ICON_NO_ADS__COLOR = "icon_no-ads--color";
            public const string ICON_NO_ADS__WHITE = "icon_no-ads--white";
            public const string ICON_QUESTION_MARK = "icon_question-mark";
            public const string ICON_SETTINGS__FULL = "icon_settings--full";
            public const string ICON_STRIKE__PLANE = "icon_strike--plane";
            public const string ICON_STRIKE__BOMB = "icon_strike--bomb";
        }

        public static class Time
        {
            public const string TIME_MM_SS = "m\\:ss";
            public const float SCENE_FADE = 0.3f;
            public const float MESSAGE_HOLD = 3f;
            public const float MESSAGE_GAP = MESSAGE_HOLD + 0.2f;
            public const float READY_GAUGE_WAIT_STD = 0.25f;
            public const float READY_GAUGE_WAIT_RAPID = 0.05f;
            public const int MIN_3 = 180; // timer 3 minutes
            public const int MIN_2 = 120; // timer 3 minutes
            public const int MIN_1 = 60; // timer 3 minutes
            public const int SEC_10 = 10; // timer 3 minutes
            public const int SEC_15 = 15; // timer 3 minutes
            public const int SEC_5 = 5; // timer 3 minutes
        }

        public static class Trajectory
        {
            public const float PERIOD = 1.5f;
            public const float ANGLE_MIN = 0;
            public const float ANGLE_MAX = 90;
            public const float POWER_MAX = 1000;
            public const float POWER_MIN = 200;
            public const float ANGLE_SLIDER_INIT = 45f;
            public const float POWER_SLIDER_INIT = 0.25f;
        }

        public static class UIRef
        {
            // Core
            public const string STYLESHEET_CORE = "tanktankboom";

            // Foreground
            public const string FOREGROUND_FADE = "foreground-fade";
            public const string FOREGROUND_MESSAGES = "foreground-messages";

            // UXML
            public const string UXML_BASE = "base";
            public const string UXML_MENU = "menu";
            public const string UXML_PLAY = "play";
            public const string UXML_INFO = "info";
            public const string UXML_MENU_LIST = "menu-list";
            public const string UXML_OPTION_ICON = "option-icon";
            public const string UXML_OPTION_TEXT = "option-text";
            public const string UXML_LONG_TEXT = "long-text";
            public const string UXML_SETTINGS = "settings";
            public const string UXML_POPUP = "popup";
            public const string UXML_SCENE = "scene";
            public const string UXML_FOREGROUND = "foreground";

            // UXML Attributes
            public const string UXML_ATR__LIST_LABEL = "listLabel";
            public const string UXML_ATR__IS_PRESSED = "isPressed";
            public const string UXML_ATR__ACTIVATED = "activated";
            public const string UXML_ATR__UI_SLIDER_INT = "uiSliderInt";
            public const string UXML_ATR__INT_SLIDER_VALUE = "intSliderValue";

            // Menu
            public const string MENU = "menu";
            public const string MENU_TOP = "menu-top";
            public const string MENU_HEADER = "menu-header";
            public const string MENU_HEADER_LABEL = "menu-header__label";
            public const string MENU_BODY = "menu-body";
            public const string MENU_FOOTER = "menu-footer";
            public const string MENU_LIST = "menu-list";
            public const string MENU_FOOTER__BUTTONS = "menu-footer__buttons";
            public const string MENU_FOOTER__INFO = "menu-footer__info";
            public const string MENU_FOOTER__PRIVACY = "menu-footer__privacy";
            public const string MENU_FOOTER__NO_ADS = "menu-footer__no-ads";

            // Play
            public const string PLAY_HEADER = "play-header";
            public const string PLAY_BODY = "play-body";
            public const string PLAY_FOOTER = "play-footer";
            public const string PLAY_TOP__BACK = "play-top__back";
            public const string PLAY_TOP__INFO = "play-top__info";
            public const string PLAY_MESSAGE = "play-message";
            public const string PLAY_TIMER = "play-timer";
            public const string PLAY_READY_GAUGE = "play-ready-gauge";
            public const string PLAY_LIVES_LABEL = "play-lives--label";
            public const string PLAY_READY_GAUGE__FIRE = "play-ready-gauge__fire";
            public const string PLAY_READY_GAUGE__FIRE_LABEL = "play-ready-gauge__rapid-fire";
            public const string PLAY_STRIKE__BASE = "play-strike__base";
            public const string PLAY_STRIKE__BUTTON = "play-strike__button";
            public const string PLAY_STRIKE__ICON = "play-strike__icon";
            public const string PLAY_LEVEL = "play-level";
            public const string PLAY_SCORE = "play-score";
            public const string PLAY_TRAJECTORY_ANGLE = "trajectory-angle";
            public const string PLAY_TRAJECTORY_POWER = "trajectory-power";
            public const string PLAY_TRAJECTORY_SLIDER = "trajectory-slider";
            public const string PLAY_ACCURACY_SLIDER = "accuracy-slider";
            public const string PLAY_WIND__LABEL = "play-wind--label";
            public const string PLAY_WIND__LEFT_ARROW = "play-wind__left-arrow";
            public const string PLAY_WIND__RIGHT_ARROW = "play-wind__right-arrow";
            public const string PLAY_FIRE__BASE = "play-fire__base";
            public const string PLAY_FIRE_BUTTON = "play-fire__button";
            public const string PLAY_SETTINGS = "play-settings";
            public const string PLAY_RETURNS = "play-return";

            // Buttons
            public const string OPTION__BUTTON = "option__button";
            public const string OPTION__LABEL = "option__label";
            public const string OPTION__ICON = "option__icon";

            // Info
            public const string INFO_HEADER = "info-header";
            public const string INFO_FOOTER = "info-footer";
            public const string INFO_TOPICS = "info-topics";
            public const string INFO_TOPIC__LABEL = "info-topic__label";
            public const string INFO_TOPIC__SCROLL = "info-topic__content-scroll";
            public const string LONG_TEXT__LABEL = "long-text__label";

            // Unity
            public const string UNITY_CONTENT_CONTAINER = "unity-content-container";
            public const string UNITY_TRACKER = "unity-tracker";
            public const string UNITY_DRAGGER = "unity-dragger";

            // PopUp
            public const string POPUP = "popup";
            public const string POPUP_CLOSE_BUTTON = "popup-close-button";
            public const string POPUP_TITLE = "popup-header-label";
            public const string POPUP_MESSAGE = "popup-message-label";
            public const string POPUP_BODY = "popup-body";
            public const string POPUP_BUTTON_CONTAINER_VERT = "popup-button-container";

            // Settings
            public const string SETTINGS_CONTAINER = "settings-container";
            public const string SETTINGS_SCROLL = "settings-scroll";

            // List inputs
            public const string LIST__LABEL = "list-button__label";

            // List button
            public const string LIST_BUTTON = "list-button";
            public const string LIST_BUTTON__INPUT = "list-button__input";
            public const string LIST_BUTTON__BUTTON = "list-button__button";
            public const string LIST_BUTTON__BUTTON_FRAME = "list-button__button-frame";
            public const string LIST_BUTTON__BUTTON__CHECKED = "list-button__button--checked";
            public const string LIST_BUTTON__BUTTON_CONTAINER = "list-button__button-container";

            // List toggle
            public const string LIST_TOGGLE = "list-toggle";
            public const string LIST_TOGGLE__INPUT = "list-toggle__input";
            public const string LIST_TOGGLE__INPUT_KNOB = "list-toggle__input-knob";
            public const string LIST_TOGGLE__INPUT_CONTAINER = "list-toggle__input-container";
            public const string LIST_TOGGLE__INPUT__CHECKED = "list-toggle__input--checked";

            // List slider
            public const string LIST_SLIDER_SCALE = "list-slider__scale";
            public const string LIST_SLIDER__CONTAINER = "list-slider__container ";
            public const string LIST_SLIDER__MESSAGE = "list-slider__message";
            public const string LIST_SLIDER__SLIDER = "list-slider__slider";
            public const string LIST_SLIDER__DRAGGER = "list-slider__dragger";
        }

        public static class USS
        {
            public const string ICON_BUTTON__ROOT = "icon-button__root";
            public const string ICON_BUTTON__BUTTON = "icon-button__button";
            public const string TOPIC_BUTTON__ROOT__PORTRAIT = "topic-button__root--portrait";
            public const string TOPIC_BUTTON__BUTTON__PORTRAIT = "topic-button__button--portrait";
            public const string TOPIC_BUTTON__ROOT__LANDSCAPE = "topic-button__root--landscape";
            public const string TOPIC_BUTTON__BUTTON__LANDSCAPE = "topic-button__button--landscape";
            public const string POPUP_OPTION_BUTTON__BUTTON = "popup__option-button__button";
            public const string POPUP_OPTION_BUTTON__LABEL = "popup__option-button__label";
            public const string MENU_OPTION_BUTTON__BUTTON = "menu__option-button__button";
            public const string MENU_OPTION_BUTTON__LABEL = "menu__option-button__label";
            public const string PLAY_FIRE_BUTTON = "play-fire__button";
            public const string PLAY_BUTTON__PRESSED = "play-button--pressed";
            public const string PLAY_FIRE__BUTTON__DISABLED = "play-fire__base--disabled";
            public const string TEMPLATE_CONTAINER_ABSOLUTE = "template_container_absolute";
            public const string TEMPLATE_CONTAINER_STRETCH = "template_container_stretch";
            public const string TEMPLATE_CONTAINER_CENTER = "template_container_center";
        }

        public static class Values
        {
            public const int UI_SLIDER_DEFAULT = 5;
        }

    }
}