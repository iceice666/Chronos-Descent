using System;
using Godot;

namespace ChronosDescent.Scripts;

public class Util
{
    public static bool NearlyEqual(double a, double b)
    {
        const double minNormal = 2.2250738585072014E-308d;
        const double epsilon = 1e-6;

        var absA = Math.Abs(a);
        var absB = Math.Abs(b);
        var diff = Math.Abs(a - b);

        if (a.Equals(b))
            // shortcut, handles infinities
            return true;

        if (a == 0 || b == 0 || absA + absB < minNormal)
            // a or b is zero or both are extremely close to it
            // relative error is less meaningful here
            return diff < epsilon * minNormal;

        {
            // use relative error
            return diff / (absA + absB) < epsilon;
        }
    }

    public static Vector2 Lerp(Vector2 from, Vector2 to, float weight)
    {
        var x = from.X + (to.X - from.X) * weight;
        var y = from.Y + (to.Y - from.Y) * weight;
        return new Vector2(x, y);
    }

    /// <summary>
    ///     Print a "Tick" message to Godot console
    /// </summary>
    public static void Tick()
    {
        GD.Print("Tick");
    }
}