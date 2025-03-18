using Godot;
using Effect = ChronosDescent.Scripts.resource.Effect;

namespace ChronosDescent.Scripts.node;

[GlobalClass]
public partial class EffectTrigger : Area2D
{
    [Export] public Effect Effect { get; set; }
    [Export] public bool OneShot { get; set; }
    [Export] public bool TriggerOnEnter { get; set; } = true;
    [Export] public bool TriggerOnExit { get; set; }
}