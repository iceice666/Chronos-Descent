using ChronosDescent.Scripts.node;
using ChronosDescent.Scripts.node.Component;
using ChronosDescent.Scripts.resource;
using Godot;

namespace ChronosDescent.Scripts.UI;

public partial class AbilityIndicator : Control
{
    private AbilityManagerComponent _abilityManager;
    private ProgressBar _progressBar;
    private Label _chargingLabel;
    private Label _channelingLabel;

    private Ability _currentAbility;
    private AbilityManagerComponent.AbilityState _currentState;

    public override void _Ready()
    {
        Visible = false;

        _progressBar = GetNode<ProgressBar>("ProgressBar");
        _chargingLabel = GetNode<Label>("ChargingLabel");
        _channelingLabel = GetNode<Label>("ChannelingLabel");
    }

    public void Initialize(Entity entity)
    {
        if (entity == null) return;

        _abilityManager = entity.AbilityManager;
        _abilityManager.AbilityStateChanged += OnAbilityStateChanged;
        
        // Add ability activated handler to catch state changes
        _abilityManager.AbilityActivated += OnAbilityActivated;
    }
    
    private void OnAbilityActivated(Ability ability)
    {
        // When an ability is activated, check if we need to hide the indicator
        // (handles cases where an ability is auto-cast or interrupted)
        if (_currentAbility != ability ||
            ability.IsCharging ||
            ability.IsChanneling ||
            (!ability.IsOnCooldown && ability.Type != Ability.AbilityType.Active)) return;
        
        _currentAbility = null;
        Visible = false;
    }

    public override void _Process(double delta)
    {
        if (!Visible || _currentAbility == null) return;
        
        // Check for state changes that might have happened
        if ((_currentState == AbilityManagerComponent.AbilityState.Charging && !_currentAbility.IsCharging) ||
            (_currentState == AbilityManagerComponent.AbilityState.Channeling && !_currentAbility.IsChanneling))
        {
            // The ability has stopped charging/channeling
            _currentAbility = null;
            Visible = false;
            return;
        }
        
        UpdateProgress();
    }

    private void OnAbilityStateChanged(Ability ability, AbilityManagerComponent.AbilityState state)
    {
        // Show the indicator only for charging and channeling states
        if (state is not (AbilityManagerComponent.AbilityState.Charging
            or AbilityManagerComponent.AbilityState.Channeling))
        {
            // If this is our current ability, hide the indicator
            if (_currentAbility != ability) return;
            
            _currentAbility = null;
            Visible = false;
            return;
        }

        _currentAbility = ability;
        _currentState = state;
        Visible = true;
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        if (_currentAbility == null) return;

        var isCharging = _currentState == AbilityManagerComponent.AbilityState.Charging;
        _chargingLabel.Visible = isCharging;
        _channelingLabel.Visible = !isCharging;
    }

    private void UpdateProgress()
    {
        if (_currentAbility == null) return;

        var progress = _currentState switch
        {
            AbilityManagerComponent.AbilityState.Charging =>
                (float)(_currentAbility.CurrentChargeTime / _currentAbility.MaxChargeTime),
            AbilityManagerComponent.AbilityState.Channeling =>
                (float)(_currentAbility.CurrentChannelingTime / _currentAbility.ChannelingDuration),
            _ => 0
        };

        _progressBar.Value = progress * 100;
    }

    public override void _ExitTree()
    {
        if (_abilityManager == null) return;
        
        _abilityManager.AbilityStateChanged -= OnAbilityStateChanged;
        _abilityManager.AbilityActivated -= OnAbilityActivated;
    }
}