public struct SkillChange : IReflectable
{
	public SkillType Skill;

	public int OldLevel;

	public int NewLevel;

	public int OldCap;

	public int NewCap;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Skill);
		reflector.Add(ref OldLevel);
		reflector.Add(ref NewLevel);
		reflector.Add(ref OldCap);
		reflector.Add(ref NewCap);
	}
}
