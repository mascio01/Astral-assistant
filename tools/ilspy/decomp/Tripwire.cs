using UnityEngine;

public class Tripwire : Prop, ITrap
{
	private bool IsTripped;

	private static PrefabResource[] UnityTripwire = new PrefabResource[2]
	{
		new PrefabResource("Prefabs\\Props\\Trap\\Tripwire", 10),
		new PrefabResource("Prefabs\\Props\\Trap\\Tripwire_Tripped", 10)
	};

	private static Vector3 PipeBombPos = new Vector3(1.5f, 0.5f, 0f);

	public virtual Vector2 WheelPos
	{
		get
		{
			if (Prototype == null)
			{
				return Vector2.zero;
			}
			return Prototype.WheelPos;
		}
	}

	public virtual Vector2 WheelOffset
	{
		get
		{
			if (Prototype == null)
			{
				return Vector2.zero;
			}
			return Prototype.WheelOffset;
		}
	}

	public virtual float ExtraFrontWheelSeparation
	{
		get
		{
			if (Prototype == null)
			{
				return 0f;
			}
			return Prototype.ExtraFrontWheelSeparation;
		}
	}

	public virtual float MaxYaw
	{
		get
		{
			if (Prototype == null)
			{
				return 15f;
			}
			return Prototype.MaxYaw;
		}
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Tripwire;
	}

	public override PrefabResource GetUnityModel()
	{
		if (Prototype != null)
		{
			if (IsTripped && Prototype.Prefabs.Count >= 2)
			{
				return Prototype.Prefabs[1];
			}
			if (Prototype.Prefabs.Count >= 1)
			{
				return Prototype.Prefabs[0];
			}
		}
		return UnityTripwire[IsTripped ? 1 : 0];
	}

	public override bool SupportsVariation()
	{
		return false;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.AddAfter(ref IsTripped, 166);
	}

	public void TriggerTrap(Character character)
	{
		bool num = IsUnityObjectActive();
		if (num)
		{
			UnityDeactivate();
		}
		UnityDelete();
		IsTripped = true;
		DamagePoints.Clear();
		Damage = 0f;
		UnityInit();
		if (num)
		{
			UnityActivate();
		}
		Character source = GetCommunity()?.GetNearestLivingNonZombieMember(Tile, null, BaseObjectType.Human, float.MaxValue);
		PipeBombProjectile.Splosion(GetPipeBombPos(), this, source, this, null, Prototype.TrapDamage, SkillType.Invalid, InfectionType.None, Prototype.TrapDamageRadius, predicted: false, frontmost: true, SecrecyMode.Public, itsATrap: true, isFromAI: false);
	}

	public void ResetTrap(Character character, bool isGathering)
	{
		bool num = IsUnityObjectActive();
		if (num)
		{
			UnityDeactivate();
		}
		UnityDelete();
		IsTripped = false;
		DamagePoints.Clear();
		Damage = 0f;
		UnityInit();
		if (num)
		{
			UnityActivate();
		}
	}

	public bool CanTriggerTrap(Character character)
	{
		return !IsTripped;
	}

	public bool CanResetTrap()
	{
		if (UnderConstructionInfo == null)
		{
			return IsTripped;
		}
		return false;
	}

	public EquipmentPrototype GetEquipmentNeededForReset()
	{
		return EquipmentPrototype.PipeBomb;
	}

	public override bool IsFriendlyFire(Character source, TileObject target, bool itsATrap, AttackType meleeAttackType)
	{
		return false;
	}

	public override Flammability GetFlammability()
	{
		if (IsTripped)
		{
			return Flammability.Invulnerable;
		}
		return base.GetFlammability();
	}

	public Vector3 GetPipeBombPos()
	{
		return World.MultiplyPoint(PipeBombPos);
	}

	public override void OnDestroyed(Character source, bool from_impact, Vector3 force)
	{
		if (!IsTripped)
		{
			TriggerTrap(null);
		}
	}

	public override bool IsImpassable(Character requester, int options, TerrainCoord tile)
	{
		if ((options & 0x80) != 0)
		{
			return true;
		}
		return base.IsImpassable(requester, options, tile);
	}

	public override bool IsTargetable()
	{
		if (Community != Session.Instance.CommunityManager.PlayerCommunity && !PropEditor.AllowTargetingAllProps)
		{
			return Session.Instance.Editor;
		}
		return true;
	}

	public override bool IsTiltableProp()
	{
		return true;
	}

	public override Matrix4x4 GetCustomModelTransform()
	{
		return TiltedProp.CalcModelTransform(GetUnityModel(), WheelOffset, WheelPos, ExtraFrontWheelSeparation, Tile, MaxYaw, World, ModelOffset, Id != 0 || this == Hud.GetGhostBuilding(), DemolitionTransition);
	}

	public override void OnPostRender()
	{
		base.OnPostRender();
		if (PropEditor.ShowWheelPositions)
		{
			TiltedProp.DebugDrawWheels(WheelOffset, WheelPos, ExtraFrontWheelSeparation, Tile, MaxYaw, World, ModelOffset);
		}
	}
}
