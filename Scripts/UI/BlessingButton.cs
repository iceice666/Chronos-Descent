using ChronosDescent.Scripts.Core.Blessing;
using Godot;

namespace ChronosDescent.Scripts.UI;

/// <summary>
///     Button for displaying and selecting a blessing in the reward room
///     NOTE: This is kept for backward compatibility. New implementations should use BlessingItem.
/// </summary>
[GlobalClass]
public partial class BlessingButton : Button
{
    // Event for blessing selection
    [Signal]
    public delegate void BlessingSelectedEventHandler(Blessing blessing);

    // Current blessing
    private Blessing _blessing;
    private Label _categoryLabel;
    private Label _deityLabel;
    private Label _descriptionLabel;
    private TextureRect _iconTexture;

    // UI elements
    private Label _titleLabel;

    // Paths to child elements
    [Export] public NodePath TitleLabelPath { get; set; } = "MarginContainer/VBoxContainer/TitleLabel";
    [Export] public NodePath DescriptionLabelPath { get; set; } = "MarginContainer/VBoxContainer/DescriptionLabel";
    [Export] public NodePath IconTexturePath { get; set; } = "MarginContainer/VBoxContainer/HBoxContainer/IconTexture";

    [Export]
    public NodePath CategoryLabelPath { get; set; } = "MarginContainer/VBoxContainer/HBoxContainer/CategoryLabel";

    [Export] public NodePath DeityLabelPath { get; set; } = "MarginContainer/VBoxContainer/DeityLabel";

    public override void _Ready()
    {
        // Get UI elements
        _titleLabel = GetNode<Label>(TitleLabelPath);
        _descriptionLabel = GetNode<Label>(DescriptionLabelPath);
        _iconTexture = GetNode<TextureRect>(IconTexturePath);
        _categoryLabel = GetNode<Label>(CategoryLabelPath);
        _deityLabel = GetNode<Label>(DeityLabelPath);

        // Connect button press signal
        Pressed += OnButtonPressed;
    }

    /// <summary>
    ///     Configure the button with a blessing
    /// </summary>
    public void SetBlessing(Blessing blessing)
    {
        _blessing = blessing;

        // Update UI
        UpdateUI();
    }

    /// <summary>
    ///     Update UI elements with blessing information
    /// </summary>
    private void UpdateUI()
    {
        if (_blessing == null) return;

        // Set title with rarity color
        _titleLabel.Text = _blessing.Title;
        _titleLabel.Modulate = _blessing.GetRarityColor();

        // Set description
        _descriptionLabel.Text = _blessing.Description;

        // Set icon if available
        if (_blessing.Icon != null)
            _iconTexture.Texture = _blessing.Icon;
        else
            // Use a default icon based on category
            switch (_blessing.Category)
            {
                case BlessingCategory.Offensive:
                    _iconTexture.Modulate = new Color(1.0f, 0.3f, 0.3f); // Red
                    break;
                case BlessingCategory.Defensive:
                    _iconTexture.Modulate = new Color(0.3f, 0.7f, 1.0f); // Blue
                    break;
                case BlessingCategory.Utility:
                    _iconTexture.Modulate = new Color(1.0f, 0.8f, 0.2f); // Yellow
                    break;
                case BlessingCategory.Movement:
                    _iconTexture.Modulate = new Color(0.2f, 0.9f, 0.5f); // Green
                    break;
            }

        // Set category
        _categoryLabel.Text = _blessing.Category.ToString();

        // Set deity
        _deityLabel.Text = "— " + Blessing.GetDeityName(_blessing.Deity);

        // Set tooltip
        TooltipText = $"{_blessing.Title} ({_blessing.Rarity})\n{_blessing.Description}";
    }

    /// <summary>
    ///     Handle button press
    /// </summary>
    private void OnButtonPressed()
    {
        if (_blessing == null) return;

        // Emit blessing selected signal
        EmitSignal(SignalName.BlessingSelected, _blessing);

        // Also publish global event for compatibility with new system
        GlobalEventBus.Instance.Publish(GlobalEventVariant.BlessingSelected, _blessing);
    }
}