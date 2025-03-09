using Godot;

namespace ChronosDescent.Scripts.node.UI;

public partial class EffectIndicator : Control
{
    // Properties
    public int CurrentStack { get; set; } = 1;
    public int MaxStack { get; set; } = 1;
    public double RemainingDuration { get; set; }

    // UI nodes
    private TextureRect _iconNode;
    private RichTextLabel _durationLabel;

    // Set icon with proper scaling
    public Texture2D Icon
    {
        set => UpdateIcon(value);
    }

    public override void _Ready()
    {
        _iconNode = GetNode<TextureRect>("Icon");
        _durationLabel = GetNode<RichTextLabel>("DurationLabel");

        // Initialize
        UpdateDurationDisplay();
    }

    public override void _Process(double delta)
    {
        // Update the duration display each frame
        UpdateDurationDisplay();
    }

    // Update the icon and scale it properly
    private void UpdateIcon(Texture2D icon)
    {
        if (icon == null) return;

        var textureSize = icon.GetSize();
        var maxSize = GetParent<Control>().Size.Y;
        var scale = maxSize / textureSize.Y;
        var scaleVec = new Vector2(scale, scale);

        _iconNode.Texture = icon;
        _iconNode.Scale = scaleVec;
    }

    // Update the duration text
    private void UpdateDurationDisplay()
    {
        if (_durationLabel == null) return;

        // Format the duration text (showing only one decimal place)
        var formattedDuration = RemainingDuration.ToString("F1");

        // Update the text with duration and stack count if applicable
        _durationLabel.Text = MaxStack > 1
            ? $"{CurrentStack}/{MaxStack} {formattedDuration}s"
            : $"{formattedDuration}s";
    }

    // Called when duration is updated
    public void SetDuration(double duration)
    {
        RemainingDuration = duration;
        UpdateDurationDisplay();
    }

    // Called when stack count is updated
    public void SetStack(int currentStack)
    {
        CurrentStack = currentStack;
        UpdateDurationDisplay();
    }
}