using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ChronosDescent.Scripts.Dungeon;
using GdUnit4;
using System.Linq;
using ChronosDescent.Scripts.Dungeon.Room;

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