using System.Collections.Generic;
using ChronosDescent.Scripts.Core.Entity;
using Godot;

namespace ChronosDescent.Scripts.Core.State;

public class PositionRecord : ISystem
{
    public record RecordUnit
    {
        public Vector2 Position { get; init; }
        public double Delta { get; init; }
    }

    private readonly LinkedList<RecordUnit> _records = [];
    private IEntity _owner;
    private double _totalTime;
    private const double MaxTime = 5.0d;

    public bool Recording { get; set; } = true;

    public void Initialize(IEntity owner)
    {
        _owner = owner;
    }

    public void Update(double delta)
    {
    }

    public void FixedUpdate(double delta)
    {
        if (!Recording) return;

        var newUnit = new RecordUnit { Position = _owner.GlobalPosition, Delta = delta };

        _totalTime += delta;
        _records.AddLast(newUnit);

        while (_totalTime >= MaxTime)
        {
            _totalTime -= _records.First?.Value.Delta ?? 0;
            _records.RemoveFirst();
        }
    }

    public IEnumerable<RecordUnit> GetPositionHistory(double seconds)
    {
        if (_records == null || seconds <= 0) yield break;

        var grandTime = 0.0d;
        var current = _records.First;

        while (current != null && grandTime < seconds)
        {
            grandTime += current.Value.Delta;
            yield return current.Value;
            current = current.Next;
        }
    }
}