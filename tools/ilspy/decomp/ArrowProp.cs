using System;
using UnityEngine;

public class ArrowProp : SingleTileProp
{
	public bool NonDeterministicHit = true;

	public Vector3 Position;

	public Quaternion Rotation;

	public TimeSpan HitTime;

	public EquipmentPrototype ArrowType;

	private static PrefabResource Model = new PrefabResource("Prefabs/Equipment/Bow/Arrow", 64);

	public static float ArrowLength = 0.75f;

	private static int Name = StringUtil.JenkinsHash("PROP_Arrow");

	public override Color32 MapColor => GameTerrain.MinimapSettings.ArrowCol;

	public override Vector3 Pos => Position;

	public override Vector3 ModelOffset => new Vector3(0f, 0f, 0f - ArrowLength);

	public override int NameHash
	{
		get
		{
			if (ArrowType == null)
			{
				return Name;
			}
			return ArrowType.NameHash;
		}
	}

	public static ArrowProp Spawn(Vector3 pos, Quaternion rotation, EquipmentPrototype arrowType)
	{
		ArrowProp arrowProp = new ArrowProp();
		arrowProp.Tile = GameTerrain.Instance.ClampTileWithinBounds(GameTerrain.Instance.GetTileCoordForPos(pos));
		arrowProp.Position = pos;
		arrowProp.Rotation = rotation;
		arrowProp.HitTime = Session.Instance.PlayTime;
		arrowProp.ArrowType = arrowType;
		arrowProp.OnSpawn();
		return arrowProp;
	}

	public override void Init()
	{
		base.Init();
		Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
		GameTerrain.Instance.ArrowMapWho.AddToMapWho(this, Tile);
	}

	public override void Delete()
	{
		GameTerrain.Instance.ArrowMapWho.RemoveFromMapWho(this, Tile);
		Session.Instance.PropManager.RemoveFromObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
		base.Delete();
	}

	public override void SetTile(TerrainCoord tile)
	{
		GameTerrain.Instance.ArrowMapWho.OnMoved(this, Tile, tile);
		base.SetTile(tile);
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref Position);
		reflector.Add(ref Rotation);
		reflector.AddAfter(ref HitTime, 92);
		if (reflector.Version >= 430)
		{
			reflector.Add(ref ArrowType);
		}
		else
		{
			ArrowType = EquipmentPrototype.Arrow;
		}
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.ArrowProp;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}

	public override void PropUpdateRare(ref bool stillNeedUpdating)
	{
		base.PropUpdateRare(ref stillNeedUpdating);
		stillNeedUpdating = true;
	}

	public override bool PropWantDelete()
	{
		return Session.Instance.PlayTime - HitTime >= Sun.DayLength;
	}

	public override Matrix4x4 GetCustomModelTransform()
	{
		Matrix4x4 mat = GetUnityModel().LocalToWorldMatrix;
		MathUtil.SetTranslation(ref mat, ModelOffset);
		return Matrix4x4.TRS(Position, Rotation, Vector3.one) * mat;
	}

	public void OnUnityNonDeterministicHit()
	{
		NonDeterministicHit = true;
		UpdateUnityObjVisibility(NonDeterministicHit);
	}

	public override void UnityActivate()
	{
		base.UnityActivate();
		UpdateUnityObjVisibility(NonDeterministicHit);
	}

	public override void UnityDeactivate()
	{
		UpdateUnityObjVisibility(visible: true);
		base.UnityDeactivate();
	}

	private void UpdateUnityObjVisibility(bool visible)
	{
		if (UnityObj != null)
		{
			UnityObj.GetComponent<MeshRenderer>().enabled = visible;
		}
	}

	public override bool IsTargetable()
	{
		return true;
	}

	public override GenderType GetGender(Language language = Language.Count)
	{
		if (language == Language.BrazilianPortuguese)
		{
			return GenderType.Female;
		}
		return base.GetGender(language);
	}

	public override EquipmentPrototype GetGrabbableEquipmentType()
	{
		return ArrowType;
	}
}
