using System;

public struct FindAttempt : IReflectable
{
	public int SearchObjectId;

	public int FailCount;

	public TimeSpan SearchTime;

	public FindType FindType;

	public EquipmentPrototype ProtoToFind;

	public LiquidPrototype Liquid;

	public Recipe FollowingRecipe;

	public int TerrainPathIndex;

	public int EntranceIndex;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref SearchObjectId);
		reflector.Add(ref FailCount);
		reflector.Add(ref SearchTime);
		reflector.Add(ref FindType);
		reflector.Add(ref ProtoToFind);
		reflector.AddAfter(ref Liquid, 65);
		reflector.Add(ref FollowingRecipe);
		reflector.AddAfter(ref TerrainPathIndex, 211);
		reflector.AddAfter(ref EntranceIndex, 215);
	}

	public bool Matches(BaseObject searchObject, int entranceIndex, int terrainPathIndex, FindType findType, EquipmentPrototype proto, LiquidPrototype liquid, Recipe recipe)
	{
		if (SearchObjectId == searchObject.Id && FindType == findType && ProtoToFind == proto && Liquid == liquid && FollowingRecipe == recipe && EntranceIndex == entranceIndex)
		{
			return TerrainPathIndex == terrainPathIndex;
		}
		return false;
	}

	public bool Matches(int searchObjectId, int entranceIndex, int terrainPathIndex, FindType findType, EquipmentPrototype proto, LiquidPrototype liquid, Recipe recipe)
	{
		if (SearchObjectId == searchObjectId && FindType == findType && ProtoToFind == proto && Liquid == liquid && FollowingRecipe == recipe && EntranceIndex == entranceIndex)
		{
			return TerrainPathIndex == terrainPathIndex;
		}
		return false;
	}
}
