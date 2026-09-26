using System;
using System.Collections.Generic;
using UnityEngine;

public class Campfire : CraftingProp
{
	private static PrefabResource UnlitModel = new PrefabResource("Prefabs/Props/Fireplace and Cooking Place/Fireplace");

	private static PrefabResource BurningModel = new PrefabResource("Prefabs/Props/Fireplace and Cooking Place/Fireplace_Burning");

	private static PrefabResource BurnedOutModel = new PrefabResource("Prefabs/Props/Fireplace and Cooking Place/Fireplace_BurnedOut");

	private static PrefabResource PotModel = new PrefabResource("Prefabs/Props/Fireplace and Cooking Place/Cookingplace");

	private static PrefabResource FryingPanModel = new PrefabResource("Prefabs/Props/Fireplace and Cooking Place/FryingPanPlace");

	private static PrefabResource SpitRoastRabbitModel = new PrefabResource("Prefabs/Props/Fireplace and Cooking Place/Spitroast_Rabbit");

	private static PrefabResource SpitRoastChickenModel = new PrefabResource("Prefabs/Props/Fireplace and Cooking Place/Spitroast_Chicken");

	private static PrefabResource SpitRoastVenisonModel = new PrefabResource("Prefabs/Props/Fireplace and Cooking Place/Spitroast_Steak");

	private static PrefabResource SpitRoastSomeKindOfMeatModel = new PrefabResource("Prefabs/Props/Fireplace and Cooking Place/Spitroast_TBoneSteak");

	public CampfireState State;

	public float WoodRemaining = 1f;

	public TimeSpan LastBurningTime = Target.Never;

	public bool TemporaryFire;

	private List<MeshRenderer> UnityMeat = new List<MeshRenderer>();

	private static string Meat = "Meat";

	public static float BoundingBoxHeight = 1f;

	public static float WoodBurningTime = Sun.DayLengthSecs;

	public static float StirringTimeFrac = 0.5f;

	private static TimeSpan DeleteTemporaryFiresAfterTime = Sun.DayLength * 7.0;

	public static int PROP_Campfire = StringUtil.JenkinsHash("PROP_Campfire");

	public override int NameHash => PROP_Campfire;

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
		return BaseObjectType.Campfire;
	}

	public override PrefabResource GetUnityModel()
	{
		if (Prototype != null && (int)State < Prototype.Prefabs.Count)
		{
			return Prototype.Prefabs[(int)State];
		}
		return State switch
		{
			CampfireState.Fresh => UnlitModel, 
			CampfireState.Burning => BurningModel, 
			CampfireState.BurnedOut => BurnedOutModel, 
			_ => null, 
		};
	}

	public override bool SupportsVariation()
	{
		return false;
	}

	public override Flammability GetFlammability()
	{
		if (State != CampfireState.Fresh)
		{
			return Flammability.Invulnerable;
		}
		return Flammability.High;
	}

	public override bool IsImpassableProp()
	{
		if (State != CampfireState.Burning)
		{
			return CraftingRecipe != null;
		}
		return true;
	}

	public override void UnityInit()
	{
		base.UnityInit();
		GameObject gameObject = null;
		switch ((CraftingRecipe != null) ? CraftingRecipe.RecipeType : RecipeType.Normal)
		{
		case RecipeType.Campfire_SpitRoast_Rabbit:
			gameObject = ((Prototype != null && 3 < Prototype.Prefabs.Count) ? Prototype.Prefabs[3] : SpitRoastRabbitModel);
			break;
		case RecipeType.Campfire_Pot:
			gameObject = ((Prototype != null && 4 < Prototype.Prefabs.Count) ? Prototype.Prefabs[4] : PotModel);
			break;
		case RecipeType.Campfire_SpitRoast_Venison:
			gameObject = ((Prototype != null && 5 < Prototype.Prefabs.Count) ? Prototype.Prefabs[5] : SpitRoastVenisonModel);
			break;
		case RecipeType.Campfire_SpitRoast_SomeKindOfMeat:
			gameObject = ((Prototype != null && 6 < Prototype.Prefabs.Count) ? Prototype.Prefabs[6] : SpitRoastSomeKindOfMeatModel);
			break;
		case RecipeType.Campfire_SpitRoast_Chicken:
			gameObject = ((Prototype != null && 7 < Prototype.Prefabs.Count) ? Prototype.Prefabs[7] : SpitRoastChickenModel);
			break;
		case RecipeType.Campfire_FryingPan:
			gameObject = ((Prototype != null && 8 < Prototype.Prefabs.Count) ? Prototype.Prefabs[8] : FryingPanModel);
			break;
		}
		if (!(gameObject != null))
		{
			return;
		}
		GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, UnityObj.transform, worldPositionStays: false);
		gameObject2.transform.localPosition = Vector3.zero;
		gameObject2.transform.localRotation = Quaternion.identity;
		gameObject2.transform.localScale = Vector3.one * 0.5f;
		RecipeType recipeType = CraftingRecipe.RecipeType;
		if ((uint)(recipeType - 4) <= 3u)
		{
			for (int i = 0; i < gameObject2.transform.childCount; i++)
			{
				GameObject gameObject3 = gameObject2.transform.GetChild(i).gameObject;
				if (gameObject3.name.StartsWith(Meat))
				{
					MeshRenderer component = gameObject3.GetComponent<MeshRenderer>();
					if (component != null)
					{
						UnityMeat.Add(component);
					}
				}
			}
		}
		foreach (MeshRenderer item in UnityMeat)
		{
			float craftingProgress = GetCraftingProgress();
			item.material.SetFloat(ShaderHash._CookedAmount, craftingProgress);
		}
	}

	public override float CalcBoundingBoxHeight()
	{
		return BoundingBoxHeight;
	}

	public override void UnityDelete()
	{
		UnityMeat.Clear();
		base.UnityDelete();
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref State);
		reflector.Add(ref WoodRemaining);
		reflector.AddAfter(ref LastBurningTime, 620);
		reflector.AddAfter(ref TemporaryFire, 620);
		if (reflector.Version < 68)
		{
			Recipe value = null;
			Character obj = null;
			bool value2 = false;
			float value3 = 0f;
			float value4 = 0f;
			reflector.Add(ref value);
			reflector.Add(ref value3);
			reflector.Add(ref value4);
			reflector.Add(ref value2);
			reflector.Add(ref obj);
			CraftingRecipe = value;
			CraftingTimeSpent = value3;
			CurrentCrafter = obj;
			CraftingFinished = value2;
			CraftingIngredientsNutrition = value4;
		}
	}

	public override CantTransferReason CanTransferEquipmentAway(Equipment item, bool onTradePage)
	{
		if (item.GetPrototype() == EquipmentPrototype.Pot || item.GetPrototype() == EquipmentPrototype.FryingPan)
		{
			if (Inventory.Count > 1)
			{
				return CantTransferReason.CookingPot;
			}
			if (!CraftingFinished && (Community == null || Community.HasAnyLivingNonZombieMembers()))
			{
				return CantTransferReason.CookingPot;
			}
		}
		return CantTransferReason.CanTransfer;
	}

	public override void OnSpawn()
	{
		base.OnSpawn();
		LastBurningTime = Session.Instance.PlayTime;
	}

	public override void Init()
	{
		base.Init();
		if (State == CampfireState.Burning)
		{
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
		}
		else if (TemporaryFire)
		{
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
		}
	}

	public override void Delete()
	{
		if (State == CampfireState.Burning)
		{
			Session.Instance.PropManager.RemoveFromObjectsThatNeedUpdating(this);
		}
		else if (TemporaryFire)
		{
			Session.Instance.PropManager.RemoveFromObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
		}
		base.Delete();
	}

	public override void PropUpdate(TimeSpan dt, ref bool stillNeedUpdating)
	{
		if (!IsBurning())
		{
			return;
		}
		float num = (float)dt.TotalSeconds;
		if (IsCrafting())
		{
			float num2 = Math.Min(num, WoodRemaining * WoodBurningTime);
			if (CraftingRecipe.RecipeType == RecipeType.Campfire_Pot || CraftingRecipe.RecipeType == RecipeType.Campfire_FryingPan)
			{
				num2 *= 1f - StirringTimeFrac;
			}
			CraftingTimeSpent += num2;
			foreach (MeshRenderer item in UnityMeat)
			{
				float craftingProgress = GetCraftingProgress();
				item.material.SetFloat(ShaderHash._CookedAmount, craftingProgress);
			}
		}
		base.PropUpdate(dt, ref stillNeedUpdating);
		WoodRemaining = Math.Max(0f, WoodRemaining - num / WoodBurningTime);
		LastBurningTime = Session.Instance.PlayTime;
		if (WoodRemaining <= 0f)
		{
			SetState(CampfireState.BurnedOut, null);
		}
		stillNeedUpdating |= State == CampfireState.Burning;
	}

	public override void PropUpdateRare(ref bool stillNeedUpdating)
	{
		stillNeedUpdating |= TemporaryFire;
	}

	public override bool PropWantDelete()
	{
		if (TemporaryFire && Session.Instance.PlayTime - LastBurningTime >= DeleteTemporaryFiresAfterTime)
		{
			return true;
		}
		return base.PropWantDelete();
	}

	public override void LightFire(Character character)
	{
		SetState(CampfireState.Burning, character);
	}

	public void PutOutFire(Character character)
	{
		if (State == CampfireState.Burning)
		{
			SetState(CampfireState.Fresh, character);
		}
	}

	public void AddWood(Character character, float amount)
	{
		WoodRemaining += amount;
		if (WoodRemaining > 0f && State == CampfireState.BurnedOut)
		{
			SetState(CampfireState.Fresh, character);
		}
		if ((Community == null || Community.GetLivingNonZombieMemberCount() == 0) && character.IsControllableByPlayer())
		{
			SetCommunity(character.Community);
			SetTemporaryFire(isTemporaryFire: false);
		}
	}

	public void SetTemporaryFire(bool isTemporaryFire)
	{
		if (TemporaryFire != isTemporaryFire)
		{
			if (TemporaryFire && State != CampfireState.Burning)
			{
				Session.Instance.PropManager.RemoveFromObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
			}
			TemporaryFire = isTemporaryFire;
			if (TemporaryFire && State != CampfireState.Burning)
			{
				Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
			}
		}
	}

	public void StirThePot(float time)
	{
		if (State == CampfireState.Burning)
		{
			CraftingTimeSpent += time * StirringTimeFrac;
		}
	}

	public void SetState(CampfireState state, Character source)
	{
		if (state == State)
		{
			return;
		}
		if (State == CampfireState.Burning)
		{
			GameTerrain.Instance.BurningMapWho.RemoveFromMapWho(this, Tile);
			if (TemporaryFire)
			{
				Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
			}
		}
		if (IsImpassableProp())
		{
			GameTerrain.Instance.AStar.AddChange(AStarChange.RemovedFixedContents(this));
		}
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
		State = state;
		if (num)
		{
			UnityInit();
		}
		if (flag)
		{
			UnityActivate();
		}
		UpdateWorldTransformAndBounds();
		if (IsImpassableProp())
		{
			GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
		}
		if (State == CampfireState.Burning)
		{
			if (TemporaryFire)
			{
				Session.Instance.PropManager.RemoveFromObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
			}
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
			GameTerrain.Instance.BurningMapWho.AddToMapWho(this, Tile);
		}
		NotificationManager.Instance.CheckCommunityStats = true;
		if (source == null || source.GetPlayerControllingMe() == null)
		{
			NotificationManager.Instance.SuppressNextAggroNotification = true;
		}
		IconIndex = -1;
	}

	public override void SetCraftingRecipe(Recipe recipe, Character chef, float ingredientsNutrition, InfectionType ingredientsInfectedWith, int craftingDesiredAmount)
	{
		base.SetCraftingRecipe(recipe, chef, ingredientsNutrition, ingredientsInfectedWith, craftingDesiredAmount);
		UnityReinit();
	}

	public override bool IsTargetable()
	{
		return true;
	}

	public override bool IsBurning()
	{
		return State == CampfireState.Burning;
	}

	public override bool IsBurningDueToAttack()
	{
		return false;
	}

	public override float GetFireEffectOnCharacter(Character character)
	{
		if (!IsBurning())
		{
			return 0f;
		}
		return 1f - Mathf.Clamp01((character.PosXZ - PosXZ).magnitude / TileObject.MaxFireHeatRange);
	}

	public override bool IsFriendlyFire(Character source, TileObject target, bool itsATrap, AttackType meleeAttackType)
	{
		return false;
	}

	public override float OnBurned(Character source, TileObject intendedTarget, Vector3 centre, float damage, float damageRadius, float fuel, SkillType skillType, InfectionType infectionType, bool assassinate, SecrecyMode secret)
	{
		WoodRemaining += fuel / TileObject.FuelBurningRateFlOzPerSecond / WoodBurningTime;
		if (State == CampfireState.Fresh)
		{
			SetState(CampfireState.Burning, source);
		}
		return 0f;
	}

	public override float OnExplosionImpact(Character source, TileObject intendedTarget, Vector3 centre, float damage, float damageRadius, bool fromFoundations, SkillType skillType, InfectionType infectionType, bool itsATrap, TileObject bomb)
	{
		if (State == CampfireState.Fresh)
		{
			SetState(CampfireState.Burning, source);
		}
		return 0f;
	}

	public bool HaveAllProductsBeenTaken()
	{
		for (int i = 0; i < Inventory.Count; i++)
		{
			Equipment item = Inventory.GetItem(i);
			if (item.GetPrototype() == EquipmentPrototype.Pot && CraftingRecipe != null && CraftingRecipe.RecipeType == RecipeType.Campfire_Pot)
			{
				if (item.GetLiquidContentsAmount() > 0f)
				{
					return false;
				}
			}
			else if (item.GetPrototype() != EquipmentPrototype.FryingPan || CraftingRecipe == null || CraftingRecipe.RecipeType != RecipeType.Campfire_FryingPan)
			{
				return false;
			}
		}
		return true;
	}

	public override TerrainCoord GetTileToStandOn(Character character)
	{
		TerrainCoord bestTile = TerrainCoord.Invalid;
		float bestDistSq = float.MaxValue;
		TerrainCoord dirFromOrientationType = Prop.GetDirFromOrientationType((OrientationType)((int)(Orientation + 1) % 4));
		TerrainCoord dirFromOrientationType2 = Prop.GetDirFromOrientationType(Orientation);
		TerrainRect.TestTile(character.Tile, GetCentreTile() + dirFromOrientationType2, 1, character, null, ref bestTile, ref bestDistSq);
		TerrainRect.TestTile(character.Tile, GetCentreTile() - dirFromOrientationType2, 1, character, null, ref bestTile, ref bestDistSq);
		if (bestTile == TerrainCoord.Invalid)
		{
			TerrainRect.TestTile(character.Tile, GetCentreTile() + dirFromOrientationType, 1, character, null, ref bestTile, ref bestDistSq);
			TerrainRect.TestTile(character.Tile, GetCentreTile() - dirFromOrientationType, 1, character, null, ref bestTile, ref bestDistSq);
		}
		if (bestTile == TerrainCoord.Invalid)
		{
			bestTile = GetNearestPassableTileTo(character.Tile, 1, character, null);
		}
		return bestTile;
	}

	public override TerrainRect GetStandingArea()
	{
		return TerrainRect.Invalid;
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
