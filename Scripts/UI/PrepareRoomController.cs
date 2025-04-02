using Godot;
using System.Collections.Generic;
using ChronosDescent.Scripts.Abilities;
using ChronosDescent.Scripts.Entities;

namespace ChronosDescent.Scripts.UI;

public partial class PrepareRoomController : Control
{
    private readonly Dictionary<string, PackedScene> _weapons = new()
    {
        { "Bow", GD.Load<PackedScene>("res://Scenes/weapon/bow.tscn") },
        { "Claymore", GD.Load<PackedScene>("res://Scenes/weapon/claymore.tscn") }
    };

    private readonly Dictionary<string, System.Type> _lifeSavingAbilities = new()
    {
        { "Dash", typeof(Dash) },
        { "Time Rewind", typeof(TimeRewind) },
        { "Heal", typeof(Heal) }
    };

    private Player _player;
    private Button _startRunButton;
    private ItemList _weaponList;
    private ItemList _abilityList;
    private Label _weaponDescription;
    private Label _abilityDescription;

    private string _selectedWeapon = "Bow";
    private string _selectedAbility = "Dash";

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

        InitializeLists();
    }

    private void InitializeLists()
    {
        // Populate weapon list
        _weaponList.Clear();
        foreach (var weapon in _weapons.Keys)
        {
            _weaponList.AddItem(weapon);
        }
        
        // Select default weapon
        for (var i = 0; i < _weaponList.ItemCount; i++)
        {
            if (_weaponList.GetItemText(i) == _selectedWeapon)
            {
                _weaponList.Select(i);
                UpdateWeaponDescription(_selectedWeapon);
                break;
            }
        }

        // Populate ability list
        _abilityList.Clear();
        foreach (var ability in _lifeSavingAbilities.Keys)
        {
            _abilityList.AddItem(ability);
        }
        
        // Select default ability
        for (var i = 0; i < _abilityList.ItemCount; i++)
        {
            if (_abilityList.GetItemText(i) == _selectedAbility)
            {
                _abilityList.Select(i);
                UpdateAbilityDescription(_selectedAbility);
                break;
            }
        }
    }

    private void OnWeaponSelected(long index)
    {
        _selectedWeapon = _weaponList.GetItemText((int)index);
        UpdateWeaponDescription(_selectedWeapon);
    }

    private void OnAbilitySelected(long index)
    {
        _selectedAbility = _abilityList.GetItemText((int)index);
        UpdateAbilityDescription(_selectedAbility);
    }

    private void UpdateWeaponDescription(string weaponName)
    {
        switch (weaponName)
        {
            case "Bow":
                _weaponDescription.Text = "A versatile ranged weapon that fires arrows with various effects.";
                break;
            case "Claymore":
                _weaponDescription.Text = "A heavy melee weapon that deals high damage in close combat.";
                break;
            default:
                _weaponDescription.Text = "";
                break;
        }
    }

    private void UpdateAbilityDescription(string abilityName)
    {
        switch (abilityName)
        {
            case "Dash":
                _abilityDescription.Text = "Quickly dash in the direction you're facing, avoiding damage.";
                break;
            case "Time Rewind":
                _abilityDescription.Text = "Rewind time to a previous position, avoiding dangerous situations.";
                break;
            case "Heal":
                _abilityDescription.Text = "Instantly recover a significant amount of health.";
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
        gameManager.SelectedWeapon = _selectedWeapon;
        gameManager.SelectedAbility = _selectedAbility;
    }
}
