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
    private Button _startRunButton;
    private Label _weaponDescription;
    private ItemList _weaponList;

    public override void _Ready()
    {
        _weaponList = GetNode<ItemList>("WeaponSelection/WeaponList");
        _abilityList = GetNode<ItemList>("AbilitySelection/AbilityList");
        _weaponDescription = GetNode<Label>("WeaponSelection/WeaponDescription");
        _abilityDescription = GetNode<Label>("AbilitySelection/AbilityDescription");
        _startRunButton = GetNode<Button>("StartRunButton");

        _startRunButton.Pressed += OnStartRunPressed;
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
            _abilityList.AddItem(TranslationManager.Tr(abilityKey));
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
        gameManager.SelectedWeapon = TranslationManager.Tr(_selectedWeaponKey);
        gameManager.SelectedAbility = TranslationManager.Tr(_selectedAbilityKey);
    }
}