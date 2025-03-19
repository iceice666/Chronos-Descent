using ChronosDescent.Scripts.Core.Entity;
using Godot;

namespace ChronosDescent.Scripts.UI;

public partial class HealthBar : Control
{
    public float DamageDelay = 1.0f;
    public float DelayedBarSpeed = 15.0f;

    protected ProgressBar CurrentHealthBar;
    protected ProgressBar DelayedHealthBar;
    protected Timer DamageDelayTimer;
    protected IEntity Entity;
    protected double TargetHealth;
    protected double DelayedHealthValue;
    protected bool IsUpdatingDelayedBar;

    [Export] public int HideThreshold = 99;


    public override void _Ready()
    {
        GD.Print("Ready");
        Entity = GetOwner<IEntity>();

        CurrentHealthBar = GetNode<ProgressBar>("HealthBar");
        DelayedHealthBar = GetNode<ProgressBar>("DelayedHealthBar");
        DamageDelayTimer = GetNode<Timer>("DamageDelayTimer");

        DamageDelayTimer.WaitTime = DamageDelay;
        DamageDelayTimer.OneShot = true;

        UpdateHealth();

        Entity.EventBus.Subscribe(EventVariant.EntityStatChanged, UpdateHealth);
    }

    public override void _ExitTree()
    {
        Entity.EventBus.Unsubscribe(EventVariant.EntityStatChanged, UpdateHealth);
    }

    public override void _Process(double delta)
    {
        if (!IsUpdatingDelayedBar) return;

        // Smoothly decrease the delayed health bar to match current health
        DelayedHealthValue = Mathf.MoveToward(
            DelayedHealthValue,
            TargetHealth,
            DelayedBarSpeed * delta
        );

        DelayedHealthBar.Value = (float)DelayedHealthValue;

        // Stop updating when we reach the target
        if (Mathf.IsEqualApprox((float)DelayedHealthValue, (float)TargetHealth))
        {
            IsUpdatingDelayedBar = false;
        }
    }


    public void UpdateHealth()
    {
        var currentHealth = Entity.StatsManager.Health;
        var maxHealth = Entity.StatsManager.MaxHealth;

        if (100 * currentHealth / maxHealth > HideThreshold)
        {
            if (Visible) Visible = false;
            return;
        }

        if (!Visible) Visible = true;

        // Update the max value of both bars
        CurrentHealthBar.MaxValue = (float)maxHealth;
        DelayedHealthBar.MaxValue = (float)maxHealth;

        // Update the current health bar immediately
        CurrentHealthBar.Value = (float)currentHealth;

        // If health decreased
        if (currentHealth < DelayedHealthValue)
        {
            // Set target health for smooth animation
            TargetHealth = currentHealth;

            // Reset and start the delay timer
            DamageDelayTimer.Stop();
            DamageDelayTimer.Start();
        }
        // If health increased (healing)
        else if (currentHealth > DelayedHealthValue)
        {
            // Update both bars immediately
            DelayedHealthValue = currentHealth;
            DelayedHealthBar.Value = (float)currentHealth;
        }
    }

    private void OnDamageDelayTimerTimeout()
    {
        IsUpdatingDelayedBar = true;
    }
}