using System.Collections.Generic;
using TrainingSimulator.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace TrainingSimulator.Highlighting
{
    // Подсветка через Quick Outline: компонент Outline добавляется по месту и
    // дальше просто включается и выключается.
    // Он умеет только 3D-рендереры, поэтому кнопки на канвасе подсвечиваются
    // штатным UnityEngine.UI.Outline тем же цветом.
    public sealed class QuickOutlineHighlightService : MonoBehaviour, IHighlightService
    {
        [SerializeField] private Color color = new(1f, 0.85f, 0.1f);
        [SerializeField, Range(0f, 10f)] private float width = 6f;
        [SerializeField] private Outline.Mode mode = Outline.Mode.OutlineVisible;

        [Header("UI-цели")]
        [SerializeField] private Vector2 uiOutlineDistance = new(8f, -8f);

        private readonly HashSet<Behaviour> _active = new();

        public void Highlight(IInteractable target)
        {
            var go = target?.GameObject;
            if (go == null) return;

            var effect = go.GetComponent<Graphic>() != null ? GetUIOutline(go) : GetMeshOutline(go);
            effect.enabled = true;
            _active.Add(effect);
        }

        public void Clear(IInteractable target)
        {
            var go = target?.GameObject;
            if (go == null) return;

            Disable(go.GetComponent<Outline>());
            Disable(go.GetComponent<UnityEngine.UI.Outline>());
        }

        public void ClearAll()
        {
            foreach (var effect in _active)
                if (effect != null) effect.enabled = false;
            _active.Clear();
        }

        private Behaviour GetMeshOutline(GameObject go)
        {
            var outline = go.GetComponent<Outline>();
            if (outline == null) outline = go.AddComponent<Outline>();
            outline.OutlineColor = color;
            outline.OutlineWidth = width;
            outline.OutlineMode = mode;
            return outline;
        }

        private Behaviour GetUIOutline(GameObject go)
        {
            var outline = go.GetComponent<UnityEngine.UI.Outline>();
            if (outline == null) outline = go.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = color;
            outline.effectDistance = uiOutlineDistance;
            return outline;
        }

        private void Disable(Behaviour effect)
        {
            if (effect == null) return;
            effect.enabled = false;
            _active.Remove(effect);
        }
    }
}
