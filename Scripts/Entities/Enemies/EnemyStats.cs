using ChronosDescent.Scripts.Core.State;

namespace ChronosDescent.Scripts.Entities.Enemies;

public class EnemyBaseStats : EntityBaseStats
{
    public override double Health { get; } = 80;
    public override double Defense { get; } = 5;
    public override double CriticalChance { get; } = 3;
    public override double CriticalDamage { get; } = 30;
    public override double MoveSpeed { get; } = 80;
    public override double AttackSpeed { get; } = 0.8;

    // Additional enemy-specific stats
    public virtual double AttackDamage { get; } = 10;
    public virtual double DetectionRadius { get; } = 200;
    public virtual double AttackRadius { get; } = 40;
}

public class MeleeEnemyStats : EnemyBaseStats
{
    public override double Health { get; } = 120;
    public override double Defense { get; } = 8;
    public override double MoveSpeed { get; } = 50;
    public override double AttackDamage { get; } = 15;
    public override double AttackRadius { get; } = 50;
}

public class RangedEnemyStats : EnemyBaseStats
{
    public override double Health { get; } = 60;
    public override double Defense { get; } = 3;
    public override double MoveSpeed { get; } = 40;
    public override double AttackDamage { get; } = 12;
    public override double DetectionRadius { get; } = 300;
    public override double AttackRadius { get; } = 180;
}

public class HealerEnemyStats : EnemyBaseStats
{
    public override double Health { get; } = 70;
    public override double Defense { get; } = 2;
    public override double MoveSpeed { get; } = 85;
    public override double AttackDamage { get; } = 5;
    public double HealAmount { get; } = 10;
    public double HealRadius { get; } = 150;
    public double HealCooldown { get; } = 5;
}

public class BufferEnemyStats : EnemyBaseStats
{
    public override double Health { get; } = 65;
    public override double Defense { get; } = 4;
    public override double MoveSpeed { get; } = 95;
    public override double AttackDamage { get; } = 8;
    public double BuffAmount { get; } = 1.3; // Multiplier
    public double BuffRadius { get; } = 120;
    public double BuffDuration { get; } = 10;
    public double BuffCooldown { get; } = 15;
}