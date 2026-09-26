using System;
using UnityEngine;

public struct RoleInfo : IReflectable
{
	public const int MaxRoles = 10;

	public Role Role;

	public EquipmentPrototype ResourceType;

	public Recipe Recipe;

	public TerrainCoord TargetLocation;

	public bool Urgent;

	public bool Paused;

	public bool FailedRecently;

	public TimeSpan LastFailedTime;

	public bool Valid => Role != Role.None;

	public RoleInfo(Role role)
	{
		Role = role;
		ResourceType = null;
		Recipe = null;
		TargetLocation = TerrainCoord.Invalid;
		Urgent = false;
		Paused = false;
		FailedRecently = false;
		LastFailedTime = Target.Never;
	}

	public RoleInfo(Role role, Recipe recipe, TerrainCoord targetLocation)
	{
		Role = role;
		ResourceType = null;
		Recipe = recipe;
		TargetLocation = targetLocation;
		Urgent = false;
		Paused = false;
		FailedRecently = false;
		LastFailedTime = Target.Never;
	}

	public RoleInfo(Role role, TerrainCoord targetLocation)
	{
		Role = role;
		ResourceType = null;
		Recipe = null;
		TargetLocation = targetLocation;
		Urgent = false;
		Paused = false;
		FailedRecently = false;
		LastFailedTime = Target.Never;
	}

	public RoleInfo(Role role, EquipmentPrototype resourceType)
	{
		Role = role;
		ResourceType = resourceType;
		Recipe = null;
		TargetLocation = TerrainCoord.Invalid;
		Urgent = false;
		Paused = false;
		FailedRecently = false;
		LastFailedTime = Target.Never;
	}

	public RoleInfo(Role role, TerrainCoord targetLocation, EquipmentPrototype resourceType)
	{
		Role = role;
		ResourceType = resourceType;
		Recipe = null;
		TargetLocation = targetLocation;
		Urgent = false;
		Paused = false;
		FailedRecently = false;
		LastFailedTime = Target.Never;
	}

	public RoleInfo(Role role, TerrainCoord targetLocation, EquipmentPrototype resourceType, Recipe recipe)
	{
		Role = role;
		ResourceType = resourceType;
		Recipe = recipe;
		TargetLocation = targetLocation;
		Urgent = false;
		Paused = false;
		FailedRecently = false;
		LastFailedTime = Target.Never;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Role);
		reflector.Add(ref ResourceType);
		reflector.Add(ref Recipe);
		reflector.Add(ref TargetLocation);
		reflector.Add(ref Urgent);
		reflector.Add(ref Paused);
		reflector.Add(ref FailedRecently);
		reflector.AddAfter(ref LastFailedTime, 616);
	}

	public bool Equals(RoleInfo other)
	{
		if (Role == other.Role)
		{
			switch (Role)
			{
			case Role.Gatherer:
				if (ResourceType == other.ResourceType)
				{
					return TargetLocation == other.TargetLocation;
				}
				return false;
			case Role.Miner:
				return ResourceType == other.ResourceType;
			case Role.Capturing:
				return TargetLocation == other.TargetLocation;
			case Role.Crafter:
				if (Recipe == other.Recipe)
				{
					return TargetLocation == other.TargetLocation;
				}
				return false;
			default:
				return true;
			}
		}
		return false;
	}

	public Color GetRoleIconCol()
	{
		if (!Paused)
		{
			if (!Urgent)
			{
				return Color.white;
			}
			return GameTerrain.MinimapSettings.UrgentCol;
		}
		return Color.gray;
	}

	public bool IsInFailedCooldown()
	{
		Role role = Role;
		if (role == Role.Crafter || role == Role.Cook)
		{
			return Session.Instance.PlayTime - LastFailedTime < CraftGoal.MinTimeBetweenAttempts;
		}
		return false;
	}
}
