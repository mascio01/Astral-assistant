using System;
using UnityEngine;

public class Boulder : Prop
{
	public int MiningProgress;

	public int ResourceRemaining = 32;

	private static PrefabResource[] Models = new PrefabResource[2]
	{
		new PrefabResource("Prefabs/Nature Package/Rock1"),
		new PrefabResource("Prefabs/Nature Package/Rock2")
	};

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
		return BaseObjectType.Boulder;
	}

	public override Matrix4x4 GetCustomModelTransform()
	{
		Matrix4x4 mat = GetUnityModel().LocalToWorldMatrix;
		MathUtil.SetTranslation(ref mat, ModelOffset);
		return World * mat * MathUtil.CreateRotationY((float)MathUtil.RandomInt(Id + 1000, 4) * (MathF.PI / 2f));
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
		if (!ForceInvulnerable)
		{
			ResourceRemaining--;
		}
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
}
