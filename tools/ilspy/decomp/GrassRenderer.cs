using UnityEngine;

public class GrassRenderer
{
	public const int NumDensities = 5;

	public const int PatchSize = 8;

	public static Resource<Material>[] GrassMaterial = new Resource<Material>[8];

	public FlatGrassPatch[,] FlatGrassPatches = new FlatGrassPatch[8, 5];

	public const int MaxDensityForAnyGrassType = 15;

	public const int MaxDensityFactor = 4;

	public const float MaxRange = 48f;

	public static void LoadContentStatic()
	{
		GrassMaterial[0] = new Resource<Material>("Materials/Grass/PatchGrass1");
		GrassMaterial[1] = new Resource<Material>("Materials/Grass/PatchGrass2");
		GrassMaterial[2] = new Resource<Material>("Materials/Grass/PatchGrass3");
		GrassMaterial[3] = new Resource<Material>("Materials/Grass/PatchFlowers");
		for (int i = 4; i <= 7; i++)
		{
			GrassMaterial[i] = GrassMaterial[3];
		}
	}

	public void LoadContent()
	{
		for (int i = 0; i < 8; i++)
		{
			GrassType grassType = (GrassType)i;
			for (int j = 0; j < 5; j++)
			{
				int densityForGrassType = GetDensityForGrassType(grassType, j);
				Resource<Mesh> mesh = new Resource<Mesh>("Models/Grass/Patch_" + grassType.ToString() + "_" + densityForGrassType);
				FlatGrassPatches[i, j] = new FlatGrassPatch(densityForGrassType, mesh, GrassMaterial[i]);
			}
		}
	}

	public static int GetMaxDensityForGrassType(GrassType grassType)
	{
		if ((int)grassType < 3)
		{
			return 15;
		}
		return 3;
	}

	public static int GetDensityForGrassType(GrassType grassType, int j)
	{
		if ((int)grassType >= 3)
		{
			return j switch
			{
				0 => 1, 
				1 => 2, 
				2 => 3, 
				3 => 6, 
				_ => 12, 
			};
		}
		return j switch
		{
			0 => 5, 
			1 => 10, 
			2 => 15, 
			3 => 30, 
			_ => 60, 
		};
	}

	public FlatGrassPatch GetFlatGrassPatchForDensity(GrassType grassType, int maxDensity)
	{
		for (int i = 0; i < 5; i++)
		{
			FlatGrassPatch flatGrassPatch = FlatGrassPatches[(uint)grassType, i];
			if (flatGrassPatch.Density >= maxDensity)
			{
				return flatGrassPatch;
			}
		}
		return FlatGrassPatches[(uint)grassType, 4];
	}
}
