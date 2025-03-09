using Godot;
using System;
using ChronosDescent.Scripts.node;
using ChronosDescent.Scripts.node.Component;
using ChronosDescent.Scripts.node.UI;
using ChronosDescent.Scripts.resource.Effects;

public partial class EffectsContainer : HBoxContainer
{
    private PackedScene _timerNode = GD.Load<PackedScene>("res://Scenes/effect_timer.tscn");

    public EffectManagerComponent EffectManager { get; set; }


    public void Init()
    {
        var player = GetNode<Player>("/root/BattleScene/Player");
        EffectManager = player.EffectManager;

        EffectManager.EffectApplied += OnEffectApplied;
        EffectManager.EffectRemoved += OnEffectRemoved;
        EffectManager.EffectTimerUpdated += OnEffectTimerUpdated;
        EffectManager.EffectRefreshed += OnEffectRefreshed;
    }

    private void OnEffectApplied(Effect effect)
    {
        var newNode = _timerNode.Instantiate<EffectTimer>();
        newNode.Icon = effect.Icon;
        newNode.MaxStack = effect.MaxStacks;
        newNode.Name = effect.Identifier;

        AddChild(newNode);
    }

    private void OnEffectRemoved(string effectId)
    {
        GetNode(effectId).QueueFree();
    }

    private void OnEffectTimerUpdated(string effectId, double remainingDuration) { }
    private void OnEffectRefreshed(string effectId, int currentStack) { }
}