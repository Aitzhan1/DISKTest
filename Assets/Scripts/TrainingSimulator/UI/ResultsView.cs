using System.Text;
using TMPro;
using TrainingSimulator.App;
using TrainingSimulator.Core;
using TrainingSimulator.Events;
using TrainingSimulator.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace TrainingSimulator.UI
{
    // Итоги прогона: список шагов со статусами, сводка и кнопки «Попытаться ещё» /
    // «Возврат в лобби». Панель (root) скрыта до конца сценария.
    public sealed class ResultsView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text headerLabel;
        [SerializeField] private TMP_Text bodyLabel;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button lobbyButton;

        private IEventBus _bus;

        private void Awake()
        {
            if (root) root.SetActive(false);
        }

        // Подписка в OnEnable: после перезагрузки сборки шина новая, а Start не повторяется.
        private void OnEnable()
        {
            _bus = SimulationContext.Current?.Bus;
            if (_bus == null)
            {
                Debug.LogError("[ResultsView] В сцене нет SimulationContext.", this);
                return;
            }
            _bus.Subscribe<ScenarioCompletedSignal>(OnScenarioCompleted);
            _bus.Subscribe<GroupStartedSignal>(OnGroupStarted);

            if (restartButton) restartButton.onClick.AddListener(SceneFlow.RestartCurrent);
            if (lobbyButton) lobbyButton.onClick.AddListener(SceneFlow.LoadLobby);
        }

        private void OnDisable()
        {
            _bus?.Unsubscribe<ScenarioCompletedSignal>(OnScenarioCompleted);
            _bus?.Unsubscribe<GroupStartedSignal>(OnGroupStarted);
            if (restartButton) restartButton.onClick.RemoveListener(SceneFlow.RestartCurrent);
            if (lobbyButton) lobbyButton.onClick.RemoveListener(SceneFlow.LoadLobby);
        }

        private void OnScenarioCompleted(ScenarioCompletedSignal signal) => Show(signal.Result);

        // Начался новый прогон — прячем прошлые итоги.
        private void OnGroupStarted(GroupStartedSignal signal)
        {
            if (root) root.SetActive(false);
        }

        private void Show(ScenarioResult result)
        {
            if (root) root.SetActive(true);

            if (headerLabel)
                headerLabel.text =
                    $"{result.ScenarioName}\n" +
                    $"Без ошибок: {result.CleanSteps}/{result.TotalSteps}    " +
                    $"С ошибкой: {result.ErrorSteps}    Пропущено: {result.SkippedSteps}";

            if (!bodyLabel) return;
            var sb = new StringBuilder();
            foreach (var group in result.Groups)
            {
                sb.AppendLine($"<b>{group.Title}</b>");
                foreach (var step in group.Steps)
                    sb.AppendLine($"  • {step.Description} — {Label(step.Status)}");
                sb.AppendLine();
            }
            bodyLabel.text = sb.ToString();
        }

        private static string Label(StepStatus status) => status switch
        {
            StepStatus.CompletedClean => "<color=#3fbf5f>выполнено без ошибок</color>",
            StepStatus.CompletedWithError => "<color=#e0a020>выполнено с ошибкой</color>",
            StepStatus.Skipped => "<color=#e05050>пропущено</color>",
            _ => "<color=#888888>не выполнено</color>"
        };
    }
}
