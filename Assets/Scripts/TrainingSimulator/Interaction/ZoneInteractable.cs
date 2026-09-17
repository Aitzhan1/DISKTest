using UnityEngine;

namespace TrainingSimulator.Interaction
{
    // Точка интереса: срабатывает, когда игрок входит в зону.
    [RequireComponent(typeof(Collider))]
    public sealed class ZoneInteractable : InteractableBase
    {
        [SerializeField, Tooltip("Тег игрока — стоит на XR Origin.")]
        private string playerTag = "Player";

        public override ExpectedActionType ActionType => ExpectedActionType.ReachZone;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag)) return;
            Report();
        }
    }
}
