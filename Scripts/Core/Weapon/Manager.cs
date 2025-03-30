using ChronosDescent.Scripts.Core.Ability;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Entity;
using Godot;

namespace ChronosDescent.Scripts.Core.Weapon;

public partial class BaseProjectile : CharacterBody2D
{
    protected float DragFactor;

    public override void _PhysicsProcess(double delta)
    {
        Velocity *= DragFactor;
        if (Velocity.LengthSquared() < 10) QueueFree();
        MoveAndSlide();
    }
}

public interface IWeapon
{
    public BaseAbility NormalAttack { get; }
    public BaseAbility SpecialAttack { get; }
    public BaseAbility Ultimate { get; }
    
    public void SetOwner(BaseEntity owner);
}

public partial class Manager : ISystem
{
    private BaseEntity _owner;
    public Node2D MountPoint;

    public IWeapon CurrentWeapon;


    public void SetWeapon<T>(PackedScene scene) where T : Node2D, IWeapon
    {
        if (_owner?.AbilityManager == null) return;


        MountPoint.GetNodeOrNull("weapon")?.QueueFree();

        CurrentWeapon = scene.Instantiate<T>();
        ((Node2D)CurrentWeapon).Name = "weapon";
        CurrentWeapon.SetOwner(_owner);
        MountPoint.AddChild((Node2D)CurrentWeapon);

        _owner.SetAbility(AbilitySlotType.Normal, CurrentWeapon.NormalAttack);
        _owner.SetAbility(AbilitySlotType.Special, CurrentWeapon.SpecialAttack);
        _owner.SetAbility(AbilitySlotType.Ultimate, CurrentWeapon.Ultimate);
    }

    public void Initialize(BaseEntity owner)
    {
        _owner = owner;
        MountPoint = (_owner).GetNodeOrNull<Node2D>("WeaponMountPoint");
    }

    public void Update(double delta)
    {
        // nop
    }

    public void FixedUpdate(double delta)
    {
        // nop
    }
}