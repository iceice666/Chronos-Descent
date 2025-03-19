using System.Diagnostics;
using System.Linq;
using ChronosDescent.Scripts.Core.State;
using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

namespace ChronosDescent.Tests.State;

[TestSuite]
public class PositionRecordTest
{
    private TestEntity _entity;
    private PositionRecord _positionRecord;

    [Before]
    public void TestSetup()
    {
        // while (!Debugger.IsAttached) ;
    }

    [BeforeTest]
    public void Setup()
    {
        _entity = new TestEntity();
        _positionRecord = new PositionRecord();
        _positionRecord.Initialize(_entity);
    }

    [AfterTest]
    public void Teardown()
    {
        _entity.Free();
    }

    [TestCase]
    public void TestInitialization()
    {
        AssertThat(_positionRecord.Recording).IsTrue();
    }

    [TestCase]
    public void TestRecordingState()
    {
        _positionRecord.Recording = true;
        AssertThat(_positionRecord.Recording).IsTrue();

        _positionRecord.Recording = false;
        AssertThat(_positionRecord.Recording).IsFalse();
    }

    [TestCase]
    public void TestFixedUpdateWithoutRecording()
    {
        _entity.GlobalPosition = new Vector2(10, 10);
        _positionRecord.Recording = false;

        // Execute several updates
        _positionRecord.FixedUpdate(0.1);
        _positionRecord.FixedUpdate(0.1);

        // No positions should be recorded
        var history = _positionRecord.GetPositionHistory(1.0);
        AssertThat(history).IsEmpty();
    }

    [TestCase]
    public void TestPositionRecording()
    {
        _positionRecord.Recording = true;

        _entity.GlobalPosition = new Vector2(10, 10);
        _positionRecord.FixedUpdate(0.1);

        _entity.GlobalPosition = new Vector2(20, 20);
        _positionRecord.FixedUpdate(0.1);

        _entity.GlobalPosition = new Vector2(30, 30);
        _positionRecord.FixedUpdate(0.1);

        var history = _positionRecord.GetPositionHistory(1.0).ToArray();

        AssertThat(history).HasSize(3);
        AssertThat(history[0].Position).IsEqual(new Vector2(10, 10));
        AssertThat(history[1].Position).IsEqual(new Vector2(20, 20));
        AssertThat(history[2].Position).IsEqual(new Vector2(30, 30));
    }

    [TestCase]
    public void TestMaxTimeLimit()
    {
        _positionRecord.Recording = true;

        // Record positions for more than 5 seconds (MaxTime constant)
        for (var i = 0; i < 60; i++)
        {
            _entity.GlobalPosition = new Vector2(i, i);
            _positionRecord.FixedUpdate(0.1); // 60 * 0.1 = 6.0 seconds total
        }

        // Even though we added 60 positions, MaxTime (5.0) should limit how many we can retrieve
        var history = _positionRecord.GetPositionHistory(10.0).ToArray(); // Ask for more than MaxTime

        // Calculate expected count (5.0 / 0.1 = 50 positions)
        AssertThat(history.Length).IsLessEqual(51); // Allow for floating point imprecision

        // Verify the positions are the most recent ones (not the oldest)
        AssertThat(history[^1].Position.X).IsEqual(59);
    }

    [TestCase]
    public void TestGetPositionHistoryWithLimitedTime()
    {
        _positionRecord.Recording = true;

        for (var i = 0; i < 10; i++)
        {
            _entity.GlobalPosition = new Vector2(i * 10, i * 10);
            _positionRecord.FixedUpdate(0.1); // 10 * 0.1 = 1.0 second total
        }

        // Request only 0.3 seconds of history
        var limitedHistory = _positionRecord.GetPositionHistory(0.3).ToArray();

        // Should get only the first 3 records (0.3 / 0.1 = 3)
        AssertThat(limitedHistory).HasSize(3);
        AssertThat(limitedHistory[0].Position).IsEqual(new Vector2(0, 0));
        AssertThat(limitedHistory[2].Position).IsEqual(new Vector2(20, 20));
    }

    [TestCase]
    public void TestGetPositionHistoryWithZeroOrNegativeTime()
    {
        _positionRecord.Recording = true;

        _entity.GlobalPosition = new Vector2(10, 10);
        _positionRecord.FixedUpdate(0.1);

        // Request zero seconds
        var zeroHistory = _positionRecord.GetPositionHistory(0).ToArray();
        AssertThat(zeroHistory).IsEmpty();

        // Request negative seconds
        var negativeHistory = _positionRecord.GetPositionHistory(-1).ToArray();
        AssertThat(negativeHistory).IsEmpty();
    }

    [TestCase]
    public void TestRestartRecording()
    {
        // Start recording
        _entity.GlobalPosition = new Vector2(10, 10);
        _positionRecord.FixedUpdate(0.1);

        // Stop recording
        _positionRecord.Recording = false;
        _entity.GlobalPosition = new Vector2(20, 20);
        _positionRecord.FixedUpdate(0.1);

        // Restart recording
        _positionRecord.Recording = true;
        _entity.GlobalPosition = new Vector2(30, 30);
        _positionRecord.FixedUpdate(0.1);

        var history = _positionRecord.GetPositionHistory(1.0).ToArray();

        // Should have only recorded positions when Recording was true
        AssertThat(history).HasSize(2);
        AssertThat(history[0].Position).IsEqual(new Vector2(10, 10));
        AssertThat(history[1].Position).IsEqual(new Vector2(30, 30));
    }

    [TestCase]
    public void TestDeltaTimeRecording()
    {
        _positionRecord.Recording = true;

        // Record positions with different delta times
        _entity.GlobalPosition = new Vector2(10, 10);
        _positionRecord.FixedUpdate(0.1);

        _entity.GlobalPosition = new Vector2(20, 20);
        _positionRecord.FixedUpdate(0.2);

        _entity.GlobalPosition = new Vector2(30, 30);
        _positionRecord.FixedUpdate(0.3);

        var history = _positionRecord.GetPositionHistory(1.0).ToArray();

        AssertThat(history).HasSize(3);
        AssertThat(history[0].Delta).IsEqual(0.1);
        AssertThat(history[1].Delta).IsEqual(0.2);
        AssertThat(history[2].Delta).IsEqual(0.3);
    }
}