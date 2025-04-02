using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ChronosDescent.Scripts.Core.Blessing;

/// <summary>
///     Registry for all available temporal blessings
/// </summary>
[GlobalClass]
public partial class Registry : Node
{
    // Dictionary of all blessings by ID
    private readonly Dictionary<string, Blessing> _allBlessings = [];

    // Track deity favor and associated blessings
    private readonly Dictionary<TimeDeity, List<Blessing>> _blessingsByDeity = [];

    // Track blessing rarity distributions
    private readonly Dictionary<BlessingRarity, List<Blessing>> _blessingsByRarity = [];
    private readonly List<Blessing> _defensiveBlessings = [];
    private readonly List<Blessing> _movementBlessings = [];

    // Lists of blessings by category
    private readonly List<Blessing> _offensiveBlessings = [];
    private readonly List<Blessing> _utilityBlessings = [];
    public static Registry Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;

        // Initialize rarity collections
        foreach (BlessingRarity rarity in Enum.GetValues(typeof(BlessingRarity))) _blessingsByRarity[rarity] = [];

        // Initialize deity collections
        foreach (TimeDeity deity in Enum.GetValues(typeof(TimeDeity))) _blessingsByDeity[deity] = [];

        // Load and register all blessings
        RegisterAllBlessings();
    }

    /// <summary>
    ///     Initialize and register all available blessings
    /// </summary>
    private void RegisterAllBlessings()
    {
        // Offensive Blessings
        RegisterBlessing(new TemporalEchoBlessing());
        RegisterBlessing(new TimeAcceleratedStrikesBlessing());
        RegisterBlessing(new EntropyBuildupBlessing());
        RegisterBlessing(new ParadoxDamageBlessing());

        // Defensive Blessings
        RegisterBlessing(new TemporalShieldBlessing());
        RegisterBlessing(new ProbabilityFieldBlessing());
        RegisterBlessing(new WoundReversalBlessing());
        RegisterBlessing(new TimelineDiversionBlessing());

        // Utility Blessings
        RegisterBlessing(new ChronoHarvesterBlessing());
        RegisterBlessing(new TimelineEfficiencyBlessing());
        RegisterBlessing(new TemporalInsightBlessing());
        RegisterBlessing(new ResourceRecursionBlessing());

        // Movement Blessings
        RegisterBlessing(new PhaseShiftingBlessing());
        RegisterBlessing(new TimeSlipstreamBlessing());
        RegisterBlessing(new SpacetimeCompressionBlessing());
        RegisterBlessing(new TemporalWakeBlessing());
    }

    /// <summary>
    ///     Register a blessing in all appropriate collections
    /// </summary>
    private void RegisterBlessing(Blessing blessing)
    {
        // Add to all blessings dictionary
        _allBlessings[blessing.Id] = blessing;

        // Add to category list
        switch (blessing.Category)
        {
            case BlessingCategory.Offensive:
                _offensiveBlessings.Add(blessing);
                break;
            case BlessingCategory.Defensive:
                _defensiveBlessings.Add(blessing);
                break;
            case BlessingCategory.Utility:
                _utilityBlessings.Add(blessing);
                break;
            case BlessingCategory.Movement:
                _movementBlessings.Add(blessing);
                break;
        }

        // Add to rarity list
        _blessingsByRarity[blessing.Rarity].Add(blessing);

        // Add to deity list
        _blessingsByDeity[blessing.Deity].Add(blessing);
    }

    /// <summary>
    ///     Get a blessing by its ID
    /// </summary>
    public Blessing GetBlessing(string id)
    {
        // Create a copy of the blessing to prevent modifying the template
        if (_allBlessings.TryGetValue(id, out var blessing)) return blessing.Duplicate() as Blessing;

        return null;
    }

    /// <summary>
    ///     Get random blessings weighted by rarity
    /// </summary>
    public List<Blessing> GetRandomBlessings(int count, int level = 1)
    {
        var result = new List<Blessing>();

        // Define rarity weights (lower level = more common blessings)
        var rarityWeights = new Dictionary<BlessingRarity, float>();

        switch (level)
        {
            // Adjust weights based on dungeon level
            case <= 3:
                // Early game favors common/uncommon
                rarityWeights[BlessingRarity.Common] = 0.45f;
                rarityWeights[BlessingRarity.Uncommon] = 0.35f;
                rarityWeights[BlessingRarity.Rare] = 0.15f;
                rarityWeights[BlessingRarity.Epic] = 0.05f;
                rarityWeights[BlessingRarity.Legendary] = 0.0f; // None at early levels
                break;
            case <= 7:
                // Mid game balanced distribution
                rarityWeights[BlessingRarity.Common] = 0.30f;
                rarityWeights[BlessingRarity.Uncommon] = 0.30f;
                rarityWeights[BlessingRarity.Rare] = 0.25f;
                rarityWeights[BlessingRarity.Epic] = 0.10f;
                rarityWeights[BlessingRarity.Legendary] = 0.05f;
                break;
            default:
                // Late game favors rare/epic/legendary
                rarityWeights[BlessingRarity.Common] = 0.15f;
                rarityWeights[BlessingRarity.Uncommon] = 0.20f;
                rarityWeights[BlessingRarity.Rare] = 0.30f;
                rarityWeights[BlessingRarity.Epic] = 0.25f;
                rarityWeights[BlessingRarity.Legendary] = 0.10f;
                break;
        }

        // Keep track of selected blessings to avoid duplicates
        var selectedIds = new HashSet<string>();

        // Select random blessings weighted by rarity
        while (result.Count < count && selectedIds.Count < _allBlessings.Count)
        {
            // Randomly select a rarity based on weights
            var rarityRoll = GD.Randf();
            var cumulativeWeight = 0f;
            var selectedRarity = BlessingRarity.Common;

            foreach (var pair in rarityWeights)
            {
                cumulativeWeight += pair.Value;
                if (!(rarityRoll <= cumulativeWeight)) continue;
                selectedRarity = pair.Key;
                break;
            }

            // If no blessings of this rarity, try another
            if (_blessingsByRarity[selectedRarity].Count == 0)
                continue;

            // Select a random blessing of this rarity
            var rarityBlessings = _blessingsByRarity[selectedRarity];
            var selectedBlessing = rarityBlessings[GD.RandRange(0, rarityBlessings.Count - 1)];

            // Skip if already selected
            if (!selectedIds.Add(selectedBlessing.Id))
                continue;

            // Add to result
            result.Add(selectedBlessing.Duplicate() as Blessing);

            // Ensure varied categories when possible
            if (result.Count < 2 || result.Count >= count) continue;
            // Try to avoid duplicate categories
            var categoriesToAvoid = new HashSet<BlessingCategory>();
            foreach (var blessing in result) categoriesToAvoid.Add(blessing.Category);

            // Get all blessings from other categories
            var diverseBlessings = _allBlessings.Values.Where(blessing =>
                !categoriesToAvoid.Contains(blessing.Category) && !selectedIds.Contains(blessing.Id)).ToList();

            // If we have diverse options, select one
            if (diverseBlessings.Count <= 0) continue;
            var diverseBlessing = diverseBlessings[GD.RandRange(0, diverseBlessings.Count - 1)];
            selectedIds.Add(diverseBlessing.Id);
            result.Add(diverseBlessing.Duplicate() as Blessing);
        }

        return result;
    }

    /// <summary>
    ///     Get blessings from a specific deity
    /// </summary>
    public List<Blessing> GetBlessingsByDeity(TimeDeity deity, int count)
    {
        var result = new List<Blessing>();
        var deityBlessings = _blessingsByDeity[deity];

        // If we don't have enough blessings for this deity, return what we have
        if (deityBlessings.Count <= count)
        {
            result.AddRange(deityBlessings.Select(blessing => blessing.Duplicate() as Blessing));

            return result;
        }

        // Otherwise select random blessings from this deity
        var selectedIndices = new HashSet<int>();

        while (result.Count < count && selectedIndices.Count < deityBlessings.Count)
        {
            var index = GD.RandRange(0, deityBlessings.Count - 1);

            if (!selectedIndices.Add(index)) continue;

            result.Add(deityBlessings[index].Duplicate() as Blessing);
        }

        return result;
    }

    /// <summary>
    ///     Get all available blessings
    /// </summary>
    public List<Blessing> GetAllBlessings()
    {
        return _allBlessings.Values.Select(blessing => blessing.Duplicate() as Blessing).ToList();
    }

    /// <summary>
    ///     Get all blessings by category
    /// </summary>
    public List<Blessing> GetBlessingsByCategory(BlessingCategory category)
    {
        var categoryList = category switch
        {
            BlessingCategory.Offensive => _offensiveBlessings,
            BlessingCategory.Defensive => _defensiveBlessings,
            BlessingCategory.Utility => _utilityBlessings,
            BlessingCategory.Movement => _movementBlessings,
            _ => []
        };

        return categoryList.Select(blessing => blessing.Duplicate() as Blessing).ToList();
    }
}