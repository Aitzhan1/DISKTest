using System.Collections.Generic;
using System.Text;
using TMPro;
using TrainingSimulator.Core;
using TrainingSimulator.Events;
using TrainingSimulator.Runtime;
using UnityEngine;

namespace TrainingSimulator.UI
{
    // Табло текущей группы: заголовок, брифинг и чек-лист с отметками.
    // Живёт на сигналах, про ScenarioRunner не знает.
    public sealed class GroupBriefingView : MonoBehaviour
    {
        [SerializeField] private TMP_Text progressLabel;   // «Группа 1/3»
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text briefingLabel;
        [SerializeField] private TMP_Text checklistLabel;

        private IEventBus _bus;
        private int _currentGroup;
        private readonly List<string> _descriptions = new();
        private readonly List<StepStatus> _statuses = new();

        // Подписка в OnEnable: после перезагрузки сборки шина новая, а Start не повторяется.
        private void OnEnable()
        {
            _bus = SimulationContext.Current?.Bus;
            if (_bus == null)
            {
                Debug.LogError("[BriefingView] В сцене нет SimulationContext.", this);
                return;
            }
            _bus.Subscribe<GroupStartedSignal>(OnGroupStarted);
            _bus.Subscribe<StepStatusChangedSignal>(OnStepStatusChanged);
        }

        private void OnDisable()
        {
            _bus?.Unsubscribe<GroupStartedSignal>(OnGroupStarted);
            _bus?.Unsubscribe<StepStatusChangedSignal>(OnStepStatusChanged);
        }

        private void OnGroupStarted(GroupStartedSignal s)
        {
            _currentGroup = s.GroupIndex;
            if (progressLabel) progressLabel.text = $"Группа {s.GroupIndex + 1}/{s.GroupCount}";
            if (titleLabel) titleLabel.text = s.Title;
            if (briefingLabel) briefingLabel.text = s.Briefing;

            _descriptions.Clear();
            _statuses.Clear();
            foreach (var d in s.StepDescriptions)
            {
                _descriptions.Add(d);
                _statuses.Add(StepStatus.Pending);
            }
            Redraw();
        }

        private void OnStepStatusChanged(StepStatusChangedSignal s)
        {
            if (s.GroupIndex != _currentGroup) return;
            if (s.StepIndex < 0 || s.StepIndex >= _statuses.Count) return;
            _statuses[s.StepIndex] = s.Status;
            Redraw();
        }

        private void Redraw()
        {
            if (!checklistLabel) return;
            var sb = new StringBuilder();
            for (int i = 0; i < _descriptions.Count; i++)
                sb.Append(Mark(_statuses[i])).Append(' ').AppendLine(_descriptions[i]);
            checklistLabel.text = sb.ToString();
        }

        // Галочек ✔/✖ в LiberationSans SDF нет, поэтому берём символы попроще.
        private static string Mark(StepStatus status) => status switch
        {
            StepStatus.CompletedClean => "<color=#3fbf5f>●</color>",
            StepStatus.CompletedWithError => "<color=#e0a020>×</color>",
            StepStatus.Skipped => "<color=#e05050>—</color>",
            _ => "<color=#8a93a6>○</color>"
        };
    }
}
