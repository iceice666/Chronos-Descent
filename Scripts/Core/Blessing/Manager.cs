using System.Collections.Generic;
using System.Linq;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Core.State;
using Godot;

namespace ChronosDescent.Scripts.Core.Blessing;

/// <summary>
///     Manages all blessings for an entity (typically the player)
/// </summary>
public class Manager : ISystem
{
    private readonly Dictionary<string, Blessing> _blessings = new();
    private BaseEntity _owner;
    private bool _statsAreDirty;

    // Initialization
    public void Initialize(BaseEntity owner)
    {
        _owner = owner;

        // Subscribe to events from the global event bus
        GlobalEventBus.Instance.Subscribe<(double, BaseEntity, EntityStats, Vector2)>(
            GlobalEventVariant.DamageDealt, OnDamageDealt);
    }

    // Update methods
    public void Update(double delta)
    {
        // Update all blessings
        foreach (var blessing in _blessings.Values) blessing.OnTick(delta);
    }

    public void FixedUpdate(double delta)
    {
        // Recalculate stats if needed
        if (_statsAreDirty) RecalculateStats();
    }

    // Blessing management
    public void AddBlessing(Blessing blessing)
    {
        if (blessing == null) return;

        var blessingId = blessing.Id;

        // Check if we already have this blessing
        if (_blessings.TryGetValue(blessingId, out var existingBlessing))
        {
            // If stackable and not at max level, level it up
            if (!existingBlessing.IsStackable || existingBlessing.CurrentLevel >= existingBlessing.MaxLevel) return;
            existingBlessing.CurrentLevel++;
            existingBlessing.OnLevelUp();

            // Mark stats as dirty if this blessing modifies stats
            if (existingBlessing.HasStatModifiers) _statsAreDirty = true;

            // Notify UI
            GlobalEventBus.Instance.Publish(GlobalEventVariant.BlessingUpgraded, existingBlessing);
        }
        else
        {
            // Add new blessing
            blessing.Owner = _owner;
            _blessings[blessingId] = blessing;

            // Apply the blessing
            blessing.OnApply();

            // Mark stats as dirty if this blessing modifies stats
            if (blessing.HasStatModifiers) _statsAreDirty = true;

            // Notify UI
            GlobalEventBus.Instance.Publish(GlobalEventVariant.BlessingAdded, blessing);
        }
    }

    public void RemoveBlessing(string blessingId)
    {
        if (!_blessings.TryGetValue(blessingId, out var blessing))
            return;

        // Run on-remove callback
        blessing.OnRemove();

        // Remove from active blessings
        _blessings.Remove(blessingId);

        // Mark stats as dirty if this was a stat modifier blessing
        if (blessing.HasStatModifiers) _statsAreDirty = true;

        // Notify UI
        GlobalEventBus.Instance.Publish(GlobalEventVariant.BlessingRemoved, blessingId);
    }

    public bool HasBlessing(string blessingId)
    {
        return _blessings.ContainsKey(blessingId);
    }

    public Blessing GetBlessing(string blessingId)
    {
        return _blessings.GetValueOrDefault(blessingId);
    }

    public IEnumerable<Blessing> GetAllBlessings()
    {
        return _blessings.Values;
    }

    public IEnumerable<Blessing> GetBlessingsByDeity(TimeDeity deity)
    {
        return _blessings.Values.Where(b => b.Deity == deity);
    }

    public int GetDeityFavor(TimeDeity deity)
    {
        return _blessings.Values.Count(b => b.Deity == deity);
    }

    // Stats recalculation
    private void RecalculateStats()
    {
        var additiveTotal = new Dictionary<StatFieldSpecifier, double>();
        var multiplicativeTotal = new Dictionary<StatFieldSpecifier, double>();

        // Accumulate modifiers from all blessings
        foreach (var blessing in _blessings.Values)
        {
            if (!blessing.HasStatModifiers) continue;

            // Process additive modifiers
            foreach (var (stat, value) in blessing.AdditiveModifiers)
            {
                additiveTotal.TryAdd(stat, 0);

                // Apply level scaling for stackable blessings
                additiveTotal[stat] += value * blessing.CurrentLevel;
            }

            // Process multiplicative modifiers
            foreach (var (stat, value) in blessing.MultiplicativeModifiers)
            {
                multiplicativeTotal.TryAdd(stat, 1.0);

                // Apply compounding for multiplicative modifiers with level
                for (var i = 0; i < blessing.CurrentLevel; i++) multiplicativeTotal[stat] *= value;
            }
        }

        // Apply the accumulated modifiers to the entity's stats
        _owner.StatsManager?.Recalculate(additiveTotal, multiplicativeTotal);

        _statsAreDirty = false;
    }

    // Event handlers
    private void OnDamageDealt((double, BaseEntity, EntityStats, Vector2) damageData)
    {
        var damage = damageData.Item1;
        var target = damageData.Item2;

        // Only process if our owner is the attacker
        if (target != _owner)
            // Notify blessings
            foreach (var blessing in _blessings.Values)
                blessing.OnDamageDealt(damage);
    }

    // Callback methods for events that blessings might want to respond to
    public void NotifyAbilityUsed(AbilityVariant abilityVariant)
    {
        foreach (var blessing in _blessings.Values) blessing.OnAbilityUsed(abilityVariant);
    }

    public void NotifyDamageTaken(double amount)
    {
        foreach (var blessing in _blessings.Values) blessing.OnDamageTaken(amount);
    }

    public void NotifyEnemyKilled()
    {
        foreach (var blessing in _blessings.Values) blessing.OnEnemyKilled();
    }

    public void NotifyTimeRewound()
    {
        foreach (var blessing in _blessings.Values) blessing.OnTimeRewound();
    }

    // Cleanup
    public void Dispose()
    {
        GlobalEventBus.Instance.Unsubscribe<(double, BaseEntity, EntityStats, Vector2)>(
            GlobalEventVariant.DamageDealt, OnDamageDealt);
    }
}