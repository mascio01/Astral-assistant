using System;
using System.Collections.Generic;
using UnityEngine;

public class CropsManager : IReflectable
{
	private TimeSpan _lastUpdate;

	private int CurrentIndex;

	public List<PlantableCrop> AllCrops = new List<PlantableCrop>();

	public List<CropPatch> Patches = new List<CropPatch>();

	public const int MaxTilesPerPatch = 64;

	public const int MaxJoinPatchRange = 32;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref _lastUpdate);
		reflector.Add(ref CurrentIndex);
		reflector.Add(ref Patches);
	}

	public void OnLoad()
	{
		for (int num = Patches.Count - 1; num >= 0; num--)
		{
			Community community = Patches[num].GetCommunity();
			if (Patches[num].CropType == null || community == null)
			{
				Debug.LogWarning("Removing crop patch with " + Patches[num].CropsInPatch.Count + " crops, belonging to " + ((community != null) ? community.GetDisplayNameString() : Patches[num].CommunityId.ToString()) + ", because it has a null CropType");
				Patches.RemoveAt(num);
			}
		}
	}

	public void Add(PlantableCrop crop)
	{
		if (crop.Prototype == null)
		{
			Debug.LogWarning("Not adding crop to crop manager because it has a null prototype");
			return;
		}
		AllCrops.Add(crop);
		TerrainCoord tile = crop.Tile;
		int communityId = crop.CommunityId;
		CropPatch cropPatch = null;
		float num = float.MaxValue;
		foreach (CropPatch patch in Patches)
		{
			if (patch.CommunityId == communityId && patch.CropType == crop.Prototype && tile.IsWithinBounds(patch.MinTile - new TerrainCoord(32, 32), patch.MaxTile + new TerrainCoord(32, 32)))
			{
				float num2 = float.MaxValue;
				for (int i = 0; i < patch.Tiles.Count; i++)
				{
					num2 = Math.Min(num2, patch.Tiles[i].GetDistSquared(tile));
				}
				for (int j = 0; j < patch.CropsInPatch.Count; j++)
				{
					num2 = Math.Min(num2, patch.CropsInPatch[j].Tile.GetDistSquared(tile));
				}
				if (num2 < num)
				{
					cropPatch = patch;
					num = num2;
				}
			}
		}
		if (cropPatch != null)
		{
			cropPatch.MinTile = TerrainCoord.Min(cropPatch.MinTile, tile);
			cropPatch.MaxTile = TerrainCoord.Max(cropPatch.MaxTile, tile);
			cropPatch.CropsInPatch.Add(crop);
			for (int num3 = Patches.Count - 1; num3 >= 0; num3--)
			{
				CropPatch cropPatch2 = Patches[num3];
				if (cropPatch != cropPatch2 && cropPatch2.CommunityId == communityId && cropPatch2.CropType == cropPatch.CropType && TerrainCoord.Overlaps(cropPatch.MinTile, cropPatch.MaxTile, cropPatch2.MinTile, cropPatch2.MaxTile) && cropPatch.Tiles.Count + cropPatch2.Tiles.Count < 64 && cropPatch.CropsInPatch.Count + cropPatch2.CropsInPatch.Count < 64)
				{
					cropPatch.MinTile = TerrainCoord.Min(cropPatch.MinTile, cropPatch2.MinTile);
					cropPatch.MaxTile = TerrainCoord.Max(cropPatch.MaxTile, cropPatch2.MaxTile);
					cropPatch.CropsInPatch.AddRange(cropPatch2.CropsInPatch);
					cropPatch.Tiles.AddRange(cropPatch2.Tiles);
					Patches.Remove(cropPatch2);
				}
			}
		}
		else
		{
			CropPatch cropPatch3 = new CropPatch();
			cropPatch3.CommunityId = communityId;
			cropPatch3.CropType = crop.Prototype;
			cropPatch3.MinTile = (cropPatch3.MaxTile = tile);
			cropPatch3.CropsInPatch.Add(crop);
			Patches.Add(cropPatch3);
		}
		Community community = crop.GetCommunity();
		if (community != null)
		{
			community.CachedPlantedNutritionAmount = -1f;
		}
	}

	public void Remove(PlantableCrop crop)
	{
		AllCrops.Remove(crop);
		foreach (CropPatch patch in Patches)
		{
			if (patch.CropsInPatch.Contains(crop))
			{
				patch.CropsInPatch.Remove(crop);
				if (patch.CropsInPatch.Count == 0 && patch.Tiles.Count == 0)
				{
					Patches.Remove(patch);
				}
				else
				{
					patch.RecalcBounds();
				}
				Community community = patch.GetCommunity();
				if (community != null)
				{
					community.CachedPlantedNutritionAmount = -1f;
				}
				break;
			}
		}
	}

	public void DeletePatchesOwnedByCommunity(int communityId)
	{
		for (int num = Patches.Count - 1; num >= 0; num--)
		{
			if (Patches[num].CommunityId == communityId)
			{
				Patches.RemoveAt(num);
			}
		}
	}

	public void SetPlantableCropType(int communityId, TerrainCoord tile, PropPrototype cropType)
	{
		CropPatch cropPatch = null;
		float num = float.MaxValue;
		foreach (CropPatch patch in Patches)
		{
			if (patch.CommunityId != communityId || !tile.IsWithinBounds(patch.MinTile - new TerrainCoord(32, 32), patch.MaxTile + new TerrainCoord(32, 32)))
			{
				continue;
			}
			if (patch.CropType != cropType)
			{
				patch.Tiles.Remove(tile);
				continue;
			}
			float num2 = float.MaxValue;
			for (int i = 0; i < patch.Tiles.Count; i++)
			{
				num2 = Math.Min(num2, patch.Tiles[i].GetDistSquared(tile));
			}
			if (num2 < num)
			{
				cropPatch = patch;
				num = num2;
			}
		}
		if (cropType == null)
		{
			return;
		}
		if (cropPatch == null)
		{
			CropPatch cropPatch2 = new CropPatch();
			cropPatch2.CommunityId = communityId;
			cropPatch2.CropType = cropType;
			cropPatch2.Tiles.Add(tile);
			cropPatch2.MinTile = (cropPatch2.MaxTile = tile);
			Patches.Add(cropPatch2);
			return;
		}
		if (!cropPatch.Tiles.Contains(tile))
		{
			cropPatch.Tiles.Add(tile);
			cropPatch.MinTile = TerrainCoord.Min(cropPatch.MinTile, tile);
			cropPatch.MaxTile = TerrainCoord.Max(cropPatch.MaxTile, tile);
		}
		if (cropPatch.Tiles.Count <= 64)
		{
			return;
		}
		CropPatch cropPatch3 = new CropPatch();
		cropPatch3.CommunityId = communityId;
		cropPatch3.CropType = cropType;
		if (cropPatch.MaxTile.x - cropPatch.MinTile.x >= cropPatch.MaxTile.y - cropPatch.MinTile.y)
		{
			int num3 = Math.Min((cropPatch.MinTile.x + cropPatch.MaxTile.x) / 2 + 1, cropPatch.GetMaxTileX());
			for (int num4 = cropPatch.Tiles.Count - 1; num4 >= 0; num4--)
			{
				if (cropPatch.Tiles[num4].x >= num3)
				{
					cropPatch3.Tiles.Add(cropPatch.Tiles[num4]);
					cropPatch.Tiles.RemoveAt(num4);
				}
			}
			for (int num5 = cropPatch.CropsInPatch.Count - 1; num5 >= 0; num5--)
			{
				if (cropPatch.CropsInPatch[num5].Tile.x >= num3)
				{
					cropPatch3.CropsInPatch.Add(cropPatch.CropsInPatch[num5]);
					cropPatch.CropsInPatch.RemoveAt(num5);
				}
			}
		}
		else
		{
			int num6 = Math.Min((cropPatch.MinTile.y + cropPatch.MaxTile.y) / 2 + 1, cropPatch.GetMaxTileY());
			for (int num7 = cropPatch.Tiles.Count - 1; num7 >= 0; num7--)
			{
				if (cropPatch.Tiles[num7].y >= num6)
				{
					cropPatch3.Tiles.Add(cropPatch.Tiles[num7]);
					cropPatch.Tiles.RemoveAt(num7);
				}
			}
			for (int num8 = cropPatch.CropsInPatch.Count - 1; num8 >= 0; num8--)
			{
				if (cropPatch.CropsInPatch[num8].Tile.y >= num6)
				{
					cropPatch3.CropsInPatch.Add(cropPatch.CropsInPatch[num8]);
					cropPatch.CropsInPatch.RemoveAt(num8);
				}
			}
		}
		cropPatch.RecalcBounds();
		cropPatch3.RecalcBounds();
		Patches.Add(cropPatch3);
	}

	public void ClearAllCropPatchesFromRect(TerrainCoord min, TerrainCoord max)
	{
		foreach (CropPatch patch in Patches)
		{
			if (!TerrainCoord.Overlaps(min, max, patch.MinTile - new TerrainCoord(32, 32), patch.MaxTile + new TerrainCoord(32, 32)))
			{
				continue;
			}
			for (int i = min.x; i <= max.x; i++)
			{
				for (int j = min.y; j <= max.y; j++)
				{
					patch.Tiles.Remove(new TerrainCoord(i, j));
				}
			}
		}
	}

	public bool AreAnyCropPatchesInRect(TerrainCoord min, TerrainCoord max)
	{
		foreach (CropPatch patch in Patches)
		{
			if (!TerrainCoord.Overlaps(min, max, patch.MinTile - new TerrainCoord(32, 32), patch.MaxTile + new TerrainCoord(32, 32)))
			{
				continue;
			}
			for (int i = min.x; i <= max.x; i++)
			{
				for (int j = min.y; j <= max.y; j++)
				{
					if (patch.Tiles.Contains(new TerrainCoord(i, j)))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public float GetPlantedNutritionAmount(int communityId)
	{
		float num = 0f;
		foreach (CropPatch patch in Patches)
		{
			if (patch.CommunityId != communityId)
			{
				continue;
			}
			foreach (PlantableCrop item in patch.CropsInPatch)
			{
				float predictedYield = item.GetPredictedYield();
				EquipmentPrototype harvestPrototype = item.GetHarvestPrototype();
				if (harvestPrototype != null)
				{
					num += harvestPrototype.GetNutrition() * predictedYield;
				}
			}
		}
		return num;
	}

	public float GetCropPatchNutritionAmount(int communityId, float farmingSkill)
	{
		float num = 0f;
		foreach (CropPatch patch in Patches)
		{
			if (patch.CommunityId == communityId)
			{
				float num2 = PlantableCrop.GetPredictedYieldForCropType(patch.CropType, farmingSkill) * (float)patch.Tiles.Count;
				EquipmentPrototype harvestPrototype = patch.CropType.HarvestPrototype;
				if (harvestPrototype != null)
				{
					num += harvestPrototype.GetNutrition() * num2;
				}
			}
		}
		return num;
	}

	public bool DoesCommunityHaveAnyCrops(int communityId)
	{
		foreach (CropPatch patch in Patches)
		{
			if (patch.CommunityId == communityId)
			{
				return true;
			}
		}
		return false;
	}

	public int GetCommunityCropsCount(int communityId)
	{
		int num = 0;
		foreach (CropPatch patch in Patches)
		{
			if (patch.CommunityId == communityId)
			{
				num += patch.CropsInPatch.Count;
			}
		}
		return num;
	}

	public int GetCommunityTilesCount(int communityId)
	{
		int num = 0;
		foreach (CropPatch patch in Patches)
		{
			if (patch.CommunityId == communityId)
			{
				num += patch.Tiles.Count;
			}
		}
		return num;
	}

	public int GetCommunityTilesCountForCropType(int communityId, PropPrototype cropType)
	{
		int num = 0;
		foreach (CropPatch patch in Patches)
		{
			if (patch.CommunityId == communityId && patch.CropType == cropType)
			{
				num += patch.Tiles.Count;
			}
		}
		return num;
	}

	public CropPatch GetPatch(TerrainCoord tile, int communityId)
	{
		foreach (CropPatch patch in Patches)
		{
			if (patch.CommunityId == communityId && tile.IsWithinBounds(patch.MinTile, patch.MaxTile) && patch.Tiles.Contains(tile))
			{
				return patch;
			}
		}
		return null;
	}

	public CropPatch GetNearestPatch(TerrainCoord tile, int communityId)
	{
		float num = float.MaxValue;
		CropPatch result = null;
		foreach (CropPatch patch in Patches)
		{
			if (patch.CommunityId == communityId && patch.CropsInPatch.Count > 0)
			{
				float closestDistSqTo = patch.Rect.GetClosestDistSqTo(tile);
				if (closestDistSqTo < num)
				{
					result = patch;
					num = closestDistSqTo;
				}
			}
		}
		return result;
	}

	public bool IsInPatchOfType(TerrainCoord tile, int communityId, PropPrototype plantingType)
	{
		CropPatch patch = GetPatch(tile, communityId);
		if (patch != null && patch.CropType == plantingType)
		{
			return true;
		}
		return false;
	}

	public bool IsFarming(int communityId, EquipmentPrototype proto)
	{
		foreach (CropPatch patch in Patches)
		{
			if (patch.CommunityId == communityId)
			{
				return patch.CropType.HarvestPrototype == proto || patch.CropType.HarvestSeedsPrototype == proto;
			}
		}
		return false;
	}

	public void Update()
	{
		int num = Math.Min(10, AllCrops.Count);
		for (int i = 0; i < num; i++)
		{
			if (CurrentIndex >= AllCrops.Count)
			{
				CurrentIndex = 0;
			}
			PlantableCrop plantableCrop = AllCrops[CurrentIndex];
			plantableCrop.Update();
			if (plantableCrop.IsDecayed())
			{
				plantableCrop.Delete();
			}
			else
			{
				CurrentIndex++;
			}
		}
	}
}
