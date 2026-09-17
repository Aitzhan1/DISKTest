using TrainingSimulator.Interaction;

namespace TrainingSimulator.Highlighting
{
    // Способ подсветки скрыт за интерфейсом: Quick Outline можно заменить на
    // Highlight Plus или Renderer Feature, не переписывая логику сценария.
    public interface IHighlightService
    {
        void Highlight(IInteractable target);
        void Clear(IInteractable target);
        void ClearAll();
    }
}
