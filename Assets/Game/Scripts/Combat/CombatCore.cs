using System;
using System.Collections.Generic;
using UnityEngine;

public readonly struct AttackId : IEquatable<AttackId>
{
    public long Value { get; }
    public AttackId(long value){Value=value;}
    public bool Equals(AttackId other)=>Value==other.Value;
    public override bool Equals(object obj)=>obj is AttackId other&&Equals(other);
    public override int GetHashCode()=>Value.GetHashCode();
}

public sealed class AttackHitRegistry
{
    private long nextAttack=1;
    private readonly Dictionary<AttackId,HashSet<int>> hits=new();
    public AttackId Begin(){var id=new AttackId(nextAttack++);hits.Add(id,new HashSet<int>());return id;}
    public bool TryRegister(AttackId attack,ActorId target)
    { return hits.TryGetValue(attack,out HashSet<int> targets)&&targets.Add(target.Value); }
    public void End(AttackId attack)=>hits.Remove(attack);
}

public static class FactionRules
{
    public static bool CanDamage(Faction source,Faction target)=>source!=Faction.Neutral&&target!=Faction.Neutral&&source!=target;
}

public sealed class MeleeAttackService
{
    private readonly AttackHitRegistry registry;
    public MeleeAttackService(AttackHitRegistry hitRegistry){registry=hitRegistry;}
    public bool TryHit(AttackId attack,Faction sourceFaction,Faction targetFaction,IDamageable target,int damage,Vector2 point)
    {
        if(target==null||!target.IsAlive||damage<=0||!FactionRules.CanDamage(sourceFaction,targetFaction))return false;
        if(!registry.TryRegister(attack,target.Id))return false;
        return target.ApplyDamage(new DamageInfo(damage,DamageKind.Normal,point));
    }
}

public sealed class HitReactionGate
{
    private float remaining;
    public bool IsReacting=>remaining>0;
    public bool TryStart(float duration){if(IsReacting)return false;remaining=Math.Max(0,duration);return true;}
    public void Tick(float deltaTime){remaining=Math.Max(0,remaining-Math.Max(0,deltaTime));}
}

public sealed class ProjectilePool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField,Min(1)] private int initialSize=12;
    private readonly Queue<GameObject> available=new();
    private void Awake(){for(int i=0;i<initialSize;i++)Create();}
    public GameObject Rent(){if(available.Count==0)Create();var item=available.Dequeue();item.SetActive(true);return item;}
    public void Return(GameObject item){if(item==null)return;item.SetActive(false);available.Enqueue(item);}
    private void Create(){if(prefab==null)return;var item=Instantiate(prefab,transform);item.SetActive(false);available.Enqueue(item);}
}
