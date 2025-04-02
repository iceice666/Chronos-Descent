using Godot;

namespace ChronosDescent.Scripts.UI;

public partial class BoardcastTitle : Label
{
    private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        GlobalEventBus.Instance.Subscribe<string>(GlobalEventVariant.BoardcastTitle, Show);
    }

    public override void _ExitTree()
    {
        GlobalEventBus.Instance.Unsubscribe<string>(GlobalEventVariant.BoardcastTitle, Show);
    }

    public void Show(string text)
    {
        Show(text, 3);
    }

    public async void Show(string text, int duration)
    {
        Text = text;
        _animationPlayer.Play("fade");
        await ToSignal(GetTree().CreateTimer(duration), "timeout");
        _animationPlayer.PlayBackwards("fade");
    }
}