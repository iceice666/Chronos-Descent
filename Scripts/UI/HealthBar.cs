using ChronosDescent.Scripts.Core.Entity;
using Godot;

namespace ChronosDescent.Scripts.UI;

public partial class HealthBar : Control
{
    protected ProgressBar CurrentHealthBar;
    public float DamageDelay = 1.0f;
    protected Timer DamageDelayTimer;
    public float DelayedBarSpeed = 15.0f;
    protected ProgressBar DelayedHealthBar;
    protected double DelayedHealthValue;
    protected BaseEntity Entity;

    [Export] public int HideThreshold = 99;
    protected bool IsUpdatingDelayedBar;
    protected double TargetHealth;


    public override void _Ready()
    {
        Entity = GetOwner<BaseEntity>();

        CurrentHealthBar = GetNode<ProgressBar>("CurrentHealthBar");
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
        if (Mathf.IsEqualApprox((float)DelayedHealthValue, (float)TargetHealth)) IsUpdatingDelayedBar = false;
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