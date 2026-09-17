using UnityEngine;
using UnityEngine.UI;

namespace TrainingSimulator.Interaction
{
    // Кнопка интерфейса. Нажатие приходит и от луча, и от мыши — источник неважен.
    [RequireComponent(typeof(Button))]
    public sealed class UIButtonInteractable : InteractableBase
    {
        [SerializeField] private Button button;

        public override ExpectedActionType ActionType => ExpectedActionType.UIButton;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!button) button = GetComponent<Button>();
            if (button) button.onClick.AddListener(Report);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (button) button.onClick.RemoveListener(Report);
        }
    }
}
