using ChronosDescent.Scripts.Ability.Node;
using Godot;

namespace ChronosDescent.Scripts.Weapon.Node;

[GlobalClass]
public partial class WeaponComponent : Godot.Node
{
	
	private Entity.Entity _owner;
	private AbilityManagerComponent _abilityManager;

	// Event for weapon changes
	public delegate void WeaponChangedEventHandler(BaseWeapon weapon);

	public event WeaponChangedEventHandler WeaponChanged;

	public BaseWeapon CurrentWeapon { get; private set; }

	public AnimationPlayer Animation => GetNode<AnimationPlayer>("Weapon/AnimationPlayer");

	public override void _Ready()
	{
		_owner = GetOwner<Entity.Entity>();
		_abilityManager = _owner.GetNode<AbilityManagerComponent>("AbilityManagerComponent");
	
	}

	// Equip a new weapon and set up its abilities
	public void EquipWeapon(BaseWeapon weapon)
	{
		// Remove previous weapon abilities
		if (CurrentWeapon != null)
		{
			_abilityManager.RemoveAbility(AbilitySlot.WeaponAttack);
			_abilityManager.RemoveAbility(AbilitySlot.WeaponSpecial);
			_abilityManager.RemoveAbility(AbilitySlot.WeaponUlt);
			
			CurrentWeapon.QueueFree();
		}

		CurrentWeapon = weapon;

		if (CurrentWeapon != null)
		{
			// Add weapon abilities to ability manager
			_abilityManager.SetAbility(AbilitySlot.WeaponAttack, CurrentWeapon.NormalAttack);
			_abilityManager.SetAbility(AbilitySlot.WeaponSpecial, CurrentWeapon.SpecialAttack);
			_abilityManager.SetAbility(AbilitySlot.WeaponUlt, CurrentWeapon.UltimateAttack);

			weapon.Name = "Weapon";
			
			AddChild(weapon);
		}

		WeaponChanged?.Invoke(CurrentWeapon);
	}
}
