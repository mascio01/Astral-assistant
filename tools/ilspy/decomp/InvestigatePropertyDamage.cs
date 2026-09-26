using System;
using UnityEngine;

public class InvestigatePropertyDamage : StateMachineGoal
{
	public int InvestigateCount;

	public int InvestigateRecordId;

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref InvestigateCount);
		reflector.Add(ref InvestigateRecordId);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.InvestigatePropertyDamage;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Survivor_InvestigatePropertyDamage;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.AlertIcon;
	}

	public override bool IsLowAlert(Character character)
	{
		return true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		int num = ((InvestigateRecordId != 0 && !character.Community.IsPropertyDamageInvestigationFinished(InvestigateRecordId)) ? InvestigateRecordId : FindPropertyDamageRecordToInvestigate(character, ignoreSightRange: true));
		if (InvestigateRecordId == 0 || InvestigateRecordId != num)
		{
			InvestigateRecordId = num;
			InvestigateCount = 0;
			TerrainRect propertyDamageRegion = character.Community.GetPropertyDamageRegion(InvestigateRecordId);
			if (character.IsGuarding() || propertyDamageRegion == TerrainRect.Invalid)
			{
				SetSubGoal(character, parent, GetShoutGoal(character));
			}
			else
			{
				SetSubGoal(character, parent, new MoveAsCloseAsPossibleTo(MovementType.Run, propertyDamageRegion.Centre, float.MaxValue, AlertGoal.CalcDontOpenOurGates(character)));
			}
		}
		else
		{
			SetSubGoal(character, parent, GetNextInvestigateGoal(character, parent));
		}
	}

	private Goal GetShoutGoal(Character character)
	{
		TerrainRect propertyDamageRegion = character.Community.GetPropertyDamageRegion(InvestigateRecordId);
		if (character.IsAuthoritative())
		{
			Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, null, SpeechSituation.SeePropertyDamage);
			if (speechForSituation != null)
			{
				character.Speak(speechForSituation, null);
			}
		}
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		return new WaitAndFace(propertyDamageRegion.Centre, TimeSpan.FromSeconds(Mathf.Lerp(2f, 4f, MathUtil.RandomFloat((float)currentTime.TotalSeconds + (float)character.Id))));
	}

	private bool IsUrgent(Character character)
	{
		return character.Community.GetTimeSinceLastPropertyDamaged(InvestigateRecordId) < TimeSpan.FromSeconds(60.0);
	}

	private Goal GetNextInvestigateGoal(Character character, Goal parent)
	{
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		TerrainRect propertyDamageRegion = character.Community.GetPropertyDamageRegion(InvestigateRecordId);
		TerrainCoord terrainCoord = TerrainCoord.Invalid;
		if (propertyDamageRegion != TerrainRect.Invalid)
		{
			TerrainCoord terrainCoord2 = new TerrainCoord(InvestigateCount, InvestigateCount) * 8;
			terrainCoord = MathUtil.RandomTile((int)currentTime.Ticks + character.Id, propertyDamageRegion.min - terrainCoord2, propertyDamageRegion.max + terrainCoord2);
			terrainCoord = GameTerrain.Instance.ClampTileWithinBounds(terrainCoord);
		}
		if (character.IsGuarding() || character.HasPersonality(CachedPersonalityType.Nervous) || terrainCoord == TerrainCoord.Invalid)
		{
			return new WaitAndFace(terrainCoord, TimeSpan.FromSeconds(Mathf.Lerp(2f, 8f, MathUtil.RandomFloat((float)currentTime.TotalSeconds + (float)character.Id))));
		}
		return new MoveAsCloseAsPossibleTo((!IsUrgent(character)) ? MovementType.Walk : MovementType.Run, terrainCoord, float.MaxValue, AlertGoal.CalcDontOpenOurGates(character));
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveAsCloseAsPossibleTo && InvestigateCount == 0)
		{
			return GetShoutGoal(character);
		}
		InvestigateCount++;
		if (InvestigateCount >= (IsUrgent(character) ? 15 : 5))
		{
			character.SetRecentActivity(RecentActivityType.HighAlert, null);
			character.Community.SetPropertyDamageInvestigated(InvestigateRecordId);
			InvestigateRecordId = 0;
			InvestigateCount = 0;
			return null;
		}
		return GetNextInvestigateGoal(character, parent);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.IsControllableByPlayer())
		{
			return false;
		}
		if (character.Community == null)
		{
			return false;
		}
		if (character.Rank == Rank.Captive)
		{
			return false;
		}
		if (InvestigateRecordId != 0 && !character.Community.IsPropertyDamageInvestigationFinished(InvestigateRecordId))
		{
			return true;
		}
		return FindPropertyDamageRecordToInvestigate(character) != 0;
	}

	public int FindPropertyDamageRecordToInvestigate(Character character, bool ignoreSightRange = false)
	{
		int result = 0;
		float num = float.MaxValue;
		if (character.Community != null && character.Community.PropertyDamageRecords.Count > 0)
		{
			int sightRange = character.GetSightRange();
			for (int i = 0; i < character.Community.PropertyDamageRecords.Count; i++)
			{
				if (character.Community.PropertyDamageRecords[i].HasAssignedBlame || character.Community.PropertyDamageRecords[i].Investigated)
				{
					continue;
				}
				float closestDistSqTo = character.Community.PropertyDamageRecords[i].Region.GetClosestDistSqTo(character.Tile);
				if (ignoreSightRange || !(closestDistSqTo > MathUtil.Squared(character.Community.PropertyDamageRecords[i].GetVisibleFromDist(sightRange))))
				{
					float num2 = (float)(Session.Instance.PlayTime - character.Community.PropertyDamageRecords[i].LastDamagedTime).TotalSeconds;
					float num3 = Mathf.Sqrt(closestDistSqTo) + num2;
					if (num3 < num)
					{
						num = num3;
						result = character.Community.PropertyDamageRecords[i].PropertyDamageRecordId;
					}
				}
			}
		}
		return result;
	}
}
