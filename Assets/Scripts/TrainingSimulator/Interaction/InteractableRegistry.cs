using System.Collections.Generic;
using UnityEngine;

namespace TrainingSimulator.Interaction
{
    // Связка «id из сценария → объект сцены». Объекты регистрируются сами,
    // так что ScriptableObject не держит ссылок на сцену.
    public static class InteractableRegistry
    {
        private static readonly Dictionary<string, IInteractable> Map = new();

        public static void Register(IInteractable interactable)
        {
            if (interactable == null || string.IsNullOrEmpty(interactable.Id)) return;
            if (Map.TryGetValue(interactable.Id, out var existing) && existing != interactable)
                Debug.LogWarning($"[Registry] Дублирующийся id '{interactable.Id}' — перезаписан.");
            Map[interactable.Id] = interactable;
        }

        public static void Unregister(IInteractable interactable)
        {
            if (interactable == null) return;
            if (Map.TryGetValue(interactable.Id, out var existing) && existing == interactable)
                Map.Remove(interactable.Id);
        }

        public static bool TryGet(string id, out IInteractable interactable)
            => Map.TryGetValue(id, out interactable);
    }
}
