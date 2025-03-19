

using ChronosDescent.Scripts.Entities;
using Godot;

namespace ChronosDescent.Scripts.UI;


public partial class PlayerHealthBar : HealthBar
{
    public override void _Ready()
    {
    }

    public void Initialize(Player player)
    {
        Entity = player;
        
        CurrentHealthBar = GetNode<ProgressBar>("CurrentHealthBar");
        DelayedHealthBar = GetNode<ProgressBar>("DelayedHealthBar");
        DamageDelayTimer = GetNode<Timer>("DamageDelayTimer");
        
        DamageDelayTimer.WaitTime = DamageDelay;
        
        // Reset the delayed bar to match the initial health
        DelayedHealthValue = player.StatsManager.Health;
        DelayedHealthBar.Value = (float)player.StatsManager.Health;
        CurrentHealthBar.Value = (float)player.StatsManager.Health;
        
        player.EventBus.Subscribe(EventVariant.EntityStatChanged, UpdateHealth);
    }
}
