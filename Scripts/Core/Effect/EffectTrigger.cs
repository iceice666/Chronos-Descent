using Godot;
using ChronosDescent.Scripts.Core.Entity;

namespace ChronosDescent.Scripts.Core.Effect;

[GlobalClass]
public partial class EffectTrigger : Area2D
{
    public IEffect Effect { get; }
    public bool OneShot { get; set; }
    public bool TriggerOnEnter { get; set; } = true;
    public bool TriggerOnExit { get; set; }

    public override void _Ready()
    {
        base._Ready();

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node body)
    {
        if (!body.IsInGroup("Entity") || !TriggerOnEnter) return;
        ((IEntity)body).ApplyEffect(Effect);
    }

    private void OnBodyExited(Node body)
    {
        if (!body.IsInGroup("Entity") || !TriggerOnExit) return;
        ((IEntity)body).ApplyEffect(Effect);
    }
}