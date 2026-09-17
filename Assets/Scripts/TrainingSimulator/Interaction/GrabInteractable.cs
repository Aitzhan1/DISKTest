using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace TrainingSimulator.Interaction
{
    // Захват объекта: заворачивает selectEntered от XRI в наш сигнал.
    [RequireComponent(typeof(XRGrabInteractable))]
    public sealed class GrabInteractable : InteractableBase
    {
        [SerializeField] private XRGrabInteractable grab;

        public override ExpectedActionType ActionType => ExpectedActionType.Grab;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!grab) grab = GetComponent<XRGrabInteractable>();
            if (grab) grab.selectEntered.AddListener(OnSelectEntered);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (grab) grab.selectEntered.RemoveListener(OnSelectEntered);
        }

        private void OnSelectEntered(SelectEnterEventArgs _) => Report();
    }
}
