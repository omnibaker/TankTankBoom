using System;
using UnityEngine;
using UnityEngine.UIElements;


namespace Sumfulla.TankTankBoom
{
    [UxmlElement]
    public partial class OptionButton : Button
    {
        internal Label _label;

        public OptionButton()
        {
            // Set USS values
            StyleSheet uss = Resources.Load<StyleSheet>(GameRef.UIRef.STYLESHEET_CORE);
            styleSheets.Add(uss);

            // Add label 
            _label = new Label("-");
            _label.pickingMode = PickingMode.Ignore;

            UpdateClasses();

            // Add to root
            Add(_label);
        }

        /// <summary>
        /// Updates USS class references
        /// </summary>
        internal virtual void UpdateClasses()
        {
            AddToClassList(GameRef.USS.POPUP_OPTION_BUTTON__BUTTON);
            _label.AddToClassList(GameRef.USS.POPUP_OPTION_BUTTON__LABEL);
        }


        /// <summary>
        /// Update the icon button listener methods
        /// </summary>
        public void AddAction(Action btnAction)
        {
            clicked += btnAction;
        }

        /// <summary>
        /// Update the icon button texture image
        /// </summary>
        public void UpdateIcon(Texture2D icon)
        {
            _label.style.backgroundImage = icon;
            _label.text = "";
        }


        /// <summary>
        /// Update the child label text
        /// </summary>
        public void UpdateText(string labelText)
        {
            _label.text = labelText;
            _label.style.backgroundImage = null;
        }
    }
}