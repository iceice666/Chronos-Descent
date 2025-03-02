using Godot;
using System;

public partial class Camera : Node2D
{
	
	private NodePath _targetPath = "/root/BattleScene/Player";
	private Node2D _target;
	private Vector2 _offset = new(0, 0);
	
		public override void _Ready()
	{
		_target = GetNode<Node2D>(_targetPath);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_target != null)
		{
			// Smoothly move camera to follow target
			Position = Position.Lerp(_target.Position + _offset, (float)delta * 5.0f);
		}
	}
}
