public enum TileFlags : byte
{
	TriFlipped = 1,
	Impassable = 2,
	BuiltOn = 4,
	Spawnable = 8,
	Enclosed = 0x10,
	River = 0x20,
	Slope = 0x40,
	Trap = 0x80
}
