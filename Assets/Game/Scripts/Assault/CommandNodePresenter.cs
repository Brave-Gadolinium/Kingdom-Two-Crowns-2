using TMPro;
using UnityEngine;

public sealed class CommandNodePresenter : MonoBehaviour
{
    [SerializeField] private TMP_Text priceLabel;
    [SerializeField] private TMP_Text compositionLabel;
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private SpriteRenderer routeIndicator;
    public void Present(RouteSafety safety,AssaultStartFailure failure=AssaultStartFailure.None)
    {
        if(priceLabel!=null)priceLabel.text="Цена: 6";
        if(compositionLabel!=null)compositionLabel.text="Командир + 4 Бойца · минимум 75 с";
        if(statusLabel!=null)statusLabel.text=FailureText(failure);
        if(routeIndicator!=null)routeIndicator.color=safety==RouteSafety.Safe?Color.green:safety==RouteSafety.Risky?Color.yellow:Color.red;
    }
    private static string FailureText(AssaultStartFailure failure)
    {
        return failure switch
        {
            AssaultStartFailure.NotNight=>"Наступление доступно только Ночью",
            AssaultStartFailure.NotEnoughTime=>"Недостаточно времени до Рассвета",
            AssaultStartFailure.NotEnoughFighters=>"Нужно 4 свободных Бойца",
            AssaultStartFailure.AlreadyActive=>"Отряд уже в походе",
            AssaultStartFailure.NotEnoughGreed=>"Недостаточно Жадности",
            _=>string.Empty
        };
    }
}
