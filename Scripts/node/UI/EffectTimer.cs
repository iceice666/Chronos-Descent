using Godot;

namespace ChronosDescent.Scripts.node.UI;


public partial class EffectTimer : Control
{
    public Texture2D Icon;
    public int CurrentStack = 1;
    public int MaxStack;
    public double RemainingDurationPercentage = 1;
}