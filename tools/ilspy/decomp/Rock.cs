using UnityEngine;

public class Rock : SingleTileProp
{
	public int MiningProgress;

	public int ResourceRemaining = 4;

	private static PrefabResource[] UnityRocks = new PrefabResource[5]
	{
		new PrefabResource("Prefabs\\Dynamic Nature\\Rocks and Stones\\prefab_rock_01_Snow", 20),
		new PrefabResource("Prefabs\\Dynamic Nature\\Rocks and Stones\\prefab_rock_02_Snow", 20),
		new PrefabResource("Prefabs\\Dynamic Nature\\Rocks and Stones\\prefab_rock_03_Snow", 20),
		new PrefabResource("Prefabs\\Dynamic Nature\\Rocks and Stones\\prefab_rock_04_Snow", 20),
		new PrefabResource("Prefabs\\Dynamic Nature\\Rocks and Stones\\prefab_rock_05_Snow", 20)
	};

	public static bool DebugShowRocks = true;

	public override Color32 MapColor => GameTerrain.MinimapSettings.GetMineralCol(GetMineralType());

	public override void OnSpawn()
	{
		base.OnSpawn();
		if (Prototype != null)
		{
			ResourceRemaining = Prototype.MineralAmount;
		}
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.AddAfter(ref MiningProgress, 66);
		reflector.AddAfter(ref ResourceRemaining, 66);
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Rock;
	}

	public override bool WantUnityObjectVisible()
	{
		return DebugShowRocks;
	}

	public static bool GetDebugShowRocks()
	{
		return DebugShowRocks;
	}

	public static void SetDebugShowRocks(bool v)
	{
		DebugShowRocks = v;
		Session.Instance.WantFullRefreshOfActiveUnityObjectsInFocusArea = true;
	}

	public override bool IsTargetable()
	{
		return true;
	}

	public override int GetMiningProgress(MineralType mineralType)
	{
		return MiningProgress;
	}

	public override int GetMiningResourceRemaining(MineralType mineralType)
	{
		return ResourceRemaining;
	}

	public override void IncrementMiningProgress(MineralType mineralType, int v)
	{
		MiningProgress += v;
	}

	public override void ExtractMiningResource(MineralType mineralType)
	{
		ResourceRemaining--;
	}

	public override EquipmentPrototype GetMiningResourceType()
	{
		return EquipmentPrototype.MiningResources[(int)GetMineralType()];
	}

	public override MineralType GetMineralType()
	{
		if (Prototype == null)
		{
			return MineralType.Stone;
		}
		return Prototype.MineralType;
	}

	protected override void RegisterWithTerrain()
	{
		base.RegisterWithTerrain();
		GameTerrain.Instance.RockMapWho.AddToMapWho(this, Tile);
	}

	protected override void UnregisterWithTerrain()
	{
		GameTerrain.Instance.RockMapWho.RemoveFromMapWho(this, Tile);
		base.UnregisterWithTerrain();
	}
}
