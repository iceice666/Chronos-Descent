using ChronosDescent.Scripts.Entities;

namespace ChronosDescent.Scripts.Core.Currency;

/// <summary>
///     Handles currency transactions for shop purchases and other exchanges
/// </summary>
public class Transaction
{
    /// <summary>
    ///     Result of a transaction attempt
    /// </summary>
    public enum Result
    {
        Success,
        InsufficientFunds,
        InvalidItem,
        MaxedOut
    }

    /// <summary>
    ///     Attempt to purchase an item with chronoshards
    /// </summary>
    /// <param name="player">The player making the purchase</param>
    /// <param name="cost">The cost in chronoshards</param>
    /// <returns>Transaction result</returns>
    public static Result TryPurchase(Player player, int cost)
    {
        // Check if player has enough funds
        if (!player.CurrencyManager.HasEnoughChronoshards(cost))
        {
            // Signal that funds are insufficient
            player.EventBus.Publish(EventVariant.CurrencyInsufficient, cost);
            return Result.InsufficientFunds;
        }

        // Spend the currency
        player.CurrencyManager.SpendChronoshards(cost);

        // Signal success
        GlobalEventBus.Instance.Publish(GlobalEventVariant.ShopTransactionCompleted, cost);

        return Result.Success;
    }

    /// <summary>
    ///     Refund a previously made purchase
    /// </summary>
    /// <param name="player">The player to refund</param>
    /// <param name="amount">The amount to refund</param>
    public static void Refund(Player player, int amount)
    {
        player.CurrencyManager.AddChronoshards(amount);
    }
}