using System;
using UnityEngine;

public class RadioProp : Prop
{
	private Character Source;

	private TimeSpan LastSoundTime;

	private TimeSpan StartedTime;

	private Vector3 Position;

	private Quaternion Rot;

	private static PrefabResource Model = new PrefabResource("Prefabs/Equipment/Radio");

	public static float Range = 24f;

	public override int NameHash => EquipmentPrototype.Radio.NameHash;

	public override Vector3 Pos => Position;

	public override Vector2 PosXZ => MathUtil.ToXZ(Position);

	public override Color32 MapColor => GameTerrain.MinimapSettings.RadioCol;

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref Source);
		reflector.Add(ref LastSoundTime);
		reflector.AddAfter(ref StartedTime, 148);
		reflector.Add(ref Position);
		reflector.Add(ref Rot);
	}

	public static RadioProp Spawn(Character source, Vector3 pos, Quaternion rot)
	{
		RadioProp radioProp = new RadioProp();
		radioProp.Source = source;
		radioProp.Tile = GameTerrain.Instance.GetTileCoordForPosXZ(MathUtil.ToXZ(pos));
		radioProp.Position = pos;
		radioProp.Rot = rot;
		radioProp.StartedTime = Session.Instance.PlayTime;
		radioProp.OnSpawn();
		return radioProp;
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.RadioProp;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}

	public override Matrix4x4 GetCustomModelTransform()
	{
		return Matrix4x4.TRS(Position, Rot, Vector3.one);
	}

	public override bool IsTargetable()
	{
		return true;
	}

	public override EquipmentPrototype GetGrabbableEquipmentType()
	{
		return EquipmentPrototype.Radio;
	}

	public override bool IsImpassableProp()
	{
		return false;
	}

	public override void Init()
	{
		base.Init();
		GameTerrain.Instance.RadioMapWho.AddToMapWho(this, Tile);
		Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
	}

	public override void Delete()
	{
		Session.Instance.PropManager.RemoveFromObjectsThatNeedUpdating(this);
		GameTerrain.Instance.RadioMapWho.RemoveFromMapWho(this, Tile);
		base.Delete();
	}

	public override void SetTile(TerrainCoord tile)
	{
		if (InTerrain)
		{
			GameTerrain.Instance.RadioMapWho.OnMoved(this, Tile, tile);
		}
		base.SetTile(tile);
	}

	public bool IsStillPlaying()
	{
		return Session.Instance.PlayTime - StartedTime < Sun.DayLength;
	}

	public override void PropUpdate(TimeSpan dt, ref bool stillNeedUpdating)
	{
		if (IsStillPlaying())
		{
			Session instance = Session.Instance;
			if (instance.PlayTime - LastSoundTime >= TimeSpan.FromSeconds(1.0))
			{
				instance.AISoundManager.AddSound(new AISound(AISoundType.Radio, Pos, Range, 0f, this, Source, null, null));
				LastSoundTime = instance.PlayTime;
			}
			stillNeedUpdating = true;
		}
		else if (UnityObj != null)
		{
			AudioSource component = UnityObj.GetComponent<AudioSource>();
			if (component != null && component.isPlaying)
			{
				component.Stop();
			}
		}
		base.PropUpdate(dt, ref stillNeedUpdating);
	}

	public override void UnityInit()
	{
		base.UnityInit();
		AudioSource audioSource = UnityObj.AddComponent<AudioSource>();
		audioSource.loop = true;
		audioSource.spatialBlend = 1f;
		audioSource.minDistance = SoundManager.AudioRolloffMinDist;
		audioSource.maxDistance = 48f;
		audioSource.RealisticRolloff();
		audioSource.clip = SoundManager.RadioMusic[0];
		UnityObj.AddComponent<AudioSourceVolumeBehaviour>();
		if (IsStillPlaying())
		{
			audioSource.Play();
		}
	}
}
