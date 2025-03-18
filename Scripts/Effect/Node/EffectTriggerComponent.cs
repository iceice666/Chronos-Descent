using Godot;

namespace ChronosDescent.Scripts.Effect.Node;

[GlobalClass]
public partial class EffectTriggerComponent : Area2D
{
    private Entity.Entity _entity;

    public override void _Ready()
    {
        _entity = GetParent<Entity.Entity>();
        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
    }

    public override void _ExitTree()
    {
        AreaEntered -= OnAreaEntered;
        AreaExited -= OnAreaExited;
    }

    // This method would be called when an entity enters the trigger
    private void OnAreaEntered(Node2D body)
    {
        if (body is EffectTrigger { TriggerOnEnter: true } trigger)
            TriggerEffect(trigger);
    }

    private void OnAreaExited(Node2D body)
    {
        if (body is EffectTrigger { TriggerOnExit: true } trigger)
            TriggerEffect(trigger);
    }

    private void TriggerEffect(EffectTrigger trigger)
    {
        _entity.ApplyEffect(trigger.Effect);
        if (trigger.OneShot) trigger.QueueFree();
    }
}