using ChronosDescent.Scripts.Core.Damage;

namespace ChronosDescent.Scripts.Items.Shop;

public partial class HealthPotion : ShopItem
{
    protected override void ApplyItemEffect()
    {
        PlayerInRange.TakeDamage(50, DamageType.Healing);
        ;
    }
}