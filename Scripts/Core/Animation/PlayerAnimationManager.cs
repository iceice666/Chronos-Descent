using System;
using ChronosDescent.Scripts.Entities;
using Godot;

namespace ChronosDescent.Scripts.Core.Animation;

[GlobalClass]
public partial class PlayerAnimationManager : Node, IAnimationPlayer
{
    private bool _isPrevIdle = true;

    private bool _isPrevLookRight = true;
    [Export] public AnimationTree AnimationTree { get; set; }
    private Player _owner;

    public override void _Ready()
    {
        _owner = GetOwner<Player>();
    }

    public override void _Process(double delta)
    {
        var isLookRight = _owner.ActionManager.LookDirection.X > 0;

        if (_isPrevLookRight != isLookRight)
        {
            _isPrevLookRight = isLookRight;
            _owner.Scale = new Vector2(-_owner.Scale.X, _owner.Scale.Y);
        }
    }

    public void Play(string animation)
    {
        if (AnimationTree == null) return;
        if (animation == "dead")
        {
            AnimationTree.Set("parameters/PlayerState/conditions/is_dead", true);
        }
    }
}