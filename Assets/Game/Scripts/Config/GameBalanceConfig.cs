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

            if (time.dawnDuration <= 0)
                errors.Add("Dawn Duration должен быть больше 0.");

            if (time.dayDuration <= 0)
                errors.Add("Day Duration должен быть больше 0.");

            if (time.duskDuration <= 0)
                errors.Add("Dusk Duration должен быть больше 0.");
        }

        if (economy != null)
        {
            if (economy.startingGreed < 0)
                errors.Add("Starting Greed не может быть меньше 0.");

            if (economy.recruitCost <= 0)
                errors.Add("Recruit Cost должен быть больше 0.");

            if (economy.wallCost <= 0)
                errors.Add("Wall Cost должен быть больше 0.");

            if (economy.towerCost <= 0)
                errors.Add("Tower Cost должен быть больше 0.");
        }

        if (territory != null)
        {
            if (territory.startingSize <= 0)
                errors.Add("Starting Size должен быть больше 0.");

            if (territory.expansionSize <= 0)
                errors.Add("Expansion Size должен быть больше 0.");

            if (territory.expansionCost <= 0)
                errors.Add("Expansion Cost должен быть больше 0.");
        }

        return errors;
    }
}
