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
        AddToGroup("EffectTrigger");
    }
}