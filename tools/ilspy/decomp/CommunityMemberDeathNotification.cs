public struct CommunityMemberDeathNotification : IReflectable
{
	public Character Victim;

	public Character Killer;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Victim);
		reflector.Add(ref Killer);
	}
}
