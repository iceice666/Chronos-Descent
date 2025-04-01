using System.Diagnostics;
using ChronosDescent.Scripts.Dungeon;
using GdUnit4;

namespace ChronosDescent.Tests;

[TestSuite]
public class TestDungeonGenerator
{
    [BeforeTest]
    public void Setup()
    {
        while (!Debugger.IsAttached) ;
    }

    [TestCase]
    public void GenerateDungeon()
    {
        var dungeon = DungeonGenerator.Generate(100);
    }
}