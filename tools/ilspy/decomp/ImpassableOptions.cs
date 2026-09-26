public enum ImpassableOptions
{
	CheckStationaryCharacters = 1,
	CheckMovingCharacters = 2,
	CheckOpenGates = 4,
	IgnoreFlammableDefences = 16,
	IgnoreExplodableDefences = 32,
	CheckCrops = 64,
	CheckTraps = 128,
	IgnoreSlopes = 256,
	IgnoreWaistHighWalls = 512,
	IgnorePitTraps = 1024,
	IgnoreDeadOrUnconsciousCharacters = 2048,
	IgnorePropsThatCanBeClearedForBuilding = 4096,
	IgnoreMovingVehicles = 8192,
	IgnoreTrees = 16384,
	IgnoreCharacters = 0,
	CheckAllCharacters = 3,
	CheckAllConsciousCharacters = 2051,
	CheckStationaryConsciousCharacters = 2049,
	All = 199
}
