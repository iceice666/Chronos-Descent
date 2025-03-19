using Godot;

namespace ChronosDescent.Scripts.Core.Entity;

public interface IActionManager
{
    public Vector2 MoveDirection { get; protected set; }
    public Vector2 LookDirection { get; protected set; }
}

public partial class ActionManager : Node, IActionManager
{
    private IEntity _owner;


    public override void _Ready()
    {
        _owner = (IEntity)GetOwner();
    }

    #region Preporities

    public Vector2 MoveDirection { get; set; }
    public Vector2 LookDirection { get; set; }

    #endregion
}