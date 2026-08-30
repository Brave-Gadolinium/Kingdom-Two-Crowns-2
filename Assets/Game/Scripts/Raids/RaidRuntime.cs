using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public sealed class RaidUnitView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private Transform weapon;
    private RaidRuntimeController owner;
    private PlayerHealth health;
    private float speed, attackInterval, attackRange, attackCooldown;
    private int damage;
    public RaidParticipant Participant { get; private set; }
    private void Awake() => health = GetComponent<PlayerHealth>();
    public void Bind(RaidParticipant participant, RaidRuntimeController controller, Vector3 spawnPosition)
    { Participant=participant;owner=controller;transform.position=spawnPosition;ConfigureStats(participant.Type);attackCooldown=0;gameObject.SetActive(true); }
    public void Release(){Participant=null;owner=null;gameObject.SetActive(false);}
    private void Update()
    {
        if(Participant==null||owner==null)return;
        if(!health.IsAlive){owner.NotifyRemoved(Participant.RuntimeId);return;}
        Transform target=owner.ResolveTarget(Participant.Target);if(target==null)return;
        float distance=Mathf.Abs(target.position.x-transform.position.x);
        if(Participant.Target==RaidTargetKind.Retreat&&distance<=.2f){owner.NotifyReturned(Participant.RuntimeId);return;}
        DamageReceiver receiver=target.GetComponent<DamageReceiver>();
        if(receiver!=null&&receiver.Damageable!=null&&receiver.Damageable.IsAlive&&distance<=attackRange)
        { attackCooldown-=Time.deltaTime;if(attackCooldown<=0){receiver.Receive(new DamageInfo(damage,DamageKind.Normal,transform.position));attackCooldown=attackInterval;owner.RefreshTargets();}return; }
        transform.position=Vector3.MoveTowards(transform.position,new Vector3(target.position.x,transform.position.y,transform.position.z),speed*Time.deltaTime);
    }
    private void ConfigureStats(HumanUnitType type)
    {
        switch(type)
        {
            case HumanUnitType.Archer:speed=3.2f;damage=9;attackInterval=1.6f;attackRange=6;break;
            case HumanUnitType.Soldier:speed=3;damage=13;attackInterval=1.2f;attackRange=1.4f;break;
            case HumanUnitType.Knight:speed=2.6f;damage=24;attackInterval=1.5f;attackRange=1.5f;break;
            case HumanUnitType.Worker:speed=3.4f;damage=6;attackInterval=1.5f;attackRange=1.2f;break;
            default:speed=3.6f;damage=7;attackInterval=1.3f;attackRange=1.2f;break;
        }
        int hitPoints=type==HumanUnitType.Knight?150:type==HumanUnitType.Soldier?75:type==HumanUnitType.Worker?45:type==HumanUnitType.Archer?40:35;
        health.Configure(10000+Participant.RuntimeId,hitPoints);
        if(visual!=null)visual.color=type==HumanUnitType.Knight?new Color(.2f,.25f,.5f):type==HumanUnitType.Soldier?new Color(.25f,.4f,.65f):type==HumanUnitType.Archer?new Color(.3f,.65f,.35f):new Color(.55f,.4f,.25f);
        if(weapon!=null)weapon.localScale=type==HumanUnitType.Archer?new Vector3(.15f,1.1f,1):type==HumanUnitType.Knight?new Vector3(1.2f,.25f,1):new Vector3(.8f,.15f,1);
    }
}

public sealed class RaidTargetPoint : MonoBehaviour
{
    [SerializeField] private RaidTargetKind kind;
    [SerializeField] private int priority;
    public RaidTargetKind Kind=>kind;
    public int Priority=>priority;
    public bool IsAlive { get { var receiver=GetComponent<DamageReceiver>();return receiver==null||receiver.Damageable==null||receiver.Damageable.IsAlive; } }
}

public sealed class RaidUnitPoolBehaviour : MonoBehaviour, IRaidUnitPool
{
    [SerializeField] private RaidUnitView prefab;
    [SerializeField,Min(1)] private int prewarm=20;
    private readonly Queue<RaidUnitView> available=new();
    private readonly Dictionary<int,RaidUnitView> active=new();
    private RaidRuntimeController owner; private Transform spawnPoint; private int nextRuntimeId=1;
    public void Initialize(RaidRuntimeController controller,Transform spawn){owner=controller;spawnPoint=spawn;if(available.Count==0)for(int i=0;i<prewarm;i++)Create();}
    public RaidParticipant Rent(HumanUnitType type)
    {
        if(available.Count==0)Create();if(available.Count==0)throw new InvalidOperationException("RaidUnitPoolBehaviour: prefab не назначен.");
        RaidUnitView view=available.Dequeue();var participant=new RaidParticipant(nextRuntimeId++,type);view.Bind(participant,owner,spawnPoint!=null?spawnPoint.position:transform.position);active.Add(participant.RuntimeId,view);return participant;
    }
    public void Return(RaidParticipant participant){if(participant==null||!active.Remove(participant.RuntimeId,out RaidUnitView view))return;view.Release();available.Enqueue(view);}
    private void Create(){if(prefab==null)return;RaidUnitView view=Instantiate(prefab,transform);view.Release();available.Enqueue(view);}
}

public sealed class RaidRuntimeController : MonoBehaviour
{
    [SerializeField] private RaidUnitPoolBehaviour pool;
    [SerializeField] private Transform humanSpawnPoint;
    [SerializeField] private Transform humanReturnPoint;
    [SerializeField] private RaidTargetPoint[] targets;
    [SerializeField] private int deterministicSeed=1701;
    private GameRoot gameRoot; private RaidService service;
    public RaidService Service=>service;
    public void Initialize(GameRoot root){Release();gameRoot=root;if(gameRoot==null||pool==null)return;pool.Initialize(this,humanSpawnPoint);service=new RaidService(pool);gameRoot.Phase.PhaseChanged+=OnPhaseChanged;}
    private void Update()=>service?.Tick(Time.deltaTime);
    private void OnPhaseChanged(PhaseChanged change){if(change.Current==DayPhase.Day)service.Start(gameRoot.RaidPlanner.CreatePlan(gameRoot.Settlement.Snapshot,change.Cycle,deterministicSeed));else if(change.Current==DayPhase.Dusk)service.BeginRetreat();}
    public Transform ResolveTarget(RaidTargetKind requested)
    { if(requested==RaidTargetKind.Retreat)return humanReturnPoint!=null?humanReturnPoint:humanSpawnPoint;RaidTargetPoint best=FindBestTarget();return best!=null?best.transform:null; }
    public void RefreshTargets(){if(service==null)return;RaidTargetPoint best=FindBestTarget();if(best!=null)foreach(var unit in service.ActiveParticipants)unit.Target=best.Kind;}
    private RaidTargetPoint FindBestTarget(){RaidTargetPoint best=null;if(targets!=null)foreach(var target in targets)if(target!=null&&target.IsAlive&&(best==null||target.Priority<best.Priority))best=target;return best;}
    public void NotifyRemoved(int runtimeId)=>service?.RemoveParticipant(runtimeId);
    public void NotifyReturned(int runtimeId)=>service?.RemoveParticipant(runtimeId);
    private void OnDestroy()=>Release();
    private void Release(){if(gameRoot!=null&&gameRoot.Phase!=null)gameRoot.Phase.PhaseChanged-=OnPhaseChanged;gameRoot=null;service=null;}
}
