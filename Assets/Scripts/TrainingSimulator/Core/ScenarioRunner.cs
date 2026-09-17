using System.Collections;
using System.Collections.Generic;
using TrainingSimulator.Content;
using TrainingSimulator.Events;
using TrainingSimulator.Feedback;
using TrainingSimulator.Highlighting;
using TrainingSimulator.Interaction;
using TrainingSimulator.Runtime;
using UnityEngine;

namespace TrainingSimulator.Core
{
    // Прохождение сценария: группы и шаги по порядку, оценка каждого действия,
    // статусы для итогов. UI, звук и подсветку дёргает через интерфейсы.
    //
    // Как трактуются действия:
    //   цель текущего шага           — верно, шаг закрывается, когда сделаны все его действия;
    //   цель будущего шага группы    — нарушение порядка, группа закрывается, остаток «пропущен»;
    //   объект не из текущей группы  — неверно, текущий шаг закрывается «с ошибкой»;
    //   объект пройденного шага      — игнор.
    public sealed class ScenarioRunner : MonoBehaviour
    {
        [Header("Сценарий")]
        [SerializeField] private ScenarioDefinition scenario;

        [Header("Сервисы")]
        [SerializeField] private MonoBehaviour highlightService; // IHighlightService
        [SerializeField] private MonoBehaviour feedbackService;  // IFeedbackService

        [Header("Диагностика")]
        [SerializeField, Tooltip("Писать в консоль каждое действие и вердикт по нему.")]
        private bool logInteractions = true;

        private IHighlightService _highlight;
        private IFeedbackService _feedback;
        private IEventBus _bus;

        private StepStatus[][] _statuses; // [группа][шаг]

        private readonly Dictionary<string, int> _groupTargetToStep = new(); // цели текущей группы
        private readonly HashSet<string> _completedInStep = new();
        private readonly List<IInteractable> _highlighted = new();

        private int _groupIndex = -1;
        private int _stepIndex;
        private bool _finished;
        private bool _beginQueued;

        // Подписка в OnEnable, а не в Start: правка скриптов в Play Mode перезагружает
        // сборку, шина пересоздаётся, и Start второй раз уже не вызовется.
        private void OnEnable()
        {
            _highlight = highlightService as IHighlightService;
            _feedback = feedbackService as IFeedbackService;
            if (highlightService && _highlight == null)
                Debug.LogError("[Runner] highlightService не реализует IHighlightService.", this);
            if (feedbackService && _feedback == null)
                Debug.LogError("[Runner] feedbackService не реализует IFeedbackService.", this);

            _bus = SimulationContext.Current?.Bus;
            if (_bus == null)
            {
                Debug.LogError("[Runner] В сцене нет SimulationContext.", this);
                return;
            }
            if (!scenario || scenario.Groups.Count == 0)
            {
                Debug.LogError("[Runner] Не задан сценарий.", this);
                return;
            }

            _bus.Subscribe<InteractionPerformedSignal>(OnInteraction);

            // Статусы пусты и при первом запуске, и после перезагрузки сборки.
            if (_statuses == null) QueueBegin();
        }

        private void OnDisable() => _bus?.Unsubscribe<InteractionPerformedSignal>(OnInteraction);

        private void QueueBegin()
        {
            if (_beginQueued) return;
            _beginQueued = true;
            StartCoroutine(BeginNextFrame());
        }

        private IEnumerator BeginNextFrame()
        {
            yield return null; // ждём кадр, чтобы UI успел подписаться
            _beginQueued = false;
            BeginScenario();
        }

        private void BeginScenario()
        {
            // Индексы переживают перезагрузку сборки, а статусы нет. Если индекс
            // не начальный, значит прогон шёл и его состояние потерялось.
            if (_groupIndex >= 0)
                Debug.LogWarning("[Runner] Состояние прогона потеряно после перезагрузки сборки. " +
                                 "Сценарий запущен с начала.", this);

            _statuses = new StepStatus[scenario.Groups.Count][];
            for (int g = 0; g < scenario.Groups.Count; g++)
                _statuses[g] = new StepStatus[scenario.Groups[g].Steps.Count];

            _groupIndex = -1;
            _finished = false;
            AdvanceGroup();
        }

        private StepGroupDefinition CurrentGroup => scenario.Groups[_groupIndex];
        private StepDefinition CurrentStep => CurrentGroup.Steps[_stepIndex];

        private void AdvanceGroup()
        {
            _groupIndex++;
            if (_groupIndex >= scenario.Groups.Count)
            {
                Finish();
                return;
            }
            EnterGroup();
        }

        private void EnterGroup()
        {
            var group = CurrentGroup;

            // Карта целей группы — по ней отличаем нарушение порядка от ловушки.
            _groupTargetToStep.Clear();
            for (int s = 0; s < group.Steps.Count; s++)
                foreach (var action in group.Steps[s].Actions)
                    if (!string.IsNullOrEmpty(action.TargetId))
                        _groupTargetToStep[action.TargetId] = s;

            _stepIndex = 0;

            var descriptions = new List<string>(group.Steps.Count);
            foreach (var step in group.Steps) descriptions.Add(step.Description);
            _bus.Publish(new GroupStartedSignal(
                _groupIndex, scenario.Groups.Count, group.Title, group.Briefing, descriptions));

            if (logInteractions)
                Debug.Log($"[Runner] Группа {_groupIndex + 1}/{scenario.Groups.Count} «{group.Title}»", this);

            EnterStep();
        }

        private void EnterStep()
        {
            _completedInStep.Clear();
            HighlightCurrentStep();

            if (!logInteractions) return;
            var expected = new List<string>();
            foreach (var action in CurrentStep.Actions)
                expected.Add(action.ExpectedType + ":" + action.TargetId);
            Debug.Log($"[Runner] Шаг {_groupIndex + 1}.{_stepIndex + 1} «{CurrentStep.Description}» — " +
                      $"ожидается {string.Join(" + ", expected)}", this);
        }

        private void OnInteraction(InteractionPerformedSignal signal)
        {
            if (_finished || _groupIndex < 0 || signal.Source == null) return;
            var id = signal.Source.Id;

            if (!_groupTargetToStep.TryGetValue(id, out int owningStep))
            {
                Log(id, signal.Source, "НЕВЕРНО: объект не входит в текущую группу → шаг с ошибкой");
                RegisterWrongAction();
                return;
            }

            if (owningStep == _stepIndex)
            {
                if (_completedInStep.Contains(id))
                {
                    Log(id, signal.Source, "повтор: действие уже выполнено, игнор");
                    return;
                }
                Log(id, signal.Source, "верно");
                RegisterCorrectAction(signal.Source);
            }
            else if (owningStep > _stepIndex)
            {
                Log(id, signal.Source, $"НАРУШЕНИЕ ПОРЯДКА: это цель шага {_groupIndex + 1}.{owningStep + 1}, " +
                                       $"а активен шаг {_groupIndex + 1}.{_stepIndex + 1} → группа закрывается");
                RegisterSequenceViolation();
            }
            else
            {
                Log(id, signal.Source, "игнор: объект пройденного шага");
            }
        }

        private void Log(string id, IInteractable source, string verdict)
        {
            if (!logInteractions) return;
            Debug.Log($"[Runner] «{id}» ({source.ActionType}) → {verdict}", source.GameObject);
        }

        private void RegisterCorrectAction(IInteractable source)
        {
            _feedback?.PlayCorrect();
            _completedInStep.Add(source.Id);
            _highlight?.Clear(source);
            _highlighted.Remove(source);

            if (IsCurrentStepComplete())
                CompleteStep(StepStatus.CompletedClean);
        }

        private void RegisterWrongAction()
        {
            _feedback?.PlayViolation();
            CompleteStep(StepStatus.CompletedWithError);
        }

        private void RegisterSequenceViolation()
        {
            _feedback?.PlayViolation();
            ClearHighlights();

            for (int s = _stepIndex; s < CurrentGroup.Steps.Count; s++)
                SetStatus(_groupIndex, s, StepStatus.Skipped);

            AdvanceGroup();
        }

        private void CompleteStep(StepStatus status)
        {
            SetStatus(_groupIndex, _stepIndex, status);
            ClearHighlights();

            _stepIndex++;
            if (_stepIndex >= CurrentGroup.Steps.Count) AdvanceGroup();
            else EnterStep();
        }

        private bool IsCurrentStepComplete()
        {
            foreach (var action in CurrentStep.Actions)
                if (!_completedInStep.Contains(action.TargetId)) return false;
            return true;
        }

        private void HighlightCurrentStep()
        {
            ClearHighlights();
            foreach (var action in CurrentStep.Actions)
            {
                if (!InteractableRegistry.TryGet(action.TargetId, out var interactable))
                {
                    Debug.LogWarning($"[Runner] Не найден объект с id '{action.TargetId}'.", this);
                    continue;
                }
                _highlight?.Highlight(interactable);
                _highlighted.Add(interactable);
            }
        }

        private void ClearHighlights()
        {
            foreach (var interactable in _highlighted) _highlight?.Clear(interactable);
            _highlighted.Clear();
        }

        private void SetStatus(int group, int step, StepStatus status)
        {
            _statuses[group][step] = status;
            _bus.Publish(new StepStatusChangedSignal(group, step, status));
        }

        private void Finish()
        {
            _finished = true;
            ClearHighlights();
            _highlight?.ClearAll();

            var result = BuildResult();
            if (logInteractions)
                Debug.Log($"[Runner] Сценарий завершён: без ошибок {result.CleanSteps}/{result.TotalSteps}, " +
                          $"с ошибкой {result.ErrorSteps}, пропущено {result.SkippedSteps}", this);
            _bus.Publish(new ScenarioCompletedSignal(result));
        }

        private ScenarioResult BuildResult()
        {
            var groups = new List<GroupResult>(scenario.Groups.Count);
            for (int g = 0; g < scenario.Groups.Count; g++)
            {
                var groupDef = scenario.Groups[g];
                var steps = new List<StepResult>(groupDef.Steps.Count);
                for (int s = 0; s < groupDef.Steps.Count; s++)
                    steps.Add(new StepResult(groupDef.Steps[s].Description, _statuses[g][s]));
                groups.Add(new GroupResult(groupDef.Title, steps));
            }
            return new ScenarioResult(scenario.DisplayName, groups);
        }
    }
}
