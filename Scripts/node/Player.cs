using ChronosDescent.Scripts.node.Component;
using ChronosDescent.Scripts.resource.Abilities;
using ChronosDescent.Scripts.UI;
using Godot;

namespace ChronosDescent.Scripts.node;

public partial class Player : Entity
{
    public override void _Ready()
    {
        base._Ready();
        AddToGroup("Player");

        GetNode<Camera>("/root/Autoload/Camera").SwitchTarget(this);
        GetNode<EffectsContainer>("/root/Autoload/UI/EffectsContainer").Initialize(this);
        GetNode<AbilityContainer>("/root/Autoload/UI/AbilityContainer").Initialize(this);
        GetNode<AbilityIndicator>("/root/Autoload/UI/AbilityIndicator").Initialize(this);


        AbilityManager.SetAbility(AbilityManagerComponent.Slot.Primary, new TimeRewindAbility());
        AbilityManager.SetAbility(AbilityManagerComponent.Slot.Secondary, new DashAbility());
    }

    public override void _PhysicsProcess(double delta)
    {
        // Handle movements
        if (Moveable)
        {
            var direction = UserInputManager.Instance.MovementInput;
            var velocity = direction * (float)Stats.MoveSpeed;

            Velocity = velocity;
            Animation!.UpdateWalkAnimation(velocity);

            MoveAndSlide();
        }
    }

    public override void _Process(double delta)
    {
        // Handle Animation
        AimDirection = UserInputManager.Instance.AimInput;
        Animation!.UpdateLookAnimation(AimDirection);

        // Handle Ability inputs
        if (Input.IsActionJustPressed("normal_attack")) ActivateAbility(AbilityManagerComponent.Slot.NormalAttack);
        else if (Input.IsActionJustReleased("normal_attack"))
            ReleaseChargedAbility(AbilityManagerComponent.Slot.NormalAttack);

        if (Input.IsActionJustPressed("ability_1")) ActivateAbility(AbilityManagerComponent.Slot.Primary);
        else if (Input.IsActionJustReleased("ability_1")) ReleaseChargedAbility(AbilityManagerComponent.Slot.Primary);

        if (Input.IsActionJustPressed("ability_2")) ActivateAbility(AbilityManagerComponent.Slot.Secondary);
        else if (Input.IsActionJustReleased("ability_2")) ReleaseChargedAbility(AbilityManagerComponent.Slot.Secondary);

        if (Input.IsActionJustPressed("weapon_ult")) ActivateAbility(AbilityManagerComponent.Slot.WeaponUlt);
        else if (Input.IsActionJustReleased("weapon_ult"))
            ReleaseChargedAbility(AbilityManagerComponent.Slot.WeaponUlt);

        if (Input.IsActionJustPressed("cancel_ability")) CancelChargedAbility();
    }
}