using System;
using System.Collections.Generic;
using System.Linq;
using ChronosDescent.Scripts.Core;
using ChronosDescent.Scripts.Core.Damage;
using ChronosDescent.Scripts.Core.Effect;
using ChronosDescent.Scripts.Core.Entity;
using ChronosDescent.Scripts.Entities;
using Godot;

namespace ChronosDescent.Scripts.Dungeon.Room;

/// <summary>
///     Controller for the Event Room, handling various time-themed events
/// </summary>
[GlobalClass]
public partial class EventRoomController : Node2D
{
	// Constants
	private const int MinReward = 10;
	private const int MaxReward = 30;
	
	// Each event has an ID, title, description, and a callback to execute when selected
	private readonly List<EventData> _availableEvents = new();
	private Button _button1;
	private Button _button2;
	private RichTextLabel _descriptionLabel;
	private RichTextLabel _eventTitleLabel;
	private Player _player;
	private BaseEffect _temporaryEffect;
	
	// Current state variables
	private EventData _currentEvent;
	private bool _eventActive;
	
	/// <summary>
	///     Event panel reference for managing UI
	/// </summary>
	[Export] public Control EventPanel { get; set; }
	
	/// <summary>
	///     Labels for choice buttons - allows for customization per event
	/// </summary>
	[Export] public string Button1Text { get; set; } = "Accept";
	[Export] public string Button2Text { get; set; } = "Decline";
	
	public override void _Ready()
	{
		// Find UI elements
		_eventTitleLabel = EventPanel.GetNode<RichTextLabel>("%EventTitle");
		_descriptionLabel = EventPanel.GetNode<RichTextLabel>("%Description");
		_button1 = EventPanel.GetNode<Button>("%Button1");
		_button2 = EventPanel.GetNode<Button>("%Button2");
		
		// Get player reference
		_player = GetTree().GetFirstNodeInGroup("Player") as Player;
		
		// Register button callbacks
		_button1.Pressed += OnButton1Pressed;
		_button2.Pressed += OnButton2Pressed;
		
		// Define available events
		InitializeEvents();
		
		// Subscribe to room entered event
		GlobalEventBus.Instance.Subscribe(GlobalEventVariant.RoomEntered, OnRoomEntered);
	}
	
	public override void _ExitTree()
	{
		GlobalEventBus.Instance.Unsubscribe(GlobalEventVariant.RoomEntered, OnRoomEntered);
		
		// Unregister button callbacks
		_button1.Pressed -= OnButton1Pressed;
		_button2.Pressed -= OnButton2Pressed;
	}
	
	private void OnRoomEntered()
	{
		// Select a random event when the room is entered
		SelectRandomEvent();
	}
	
	private void SelectRandomEvent()
	{
		if (_availableEvents.Count == 0)
		{
			GD.PushError("No events available!");
			return;
		}
		
		// Select a random event, potentially weighting by dungeon level or player stats
		var dungeonLevel = DungeonManager.Instance.Level;
		
		// Filter events based on dungeon level (more difficult events at higher levels)
		var eligibleEvents = _availableEvents.Where(e => e.MinLevel <= dungeonLevel).ToList();
		
		if (eligibleEvents.Count == 0)
		{
			// Fallback to any event if none match the criteria
			eligibleEvents = _availableEvents;
		}
		
		// Select a random event
		_currentEvent = eligibleEvents[GD.RandRange(0, eligibleEvents.Count - 1)];
		
		// Update UI with special formatting for the title
		_eventTitleLabel.Text = $"[center][wave amp=50.0 freq=5.0][color=#add8e6]{_currentEvent.Title}[/color][/wave][/center]";
		_descriptionLabel.Text = _currentEvent.Description;
		
		// Update button text if specified in the event
		_button1.Text = _currentEvent.Button1Text ?? Button1Text;
		_button2.Text = _currentEvent.Button2Text ?? Button2Text;
		
		// Activate event
		_eventActive = true;

		Visible = true;
	}
	
	private void OnButton1Pressed()
	{
		if (!_eventActive || _currentEvent == null) return;
		
		// Execute event's option 1 callback
		_currentEvent.OnOption1Selected?.Invoke();
		
		// Complete the event
		CompleteEvent();
	}
	
	private void OnButton2Pressed()
	{
		if (!_eventActive || _currentEvent == null) return;
		
		// Execute event's option 2 callback
		_currentEvent.OnOption2Selected?.Invoke();
		
		// Complete the event
		CompleteEvent();
	}
	
	private void CompleteEvent()
	{
		_eventActive = false;
		
		// Clear UI
		Visible = false;
		
		
		// Allow room to be cleared
		GlobalEventBus.Instance.Publish(GlobalEventVariant.RoomCleared);
		
		// Show notification
		GlobalEventBus.Instance.Publish(
			GlobalEventVariant.BoardcastTitle,
			"Event completed. Proceed to the next room.");
	}
	
	#region Event Definitions
	
	private void InitializeEvents()
	{
		// 1. Temporal Echo - A simple dilemma with health or currency reward
		_availableEvents.Add(new EventData
		{
			ID = "temporal_echo",
			Title = "Temporal Echo",
			Description = "You encounter a temporal echo of yourself. It offers you " +
						"a choice: receive chronoshards or restore health.",
			MinLevel = 1,
			Button1Text = "Take Chronoshards",
			Button2Text = "Restore Health",
			OnOption1Selected = () =>
			{
				// Award chronoshards based on dungeon level
				var amount = MinReward + DungeonManager.Instance.Level * 2;
				_player.CurrencyManager.AddChronoshards(amount);
				
				GlobalEventBus.Instance.Publish(
					GlobalEventVariant.BoardcastTitle,
					$"Gained {amount} chronoshards!");
			},
			OnOption2Selected = () =>
			{
				// Restore health (20-40% of max health)
				var healAmount = _player.StatsManager.MaxHealth * (0.2 + DungeonManager.Instance.Level * 0.02);
				healAmount = Math.Min(healAmount, _player.StatsManager.MaxHealth * 0.4);
				
				_player.TakeDamage(healAmount, DamageType.Healing);
				
				GlobalEventBus.Instance.Publish(
					GlobalEventVariant.BoardcastTitle,
					$"Restored {healAmount:F0} health!");
			}
		});
		
		// 2. Time Distortion - Gamble with a chance for benefit or detriment
		_availableEvents.Add(new EventData
		{
			ID = "time_distortion",
			Title = "Time Distortion",
			Description = "A mysterious time rift pulses with energy. Interacting with it could " +
						"provide a boost to your abilities, or it might harm you.",
			MinLevel = 1,
			Button1Text = "Interact with the rift",
			Button2Text = "Leave it alone",
			OnOption1Selected = () =>
			{
				// 70% chance of positive outcome, 30% negative
				if (GD.Randf() < 0.7f)
				{
					// Positive outcome: Reduce ability cooldowns
					var abilities = _player.AbilityManager.GetAllAbilities();
					foreach (var ability in abilities)
					{
						ability.ReduceCooldown(ability.Cooldown * 0.5);
					}
					
					GlobalEventBus.Instance.Publish(
						GlobalEventVariant.BoardcastTitle,
						"The rift's energy reduces your ability cooldowns!");
				}
				else
				{
					// Negative outcome: Take damage
					var damage = _player.StatsManager.MaxHealth * 0.2;
					_player.TakeDamage(damage, DamageType.Normal);
					
					GlobalEventBus.Instance.Publish(
						GlobalEventVariant.BoardcastTitle,
						$"The rift's energy damages you for {damage:F0} health!");
				}
			},
			OnOption2Selected = () =>
			{
				GlobalEventBus.Instance.Publish(
					GlobalEventVariant.BoardcastTitle,
					"You wisely leave the rift alone.");
				
				// Small reward for being cautious
				_player.CurrencyManager.AddChronoshards(5);
			}
		});
		
		// 3. Temporal Sacrifice - Trade health for currency
		_availableEvents.Add(new EventData
		{
			ID = "temporal_sacrifice",
			Title = "Temporal Sacrifice",
			Description = "A forgotten altar dedicated to Chronos stands before you. It demands a " +
						"sacrifice of your life essence in exchange for valuable chronoshards.",
			MinLevel = 2,
			Button1Text = "Make the sacrifice",
			Button2Text = "Refuse",
			OnOption1Selected = () =>
			{
				// Take 30% of current health as damage
				var health = _player.StatsManager.Health;
				var sacrifice = health * 0.3;
				_player.TakeDamage(sacrifice, DamageType.Normal);
				
				// Award chronoshards based on sacrifice
				var reward = (int)(sacrifice * 0.5);
				_player.CurrencyManager.AddChronoshards(reward);
				
				GlobalEventBus.Instance.Publish(
					GlobalEventVariant.BoardcastTitle,
					$"Sacrificed {sacrifice:F0} health for {reward} chronoshards!");
			},
			OnOption2Selected = () =>
			{
				GlobalEventBus.Instance.Publish(
					GlobalEventVariant.BoardcastTitle,
					"You refuse to make the sacrifice.");
			}
		});
		
		// 4. Timeline Intersection - A more strategic event with risk/reward
		_availableEvents.Add(new EventData
		{
			ID = "timeline_intersection", 
			Title = "Timeline Intersection",
			Description = "You've encountered a point where multiple timelines intersect. You can attempt to " +
						"glimpse an alternate future, but doing so carries a risk of psychic feedback that could harm you.",
			MinLevel = 3,
			Button1Text = "Glimpse the future",
			Button2Text = "Avoid the risk",
			OnOption1Selected = () =>
			{
				// Roll for outcome
				if (GD.Randf() < 0.6f)  // 60% success chance
				{
					// Success - reset all ability cooldowns
					var abilities = _player.AbilityManager.GetAllAbilities();
					foreach (var ability in abilities)
					{
						// Completely reset cooldown
						ability.ReduceCooldown(ability.CurrentCooldown);
					}
					
					GlobalEventBus.Instance.Publish(
						GlobalEventVariant.BoardcastTitle,
						"You glimpse the future and your abilities are refreshed!");
				}
				else
				{
					// Failure - take moderate damage
					var damage = _player.StatsManager.MaxHealth * 0.4;
					_player.TakeDamage(damage, DamageType.Normal);
					
					GlobalEventBus.Instance.Publish(
						GlobalEventVariant.BoardcastTitle,
						$"The psychic feedback deals {damage:F0} damage!");
				}
			},
			OnOption2Selected = () =>
			{
				// Small reward for caution
				_player.CurrencyManager.AddChronoshards(5);
				
				GlobalEventBus.Instance.Publish(
					GlobalEventVariant.BoardcastTitle,
					"You wisely avoid the risk of timeline interference.");
			}
		});
		
		// 5. Chronomancer's Challenge - A skill test
		_availableEvents.Add(new EventData
		{
			ID = "chronomancer_challenge",
			Title = "Chronomancer's Challenge",
			Description = "An ancient chronomancer offers you a challenge: survive a brief assault of " +
						"time-displaced attacks. Success will bring a valuable reward.",
			MinLevel = 4,
			Button1Text = "Accept the challenge",
			Button2Text = "Decline the challenge",
			OnOption1Selected = () =>
			{
				// Determine outcome based on player's current health percentage
				// This simulates the player surviving a challenge
				var healthPercentage = _player.StatsManager.Health / _player.StatsManager.MaxHealth;
				
				if (healthPercentage > 0.5 || GD.Randf() < healthPercentage + 0.3)  // Higher chance with more health
				{
					// Success - give substantial reward
					var reward = 30 + DungeonManager.Instance.Level * 5;
					_player.CurrencyManager.AddChronoshards(reward);
					
					// Also partially heal the player
					var healAmount = _player.StatsManager.MaxHealth * 0.2;
					_player.TakeDamage(healAmount, DamageType.Healing);
					
					GlobalEventBus.Instance.Publish(
						GlobalEventVariant.BoardcastTitle,
						$"You succeed! Gained {reward} chronoshards and some health!");
				}
				else
				{
					// Failure - take significant damage
					var damage = _player.StatsManager.MaxHealth * 0.6;
					_player.TakeDamage(damage, DamageType.Normal);
					
					// Small consolation prize
					_player.CurrencyManager.AddChronoshards(10);
					
					GlobalEventBus.Instance.Publish(
						GlobalEventVariant.BoardcastTitle,
						$"You failed the challenge and took {damage:F0} damage!");
				}
			},
			OnOption2Selected = () =>
			{
				GlobalEventBus.Instance.Publish(
					GlobalEventVariant.BoardcastTitle,
					"You decline the chronomancer's challenge and move on.");
			}
		});
	}
	
	#endregion
	
	/// <summary>
	///     Data structure for event information
	/// </summary>
	private class EventData
	{
		public string ID { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public int MinLevel { get; set; } = 1;
		public string Button1Text { get; set; }
		public string Button2Text { get; set; }
		public Action OnOption1Selected { get; set; }
		public Action OnOption2Selected { get; set; }
	}
}
