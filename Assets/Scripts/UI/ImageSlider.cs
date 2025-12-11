using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sumfulla.TankTankBoom
{
    [UxmlElement]
    partial class ImageSlider : VisualElement
    {
        public Label Lbl;
        public Slider Sld;
        

        private VisualElement _dragger;
        private VisualElement _tracker;
        private VisualElement _draggerIcon;


        [UxmlAttribute]
        public Texture2D Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                _draggerIcon.style.backgroundImage = _icon;
            }
        }
        private Texture2D _icon;

        [UxmlAttribute]
        public string SliderLabel
        {
            get => _sliderLabel;
            set
            {
                _sliderLabel = value;
                Lbl.text = _sliderLabel;
            }
        }
        private string _sliderLabel { get; set; }


        [UxmlAttribute]
        public SliderDirection SliderDir
        {
            get => _sliderDirection;
            set
            {
                _sliderDirection = value;
                Sld.direction = _sliderDirection;
            }
        }
        private SliderDirection _sliderDirection { get; set; }


        public ImageSlider()
        {
            // Don't get in way of button
            pickingMode = PickingMode.Ignore;

            // Set USS values
            StyleSheet uss = Resources.Load<StyleSheet>(GameRef.UIRef.STYLESHEET_CORE);
            styleSheets.Add(uss);

            // Add button 
            Sld = new Slider();
            Sld.name = "trajectory-slider";
            Sld.AddToClassList("TBA");
            Sld.style.flexGrow = 1;
            Sld.direction = SliderDir;

            // Tracker
            _tracker = Sld.Q("unity-tracker");
            if (_sliderDirection == SliderDirection.Horizontal)
            {
                _tracker.style.height = 4;
            }
            else
            {
                _tracker.style.width = 4;
            }
            _tracker.style.borderTopColor = Color.white;
            _tracker.style.borderRightColor = Color.white;
            _tracker.style.borderBottomColor = Color.white;
            _tracker.style.borderLeftColor = Color.white;
            _tracker.style.backgroundColor = Color.white;

            // Dragger
            _dragger = Sld.Q("unity-dragger");
            _dragger.style.height = 100;
            _dragger.style.width = 100;
            _dragger.style.borderTopLeftRadius = Length.Percent(50);
            _dragger.style.borderTopRightRadius = Length.Percent(50);
            _dragger.style.borderBottomLeftRadius = Length.Percent(50);
            _dragger.style.borderBottomRightRadius = Length.Percent(50);
            _dragger.style.borderTopWidth = 4;
            _dragger.style.borderRightWidth = 4;
            _dragger.style.borderBottomWidth = 4;
            _dragger.style.borderLeftWidth = 4;
            _dragger.style.borderTopColor = Color.white;
            _dragger.style.borderRightColor = Color.white;
            _dragger.style.borderBottomColor = Color.white;
            _dragger.style.borderLeftColor = Color.white;
            _dragger.style.backgroundColor = Color.black;
            _dragger.style.alignItems = Align.Center;
            _dragger.style.justifyContent = Justify.Center;

            // Dragger Icon
            _draggerIcon = new VisualElement();
            _draggerIcon.style.unityBackgroundImageTintColor = Color.white;
            _draggerIcon.style.height = 60;
            _draggerIcon.style.width = 60;
            _dragger.Add(_draggerIcon);

            // Add label
            Lbl = new Label();
            Lbl.AddToClassList("TBA");
            Lbl.style.width = 150;
            Lbl.style.fontSize = 35;
            Lbl.style.textShadow = new StyleTextShadow(new TextShadow
            {
                color = Color.black,
                offset = new Vector2(2, 2),
                blurRadius = 2
            });
            Lbl.style.color = Color.white;
            Lbl.style.unityFontStyleAndWeight = FontStyle.Bold;

            // Add to root
            Add(Sld);
            Add(Lbl);

            // TEMP
            UpdateIcon(GameUtils.UITK.GetUIIcon(GameRef.Textures.ICON_QUESTION_MARK));
            Updatelabel("0");


            // Defer layout update until geometry is ready
            RegisterCallback<GeometryChangedEvent>(_ => UpdateLayout());
        }

        /// <summary>
        /// Post-creation setup based on dynamic propeties
        /// </summary>
        private void UpdateLayout()
        {
            if (Sld == null) return;

            if (_sliderDirection == SliderDirection.Horizontal)
            {
                style.flexDirection = FlexDirection.Row;
                style.marginTop = 20;
                style.marginBottom = 4;
                style.alignItems = Align.Center;

                if (_tracker != null)
                {
                    _tracker.style.height = 4;
                    _tracker.style.width = StyleKeyword.Auto;
                }

                if(Lbl != null)
                {
                    Lbl.style.marginLeft = 10;
                    Lbl.style.unityTextAlign = TextAnchor.MiddleLeft;
                }

                if(_dragger != null)
                {
                    _dragger.style.marginTop = -50;
                }
            }
            else
            {
                style.flexDirection = FlexDirection.Column;
                style.marginLeft = 4;
                style.marginRight = 20;
                style.alignItems = Align.Center;

                if (_tracker != null)
                {
                    _tracker.style.width = 4;
                    _tracker.style.height = StyleKeyword.Auto;
                }

                if (Lbl != null)
                {
                    Lbl.style.marginTop = 10;
                    Lbl.style.unityTextAlign = TextAnchor.LowerCenter;
                }

                if (_dragger != null)
                {
                    _dragger.style.marginLeft = -50;
                }
            }
        }

        /// <summary>
        /// Updates the slider's icon texture
        /// </summary>
        public void UpdateIcon(Texture2D texture )
        {
            _draggerIcon.style.backgroundImage = texture;
        }
        
        /// <summary>
        /// Updates the slider's label text
        /// </summary>
        public void Updatelabel(string labelText)
        {
            Lbl.text = labelText;
        }

        /// <summary>
        /// Sets min/max range for the slider
        /// </summary>
        public void UpdateMinMaxDefault(float min, float max, float def)
        {
            Sld.lowValue = min;
            Sld.highValue = max;
            Sld.value = def;
        }
    }
}