using ChronosDescent.Scripts.Core.Entity;

namespace ChronosDescent.Scripts.Core.Currency;

/// <summary>
///     Manages player's currency in the game
/// </summary>
public class CurrencyManager : ISystem
{
    // Event for when currency amount changes
    public delegate void CurrencyChangedHandler(int amount);

    private BaseEntity _owner;

    // The main currency of the game
    public int Chronoshards { get; private set; }

    public void Initialize(BaseEntity owner)
    {
        _owner = owner;
    }

    public void Update(double delta)
    {
        // No update logic needed
    }

    public void FixedUpdate(double delta)
    {
        // No fixed update logic needed
    }

    public event CurrencyChangedHandler OnChronoshardsChanged;

    /// <summary>
    ///     Add currency to the player
    /// </summary>
    /// <param name="amount">Amount to add (positive value)</param>
    public void AddChronoshards(int amount)
    {
        if (amount <= 0) return;

        Chronoshards += amount;
        OnChronoshardsChanged?.Invoke(Chronoshards);

        // Notify through event bus
        _owner?.EventBus.Publish(EventVariant.CurrencyChanged, Chronoshards);
    }

    /// <summary>
    ///     Remove currency from the player
    /// </summary>
    /// <param name="amount">Amount to remove (positive value)</param>
    /// <returns>True if the player had enough currency, false otherwise</returns>
    public bool SpendChronoshards(int amount)
    {
        if (amount <= 0) return true;
        if (Chronoshards < amount) return false;

        Chronoshards -= amount;
        OnChronoshardsChanged?.Invoke(Chronoshards);

        // Notify through event bus
        _owner?.EventBus.Publish(EventVariant.CurrencyChanged, Chronoshards);

        return true;
    }

    /// <summary>
    ///     Check if the player has enough currency
    /// </summary>
    /// <param name="amount">Amount to check</param>
    /// <returns>True if the player has enough currency</returns>
    public bool HasEnoughChronoshards(int amount)
    {
        return Chronoshards >= amount;
    }

    /// <summary>
    ///     Reset currency to zero
    /// </summary>
    public void Reset()
    {
        Chronoshards = 0;
        OnChronoshardsChanged?.Invoke(Chronoshards);
    }

    /// <summary>
    ///     Set currency to a specific amount
    /// </summary>
    /// <param name="amount">Amount to set</param>
    public void SetChronoshards(int amount)
    {
        if (amount < 0) amount = 0;

        Chronoshards = amount;
        OnChronoshardsChanged?.Invoke(Chronoshards);

        // Notify through event bus
        _owner?.EventBus.Publish(EventVariant.CurrencyChanged, Chronoshards);
    }
}