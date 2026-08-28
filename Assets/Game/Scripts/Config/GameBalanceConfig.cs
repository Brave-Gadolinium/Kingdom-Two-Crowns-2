using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "GameBalance",
    menuName = "Game/Balance/Game Balance")]
public class GameBalanceConfig : ScriptableObject
{
    public TimeBalanceConfig time;
    public EconomyBalanceConfig economy;
    public TerritoryBalanceConfig territory;

    public List<string> ValidateConfig()
    {
        var errors = new List<string>();

        if (time == null)
            errors.Add("Не назначен TimeBalanceConfig.");

        if (economy == null)
            errors.Add("Не назначен EconomyBalanceConfig.");

        if (territory == null)
            errors.Add("Не назначен TerritoryBalanceConfig.");

        if (time != null)
        {
            if (time.nightDuration <= 0)
                errors.Add("Night Duration должен быть больше 0.");

            if (time.dayDuration <= 0)
                errors.Add("Day Duration должен быть больше 0.");
        }

        if (economy != null && economy.startingGreed < 0)
            errors.Add("Starting Greed не может быть меньше 0.");

        if (territory != null && territory.expansionSize <= 0)
            errors.Add("Expansion Size должен быть больше 0.");

        return errors;
    }
}