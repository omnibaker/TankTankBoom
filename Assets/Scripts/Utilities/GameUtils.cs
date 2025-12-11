using JetBrains.Annotations;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sumfulla.TankTankBoom
{
    public class GameUtils
    {
        public class Settings
        {
            /// <summary>
            /// Returns bool state as to whether music is currently enabled
            /// </summary>
            public static bool GetMusicState()
            {
                return PlayerPrefs.GetInt(GameRef.PrefRef.MUSIC_STATE, GameRef.Default.DEF_MUSIC) == 1;
            }

            /// <summary>
            /// Returns bool state as to whether sound effects (game and UI) are currently enabled
            /// </summary>
            public static bool GetSoundState()
            {
                return PlayerPrefs.GetInt(GameRef.PrefRef.SOUND_STATE, GameRef.Default.DEF_SOUND) == 1;
            }
        }

        public class UITK
        {
            /// <summary>
            /// Finds locals UI Document and root VisualElement, and then sets core USS stylesheet
            /// </summary>
            public static VisualElement ConfigureRoot(UIDocument uid, string visualTreeAsset)
            {
                // Confirm UI document controller to find root
                VisualElement root = uid != null ? uid.rootVisualElement : null;

                // Exit with null root if nothing found
                if (root == null) return root;

                // Attach stylesheet to document
                StyleSheet uss = Resources.Load<StyleSheet>(GameRef.UIRef.STYLESHEET_CORE);
                root.styleSheets.Add(uss);

                // Create root UI element if not attached already in Editor
                VisualElement ui;

                if (root.childCount == 0)
                {
                    VisualTreeAsset uxml = Resources.Load<VisualTreeAsset>(visualTreeAsset);
                    ui = uxml.Instantiate();
                    ui.AddToClassList(GameRef.USS.TEMPLATE_CONTAINER_ABSOLUTE);
                    root.Add(ui);
                }

                return root;
            }

            /// <summary>
            /// Finds UXML file in Resources, instantiates an instance and returns root VisualElement
            /// </summary>
            public static VisualElement InstantiateUXML(string visualTreeAsset, TemplateShape shape)
            {
                // Find template and its root VisualElement
                VisualTreeAsset uxml = Resources.Load<VisualTreeAsset>(visualTreeAsset);
                VisualElement root = uxml.Instantiate();

                // Unity sucks, and does make templates flex by default
                switch (shape)
                {
                    case TemplateShape.ABSOLUTE: root.AddToClassList(GameRef.USS.TEMPLATE_CONTAINER_ABSOLUTE); break;
                    case TemplateShape.STRETCH: root.AddToClassList(GameRef.USS.TEMPLATE_CONTAINER_STRETCH); break;
                    case TemplateShape.CENTER: root.AddToClassList(GameRef.USS.TEMPLATE_CONTAINER_CENTER); break;
                    case TemplateShape.NONE:
                    default: break;
                }

                return root;
            }

            /// <summary>
            /// Instantiates UI template for long text from existing text file, ads content and returns as VE
            /// </summary>
            public static VisualElement GetLongTextVisualElement(string textAssetName)
            {
                VisualElement textFileContent = GameUtils.UITK.InstantiateUXML(GameRef.UIRef.UXML_LONG_TEXT, TemplateShape.STRETCH);
                Label longTextLabel = textFileContent.Q<Label>(GameRef.UIRef.LONG_TEXT__LABEL);
                longTextLabel.text = GameUtils.UITK.GetLongText(textAssetName);
                return textFileContent;
            }

            /// <summary>
            /// Parse text asset from Resources folder and return as string
            /// </summary>
            public static string GetLongText(string textAssetName)
            {
                TextAsset jsonFile = Resources.Load<TextAsset>(textAssetName);
                return jsonFile == null || string.IsNullOrWhiteSpace(jsonFile.text)
                    ? "ERR: data not found"
                    : jsonFile.text;
            }

            /// <summary>
            /// Seraches Resources icons folder for file matchign string parameter
            /// </summary>
            public static Texture2D GetUIIcon(string name)
            {
                Texture2D tex = Resources.Load<Texture2D>(name);
                return tex == null
                    ? Resources.Load<Texture2D>(GameRef.Textures.ICON_DEFAULT)
                    : tex;
            }
        }

        public class Time
        {
            /// <summary>
            /// Translates seconds into a mm:ss format and returns as string
            /// </summary>
            public static string GetTimeInFormattedString(float timeLeftInSecond)
            {
                return TimeSpan.FromSeconds(timeLeftInSecond).ToString(GameRef.Time.TIME_MM_SS);
            }
        }





        public const int DEFAULT_DP = 3;
        private const string TIME_FORMAT_MM_SS = "mm\\:ss";
        public static string GetTimeInFormattedString(float timeLeftInSecond)
        {
            return TimeSpan.FromSeconds(timeLeftInSecond).ToString(TIME_FORMAT_MM_SS);
        }

        public static string FormatFloats(float rawNumber, int dp = DEFAULT_DP)
        {
            string prefix = rawNumber < 0 ? "-" : " ";

            string formatPattern = "";
            switch (dp)
            {
                case 0: formatPattern = "{0:0}"; break;
                case 1: formatPattern = "{0:0.0}"; break;
                case 2: formatPattern = "{0:0.00}"; break;
                case 3: formatPattern = "{0:0.000}"; break;
                case 4: formatPattern = "{0:0.0000}"; break;
                case 5: formatPattern = "{0:0.00000}"; break;
                default: formatPattern = "{0:0.000}"; break;
            }

            string formatted = string.Format(formatPattern, Mathf.Abs(rawNumber));
            return $"{prefix}{formatted}";
        }

        public static Color GetColorFromHexadecimal(string hexadecimalColor)
        {
            if (ColorUtility.TryParseHtmlString($"#{hexadecimalColor}", out Color rgbaColor))
            {
                return rgbaColor;
            }

            GameLog.Shout($"ERR: Could not get color from #{hexadecimalColor}");
            return Color.gray;
        }

        public static string LimitDP(float rawNumber, int dp = 3, bool sign = true)
        {
            string prefix = "";
            if (sign)
            {
                prefix = rawNumber < 0 ? "-" : " ";
            }
            return $"{prefix}{Mathf.Abs((float)Math.Round((decimal)rawNumber, dp))}";
        }

        public static string GetVector2String(string label, Vector2 v2, int dp = DEFAULT_DP)
        {
            label = string.IsNullOrWhiteSpace(label) ? "" : $"{label}: ";
            return $"{label}{FormatFloats(v2.x, dp)}, {FormatFloats(v2.y, dp)}";
        }

        public static string GetVector3String(string label, Vector3 v3, int dp = DEFAULT_DP)
        {
            label = string.IsNullOrWhiteSpace(label) ? "" : $"{label}: ";
            return $"{label}{FormatFloats(v3.x, dp)}, {FormatFloats(v3.y, dp)}, {FormatFloats(v3.z, dp)}";
        }

        public static string GetVector3IntString(string label, Vector3Int v3, int dp = DEFAULT_DP)
        {
            label = string.IsNullOrWhiteSpace(label) ? "" : $"{label}: ";
            return $"{label}{FormatFloats(v3.x, dp)}, {FormatFloats(v3.y, dp)}, {FormatFloats(v3.z, dp)}";
        }


    }
}