using System;
using System.Collections.Generic;
using UnityEngine;

public class InvisibleStrainSpreadGoal : StateMachineGoal
{
	private TerrainCoord StartTile;

	private Equipment PreviouslyEquippedItem;

	private static GameProfiler CalcBestTargetTimer = new GameProfiler("InvisibleStrainSpreadCalcBestTarget");

	public static int MinObserverRange = 16;

	private static List<Character> NearbyCharacters = new List<Character>();

	public override GoalType GetGoalType()
	{
		return GoalType.InvisibleStrainSpreadGoal;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		if (Target.GetFlag(TargetFlags.DeadOrZombie) && (SubGoal is MoveWithinRangeOfTarget || SubGoal is MoveDirectlyToTarget || SubGoal is BiteTarget))
		{
			return false;
		}
		return true;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Survivor_InvisibleStrainSpread;
	}

	public override Target CalcBestTarget(Character character, Goal parent)
	{
		using (new ProfileMarker(CalcBestTargetTimer))
		{
			if (character.InvisibleStrain == InvisibleStrainType.None)
			{
				return null;
			}
			if (Active)
			{
				if (SubGoal is MoveWithinRangeOfTarget)
				{
					foreach (PlayerRecord playerRecord in Session.Instance.PlayerRecords)
					{
						if ((playerRecord.SyncedCamFocusPosXZ - character.PosXZ).magnitude <= 24f)
						{
							return null;
						}
					}
				}
				return Target;
			}
			if (Session.Instance.PlayTime < character.InvisibleStrainNextSpread)
			{
				return null;
			}
			foreach (PlayerRecord playerRecord2 in Session.Instance.PlayerRecords)
			{
				if ((playerRecord2.SyncedCamFocusPosXZ - character.PosXZ).magnitude <= 32f)
				{
					return null;
				}
			}
			bool flag = character.IsControllableByPlayer();
			Target target = null;
			float num = MinObserverRange;
			int num2 = 0;
			bool flag2 = false;
			foreach (Target target3 in character.Targets)
			{
				if (target3.Object == null || target3.Object.Deleted || !(target3.Object is Human { IsAwake: not false, Zombie: false } human) || target3.GetFlag(TargetFlags.DeadOrZombie) || human.InvisibleStrain != InvisibleStrainType.None || !human.IsOutdoors() || character.IsEnemy(human) || !target3.FullyTracked)
				{
					continue;
				}
				bool flag3 = false;
				Target target2 = human.GetTarget(character);
				if (target2 != null && target2.FullyTracked)
				{
					num2++;
					flag3 = true;
					if (num2 >= 2)
					{
						return null;
					}
				}
				if ((flag || human.IsControllableByPlayer()) && human != character.DontInfectWithInvisibleStrain && human.InsideBuilding == null && !human.DirectControlled && !human.IsPlayerAvatar())
				{
					float magnitude = MathUtil.ToXZ(target3.LastKnownPosition - character.Pos).magnitude;
					if (magnitude < num)
					{
						target = target3;
						num = magnitude;
						flag2 = flag3;
					}
				}
			}
			if (flag2)
			{
				num2--;
			}
			if (num2 > 0)
			{
				return null;
			}
			if (target != null)
			{
				TerrainCoord tile = target.Object.GetTile();
				GameTerrain.Instance.CharacterMapWho.GetObjectsInRect(tile - new TerrainCoord(MinObserverRange, MinObserverRange), tile + new TerrainCoord(MinObserverRange, MinObserverRange), NearbyCharacters);
				foreach (Character nearbyCharacter in NearbyCharacters)
				{
					if (nearbyCharacter != character && nearbyCharacter != target.Object && nearbyCharacter.IsAwake && nearbyCharacter.InvisibleStrain == InvisibleStrainType.None && nearbyCharacter.IsOutdoors() && nearbyCharacter.GetBaseObjectType() == BaseObjectType.Human && !((Character)target.Object).IsEnemy(nearbyCharacter))
					{
						NearbyCharacters.Clear();
						return null;
					}
				}
				NearbyCharacters.Clear();
			}
			return target;
		}
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		StartTile = character.Tile;
		PreviouslyEquippedItem = character.EquippedItem;
		character.InvisibleStrainNextSpread = Session.Instance.PlayTime + TimeSpan.FromSeconds(Mathf.Lerp(30f, 60f, Session.Instance.DeterministicRand.RandomFloat()));
		SetSubGoal(character, parent, new MoveWithinRangeOfTarget(MovementType.Run, aiming: false, 0f, 1.25f, dontOpenOurGates: true));
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.AddAfter(ref StartTile, 404);
		reflector.AddAfter(ref PreviouslyEquippedItem, 404);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		CustomRandom deterministicRand = Session.Instance.DeterministicRand;
		Character targetCharacter = GetTargetCharacter();
		if (SubGoal is MoveWithinRangeOfTarget moveWithinRangeOfTarget)
		{
			if (moveWithinRangeOfTarget.Success && targetCharacter != null && targetCharacter.InsideBuilding == null)
			{
				return new MoveDirectlyToTarget(character, character.ShouldLimp() ? MovementType.Walk : MovementType.Run, 0.75f);
			}
			character.InvisibleStrainNextSpread = Session.Instance.PlayTime + TimeSpan.FromSeconds(Mathf.Lerp(Sun.DayLengthSecs * 0.5f, Sun.DayLengthSecs, deterministicRand.RandomFloat()));
			return new MoveAsCloseAsPossibleTo(MovementType.Run, StartTile);
		}
		if (SubGoal is MoveDirectlyToTarget moveDirectlyToTarget)
		{
			if (moveDirectlyToTarget.Success && targetCharacter != null && targetCharacter.InsideBuilding == null)
			{
				bool flag = character.Inventory.HasAnyGuns(character, withAmmo: true);
				bool flag2 = character.Inventory.GetHuntingKnife() != null;
				if ((flag || flag2) && deterministicRand.RandomChoice(0.5f))
				{
					if (flag)
					{
						return new RangedAttack(MovementType.Run, assassinate: true, SecrecyMode.Private, dontOpenOurGates: false, default(StayInRangeParams));
					}
					return new MoveToAndChokeHold(character, targetCharacter, MovementType.Run, HoldType.SlitThroat, assassinate: true, SecrecyMode.Private);
				}
				return new BiteTarget(fromJumping: false);
			}
			character.InvisibleStrainNextSpread = Session.Instance.PlayTime + TimeSpan.FromSeconds(Mathf.Lerp(Sun.DayLengthSecs * 0.25f, Sun.DayLengthSecs * 0.5f, deterministicRand.RandomFloat()));
			return new MoveAsCloseAsPossibleTo(MovementType.Run, StartTile);
		}
		if (SubGoal is MoveToAndChokeHold { Success: false } && targetCharacter != null && character.GetFatigue() < 0.9f)
		{
			return new MoveToAndChokeHold(character, targetCharacter, MovementType.Run, (character.Inventory.GetHuntingKnife() != null) ? HoldType.SlitThroat : HoldType.ChokeHold, assassinate: true, SecrecyMode.Private);
		}
		if ((SubGoal is BiteTarget || SubGoal is RangedAttack || SubGoal is MoveToAndChokeHold) && character.EquippedItem != PreviouslyEquippedItem && (PreviouslyEquippedItem == null || character.InventoryContains(PreviouslyEquippedItem)))
		{
			return new Equip(PreviouslyEquippedItem);
		}
		if (SubGoal is BiteTarget || SubGoal is RangedAttack || SubGoal is Equip || SubGoal is MoveToAndChokeHold)
		{
			character.InvisibleStrainNextSpread = Session.Instance.PlayTime + TimeSpan.FromSeconds(Mathf.Lerp(Sun.DayLengthSecs, Sun.DayLengthSecs * 2f, deterministicRand.RandomFloat()));
			if (targetCharacter != null)
			{
				targetCharacter.InvisibleStrainNextSpread = Session.Instance.PlayTime + TimeSpan.FromSeconds(Mathf.Lerp(Sun.DayLengthSecs, Sun.DayLengthSecs * 2f, deterministicRand.RandomFloat()));
			}
			TerrainCoord destTile = StartTile;
			if (!(SubGoal is BiteTarget) && targetCharacter != null && destTile.GetDist(targetCharacter.Tile) < (float)MinObserverRange)
			{
				List<TerrainCoord> list = new List<TerrainCoord>();
				foreach (Prop building2 in character.Community.Buildings)
				{
					list.Add(deterministicRand.RandomTileOnOutsideEdge(building2.GetMinTile(), building2.GetMaxTile(), MinObserverRange));
				}
				foreach (Character member in character.Community.Members)
				{
					if (member.AliveAndNotZombie && member.GetPlayerControllingMe() == null)
					{
						list.Add(GameTerrain.Instance.GetTileCoordForPosXZ(member.PosXZ + MathUtil.GetDirFromAngle(deterministicRand.RandomFloat() * (MathF.PI * 2f)) * MinObserverRange));
					}
				}
				while (list.Count > 0)
				{
					int index = deterministicRand.Next(list.Count);
					TerrainCoord terrainCoord = list[index];
					list.RemoveAt(index);
					if (terrainCoord.GetDist(character.Tile) < 128f && !GameTerrain.Instance.IsImpassable(terrainCoord.x, terrainCoord.y, 1, character, null))
					{
						destTile = terrainCoord;
						break;
					}
				}
			}
			Building building = GameTerrain.Instance.GetBuilding(destTile.x, destTile.y);
			if (building != null && building.CanEnter(character))
			{
				return new MoveToAndEnterBuilding(character, building, MovementType.Run);
			}
			return new MoveAsCloseAsPossibleTo(MovementType.Run, destTile);
		}
		return base.GetNextSubGoal(character, parent);
	}
}
