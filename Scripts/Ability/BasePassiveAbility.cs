using Godot;

namespace ChronosDescent.Scripts.Ability;

/// <summary>
///     Passive ability that is always active and doesn't require activation.
/// </summary>
[GlobalClass]
public partial class BasePassiveAbility : BaseAbility
{
    /// <summary>
    ///     Passive abilities cannot be manually activated
    /// </summary>
    public override bool CanActivate()
    {
        return false;
    }

    /// <summary>
    ///     Passive abilities are always active and don't need manual activation
    /// </summary>
    public override void Activate()
    {
        // Passive abilities can't be activated manually
    }

    public override void Update(double delta)
    {
        // Continuously update passive effect
        OnPassiveTick(delta);
    }

    /// <summary>
    ///     Called every frame to update the passive effect
    /// </summary>
    protected virtual void OnPassiveTick(double delta)
    {
        // Override in derived classes to provide passive effects
    }
}