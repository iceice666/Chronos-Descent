using Godot;

namespace ChronosDescent.Scripts.UI;

[GlobalClass]
public partial class DungeonTimer : Label
{
    private double _elapsedTime;
    [Export] public bool AutoStart { get; set; } = true;

    public override void _Ready()
    {
        Text = "00:00";
        GlobalEventBus.Instance.Subscribe(GlobalEventVariant.RoomStarted, OnRoomStarted);
        GlobalEventBus.Instance.Subscribe(GlobalEventVariant.RoomCleared, OnRoomCleared);

        SetPhysicsProcess(false);
    }

    public override void _PhysicsProcess(double delta)
    {
        _elapsedTime += delta;
        UpdateTimerDisplay();
    }

    public override void _ExitTree()
    {
        GlobalEventBus.Instance.Unsubscribe(GlobalEventVariant.RoomStarted, OnRoomStarted);
        GlobalEventBus.Instance.Unsubscribe(GlobalEventVariant.RoomCleared, OnRoomCleared);
    }

    private void UpdateTimerDisplay()
    {
        var minutes = Mathf.FloorToInt((float)_elapsedTime / 60.0f);
        var seconds = Mathf.FloorToInt((float)_elapsedTime) % 60;
        Text = $"{minutes:00}:{seconds:00}";
    }

    private void OnRoomStarted()
    {
        SetPhysicsProcess(true);
    }

    private void OnRoomCleared()
    {
        SetPhysicsProcess(false);
    }


    public void ResetTimer()
    {
        _elapsedTime = 0.0;
        UpdateTimerDisplay();
    }

    // Gets the current time in seconds
    public double GetElapsedTime()
    {
        return _elapsedTime;
    }
}