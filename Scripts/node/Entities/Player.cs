using ChronosDescent.Scripts.node.Component;
using ChronosDescent.Scripts.UI;
using Godot;

namespace ChronosDescent.Scripts.node.Entities;

public partial class Player : Entity
{
    public override void _Ready()
    {
        base._Ready();
        AddToGroup("Player");

        GetNode<Camera>("/root/Autoload/Camera").SwitchTarget(this);
        GetNode<EffectsContainer>("/root/Autoload/UI/EffectsContainer").Initialize(this);
        GetNode<AbilityContainer>("/root/Autoload/UI/AbilityContainer").Initialize(this);
        GetNode<PlayerHealthBar>("/root/Autoload/UI/PlayerHealthBar").Initialize(this);
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
        if (Input.IsActionJustPressed("cancel_ability"))
            CancelChargedAbility();
        else
            foreach (var slot in AbilityManagerComponent.GetAllSlots())
            {
                var slotName = slot.GetSlotName();
                if (Input.IsActionJustPressed(slotName))
                    ActivateAbility(slot);
                else if (Input.IsActionJustReleased(slotName))
                    ReleaseChargedAbility(slot);
            }
    }
}