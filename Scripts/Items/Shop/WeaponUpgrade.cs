using ChronosDescent.Scripts.Core.State;
using ChronosDescent.Scripts.Effects;
using Godot.Collections;

namespace ChronosDescent.Scripts.Items.Shop;

public partial class WeaponUpgrade : ShopItem
{
    protected override void ApplyItemEffect()
    {
        PlayerInRange.ApplyEffect(new SimpleEffect(
            "simple_crit_change",
            -1,
            new Dictionary<StatFieldSpecifier, double>
            {
                { StatFieldSpecifier.CriticalChance, 5 }
            }
        ));
    }
}