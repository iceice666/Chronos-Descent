using ChronosDescent.Scripts.node;
using ChronosDescent.Scripts.node.Component;
using ChronosDescent.Scripts.resource;
using Godot;

namespace ChronosDescent.Scripts.UI;

public partial class AbilityProgressBar : Control
{
    private AbilityManagerComponent _abilityManager;
    private Label _channelingLabel;
    private Label _chargingLabel;

    private BaseAbility _currentAbility;
    private BaseAbility.AbilityState _currentState;
    private ProgressBar _progressBar;

    public override void _Ready()
    {
        Visible = false;

        _progressBar = GetNode<ProgressBar>("ProgressBar");
        _chargingLabel = GetNode<Label>("ChargingLabel");
        _channelingLabel = GetNode<Label>("ChannelingLabel");
    }

    public void Initialize(Entity entity)
    {
        if (entity == null)
            return;

        _abilityManager = entity.AbilityManager;
        _abilityManager.AbilityStateChanged += OnAbilityStateChanged;

        // Add ability activated handler to catch state changes
        _abilityManager.AbilityActivated += OnAbilityActivated;
    }

    private void OnAbilityActivated(object sender, AbilityManagerComponent.AbilityEventArgs e)
    {
        // When an ability is activated, check if we need to hide the indicator
        // (handles cases where an ability is auto-cast or interrupted)
        var ability = e.Ability;
        if (
            _currentAbility != ability
            || ability is BaseChanneledAbility { IsChanneling: true }
            || ability is BaseChargedAbility { IsCharging: true }
            || (!ability.IsOnCooldown && ability is not BaseActiveAbility)
        )
            return;

        _currentAbility = null;
        Visible = false;
    }

    public override void _Process(double delta)
    {
        if (!Visible || _currentAbility == null)
            return;

        // Check for state changes that might have happened
        if (
            (
                _currentState == BaseAbility.AbilityState.Charging
                && !((BaseChargedAbility)_currentAbility).IsCharging
            )
            || (
                _currentState == BaseAbility.AbilityState.Channeling
                && !((BaseChanneledAbility)_currentAbility).IsChanneling
            )
        )
        {
            // The ability has stopped charging/channeling
            _currentAbility = null;
            Visible = false;
            return;
        }

        UpdateProgress();
    }

    private void OnAbilityStateChanged(
        object sender,
        AbilityManagerComponent.AbilityStateEventArgs e
    )
    {
        var ability = e.Ability;
        var state = e.State;

        // Show the indicator only for charging and channeling states
        if (state is not (BaseAbility.AbilityState.Charging or BaseAbility.AbilityState.Channeling))
        {
            // If this is our current ability, hide the indicator
            if (_currentAbility != ability)
                return;

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
        if (_currentAbility == null)
            return;

        var isCharging = _currentState == BaseAbility.AbilityState.Charging;
        _chargingLabel.Visible = isCharging;
        _channelingLabel.Visible = !isCharging;
    }

    private void UpdateProgress()
    {
        float progress;

        if (_currentAbility == null)
            return;

        switch (_currentState)
        {
            case BaseAbility.AbilityState.Charging:
            {
                var ab = (BaseChargedAbility)_currentAbility;
                progress = (float)(ab.CurrentChargeTime / ab.MaxChargeTime);
                break;
            }
            case BaseAbility.AbilityState.Channeling:
            {
                var ab = (BaseChanneledAbility)_currentAbility;

                progress = (float)(ab.CurrentChannelingTime / ab.ChannelingDuration);
                break;
            }
            default:
                progress = 0;
                break;
        }

        _progressBar.Value = progress * 100;
    }

    public override void _ExitTree()
    {
        if (_abilityManager == null)
            return;

        _abilityManager.AbilityStateChanged -= OnAbilityStateChanged;
        _abilityManager.AbilityActivated -= OnAbilityActivated;
    }
}
