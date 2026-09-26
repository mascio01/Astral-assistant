using System;
using UnityEngine;

public class Bush : SingleTileProp
{
	private static PrefabResource[] UnityBushes = new PrefabResource[5]
	{
		new PrefabResource("Prefabs/Trees/Bush01", 20),
		new PrefabResource("Prefabs/Trees/Bush02", 20),
		new PrefabResource("Prefabs/Trees/Bush03", 20),
		new PrefabResource("Prefabs/Trees/Bush04", 20),
		new PrefabResource("Prefabs/Trees/Bush05", 20)
	};

	public static bool DebugShowBushes = true;

	public static float RustleRadius = 0.5f;

	public override Color32 MapColor => GameTerrain.MinimapSettings.BushCol;

	public static Bush Spawn(TerrainCoord tile)
	{
		Bush bush = new Bush();
		bush.Tile = tile;
		bush.OnSpawn();
		return bush;
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Bush;
	}

	public override float GetPlantCoverForCamouflage()
	{
		if (Prototype == null)
		{
			return 1f;
		}
		return Prototype.PlantCover;
	}

	public override Matrix4x4 GetCustomModelTransform()
	{
		float a = 0.75f;
		float b = 1.25f;
		int unityModelIndexFromPrototype = UnityModelIndexFromPrototype;
		if (Prototype != null && unityModelIndexFromPrototype < Prototype.MinMaxScale.Count)
		{
			a = Prototype.MinMaxScale[unityModelIndexFromPrototype].x;
			b = Prototype.MinMaxScale[unityModelIndexFromPrototype].y;
		}
		return MathUtil.CreateTranslation(Pos) * MathUtil.CreateRotationY(MathUtil.RandomFloat(Id) * (MathF.PI * 2f)) * MathUtil.CreateScale(Mathf.Lerp(a, b, MathUtil.RandomFloat(Id + 1000)) * 3f);
	}

	public override Texture2D GetIconResource()
	{
		return Prop.BushIcon;
	}

	public override bool WantUnityObjectVisible()
	{
		return DebugShowBushes;
	}

	public static bool GetDebugShowBushes()
	{
		return DebugShowBushes;
	}

	public static void SetDebugShowBushes(bool v)
	{
		DebugShowBushes = v;
		Session.Instance.WantFullRefreshOfActiveUnityObjectsInFocusArea = true;
	}

	public override bool IsTargetable()
	{
		return true;
	}
}
