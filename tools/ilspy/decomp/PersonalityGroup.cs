using System.Collections.Generic;

public class PersonalityGroup
{
	public static string NormalFaction = "Normal";

	public static string LooterFaction = "Looter";

	public string Faction;

	public List<PersonalityProbability> Personalities = new List<PersonalityProbability>();

	public bool Contains(string personality)
	{
		for (int i = 0; i < Personalities.Count; i++)
		{
			if (Personalities[i].Personality == personality)
			{
				return true;
			}
		}
		return false;
	}

	public float GetProbabilitySum()
	{
		float num = 0f;
		for (int i = 0; i < Personalities.Count; i++)
		{
			num += Personalities[i].Probability;
		}
		return num;
	}

	public bool IsNormalFaction()
	{
		return Faction == NormalFaction;
	}

	public bool Matches(PersonalityGroup rhs)
	{
		return Personalities[0].Personality == rhs.Personalities[0].Personality;
	}
}
