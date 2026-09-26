public struct ModifiedPatch : IReflectable
{
	public int ModifiedPatchId;

	public int BuildingId;

	public TerrainModificationType Type;

	public TerrainCoord MinTile;

	public TerrainCoord MaxTile;

	public float FlattenHeight;

	public float FlattenBorder;

	public float DitchHeight;

	public TerrainTex Tex;

	public GrassType GrassType;

	public void Reflect(Reflector reflector)
	{
		reflector.AddAfter(ref ModifiedPatchId, 425);
		reflector.AddAfter(ref BuildingId, 428);
		reflector.Add(ref Type);
		reflector.Add(ref MinTile);
		reflector.Add(ref MaxTile);
		switch (Type)
		{
		case TerrainModificationType.Flatten:
			reflector.Add(ref FlattenHeight);
			reflector.Add(ref FlattenBorder);
			break;
		case TerrainModificationType.PitTrap:
			reflector.Add(ref DitchHeight);
			break;
		case TerrainModificationType.ApplyTexture:
			reflector.Add(ref FlattenBorder);
			reflector.Add(ref Tex);
			break;
		case TerrainModificationType.ApplyGrass:
			reflector.Add(ref FlattenBorder);
			reflector.Add(ref GrassType);
			break;
		}
	}
}
