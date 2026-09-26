public struct PendingTrade : IReflectable
{
	public Equipment Item;

	public int ItemId;

	public int Amount;

	public bool Selling;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref ItemId);
		reflector.Add(ref Amount);
		reflector.Add(ref Selling);
	}
}
