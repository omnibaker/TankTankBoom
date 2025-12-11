using System;
using UnityEngine.UIElements;

namespace Sumfulla.TankTankBoom
{
    [UxmlElement]
    public partial class ListButton : BaseField<bool>
    {
        private Label _listLabelField;
        private VisualElement _inputArea;
        private VisualElement _buttonContainer;
        private VisualElement _button;
        private VisualElement _buttonFrame;

        public event Action OnSelectionEvent;

        // Label handler
        [UxmlAttribute(GameRef.UIRef.UXML_ATR__LIST_LABEL)]
        public string ListButtonLabel
        {
            get => _listButtonLabel;
            set
            {
                _listButtonLabel = value;
                _listLabelField.text = _listButtonLabel;
            }
        }
        private string _listButtonLabel;

        // Button handler
        [UxmlAttribute(GameRef.UIRef.UXML_ATR__IS_PRESSED)]
        public bool IsPressed
        {
            get => _isHeld;
            set => _isHeld = value;
        }
        private bool _isHeld;


        // Activated handler
        [UxmlAttribute(GameRef.UIRef.UXML_ATR__ACTIVATED)]
        public bool Activated
        {
            get => _activated;
            set => _activated = value;
        }
        private bool _activated;

        public ListButton() : this(null) { }

        public ListButton(string label) : base(label, null)
        {
            // Root Style
            AddToClassList(ussClassName);
            AddToClassList(GameRef.UIRef.LIST_BUTTON);

            // Get the BaseField's visual input element and use it as the background of the slide.
            _inputArea = this.Q(className: BaseField<bool>.inputUssClassName);
            _inputArea.AddToClassList(GameRef.UIRef.LIST_BUTTON__INPUT);
            _inputArea.name = GameRef.UIRef.LIST_BUTTON__INPUT;
            Add(_inputArea);

            // Add row label
            _listLabelField = new Label(label);
            _listLabelField.AddToClassList(GameRef.UIRef.LIST__LABEL);
            _listLabelField.name = GameRef.UIRef.LIST__LABEL;
            _inputArea.Add(_listLabelField);

            // Add button container
            _buttonContainer = new();
            _buttonContainer.AddToClassList(GameRef.UIRef.LIST_BUTTON__BUTTON_CONTAINER);
            _buttonContainer.name = GameRef.UIRef.LIST_BUTTON__BUTTON_CONTAINER;
            _inputArea.Add(_buttonContainer);

            // Add button frame
            _buttonFrame = new();
            _buttonFrame.AddToClassList(GameRef.UIRef.LIST_BUTTON__BUTTON_FRAME);
            _buttonFrame.name = GameRef.UIRef.LIST_BUTTON__BUTTON_FRAME;
            _buttonContainer.Add(_buttonFrame);

            // Add button 
            _button = new();
            _button.AddToClassList(GameRef.UIRef.LIST_BUTTON__BUTTON);
            _button.EnableInClassList(GameRef.UIRef.LIST_BUTTON__BUTTON__CHECKED, false);
            _button.name = GameRef.UIRef.LIST_BUTTON__BUTTON;
            _buttonFrame.Add(_button);
        }

        /// <summary>
        /// Set up event callbacks for buttons when pressed
        /// </summary>
        public void RegisterOnClickCallback(bool noActivationVisual)
        {
            RegisterCallback<ClickEvent>(evt => OnClick(evt, noActivationVisual));

            if (!noActivationVisual) return;

            RegisterCallback<PointerDownEvent>(evt =>
            {
                _isHeld = true;
            });

            RegisterCallback<PointerUpEvent>(evt =>
            {
                _isHeld = false;
            });

            RegisterCallback<PointerOutEvent>(evt =>
            {
                _isHeld = false;
            });

            RegisterCallback<PointerLeaveEvent>(evt =>
            {
                _isHeld = false;
            });
        }

        /// <summary>
        /// Change button display with coloured indication of being pressed/active
        /// </summary>
        private void ActivateButton(bool activated)
        {
            _button.EnableInClassList(GameRef.UIRef.LIST_BUTTON__BUTTON__CHECKED, activated);
        }

        /// <summary>
        /// Behaviour when button is clicked/pressed 
        /// </summary>    
        private void OnClick(ClickEvent evt, bool noActivationVisual)
        {
            OnSelectionEvent?.Invoke();
            _activated = !_activated;

            // Don't display 'on' green marker if undesired
            if (!noActivationVisual)
            {
                ActivateButton(_activated);
            }
        }

        /// <summary>
        /// Includes a foregin action to the selection event
        /// </summary>
        public void UpdateButtonEvent(Action action)
        {
            OnSelectionEvent += () => action.Invoke();
        }
    } 
}