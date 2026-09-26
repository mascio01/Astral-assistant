using System;
using System.Collections.Generic;
using UnityEngine;

public class FallenTreeProp : Prop
{
	public TreeType TreeType;

	public TreeType TreeTypeIncludingGrowth;

	public float Rotation;

	public float Scale;

	public float FallingTransition = 1f;

	public bool FastFall;

	private OrientationType _fallDirection;

	public int WoodRemaining = 1;

	public int ChoppedLogProgress;

	public TimeSpan RegrowTime;

	private static int Name = StringUtil.JenkinsHash("PROP_FallenTree");

	private static float FallingTime = 1.5f;

	private static float FastFallingTime = 0.5f;

	private const string Stump = "Stump";

	private static float TrunkWidth = 0.025f;

	public OrientationType FallDirection => (OrientationType)(((int)_fallDirection + (int)Orientation) % 4);

	public override string Category => "Nature/Fallen Trees";

	public override int NameHash => Name;

	public override TerrainCoord ExtentsMin => new TerrainCoord(0, 0);

	public override TerrainCoord ExtentsMax => new TerrainCoord(0, 0);

	public override CoverType CoverType => CoverType.WaistHigh;

	public static FallenTreeProp Spawn(TreeProp treeProp)
	{
		FallenTreeProp fallenTreeProp = new FallenTreeProp();
		fallenTreeProp.Tile = treeProp.Tile;
		fallenTreeProp.Orientation = OrientationType.Deg0;
		fallenTreeProp.TreeType = treeProp.TreeType;
		fallenTreeProp.TreeTypeIncludingGrowth = treeProp.GetTreeTypeIncludingGrowth();
		fallenTreeProp.Rotation = treeProp.Rotation;
		fallenTreeProp.Scale = treeProp.GetScaleIncludingGrowth();
		fallenTreeProp.FallingTransition = 0f;
		fallenTreeProp.OnSpawn();
		return fallenTreeProp;
	}

	public static FallenTreeProp Spawn(TerrainCoord tile, TreeType treeType, CustomRandom rand)
	{
		FallenTreeProp obj = new FallenTreeProp
		{
			Tile = tile,
			Orientation = OrientationType.Deg0
		};
		obj.TreeType = (obj.TreeTypeIncludingGrowth = treeType);
		obj.Rotation = rand.RandomFloat() * (MathF.PI * 2f);
		obj.Scale = TreeProp.PickScale(rand);
		obj.FallingTransition = 1f;
		obj.OnSpawn();
		return obj;
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.FallenTreeProp;
	}

	public override BulletHitEffect GetBulletHitEffect(Vector3 nondeterministicHitPos)
	{
		return BulletHitEffect.Smoke;
	}

	public override PrefabResource GetUnityModel()
	{
		return GameTerrain.Trees[(int)TreeTypeIncludingGrowth];
	}

	public int GetChopsNeededToHarvestWood()
	{
		return 1;
	}

	public override Flammability GetFlammability()
	{
		return Flammability.Medium_RequiresFuel;
	}

	public override float GetMaxDamage()
	{
		return 20f;
	}

	public override bool IsImpassableProp()
	{
		return true;
	}

	public override float CalcBoundingBoxHeight()
	{
		return 1f;
	}

	public override Texture2D GetIconResource()
	{
		return Prop.StumpIcon;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref TreeType);
		reflector.AddAfter(ref TreeTypeIncludingGrowth, 30);
		reflector.Add(ref Rotation);
		reflector.Add(ref Scale);
		reflector.Add(ref FallingTransition);
		reflector.AddAfter(ref FastFall, 596);
		reflector.Add(ref _fallDirection);
		reflector.Add(ref ChoppedLogProgress);
		reflector.AddAfter(ref RegrowTime, 108);
		if (reflector.Version < 108)
		{
			float value = 0f;
			reflector.AddAfter(ref value, 30);
		}
		if (reflector.Version < 2)
		{
			WoodRemaining = CalcFallenLength();
			return;
		}
		reflector.Add(ref WoodRemaining);
		if (reflector.Version < 108 && WoodRemaining == 0)
		{
			RegrowTime = Session.Instance.PlayTime + TimeSpan.FromSeconds((float)Weather.DaysInAMonth * Sun.DayLengthSecs * MathUtil.NonDeterministicRand.RandomFloat());
		}
	}

	public override void OnSpawn()
	{
		base.OnSpawn();
		WoodRemaining = Math.Max(1, CalcFallenLength() / 2);
	}

	public override void Init()
	{
		base.Init();
		if (FallingTransition < 1f)
		{
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
		}
		if (WoodRemaining == 0)
		{
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
		}
	}

	public override void Delete()
	{
		Session.Instance.PropManager.RemoveFromObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
		base.Delete();
	}

	public int CalcFallenLength()
	{
		return Math.Max(1, (int)(GetUnityModel().Height * Scale));
	}

	public void StartFalling(Vector2 preferredDirXZ, TileObject chopper)
	{
		SoundManager.PlaySound3D(SoundManager.TreeFallSound, Pos, 0.5f);
		Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
		int num = CalcFallenLength();
		List<OrientationType> list = new List<OrientationType>();
		float num2 = float.MaxValue;
		for (int i = 0; i < 4; i++)
		{
			OrientationType orientationType = (OrientationType)i;
			TerrainCoord dirFromOrientationType = Prop.GetDirFromOrientationType((OrientationType)(((int)orientationType + (int)Orientation) % 4));
			float num3 = 0f - Vector2.Dot(preferredDirXZ, new Vector2(dirFromOrientationType.x, dirFromOrientationType.y));
			for (int j = 1; j < num; j++)
			{
				TerrainCoord terrainCoord = Tile + dirFromOrientationType * j;
				if (GameTerrain.Instance.IsImpassable(terrainCoord.x, terrainCoord.y, 257, null, chopper))
				{
					num3 += 10000f;
					continue;
				}
				for (int k = -2; k <= 2; k++)
				{
					TerrainCoord terrainCoord2 = terrainCoord + MathUtil.RightNormal(dirFromOrientationType);
					TileObject fixedObjectOnTile = GameTerrain.Instance.GetFixedObjectOnTile(terrainCoord2.x, terrainCoord2.y);
					if (fixedObjectOnTile != null)
					{
						num3 += 1f;
						if (fixedObjectOnTile is Campfire)
						{
							num3 += 100f;
						}
					}
				}
			}
			if (num3 <= num2)
			{
				if (num3 < num2)
				{
					list.Clear();
				}
				list.Add(orientationType);
				num2 = num3;
			}
		}
		_fallDirection = list[MathUtil.RandomInt(Id, list.Count)];
		FastFall = chopper is EnterableVehicle;
	}

	public override void PropUpdate(TimeSpan dt, ref bool stillNeedUpdating)
	{
		if (FallingTransition < 1f)
		{
			FallingTransition += (float)dt.TotalSeconds / (FastFall ? FastFallingTime : FallingTime);
			stillNeedUpdating = FallingTransition < 1f;
			UnityUpdateBendiness(UnityObj);
			UnityUpdateFalling();
		}
		base.PropUpdate(dt, ref stillNeedUpdating);
	}

	public override void PropUpdateRare(ref bool stillNeedUpdating)
	{
		if (WoodRemaining == 0)
		{
			stillNeedUpdating = true;
			if (Session.Instance.PlayTime >= RegrowTime && GameTerrain.Instance.GetTreePropOnTile(Tile.x, Tile.y) == null)
			{
				TreeProp.Spawn(TreeTypeIncludingGrowth, Tile, 0.2f, Session.Instance.DeterministicRand);
			}
		}
		base.PropUpdateRare(ref stillNeedUpdating);
	}

	public override bool PropWantDelete()
	{
		if (WoodRemaining != 0 || !(Session.Instance.PlayTime >= RegrowTime))
		{
			return base.PropWantDelete();
		}
		return true;
	}

	public override void UnityInit()
	{
		LOD[] lODs = GetUnityModel().GetAsset().GetComponent<LODGroup>().GetLODs();
		GameObject original = null;
		GameObject original2 = null;
		Renderer[] renderers = lODs[0].renderers;
		foreach (Renderer renderer in renderers)
		{
			if (renderer.gameObject.name.Contains("Stump"))
			{
				original = renderer.gameObject;
			}
			else
			{
				original2 = renderer.gameObject;
			}
		}
		UnityObj = UnityEngine.Object.Instantiate(original, (Id != 0) ? GameTerrain.Instance.UnityTerrainObj.transform : null, worldPositionStays: true);
		UnityEngine.Object.Instantiate(original2, UnityObj.transform, worldPositionStays: true).layer = Character.GibLayer;
		UnityObj.layer = Character.GibLayer;
		UnityObj.name = GetDisplayNameString();
		UpdateUnityTransform();
		UnityUpdateBendiness(UnityObj);
		UnityUpdateFalling();
		UnityObj.SetActive(value: false);
		UnitySetupIdBehaviour();
		if (IsUnityObjectAlwaysActive() || Session.Instance.IsInFocusArea(GetTileRect()))
		{
			UnityActivate();
		}
	}

	public override Matrix4x4 GetCustomModelTransform()
	{
		Matrix4x4 mat = GetUnityModel().LocalToWorldMatrix * MathUtil.CreateRotationY(Rotation) * MathUtil.CreateScale(Scale);
		MathUtil.SetTranslation(ref mat, ModelOffset);
		return World * mat;
	}

	private void UnityUpdateBendiness(GameObject obj)
	{
		MeshRenderer component = obj.GetComponent<MeshRenderer>();
		Material[] materials = component.materials;
		for (int i = 0; i < materials.Length; i++)
		{
			float num = component.sharedMaterials[i].GetFloat(ShaderHash._Bendiness);
			materials[i].SetFloat(ShaderHash._Bendiness, (1f - FallingTransition) * num);
		}
		for (int j = 0; j < obj.transform.childCount; j++)
		{
			UnityUpdateBendiness(obj.transform.GetChild(j).gameObject);
		}
	}

	private void UnityUpdateFalling()
	{
		if (FallingTransition > 0f && UnityObj != null)
		{
			int num = CalcFallenLength();
			float num2 = -MathF.PI;
			for (int i = 1; i < num; i++)
			{
				TerrainCoord tile = Tile + Prop.GetDirFromOrientationType(FallDirection) * i;
				float val = (float)Math.Atan2(GameTerrain.Instance.GetTileCentrePos(tile).y - Pos.y, (float)i);
				num2 = Math.Max(num2, val);
			}
			float num3 = (MathF.PI / 2f - num2) * 57.29578f;
			GameObject gameObject = UnityObj.transform.GetChild(0).gameObject;
			Vector3 vector = MathUtil.ToX0Y(MathUtil.GetDirFromAngle(Prop.GetAngleFromOrientationType(_fallDirection) + MathF.PI / 2f));
			Vector3 axis = MathUtil.CreateRotationY(0f - Rotation).MultiplyVector(vector);
			gameObject.transform.localRotation = Quaternion.AngleAxis(num3 * FallingTransition * FallingTransition, axis);
			gameObject.transform.localPosition = new Vector3(0f, TrunkWidth * Scale * FallingTransition * FallingTransition, 0f);
			gameObject.SetActive(WoodRemaining > 0);
		}
	}

	public bool IsStump()
	{
		return WoodRemaining == 0;
	}

	public override bool CanBeClearedForBuilding(Community builderCommunity, TileObject newBuilding)
	{
		return IsStump();
	}

	public override float OnVehicleCollision(BaseObject other, Vector3 relativeVelocity, Vector3 contactPoint, bool predicted)
	{
		if (FallingTransition < 1f)
		{
			return 0f;
		}
		return base.OnVehicleCollision(other, relativeVelocity, contactPoint, predicted);
	}

	public override void OnTerrainHeightChanged()
	{
		base.OnTerrainHeightChanged();
		UnityUpdateFalling();
	}

	public override void SetTile(TerrainCoord tile)
	{
		base.SetTile(tile);
		UnityUpdateFalling();
	}

	public override void SetOrientationType(OrientationType orientation)
	{
		base.SetOrientationType(orientation);
		UnityUpdateFalling();
	}

	public void ChopWood(Character character)
	{
		Session.Instance.AISoundManager.AddSound(new AISound(AISoundType.Suspicious, character.Pos, 16f, 16f, character, character, null, null));
		int num = 1;
		if (!IsStump())
		{
			ChoppedLogProgress += num;
			Community communityThatOwnsThisArea = GetCommunityThatOwnsThisArea();
			if (communityThatOwnsThisArea != null && !character.Community.CachedAllies.Contains(communityThatOwnsThisArea))
			{
				character.OnStoleSomething(communityThatOwnsThisArea, null, EquipmentPrototype.Wood.BasePrice);
			}
		}
		character.Skillset.AddProgress(character, SkillType.Strength, num);
	}

	public void HarvestWood(Character character, bool gathering)
	{
		int num = 0;
		if (ChoppedLogProgress >= GetChopsNeededToHarvestWood() && WoodRemaining > 0)
		{
			num++;
			ChoppedLogProgress -= GetChopsNeededToHarvestWood();
			WoodRemaining--;
			if (WoodRemaining == 0)
			{
				RegrowTime = Session.Instance.PlayTime + TimeSpan.FromSeconds((float)Weather.DaysInAMonth * Sun.DayLengthSecs);
			}
		}
		if (num > 0)
		{
			Equipment equipment = character.Inventory.Add(character, Equipment.Spawn(EquipmentPrototype.Wood, num));
			if (gathering)
			{
				equipment.IncrementGatheredAmount(num);
			}
			NotificationManager.Instance.AddEquipmentNotification(null, character, equipment, num);
			character.Skillset.AddProgress(character, SkillType.Construction, num);
			UnityUpdateFalling();
			Session.Instance.AISoundManager.AddSound(new AISound(AISoundType.Suspicious, character.Pos, 16f, 16f, character, character, null, null));
			Community communityThatOwnsThisArea = GetCommunityThatOwnsThisArea();
			if (communityThatOwnsThisArea != null && !character.Community.CachedAllies.Contains(communityThatOwnsThisArea))
			{
				character.OnStoleSomething(communityThatOwnsThisArea, null, EquipmentPrototype.Wood.BasePrice);
			}
		}
		else
		{
			ChopWood(character);
		}
	}

	public override bool IsTargetable()
	{
		return true;
	}
}
