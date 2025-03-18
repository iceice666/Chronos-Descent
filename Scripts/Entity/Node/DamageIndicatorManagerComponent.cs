using ChronosDescent.Scripts.Entity.UI;
using Godot;

namespace ChronosDescent.Scripts.Entity.Node;

[GlobalClass]
public partial class DamageIndicatorManagerComponent : Godot.Node
{
    private PackedScene _indicatorScene;

    public override void _Ready()
    {
        _indicatorScene = GD.Load<PackedScene>("res://Scenes/ui/damage_indicator.tscn");

        GD.Print("DamageIndicatorManager initialized");
    }


    // Spawn a damage indicator at the given position with the given amount and type
    public void ShowDamageIndicator(Vector2 position, double amount,
        DamageIndicator.DamageType type = DamageIndicator.DamageType.Normal)
    {
        var indicator = _indicatorScene.Instantiate<DamageIndicator>();
        GetTree().Root.AddChild(indicator);

        // Set global position to match the entity's position (slightly above it)
        indicator.GlobalPosition = position + new Vector2(0, -50);

        // Set up the indicator
        indicator.Setup(amount, type);
    }
}