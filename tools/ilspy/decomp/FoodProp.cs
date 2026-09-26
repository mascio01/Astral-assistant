using System;
using UnityEngine;

public class FoodProp : Prop
{
	public EquipmentPrototype FoodProto;

	public InfectionType Infection;

	public Vector3 Position;

	public Quaternion Rot;

	public float ConsumedAmount;

	public InfectionType InfectedWith;

	public Character ThrownBy;

	public override int NameHash
	{
		get
		{
			if (FoodProto == null)
			{
				return 0;
			}
			return FoodProto.NameHash;
		}
	}

	public override Vector3 Pos => Position;

	public override Vector2 PosXZ => MathUtil.ToXZ(Position);

	public override Color32 MapColor => GameTerrain.MinimapSettings.FlintCol;

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref FoodProto);
		reflector.Add(ref Infection);
		reflector.Add(ref Position);
		reflector.Add(ref Rot);
		reflector.AddAfter(ref ConsumedAmount, 152);
		reflector.AddAfter(ref InfectedWith, 154);
		reflector.AddAfter(ref ThrownBy, 359);
	}

	public static FoodProp Spawn(EquipmentPrototype foodProto, Vector3 pos, Quaternion rot, InfectionType infectedWith, Character thrownBy)
	{
		FoodProp foodProp = new FoodProp();
		foodProp.FoodProto = foodProto;
		foodProp.Tile = GameTerrain.Instance.GetTileCoordForPosXZ(MathUtil.ToXZ(pos));
		foodProp.Position = pos;
		foodProp.Rot = rot;
		foodProp.InfectedWith = infectedWith;
		foodProp.ThrownBy = thrownBy;
		foodProp.OnSpawn();
		return foodProp;
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.FoodProp;
	}

	public override PrefabResource GetUnityModel()
	{
		return FoodProto.EquippedModelProperties.Prefab;
	}

	public override Matrix4x4 GetCustomModelTransform()
	{
		return Matrix4x4.TRS(Position, Rot, FoodProto.EquippedModelProperties.LocalScale * Vector3.one);
	}

	public override bool IsTargetable()
	{
		return true;
	}

	public override EquipmentPrototype GetGrabbableEquipmentType()
	{
		return FoodProto;
	}

	public override InfectionType GetGrabbableEquipmentInfectedWith()
	{
		return InfectedWith;
	}

	public override bool IsImpassableProp()
	{
		return false;
	}

	public override void Init()
	{
		base.Init();
		GameTerrain.Instance.FoodMapWho.AddToMapWho(this, Tile);
	}

	public override void Delete()
	{
		GameTerrain.Instance.FoodMapWho.RemoveFromMapWho(this, Tile);
		base.Delete();
	}

	public override float GetVisibility(Character from, Vector3 eyePos, Vector3 eyeDir, float sightRange, out bool hasLineOfSightIgnoringPlantCover, out bool canHearFootsteps)
	{
		canHearFootsteps = false;
		Vector3 vector = Position - eyePos;
		float magnitude = MathUtil.ToXZ(vector).magnitude;
		if (magnitude > sightRange)
		{
			hasLineOfSightIgnoringPlantCover = false;
			return 0f;
		}
		RaycastResult raycastResult = GameTerrain.Instance.RayCast(new Ray(eyePos, vector), magnitude, 32808, from.InsideBuilding);
		CharacterManager.DeterministicTimeSpentThinking += CharacterManager.ThinkTimeUnit;
		hasLineOfSightIgnoringPlantCover = raycastResult.HitObject == null || raycastResult.HitObject == this;
		if (!hasLineOfSightIgnoringPlantCover)
		{
			return 0f;
		}
		return 1f - Math.Min(1f, raycastResult.PlantCover);
	}

	public override void Consume(Character character, float amount, InfectionType infectionType)
	{
		ConsumedAmount = Math.Min(ConsumedAmount + amount * 10f, 1f);
		InfectedWith = (InfectionType)Math.Max((int)InfectedWith, (int)infectionType);
	}

	public override float GetConsumedAmount()
	{
		return ConsumedAmount;
	}

	public override bool DeleteWhenSkinned()
	{
		return true;
	}
}
