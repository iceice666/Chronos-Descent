using Godot;

namespace ChronosDescent.Scripts.node.Component;

[GlobalClass]
public partial class AnimationComponent : AnimatedSprite2D
{
    private bool _disableAnimationUpdate;

    /// <summary>
    /// Update sprite flip when velocity change
    /// </summary>
    /// <param name="velocity">The velocity last time applied to entity.</param>
    public void UpdateAnimation(Vector2 velocity)
    {
        if (_disableAnimationUpdate) return;

        if (Animation == "walk" && velocity == Vector2.Zero)
        {
            Animation = "idle";
        }
        else if (velocity.X < 0)
        {
            FlipH = true;
            Animation = "walk";
        }
        else if (velocity.X > 0)
        {
            FlipH = false;
            Animation = "walk";
        }
        else if (velocity.Y != 0)
        {
            Animation = "walk";
        }
    }

    public void ResetAnimation()
    {
        _disableAnimationUpdate = false;
    }

    public void PlayAnimation(string animationName)
    {
        Play(animationName);
    }

   
}