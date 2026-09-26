public struct StoryId : IReflectable
{
	public string Folder;

	public ulong WorkshopId;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Folder);
		reflector.Add(ref WorkshopId);
	}

	public bool IsNull()
	{
		if (string.IsNullOrEmpty(Folder))
		{
			return WorkshopId == 0;
		}
		return false;
	}
}
