using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class PhaseAudioPresenter : MonoBehaviour
{
    [SerializeField] private PhaseDriver phaseDriver;
    [SerializeField] private AudioClip dawnWarning;
    [SerializeField] private AudioClip nightStarted;

    private AudioSource source;
    private PhaseService subscribedService;

    private void Awake() => source = GetComponent<AudioSource>();

    private void Update()
    {
        PhaseService service = phaseDriver != null ? phaseDriver.Service : null;
        if (ReferenceEquals(service, subscribedService))
            return;

        Unsubscribe();
        subscribedService = service;
        if (subscribedService != null)
            subscribedService.PhaseChanged += OnPhaseChanged;
    }

    private void OnPhaseChanged(PhaseChanged change)
    {
        AudioClip clip = change.Current == DayPhase.Dawn
            ? dawnWarning
            : change.Current == DayPhase.Night ? nightStarted : null;

        if (clip != null && source != null)
            source.PlayOneShot(clip);
    }

    private void OnDestroy() => Unsubscribe();

    private void Unsubscribe()
    {
        if (subscribedService != null)
            subscribedService.PhaseChanged -= OnPhaseChanged;

        subscribedService = null;
    }
}
