using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace TrainingSimulator.Interaction
{
    // Выбор объекта лучом. В отличие от захвата объект остаётся на месте.
    [RequireComponent(typeof(XRSimpleInteractable))]
    public sealed class PointerClickInteractable : InteractableBase
    {
        [SerializeField] private XRSimpleInteractable simpleInteractable;

        public override ExpectedActionType ActionType => ExpectedActionType.PointerClick;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!simpleInteractable) simpleInteractable = GetComponent<XRSimpleInteractable>();
            if (simpleInteractable) simpleInteractable.selectEntered.AddListener(OnSelectEntered);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (simpleInteractable) simpleInteractable.selectEntered.RemoveListener(OnSelectEntered);
        }

        private void OnSelectEntered(SelectEnterEventArgs _) => Report();
    }
}
