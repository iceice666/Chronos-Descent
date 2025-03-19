using ChronosDescent.Scripts.Ability;
using Godot;

namespace ChronosDescent.Scripts.Weapon;

public partial class BaseWeapon : Node2D
{
    public delegate void HitEntityEventHandler(Entity.Entity entity);

    // Weapon properties
    public string Id { get; set; } = "Weapon";
    public string Description { get; set; } = "";
    public Texture2D Icon { get; set; }

    // The three ability slots every weapon contains
    public BaseAbility NormalAttack { get; protected set; } = new();
    public BaseAbility SpecialAttack { get; protected set; } = new();
    public BaseAbility UltimateAttack { get; protected set; } = new();

    // Stats that weapons might modify
    public float AttackDamage { get; set; }
    public float AttackSpeed { get; set; }
    public float Range { get; set; }
    public event HitEntityEventHandler HitEntity;

    protected void OnHitEntity(Entity.Entity entity)
    {
        HitEntity?.Invoke(entity);
    }


    public virtual void UpdateLookAnimation(Vector2 vec)
    {
    }
}