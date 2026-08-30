using System.Collections.Generic;
using UnityEngine;

public sealed class AssaultRuntimeController : MonoBehaviour
{
    [SerializeField] private GameObject fighterPrefab;
    [SerializeField] private GameObject commanderPrefab;
    [SerializeField] private Transform formationPoint;
    [SerializeField] private Transform infectionReturnPoint;
    [SerializeField] private Transform[] orderedTargets;
    [SerializeField] private SunExposureCoordinator sunExposureCoordinator;
    private readonly List<AssaultUnitView> units=new();
    private GameRoot gameRoot;

    public void Initialize(GameRoot root)
    {
        ReleaseSubscriptions();gameRoot=root;
        if(gameRoot!=null)gameRoot.Assault.Started+=OnStarted;
    }
    private void OnStarted()
    {
        ClearViews();int index=0;
        foreach(ActorId actor in gameRoot.Assault.ReservedFighters)Spawn(fighterPrefab,actor,false,index++);
        Spawn(commanderPrefab,default,true,index);
        gameRoot.Assault.FormationReady();
    }
    private void Spawn(GameObject prefab,ActorId actor,bool commander,int index)
    {
        if(prefab==null||formationPoint==null)return;
        GameObject instance=Instantiate(prefab,formationPoint.position+Vector3.left*(index*.7f),Quaternion.identity,transform);
        AssaultUnitView view=instance.GetComponent<AssaultUnitView>()??instance.AddComponent<AssaultUnitView>();
        view.Initialize(this,actor,commander);sunExposureCoordinator?.Register(view);units.Add(view);
    }
    public Transform ResolveDestination()
    {
        if(gameRoot==null)return null;
        if(gameRoot.Assault.State==AssaultState.Returning)return infectionReturnPoint;
        if(orderedTargets!=null)foreach(Transform target in orderedTargets)
        {
            if(target==null)continue;DamageReceiver receiver=target.GetComponent<DamageReceiver>();
            if(receiver==null||receiver.Damageable==null||receiver.Damageable.IsAlive)return target;
        }
        return infectionReturnPoint;
    }
    public bool IsReturning=>gameRoot!=null&&gameRoot.Assault.State==AssaultState.Returning;
    public void NotifyReachedTarget(){if(gameRoot!=null)gameRoot.Assault.ReachedTarget();}
    public void NotifyReturned(AssaultUnitView view)
    {
        if(view.IsCommander){gameRoot.Assault.RegisterCommanderReturn();}
        else gameRoot.Assault.RegisterReturn(view.Actor);
        sunExposureCoordinator?.Unregister(view);units.Remove(view);Destroy(view.gameObject);
    }
    public void NotifyDied(AssaultUnitView view)
    {
        if(view.IsCommander)gameRoot.Assault.RegisterCommanderDeath();else gameRoot.Assault.RegisterFighterDeath(view.Actor);
        sunExposureCoordinator?.Unregister(view);units.Remove(view);Destroy(view.gameObject);
    }
    private void OnDestroy(){ReleaseSubscriptions();ClearViews();}
    private void ReleaseSubscriptions(){if(gameRoot!=null)gameRoot.Assault.Started-=OnStarted;gameRoot=null;}
    private void ClearViews(){for(int i=units.Count-1;i>=0;i--)if(units[i]!=null)Destroy(units[i].gameObject);units.Clear();}
}

[RequireComponent(typeof(PlayerHealth))]
public sealed class AssaultUnitView : MonoBehaviour, ISunExposureTarget
{
    private AssaultRuntimeController owner; private PlayerHealth health; private float cooldown;
    private SpriteRenderer visual;
    private Color baseColor=Color.white;
    public ActorId Actor { get; private set; }
    public bool IsCommander { get; private set; }
    public float WorldX=>transform.position.x;
    public IDamageable Damageable=>health;
    private void Awake()=>health=GetComponent<PlayerHealth>();
    public void Initialize(AssaultRuntimeController controller,ActorId actor,bool commander){owner=controller;Actor=actor;IsCommander=commander;health.Configure(commander?9000:actor.Value,commander?180:65);visual=GetComponentInChildren<SpriteRenderer>();if(visual!=null)baseColor=visual.color;}
    private void Update()
    {
        if(owner==null)return;if(!health.IsAlive){owner.NotifyDied(this);return;}
        Transform target=owner.ResolveDestination();if(target==null)return;float distance=Mathf.Abs(target.position.x-transform.position.x);
        if(owner.IsReturning&&distance<=.2f){owner.NotifyReturned(this);return;}
        DamageReceiver receiver=target.GetComponent<DamageReceiver>();
        if(receiver!=null&&receiver.Damageable!=null&&receiver.Damageable.IsAlive&&distance<=1.5f)
        {cooldown-=Time.deltaTime;if(cooldown<=0){receiver.Receive(new DamageInfo(IsCommander?20:12,DamageKind.Normal,transform.position));cooldown=IsCommander?1.5f:1.1f;}return;}
        transform.position=Vector3.MoveTowards(transform.position,new Vector3(target.position.x,transform.position.y,transform.position.z),AssaultService.SquadSpeed*Time.deltaTime);
        if(distance<=1.6f)owner.NotifyReachedTarget();
    }
    public void SetSunWarning(bool active)
    {
        if(visual!=null)visual.color=active?new Color(1f,.65f,.65f,1f):baseColor;
    }
}
