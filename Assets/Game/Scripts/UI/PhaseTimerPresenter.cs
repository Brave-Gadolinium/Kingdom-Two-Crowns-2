using TMPro;
using UnityEngine;

public sealed class PhaseTimerPresenter : MonoBehaviour
{
    [SerializeField] private PhaseDriver phaseDriver;
    [SerializeField] private TMP_Text label;

    private int displayedSeconds = -1;
    private DayPhase displayedPhase = (DayPhase)(-1);

    private void Update()
    {
        PhaseService service = phaseDriver != null ? phaseDriver.Service : null;
        if (service == null || label == null)
            return;

        int seconds = Mathf.CeilToInt(service.RemainingTime);
        if (seconds == displayedSeconds && service.Phase == displayedPhase)
            return;

        displayedSeconds = seconds;
        displayedPhase = service.Phase;
        label.text = $"{GetPhaseName(service.Phase)}: {seconds / 60:00}:{seconds % 60:00}";
    }

    private static string GetPhaseName(DayPhase phase)
    {
        return phase switch
        {
            DayPhase.Night => "Ночь",
            DayPhase.Dawn => "Рассвет",
            DayPhase.Day => "День",
            DayPhase.Dusk => "Закат",
            _ => string.Empty
        };
    }
}
