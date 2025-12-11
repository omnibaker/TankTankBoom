using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sumfulla.TankTankBoom
{
    public class PopUp
    {
        private VisualElement _root;
        private Label _header;
        private Label _message;
        private VisualElement _base;
        private VisualElement _body;
        private VisualElement _buttonContainer;
        private IconButton _close;
        public VisualElement Root { get { return _root; } } 

        private event Action OnCloseEvent;

        private bool _isDisposed;
        public PopUp()
        {
            _root = GameUtils.UITK.InstantiateUXML(GameRef.UIRef.UXML_POPUP, TemplateShape.ABSOLUTE);

            _base = _root.Q(GameRef.UIRef.POPUP);
            _header = _root.Q<Label>(GameRef.UIRef.POPUP_TITLE);
            _message = _root.Q<Label>(GameRef.UIRef.POPUP_MESSAGE);
            _body = _root.Q(GameRef.UIRef.POPUP_BODY);
            _buttonContainer = _root.Q(GameRef.UIRef.POPUP_BUTTON_CONTAINER_VERT);

            // Assign close callback (will need to be enabled manually to display
            _close = _root.Q<IconButton>(GameRef.UIRef.POPUP_CLOSE_BUTTON);
            _close.Btn?.RegisterCallback<ClickEvent>((evt) =>
            {
                evt.StopPropagation();
                OnCloseEvent?.Invoke();
            });

            // Hide header by default
            RemoveTitle();

            // Hide top-right close icon by default
            EnableCloseButton(false);

            // Add 'destroy this' to close action
            AddActionToClose(KillMe);
        }

        /// <summary>
        /// Static public getter to create, configure and place popup in UI
        /// </summary>
        public static PopUp InstantiatePopUp()
        {
            if (GameData.Foreground == null)
            {
                GameLog.Shout("No foreground - cannot create popup item");
                return null;
            }

            // Instantiate new class instance
            PopUp newPop = new PopUp();

            // Enable close/destroy mechanism
            newPop.AddActionToClose(() => { newPop = null; });

            // Add to current foreground UI
            GameData.Foreground.Add(newPop.Root);

            // Return instance to caller
            return newPop;
        }

        /// <summary>
        /// Adds labelled button to popup panel
        /// </summary>
        public void AddButton(Action action, string label, bool closer = true)
        {
            // Create button for SCENE.PLAY, update label, add behaviour, add to list
            OptionButton option_play = new OptionButton();
            option_play.UpdateText(label);
            option_play.AddAction(() =>
            {
                GameLog.Say($"Pressed popup button");
                action?.Invoke();
                if (closer) OnCloseEvent?.Invoke();
            });

            _buttonContainer.Add(option_play);
        }

        /// <summary>
        /// Hides the header section if not required
        /// </summary>
        public void RemoveTitle()
        {
            _header.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Updates text of popup title in header
        /// </summary>
        public void UpdateTitle(string message)
        {
            _header.text = message;
            _header.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Updates text of popup main message area
        /// </summary>
        public void UpdateMessage(string message)
        {
            _message.text = message;
        }

        /// <summary>
        /// Hides the close button at top-right if not required
        /// </summary>
        public void EnableCloseButton(bool enable)
        {
            _close.style.display = enable ? DisplayStyle.Flex: DisplayStyle.None;
        }

        /// <summary>
        /// Adds any actions to the 'close' icon at the top-right
        /// </summary>
        public void AddActionToClose(Action action)
        {
            OnCloseEvent += action;
        }

        /// <summary>
        /// Removes VisualElement and deletes this object from memory
        /// </summary>
        public void KillMe()
        {
            // Avoid a 'double dispose' is somehow out of sync
            if (_isDisposed) return;
            _isDisposed = true;

            // Remove event subscription to avoid leaks
            _close.Btn.clicked -= OnCloseEvent;
            _root.RemoveFromHierarchy();
        }
    }
}