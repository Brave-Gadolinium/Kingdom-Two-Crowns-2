using UnityEngine;

public enum Faction { Greed, Human, Neutral }

public sealed class FactionMember : MonoBehaviour
{
    [SerializeField] private Faction faction = Faction.Greed;
    public Faction Faction => faction;
}
