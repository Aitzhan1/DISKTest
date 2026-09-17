using UnityEngine.SceneManagement;

namespace TrainingSimulator.App
{
    // Имена сцен должны совпадать с ассетами в Build Settings.
    public static class SceneFlow
    {
        public const string Lobby = "Lobby";
        public const string Training = "Training";

        public static void LoadLobby() => SceneManager.LoadScene(Lobby);
        public static void LoadTraining() => SceneManager.LoadScene(Training);

        // Перезагрузка сцены сбрасывает и прогресс, и все интерактивные объекты.
        public static void RestartCurrent() =>
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
