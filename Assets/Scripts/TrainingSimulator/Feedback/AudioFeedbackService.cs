using UnityEngine;

namespace TrainingSimulator.Feedback
{
    // Клипы можно подставить свои; если их нет, тоны генерируются в рантайме,
    // чтобы не тянуть в проект внешние ассеты.
    [RequireComponent(typeof(AudioSource))]
    public sealed class AudioFeedbackService : MonoBehaviour, IFeedbackService
    {
        [SerializeField, Tooltip("Необязательно. Если пусто — тон сгенерируется.")]
        private AudioClip correctClip;

        [SerializeField, Tooltip("Необязательно. Если пусто — тон сгенерируется.")]
        private AudioClip violationClip;

        [SerializeField, Range(0f, 1f)] private float volume = 0.7f;

        private AudioSource _source;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _source.playOnAwake = false;

            if (!correctClip) correctClip = CreateTone("correct", 880f, 0.16f);
            if (!violationClip) violationClip = CreateTone("violation", 160f, 0.30f);
        }

        public void PlayCorrect() => Play(correctClip);
        public void PlayViolation() => Play(violationClip);

        private void Play(AudioClip clip)
        {
            if (clip && _source) _source.PlayOneShot(clip, volume);
        }

        // Синус с затуханием к концу — без него на обрыве слышен щелчок.
        private static AudioClip CreateTone(string clipName, float frequency, float duration)
        {
            const int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            var data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Clamp01((duration - t) / duration);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope;
            }

            var clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
