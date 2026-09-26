using UnityEngine;

public class SurvivorGoal : PrioritiserGoal
{
	private static GameProfiler PreUpdateTimer = new GameProfiler("SurvivorPreUpdate");

	private static GameProfiler UpdateTimer = new GameProfiler("SurvivorUpdate");

	public ScriptedGoal ScriptedGoal;

	public GatherGoal GatherGoal;

	public CaptureGoal CaptureGoal;

	public RepairGoal RepairGoal;

	public CraftGoal CraftGoal;

	public BuildGoal BuildGoal;

	public WarmGoal WarmGoal;

	public FarmingGoal FarmingGoal;

	public GuardGoal GuardGoal;

	public LumberjackGoal LumberjackGoal;

	public MinerGoal MinerGoal;

	public OrganizerGoal OrganizerGoal;

	public TrapperGoal TrapperGoal;

	public AnimalFeederGoal AnimalFeederGoal;

	public MedicGoal MedicGoal;

	public ObeyLeaderGoal ObeyLeaderGoal;

	public override GoalType GetGoalType()
	{
		return GoalType.SurvivorGoal;
	}

	protected override bool FinishOnNullSubGoal()
	{
		return false;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		AddSubGoal(new BoredGoal());
		AddSubGoal(new FarmingGoal());
		AddSubGoal(new GuardGoal());
		AddSubGoal(new GatherGoal());
		AddSubGoal(new BuildGoal());
		AddSubGoal(new CaptureGoal());
		AddSubGoal(new RepairGoal());
		AddSubGoal(new CraftGoal());
		AddSubGoal(new LumberjackGoal());
		AddSubGoal(new MinerGoal());
		AddSubGoal(new TrapperGoal());
		AddSubGoal(new AnimalFeederGoal());
		AddSubGoal(new OrganizerGoal());
		AddSubGoal(new MedicGoal());
		AddSubGoal(new DepressedGoal());
		AddSubGoal(new SatisfyHunger());
		AddSubGoal(new SatisfyThirst());
		AddSubGoal(new SatisfyNeedForSleep());
		AddSubGoal(new ToiletGoal());
		AddSubGoal(new WarmGoal());
		AddSubGoal(new ClothingGoal());
		AddSubGoal(new SkinAnimalsGoal());
		AddSubGoal(new AutoCollectGoal());
		AddSubGoal(new AutoDepositGoal());
		AddSubGoal(new BandageSelfGoal());
		AddSubGoal(new MedicateSelfGoal());
		AddSubGoal(new RepairArmorGoal());
		AddSubGoal(new RestockAmmoGoal());
		AddSubGoal(new ConsumeGift());
		AddSubGoal(new WatchFisticuffs());
		AddSubGoal(new AftermathGoal());
		AddSubGoal(new FollowGoal());
		AddSubGoal(new ScriptedGoal());
		AddSubGoal(new Conversation());
		AddSubGoal(new SwapSuppliesGoal());
		AddSubGoal(new MakeSureWeHaveReloadedGoal());
		AddSubGoal(new InvestigatePropertyDamage());
		AddSubGoal(new InvestigateCorpse());
		AddSubGoal(new AlertGoal());
		AddSubGoal(new TargetedGoal());
		AddSubGoal(new Attack());
		AddSubGoal(new RescueGoal());
		AddSubGoal(new ObeyLeaderGoal());
		AddSubGoal(new GetBackInZoneGoal());
		AddSubGoal(new PanicGoal());
		AddSubGoal(new UnconsciousGoal());
		AddSubGoal(new InvisibleStrainSpreadGoal());
		if (character.Appearance.Gender == GenderType.Female)
		{
			AddSubGoal(new BirthGoal());
		}
		if (character.PlayDead)
		{
			AddSubGoal(new PlayDead());
		}
		CacheGoals();
		base.OnActivate(character, parent);
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		if (!reflector.IsDeserialising)
		{
			return;
		}
		if (reflector.Version < 36)
		{
			AddSubGoal(new WatchFisticuffs());
		}
		if (reflector.Version < 42)
		{
			AddSubGoal(new ClothingGoal());
		}
		if (reflector.Version < 67)
		{
			AddSubGoal(new MinerGoal());
		}
		if (reflector.Version < 76)
		{
			AddSubGoal(new ToiletGoal());
		}
		if (reflector.Version < 163)
		{
			AddSubGoal(new TrapperGoal());
		}
		if (reflector.Version < 192)
		{
			AddSubGoal(new InvisibleStrainSpreadGoal());
		}
		if (reflector.Version == 205)
		{
			foreach (Goal subGoal in SubGoals)
			{
				if (subGoal.GetGoalType() == GoalType.AnimationGoal && FindSubGoalByType(GoalType.SatisfyHunger) == null)
				{
					Debug.Log("Fixing up weird corrupt savegame bug: " + character.GetDisplayNameString());
					SubGoals.Remove(subGoal);
					SubGoals.Add(new SatisfyHunger());
					break;
				}
			}
		}
		if (reflector.Version < 208)
		{
			AddSubGoal(new GetBackInZoneGoal());
		}
		if (reflector.Version < 236)
		{
			AddSubGoal(new InvestigatePropertyDamage());
		}
		if (reflector.Version < 278)
		{
			AddSubGoal(new RepairArmorGoal());
		}
		if (reflector.Version < 300)
		{
			AddSubGoal(new AutoCollectGoal());
		}
		if (reflector.Version < 373)
		{
			AddSubGoal(new AutoDepositGoal());
		}
		if (reflector.Version < 320)
		{
			AddSubGoal(new AnimalFeederGoal());
		}
		if (reflector.Version < 324)
		{
			AddSubGoal(new SkinAnimalsGoal());
		}
		if (reflector.Version < 350)
		{
			AddSubGoal(new RescueGoal());
		}
		if (reflector.Version < 354)
		{
			AddSubGoal(new OrganizerGoal());
		}
		if (reflector.Version < 589)
		{
			AddSubGoal(new MedicGoal());
		}
		if (reflector.Version < 389)
		{
			AddSubGoal(new RepairGoal());
		}
		if (reflector.Version < 484)
		{
			AddSubGoal(new BirthGoal());
		}
		CacheGoals();
	}

	public override void PreUpdate(Character character, Goal parent)
	{
		using (new ProfileMarker(PreUpdateTimer))
		{
			base.PreUpdate(character, parent);
		}
	}

	public override void Update(Character character, Goal parent)
	{
		using (new ProfileMarker(UpdateTimer))
		{
			base.Update(character, parent);
		}
	}

	public override bool OnCraftingFinished(Character character, Goal parent, TileObject obj)
	{
		if (base.OnCraftingFinished(character, parent, obj))
		{
			return true;
		}
		if (CraftGoal == null)
		{
			return false;
		}
		return CraftGoal.OnCraftingFinished(character, this, obj);
	}

	public override void SetLeaderCommand(Character character, Goal parent, Goal command, TileObject target, ObeyLeaderGoal.SourceType source, PlayerRecord playerRecord)
	{
		if (source == ObeyLeaderGoal.SourceType.Player && command != null)
		{
			if (playerRecord != null && (character == playerRecord.PlayerCharacter || character.SquadLeader == null || !playerRecord.SelectedCharacters.Contains(character.SquadLeader)))
			{
				character.MakeMeGroupLeader();
			}
			character.CancelOneOffCrafting();
		}
		if (ObeyLeaderGoal != null)
		{
			ObeyLeaderGoal.SetCommand(character, this, command, target, source);
		}
	}

	public override Goal GetLeaderCommand()
	{
		if (SubGoal != ObeyLeaderGoal || ObeyLeaderGoal == null)
		{
			return null;
		}
		return ObeyLeaderGoal.GetCommand();
	}

	public override Goal GetLeaderCommandEvenIfItIsInactive()
	{
		if (ObeyLeaderGoal != null)
		{
			return ObeyLeaderGoal.GetCommand();
		}
		return null;
	}

	public override bool IsLeaderCommandFinished(Character character)
	{
		if (ObeyLeaderGoal != null)
		{
			if (ObeyLeaderGoal.GetCommand() != null)
			{
				return ObeyLeaderGoal.GetCommand().CalcPriority(character, ObeyLeaderGoal) == GoalPriority.Impossible;
			}
			return true;
		}
		return true;
	}

	public void SetScriptedGoalMarker(Character character, Goal parent, TileObject marker, MovementType movementType, bool disableWhenReachedMarker, ScriptedMoveImportance importance, bool teleportIfMoveFailed, bool sit, bool avoidHostileBases)
	{
		foreach (Goal subGoal in SubGoals)
		{
			if (subGoal is ScriptedGoal scriptedGoal)
			{
				scriptedGoal.SetMarker(character, parent, marker, movementType, disableWhenReachedMarker, importance, teleportIfMoveFailed, sit, avoidHostileBases);
				break;
			}
		}
	}

	public ScriptedGoal GetScriptedGoal()
	{
		foreach (Goal subGoal in SubGoals)
		{
			if (subGoal is ScriptedGoal result)
			{
				return result;
			}
		}
		return null;
	}

	private void CacheGoals()
	{
		ScriptedGoal = FindSubGoalByType(GoalType.ScriptedGoal) as ScriptedGoal;
		GatherGoal = FindSubGoalByType(GoalType.GatherGoal) as GatherGoal;
		CaptureGoal = FindSubGoalByType(GoalType.CaptureGoal) as CaptureGoal;
		RepairGoal = FindSubGoalByType(GoalType.RepairGoal) as RepairGoal;
		CraftGoal = FindSubGoalByType(GoalType.CraftGoal) as CraftGoal;
		BuildGoal = FindSubGoalByType(GoalType.BuildGoal) as BuildGoal;
		WarmGoal = FindSubGoalByType(GoalType.WarmGoal) as WarmGoal;
		FarmingGoal = FindSubGoalByType(GoalType.FarmingGoal) as FarmingGoal;
		GuardGoal = FindSubGoalByType(GoalType.GuardGoal) as GuardGoal;
		LumberjackGoal = FindSubGoalByType(GoalType.LumberjackGoal) as LumberjackGoal;
		MinerGoal = FindSubGoalByType(GoalType.MinerGoal) as MinerGoal;
		OrganizerGoal = FindSubGoalByType(GoalType.OrganizerGoal) as OrganizerGoal;
		TrapperGoal = FindSubGoalByType(GoalType.TrapperGoal) as TrapperGoal;
		AnimalFeederGoal = FindSubGoalByType(GoalType.AnimalFeederGoal) as AnimalFeederGoal;
		MedicGoal = FindSubGoalByType(GoalType.MedicGoal) as MedicGoal;
		ObeyLeaderGoal = FindSubGoalByType(GoalType.ObeyLeaderGoal) as ObeyLeaderGoal;
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
