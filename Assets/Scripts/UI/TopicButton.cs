using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sumfulla.TankTankBoom
{
    [UxmlElement]
    partial class TopicButton : IconButton
    {
        /// <summary>
        /// Updates USS class references
        /// </summary>
        public TopicButton()
        {
            AddToClassList(GameRef.USS.TOPIC_BUTTON__ROOT__LANDSCAPE);
            Btn.AddToClassList(GameRef.USS.TOPIC_BUTTON__BUTTON__LANDSCAPE);
        }
    }
}