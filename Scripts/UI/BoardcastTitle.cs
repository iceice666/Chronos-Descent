using Godot;

namespace ChronosDescent.Scripts.UI;

public partial class BoardcastTitle : Label
{
    private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        GlobalEventBus.Instance.Subscribe<string>(GlobalEventVariant.BoardcastTitle, text => Show(text));
    }

    public override void _ExitTree()
    {
        GlobalEventBus.Instance.Unsubscribe<string>(GlobalEventVariant.BoardcastTitle, text => Show(text));
    }

    public async void Show(string text, int duration = 3)
    {
        Text = text;
        _animationPlayer.Play("fade");
        await ToSignal(GetTree().CreateTimer(duration), "timeout");
        _animationPlayer.PlayBackwards("fade");
    }
}