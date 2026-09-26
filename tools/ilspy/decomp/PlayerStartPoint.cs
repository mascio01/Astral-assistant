using System.Text;
using UnityEngine;

public class PlayerStartPoint : SpawnPoint
{
	public float Angle;

	private static string DisplayNameStr = "PlayerStartPoint";

	private static PrefabResource Model = new PrefabResource("Prefabs/SpawnPoints/PlayerStartPoint");

	public override Color32 MapColor
	{
		get
		{
			if (!Session.Instance.Editor && !SpawnPoint.DebugShowSpawnPoints)
			{
				return MathUtil.TransparentBlack;
			}
			return GameTerrain.MinimapSettings.FriendCol;
		}
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.PlayerStartPoint;
	}

	public static PlayerStartPoint Spawn(TerrainCoord tile, float angle)
	{
		PlayerStartPoint playerStartPoint = new PlayerStartPoint();
		playerStartPoint.Tile = tile;
		playerStartPoint.Angle = angle;
		playerStartPoint.Init();
		return playerStartPoint;
	}

	public override void BuildDisplayName(StringBuilder sb, bool noStrangers, bool englishOnly)
	{
		sb.Append(DisplayNameStr);
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.AddAfter(ref Angle, 3);
	}

	public override void Init()
	{
		base.Init();
		StoryManager.Instance.PlayerStartPoints.Add(this);
	}

	public override void Delete()
	{
		StoryManager.Instance.PlayerStartPoints.Remove(this);
		base.Delete();
	}

	public override void UnityInit()
	{
		base.UnityInit();
		UnityObj.transform.eulerAngles = new Vector3(0f, Angle, 0f);
	}

	public void SetAngle(float angle)
	{
		Angle = angle;
		UnityObj.transform.eulerAngles = new Vector3(0f, Angle, 0f);
	}

	public float GetAngle()
	{
		return Angle;
	}
}
