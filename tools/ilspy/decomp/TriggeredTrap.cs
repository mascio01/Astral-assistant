public struct TriggeredTrap : IReflectable
{
	public Character Triggerer;

	public TileObject Trap;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Triggerer);
		reflector.Add(ref Trap);
	}
}
