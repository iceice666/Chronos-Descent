using Godot;

namespace ChronosDescent.Scripts.Effect.Node;

[GlobalClass]
public partial class EffectTrigger : Area2D
{
    [Export] public Effect Effect { get; set; }
    [Export] public bool OneShot { get; set; }
    [Export] public bool TriggerOnEnter { get; set; } = true;
    [Export] public bool TriggerOnExit { get; set; }
}