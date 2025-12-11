using System;
using UnityEngine;
using UnityEngine.UIElements;


namespace Sumfulla.TankTankBoom
{
    [UxmlElement]
    public partial class MenuButton : OptionButton
    {
        /// <summary>
        /// Updates USS class references
        /// </summary>
        internal override void UpdateClasses()
        {
            AddToClassList(GameRef.USS.MENU_OPTION_BUTTON__BUTTON);
            _label.AddToClassList(GameRef.USS.MENU_OPTION_BUTTON__LABEL);
        }
    }
}