using System.Collections.Generic;

public class CheckPathToBuildingEntrance : Goal
{
	public int EntranceIndex;

	public bool DontOpenOurGates;

	public bool AvoidHostileBases;

	public bool Success;

	private AStarRequester Requester;

	public CheckPathToBuildingEntrance()
	{
	}

	public CheckPathToBuildingEntrance(int entranceIndex, bool dontOpenOurGates, bool avoidHostileBases)
	{
		EntranceIndex = entranceIndex;
		DontOpenOurGates = dontOpenOurGates;
		AvoidHostileBases = avoidHostileBases;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.CheckPathToBuildingEntrance;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref EntranceIndex);
		reflector.Add(ref DontOpenOurGates);
		reflector.Add(ref AvoidHostileBases);
		reflector.Add(ref Success);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (Finished)
		{
			return;
		}
		if (!character.IsAuthoritative())
		{
			Success = true;
			Finished = true;
		}
		else if (Requester == null)
		{
			Building targetBuilding = GetTargetBuilding();
			if (targetBuilding != null)
			{
				TerrainCoord entranceTile = targetBuilding.GetEntranceTile(EntranceIndex);
				TerrainCoord start = character.Tile;
				if (character.InsideBuilding != null)
				{
					start = character.InsideBuilding.GetEntranceTile(character.InsideBuilding.GetClosestEntranceTo(entranceTile));
				}
				if (GameTerrain.Instance.IsTileOutsideBounds(start.x, start.y) || GameTerrain.Instance.IsTileOutsideBounds(entranceTile.x, entranceTile.y))
				{
					Success = true;
					Finished = true;
					return;
				}
				List<TerrainCoord> list = new List<TerrainCoord>();
				for (int i = 0; i < targetBuilding.GetEntranceDefs().Length; i++)
				{
					list.Add(targetBuilding.GetEntranceTile(i));
				}
				Requester = new AStarRequester();
				Requester.StartBuildingEntrancesRequest(start, entranceTile, list, character, null, AStarPriority.Unknown, DontOpenOurGates, respectMovementZone: true, AvoidHostileBases, 0);
			}
			else
			{
				Finished = true;
			}
		}
		else
		{
			switch (Requester.Result)
			{
			case AStarResult.Success:
				Requester = null;
				Success = true;
				Finished = true;
				break;
			case AStarResult.Fail:
				Requester = null;
				Success = false;
				Finished = true;
				break;
			}
		}
	}
}
