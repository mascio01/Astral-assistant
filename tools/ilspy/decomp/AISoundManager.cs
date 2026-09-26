using System;
using System.Collections.Generic;
using UnityEngine;

public class AISoundManager : IReflectable
{
	private List<TileObject> _nearbyObjects = new List<TileObject>();

	public void Reflect(Reflector reflector)
	{
		if (reflector.Version < 235)
		{
			List<AISound> list = new List<AISound>();
			reflector.Add(ref list);
		}
	}

	public void AddSound(AISound sound)
	{
		if (PredictedObjectManager.Instance.IsPredictingFrames)
		{
			Debug.LogWarning("Probably Out Of Sync Error - AI sound triggered during prediction");
		}
		GameTerrain instance = GameTerrain.Instance;
		float num = Math.Max(sound.SoundRadius, sound.SightRadius);
		TerrainCoord tileCoordForPos = instance.GetTileCoordForPos(sound.Pos - new Vector3(num, 0f, num));
		TerrainCoord tileCoordForPos2 = instance.GetTileCoordForPos(sound.Pos + new Vector3(num, 0f, num));
		instance.GetObjectsOfTypeInRect(tileCoordForPos, tileCoordForPos2, _nearbyObjects, typeof(MultiTileObject));
		foreach (TileObject nearbyObject in _nearbyObjects)
		{
			if (sound.Source != null && nearbyObject != sound.Source)
			{
				nearbyObject.OnHearSound(sound);
			}
		}
		GameTerrain.Instance.BirdSongManager.OnSound(sound);
		_nearbyObjects.Clear();
	}
}
