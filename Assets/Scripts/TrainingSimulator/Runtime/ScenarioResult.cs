using System;
using System.Collections.Generic;

namespace TrainingSimulator.Runtime
{
    public sealed class StepResult
    {
        public string Description { get; }
        public StepStatus Status { get; }

        public StepResult(string description, StepStatus status)
        {
            Description = description;
            Status = status;
        }
    }

    public sealed class GroupResult
    {
        public string Title { get; }
        public IReadOnlyList<StepResult> Steps { get; }

        public GroupResult(string title, IReadOnlyList<StepResult> steps)
        {
            Title = title;
            Steps = steps;
        }
    }

    // Итог прогона для экрана результатов: дерево групп и шагов плюс счётчики.
    public sealed class ScenarioResult
    {
        public string ScenarioName { get; }
        public IReadOnlyList<GroupResult> Groups { get; }

        public ScenarioResult(string scenarioName, IReadOnlyList<GroupResult> groups)
        {
            ScenarioName = scenarioName;
            Groups = groups;
        }

        public int TotalSteps => Count(_ => true);
        public int CleanSteps => Count(s => s.Status == StepStatus.CompletedClean);
        public int ErrorSteps => Count(s => s.Status == StepStatus.CompletedWithError);
        public int SkippedSteps => Count(s => s.Status == StepStatus.Skipped);

        private int Count(Func<StepResult, bool> predicate)
        {
            var n = 0;
            foreach (var g in Groups)
                foreach (var s in g.Steps)
                    if (predicate(s)) n++;
            return n;
        }
    }
}
