using ChronosDescent.Scripts.Core.Entity;
using Godot;

namespace ChronosDescent.Scripts.Core.Effect;

[GlobalClass]
public partial class EffectBox : Area2D
{
    private BaseEntity _owner;

    public override void _Ready()
    {
        _owner = GetOwner<BaseEntity>();

        AreaEntered += OnEnteredEffectTrigger;
        AreaExited += OnExitedEffectTrigger;
    }

    private void OnEnteredEffectTrigger(Area2D area)
    {
        if (!area.IsInGroup("EffectTrigger")) return;
        var trigger = (EffectTrigger)area;
        if (trigger.TriggerOnEnter)
        {
            _owner.ApplyEffect(trigger.Effect);
        }
    }

    private void OnExitedEffectTrigger(Area2D area)
    {
        if (!area.IsInGroup("EffectTrigger")) return;
        var trigger = (EffectTrigger)area;
        if (trigger.TriggerOnExit)
        {
            _owner.ApplyEffect(trigger.Effect);
        }
    }
}
