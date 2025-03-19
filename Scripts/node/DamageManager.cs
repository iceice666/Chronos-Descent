using ChronosDescent.Scripts.Entity.Resource;
using Godot;

namespace ChronosDescent.Scripts.node;

[GlobalClass]
public partial class DamageManager : Node
{
    public static DamageManager Instance { get;private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public void DealDamage(BaseStats attackerStats, Entity.Entity attackee, double rawDamage)
    {
    }
}