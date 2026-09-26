using System.Collections.Generic;

public class SkillsChangePopup : IReflectable
{
	public Character Character;

	public List<SkillChange> SkillChanges = new List<SkillChange>();

	public float Displayed;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Character);
		reflector.Add(ref SkillChanges);
		reflector.Add(ref Displayed);
	}

	public int FindSkillChange(SkillType skillType)
	{
		for (int i = 0; i < SkillChanges.Count; i++)
		{
			if (SkillChanges[i].Skill == skillType)
			{
				return i;
			}
		}
		return -1;
	}
}
