public class RetreatToBaseGoal : StateMachineGoal
{
	public Gate GateToRunInside;

	private static float MaxDistFromBase = 32f;

	public override GoalType GetGoalType()
	{
		return GoalType.RetreatToBaseGoal;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Attack_RetreatToBase;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref GateToRunInside);
	}

	public bool IsOutsideNearbyBase(Character character, float maxDist, out Gate bestGate)
	{
		bestGate = null;
		float num = maxDist * maxDist;
		Human targetHuman = GetTargetHuman();
		if (targetHuman == null)
		{
			return false;
		}
		if (character.Community == null)
		{
			return false;
		}
		if (character.InsideBuilding != null)
		{
			return false;
		}
		GameTerrain instance = GameTerrain.Instance;
		TerrainCoord tile = character.Tile;
		if (instance.IsTileEnclosedOrBuiltOn(tile))
		{
			return false;
		}
		TerrainCoord tileCoordForPos = instance.GetTileCoordForPos(Target.LastKnownPosition);
		if (instance.IsTileEnclosedOrBuiltOn(tileCoordForPos))
		{
			return false;
		}
		if (character.SquadLeader != null && !instance.IsTileEnclosedOrBuiltOn(character.SquadLeader.Tile))
		{
			return false;
		}
		if (character.SparringPartner == targetHuman && (character.SparringType == SparringType.Feuding || character.SparringType == SparringType.FightToTheDeath || character.IsControllableByOrFollowingPlayer()))
		{
			return false;
		}
		foreach (Prop building in character.Community.Buildings)
		{
			if (!(building is Gate { GatePolicy: GatePolicy.OpenableByFriendsExceptWhenInsideAndUnderAttack } gate) || gate.GetPropPrototype().CoverType < CoverType.Full)
			{
				continue;
			}
			TerrainCoord tileInsideGate = gate.GetTileInsideGate();
			if (instance.IsTileEnclosed(tileInsideGate.x, tileInsideGate.y) && !instance.IsImpassable(tileInsideGate.x, tileInsideGate.y, 0, character, null))
			{
				float distSquared = tileInsideGate.GetDistSquared(tile);
				if (distSquared < num)
				{
					num = distSquared;
					bestGate = gate;
				}
			}
		}
		return bestGate != null;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		Attack attack = parent as Attack;
		if (!attack._dontOpenOurGates)
		{
			return false;
		}
		if (character.IsControllableByPlayer() && attack.PreferredTarget != null)
		{
			return false;
		}
		if (Active)
		{
			return true;
		}
		if (!IsOutsideNearbyBase(character, MaxDistFromBase, out var _))
		{
			return false;
		}
		return true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (IsOutsideNearbyBase(character, MaxDistFromBase, out var bestGate))
		{
			GateToRunInside = bestGate;
			SetSubGoal(character, parent, new MoveTo(MovementType.Run, bestGate.GetTileInsideGate(), dontOpenOurGates: true));
		}
		else
		{
			Finished = true;
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveTo { Success: not false } && GateToRunInside != null && !GateToRunInside.IsDestroyed() && !IsAnyoneElseRunningToGate(character))
		{
			Target.SetFlag(TargetFlags.OutsideBase, on: true);
			return new TurnTo(GateToRunInside.Tile);
		}
		if (SubGoal is TurnTo && GateToRunInside != null && !GateToRunInside.IsDestroyed() && !IsAnyoneElseRunningToGate(character))
		{
			return new CloseGateGoal(character, GateToRunInside);
		}
		return base.GetNextSubGoal(character, parent);
	}

	private bool IsAnyoneElseRunningToGate(Character character)
	{
		if (character.Community != null)
		{
			foreach (Character member in character.Community.Members)
			{
				if (member != character && member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human && member.FindActiveGoal(GoalType.RetreatToBaseGoal) is RetreatToBaseGoal retreatToBaseGoal && retreatToBaseGoal.GateToRunInside == GateToRunInside)
				{
					return true;
				}
			}
		}
		return false;
	}
}
