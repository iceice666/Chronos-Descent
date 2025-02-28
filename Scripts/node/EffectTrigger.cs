using ChronosDescent.Scripts.resource;
using Godot;

namespace ChronosDescent.Scripts.node;

[GlobalClass]
public partial class EffectTrigger : Area2D
{
    [Export] public Effect Effect { get; set; }
    [Export] public bool OneTimeUse { get; set; } = true;

    public override void _Ready()
    {
        AddToGroup("EffectTrigger");
        
        BodyEntered += OnBodyEntered;
    }

    // This method would be called when a character entity enters the trigger
    public void OnBodyEntered(Node2D body)
    {
        if (!body.IsInGroup("Entity")) return;

        ((Entity)body).ApplyEffect(Effect);

        if (OneTimeUse)
        {
            QueueFree(); // Remove the trigger after use
        }
    }
}