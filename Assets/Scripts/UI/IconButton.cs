using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sumfulla.TankTankBoom
{
    [UxmlElement]
    partial class IconButton : VisualElement
    {
        public Button Btn;

        [UxmlAttribute]
        public Length Height
        {
            get => _height;
            set
            {
                _height = value;
                Btn.style.height = _height;
            }
        }
        private Length _height { get; set; } = new Length(100, LengthUnit.Percent);

        [UxmlAttribute]
        public Length Width
        {
            get => _width;
            set
            {
                _width = value;
                Btn.style.width = _width;
            }
        }
        private Length _width { get; set; } = new Length(100, LengthUnit.Percent);

        [UxmlAttribute]
        public Length Left
        {
            get => _left;
            set
            {
                _left = value;
                Btn.style.left = _left;
            }
        }
        private Length _left { get; set; } = Length.Auto();

        [UxmlAttribute]
        public Length Right
        {
            get => _right;
            set
            {
                _right = value;
                Btn.style.right = _right;
            }
        }
        private Length _right { get; set; } = Length.Auto();

        [UxmlAttribute]
        public Length Top
        {
            get => _top;
            set
            {
                _top = value;
                Btn.style.top = _top;
            }
        }
        private Length _top { get; set; } = Length.Auto();

        [UxmlAttribute]
        public Length Bottom
        {
            get => _bottom;
            set
            {
                _bottom = value;
                Btn.style.bottom = _bottom;
            }
        }
        private Length _bottom { get; set; } = Length.Auto();

        public IconButton()
        {
            // Don't get in way of button
            pickingMode = PickingMode.Ignore;

            // Set USS values
            StyleSheet uss = Resources.Load<StyleSheet>(GameRef.UIRef.STYLESHEET_CORE);
            styleSheets.Add(uss);
            AddToClassList(GameRef.USS.ICON_BUTTON__ROOT);

            // Add button 
            Btn = new Button();
            Btn.AddToClassList(GameRef.USS.ICON_BUTTON__BUTTON);

            // Add to root
            Add(Btn);
        }

        /// <summary>
        /// Update the icon button listener methods
        /// </summary>
        public void AddAction(Action btnAction)
        {
            Btn.clicked += btnAction;
        }

        /// <summary>
        /// Update the icon button texture image
        /// </summary>
        public void UpdateIcon(Texture2D icon)
        {
            style.backgroundImage = icon;
        }


    }
}