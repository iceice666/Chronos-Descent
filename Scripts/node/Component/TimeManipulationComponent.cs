using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ChronosDescent.Scripts.node.Component;

[GlobalClass]
public partial class TimeManipulationComponent : Node
{
    private readonly LinkedList<PositionSpan> _positionHistory = [];

    private double _currentTime;
    private Entity _entity;

    [Export] public double MaxPeriod = 10.0f;

    public bool NeedRecording;

    public override void _Ready()
    {
        _entity = GetParent<Entity>();
    }

    public void PauseRecording()
    {
        SetPhysicsProcess(false);
    }

    public void ResumeRecording()
    {
        SetPhysicsProcess(true);
    }

    public override void _PhysicsProcess(double delta)
    {
        _currentTime += delta;
        var newPosition = new PositionSpan(_currentTime, _entity.Position);

        if (_positionHistory.Count == 0)
        {
            _positionHistory.AddFirst(newPosition);
            return;
        }

        var last = _positionHistory.Last!.Value.DeltaTime;
        if (delta - last > MaxPeriod) _positionHistory.RemoveLast();

        _positionHistory.AddFirst(newPosition);
    }

    public IEnumerable<PositionSpan> GetPositionHistory()
    {
        return _positionHistory;
    }

    public IEnumerable<PositionSpan> GetPositionHistory(int to)
    {
        return _positionHistory.Take(to);
    }

    public IEnumerable<PositionSpan> GetPositionHistory(double seconds)
    {
        if (_positionHistory.Count == 0 || seconds <= 0) return [];

        return _positionHistory
            .Aggregate(new List<PositionSpan>(), (acc, span) =>
            {
                if (acc.Sum(x => x.DeltaTime) < seconds)
                    acc.Add(span);
                return acc;
            });
    }

    public void Reset()
    {
        _positionHistory.Clear();
        _currentTime = 0;
    }

    public struct PositionSpan(
        double deltaTime,
        Vector2 position
    )
    {
        public readonly double DeltaTime = deltaTime;
        public readonly Vector2 Position = position;
    }
}