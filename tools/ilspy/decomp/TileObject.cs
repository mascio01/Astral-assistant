using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public abstract class TileObject : BaseObject
{
	public int FoundInList;

	private static float PipFocusDistMul = 2f;

	public static float FuelBurningRateFlOzPerSecond = 1f;

	public static float MaxFireHeatRange = 12f;

	public static float FireIdealWarmthRange = 6f;

	private int LastFrameWithinUnityActivationRange;

	public virtual float Height => GetBoundingBox().size.y;

	public virtual Vector3 Pos => GetBoundingBoxBottom();

	public virtual Vector2 PosXZ => MathUtil.ToXZ(Pos);

	public virtual float TarmacDist => 0f;

	public virtual CoverType CoverType => CoverType.None;

	public virtual Color32 MapColor => MathUtil.TransparentBlack;

	public virtual int UnityModelIndexFromPrototype => 0;

	public static TileObject CreateProp(PropPrototype proto)
	{
		TileObject tileObject = (TileObject)BaseObjectManager.Create(proto.TypeName);
		tileObject?.SetPropPrototype(proto);
		return tileObject;
	}

	public static TileObject SpawnProp(PropPrototype proto, TerrainCoord tile, Prop.OrientationType orientation)
	{
		TileObject tileObject = CreateProp(proto);
		tileObject.SetOrientationType(orientation);
		tileObject.SetTileGhost(tile);
		tileObject.OnSpawn();
		return tileObject;
	}

	public virtual void SetPropPrototype(PropPrototype propPrototype)
	{
	}

	public virtual PropPrototype GetPropPrototype()
	{
		return null;
	}

	public abstract Bounds GetBoundingBox();

	public virtual float GetHeightIgnoringCrouching()
	{
		return GetBoundingBox().size.y;
	}

	public abstract TerrainCoord GetMinTile();

	public abstract TerrainCoord GetMaxTile();

	public TerrainRect GetTileRect()
	{
		return new TerrainRect(GetMinTile(), GetMaxTile());
	}

	public TerrainRect GetVertRect()
	{
		return new TerrainRect(GetMinTile(), GetMaxTile() + new TerrainCoord(1, 1));
	}

	public TerrainCoord GetCentreTile()
	{
		return (GetMinTile() + GetMaxTile()) / 2;
	}

	public Vector3 GetBoundingBoxCentre()
	{
		Bounds boundingBox = GetBoundingBox();
		return (boundingBox.min + boundingBox.max) * 0.5f;
	}

	public Vector3 GetBoundingBoxBottom()
	{
		Bounds boundingBox = GetBoundingBox();
		Vector3 result = (boundingBox.min + boundingBox.max) * 0.5f;
		result.y = boundingBox.min.y;
		return result;
	}

	public virtual Vector3 GetCentreTop()
	{
		Bounds boundingBox = GetBoundingBox();
		Vector3 result = (boundingBox.min + boundingBox.max) * 0.5f;
		result.y = boundingBox.max.y;
		return result;
	}

	public virtual float GetPipFocusDist()
	{
		Bounds boundingBox = GetBoundingBox();
		return (boundingBox.max - boundingBox.min).magnitude * 0.5f * PipFocusDistMul;
	}

	public virtual float GetPipClipDist()
	{
		return MathUtil.ToXZ(GetBoundingBox().extents).magnitude;
	}

	public virtual Vector3 GetPipFocusPos()
	{
		return GetBoundingBoxCentre();
	}

	public virtual Matrix4x4 GetPipFocusWorldMatrix()
	{
		return GetWorld();
	}

	public virtual Matrix4x4 GetWorld()
	{
		return MathUtil.CreateTranslation(Pos);
	}

	public virtual int GetTileX()
	{
		return GetTile().x;
	}

	public virtual int GetTileY()
	{
		return GetTile().y;
	}

	public override TerrainCoord GetTile()
	{
		return GameTerrain.Instance.GetTileCoordForPos(Pos);
	}

	public virtual void SetTileX(int x)
	{
	}

	public virtual void SetTileY(int y)
	{
	}

	public virtual void SetTile(TerrainCoord tile)
	{
	}

	public virtual void SetTileGhost(TerrainCoord tile)
	{
		SetTile(tile);
	}

	public virtual void SetOrientationType(Prop.OrientationType orientation)
	{
	}

	public virtual Prop.OrientationType GetOrientationType()
	{
		return Prop.OrientationType.Deg0;
	}

	public virtual bool CanSetPropName()
	{
		return false;
	}

	public virtual int GetCommunityId()
	{
		return 0;
	}

	public override Community GetCommunity()
	{
		return null;
	}

	public virtual void SetCommunity(Community community)
	{
	}

	public virtual Community GetCommunityThatOwnsThisArea()
	{
		Community community = GetCommunity();
		if (community != null)
		{
			return community;
		}
		TerrainCoord tile = GetTile();
		int ownerCommunityIdForTile = GameTerrain.Instance.GetOwnerCommunityIdForTile(tile.x, tile.y);
		if (ownerCommunityIdForTile != 0)
		{
			return BaseObjectManager.Instance.FindBaseObjectByID(ownerCommunityIdForTile) as Community;
		}
		return null;
	}

	public virtual void GetSubDisplayCol(out Color col, out Color col2)
	{
		CommunityManager communityManager = Session.Instance.CommunityManager;
		Community community = GetCommunity();
		if (community == communityManager.PlayerCommunity)
		{
			col = GameTerrain.MinimapSettings.FriendCol;
			col2 = GameTerrain.MinimapSettings.FriendCol2;
			return;
		}
		switch (communityManager.GetRelationship(community, communityManager.PlayerCommunity))
		{
		case CommunityRelationshipType.Hostile:
			col = GameTerrain.MinimapSettings.EnemyCol;
			col2 = GameTerrain.MinimapSettings.EnemyCol2;
			break;
		case CommunityRelationshipType.Allied:
			col = GameTerrain.MinimapSettings.AllyCol;
			col2 = GameTerrain.MinimapSettings.AllyCol2;
			break;
		default:
			col = GameTerrain.MinimapSettings.NeutralCol;
			col2 = GameTerrain.MinimapSettings.NeutralCol2;
			break;
		}
	}

	public override void Delete()
	{
		if (GetUnderConstructionInfo() != null)
		{
			Community community = GetCommunity();
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
		}
		if (Hud.Instance.EditorSelectedObject == this)
		{
			Hud.Instance.EditorSelectedObject = null;
			Hud.Instance.Pip.FocusObject = null;
		}
		base.Delete();
	}

	public void UpdateNearbyPitTraps()
	{
		if (this is PitTrap)
		{
			return;
		}
		GameTerrain instance = GameTerrain.Instance;
		TerrainCoord minTile = GetMinTile();
		TerrainCoord maxTile = GetMaxTile();
		for (int i = minTile.x - 1; i <= maxTile.x + 1; i++)
		{
			for (int j = minTile.y - 1; j <= maxTile.y + 1; j++)
			{
				if (!instance.IsTileOutsideBounds(i, j) && instance.IsTileTrap(i, j) && instance.GetFixedObjectOnTile(i, j) is PitTrap pitTrap)
				{
					pitTrap.UpdateNearbyJoiners(deletingMe: false);
				}
			}
		}
	}

	public virtual bool IsDestroyed()
	{
		return false;
	}

	public virtual bool IsDestroyedNotIncludingDeadCrops()
	{
		return IsDestroyed();
	}

	public virtual Flammability GetFlammability()
	{
		return Flammability.Invulnerable;
	}

	public bool IsFlammable()
	{
		return GetFlammability() >= Flammability.Medium_RequiresFuel;
	}

	public bool IsExplosionProof()
	{
		return GetFlammability() == Flammability.Invulnerable;
	}

	public virtual bool IsForcedInvulnerable()
	{
		return false;
	}

	public virtual void SetForceInvulnerable(bool v)
	{
	}

	public virtual bool IsBurningEnoughToDestroy()
	{
		return false;
	}

	public virtual bool IsBurningDueToAttack()
	{
		return IsBurning();
	}

	public virtual bool IsBurning()
	{
		return false;
	}

	public virtual void LightFire(Character character)
	{
	}

	public virtual float GetFuel()
	{
		return 0f;
	}

	public virtual void AddFuel(Character character, float amount)
	{
	}

	public virtual void Consume(Character character, float amount, InfectionType infectionType)
	{
	}

	public virtual float GetConsumedAmount()
	{
		return 0f;
	}

	public virtual bool DeleteWhenSkinned()
	{
		return false;
	}

	public virtual void DeleteOrDisappear()
	{
		DeleteOrDisappear(fromGoal: false);
	}

	public virtual void DeleteOrDisappear(bool fromGoal)
	{
		Delete();
	}

	public virtual bool WantFlattenTerrain()
	{
		return false;
	}

	public virtual bool WantClearGrass()
	{
		return false;
	}

	public virtual bool WantDebrisOnDemolition()
	{
		return true;
	}

	public virtual Resource<AudioClip> GetDemolitionSound()
	{
		return SoundManager.DemolitionSound;
	}

	public virtual bool IsTiltableProp()
	{
		return false;
	}

	public virtual bool IsLooterProp()
	{
		return false;
	}

	public virtual float GetFireEffectOnCharacter(Character character)
	{
		if (!IsBurning())
		{
			return 0f;
		}
		return 1f - Mathf.Clamp01((character.PosXZ - PosXZ).magnitude / MaxFireHeatRange);
	}

	public bool CanBeRepaired()
	{
		if (GetDamageFraction() > 0f)
		{
			return GetRepairResourceType() != null;
		}
		return false;
	}

	public virtual float GetDamageFraction()
	{
		return 0f;
	}

	public virtual float GetCaptureFraction()
	{
		return 0f;
	}

	public virtual void RepairDamage(float amount, Vector3 fromPos)
	{
	}

	public virtual bool CaptureBuilding(float amount, Character capturedBy)
	{
		return false;
	}

	public virtual void AbandonBuilding()
	{
		SetCommunity(null);
	}

	public virtual EquipmentPrototype GetRepairResourceType()
	{
		return null;
	}

	public virtual float GetRepairResourceNeeded()
	{
		return 0f;
	}

	public virtual int GetRepairSkillNeeded()
	{
		return 0;
	}

	public virtual EquipmentPrototype GetCaptureResourceType()
	{
		return null;
	}

	public virtual float GetCaptureResourceNeeded()
	{
		return 0f;
	}

	public virtual int GetCaptureSkillNeeded()
	{
		return 0;
	}

	public virtual float OnExplosionImpact(Character source, TileObject intendedTarget, Vector3 centre, float damage, float damageRadius, bool fromFoundations, SkillType skillType, InfectionType infectionType, bool itsATrap, TileObject bomb)
	{
		return 0f;
	}

	public virtual float OnBurned(Character source, TileObject intendedTarget, Vector3 centre, float damage, float damageRadius, float fuel, SkillType skillType, InfectionType infectionType, bool assassinate, SecrecyMode secret)
	{
		return 0f;
	}

	public virtual void OnTerrainHeightChanged()
	{
	}

	public virtual void OnHearSound(AISound sound)
	{
	}

	public virtual float GetPlantCoverForCamouflage()
	{
		return 0f;
	}

	public virtual float GetVisibility(Character from, Vector3 eyePos, Vector3 eyeDir, float sightRange, out bool hasLineOfSightIgnoringPlantCover, out bool canHearFootsteps)
	{
		GameTerrain instance = GameTerrain.Instance;
		TerrainCoord nearestTileTo = GetNearestTileTo(instance.GetTileCoordForPos(eyePos));
		hasLineOfSightIgnoringPlantCover = (instance.GetTileCentreXZ(nearestTileTo) - MathUtil.ToXZ(eyePos)).sqrMagnitude <= sightRange * sightRange;
		canHearFootsteps = false;
		if (!hasLineOfSightIgnoringPlantCover)
		{
			return 0f;
		}
		return 1f;
	}

	public virtual bool IsVisibleInFogOfWar()
	{
		if (!FogOfWar.DebugFogOfWarEnabled)
		{
			return true;
		}
		return GameTerrain.Instance.FogOfWar.IsAnyTileInRectVisible(GetMinTile(), GetMaxTile());
	}

	public virtual bool IsTargetable()
	{
		return false;
	}

	public virtual float GetWeaponRangeFactorWhenAimedAtMe()
	{
		return 1f;
	}

	public virtual float GetTargetableRadiusFactor(bool isUsingRangedWeapon)
	{
		return 1f;
	}

	public virtual float GetTargetableAngle()
	{
		return MathF.PI / 2f;
	}

	public bool IsFriendlyFire(Character source, TileObject target, bool itsATrap)
	{
		return IsFriendlyFire(source, target, itsATrap, AttackType.Invalid);
	}

	public virtual bool IsFriendlyFire(Character source, TileObject target, bool itsATrap, AttackType meleeAttackType)
	{
		if (source != null)
		{
			Community community = GetCommunity();
			if (community != null)
			{
				if (community == source.Community)
				{
					return true;
				}
				if (community.CachedAllies.Contains(source.Community))
				{
					return true;
				}
				if (target != null && target != this && target.GetCommunity() != community && !source.IsEnemy(this) && community.GetLivingNonZombieMemberCount() != 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual EquipmentPrototype GetGrabbableEquipmentType()
	{
		return null;
	}

	public virtual InfectionType GetGrabbableEquipmentInfectedWith()
	{
		return InfectionType.None;
	}

	public virtual bool IsImpassable(Character requester, int options, TerrainCoord tile)
	{
		return false;
	}

	public virtual bool IsRound()
	{
		return false;
	}

	public virtual bool IsSmallAnimal()
	{
		return false;
	}

	public virtual bool IsAccommodation()
	{
		return false;
	}

	public virtual bool IsGuardPost()
	{
		return false;
	}

	public virtual bool CanBeClearedForBuilding(Community builderCommunity, TileObject newBuilding)
	{
		return false;
	}

	public virtual bool CanEncloseAnArea()
	{
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
		iconResource = IconGenerator.Instance.GetIconForObjectType(GetBaseObjectType(), out mat, highlighted);
		col = ((iconResource != null) ? Color.white : MathUtil.TransparentBlackCol);
		return iconResource;
	}

	public virtual float GetWeightWhenCarried()
	{
		return 0f;
	}

	public virtual void SetupPropIconCam(Camera cam)
	{
		PrefabResource unityModel = GetUnityModel();
		Bounds bounds = ((unityModel != null) ? Prop.CalcTransformedBounds(unityModel.IdentityBounds, GetUnityModel().LocalToWorldMatrix) : default(Bounds));
		Vector2 vector = Vector2.zero;
		if (this is Prop prop)
		{
			vector = new Vector2(Mathf.Lerp((float)prop.ExtentsMin.x - 0.5f, (float)prop.ExtentsMax.x + 0.5f, 0.5f), Mathf.Lerp((float)prop.ExtentsMin.y - 0.5f, (float)prop.ExtentsMax.y + 0.5f, 0.5f)) - MathUtil.ToXZ(prop.ModelOffset);
		}
		Vector2 vector2 = new Vector2(vector.x, bounds.extents.z + 10f);
		Building building = this as Building;
		if (building != null && Math.Abs(building.GetEntranceDefs()[0].EntranceOffset.x) > Math.Abs(building.GetEntranceDefs()[0].EntranceOffset.y))
		{
			vector2 = new Vector2(Mathf.Sign(building.GetEntranceDefs()[0].EntranceOffset.x) * (bounds.extents.x + 10f), vector.y);
		}
		else if (building != null && Math.Abs(building.GetEntranceDefs()[0].EntranceOffset.y) > Math.Abs(building.GetEntranceDefs()[0].EntranceOffset.x))
		{
			vector2 = new Vector2(vector.x, Mathf.Sign(building.GetEntranceDefs()[0].EntranceOffset.y) * (bounds.extents.z + 10f));
		}
		else if (this is Gate || this is BaseFence)
		{
			vector2 = new Vector2(bounds.extents.x + 10f, vector.y);
		}
		cam.transform.position = Sun.IconGenerationPos + new Vector3(vector2.x, bounds.extents.y, vector2.y);
		cam.transform.LookAt(Sun.IconGenerationPos + new Vector3(vector.x, bounds.extents.y, vector.y), Vector3.up);
		cam.orthographicSize = Math.Max(bounds.extents.y, (Math.Abs(vector2.y) > Math.Abs(vector2.x)) ? bounds.extents.x : bounds.extents.z) * 1.1f;
	}

	public virtual UnderConstructionInfo GetUnderConstructionInfo()
	{
		return null;
	}

	public virtual void SetUnderConstructionInfo(UnderConstructionInfo underConstructionInfo)
	{
	}

	public virtual float? Raycast(Ray ray, float length, int flags, ref Vector3 normal, ref Bone bone, ref Vector3 hitPosInBoneSpace, ref float plantCover, Character source, TileObject target)
	{
		return null;
	}

	public virtual float? RaycastThreadSafe(Ray ray, float length, AStarMoveableObstacle obstacle, TerrainCoord tile, byte modelIndex, bool includeFenceWire)
	{
		return null;
	}

	public virtual EquipmentContainer GetInventory()
	{
		return null;
	}

	public virtual float GetMaxInventoryWeight()
	{
		return 0f;
	}

	public virtual float GetMaxInventoryWeightIncludingBackpack(Equipment includeBackpack)
	{
		return GetMaxInventoryWeight();
	}

	public virtual CantTransferReason CanTransferEquipmentAway(Equipment item)
	{
		return CanTransferEquipmentAway(item, onTradePage: false);
	}

	public virtual CantTransferReason CanTransferEquipmentAway(Equipment item, bool onTradePage)
	{
		return CantTransferReason.CanTransfer;
	}

	public virtual ActionAnim GetTakeAnim()
	{
		return ActionAnim.Scavenge;
	}

	public virtual bool IsInvestigated()
	{
		return true;
	}

	public virtual void MarkInvestigated(Character investigator)
	{
	}

	public virtual bool IsUsingForCrafting(Equipment item, bool willTransferContainers, FindType findType, out int needed)
	{
		needed = 0;
		return false;
	}

	public virtual bool IsLockable()
	{
		return false;
	}

	public virtual bool WantToKeepGift(Equipment item)
	{
		return false;
	}

	public float GetAvailableInventorySpace()
	{
		EquipmentContainer inventory = GetInventory();
		if (inventory == null)
		{
			return 0f;
		}
		return Math.Max(0f, GetMaxInventoryWeight() - inventory.GetWeight(this));
	}

	public int GetAmountOfEquipmentThatCanBeStored(Equipment item)
	{
		return (int)(GetAvailableInventorySpace() / item.GetWeight());
	}

	public bool HasInventorySpaceFor(float weight)
	{
		EquipmentContainer inventory = GetInventory();
		if (inventory != null)
		{
			return weight <= GetMaxInventoryWeight() - inventory.GetWeight(this);
		}
		return false;
	}

	public bool InventoryContains(Equipment item)
	{
		return GetInventory()?.Contains(item) ?? false;
	}

	public int SpawnEquipmentIfSpaceIsAvailable(EquipmentPrototype proto, int amount, bool fillLiquidContainers)
	{
		Equipment item;
		return SpawnEquipmentIfSpaceIsAvailable(proto, amount, fillLiquidContainers, out item);
	}

	public int SpawnEquipmentIfSpaceIsAvailable(EquipmentPrototype proto, int amount, bool fillLiquidContainers, out Equipment item)
	{
		item = null;
		amount = Math.Min(amount, (int)(GetAvailableInventorySpace() / proto.Weight));
		if (amount > 0)
		{
			if (proto.CanBeCombined && proto.DefaultInfectionTypeMin == InfectionType.None)
			{
				item = GetInventory().Add(this, Equipment.Spawn(proto, amount));
			}
			else
			{
				for (int i = 0; i < amount; i++)
				{
					Equipment equipment = Equipment.Spawn(proto);
					if (proto.DefaultInfectionTypeMin > InfectionType.None && equipment != null)
					{
						equipment.InfectedWith = (InfectionType)MathUtil.RandomInt(equipment.Id, (int)proto.DefaultInfectionTypeMin, (int)(proto.DefaultInfectionTypeMax + 1));
					}
					item = GetInventory().Add(this, equipment);
					if (fillLiquidContainers && item.GetLiquidCapacity() > 0f && proto.DefaultLiquidPrototype != null)
					{
						float num = Mathf.Lerp(proto.DefaultLiquidFilledMin, proto.DefaultLiquidFilledMax, MathUtil.RandomFloat(item.Id)) / 100f;
						item.FillLiquid(proto.DefaultLiquidPrototype, item.GetLiquidCapacity() * num, InfectionType.None);
					}
				}
			}
		}
		return amount;
	}

	public virtual bool WantPrediction()
	{
		return false;
	}

	public virtual int GetPredictedFreshFrame()
	{
		return -1;
	}

	public virtual TileObject GetPredicted()
	{
		return null;
	}

	public virtual TileObject GetAuthoritative()
	{
		return null;
	}

	public virtual void SetPredictedFreshFrame(int frame)
	{
	}

	public virtual void SetPredicted(TileObject obj)
	{
	}

	public virtual void SetAuthoritative(TileObject obj)
	{
	}

	public virtual void InitPredicted()
	{
	}

	public virtual void DeletePredicted()
	{
	}

	public virtual void PredictedUpdate(TimeSpan dt)
	{
	}

	public virtual void PredictedFixup()
	{
	}

	public virtual bool PredictedWantDelete()
	{
		return false;
	}

	public virtual bool PredictedWantContinueAfterAuthoritativeHasBeenDeleted()
	{
		return false;
	}

	public virtual byte[] GetPredictedActionCounter()
	{
		return null;
	}

	public virtual List<PredictedEvent> GetPredictedEvents()
	{
		return null;
	}

	public bool CheckFrontmostPrediction(PredictedEventType predictedEventType)
	{
		return CheckFrontmostPrediction(predictedEventType, 0);
	}

	public bool CheckFrontmostPrediction(PredictedEventType predictedEventType, int extraData)
	{
		byte[] predictedActionCounter = GetPredictedActionCounter();
		if (IsAuthoritative())
		{
			predictedActionCounter[(int)predictedEventType]++;
			return !IsBeingPredicted();
		}
		TileObject authoritative = GetAuthoritative();
		if (authoritative != null && authoritative.CheckFrontmostPrediction(predictedActionCounter[(int)predictedEventType], predictedEventType, extraData))
		{
			predictedActionCounter[(int)predictedEventType]++;
			return true;
		}
		return false;
	}

	private bool CheckFrontmostPrediction(int actionCounter, PredictedEventType predictedEventType, int extraData)
	{
		List<PredictedEvent> predictedEvents = GetPredictedEvents();
		TimeSpan timeSpan = Session.Instance.PlayTime - TimeSpan.FromSeconds(2.0);
		for (int num = predictedEvents.Count - 1; num >= 0; num--)
		{
			if (predictedEvents[num].ActionCounter == actionCounter && predictedEvents[num].EventType == predictedEventType && predictedEvents[num].ExtraData == extraData)
			{
				return false;
			}
			if (predictedEvents[num].Time < timeSpan)
			{
				predictedEvents.RemoveAt(num);
			}
		}
		predictedEvents.Add(new PredictedEvent(PredictedObjectManager.Instance.PredictedTime, actionCounter, predictedEventType, extraData));
		return true;
	}

	public TileObject GetAuthoritativeOrElseThis()
	{
		if (!IsAuthoritative())
		{
			return GetAuthoritative();
		}
		return this;
	}

	public TileObject GetPredictedOrElseThis()
	{
		TileObject predicted = GetPredicted();
		if (predicted == null)
		{
			return this;
		}
		return predicted;
	}

	public bool IsBeingPredicted()
	{
		return GetPredicted() != null;
	}

	public bool IsFrontmostPrediction()
	{
		if (IsAuthoritative())
		{
			return !IsBeingPredicted();
		}
		return PredictedObjectManager.Instance.IsFrontmostPrediction();
	}

	public TerrainCoord GetNearestTileTo(TerrainCoord tile)
	{
		TerrainCoord minTile = GetMinTile();
		TerrainCoord maxTile = GetMaxTile();
		int x = Math.Max(minTile.x, Math.Min(maxTile.x, tile.x));
		int y = Math.Max(minTile.y, Math.Min(maxTile.y, tile.y));
		return new TerrainCoord(x, y);
	}

	public TerrainCoord GetNearestVertexTo(TerrainCoord vert)
	{
		TerrainCoord minTile = GetMinTile();
		TerrainCoord terrainCoord = GetMaxTile() + new TerrainCoord(1, 1);
		int x = Math.Max(minTile.x, Math.Min(terrainCoord.x, vert.x));
		int y = Math.Max(minTile.y, Math.Min(terrainCoord.y, vert.y));
		return new TerrainCoord(x, y);
	}

	public Vector2 GetNearestPosXZTo(Vector2 posXZ)
	{
		Bounds boundingBox = GetBoundingBox();
		float x = Math.Max(boundingBox.min.x, Math.Min(boundingBox.max.x, posXZ.x));
		float y = Math.Max(boundingBox.min.z, Math.Min(boundingBox.max.z, posXZ.y));
		return new Vector2(x, y);
	}

	public TerrainCoord GetNearestPassableTileTo(TerrainCoord tile, int options, Character requester, TileObject ignore, bool mustBeSpawnable = false, bool checkPath = false)
	{
		return new TerrainRect(GetMinTile(), GetMaxTile()).GetNearestPassableTileTo(tile, options, requester, ignore, mustBeSpawnable, checkPath);
	}

	public override void BuildSubDisplayName(StringBuilder sb)
	{
		Community community = GetCommunity();
		if (community != null && (InfoScreen.AllowViewInfoOnAnyone || community.CommunityType != CommunityType.Player || community.GetLivingNonZombieMemberCount() > 1 || community.Buildings.Count != 0) && (InfoScreen.AllowViewInfoOnAnyone || Session.Instance.Editor || community.CommunityNameKnown || community.IsAnimalCommunity() || community.IsAlwaysHostileCommunity()))
		{
			community.BuildDisplayName(sb, noStrangers: true, englishOnly: false);
		}
	}

	public virtual void OnPickedUpBy(Character character, bool startTransition)
	{
	}

	public virtual void OnDroppedBy(Character character)
	{
	}

	public virtual bool IsBeingCarried()
	{
		return false;
	}

	protected void AddToActiveMovingUnityObjects()
	{
		if (!Session.Instance.ActiveMovingUnityObjects.Contains(this))
		{
			Session.Instance.ActiveMovingUnityObjects.Add(this);
		}
	}

	protected void RemoveFromActiveMovingUnityObjects()
	{
		Session.Instance.ActiveMovingUnityObjects.Remove(this);
	}

	public override void UnityDelete()
	{
		if (IsUnityObjectActive())
		{
			UnityDeactivate();
		}
	}

	public virtual void UnityActivate()
	{
		if (!IsUnityObjectStatic() && !IsGhost())
		{
			AddToActiveMovingUnityObjects();
		}
	}

	public virtual void UnityDeactivate()
	{
		if (!IsUnityObjectStatic() && !IsGhost())
		{
			RemoveFromActiveMovingUnityObjects();
		}
	}

	public virtual void UnityUpdate()
	{
	}

	public virtual void UnityUpdateLayer(bool pip, Vector3 camPos, float farClipPlane)
	{
	}

	public virtual void UnitySendPhysicsStateToClients(InputFrame inputFrame)
	{
	}

	public virtual void OnPostRender()
	{
	}

	public virtual bool HasUnityObject()
	{
		return false;
	}

	public virtual bool HasUnityObjectPool()
	{
		return false;
	}

	public virtual bool IsUnityObjectActive()
	{
		return false;
	}

	public virtual bool IsUnityObjectStatic()
	{
		return true;
	}

	public virtual bool IsUnityObjectAlwaysActive()
	{
		return false;
	}

	public virtual bool WantUnityObjectVisible()
	{
		return true;
	}

	public virtual bool WantUnityObjectToStayActive()
	{
		if (!IsUnityObjectAlwaysActive() && LastFrameWithinUnityActivationRange < Time.frameCount)
		{
			return IsPredicted();
		}
		return true;
	}

	public void MarkWithinUnityActivationRange()
	{
		LastFrameWithinUnityActivationRange = Time.frameCount;
	}

	public virtual PrefabResource GetUnityModel()
	{
		return null;
	}

	public virtual void OnUnityModelChanged()
	{
	}

	public virtual GameObject GetUnityObject()
	{
		return null;
	}

	public virtual AudioSource GetUnityAudioSource()
	{
		GameObject unityObject = GetUnityObject();
		if (!(unityObject != null))
		{
			return null;
		}
		return unityObject.GetComponent<AudioSource>();
	}

	public virtual Vector3 GetUnityPos()
	{
		return Pos;
	}

	public virtual float GetUnityOverheadIconYOffset()
	{
		return GetBoundingBox().size.y;
	}

	public virtual void OnRigidBodyMove(Vector3 pos, Quaternion rot, float wheelRPM)
	{
	}

	public virtual void OnRigidBodyStopMoving(Vector3 pos, Quaternion rot)
	{
	}

	public bool CanUnityObjectBeActivated()
	{
		if (HasUnityObject() || HasUnityObjectPool() || IsBeingPredicted() || GetBaseObjectType() == BaseObjectType.Zone || GetBaseObjectType() == BaseObjectType.InvisibleWall)
		{
			if (!Deleted)
			{
				return WantUnityObjectVisible();
			}
			return false;
		}
		return false;
	}

	public void UnityReinit()
	{
		bool num = HasUnityObject();
		bool flag = IsUnityObjectActive();
		if (flag)
		{
			UnityDeactivate();
		}
		if (num)
		{
			UnityDelete();
		}
		if (num)
		{
			UnityInit();
		}
		if (flag)
		{
			UnityActivate();
		}
	}

	public virtual bool ShouldPlaySoundInPip()
	{
		return Hud.Instance.Pip.FocusObject == this;
	}

	public void PlaySoundOneShotFromList(List<Resource<AudioClip>> clips)
	{
		PlaySoundOneShotFromList(clips, 1f);
	}

	public virtual void PlaySoundOneShotFromList(List<Resource<AudioClip>> clips, float volume)
	{
		AudioSource unityAudioSource = GetUnityAudioSource();
		if (unityAudioSource == null || !IsUnityObjectActive() || clips.Count == 0)
		{
			return;
		}
		if (SoundManager.Instance.IsTooFarAwayToHearSound(Pos))
		{
			if (ShouldPlaySoundInPip())
			{
				SoundManager.PlayPipSoundFromList(clips, volume);
			}
		}
		else
		{
			unityAudioSource.volume = SoundManager.WorldSoundVolume;
			unityAudioSource.PlayOneShot(clips[MathUtil.NonDeterministicRand.Next(clips.Count)], volume);
		}
	}

	public void PlaySoundOneShot(AudioClip clip)
	{
		PlaySoundOneShot(clip, 1f);
	}

	public virtual void PlaySoundOneShot(AudioClip clip, float volume)
	{
		AudioSource unityAudioSource = GetUnityAudioSource();
		if (unityAudioSource == null || !IsUnityObjectActive())
		{
			return;
		}
		if (SoundManager.Instance.IsTooFarAwayToHearSound(Pos))
		{
			if (ShouldPlaySoundInPip())
			{
				SoundManager.PlayPipSound(clip, volume);
			}
		}
		else
		{
			unityAudioSource.volume = SoundManager.WorldSoundVolume;
			unityAudioSource.PlayOneShot(clip, volume);
		}
	}

	public void UnitySetupIdBehaviour()
	{
		GameObject unityObject = GetUnityObject();
		if (unityObject != null)
		{
			IdBehaviour idBehaviour = unityObject.GetComponent<IdBehaviour>();
			if (idBehaviour == null)
			{
				idBehaviour = unityObject.AddComponent<IdBehaviour>();
			}
			idBehaviour.enabled = true;
			idBehaviour.Id = Id;
		}
	}

	public virtual int GetMiningProgress(MineralType mineralType)
	{
		return 0;
	}

	public virtual int GetMiningProgressNeededToExtract(MineralType mineralType)
	{
		return MoveToAndMine.ProgressNeededToExtractRichDeposits;
	}

	public virtual int GetMiningResourceRemaining(MineralType mineralType)
	{
		return 0;
	}

	public virtual void IncrementMiningProgress(MineralType mineralType, int v)
	{
	}

	public virtual void ExtractMiningResource(MineralType mineralType)
	{
	}

	public virtual EquipmentPrototype GetMiningResourceType()
	{
		return null;
	}

	public virtual MineralType GetMineralType()
	{
		return MineralType.None;
	}

	public bool Mine(MineralType mineralType)
	{
		if (GetMiningProgress(mineralType) > 0)
		{
			if (GetMiningProgress(mineralType) >= GetMiningProgressNeededToExtract(mineralType))
			{
				IncrementMiningProgress(mineralType, -GetMiningProgressNeededToExtract(mineralType));
				if (GetMiningResourceRemaining(mineralType) > 0)
				{
					ExtractMiningResource(mineralType);
				}
				if (GetMiningResourceRemaining(mineralType) <= 0)
				{
					SoundManager.PlaySound3DFromList(SoundManager.DemolishSounds, Pos);
					if (WantDebrisOnDemolition())
					{
						GameTerrain.Instance.SetDebris(GetMinTile(), GetMaxTile());
					}
					Delete();
				}
				return true;
			}
			IncrementMiningProgress(mineralType, 1);
		}
		return false;
	}

	public override Texture2D GetIconResource()
	{
		EquipmentPrototype grabbableEquipmentType = GetGrabbableEquipmentType();
		if (grabbableEquipmentType != null && grabbableEquipmentType.Tex != null && grabbableEquipmentType.Tex.GetAsset() != null)
		{
			return grabbableEquipmentType.Tex;
		}
		EquipmentPrototype miningResourceType = GetMiningResourceType();
		if (miningResourceType != null && miningResourceType.Tex != null && miningResourceType.Tex.GetAsset() != null)
		{
			return miningResourceType.Tex;
		}
		return base.GetIconResource();
	}

	public virtual Vector2[] GetConnectors()
	{
		return null;
	}
}
