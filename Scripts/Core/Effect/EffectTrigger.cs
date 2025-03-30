using ChronosDescent.Scripts.Core.Entity;
using Godot;

namespace ChronosDescent.Scripts.Core.Effect;

[GlobalClass]
public partial class EffectTrigger : Area2D
{
    [Export] public BaseEffect Effect { get; set; }
    [Export] public bool OneShot { get; set; }
    [Export] public bool TriggerOnEnter { get; set; } = true;
    [Export] public bool TriggerOnExit { get; set; }

    public override void _Ready()
    {
        BodyEntered += OnEnteredEffectTrigger;
        BodyExited += OnExitedEffectTrigger;
    }

    private void OnEnteredEffectTrigger(Node2D body)
    {
        if (body is not BaseEntity entity) return;
        if (TriggerOnEnter) entity.ApplyEffect(Effect);
    }

    private void OnExitedEffectTrigger(Node2D body)
    {
        if (body is not BaseEntity entity) return;
        if (TriggerOnExit) entity.ApplyEffect(Effect);
    }
}