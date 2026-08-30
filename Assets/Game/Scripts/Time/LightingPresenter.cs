using UnityEngine;
using UnityEngine.Rendering.Universal;

public sealed class LightingPresenter : MonoBehaviour
{
    [SerializeField] private PhaseDriver phaseDriver;
    [SerializeField] private Light2D globalLight;
    [SerializeField] private Color nightColor = new Color(0.16f, 0.22f, 0.45f);
    [SerializeField] private Color dawnColor = new Color(0.85f, 0.48f, 0.34f);
    [SerializeField] private Color dayColor = Color.white;
    [SerializeField] private Color duskColor = new Color(0.72f, 0.32f, 0.42f);

    private void Update()
    {
        PhaseService service = phaseDriver != null ? phaseDriver.Service : null;
        if (service == null || globalLight == null)
            return;

        globalLight.color = GetPhaseColor(service.Phase, service.PhaseProgress01);
    }

    private Color GetPhaseColor(DayPhase phase, float progress)
    {
        return phase switch
        {
            DayPhase.Night => nightColor,
            DayPhase.Dawn => progress < 0.5f
                ? Color.Lerp(nightColor, dawnColor, progress * 2f)
                : Color.Lerp(dawnColor, dayColor, (progress - 0.5f) * 2f),
            DayPhase.Day => dayColor,
            DayPhase.Dusk => progress < 0.5f
                ? Color.Lerp(dayColor, duskColor, progress * 2f)
                : Color.Lerp(duskColor, nightColor, (progress - 0.5f) * 2f),
            _ => dayColor
        };
    }
}
