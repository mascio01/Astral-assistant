using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Zone : MultiTileObject
{
	public TerrainCoord ZoneMin;

	public TerrainCoord ZoneMax;

	public List<ZoneTrigger> Triggers = new List<ZoneTrigger>();

	public List<Character> CharactersInZone = new List<Character>();

	private static string DisplayNameStr = "Trigger Zone";

	public override string Category => "SpawnPoints";

	public override float Height => 0.5f;

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		if (reflector.Version < 455)
		{
			List<string> list = new List<string>();
			reflector.AddStringList(ref list);
			foreach (string item2 in list)
			{
				ZoneTrigger item = new ZoneTrigger
				{
					TriggerName = item2
				};
				Triggers.Add(item);
			}
		}
		else
		{
			reflector.Add(ref Triggers);
		}
		reflector.AddAfter(ref ZoneMin, 214);
		reflector.AddAfter(ref ZoneMax, 214);
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Zone;
	}

	public void UpdateWorldTransformAndBounds()
	{
		GameTerrain instance = GameTerrain.Instance;
		float tileMaxHeight = instance.GetTileMaxHeight((ZoneMin.x + ZoneMax.x) / 2, (ZoneMin.y + ZoneMax.y) / 2);
		Bounds boundingBox = MathUtil.CreateBoundsMinMax(new Vector3((float)ZoneMin.x - instance.HalfSize, tileMaxHeight, (float)ZoneMin.y - instance.HalfSize), new Vector3((float)ZoneMax.x - instance.HalfSize + 0.99f, tileMaxHeight + Height, (float)ZoneMax.y - instance.HalfSize + 0.99f));
		SetBoundingBox(boundingBox);
		Vector3 pos = (boundingBox.max + boundingBox.min) * 0.5f;
		Vector3 scale = boundingBox.max - boundingBox.min;
		World = MathUtil.CreateScale(scale) * MathUtil.CreateTranslation(pos);
	}

	public static Zone Spawn(TerrainCoord tile)
	{
		Zone zone = new Zone();
		zone.ZoneMin = tile;
		zone.ZoneMax = tile;
		zone.OnSpawn();
		return zone;
	}

	public override TerrainCoord GetMinTile()
	{
		return ZoneMin;
	}

	public override TerrainCoord GetMaxTile()
	{
		return ZoneMax;
	}

	public int GetMinTileX()
	{
		return ZoneMin.x;
	}

	public int GetMinTileY()
	{
		return ZoneMin.y;
	}

	public int GetMaxTileX()
	{
		return ZoneMax.x;
	}

	public int GetMaxTileY()
	{
		return ZoneMax.y;
	}

	public void SetMinTileX(int x)
	{
		ZoneMin.x = Math.Min(ZoneMax.x, x);
		UpdateWorldTransformAndBounds();
	}

	public void SetMinTileY(int y)
	{
		ZoneMin.y = Math.Min(ZoneMax.y, y);
		UpdateWorldTransformAndBounds();
	}

	public void SetMaxTileX(int x)
	{
		ZoneMax.x = Math.Max(ZoneMin.x, x);
		UpdateWorldTransformAndBounds();
	}

	public void SetMaxTileY(int y)
	{
		ZoneMax.y = Math.Max(ZoneMin.y, y);
		UpdateWorldTransformAndBounds();
	}

	public override void Init()
	{
		base.Init();
		UpdateWorldTransformAndBounds();
		AddToTerrain();
		StoryManager.Instance.Zones.Add(this);
		for (int i = 0; i < Triggers.Count; i++)
		{
			ZoneTrigger value = Triggers[i];
			value.Trigger = GameImpl.Instance.FindTriggerByUniqueID(Triggers[i].TriggerName);
			Triggers[i] = value;
		}
	}

	public override void Delete()
	{
		StoryManager.Instance.Zones.Remove(this);
		RemoveFromTerrain();
		base.Delete();
	}

	public void OnCharacterEntered(Character character)
	{
		CharactersInZone.Add(character);
		StoryManager.Instance.SetConditionsDirty();
	}

	public void OnCharacterLeft(Character character)
	{
		CharactersInZone.Remove(character);
		StoryManager.Instance.SetConditionsDirty();
	}

	public bool IsCharacterInZone(Character character)
	{
		return CharactersInZone.IndexOf(character) != -1;
	}

	public void Evaluate()
	{
		for (int i = 0; i < Triggers.Count; i++)
		{
			if (CharactersInZone.Count <= 0 || !Triggers[i].CanTrigger())
			{
				continue;
			}
			foreach (Character item in CharactersInZone)
			{
				if (StoryManager.AreAllConditionsSatisfied(Triggers[i].Trigger.Conditions, item, null, this, default(MemoryParam)))
				{
					ZoneTrigger value = Triggers[i];
					value.LastTriggeredTime = Session.Instance.PlayTime;
					Triggers[i] = value;
					StoryManager.QueueEvents(Triggers[i].Trigger.Events, item, null, this, default(MemoryParam));
				}
			}
		}
	}

	private void RegisterWithTerrain()
	{
		GameTerrain.Instance.AddMultiTileObject(ZoneMin, ZoneMax, this);
	}

	private void UnregisterWithTerrain()
	{
		GameTerrain.Instance.RemoveMultiTileObject(ZoneMin, ZoneMax, this);
	}

	public override bool IsTargetable()
	{
		if (!Session.Instance.Editor)
		{
			return SpawnPoint.GetDebugShowSpawnPoints();
		}
		return true;
	}

	public override float? Raycast(Ray ray, float length, int flags, ref Vector3 normal, ref Bone bone, ref Vector3 hitPosInBoneSpace, ref float plantCover, Character source, TileObject target)
	{
		if ((flags & 0x100) == 0)
		{
			return null;
		}
		if (GetBoundingBox().IntersectRay(ray, out var distance) && distance < length)
		{
			return distance;
		}
		return null;
	}

	public override void BuildDisplayName(StringBuilder sb, bool noStrangers, bool englishOnly)
	{
		sb.Append(DisplayNameStr);
	}

	public override void BuildSubDisplayName(StringBuilder sb)
	{
		sb.Append(UniqueID);
	}

	public override void OnTerrainHeightChanged()
	{
		UpdateWorldTransformAndBounds();
	}

	public override void OnPostRender()
	{
		if (Session.Instance.Editor || SpawnPoint.GetDebugShowSpawnPoints())
		{
			Bounds boundingBox = GetBoundingBox();
			DebugGraphics.StartDrawLines(Matrix4x4.identity);
			DebugGraphics.DrawBox(boundingBox.center, boundingBox.extents + new Vector3(0f, 1f, 0f), Color.red);
			DebugGraphics.EndDrawLines();
		}
	}
}
