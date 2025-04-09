using System;
using System.Collections.Generic;
using ChronosDescent.Scripts.Abilities;
using ChronosDescent.Scripts.Core;
using ChronosDescent.Scripts.Entities;
using Godot;

namespace ChronosDescent.Scripts.UI;

public partial class PrepareMenu : Control
{
    private readonly Dictionary<string, Type> _lifeSavingAbilities = new()
    {
        { "LifeSaving_Dash", typeof(Dash) },
        { "LifeSaving_TimeRewind", typeof(TimeRewind) },
        { "LifeSaving_Heal", typeof(Heal) }
    };

    private readonly Dictionary<string, PackedScene> _weapons = new()
    {
        { "Weapon_Bow", GD.Load<PackedScene>("res://Scenes/weapon/bow.tscn") },
        { "Weapon_Claymore", GD.Load<PackedScene>("res://Scenes/weapon/claymore.tscn") }
    };

    private Label _abilityDescription;
    private ItemList _abilityList;

    private Player _player;
    private string _selectedAbilityKey = "LifeSaving_Dash";

    private string _selectedWeaponKey = "Weapon_Bow";
    private SpecialButton _startRunButton;
    private Label _weaponDescription;
    private ItemList _weaponList;

    public override void _Ready()
    {
        _weaponList = GetNode<ItemList>("WeaponSelection/WeaponList");
        _abilityList = GetNode<ItemList>("AbilitySelection/AbilityList");
        _weaponDescription = GetNode<Label>("WeaponSelection/WeaponDescription");
        _abilityDescription = GetNode<Label>("AbilitySelection/AbilityDescription");
        _startRunButton = GetNode<SpecialButton>("StartRunButton");

        _startRunButton.Init(OnStartRunPressed);
        _weaponList.ItemSelected += OnWeaponSelected;
        _abilityList.ItemSelected += OnAbilitySelected;

        GetNode<Label>("Title").SetTextTr("Prepare_Title");
        GetNode<Label>("WeaponSelection/Label").SetTextTr("Prepare_Weapon_Title");
        GetNode<Label>("AbilitySelection/Label").SetTextTr("Prepare_Ability_Title");

        InitializeLists();
    }

    private void InitializeLists()
    {
        // Populate weapon list
        _weaponList.Clear();

        foreach (var weaponKey in _weapons.Keys)
        {
            // Extract weapon name from the key (e.g., "Weapon_Bow" -> "bow")
            var weaponName = weaponKey.Split('_')[1].ToLower();

            // Load weapon icon with error handling
            var iconPath = $"res://Assets/icons/{weaponName}.png";
            Texture2D icon;

            try
            {
                if (ResourceLoader.Exists(iconPath))
                {
                    icon = GD.Load<Texture2D>(iconPath);
                }
                else
                {
                    GD.PushWarning($"Weapon icon not found: {iconPath}, using blank texture");
                    icon = null;
                }
            }
            catch (Exception ex)
            {
                GD.PushError($"Error loading weapon icon {iconPath}: {ex.Message}");
                icon = null;
            }

            // Add item with icon (or without if icon failed to load)
            if (icon != null)
                _weaponList.AddItem(
                    TranslationManager.Tr(weaponKey),
                    icon
                );
            else
                _weaponList.AddItem(TranslationManager.Tr(weaponKey));

            // Store the key as metadata
            _weaponList.SetItemMetadata(_weaponList.ItemCount - 1, weaponKey);
        }

        // Select default weapon
        for (var i = 0; i < _weaponList.ItemCount; i++)
            if (_weaponList.GetItemMetadata(i).AsString() == _selectedWeaponKey)
            {
                _weaponList.Select(i);
                UpdateWeaponDescription(_selectedWeaponKey);
                break;
            }

        // Populate ability list
        _abilityList.Clear();

        foreach (var abilityKey in _lifeSavingAbilities.Keys)
        {
            // Extract ability name from the key (e.g., "LifeSaving_Dash" -> "dash")
            var abilityName = abilityKey.Split('_')[1].ToLower();

            // Try to load ability icon, fallback to missing icon if not found
            var icon = GD.Load<Texture2D>($"res://Assets/icons/{abilityName}.png");


            // Add item with icon
            _abilityList.AddItem(
                TranslationManager.Tr(abilityKey),
                icon
            );


            // Store the key as metadata
            _abilityList.SetItemMetadata(_abilityList.ItemCount - 1, abilityKey);
        }

        // Select default ability
        for (var i = 0; i < _abilityList.ItemCount; i++)
            if (_abilityList.GetItemMetadata(i).AsString() == _selectedAbilityKey)
            {
                _abilityList.Select(i);
                UpdateAbilityDescription(_selectedAbilityKey);
                break;
            }


        _weaponList.GrabFocus();
    }

    private void OnWeaponSelected(long index)
    {
        _selectedWeaponKey = _weaponList.GetItemMetadata((int)index).AsString();
        UpdateWeaponDescription(_selectedWeaponKey);
    }

    private void OnAbilitySelected(long index)
    {
        _selectedAbilityKey = _abilityList.GetItemMetadata((int)index).AsString();
        UpdateAbilityDescription(_selectedAbilityKey);
    }

    private void UpdateWeaponDescription(string weaponKey)
    {
        switch (weaponKey)
        {
            case "Weapon_Bow":
                _weaponDescription.SetTextTr("Prepare_Bow_Desc");
                break;
            case "Weapon_Claymore":
                _weaponDescription.SetTextTr("Prepare_Claymore_Desc");
                break;
            default:
                _weaponDescription.Text = "";
                break;
        }
    }

    private void UpdateAbilityDescription(string abilityKey)
    {
        switch (abilityKey)
        {
            case "LifeSaving_Dash":
                _abilityDescription.SetTextTr("Prepare_Dash_Desc");
                break;
            case "LifeSaving_TimeRewind":
                _abilityDescription.SetTextTr("Prepare_TimeRewind_Desc");
                break;
            case "LifeSaving_Heal":
                _abilityDescription.SetTextTr("Prepare_Heal_Desc");
                break;
            default:
                _abilityDescription.Text = "";
                break;
        }
    }

    private void OnStartRunPressed()
    {
        SavePlayerSelections();

        // Start the game
        GetTree().ChangeSceneToFile("res://Scenes/dungeon.tscn");
    }

    private void SavePlayerSelections()
    {
        // Store selections in the GameManager singleton
        var gameManager = GetNode<GameManager>("/root/GameManager");
        gameManager.SelectedWeapon = _selectedWeaponKey;
        gameManager.SelectedAbility = _selectedAbilityKey;
    }
}