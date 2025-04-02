using System;
using Godot;

namespace ChronosDescent.Scripts.Core;

public sealed class GameStats
{
    private static GameStats _instance;
    public static GameStats Instance => _instance ??= new GameStats();

    private double _startTime;
    private double _gameTime;
    private bool _timerActive;

    public int EnemiesDefeated { get; private set; }
    public double DamageCaused { get; private set; }
    public int CurrentLevel { get; private set; }
    public double TimePlayed => _timerActive ? Time.GetUnixTimeFromSystem() - _startTime + _gameTime : _gameTime;

    private GameStats()
    {
        Reset();
    }

    public void StartGame(int startLevel = 1)
    {
        Reset();
        CurrentLevel = startLevel;
        _startTime = Time.GetUnixTimeFromSystem();
        _timerActive = true;
    }

    public void PauseTimer()
    {
        if (!_timerActive) return;
        
        _gameTime += Time.GetUnixTimeFromSystem() - _startTime;
        _timerActive = false;
    }

    public void ResumeTimer()
    {
        if (_timerActive) return;
        
        _startTime = Time.GetUnixTimeFromSystem();
        _timerActive = true;
    }

    public void Reset()
    {
        EnemiesDefeated = 0;
        DamageCaused = 0;
        CurrentLevel = 0;
        _gameTime = 0;
        _timerActive = false;
    }

    public void RecordEnemyDefeat()
    {
        EnemiesDefeated++;
    }

    public void RecordDamageCaused(double damage)
    {
        DamageCaused += damage;
    }

    public void SetLevel(int level)
    {
        CurrentLevel = level;
    }

    public string FormatTimePlayed()
    {
        var totalSeconds = TimePlayed;
        var hours = (int)(totalSeconds / 3600);
        var minutes = (int)((totalSeconds % 3600) / 60);
        var seconds = (int)(totalSeconds % 60);
        
        return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }
}