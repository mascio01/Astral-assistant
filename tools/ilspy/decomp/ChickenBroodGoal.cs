using System;

public class ChickenBroodGoal : StateMachineGoal
{
	private TimeSpan LastAttemptedTime;

	public static GameProfiler _Timer = new GameProfiler("Update.ChickenBroodGoalSearch");

	public override GoalType GetGoalType()
	{
		return GoalType.ChickenBroodGoal;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Animal_Brood;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		Chicken chicken = (Chicken)character;
		if (!chicken.IsBroody())
		{
			return false;
		}
		if (Session.Instance.PlayTime - LastAttemptedTime < TimeSpan.FromSeconds(10.0))
		{
			return false;
		}
		if (SubGoal is Idle)
		{
			if (character.InsideBuilding == null)
			{
				return false;
			}
			if (!CanBroodInBuilding(chicken, character.InsideBuilding, out var _))
			{
				return false;
			}
		}
		return true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		CharacterManager.DeterministicTimeSpentThinking += CharacterManager.ThinkTimeUnit;
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		Chicken chicken = character as Chicken;
		TerrainCoord tile = character.Tile;
		bool flag = character.IsOnPlayersTeam();
		float num = float.MaxValue;
		Building building = null;
		using (new ProfileMarker(_Timer))
		{
			foreach (Prop allProp in Session.Instance.PropManager.AllProps)
			{
				if (!(allProp is Building building2))
				{
					continue;
				}
				float num2 = building2.Tile.GetDist(tile);
				if (!(num2 >= num) && CanBroodInBuilding(chicken, building2, out var myEgg))
				{
					if (!myEgg)
					{
						num2 += 200f;
					}
					if (building2.Prototype.EnterableBySpecies != character.GetBaseObjectType())
					{
						num2 += 1000f;
					}
					if (building2.Community != character.Community)
					{
						num2 += 250f;
					}
					if ((!character.HasMovementZone() || character.MovementZone.Overlaps(building2.GetTileRect())) && (!flag || instance2.FogOfWar.IsAnyTileInRectCornersExplored(building2.MinTile, building2.MaxTile)))
					{
						building = building2;
						num = num2;
					}
				}
			}
		}
		if (building != null)
		{
			if (building.CanEnter(character) && !building.IsBurning())
			{
				SetSubGoal(character, parent, new MoveToAndEnterBuilding(character, building));
				return;
			}
			Finished = true;
			LastAttemptedTime = instance.PlayTime;
		}
		else
		{
			Finished = true;
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveToAndEnterBuilding { Success: not false })
		{
			return new Idle();
		}
		LastAttemptedTime = Session.Instance.PlayTime;
		return base.GetNextSubGoal(character, parent);
	}

	private bool CanBroodInBuilding(Chicken chicken, Building building, out bool myEgg)
	{
		myEgg = true;
		if (building.Inventory.FindFertilizedEgg(chicken) == null)
		{
			int num = building.Inventory.CountItemsOfClass(typeof(FertilizedEgg));
			if (num == 0)
			{
				return false;
			}
			int num2 = 0;
			Character[] inhabitants = building.Inhabitants;
			foreach (Character character in inhabitants)
			{
				if (character != null && character != chicken && character.FindActiveGoal(GoalType.ChickenBroodGoal) != null)
				{
					num2++;
				}
			}
			if (num2 >= num)
			{
				return false;
			}
			myEgg = false;
		}
		return true;
	}
}
