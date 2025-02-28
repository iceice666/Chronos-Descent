using ChronosDescent.Scripts.resource;
using Godot;

namespace ChronosDescent.Scripts.node;

[GlobalClass]
public partial class EffectTrigger : Area2D
{
    [Export] public Effect Effect { get; set; }
    [Export] public bool OneShot { get; set; } 
    [Export] public bool TriggerOnEnter { get; set; } = true;
    [Export] public bool TriggerOnExit { get; set; }

    public override void _Ready()
    {
        AddToGroup("EffectTrigger");

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void ApplyEffect(Node2D body)
    {
        if (!body.IsInGroup("Entity")) return;

        ((Entity)body).ApplyEffect(Effect);

        if (OneShot)
        {
            QueueFree(); // Remove the trigger after use
        }
    }

    // This method would be called when a character entity enters the trigger
    private void OnBodyEntered(Node2D body)
    {
        if (TriggerOnEnter) ApplyEffect(body);
    }

    private void OnBodyExited(Node2D body)
    {
        if (TriggerOnExit) ApplyEffect(body);
    }
}