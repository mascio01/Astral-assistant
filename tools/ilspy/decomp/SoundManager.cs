using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
	public static float AudioRolloffMinDist = 1f;

	public static float AudioRolloffMaxDist = 64f;

	public static SoundManager Instance;

	public static Resource<AudioClip> DemolitionSound;

	public static Resource<AudioClip> EnterBuildingSound;

	public static Resource<AudioClip> ExitBuildingSound;

	public static Resource<AudioClip> EnterVehicleSound;

	public static Resource<AudioClip> ExitVehicleSound;

	public static Resource<AudioClip> OpenGateSound;

	public static Resource<AudioClip> CloseGateSound;

	public static Resource<AudioClip> MaleCatchingBreathSound;

	public static Resource<AudioClip> FemaleCatchingBreathSound;

	public static Resource<AudioClip> SlidingSound;

	public static Resource<AudioClip> TreeFallSound;

	public static Resource<AudioClip> StumpFallSound;

	public static Resource<AudioClip> BushFallSound;

	public static Resource<AudioClip> BonfireSound;

	public static Resource<AudioClip> KnockSound;

	public static Resource<AudioClip> YawnMaleSound;

	public static Resource<AudioClip> YawnFemaleSound;

	public static Resource<AudioClip> HelicopterSound;

	public static Resource<AudioClip> EngineStartSound;

	public static Resource<AudioClip> EngineStopSound;

	public static Resource<AudioClip> EngineIdleSound;

	public static Resource<AudioClip> EngineRunningSound;

	public static Resource<AudioClip> EngineReverseSound;

	public static Resource<AudioClip> BrakingSound;

	public static Resource<AudioClip> StartFailSound;

	public static List<Resource<AudioClip>> GearShiftUpSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> GearShiftDownSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> CarCrashSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> CarHitBodySounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> HornSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>>[] ZombieIdleSounds = new List<Resource<AudioClip>>[2];

	public static List<Resource<AudioClip>> ZombieAlertSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>>[] ZombieAttackSounds = new List<Resource<AudioClip>>[2];

	public static List<Resource<AudioClip>> ZombieBiteSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> ZombiePainSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> ZombieDieSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> ZombieEatSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> GoreSplatSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> BladeHitSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> ChokeSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> RabbitDeathSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> DeerDoeDeathSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> DeerStagDeathSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> DeerDoeAlertSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> DeerStagAlertSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> DeerDoeFleeSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> DeerStagFleeSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> ChickPeepSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> ChickenIdleSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> ChickenDeathSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> ChickenAlertSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> ChickenFleeSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> ChickenMatingSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> RoosterCrowSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> PunchSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> PunchMissSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> PunchBlockedSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> BulletHitSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> RicochetSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> ParrySounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> HitArmorSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> FootstepSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> ForgeSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> ThrowSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> GlassHitSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> SplatSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> ImpactSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> LightImpactSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> DrinkSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> EatSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> PeeingSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> PouringSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> PlantingSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> HarvestingSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> BandageSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> BurningSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> DemolishSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> HammerSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> DigSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> DigThrowSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> ChopWoodSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> BreakRockSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> FlintSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> MatchSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> SkinningSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> ArrowHitSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> ArrowHitRockSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> RadioMusic = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> PitTrapTriggerSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> SnowballHitSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>>[] WalkingFootstepSounds = new List<Resource<AudioClip>>[11];

	public static List<Resource<AudioClip>>[] RunningFootstepSounds = new List<Resource<AudioClip>>[11];

	public static float[] WalkingFootstepVolume = new float[11];

	public static float[] RunningFootstepVolume = new float[11];

	public static List<Resource<AudioClip>> BushRustleSounds = new List<Resource<AudioClip>>();

	public static Resource<AudioClip>[] PistolFireSound = new Resource<AudioClip>[2];

	public static Resource<AudioClip>[] ShotgunFireSound = new Resource<AudioClip>[1];

	public static Resource<AudioClip>[] AssaultRifleFireSound = new Resource<AudioClip>[3];

	public static Resource<AudioClip>[] SniperRifleFireSound = new Resource<AudioClip>[2];

	public static Resource<AudioClip>[] BowReleaseSound = new Resource<AudioClip>[1];

	public static List<Resource<AudioClip>> ExplosionSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> SilencedPistolFireSound = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> SilencedSniperFireSound = new List<Resource<AudioClip>>();

	public static Resource<AudioClip> RPGRocketSound;

	public static Resource<AudioClip> PistolReleaseSound;

	public static Resource<AudioClip> PistolInsertSound;

	public static Resource<AudioClip> PistolSlideSound;

	public static Resource<AudioClip> ShotgunPumpSound;

	public static Resource<AudioClip> AssaultRifleReleaseSound;

	public static Resource<AudioClip> AssaultRifleInsertSound;

	public static Resource<AudioClip> AssaultRifleSlideSound;

	public static Resource<AudioClip> SniperRifleReleaseSound;

	public static Resource<AudioClip> SniperRifleInsertSound;

	public static Resource<AudioClip> GunCockSound;

	public static Resource<AudioClip> GunUnequipSound;

	public static Resource<AudioClip> GunDryFireSound;

	public static Resource<AudioClip> BowPullSound;

	public static Resource<AudioClip> InGameClickSound;

	public static Resource<AudioClip> InGameDoubleClickSound;

	public static Resource<AudioClip> SelectSound;

	public static Resource<AudioClip> DenySelectSound;

	public static Resource<AudioClip> HudOnSound;

	public static Resource<AudioClip> HudOffSound;

	public static Resource<AudioClip> MoveSelectSound;

	public static Resource<AudioClip> HoverSound;

	public static Resource<AudioClip> CancelSound;

	public static Resource<AudioClip> DeleteSound;

	public static Resource<AudioClip> MakeMolotovSound;

	public static Resource<AudioClip> TabSound;

	public static Resource<AudioClip> TargetSound;

	public static Resource<AudioClip> HintSound;

	public static Resource<AudioClip> ForwardPageSound;

	public static Resource<AudioClip> BackwardPageSound;

	public static Resource<AudioClip> StartSaveGameSound;

	public static Resource<AudioClip> DialogOpenSound;

	public static Resource<AudioClip> HallelujahSound;

	public static Resource<AudioClip> SuccessSound;

	public static Resource<AudioClip> StatusNotificationSound;

	public static List<Resource<AudioClip>> EquipmentNotificationSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> NotificationSounds = new List<Resource<AudioClip>>();

	public static List<Resource<AudioClip>> CoinSounds = new List<Resource<AudioClip>>();

	public static float HallelujahVolume = 0.5f;

	public AudioSource UnityAudioSource;

	public AudioSource UnityHeartBeatAudioSource;

	public AudioSource UnityMalePantingAudioSource;

	public AudioSource UnityFemalePantingAudioSource;

	private static List<KeyValuePair<AudioClip, float>> QueuedSounds = new List<KeyValuePair<AudioClip, float>>();

	public static float MainMaxSoundDist = 40f;

	public static float WorldSoundVolume => GameImpl.Instance.Settings.SoundFXVolume;

	public static float BackgroundSoundVolume => GameImpl.Instance.Settings.BackgroundSoundVolume;

	public static float MusicSoundVolume => GameImpl.Instance.Settings.MusicSoundVolume;

	public static float MenuSoundVolume => GameImpl.Instance.Settings.MenuSoundVolume;

	public static void LoadContent()
	{
		DemolitionSound = new Resource<AudioClip>("Sounds\\Building\\timerexpl");
		EnterBuildingSound = new Resource<AudioClip>("Sounds\\Building\\wooden_door_internal_open_001");
		ExitBuildingSound = new Resource<AudioClip>("Sounds\\Building\\wooden_door_internal_shut_001");
		EnterVehicleSound = new Resource<AudioClip>("Sounds\\Vehicles\\car_door_open");
		ExitVehicleSound = new Resource<AudioClip>("Sounds\\Vehicles\\car_door_close");
		OpenGateSound = new Resource<AudioClip>("Sounds\\Building\\old_horror_gate_open");
		CloseGateSound = new Resource<AudioClip>("Sounds\\Building\\old_horror_gate_close");
		TreeFallSound = new Resource<AudioClip>("Sounds\\Prop\\treefall");
		StumpFallSound = new Resource<AudioClip>("Sounds\\Prop\\stumpfall");
		BushFallSound = new Resource<AudioClip>("Sounds\\Prop\\bushfall");
		BonfireSound = new Resource<AudioClip>("Sounds\\Prop\\bonfire");
		KnockSound = new Resource<AudioClip>("Sounds\\Prop\\knock");
		YawnMaleSound = new Resource<AudioClip>("Sounds\\Character\\yawn_male");
		YawnFemaleSound = new Resource<AudioClip>("Sounds\\Character\\yawn_female");
		HelicopterSound = new Resource<AudioClip>("Sounds\\Vehicles\\Helicopter");
		EngineStartSound = new Resource<AudioClip>("Sounds\\Vehicles\\Vehicle_Car_Start_Engine_Exterior");
		EngineStopSound = new Resource<AudioClip>("Sounds\\Vehicles\\Vehicle_Car_Stop_Engine_Exterior");
		EngineIdleSound = new Resource<AudioClip>("Sounds\\Vehicles\\Vehicle_Car_Engine_Idle_Exterior_Loop_01");
		EngineRunningSound = new Resource<AudioClip>("Sounds\\Vehicles\\Vehicle_Car_Engine_2000_RPM_Rear_Exterior_Loop");
		EngineReverseSound = new Resource<AudioClip>("Sounds\\Vehicles\\Vehicle_Car_Engine_1000_RPM_Rear_Exterior_Loop");
		BrakingSound = new Resource<AudioClip>("Sounds\\Vehicles\\Brakes");
		StartFailSound = new Resource<AudioClip>("Sounds\\Vehicles\\StartFail");
		GearShiftDownSounds.Add(new Resource<AudioClip>("Sounds\\Vehicles\\Car Gear Shift _6"));
		GearShiftDownSounds.Add(new Resource<AudioClip>("Sounds\\Vehicles\\Car Gear Shift _8"));
		GearShiftDownSounds.Add(new Resource<AudioClip>("Sounds\\Vehicles\\Car Gear Shift _9"));
		GearShiftUpSounds.Add(new Resource<AudioClip>("Sounds\\Vehicles\\Car Gear Shift "));
		GearShiftUpSounds.Add(new Resource<AudioClip>("Sounds\\Vehicles\\Car Gear Shift _1"));
		GearShiftUpSounds.Add(new Resource<AudioClip>("Sounds\\Vehicles\\Car Gear Shift _2"));
		GearShiftUpSounds.Add(new Resource<AudioClip>("Sounds\\Vehicles\\Car Gear Shift _3"));
		GearShiftUpSounds.Add(new Resource<AudioClip>("Sounds\\Vehicles\\Car Gear Shift _7"));
		for (int i = 1; i <= 5; i++)
		{
			CarCrashSounds.Add(new Resource<AudioClip>("Sounds\\Vehicles\\Crash" + i));
		}
		for (int j = 1; j <= 10; j++)
		{
			CarHitBodySounds.Add(new Resource<AudioClip>("Sounds\\Vehicles\\Punch Impact (Flesh) " + j));
		}
		HornSounds.Add(new Resource<AudioClip>("Sounds\\Vehicles\\Car Horn-Honk"));
		HornSounds.Add(new Resource<AudioClip>("Sounds\\Vehicles\\Car Horn-Honk_1"));
		HornSounds.Add(new Resource<AudioClip>("Sounds\\Vehicles\\Car Horn-Honk_2"));
		for (int k = 1; k <= 3; k++)
		{
			DemolishSounds.Add(new Resource<AudioClip>("Sounds\\Building\\bustcrate" + k));
		}
		for (int l = 1; l <= 4; l++)
		{
			HammerSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\Hammer\\hammer" + l));
		}
		for (int m = 1; m <= 3; m++)
		{
			DigSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\Shovel\\dig" + m));
		}
		for (int n = 1; n <= 3; n++)
		{
			DigThrowSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\Shovel\\digthrow" + n));
		}
		for (int num = 1; num <= 6; num++)
		{
			ChopWoodSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\Axe\\chopwood" + num));
		}
		for (int num2 = 1; num2 <= 6; num2++)
		{
			BreakRockSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\Pickaxe\\pickaxe" + num2));
		}
		for (int num3 = 1; num3 <= 4; num3++)
		{
			FlintSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\Flint\\flint" + num3));
		}
		MatchSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\Match\\match"));
		for (int num4 = 1; num4 <= 4; num4++)
		{
			SkinningSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\HuntingKnife\\skinning" + num4));
		}
		for (int num5 = 1; num5 <= 2; num5++)
		{
			PitTrapTriggerSounds.Add(new Resource<AudioClip>("Sounds\\Prop\\pit_trap_trigger" + num5));
		}
		for (int num6 = 1; num6 <= 3; num6++)
		{
			SnowballHitSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\Snowball\\SnowballHit" + num6, includeInList: true));
		}
		for (int num7 = 0; num7 < 11; num7++)
		{
			WalkingFootstepSounds[num7] = new List<Resource<AudioClip>>();
			RunningFootstepSounds[num7] = new List<Resource<AudioClip>>();
			WalkingFootstepVolume[num7] = 1f;
			RunningFootstepVolume[num7] = 1f;
		}
		for (int num8 = 1; num8 <= 15; num8++)
		{
			WalkingFootstepSounds[0].Add(new Resource<AudioClip>("Sounds\\Character\\Footsteps\\Footsteps Grass (Walking) " + num8));
		}
		for (int num9 = 1; num9 <= 15; num9++)
		{
			RunningFootstepSounds[0].Add(new Resource<AudioClip>("Sounds\\Character\\Footsteps\\Footsteps Grass (Running) " + num9));
		}
		for (int num10 = 1; num10 <= 15; num10++)
		{
			WalkingFootstepSounds[1].Add(new Resource<AudioClip>("Sounds\\Character\\Footsteps\\Footsteps Mud (Walking) " + num10));
		}
		for (int num11 = 1; num11 <= 15; num11++)
		{
			RunningFootstepSounds[1].Add(new Resource<AudioClip>("Sounds\\Character\\Footsteps\\Footsteps Mud (Running) " + num11));
		}
		for (int num12 = 1; num12 <= 15; num12++)
		{
			WalkingFootstepSounds[2].Add(new Resource<AudioClip>("Sounds\\Character\\Footsteps\\Footsteps Concrete (Walking) " + num12));
		}
		for (int num13 = 1; num13 <= 15; num13++)
		{
			RunningFootstepSounds[2].Add(new Resource<AudioClip>("Sounds\\Character\\Footsteps\\Footsteps Concrete Running " + num13));
		}
		for (int num14 = 1; num14 <= 15; num14++)
		{
			WalkingFootstepSounds[3].Add(new Resource<AudioClip>("Sounds\\Character\\Footsteps\\Footsteps Dirt (Walking) " + num14));
		}
		for (int num15 = 1; num15 <= 14; num15++)
		{
			RunningFootstepSounds[3].Add(new Resource<AudioClip>("Sounds\\Character\\Footsteps\\Footsteps Dirt (Running) " + num15));
		}
		for (int num16 = 1; num16 <= 15; num16++)
		{
			WalkingFootstepSounds[4].Add(new Resource<AudioClip>("Sounds\\Character\\Footsteps\\Footsteps Bush (Walking) " + num16));
		}
		for (int num17 = 1; num17 <= 15; num17++)
		{
			RunningFootstepSounds[4].Add(new Resource<AudioClip>("Sounds\\Character\\Footsteps\\Footsteps Bush (Running) " + num17));
		}
		for (int num18 = 1; num18 <= 5; num18++)
		{
			WalkingFootstepSounds[5].Add(new Resource<AudioClip>("Sounds\\Character\\Footsteps\\Light  Glass footsteps " + num18));
		}
		for (int num19 = 1; num19 <= 5; num19++)
		{
			RunningFootstepSounds[5].Add(new Resource<AudioClip>("Sounds\\Character\\Footsteps\\Heavy Running  Glass footsteps " + num19));
		}
		for (int num20 = 1; num20 <= 8; num20++)
		{
			WalkingFootstepSounds[8].Add(new Resource<AudioClip>("Sounds\\Character\\Footsteps\\Snow footstep " + num20));
		}
		for (int num21 = 1; num21 <= 8; num21++)
		{
			RunningFootstepSounds[8].Add(new Resource<AudioClip>("Sounds\\Character\\Footsteps\\Snow footstep running " + num21));
		}
		for (int num22 = 1; num22 <= 5; num22++)
		{
			WalkingFootstepSounds[9].Add(new Resource<AudioClip>("Sounds\\Character\\Footsteps\\Light  Ice Footsteps " + num22));
		}
		for (int num23 = 1; num23 <= 5; num23++)
		{
			RunningFootstepSounds[9].Add(new Resource<AudioClip>("Sounds\\Character\\Footsteps\\Heavy Running  Ice Footsteps " + num23));
		}
		for (int num24 = 0; num24 < WalkingFootstepSounds[9].Count; num24++)
		{
			WalkingFootstepSounds[10].Add(WalkingFootstepSounds[9][num24]);
		}
		for (int num25 = 0; num25 < RunningFootstepSounds[9].Count; num25++)
		{
			RunningFootstepSounds[10].Add(RunningFootstepSounds[9][num25]);
		}
		for (int num26 = 1; num26 <= 10; num26++)
		{
			WalkingFootstepSounds[7].Add(new Resource<AudioClip>("Sounds\\Character\\Footsteps\\Water footsteps " + num26));
		}
		for (int num27 = 1; num27 <= 10; num27++)
		{
			RunningFootstepSounds[7].Add(WalkingFootstepSounds[7][num27 - 1]);
		}
		for (int num28 = 1; num28 <= 5; num28++)
		{
			WalkingFootstepSounds[6].Add(new Resource<AudioClip>("Sounds\\Character\\Footsteps\\Light  Water footsteps " + num28));
		}
		for (int num29 = 1; num29 <= 5; num29++)
		{
			RunningFootstepSounds[6].Add(new Resource<AudioClip>("Sounds\\Character\\Footsteps\\Heavy Running  Water footsteps " + num29));
		}
		WalkingFootstepVolume[0] = 2f;
		RunningFootstepVolume[0] = 1.5f;
		WalkingFootstepVolume[1] = 1.25f;
		RunningFootstepVolume[1] = 1.25f;
		WalkingFootstepVolume[2] = 2f;
		RunningFootstepVolume[2] = 2f;
		WalkingFootstepVolume[3] = 1.25f;
		RunningFootstepVolume[3] = 1.25f;
		WalkingFootstepVolume[4] = 1.75f;
		RunningFootstepVolume[4] = 1.75f;
		WalkingFootstepVolume[5] = 0.25f;
		RunningFootstepVolume[5] = 0.25f;
		WalkingFootstepVolume[6] = 0.5f;
		RunningFootstepVolume[6] = 0.5f;
		WalkingFootstepVolume[7] = 0.5f;
		RunningFootstepVolume[7] = 0.5f;
		WalkingFootstepVolume[8] = 0.25f;
		RunningFootstepVolume[8] = 0.5f;
		WalkingFootstepVolume[9] = 0.35f;
		RunningFootstepVolume[9] = 0.35f;
		WalkingFootstepVolume[10] = 0.2f;
		RunningFootstepVolume[10] = 0.2f;
		for (int num30 = 1; num30 <= 7; num30++)
		{
			BushRustleSounds.Add(new Resource<AudioClip>("Sounds\\Prop\\Bush\\rustle" + num30));
		}
		for (int num31 = 0; num31 < ZombieIdleSounds.Length; num31++)
		{
			ZombieIdleSounds[num31] = new List<Resource<AudioClip>>();
		}
		for (int num32 = 0; num32 < ZombieAttackSounds.Length; num32++)
		{
			ZombieAttackSounds[num32] = new List<Resource<AudioClip>>();
		}
		for (int num33 = 1; num33 <= 4; num33++)
		{
			ZombieIdleSounds[0].Add(new Resource<AudioClip>("Sounds\\Zombie2\\IdleMale" + num33));
		}
		for (int num34 = 1; num34 <= 12; num34++)
		{
			ZombieIdleSounds[1].Add(new Resource<AudioClip>("Sounds\\Zombie2\\IdleFemale" + num34));
		}
		for (int num35 = 1; num35 <= 4; num35++)
		{
			ZombieAlertSounds.Add(new Resource<AudioClip>("Sounds\\Zombie2\\AlertMale" + num35));
		}
		for (int num36 = 1; num36 <= 8; num36++)
		{
			ZombieAttackSounds[0].Add(new Resource<AudioClip>("Sounds\\Zombie2\\AttackMale" + num36));
		}
		for (int num37 = 1; num37 <= 12; num37++)
		{
			ZombieAttackSounds[1].Add(new Resource<AudioClip>("Sounds\\Zombie2\\AttackFemale" + num37));
		}
		for (int num38 = 1; num38 <= 6; num38++)
		{
			ZombiePainSounds.Add(new Resource<AudioClip>("Sounds\\Zombie2\\PainMale" + num38));
		}
		for (int num39 = 1; num39 <= 5; num39++)
		{
			ZombieDieSounds.Add(new Resource<AudioClip>("Sounds\\Zombie2\\DeathMale" + num39));
		}
		for (int num40 = 1; num40 <= 8; num40++)
		{
			ZombieEatSounds.Add(new Resource<AudioClip>("Sounds\\Zombie2\\EatMale" + num40));
		}
		for (int num41 = 1; num41 <= 3; num41++)
		{
			ZombieBiteSounds.Add(new Resource<AudioClip>("Sounds\\Zombie2\\BiteMale" + num41));
		}
		for (int num42 = 1; num42 <= 2; num42++)
		{
			GoreSplatSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\Melee\\goresplat" + num42));
		}
		for (int num43 = 1; num43 <= 3; num43++)
		{
			BladeHitSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\Melee\\machete_hitbod" + num43));
		}
		for (int num44 = 1; num44 <= 5; num44++)
		{
			ChokeSounds.Add(new Resource<AudioClip>("Sounds\\Character\\choke" + num44));
		}
		RabbitDeathSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\rabbit_scream"));
		DeerDoeDeathSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Deer & Stag\\deer_die_1"));
		DeerStagDeathSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Deer & Stag\\stag_die"));
		DeerStagDeathSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Deer & Stag\\stag_die2"));
		DeerDoeAlertSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Deer & Stag\\deer_spot"));
		DeerStagAlertSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Deer & Stag\\stag_spot"));
		for (int num45 = 1; num45 <= 3; num45++)
		{
			DeerDoeFleeSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Deer & Stag\\deer_attack" + num45));
		}
		for (int num46 = 1; num46 <= 3; num46++)
		{
			DeerStagFleeSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Deer & Stag\\stag_attack" + num46));
		}
		ChickenDeathSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Chicken\\Chicken 01"));
		ChickenIdleSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Chicken\\Chicken 02"));
		ChickenIdleSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Chicken\\Chicken 03"));
		ChickenAlertSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Chicken\\Chicken 04"));
		ChickenAlertSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Chicken\\Chicken 05"));
		ChickenIdleSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Chicken\\Chicken 06"));
		ChickenIdleSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Chicken\\Chicken 07"));
		ChickenAlertSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Chicken\\Chicken 08"));
		ChickenIdleSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Chicken\\Chicken 09"));
		ChickenAlertSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Chicken\\Chicken 10"));
		ChickenDeathSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Chicken\\Chicken 11"));
		ChickenIdleSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Chicken\\Chicken 12"));
		for (int num47 = 1; num47 <= 5; num47++)
		{
			ChickenIdleSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Chicken\\Chicken Cluck 0" + num47));
		}
		ChickenFleeSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Chicken\\Chicken Wings 01"));
		ChickenFleeSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Chicken\\Chicken Wings 02"));
		ChickenFleeSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Chicken\\Chicken Wings 05"));
		ChickenFleeSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Chicken\\Chicken Wings 06"));
		ChickenMatingSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Chicken\\Chicken Wings 03"));
		ChickenMatingSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Chicken\\Chicken Wings 04"));
		ChickenMatingSounds.AddRange(ChickenFleeSounds);
		for (int num48 = 1; num48 <= 3; num48++)
		{
			RoosterCrowSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Chicken\\Cockerel 0" + num48));
		}
		for (int num49 = 1; num49 <= 8; num49++)
		{
			ChickPeepSounds.Add(new Resource<AudioClip>("Sounds\\Animal\\Chicken\\Chick Peep 0" + num49));
		}
		for (int num50 = 1; num50 <= 2; num50++)
		{
			PunchSounds.Add(new Resource<AudioClip>("Sounds\\Zombie\\gorilla\\punch" + num50));
		}
		for (int num51 = 1; num51 <= 2; num51++)
		{
			PunchMissSounds.Add(new Resource<AudioClip>("Sounds\\Zombie\\zombie\\claw_miss" + num51));
		}
		PunchBlockedSounds.Add(new Resource<AudioClip>("Sounds\\Character\\blocked_punch"));
		for (int num52 = 1; num52 <= 2; num52++)
		{
			DrinkSounds.Add(new Resource<AudioClip>("Sounds\\Character\\glug" + num52));
		}
		for (int num53 = 1; num53 <= 2; num53++)
		{
			PeeingSounds.Add(new Resource<AudioClip>("Sounds\\Character\\peeing" + num53));
		}
		EatSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\funny_bite"));
		BandageSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\smallmedkit1"));
		PouringSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\Pouring Liquid"));
		PlantingSounds.Add(new Resource<AudioClip>("Sounds\\Prop\\PlantableCrop\\Digging 14"));
		PlantingSounds.Add(new Resource<AudioClip>("Sounds\\Prop\\PlantableCrop\\Digging 18"));
		PlantingSounds.Add(new Resource<AudioClip>("Sounds\\Prop\\PlantableCrop\\Digging 23"));
		for (int num54 = 1; num54 <= 4; num54++)
		{
			HarvestingSounds.Add(new Resource<AudioClip>("Sounds\\Prop\\PlantableCrop\\Gathering Plants " + num54));
		}
		MaleCatchingBreathSound = new Resource<AudioClip>("Sounds\\Character\\Male Sprint Out of Breath - catching");
		FemaleCatchingBreathSound = new Resource<AudioClip>("Sounds\\Character\\Female Sprint Out of Breath - catching");
		SlidingSound = new Resource<AudioClip>("Sounds\\Character\\sliding");
		PistolFireSound[0] = new Resource<AudioClip>("Sounds\\Equipment\\Pistol\\colt_fire1", includeInList: true);
		PistolFireSound[1] = new Resource<AudioClip>("Sounds\\Equipment\\Pistol\\colt_fire2", includeInList: true);
		for (int num55 = 1; num55 <= 6; num55++)
		{
			SilencedPistolFireSound.Add(new Resource<AudioClip>("Sounds\\Equipment\\Pistol\\Silenced Pistol " + num55, includeInList: true));
		}
		for (int num56 = 1; num56 <= 3; num56++)
		{
			SilencedSniperFireSound.Add(new Resource<AudioClip>("Sounds\\Equipment\\SniperRifle\\Silenced Sniper Rifle " + num56, includeInList: true));
		}
		ShotgunFireSound[0] = new Resource<AudioClip>("Sounds\\Equipment\\Shotgun\\870_buckshot", includeInList: true);
		AssaultRifleFireSound[0] = new Resource<AudioClip>("Sounds\\Equipment\\AssaultRifle\\m16_fire1", includeInList: true);
		AssaultRifleFireSound[1] = new Resource<AudioClip>("Sounds\\Equipment\\AssaultRifle\\m16_fire2", includeInList: true);
		AssaultRifleFireSound[2] = new Resource<AudioClip>("Sounds\\Equipment\\AssaultRifle\\m16_fire3", includeInList: true);
		SniperRifleFireSound[0] = new Resource<AudioClip>("Sounds\\Equipment\\SniperRifle\\m21_shot1", includeInList: true);
		SniperRifleFireSound[1] = new Resource<AudioClip>("Sounds\\Equipment\\SniperRifle\\m21_shot2", includeInList: true);
		BowReleaseSound[0] = new Resource<AudioClip>("Sounds\\Equipment\\Bow\\release1", includeInList: true);
		ArrowHitSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\Bow\\hit1"));
		for (int num57 = 1; num57 <= 3; num57++)
		{
			ArrowHitRockSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\Bow\\hit_rock" + num57));
		}
		ExplosionSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\RPG\\explode3"));
		ExplosionSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\RPG\\explode4"));
		ExplosionSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\RPG\\explode5"));
		RPGRocketSound = new Resource<AudioClip>("Sounds\\Equipment\\RPG\\rocket1");
		PistolReleaseSound = new Resource<AudioClip>("Sounds\\Equipment\\Pistol\\colt_release", includeInList: true);
		PistolInsertSound = new Resource<AudioClip>("Sounds\\Equipment\\Pistol\\colt_insert", includeInList: true);
		PistolSlideSound = new Resource<AudioClip>("Sounds\\Equipment\\Pistol\\colt_slide", includeInList: true);
		ShotgunPumpSound = new Resource<AudioClip>("Sounds\\Equipment\\Shotgun\\870_pump", includeInList: true);
		AssaultRifleReleaseSound = new Resource<AudioClip>("Sounds\\Equipment\\AssaultRifle\\m16_cliprelease1", includeInList: true);
		AssaultRifleInsertSound = new Resource<AudioClip>("Sounds\\Equipment\\AssaultRifle\\m16_clipinsert1", includeInList: true);
		AssaultRifleSlideSound = new Resource<AudioClip>("Sounds\\Equipment\\AssaultRifle\\m16_slide1", includeInList: true);
		SniperRifleReleaseSound = new Resource<AudioClip>("Sounds\\Equipment\\SniperRifle\\m21_clipout", includeInList: true);
		SniperRifleInsertSound = new Resource<AudioClip>("Sounds\\Equipment\\SniperRifle\\m21_clipin", includeInList: true);
		GunCockSound = new Resource<AudioClip>("Sounds\\Equipment\\gun_cock");
		GunUnequipSound = new Resource<AudioClip>("Sounds\\Equipment\\gun_unequip");
		GunDryFireSound = new Resource<AudioClip>("Sounds/Equipment/gun_dry_fire");
		BowPullSound = new Resource<AudioClip>("Sounds\\Equipment\\Bow\\pull1", includeInList: true);
		for (int num58 = 1; num58 <= 2; num58++)
		{
			BulletHitSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\bullet_hit" + num58));
		}
		for (int num59 = 1; num59 <= 5; num59++)
		{
			RicochetSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\ric" + num59));
		}
		for (int num60 = 1; num60 <= 3; num60++)
		{
			ForgeSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\Melee\\Cling" + num60));
		}
		for (int num61 = 1; num61 <= 3; num61++)
		{
			ParrySounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\Melee\\Cling" + num61));
		}
		for (int num62 = 1; num62 <= 3; num62++)
		{
			ParrySounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\Melee\\Shield" + num62));
		}
		for (int num63 = 4; num63 <= 6; num63++)
		{
			HitArmorSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\Melee\\Shield" + num63));
		}
		for (int num64 = 1; num64 <= 4; num64++)
		{
			GlassHitSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\MolotovCocktail\\glass" + num64));
		}
		ImpactSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\impact"));
		LightImpactSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\light_impact"));
		SplatSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\splat"));
		for (int num65 = 1; num65 <= 2; num65++)
		{
			ThrowSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\MolotovCocktail\\colt_swing" + num65));
		}
		for (int num66 = 1; num66 <= 5; num66++)
		{
			BurningSounds.Add(new Resource<AudioClip>("Sounds\\Equipment\\MolotovCocktail\\burning_section" + num66));
		}
		InGameClickSound = new Resource<AudioClip>("Sounds\\Widget\\342200__christopherderp__videogame-menu-button-click");
		InGameDoubleClickSound = new Resource<AudioClip>("Sounds\\Widget\\333042__christopherderp__videogame-menu-button-clicking-sound-17");
		SelectSound = new Resource<AudioClip>("Sounds\\Widget\\Menu_Select");
		DenySelectSound = new Resource<AudioClip>("Sounds\\Widget\\menu_wrong_1");
		HudOnSound = new Resource<AudioClip>("Sounds\\Widget\\multimedia_maximise_window");
		HudOffSound = new Resource<AudioClip>("Sounds\\Widget\\multimedia_minimise_window");
		MoveSelectSound = new Resource<AudioClip>("Sounds\\Widget\\multimedia_rollover_078");
		HoverSound = new Resource<AudioClip>("Sounds\\Widget\\Menu_LoadSave_HoverOver");
		CancelSound = new Resource<AudioClip>("Sounds\\Widget\\multimedia_minimise_window_cancel");
		DeleteSound = new Resource<AudioClip>("Sounds\\Widget\\bustmetal2");
		MakeMolotovSound = new Resource<AudioClip>("Sounds\\Equipment\\MolotovCocktail\\vodka_in_bottle_movement");
		TabSound = new Resource<AudioClip>("Sounds/Tab");
		TargetSound = new Resource<AudioClip>("Sounds\\Widget\\fan4");
		HintSound = new Resource<AudioClip>("Sounds\\Widget\\desk_bell");
		ForwardPageSound = new Resource<AudioClip>("Sounds\\Widget\\Menu_Select_ForwardPage");
		BackwardPageSound = new Resource<AudioClip>("Sounds\\Widget\\Menu_Select_BackPage");
		StartSaveGameSound = new Resource<AudioClip>("Sounds\\Widget\\Menu_LoadSave_StartSaveGame");
		DialogOpenSound = new Resource<AudioClip>("Sounds\\Widget\\Menu_LoadSave_SessionTypePopup");
		HallelujahSound = new Resource<AudioClip>("Sounds\\Widget\\hallelujah");
		SuccessSound = new Resource<AudioClip>("Sounds\\Hud\\Success");
		StatusNotificationSound = new Resource<AudioClip>("Sounds\\Widget\\146718__leszek-szary__button");
		for (int num67 = 1; num67 <= 3; num67++)
		{
			NotificationSounds.Add(new Resource<AudioClip>("Sounds\\Widget\\comedy_bubble_pop_00" + num67));
		}
		for (int num68 = 1; num68 <= 5; num68++)
		{
			EquipmentNotificationSounds.Add(new Resource<AudioClip>("Sounds\\Widget\\pop" + num68));
		}
		CoinSounds.Add(new Resource<AudioClip>("Sounds\\Widget\\Coins 2"));
		CoinSounds.Add(new Resource<AudioClip>("Sounds\\Widget\\Coins 4"));
		CoinSounds.Add(new Resource<AudioClip>("Sounds\\Widget\\Coins 5"));
		RadioMusic.Add(new Resource<AudioClip>("Music\\148695__strangereight__ambient-acoustic"));
	}

	public SoundManager()
	{
		Instance = this;
	}

	private void Awake()
	{
		UnityAudioSource = GetComponent<AudioSource>();
		UnityHeartBeatAudioSource = base.transform.Find("HeartBeatSoundSource").GetComponent<AudioSource>();
		UnityMalePantingAudioSource = base.transform.Find("MalePantingSoundSource").GetComponent<AudioSource>();
		UnityFemalePantingAudioSource = base.transform.Find("FemalePantingSoundSource").GetComponent<AudioSource>();
	}

	public void OnMenuOpen()
	{
		UnityHeartBeatAudioSource.Stop();
		UnityMalePantingAudioSource.Stop();
		UnityFemalePantingAudioSource.Stop();
		GameImpl.Instance.UnityRainSound.Stop();
		GameImpl.Instance.UnityCricketsSound.Stop();
		GameImpl.Instance.UnityWindInGrassSound.Stop();
	}

	public static void PlayMenuSoundFromList(List<Resource<AudioClip>> clips)
	{
		PlayMenuSoundFromList(clips, 1f);
	}

	public static void PlayMenuSoundFromList(List<Resource<AudioClip>> clips, float volume)
	{
		if (clips.Count > 0)
		{
			PlayMenuSound(clips[MathUtil.NonDeterministicRand.Next(clips.Count)], volume);
		}
	}

	public static void PlayMenuSound(AudioClip clip)
	{
		PlayMenuSound(clip, 1f);
	}

	public static void PlayMenuSound(AudioClip clip, float volume)
	{
		Thread currentThread = Thread.CurrentThread;
		if (currentThread == GameImpl.Instance.MainThread)
		{
			Instance.UnityAudioSource.PlayOneShot(clip, MenuSoundVolume * volume);
		}
		else if (currentThread == GameImpl.Instance.HandleInputThread)
		{
			QueuedSounds.Add(new KeyValuePair<AudioClip, float>(clip, volume));
		}
		else
		{
			Debug.LogWarning("PlayMenuSound called from unexpected thread");
		}
	}

	public static void PlayQueuedSounds()
	{
		for (int i = 0; i < QueuedSounds.Count; i++)
		{
			KeyValuePair<AudioClip, float> keyValuePair = QueuedSounds[i];
			Instance.UnityAudioSource.PlayOneShot(keyValuePair.Key, MenuSoundVolume * keyValuePair.Value);
		}
		QueuedSounds.Clear();
	}

	public static void PlayPipSoundFromList(List<Resource<AudioClip>> clips)
	{
		PlayPipSoundFromList(clips, 1f);
	}

	public static void PlayPipSoundFromList(List<Resource<AudioClip>> clips, float volume)
	{
		if (clips.Count > 0)
		{
			PlayPipSound(clips[MathUtil.NonDeterministicRand.Next(clips.Count)], volume);
		}
	}

	public static void PlayPipSound(AudioClip clip)
	{
		PlayPipSound(clip, 1f);
	}

	public static void PlayPipSound(AudioClip clip, float volume)
	{
		Instance.UnityAudioSource.PlayOneShot(clip, WorldSoundVolume * volume);
	}

	public static void PlaySound3DFromList(List<Resource<AudioClip>> clips, Vector3 pos)
	{
		PlaySound3DFromList(clips, pos, 1f, isBackground: false, isBush: false);
	}

	public static void PlaySound3DFromList(List<Resource<AudioClip>> clips, Vector3 pos, float volume)
	{
		PlaySound3DFromList(clips, pos, 1f, isBackground: false, isBush: false);
	}

	public static void PlaySound3DFromList(List<Resource<AudioClip>> clips, Vector3 pos, float volume, bool isBackground, bool isBush)
	{
		if (clips.Count > 0)
		{
			PlaySound3D(clips[MathUtil.NonDeterministicRand.Next(clips.Count)], pos, volume, isBackground, isBush);
		}
	}

	public static void PlaySound3D(AudioClip clip, Vector3 pos)
	{
		PlaySound3D(clip, pos, 1f, isBackground: false, isBush: false);
	}

	public static void PlaySound3D(AudioClip clip, Vector3 pos, float volume)
	{
		PlaySound3D(clip, pos, volume, isBackground: false, isBush: false);
	}

	public static void PlaySound3D(AudioClip clip, Vector3 pos, float volume, bool isBackground, bool isBush)
	{
		if (!Instance.IsTooFarAwayToHearSound(pos))
		{
			GameObject obj = new GameObject();
			obj.transform.position = pos;
			AudioSource audioSource = obj.AddComponent<AudioSource>();
			audioSource.clip = clip;
			audioSource.volume = (isBackground ? BackgroundSoundVolume : WorldSoundVolume) * volume;
			audioSource.minDistance = AudioRolloffMinDist;
			if (isBush)
			{
				audioSource.maxDistance = Character.FootstepSoundDist;
				audioSource.rolloffMode = AudioRolloffMode.Linear;
			}
			else
			{
				audioSource.maxDistance = AudioRolloffMaxDist;
				audioSource.RealisticRolloff();
			}
			audioSource.Play();
			Object.Destroy(obj, clip.length);
		}
	}

	public bool IsTooFarAwayToHearSound(Vector3 pos)
	{
		if ((pos - HudBehaviour.Instance.UnityGameCameraObj.transform.position).sqrMagnitude <= MainMaxSoundDist * MainMaxSoundDist)
		{
			return false;
		}
		return true;
	}
}
