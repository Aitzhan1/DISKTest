using UnityEngine;
using UnityEngine.UI;

namespace TrainingSimulator.App
{
    // Кнопки лобби. Слушатели навешиваются кодом, чтобы связи не терялись в инспекторе.
    public sealed class LobbyMenu : MonoBehaviour
    {
        [SerializeField] private Button startTrainingButton;
        [SerializeField] private Button quitButton;

        private void OnEnable()
        {
            if (startTrainingButton) startTrainingButton.onClick.AddListener(StartTraining);
            if (quitButton) quitButton.onClick.AddListener(Quit);
        }

        private void OnDisable()
        {
            if (startTrainingButton) startTrainingButton.onClick.RemoveListener(StartTraining);
            if (quitButton) quitButton.onClick.RemoveListener(Quit);
        }

        public void StartTraining() => SceneFlow.LoadTraining();

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
