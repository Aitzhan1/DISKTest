namespace TrainingSimulator.Runtime
{
    public enum StepStatus
    {
        Pending,             // ещё не выполнялся
        CompletedClean,      // выполнен без ошибок
        CompletedWithError,  // выполнен, но было неверное действие
        Skipped              // пропущен из-за нарушения порядка в группе
    }
}
