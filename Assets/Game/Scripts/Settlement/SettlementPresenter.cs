using UnityEngine;

public sealed class SettlementPresenter : MonoBehaviour
{
    [SerializeField] private GameObject tents;
    [SerializeField] private GameObject houses;
    [SerializeField] private GameObject fortifications;
    [SerializeField] private GameObject cityElements;
    [SerializeField] private GameObject castle;
    private SettlementService service;

    public void Initialize(SettlementService source)
    {
        if(service!=null)service.Changed-=OnChanged;
        service=source;
        if(service==null)return;
        service.Changed+=OnChanged;
        Present(service.Snapshot.Tier);
    }
    private void OnChanged(SettlementChanged change){if(change.TierChanged)Present(change.Snapshot.Tier);}
    private void OnDestroy(){if(service!=null)service.Changed-=OnChanged;}
    public void Present(SettlementTier tier)
    {
        Set(tents,true);
        Set(houses,tier>=SettlementTier.Village);
        Set(fortifications,tier>=SettlementTier.FortifiedVillage);
        Set(cityElements,tier>=SettlementTier.City);
        Set(castle,tier>=SettlementTier.Castle);
    }
    private static void Set(GameObject target,bool value){if(target!=null)target.SetActive(value);}
}
