public struct ConstructionRecord : IReflectable
{
	public PropPrototype Proto;

	public TerrainCoord Tile;

	public Prop.OrientationType Orientation;

	public void Reflect(Reflector reflector)
	{
		if (reflector.Version < 219)
		{
			BaseObjectType value = BaseObjectType.Invalid;
			reflector.Add(ref value);
			Proto = GameImpl.Instance.FindPropPrototypeByName(value.ToString());
		}
		else
		{
			reflector.Add(ref Proto);
		}
		reflector.Add(ref Tile);
		reflector.Add(ref Orientation);
	}

	public PropPrototype GetTypeToBuild()
	{
		PropPrototype proto = Proto;
		if (proto != null && proto.FallbackPropPrototype != null)
		{
			return proto.FallbackPropPrototype;
		}
		return proto;
	}

	public bool CanRebuildHere(Community community)
	{
		PropPrototype typeToBuild = GetTypeToBuild();
		Prop prop = ((typeToBuild != null) ? (typeToBuild.ProtoInstance as Prop) : null);
		if (prop != null)
		{
			prop.CalcMinMaxTile(Tile, Orientation, out var minTile, out var maxTile);
			if (GameCursor.CanBuildHere(null, prop, minTile, maxTile, checkOtherCharacters: false, checkCropPatches: false, community) != CursorActionDisabledReason.Enabled)
			{
				return false;
			}
		}
		else if (GameTerrain.Instance.GetFixedObjectOnTile(Tile.x, Tile.y) != null)
		{
			return false;
		}
		return true;
	}
}
