using System.Collections.Generic;
using UnityEngine;

public sealed class SunExposureCoordinator : MonoBehaviour
{
    [SerializeField] private PhaseDriver phaseDriver;
    [SerializeField] private TimeBalanceConfig timeBalance;

    private SunExposureService service;
    private readonly HashSet<ISunExposureTarget> registeredTargets = new();

    private IInfectionTerritory territory;

    public void Initialize(IInfectionTerritory infectionTerritory)
    {
        territory = infectionTerritory;
    }

    private void Start()
    {
        if (phaseDriver == null || phaseDriver.Service == null || territory == null || timeBalance == null)
        {
            Debug.LogError("SunExposureCoordinator: не назначены обязательные зависимости.", this);
            enabled = false;
            return;
        }

        service = new SunExposureService(
            phaseDriver.Service,
            new TerritoryInfectionQuery(territory),
            timeBalance.sunDamagePerSecond,
            timeBalance.sunGraceDuration);

        foreach (ISunExposureTarget target in registeredTargets)
            service.Register(target);
    }

    private void Update()
    {
        service?.Tick(Time.deltaTime);
    }

    public void Register(ISunExposureTarget target)
    {
        if (target == null || !registeredTargets.Add(target))
            return;

        service?.Register(target);
    }

    public void Unregister(ISunExposureTarget target)
    {
        if (target == null || !registeredTargets.Remove(target))
            return;

        service?.Unregister(target);
    }
}
