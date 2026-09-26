using System;
using UnityEngine;

public class SingleTileProp : SingleTileObject
{
	public PropPrototype Prototype;

	public float Damage;

	public float Fuel;

	public bool Burning;

	protected DeletionState _deleted;

	private Matrix4x4 CachedWorldTrans = Matrix4x4.zero;

	public GameObject UnityObj;

	public GameObject UnityBurningEffect;

	public int CommunityId;

	public UnderConstructionInfo UnderConstructionInfo;

	private static string SingleTilePropRaycastStr = "SingleTilePropRaycast";

	public static float MinDamageToShowHealthBar = 0.5f;

	public override int NameHash
	{
		get
		{
			if (Prototype == null)
			{
				return base.NameHash;
			}
			return Prototype.NameHash;
		}
	}

	public override string Category
	{
		get
		{
			if (Prototype == null)
			{
				return string.Empty;
			}
			return Prototype.Category;
		}
	}

	public override int UnityModelIndexFromPrototype => MathUtil.RandomInt(Id, Prototype.Prefabs.Count);

	public override bool Deleted => _deleted == DeletionState.Deleted;

	public override Color32 MapColor
	{
		get
		{
			if (GetGrabbableEquipmentType() == null)
			{
				if (!IsImpassableSingleTileProp())
				{
					return MathUtil.TransparentBlack;
				}
				return GameTerrain.MinimapSettings.BoulderCol;
			}
			return GameTerrain.MinimapSettings.ArrowCol;
		}
	}

	public override CoverType CoverType
	{
		get
		{
			if (Prototype == null)
			{
				return base.CoverType;
			}
			return Prototype.CoverType;
		}
	}

	public virtual Vector3 ModelOffset
	{
		get
		{
			if (Prototype == null || Prototype.ModelOffset.Count <= 0)
			{
				return Vector3.zero;
			}
			return Prototype.ModelOffset[Math.Min(UnityModelIndexFromPrototype, Prototype.ModelOffset.Count - 1)];
		}
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.SingleTileProp;
	}

	public override void SetPropPrototype(PropPrototype propPrototype)
	{
		Prototype = propPrototype;
	}

	public override PropPrototype GetPropPrototype()
	{
		return Prototype;
	}

	public override PrefabResource GetUnityModel()
	{
		if (Prototype == null || Prototype.Prefabs.Count <= 0)
		{
			return base.GetUnityModel();
		}
		return Prototype.Prefabs[MathUtil.RandomInt(Id, Prototype.Prefabs.Count)];
	}

	public override EquipmentPrototype GetGrabbableEquipmentType()
	{
		if (Prototype == null)
		{
			return null;
		}
		return Prototype.GrabbableEquipmentPrototype;
	}

	public override Vector2[] GetConnectors()
	{
		if (Prototype == null)
		{
			return null;
		}
		return Prototype.Connectors;
	}

	public override bool IsDestroyed()
	{
		return _deleted != DeletionState.None;
	}

	public override bool PropWantDelete()
	{
		return _deleted == DeletionState.WantDelete;
	}

	public override float GetFuel()
	{
		return Fuel;
	}

	public override void AddFuel(Character character, float amount)
	{
		Fuel += amount;
	}

	public override EquipmentPrototype GetRepairResourceType()
	{
		if (Prototype == null)
		{
			return base.GetRepairResourceType();
		}
		return Prototype.RepairResourceProto;
	}

	public override float GetRepairResourceNeeded()
	{
		if (Prototype == null)
		{
			return base.GetRepairResourceNeeded();
		}
		return Prototype.RepairResourceNeeded;
	}

	public override int GetRepairSkillNeeded()
	{
		if (Prototype == null)
		{
			return base.GetRepairSkillNeeded();
		}
		return Prototype.RepairSkillNeeded;
	}

	public override EquipmentPrototype GetCaptureResourceType()
	{
		if (Prototype == null)
		{
			return base.GetCaptureResourceType();
		}
		return Prototype.CaptureResourceProto;
	}

	public override float GetCaptureResourceNeeded()
	{
		if (Prototype == null || Prototype.CaptureResourceProto == null)
		{
			return 0f;
		}
		return 1f;
	}

	public override int GetCaptureSkillNeeded()
	{
		if (Prototype == null)
		{
			return base.GetCaptureSkillNeeded();
		}
		return Prototype.CaptureSkillNeeded;
	}

	public override bool CanBeClearedForBuilding(Community builderCommunity, TileObject newBuilding)
	{
		if (Prototype == null)
		{
			return false;
		}
		return Prototype.CanBeClearedForBuilding;
	}

	public override bool WantClearGrass()
	{
		if (Prototype == null)
		{
			return false;
		}
		return Prototype.WantClearGrass;
	}

	public override bool IsRound()
	{
		if (Prototype == null)
		{
			return false;
		}
		return Prototype.Round;
	}

	public override GenderType GetGender(Language language = Language.Count)
	{
		if (Prototype == null)
		{
			return base.GetGender(language);
		}
		return Prototype.GetGenderInLanguage(language);
	}

	public void MarkForDeletion()
	{
		if (_deleted == DeletionState.None)
		{
			_deleted = DeletionState.WantDelete;
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
		}
	}

	public static SingleTileProp Spawn(BaseObjectType objectType, TerrainCoord tile)
	{
		SingleTileProp obj = (SingleTileProp)BaseObjectManager.Create(objectType);
		obj.Tile = tile;
		obj.OnSpawn();
		return obj;
	}

	public static SingleTileProp Spawn(PropPrototype proto, TerrainCoord tile)
	{
		SingleTileProp obj = (SingleTileProp)BaseObjectManager.Create(proto.TypeName);
		obj.Prototype = proto;
		obj.Tile = tile;
		obj.OnSpawn();
		return obj;
	}

	public override bool IsImpassableSingleTileProp()
	{
		if (Prototype == null || (UnderConstructionInfo != null && !UnderConstructionInfo.IsBuiltEnoughToBeAnObstacle()))
		{
			return false;
		}
		return Prototype.CoverType != CoverType.None;
	}

	public override bool CanEncloseAnArea()
	{
		return IsImpassableSingleTileProp();
	}

	public override bool IsImpassable(Character requester, int options, TerrainCoord tile)
	{
		if ((options & 0x200) != 0 && CoverType == CoverType.WaistHigh)
		{
			return false;
		}
		return IsImpassableSingleTileProp();
	}

	public override BulletHitEffect GetBulletHitEffect(Vector3 nondeterministicHitPos)
	{
		if (Prototype == null)
		{
			return BulletHitEffect.Ricochet;
		}
		return Prototype.BulletHitEffect;
	}

	public override Flammability GetFlammability()
	{
		if (Prototype == null)
		{
			return base.GetFlammability();
		}
		return Prototype.Flammability;
	}

	public override ImpactSusceptibility GetImpactSusceptibility()
	{
		if (Prototype == null)
		{
			return base.GetImpactSusceptibility();
		}
		return Prototype.ImpactSusceptibility;
	}

	public override bool IsLooterProp()
	{
		if (Prototype == null)
		{
			return false;
		}
		return Prototype.IsLooterProp;
	}

	public override bool WantDebrisOnDemolition()
	{
		if (Prototype == null)
		{
			return base.WantDebrisOnDemolition();
		}
		return Prototype.WantDebrisOnDemolition;
	}

	public override void SetupPropIconCam(Camera cam)
	{
		base.SetupPropIconCam(cam);
		if (Prototype != null && Prototype.IconScale > 0f)
		{
			cam.orthographicSize /= Prototype.IconScale;
		}
	}

	public override void OnSpawn()
	{
		base.OnSpawn();
		if (!Session.Instance.Editor && GetCommunityId() != 0)
		{
			GameTerrain.Instance.BuildCommunityAreaOwner(GetTileRect());
		}
		if (GameTerrain.Instance.UnityTerrainObj != null)
		{
			GameTerrain.Instance.BuildMinimap(GetMinTile(), GetMaxTile());
		}
	}

	public override void Init()
	{
		base.Init();
		if (IsImpassableSingleTileProp() && GetBaseObjectType() != BaseObjectType.PitTrap)
		{
			GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
		}
		if (Burning)
		{
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
		}
		Community community = GetCommunity();
		if (UnderConstructionInfo != null && community != null && !community.UnderConstructionBuildings.Contains(this))
		{
			community.UnderConstructionBuildings.Add(this);
		}
		if (CanBeRepaired() && community != null && !community.NeedsRepair.Contains(this))
		{
			community.NeedsRepair.Add(this);
		}
		Session.Instance.ClearGrassForBuilding(this);
	}

	public Matrix4x4 GetWorldTrans()
	{
		if (CachedWorldTrans.m33 == 0f)
		{
			CachedWorldTrans = GetCustomModelTransform();
		}
		return CachedWorldTrans;
	}

	public void MarkCachedWorldTransDirty()
	{
		CachedWorldTrans.m33 = 0f;
	}

	public override void Delete()
	{
		Community community = GetCommunity();
		if (CanBeRepaired())
		{
			community?.NeedsRepair.Remove(this);
		}
		if (UnderConstructionInfo != null)
		{
			community?.UnderConstructionBuildings.Remove(this);
		}
		if (community != null)
		{
			foreach (Character member in community.Members)
			{
				if (member.IsBuildingSomething() == this)
				{
					member.OnThingIWasBuildingGotDeleted(this);
				}
			}
		}
		if (IsImpassableSingleTileProp() && GetBaseObjectType() != BaseObjectType.PitTrap)
		{
			GameTerrain.Instance.AStar.AddChange(AStarChange.RemovedFixedContents(this));
		}
		Session.Instance.PropManager.RemoveFromObjectsThatNeedUpdating(this);
		base.Delete();
		_deleted = DeletionState.Deleted;
		if (GameTerrain.Instance.UnityTerrainObj != null)
		{
			GameTerrain.Instance.BuildMinimap(GetMinTile(), GetMaxTile());
		}
		UpdateNearbyPitTraps();
	}

	public override void ReflectEarly(Reflector reflector)
	{
		base.ReflectEarly(reflector);
		if (reflector.Version < 216)
		{
			Prototype = GameImpl.Instance.FindPropPrototypeByName(GetBaseObjectType().ToString());
		}
		else if (reflector.Version >= 221)
		{
			reflector.Add(ref Prototype);
		}
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		if (reflector.Version >= 216 && reflector.Version < 221)
		{
			reflector.Add(ref Prototype);
		}
		bool value = UnderConstructionInfo != null;
		reflector.Add(ref value);
		if (value)
		{
			if (reflector.IsDeserialising)
			{
				UnderConstructionInfo = new UnderConstructionInfo();
			}
			UnderConstructionInfo.Reflect(reflector, this);
		}
		reflector.Add(ref Damage);
		reflector.Add(ref Fuel);
		reflector.AddAfter(ref Burning, 9);
		reflector.Add(ref _deleted);
		reflector.AddAfter(ref CommunityId, 19);
	}

	public override void OnStoryReloaded()
	{
		if (Prototype != null)
		{
			Prototype = GameImpl.Instance.FindPropPrototypeByNameHash(Prototype.NameHash);
		}
	}

	public override int GetCommunityId()
	{
		return CommunityId;
	}

	public override Community GetCommunity()
	{
		if (CommunityId == 0)
		{
			return null;
		}
		return BaseObjectManager.Instance.FindBaseObjectByID(CommunityId) as Community;
	}

	public override void SetCommunity(Community community)
	{
		Community community2 = GetCommunity();
		if (UnderConstructionInfo != null)
		{
			community2?.UnderConstructionBuildings.Remove(this);
		}
		if (CanBeRepaired())
		{
			community2?.NeedsRepair.Remove(this);
		}
		if (!Session.Instance.Editor)
		{
			community2?.AddConstructionRecord(Prototype, Tile, Prop.OrientationType.Deg0);
		}
		CommunityId = community?.Id ?? 0;
		if (CanBeRepaired() && community != null && !community.NeedsRepair.Contains(this))
		{
			community.NeedsRepair.Add(this);
		}
		if (UnderConstructionInfo != null && community != null && !community.UnderConstructionBuildings.Contains(this))
		{
			community.UnderConstructionBuildings.Add(this);
		}
		if (!Session.Instance.Editor)
		{
			GameTerrain.Instance.BuildCommunityAreaOwner(GetTileRect());
		}
	}

	public override UnderConstructionInfo GetUnderConstructionInfo()
	{
		return UnderConstructionInfo;
	}

	public override void SetUnderConstructionInfo(UnderConstructionInfo underConstructionInfo)
	{
		bool flag = IsImpassableSingleTileProp();
		bool num = HasUnityObject();
		bool flag2 = IsUnityObjectActive();
		if (flag2)
		{
			UnityDeactivate();
		}
		if (num)
		{
			UnityDelete();
		}
		Community community = GetCommunity();
		if (UnderConstructionInfo != null)
		{
			community?.UnderConstructionBuildings.Remove(this);
		}
		UnderConstructionInfo = underConstructionInfo;
		if (UnderConstructionInfo != null && community != null && !community.UnderConstructionBuildings.Contains(this))
		{
			community.UnderConstructionBuildings.Add(this);
		}
		if (num)
		{
			UnityInit();
		}
		if (flag2)
		{
			UnityActivate();
		}
		if (underConstructionInfo == null && GetBaseObjectType() != BaseObjectType.PitTrap)
		{
			bool flag3 = IsImpassableSingleTileProp();
			if (flag && !flag3)
			{
				GameTerrain.Instance.AStar.AddChange(AStarChange.RemovedFixedContents(this));
			}
			else if (!flag && flag3)
			{
				GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
			}
		}
		if (community != null && community.CommunityType == CommunityType.Player && UnderConstructionInfo == null && Prototype != null)
		{
			Prototype.MarkDiscovered();
		}
	}

	public override void PropUpdate(TimeSpan dt, ref bool stillNeedUpdating)
	{
		base.PropUpdate(dt, ref stillNeedUpdating);
		if (!Burning)
		{
			return;
		}
		stillNeedUpdating = true;
		ApplyDamage(null, (float)dt.TotalSeconds * StructureDamagePoint.FireDamageRate, from_impact: false, 0f);
		Fuel = Math.Max(0f, Fuel - TileObject.FuelBurningRateFlOzPerSecond * (float)dt.TotalSeconds);
		if (Fuel > 0f || GetFlammability() >= Flammability.High)
		{
			if (UnityBurningEffect == null && IsUnityObjectActive())
			{
				UnityBurningEffect = SpecialEffectManager.Instance.SpawnBurningEffect(UnityObj);
			}
			return;
		}
		if (UnityBurningEffect != null)
		{
			SpecialEffectManager.StopParticleEffect(UnityBurningEffect);
			UnityBurningEffect = null;
		}
		Burning = false;
	}

	public override bool IsBurningEnoughToDestroy()
	{
		if (Burning)
		{
			return Fuel / TileObject.FuelBurningRateFlOzPerSecond * StructureDamagePoint.FireDamageRate > (GetMaxDamage() - Damage) * 1.1f;
		}
		return false;
	}

	public override bool IsBurning()
	{
		return Burning;
	}

	public override void LightFire(Character character)
	{
		if (!Burning)
		{
			Burning = true;
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
		}
	}

	public void ApplyDamage(Character source, float damage, bool from_impact, float damageRadius)
	{
		if (_deleted != DeletionState.None)
		{
			return;
		}
		float maxDamage = GetMaxDamage();
		if (maxDamage >= float.MaxValue)
		{
			return;
		}
		Community community = GetCommunity();
		community?.OnPropertyDamaged(Tile, damageRadius, this, source);
		if (UnderConstructionInfo != null)
		{
			if (!UnderConstructionInfo.IsBuiltEnoughToBeAnObstacle())
			{
				community?.AddConstructionRecord(Prototype, Tile, Prop.OrientationType.Deg0);
				Damage = maxDamage;
				Delete();
				return;
			}
			damage *= 2f - UnderConstructionInfo.GetProgress();
		}
		Damage += damage;
		if (Damage >= maxDamage)
		{
			Damage = maxDamage;
			OnDestroyed(source, from_impact);
		}
	}

	public override Resource<AudioClip> GetDemolitionSound()
	{
		if (Prototype == null || !Prototype.WantSoundOnDemolition)
		{
			return null;
		}
		return SoundManager.DemolitionSound;
	}

	public void OnDestroyed(Character source, bool from_impact)
	{
		if (_deleted == DeletionState.None)
		{
			if (IsUnityObjectActive() && GetDemolitionSound() != null && !from_impact)
			{
				SoundManager.PlaySound3D(GetDemolitionSound(), Pos);
			}
			Community community = GetCommunity();
			community?.AddConstructionRecord(Prototype, Tile, Prop.OrientationType.Deg0);
			if (WantDebrisOnDemolition())
			{
				GameTerrain.Instance.SetDebris(Tile, Tile);
			}
			InvaderInstance invaderInstanceThatCreatedHunter = StoryManager.Instance.GetInvaderInstanceThatCreatedHunter(this);
			if (invaderInstanceThatCreatedHunter != null)
			{
				StoryManager.QueueEvents(invaderInstanceThatCreatedHunter.Invader.OnKilledEvents, null, null, this, new MemoryParam(invaderInstanceThatCreatedHunter.SourceObject));
			}
			MarkForDeletion();
			if (source != null && community != null && source.Community != community && source.IsControllableByPlayer() && community.HasAnyActiveMembers())
			{
				Memory.OnMemorableEvent(MemoryPrototype.DestroyedProperty, source, community, null, 0.1f, SecrecyMode.Public, null, fakeNews: false, this);
			}
			StoryManager.Instance.TriggerEnabledTriggersOfType(TriggerType.BuildingDestroyed, source, this);
		}
	}

	public float GetExplosionDamageFrac(Vector3 centre, float damageRadius)
	{
		Bounds boundingBox = GetBoundingBox();
		if (!boundingBox.Intersects(new BoundingSphere(centre, damageRadius)))
		{
			return 0f;
		}
		float pointDistFromRect = MathUtil.GetPointDistFromRect(MathUtil.ToXZ(centre), MathUtil.ToXZ(boundingBox.min), MathUtil.ToXZ(boundingBox.max));
		if (!(damageRadius > 0f))
		{
			return 0f;
		}
		return Math.Max(0f, 1f - pointDistFromRect / damageRadius);
	}

	public override float OnExplosionImpact(Character source, TileObject intendedTarget, Vector3 centre, float damage, float damageRadius, bool fromFoundations, SkillType skillType, InfectionType infectionType, bool itsATrap, TileObject bomb)
	{
		float explosionDamageFrac = GetExplosionDamageFrac(centre, damageRadius);
		if (explosionDamageFrac <= 0f)
		{
			return 0f;
		}
		damage *= explosionDamageFrac * explosionDamageFrac * explosionDamageFrac * explosionDamageFrac;
		if (this is PlantableCrop || this is Flower || this is Bush)
		{
			Fuel += Mathf.Lerp(4f, 6f, MathUtil.RandomFloat((float)PredictedObjectManager.Instance.GetCurrentTime(IsPredicted()).TotalSeconds + (float)Id * 0.2836f)) * TileObject.FuelBurningRateFlOzPerSecond * explosionDamageFrac;
			LightFire(source);
		}
		else
		{
			ApplyDamage(source, damage, from_impact: false, damageRadius);
		}
		source?.Skillset.AddProgress(source, skillType, damage * 0.01f);
		return damage;
	}

	public override float OnBurned(Character source, TileObject intendedTarget, Vector3 centre, float damage, float damageRadius, float fuel, SkillType skillType, InfectionType infectionType, bool assassinate, SecrecyMode secret)
	{
		if (!IsFlammable())
		{
			return 0f;
		}
		float explosionDamageFrac = GetExplosionDamageFrac(centre, damageRadius);
		if (explosionDamageFrac <= 0f)
		{
			return 0f;
		}
		Fuel += fuel;
		LightFire(source);
		damage *= explosionDamageFrac;
		if (!(this is PlantableCrop) && !(this is Flower) && !(this is Bush))
		{
			ApplyDamage(source, damage, from_impact: false, damageRadius);
		}
		source?.Skillset.AddProgress(source, skillType, damage * 0.1f);
		return damage;
	}

	public override void OnProjectileHit(Character shooter, Vector3 deterministicDir, Vector3 deterministicHitPos, float damage, bool predicted)
	{
		if (predicted)
		{
			if (Damage + damage >= GetMaxDamage())
			{
				UnityImpactDamageDestruction(deterministicDir * damage * Prop.ProjectileImpactFactor, deterministicHitPos);
			}
			return;
		}
		ApplyDamage(shooter, damage, from_impact: true, 0f);
		if (_deleted == DeletionState.WantDelete)
		{
			UnityImpactDamageDestruction(deterministicDir * damage * Prop.ProjectileImpactFactor, deterministicHitPos);
		}
	}

	public override float OnVehicleCollision(BaseObject other, Vector3 relativeVelocity, Vector3 contactPoint, bool predicted)
	{
		Character otherDriver;
		float num = Prop.CalcVehicleCollisionDamageToProp(this, other, relativeVelocity, out otherDriver);
		if (num > 0f)
		{
			bool flag = IsSusceptibleToVehicleCollisions(juggernaut: false);
			if (predicted)
			{
				if (Damage + num >= GetMaxDamage() && flag)
				{
					UnityImpactDamageDestruction(relativeVelocity * other.GetMassEstimate() * Prop.VehicleImpactFactor, contactPoint);
				}
			}
			else
			{
				ApplyDamage(otherDriver, num, flag, 0f);
				if (_deleted == DeletionState.WantDelete && flag)
				{
					UnityImpactDamageDestruction(relativeVelocity * other.GetMassEstimate() * Prop.VehicleImpactFactor, contactPoint);
				}
			}
		}
		return num;
	}

	public void UnityImpactDamageDestruction(Vector3 impactForce, Vector3 contactPoint)
	{
		if (UnityObj != null && UnderConstructionInfo == null)
		{
			UnityObj.AddComponent<ImpactedBehaviour>().Init(GetAuthoritativeOrElseThis(), GetUnityModel(), impactForce, contactPoint, this is BaseFence);
			UnityObj = null;
		}
	}

	public override float GetMassEstimate()
	{
		if (Prototype != null)
		{
			return Prototype.Mass;
		}
		return base.GetMassEstimate();
	}

	public virtual float GetMaxDamage()
	{
		if (Prototype == null || !(Prototype.MaxDamage > 0f))
		{
			return float.MaxValue;
		}
		return Prototype.MaxDamage;
	}

	public override float GetDamageFraction()
	{
		return Damage / GetMaxDamage();
	}

	public override void RepairDamage(float amount, Vector3 fromPos)
	{
		Damage = Math.Min(0f, Damage - amount * GetMaxDamage());
		if (Damage < 0.001f)
		{
			Damage = 0f;
		}
		if (Damage <= 0f)
		{
			GetCommunity()?.NeedsRepair.Remove(this);
		}
	}

	public override bool CaptureBuilding(float amount, Character capturedBy)
	{
		Community community = GetCommunity();
		SetCommunity(capturedBy.Community);
		capturedBy.Community.OnCompletedBuildingAddedToCommunity(capturedBy, this, community);
		return true;
	}

	public override void UnityInit()
	{
		if (UnderConstructionInfo != null)
		{
			UnityObj = UnderConstructionInfo.UnityInitCordon(this);
		}
		else
		{
			if (HasUnityObjectPool())
			{
				if (Session.Instance != null && Session.Instance.IsInFocusArea(GetTileRect()))
				{
					UnityActivate();
				}
				return;
			}
			PrefabResource unityModel = GetUnityModel();
			if (unityModel == null)
			{
				return;
			}
			UnityObj = UnityEngine.Object.Instantiate(unityModel.GetAsset(), (Id != 0) ? GameTerrain.Instance.UnityTerrainObj.transform : null, worldPositionStays: true);
		}
		UnityObj.name = GetDisplayNameString();
		UpdateUnityTransform();
		UnitySetupIdBehaviour();
		UnityObj.SetActive(value: false);
		if (Session.Instance != null && Session.Instance.IsInFocusArea(GetTileRect()))
		{
			UnityActivate();
		}
	}

	public override void UnityDelete()
	{
		base.UnityDelete();
		if (UnderConstructionInfo != null)
		{
			if (UnityObj != null)
			{
				UnityEngine.Object.Destroy(UnityObj);
				UnityObj = null;
			}
		}
		else if (UnityObj != null)
		{
			GetUnityModel().DeletePrefab(UnityObj);
			UnityObj = null;
		}
	}

	public override void UnityActivate()
	{
		if (UnderConstructionInfo == null && UnityObj == null && HasUnityObjectPool())
		{
			UnityObj = GetUnityModel().InstantiatePrefab((Id != 0) ? GameTerrain.Instance.UnityTerrainObj.transform : null);
			UnitySetupIdBehaviour();
			MarkCachedWorldTransDirty();
			UpdateUnityTransform();
		}
		if (UnityObj != null)
		{
			UnityObj.SetActive(value: true);
		}
		base.UnityActivate();
	}

	public override void UnityDeactivate()
	{
		base.UnityDeactivate();
		if ((bool)UnityBurningEffect)
		{
			UnityEngine.Object.DestroyImmediate(UnityBurningEffect);
			UnityBurningEffect = null;
		}
		if (UnityObj != null)
		{
			UnityObj.SetActive(value: false);
			if (UnderConstructionInfo == null && HasUnityObjectPool())
			{
				GetUnityModel().DeletePrefab(UnityObj);
				UnityObj = null;
			}
		}
	}

	public virtual void UpdateUnityTransform()
	{
		if (UnityObj != null)
		{
			if (UnderConstructionInfo != null)
			{
				UnityObj.transform.position = Pos;
				return;
			}
			Matrix4x4 worldTrans = GetWorldTrans();
			UnityObj.transform.position = worldTrans.Translation();
			UnityObj.transform.rotation = Quaternion.LookRotation(worldTrans.Forward(), worldTrans.Up());
			UnityObj.transform.localScale = new Vector3(worldTrans.Right().magnitude, worldTrans.Up().magnitude, worldTrans.Forward().magnitude);
		}
	}

	public override void SetTileGhost(TerrainCoord tile)
	{
		base.SetTileGhost(tile);
		MarkCachedWorldTransDirty();
		UpdateUnityTransform();
	}

	public virtual Matrix4x4 GetCustomModelTransform()
	{
		if (Prototype != null && Prototype.TiltedWithGroundSlope)
		{
			return GetCustomModelTransformTiltedWithGroundSlope(Vector2.zero);
		}
		Matrix4x4 mat = GetUnityModel()?.LocalToWorldMatrix ?? Matrix4x4.identity;
		MathUtil.SetTranslation(ref mat, ModelOffset);
		return MathUtil.CreateTranslation(Pos) * MathUtil.CreateRotationY(MathUtil.RandomFloat(Id) * (MathF.PI * 2f)) * mat;
	}

	public Matrix4x4 GetCustomModelTransformTiltedWithGroundSlope(Vector2 offset)
	{
		Vector2 v = ((GameTerrain.Instance != null) ? GameTerrain.Instance.GetTileCentreXZ(Tile) : Vector2.zero);
		Vector3 obj = ((GameTerrain.Instance != null) ? GameTerrain.Instance.GetNormalAtPos(v.x, v.y) : Vector3.up);
		float z = (0f - Mathf.Asin(obj.x)) * 57.29578f;
		float x = Mathf.Asin(obj.z) * 57.29578f;
		Vector3 pos = ((GameTerrain.Instance != null) ? GameTerrain.Instance.ClampPosToSurface(MathUtil.ToX0Y(v)) : Vector3.zero);
		Quaternion q = Quaternion.Euler(x, 0f, z) * Quaternion.Euler(0f, MathUtil.RandomFloat(Id) * 360f, 0f);
		return Matrix4x4.TRS(pos, q, Vector3.one);
	}

	public override bool HasUnityObject()
	{
		return UnityObj != null;
	}

	public override bool IsUnityObjectActive()
	{
		if (UnityObj != null)
		{
			return UnityObj.activeSelf;
		}
		return false;
	}

	public override bool HasUnityObjectPool()
	{
		PrefabResource unityModel = GetUnityModel();
		if (unityModel != null)
		{
			return unityModel.Pool != null;
		}
		return false;
	}

	public override GameObject GetUnityObject()
	{
		return UnityObj;
	}

	public override void OnTerrainHeightChanged()
	{
		MarkCachedWorldTransDirty();
		UpdateUnityTransform();
		base.OnTerrainHeightChanged();
	}

	public override void SetTile(TerrainCoord tile)
	{
		base.SetTile(tile);
		MarkCachedWorldTransDirty();
	}

	public override Bounds GetBoundingBox()
	{
		Vector3 pos = Pos;
		float y;
		if (UnderConstructionInfo != null)
		{
			y = UnderConstructionInfo.UnderConstructionHeight;
		}
		else
		{
			PrefabResource unityModel = GetUnityModel();
			y = ((unityModel != null) ? (unityModel.Height * 0.5f) : 0.5f);
		}
		return MathUtil.CreateBoundsCentreExtents(pos + new Vector3(0f, y, 0f), new Vector3(0.5f, y, 0.5f));
	}

	public override float? Raycast(Ray ray, float length, int flags, ref Vector3 normal, ref Bone bone, ref Vector3 hitPosInBoneSpace, ref float plantCover, Character source, TileObject target)
	{
		using (new UnityProfileMarker(SingleTilePropRaycastStr))
		{
			float distance3;
			if (IsImpassableSingleTileProp())
			{
				if ((flags & 4) != 0 && GetBoundingBox().IntersectRay(ray, out var distance) && distance < length)
				{
					return distance;
				}
				if ((flags & 8) != 0)
				{
					if (UnderConstructionInfo != null)
					{
						return null;
					}
					if (GetBoundingBox().IntersectRay(ray, out var distance2) && distance2 < length)
					{
						return Prop.RaycastAgainstModel(GetUnityModel(), GetWorldTrans(), ray, length, 0, out normal, 0f, (flags & 0x1000) != 0);
					}
				}
			}
			else if ((flags & 0x10000) != 0 && GetBoundingBox().IntersectRay(ray, out distance3) && distance3 < length)
			{
				return distance3;
			}
			if ((flags & 0x8000) != 0)
			{
				float plantCoverForCamouflage = GetPlantCoverForCamouflage();
				if (plantCoverForCamouflage > 0f)
				{
					Bounds boundingBox = GetBoundingBox();
					if (boundingBox.IntersectRay(ray, out var distance4) && distance4 < length)
					{
						Vector3 point = ray.GetPoint(length);
						if (boundingBox.Contains(point))
						{
							plantCover = plantCoverForCamouflage * (length - distance4);
						}
						else
						{
							if (!boundingBox.IntersectRay(new Ray(point, -ray.direction), out var distance5))
							{
								return null;
							}
							plantCover = plantCoverForCamouflage * (length - distance4 - distance5);
						}
						return distance4;
					}
				}
			}
		}
		return null;
	}

	public override float? RaycastThreadSafe(Ray ray, float length, AStarMoveableObstacle obstacle, TerrainCoord tile, byte modelIndex, bool includeWireFences)
	{
		if (IsImpassableSingleTileProp())
		{
			PrefabResource unityModel = GetUnityModel();
			Vector3 aStarTileCentrePos = GameTerrain.Instance.GetAStarTileCentrePos(tile);
			if (MathUtil.CreateBoundsMinMax(aStarTileCentrePos - new Vector3(0.5f, 0f, 0.5f), aStarTileCentrePos + new Vector3(0.5f, unityModel.Height, 0.5f)).IntersectRay(ray))
			{
				Matrix4x4 mat = unityModel.LocalToWorldMatrix;
				MathUtil.SetTranslation(ref mat, ModelOffset);
				Matrix4x4 rootTransform = MathUtil.CreateTranslation(aStarTileCentrePos) * MathUtil.CreateRotationY(MathUtil.RandomFloat(Id) * (MathF.PI * 2f)) * mat;
				Vector3 hitNormal;
				return Prop.RaycastAgainstModel(unityModel, rootTransform, ray, length, 0, out hitNormal, 0f, includeWireFences);
			}
		}
		return null;
	}

	public override void OnPostRender()
	{
		if (Character.DrawBoundingBoxes)
		{
			Bounds boundingBox = GetBoundingBox();
			DebugGraphics.StartDrawLines(Matrix4x4.identity);
			DebugGraphics.DrawBox(boundingBox.center, boundingBox.extents, Color.blue);
			DebugGraphics.EndDrawLines();
		}
		if (Character.DrawHitBoxes)
		{
			PrefabResource unityModel = GetUnityModel();
			if (unityModel != null)
			{
				Prop.DrawCollisionMesh(unityModel, GetWorldTrans(), 0f);
			}
		}
	}

	public override bool IsTargetable()
	{
		Session instance = Session.Instance;
		if (instance.Editor || PropEditor.AllowTargetingAllProps)
		{
			return true;
		}
		if (!FogOfWar.DebugFogOfWarEnabled || GameTerrain.Instance.FogOfWar.IsTileExplored(Tile.x, Tile.y))
		{
			if (GetGrabbableEquipmentType() != null)
			{
				return true;
			}
			if (UnderConstructionInfo != null)
			{
				return true;
			}
			if (Prototype != null)
			{
				Character localControlledCharacter = instance.Hud.LocalControlledCharacter;
				if (localControlledCharacter != null)
				{
					if (Prototype.Flammability == Flammability.Medium_RequiresFuel)
					{
						if (localControlledCharacter.EquippedItem != null && localControlledCharacter.EquippedItem.GetLiquidContentsType() != null && localControlledCharacter.EquippedItem.GetLiquidContentsType().Flammable)
						{
							return true;
						}
						if (Fuel > 0f)
						{
							if (localControlledCharacter.Inventory.FindItemOfType(EquipmentPrototype.Match) != null)
							{
								return true;
							}
							if (localControlledCharacter.Inventory.GetHuntingKnife() != null && localControlledCharacter.Inventory.FindItemOfType(EquipmentPrototype.Flint) != null)
							{
								return true;
							}
						}
					}
					if (localControlledCharacter.Community != null && !instance.GameCamera.FlyCam && Prototype.CanBeDemolished)
					{
						Community community = GetCommunity();
						if (localControlledCharacter.Community == community || community == null || !community.HasAnyActiveMembers())
						{
							Recipe recipe = GameImpl.Instance.FindRecipeByProduct(GetPropPrototype(), null);
							if (recipe != null)
							{
								if (recipe.IsRecipeToolType(localControlledCharacter.EquippedItem))
								{
									return true;
								}
							}
							else if (localControlledCharacter.EquippedItem is Toolbox)
							{
								return true;
							}
						}
					}
				}
			}
			return Damage >= MinDamageToShowHealthBar;
		}
		return false;
	}

	public override Texture2D GetIcon(out Material mat, out Color col, bool highlighted)
	{
		Texture2D iconResource = GetIconResource();
		if (iconResource != null)
		{
			mat = (highlighted ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
			col = Color.white;
			return iconResource;
		}
		iconResource = IconGenerator.Instance.GetIconForObjectType(Prototype, out mat, highlighted);
		col = ((iconResource != null) ? Color.white : MathUtil.TransparentBlackCol);
		return iconResource;
	}
}
