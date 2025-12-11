using System;
using UnityEngine.UIElements;

namespace Sumfulla.TankTankBoom
{
    [UxmlElement]
    public partial class ListToggle : BaseField<bool>
    {
        private Label _listLabelField;
        private VisualElement _inputArea;
        private VisualElement _inputAreaContainer;
        private VisualElement _knob;

        public event Action<bool> OnToggleValueChangeEvent;

        // Label handler
        [UxmlAttribute(GameRef.UIRef.UXML_ATR__LIST_LABEL)]
        public string ListToggleLabel
        {
            get => _listToggleLabel;
            set
            {
                _listToggleLabel = value;
                _listLabelField.text = _listToggleLabel;
            }
        }
        private string _listToggleLabel;

        /// <summary>
        /// Custom controls need a default constructor. This default constructor calls the other constructor in this class.
        /// </summary>
        public ListToggle() : this(null) { }

        /// <summary>
        /// This constructor allows users to set the contents of the label.
        /// </summary>
        public ListToggle(string label) : base(label, null)
        {
            // Root Style
            AddToClassList(ussClassName);
            AddToClassList(GameRef.UIRef.LIST_TOGGLE);

            // Add row label
            _listLabelField = new Label(ListToggleLabel);
            _listLabelField.AddToClassList(GameRef.UIRef.LIST__LABEL);
            _listLabelField.name = GameRef.UIRef.LIST__LABEL;
            Add(_listLabelField);

            // Add input container
            _inputAreaContainer = new VisualElement();
            _inputAreaContainer.name = GameRef.UIRef.LIST_TOGGLE__INPUT_CONTAINER;
            Add(_inputAreaContainer);

            // Add toggle area
            _inputArea = this.Q(className: BaseField<bool>.inputUssClassName);
            _inputArea.AddToClassList(GameRef.UIRef.LIST_TOGGLE__INPUT);
            _inputArea.name = GameRef.UIRef.LIST_TOGGLE__INPUT;
            _inputAreaContainer.Add(_inputArea);

            // Add toggle know
            _knob = new();
            _knob.AddToClassList(GameRef.UIRef.LIST_TOGGLE__INPUT_KNOB);
            _knob.name = GameRef.UIRef.LIST_TOGGLE__INPUT_KNOB;
            _inputArea.Add(_knob);

            RegisterCallback<ClickEvent>(evt => OnClick(evt));

            // NavigationSubmitEvent detects input from keyboards, gamepads, or other devices at runtime.
            RegisterCallback<NavigationSubmitEvent>(evt => OnSubmit(evt));
        }

        /// <summary>
        /// Tiggers submission of toggle update
        /// </summary>
        private static void OnSubmit(NavigationSubmitEvent evt)
        {
            ListToggle slideToggle = evt.currentTarget as ListToggle;
            slideToggle.ToggleValue();

            evt.StopPropagation();
        }

        /// <summary>
        /// Behaviour when button is clicked/pressed 
        /// </summary>  
        private void OnClick(ClickEvent evt)
        {
            ListToggle slideToggle = evt.currentTarget as ListToggle;
            slideToggle.ToggleValue();

            evt.StopPropagation();
        }

        /// <summary>
        /// All three callbacks call this method.
        /// </summary>
        private void ToggleValue()
        {
            value = !value;
            OnToggleValueChangeEvent?.Invoke(value);
        }

        // Because ToggleValue() sets the value property, the BaseField class dispatches a ChangeEvent. This results in a
        // call to SetValueWithoutNotify(). This example uses it to style the toggle based on whether it's currently
        // enabled.
        public override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);

            //This line of code styles the input element to look enabled or disabled.
            _inputArea.EnableInClassList(GameRef.UIRef.LIST_TOGGLE__INPUT__CHECKED, newValue);
        }

        /// <summary>
        /// Includes a foregin action to the selection event
        /// </summary>
        public void UpdateToggleEvent(Action<bool> action)
        {
            OnToggleValueChangeEvent += action;
        }
    }
}