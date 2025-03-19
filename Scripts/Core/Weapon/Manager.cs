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
}

public partial class Manager : ISystem
{
    private IEntity _owner;
    public Node2D MountPoint;


    public void SetWeapon<T>(PackedScene scene) where T : Node2D, IWeapon
    {
        if (_owner?.AbilityManager == null) return;


        MountPoint.GetNodeOrNull("weapon")?.QueueFree();

        var newWeapon = scene.Instantiate<T>();
        newWeapon.Name = "weapon";
        MountPoint.AddChild(newWeapon);

        _owner.SetAbility(AbilitySlotType.Normal, newWeapon.NormalAttack);
        _owner.SetAbility(AbilitySlotType.Special, newWeapon.SpecialAttack);
        _owner.SetAbility(AbilitySlotType.Ultimate, newWeapon.Ultimate);
    }

    public void Initialize(IEntity owner)
    {
        _owner = owner;
        MountPoint = ((Node2D)_owner).GetNodeOrNull<Node2D>("WeaponMountPoint");
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