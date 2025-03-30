using Godot;

public class Utils
{
    // Rotates a target point to a desired angle on the unit circle
    public static Vector2 RotateAngle(Vector2 originPoint, Vector2 targetPoint, double angleRadians)
    {
        // Calculate the offset
        var offset = targetPoint - originPoint;

        // Multiply the rotation matrix
        var sin = Mathf.Sin(angleRadians);
        var cos = Mathf.Cos(angleRadians);

        var newOffsetX = offset.X * cos - offset.Y * sin;
        var newOffsetY = offset.X * sin + offset.Y * cos;

        // Add origin point
        return new Vector2((float)newOffsetX, (float)newOffsetY) + originPoint;
    }
}