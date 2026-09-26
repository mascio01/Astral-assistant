using System;
using UnityEngine;

public class TreeProp : SingleTileObject
{
	public static float Radius = 0.14f;

	public TreeType TreeType;

	public float Rotation;

	public float Scale;

	public int ChoppedDownProgress;

	public float Growth = 1f;

	public TimeSpan LastUpdateTime;

	protected bool _deleted;

	private static int Name = StringUtil.JenkinsHash("PROP_Tree");

	private static float TimeTillFullGrown = Sun.DayLengthSecs * (float)Weather.DaysInAMonth * 3f;

	private float LastUpdatedAtGrowth;

	public override int NameHash => Name;

	public override bool Deleted => _deleted;

	public override Color32 MapColor => GameTerrain.MinimapSettings.TreeCol;

	public override CoverType CoverType => CoverType.Full;

	public override Vector3 Pos => GameTerrain.Instance.GetTileCentreMinHeightPos(Tile);

	public override Vector2 PosXZ => GameTerrain.Instance.GetTileCentreXZ(Tile);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.TreeProp;
	}

	public static float PickScale(CustomRandom rand)
	{
		return Mathf.Lerp(0.75f, 1.25f, rand.RandomFloat()) * 3f;
	}

	public static TreeProp Spawn(TreeType treeType, TerrainCoord tile, float growth, CustomRandom rand)
	{
		TreeProp treeProp = new TreeProp();
		treeProp.TreeType = treeType;
		treeProp.Tile = tile;
		treeProp.Rotation = rand.RandomFloat() * (MathF.PI * 2f);
		treeProp.Scale = PickScale(rand);
		treeProp.Growth = growth;
		treeProp.LastUpdateTime = Session.Instance.PlayTime;
		treeProp.OnSpawn();
		return treeProp;
	}

	public override void OnSpawn()
	{
		base.OnSpawn();
		if (GameTerrain.Instance.UnityTerrainObj != null)
		{
			GameTerrain.Instance.BuildMinimap(GetMinTile(), GetMaxTile());
		}
	}

	public override void Init()
	{
		base.Init();
		GameTerrain.Instance.AddTreeProp(this);
		GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
		GameTerrain.Instance.TreeMapWho.AddToMapWho(this, Tile);
		if (Growth < 1f)
		{
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
		}
	}

	public override void Delete()
	{
		if (Growth < 1f)
		{
			Session.Instance.PropManager.RemoveFromObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
		}
		GameTerrain.Instance.AStar.AddChange(AStarChange.RemovedFixedContents(this));
		GameTerrain.Instance.RemoveTreeProp(this);
		GameTerrain.Instance.TreeMapWho.RemoveFromMapWho(this, Tile);
		base.Delete();
		_deleted = true;
		if (GameTerrain.Instance.UnityTerrainObj != null)
		{
			GameTerrain.Instance.BuildMinimap(GetMinTile(), GetMaxTile());
		}
	}

	public override void SetTile(TerrainCoord tile)
	{
		TerrainCoord tile2 = Tile;
		GameTerrain.Instance.AStar.AddChange(AStarChange.RemovedFixedContents(this));
		GameTerrain.Instance.RemoveTreeProp(this);
		GameTerrain.Instance.TreeMapWho.RemoveFromMapWho(this, Tile);
		base.SetTile(tile);
		GameTerrain.Instance.AddTreeProp(this);
		GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
		GameTerrain.Instance.TreeMapWho.AddToMapWho(this, Tile);
		if (GameTerrain.Instance.UnityTerrainObj != null)
		{
			GameTerrain.Instance.BuildMinimap(TerrainCoord.Min(tile, tile2), TerrainCoord.Max(tile, tile2));
		}
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref TreeType);
		reflector.Add(ref Rotation);
		reflector.Add(ref Scale);
		reflector.Add(ref ChoppedDownProgress);
		reflector.AddAfter(ref Growth, 30);
		reflector.Add(ref _deleted);
		reflector.AddAfter(ref LastUpdateTime, 92);
	}

	public override bool IsDestroyed()
	{
		return _deleted;
	}

	public override bool CanEncloseAnArea()
	{
		return true;
	}

	public override bool IsImpassable(Character requester, int options, TerrainCoord tile)
	{
		return (options & 0x4000) == 0;
	}

	public override bool IsRound()
	{
		return true;
	}

	public override Texture2D GetIconResource()
	{
		return Prop.TreeIcon;
	}

	public override BulletHitEffect GetBulletHitEffect(Vector3 nondeterministicHitPos)
	{
		return BulletHitEffect.Smoke;
	}

	public override Flammability GetFlammability()
	{
		return Flammability.Invulnerable;
	}

	public override Matrix4x4 GetWorld()
	{
		return MathUtil.CreateTranslation(Pos) * MathUtil.CreateRotationY(Rotation);
	}

	public override float GetPlantCoverForCamouflage()
	{
		TreeType treeType = TreeType;
		if ((uint)(treeType - 5) <= 2u)
		{
			return 1f;
		}
		return 0f;
	}

	public override Bounds GetBoundingBox()
	{
		float radius = Radius;
		float num = 10f;
		return MathUtil.CreateBoundsCentreExtents(Pos + new Vector3(0f, num * 0.5f, 0f), new Vector3(radius, num * 0.5f, radius));
	}

	public override float? Raycast(Ray ray, float length, int flags, ref Vector3 normal, ref Bone bone, ref Vector3 hitPosInBoneSpace, ref float plantCover, Character source, TileObject target)
	{
		if ((flags & 4) != 0 && GetBoundingBox().IntersectRay(ray, out var distance) && distance < length)
		{
			return distance;
		}
		if ((flags & 8) != 0 && GetBoundingBox().IntersectRay(ray, out var distance2) && distance2 < length)
		{
			return distance2;
		}
		return null;
	}

	public override bool IsTargetable()
	{
		return true;
	}

	public int GetChopsNeededToCutDown()
	{
		return 1;
	}

	public float GetScaleIncludingGrowth()
	{
		return Growth * Scale;
	}

	public TreeType GetTreeTypeIncludingGrowth()
	{
		return TreeType;
	}

	public override void PropUpdateRare(ref bool stillNeedUpdating)
	{
		base.PropUpdateRare(ref stillNeedUpdating);
		TimeSpan timeSpan = Session.Instance.PlayTime - LastUpdateTime;
		LastUpdateTime = Session.Instance.PlayTime;
		Growth = Math.Min(Growth + (float)timeSpan.TotalSeconds / TimeTillFullGrown, 1f);
		stillNeedUpdating |= Growth < 1f;
		if (Growth - LastUpdatedAtGrowth >= 0.001f)
		{
			int treeTypeIncludingGrowth = (int)GetTreeTypeIncludingGrowth();
			int num = GameTerrain.Instance.TreeProps.IndexOf(this);
			TreeInstance instance = GameTerrain.Instance.UnityTerrain.terrainData.treeInstances[num];
			if (instance.prototypeIndex == treeTypeIncludingGrowth)
			{
				instance.widthScale = (instance.heightScale = GetScaleIncludingGrowth());
				GameTerrain.Instance.UnityTerrain.terrainData.SetTreeInstance(num, instance);
			}
			else
			{
				GameTerrain.Instance.RebuildTreeProps();
			}
			LastUpdatedAtGrowth = Growth;
		}
	}

	public override float OnVehicleCollision(BaseObject other, Vector3 relativeVelocity, Vector3 contactPoint, bool predicted)
	{
		if (other is EnterableVehicle enterableVehicle)
		{
			FallenTreeProp.Spawn(this).StartFalling(MathUtil.SafeNormalize(MathUtil.ToXZ(relativeVelocity), MathUtil.ToXZ(enterableVehicle.Forward)), enterableVehicle);
			Delete();
		}
		return 0f;
	}
}
