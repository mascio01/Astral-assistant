using System;
using System.Collections.Generic;
using UnityEngine;

public class InvaderInstance : IReflectable
{
	public Invader Invader;

	public BaseObject SourceObject;

	public List<BaseObject> CreatedObjects = new List<BaseObject>();

	public TimeSpan LastSpawnedTime = Target.Never;

	public TimeSpan TriggeredTime = Target.Never;

	public bool Active = true;

	public float TimeoutLerp;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Invader);
		reflector.Add(ref SourceObject);
		reflector.AddGameObjectRefList(ref CreatedObjects);
		reflector.AddAfter(ref LastSpawnedTime, 251);
		reflector.AddAfter(ref Active, 280);
		if (reflector.Version >= 280)
		{
			reflector.Add(ref TriggeredTime);
		}
		else
		{
			TriggeredTime = Session.Instance.PlayTime;
		}
		if (reflector.Version >= 281)
		{
			reflector.AddAfter(ref TimeoutLerp, 281);
		}
		else
		{
			TimeoutLerp = MathUtil.RandomFloat((SourceObject != null) ? SourceObject.Id : 0);
		}
	}

	public BaseObject SpawnInvader()
	{
		LastSpawnedTime = Session.Instance.PlayTime;
		Template template = GameImpl.Instance.FindTemplateByUniqueID(Invader.TemplateID);
		if (template != null)
		{
			List<CommunityManager.TileAndRadius> cachedPlayerCommunityTiles = null;
			TerrainCoord tile = TerrainCoord.Invalid;
			if (SourceObject != null)
			{
				tile = SourceObject.GetTile();
			}
			BaseObject baseObject = template.SpawnFromTemplate(tile, canBeEnclosed: true, null, null, null, null, null, SourceObject, new MemoryParam(SourceObject), 1, this, ref cachedPlayerCommunityTiles);
			if (baseObject != null)
			{
				StoryManager.QueueEvents(Invader.OnSpawnedEvents, null, null, baseObject, new MemoryParam(SourceObject));
				CreatedObjects.Add(baseObject);
				return baseObject;
			}
		}
		return null;
	}

	public string GetDisplayNameString()
	{
		return Invader.GetDescriptionString(SourceObject, this);
	}

	public TimeSpan CalcCooldown()
	{
		DifficultySettings difficultySettings = Session.Instance.DifficultySettings;
		float num = Math.Max(0.1f, difficultySettings.GetDifficultySetting(Invader.DifficultySetting));
		return TimeSpan.FromSeconds(Sun.DayLengthSecs * Mathf.Lerp(Invader.MinCooldownDays, Invader.MaxCooldownDays, MathUtil.RandomFloat((float)LastSpawnedTime.TotalMilliseconds)) / num);
	}

	public TimeSpan CalcTimeout()
	{
		return TimeSpan.FromSeconds(Sun.DayLengthSecs * Mathf.Lerp(Invader.MinTimeoutDays, Invader.MaxTimeoutDays, TimeoutLerp));
	}

	public bool ShowOnMapSidebar()
	{
		if (Active)
		{
			return true;
		}
		foreach (BaseObject createdObject in CreatedObjects)
		{
			Community community = createdObject.GetCommunity();
			if (community != null)
			{
				if (community.HasAnyActiveMembers())
				{
					return true;
				}
				continue;
			}
			return true;
		}
		return false;
	}
}
