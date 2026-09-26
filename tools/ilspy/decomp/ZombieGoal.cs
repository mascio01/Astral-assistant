using UnityEngine;

public class ZombieGoal : PrioritiserGoal
{
	public override GoalType GetGoalType()
	{
		return GoalType.ZombieGoal;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		if (reflector.IsDeserialising && reflector.Version < 149)
		{
			AddSubGoal(new FeedOnCorpse());
		}
	}

	public override void OnActivate(Character character, Goal parent)
	{
		AddSubGoal(new Wander());
		AddSubGoal(new FeedOnCorpse());
		AddSubGoal(new ZombieAlertGoal());
		AddSubGoal(new FeedOnLiving());
		if (character.PlayDead)
		{
			AddSubGoal(new PlayDead());
		}
		if (character.Community != null && character.Community.CommunityType == CommunityType.HunterZombie)
		{
			AddSubGoal(new FollowGoal());
			AddSubGoal(new ObeyLeaderGoal());
		}
		AddSubGoal(new PanicGoal());
		AddSubGoal(new UnconsciousGoal());
		base.OnActivate(character, parent);
	}

	public override void SetLeaderCommand(Character character, Goal parent, Goal command, TileObject target, ObeyLeaderGoal.SourceType source, PlayerRecord playerRecord)
	{
		foreach (Goal subGoal in SubGoals)
		{
			if (subGoal is ObeyLeaderGoal obeyLeaderGoal)
			{
				obeyLeaderGoal.SetCommand(character, this, command, target, source);
				return;
			}
		}
		ObeyLeaderGoal obeyLeaderGoal2 = new ObeyLeaderGoal();
		AddSubGoal(obeyLeaderGoal2);
		obeyLeaderGoal2.SetCommand(character, this, command, target, source);
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		if (SubGoal == null)
		{
			return null;
		}
		return SubGoal.GetOverheadActionIcon(character);
	}
}
