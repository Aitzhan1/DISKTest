namespace TrainingSimulator.Feedback
{
    // Отклик на верное действие и на нарушение. За интерфейсом, чтобы можно было
    // добавить хаптику или VFX, не трогая логику сценария.
    public interface IFeedbackService
    {
        void PlayCorrect();
        void PlayViolation();
    }
}
