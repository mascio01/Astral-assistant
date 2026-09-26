using System;
using System.Collections.Generic;
using UnityEngine;

public class BirdSongManager
{
	public static List<BirdType> BirdTypes = new List<BirdType>();

	public const int AggroTileSize = 16;

	public static AggroTile[,] AggroTiles;

	private TimeSpan NextBirdCallTime;

	public List<BirdSong> CurrentBirdSongs = new List<BirdSong>();

	public TimeSpan BirdSongTime;

	public static float TimeToStartSingingAfterAggro = 10f;

	public static float TimeToFullSingingAfterAggro = 15f;

	public static float OverallBirdCallProb = 0.005f;

	public static float TreeProb = 0.75f;

	public static float MaxFlyingHeight = 16f;

	private static List<TreeProp> TempTrees = new List<TreeProp>();

	public static void LoadContent()
	{
		AddBirdType("BarredOwl", 4, 2, 3, Distribution.Medium, 1f);
		AddBirdType("BlueJay", 5, 1, 1, Distribution.Common, 0.5f);
		AddBirdType("Chickadee", 10, 1, 3, Distribution.Common, 0.25f);
		AddBirdType("LittleSpottedOwl", 4, 2, 3, Distribution.Medium, 0.5f);
		AddBirdType("LongEaredOwl", 6, 2, 3, Distribution.Medium, 0.5f);
		AddBirdType("NightHawk", 2, 2, 2, Distribution.Medium, 0.75f);
		AddBirdType("Pigeon", 5, 1, 3, Distribution.Common, 1f);
		AddBirdType("RedHeadedWoodpecker", 2, 1, 2, Distribution.Common, 0.5f);
		AddBirdType("RedShoulderedHawk", 1, 1, 2, Distribution.Rare, 0.1f);
		AddBirdType("RedTailHawk", 2, 1, 3, Distribution.Rare, 0.1f);
		AddBirdType("Robin", 5, 1, 2, Distribution.Common, 0.5f);
		AddBirdType("Sparrow", 3, 1, 3, Distribution.Common, 0.25f);
		AddBirdType("VauxsSwift", 1, 1, 2, Distribution.Common, 0.5f);
		AddBirdType("YellowWarbler", 4, 1, 2, Distribution.Common, 0.25f);
	}

	private static void AddBirdType(string name, int count, int timeOfDayMask, int seasonMask, Distribution distribution, float volume)
	{
		BirdType birdType = new BirdType();
		birdType.Name = name;
		birdType.TimeOfDayMask = timeOfDayMask;
		birdType.SeasonMask = seasonMask;
		birdType.Distribution = distribution;
		birdType.Volume = volume;
		for (int i = 0; i < count; i++)
		{
			birdType.BirdCalls.Add(new Resource<AudioClip>("Sounds/Birds/" + name + (i + 1)));
		}
		birdType.TimeOfDayCurve = new AnimationCurve();
		birdType.TimeOfDayCurve.AddKey(new Keyframe(0f, ((float)(timeOfDayMask & 2) != 0f) ? 1f : 0f));
		birdType.TimeOfDayCurve.AddKey(new Keyframe(5f / 24f, ((float)(timeOfDayMask & 2) != 0f) ? 1f : 0f));
		birdType.TimeOfDayCurve.AddKey(new Keyframe(0.25f, ((float)(timeOfDayMask & 1) != 0f) ? 1f : 0f));
		birdType.TimeOfDayCurve.AddKey(new Keyframe(0.75f, ((float)(timeOfDayMask & 1) != 0f) ? 1f : 0f));
		birdType.TimeOfDayCurve.AddKey(new Keyframe(19f / 24f, ((float)(timeOfDayMask & 1) != 0f) ? 1f : 0f));
		birdType.TimeOfDayCurve.AddKey(new Keyframe(1f, ((float)(timeOfDayMask & 2) != 0f) ? 1f : 0f));
		birdType.SeasonCurve = new AnimationCurve();
		birdType.SeasonCurve.AddKey(new Keyframe(0f, ((seasonMask & 1) != 0) ? 1f : 0f));
		birdType.SeasonCurve.AddKey(new Keyframe(0.25f, ((seasonMask & 1) != 0) ? 1f : 0f));
		birdType.SeasonCurve.AddKey(new Keyframe(0.5f, ((seasonMask & 2) != 0) ? 1f : 0f));
		birdType.SeasonCurve.AddKey(new Keyframe(0.75f, ((seasonMask & 2) != 0) ? 1f : 0f));
		birdType.SeasonCurve.AddKey(new Keyframe(1f, ((seasonMask & 1) != 0) ? 1f : 0f));
		BirdTypes.Add(birdType);
	}

	public void InitSize(int size)
	{
	}

	public void Unload()
	{
		Array.Clear(AggroTiles, 0, AggroTiles.Length);
	}

	public void Reflect(Reflector reflector, int size)
	{
		if (reflector.Version < 195 || (reflector.IsFastPath && reflector.Version >= 205))
		{
			return;
		}
		int num = size / 16;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				reflector.Add(ref AggroTiles[i, j].AggroTime);
			}
		}
	}

	public void OnSound(AISound sound)
	{
		if (sound.SoundRadius == 0f)
		{
			return;
		}
		AISoundType type = sound.Type;
		if ((uint)(type - 6) <= 3u || type == AISoundType.Interesting)
		{
			return;
		}
		TerrainCoord tl = GameTerrain.Instance.GetTileCoordForPos(sound.Pos - new Vector3(sound.SoundRadius, 0f, sound.SoundRadius));
		TerrainCoord br = GameTerrain.Instance.GetTileCoordForPos(sound.Pos + new Vector3(sound.SoundRadius, 0f, sound.SoundRadius));
		if (!GameTerrain.Instance.ClampMinMaxTileWithinBounds(ref tl, ref br))
		{
			return;
		}
		tl /= 16;
		br /= 16;
		for (int i = tl.x; i <= br.x; i++)
		{
			for (int j = tl.y; j <= br.y; j++)
			{
				AggroTiles[i, j].AggroTime = Session.Instance.PlayTime;
			}
		}
	}

	public void Update(bool paused)
	{
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		CustomRandom nonDeterministicRand = MathUtil.NonDeterministicRand;
		BirdSongTime += TimeSpan.FromSeconds(paused ? 0f : (1f / 60f));
		if (BirdSongTime >= NextBirdCallTime)
		{
			TerrainCoord terrainCoord = instance2.ClampTileWithinBounds(instance2.GetTileCoordForPos(instance.GameCamera.Focus));
			instance2.TreeMapWho.GetObjectsInRect(terrainCoord - new TerrainCoord(16, 16), terrainCoord + new TerrainCoord(16, 16), TempTrees);
			float num = 1f - TreeProb + (float)TempTrees.Count / 10f * TreeProb;
			float time = instance.DaysSinceStart % 1f;
			float time2 = instance.Weather.CalcSeason() / 4f;
			for (int i = 0; i < BirdTypes.Count; i++)
			{
				BirdType birdType = BirdTypes[i];
				float num2 = birdType.TimeOfDayCurve.Evaluate(time);
				float num3 = birdType.SeasonCurve.Evaluate(time2);
				float songProbability = AggroTiles[terrainCoord.x / 16, terrainCoord.y / 16].GetSongProbability();
				float num4 = 0f;
				switch (birdType.Distribution)
				{
				case Distribution.Common:
					num4 = 1f;
					break;
				case Distribution.Medium:
					num4 = 0.5f;
					break;
				case Distribution.Rare:
					num4 = 0.25f;
					break;
				}
				float probability = num2 * num3 * num4 * songProbability * num * OverallBirdCallProb;
				if (nonDeterministicRand.RandomChoice(probability))
				{
					Vector3 birdPos;
					if (TempTrees.Count > 0 && nonDeterministicRand.RandomChoice(TreeProb))
					{
						TreeProp treeProp = TempTrees[nonDeterministicRand.Next(TempTrees.Count)];
						birdPos = treeProp.Pos + Mathf.Lerp(0.25f, 0.75f, nonDeterministicRand.RandomFloat()) * treeProp.Height * Vector3.up;
					}
					else
					{
						birdPos = instance2.ClampPosToSurface(instance.GameCamera.Focus + MathUtil.ToX0Y(nonDeterministicRand.RandomVec2()) * 16f) + nonDeterministicRand.RandomFloat() * MaxFlyingHeight * Vector3.up;
					}
					BirdSong item = new BirdSong
					{
						BirdPos = birdPos,
						BirdTypeIndex = i,
						NextTime = BirdSongTime + TimeSpan.FromSeconds(Mathf.Lerp(0f, 1f, nonDeterministicRand.RandomFloat())),
						Count = Math.Min(birdType.BirdCalls.Count, nonDeterministicRand.Next() % 5)
					};
					CurrentBirdSongs.Add(item);
				}
			}
			TempTrees.Clear();
			NextBirdCallTime = BirdSongTime + TimeSpan.FromSeconds(Mathf.Lerp(0.1f, 2f, nonDeterministicRand.RandomFloat()));
		}
		for (int j = 0; j < CurrentBirdSongs.Count; j++)
		{
			BirdSong value = CurrentBirdSongs[j];
			if (value.NextTime <= BirdSongTime)
			{
				BirdType birdType2 = BirdTypes[value.BirdTypeIndex];
				int index = nonDeterministicRand.Next() % birdType2.BirdCalls.Count;
				AudioClip audioClip = birdType2.BirdCalls[index];
				SoundManager.PlaySound3D(audioClip, value.BirdPos, birdType2.Volume, isBackground: true, isBush: false);
				BirdSongDebugMenu.LogBirdCall(birdType2.Name, index, value.Count);
				if (value.Count > 1)
				{
					value.NextTime = BirdSongTime + TimeSpan.FromSeconds(audioClip.length + Mathf.Lerp(0.1f, 2f, nonDeterministicRand.RandomFloat()));
					value.Count--;
					CurrentBirdSongs[j] = value;
				}
				else
				{
					CurrentBirdSongs.RemoveAt(j);
					j--;
				}
			}
		}
	}
}
