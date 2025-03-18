using Godot;

namespace ChronosDescent.Scripts.Entity.Node;

[GlobalClass]
public partial class AnimationComponent : AnimatedSprite2D
{
    private bool _disableAnimationUpdate;


    public void UpdateLookAnimation(Vector2 vec)
    {
        if (_disableAnimationUpdate) return;

        FlipH = vec.X switch
        {
            < 0 => true,
            > 0 => false,
            _ => FlipH
        };
    }

    public void UpdateWalkAnimation(Vector2 vec)
    {
        if (_disableAnimationUpdate) return;

        Animation = vec.LengthSquared() == 0 ? "idle" : "walk";
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