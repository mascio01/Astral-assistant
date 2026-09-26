using System.Text;
using UnityEngine;

public class Marker : SpawnPoint
{
	public float Angle;

	protected string UniqueID;

	private static string DisplayNameStr = "Marker";

	private static PrefabResource Model = new PrefabResource("Prefabs/SpawnPoints/Marker", 10);

	public void SetAngle(float angle)
	{
		Angle = angle;
		if (UnityObj != null)
		{
			UnityObj.transform.eulerAngles = new Vector3(0f, Angle, 0f);
		}
	}

	public override void UnityActivate()
	{
		base.UnityActivate();
		if (UnityObj != null)
		{
			UnityObj.transform.eulerAngles = new Vector3(0f, Angle, 0f);
		}
	}

	public float GetAngle()
	{
		return Angle;
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Marker;
	}

	public static Marker Spawn(TerrainCoord tile)
	{
		Marker marker = new Marker();
		marker.Tile = tile;
		marker.Init();
		return marker;
	}

	public override string GetUniqueID()
	{
		return UniqueID;
	}

	public override void SetUniqueID(string id)
	{
		if (!string.IsNullOrEmpty(UniqueID))
		{
			BaseObjectManager.Instance.UnregisterUniqueID(UniqueID, this);
		}
		UniqueID = id;
		if (!string.IsNullOrEmpty(UniqueID))
		{
			BaseObjectManager.Instance.RegisterUniqueID(UniqueID, this);
		}
	}

	public override void Init()
	{
		base.Init();
		if (!string.IsNullOrEmpty(UniqueID))
		{
			BaseObjectManager.Instance.RegisterUniqueID(UniqueID, this);
		}
	}

	public override void Delete()
	{
		if (!string.IsNullOrEmpty(UniqueID))
		{
			BaseObjectManager.Instance.UnregisterUniqueID(UniqueID, this);
		}
		base.Delete();
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref UniqueID);
		reflector.Add(ref Angle);
	}

	public override void BuildDisplayName(StringBuilder sb, bool noStrangers, bool englishOnly)
	{
		sb.Append(DisplayNameStr);
	}

	public override void BuildSubDisplayName(StringBuilder sb)
	{
		sb.Append(UniqueID);
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
