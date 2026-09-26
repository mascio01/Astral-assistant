using System;
using UnityEngine;

public class Pip
{
	private static Vector3 KickTarget = new Vector3(0f, 1.35f, 0f);

	private static Vector3 KickPos = new Vector3(-1.35f, 1.35f, 0f);

	private static Vector3 PinnedTarget = new Vector3(0f, 0.25f, 0f);

	private static Vector3 PinnedPos = new Vector3(-2f, 0.25f, 0f);

	private static Vector3 ZombieEatingTarget = new Vector3(0f, 0.25f, 0.6f);

	private static Vector3 ZombieEatingPos = new Vector3(-1f, 1f, 0.6f);

	private static Vector3 GetUpTarget = new Vector3(0f, 1f, 0f);

	private static Vector3 GetUpPos = new Vector3(0f, 1f, 2f);

	public static float DefaultYOffset = -0.02f;

	public static float DefaultDistFromFocus = 0.45f;

	public static float LooseCloseUpYOffset = -0.08f;

	public static float LooseCloseUpDistFromFocus = 0.6f;

	public static float LooseCloseUpCrouchingYOffset = -0.15f;

	public static float LooseCloseUpCrouchingDistFromFocus = 1f;

	public static float MediumShotYOffset = -0.18f;

	public static float MediumShotDistFromFocus = 1f;

	public static float WalkingYOffset = -0.07f;

	public static float WalkingYOffsetLimping = -0.25f;

	public static float WalkingDistFromFocus = 0.5f;

	public static float WalkingDistFromFocusLimping = 1f;

	public static float SprintingYOffset = -0.2f;

	public static float SprintingYOffsetLimping = -0.5f;

	public static float SprintDistFromFocus = 0.6f;

	public static float SprintDistFromFocusLimping = 1.2f;

	public static float SittingFocusYFrac = 0.4f;

	public static float SittingDistFromFocus = 0.5f;

	public static float CrouchFocusXFrac = 0.075f;

	public static float CrouchFocusYFrac = 0.45f;

	public static float CrouchFocusXFracBow = 0.1f;

	public static float CrouchFocusYFracBow = 0.6f;

	public static float CrouchDistFromFocus = 1f;

	public static float CrouchMovingFocusXFrac = 0.075f;

	public static float CrouchMovingFocusYFrac = 0.5f;

	public static float CrouchMovingFocusXFracBow = 0.1f;

	public static float CrouchMovingFocusYFracBow = 0.65f;

	public static float CrouchMovingDistFromFocus = 1.25f;

	public static float CrouchAimingFocusXFrac = 0f;

	public static float CrouchAimingFocusYFrac = 0.5f;

	public static float CrouchAimingFocusXFracBow = 0.05f;

	public static float CrouchAimingFocusYFracBow = 0.65f;

	public static float CrouchAimingDistFromFocus = 1.25f;

	public static float CrouchMovingAimingFocusXFrac = 0.1f;

	public static float CrouchMovingAimingFocusYFrac = 0.5f;

	public static float CrouchMovingAimingFocusXFracBow = 0.1f;

	public static float CrouchMovingAimingFocusYFracBow = 0.75f;

	public static float CrouchMovingAimingDistFromFocus = 1.25f;

	public static float AimingFocusXFrac = 0.05f;

	public static float AimingFocusYFrac = 0.75f;

	public static float AimingFocusXFracBow = 0.05f;

	public static float AimingFocusYFracBow = 0.8f;

	public static float AimingDistFromFocus = 1.25f;

	public static float CorpseTransitionSpeed = 4f;

	public static float DeadDistFromFocus = 0.75f;

	public static float DeadPitch = MathF.PI * 3f / 8f;

	public static float PitchDelt = MathF.PI * 3f / 8f;

	public static float BunnyDistFromFocus = 0.6f;

	public static float BunnyZOffset = 0.1f;

	public static float BunnyAlertYOffset = 0.1f;

	public static float BunnyGrassCullDist = 1.5f;

	public static float CarryingYaw = -MathF.PI / 8f;

	public static float CarryingCrouchingYaw = 0f;

	public static float WalkBounceFrequency = 1f;

	public static float WalkBounceAmplitude = 0.005f;

	public static float JogBounceFrequency = 1.5f;

	public static float JogBounceAmplitude = 0.0075f;

	public static float RunBounceFrequency = 2f;

	public static float RunBounceAmplitude = 0.01f;

	public static float BounceFrequencyDelt = 4f;

	public static float BounceAmplitudeDelt = 0.02f;

	public static float FocusBuildingPitch = 0f;

	public static float FocusBuildingCamRotSpeed = 0.05f;

	public static float FocusBuildingMinHeight = 0.5f;

	private float BounceAmplitude;

	private float BounceFrequency;

	private float BounceTime;

	private float YOffset = DefaultYOffset;

	private float XOffset;

	private float ZOffset;

	private float Yaw;

	private float Pitch;

	private float DistFromFocusCharacter = DefaultDistFromFocus;

	private float CorpseTransition;

	public bool CanUseRightStickToGoToPipObject;

	public TimeSpan FocusObjectLastUpdatedTime;

	public TileObject FocusObject;

	public bool FocusObjectIsInteresting;

	public TileObject FocusObjectPredicted
	{
		get
		{
			TileObject obj = FocusObject;
			PredictedObjectManager.ConvertToPredictedCharacterIfItExists(ref obj);
			return obj;
		}
	}

	public Vector3 GetFocusPos()
	{
		if (FocusObject == null)
		{
			return Vector3.zero;
		}
		return FocusObject.Pos;
	}

	public void OnStart()
	{
		UpdateCam(start: true);
	}

	public void Update()
	{
		UpdateCam(start: false);
	}

	private void UpdateCam(bool start)
	{
		Session instance = Session.Instance;
		Hud instance2 = Hud.Instance;
		HudBehaviour instance3 = HudBehaviour.Instance;
		bool piPBackgroundEnabled = GameImpl.Instance.Settings.PiPBackgroundEnabled;
		bool sidebarLayoutEnabled = GameImpl.Instance.Settings.SidebarLayoutEnabled;
		bool flag = instance.IsPaused();
		FocusObjectLastUpdatedTime = instance.PlayTime;
		CanUseRightStickToGoToPipObject = false;
		bool flag2 = false;
		bool flag3 = false;
		TileObject tileObject = (sidebarLayoutEnabled ? instance2.LocalControlledCharacter : null);
		if (instance2.LocalControlledCharacter != null && instance2.LocalControlledCharacter.InsideBuilding != null && !instance2.LocalControlledCharacter.IsOutdoors())
		{
			tileObject = instance2.LocalControlledCharacter.InsideBuilding;
		}
		if (!sidebarLayoutEnabled && tileObject is EnterableVehicle enterableVehicle && enterableVehicle.IsDrivingTooFastToShowMenu())
		{
			tileObject = null;
		}
		if (instance2.LocalTargetObject is Character)
		{
			tileObject = instance2.LocalTargetObject;
			flag3 |= instance2.LocalWantLockOnTarget;
		}
		if (instance.GameCamera.FlyCam && instance2.CursorTargetObject is Character && instance2.CursorTargetObject.IsVisibleInFogOfWar())
		{
			tileObject = instance2.CursorTargetObject;
		}
		if (instance.Editor)
		{
			if (instance2.EditorSelectedObject != null)
			{
				tileObject = instance2.EditorSelectedObject;
			}
			else if (instance2.CursorRayCastResult.HitObject is TileObject)
			{
				tileObject = instance2.CursorRayCastResult.HitObject as TileObject;
			}
		}
		if (InfoScreen.Instance.Active)
		{
			tileObject = InfoScreen.Instance.CurrentObject;
			flag3 = true;
		}
		if (sidebarLayoutEnabled && tileObject == null)
		{
			tileObject = Session.Instance.CommunityManager.PlayerCommunity.Leader;
		}
		if (!InfoScreen.Instance.Active && instance.PlaySpeed != PlaySpeed.Paused)
		{
			foreach (PlayerRecord playerRecord in instance.PlayerRecords)
			{
				if (playerRecord.PlayerMode == PlayerMode.Controlling && !playerRecord.IsLocal && playerRecord.PlayerCharacter.FindActiveGoal(GoalType.Conversation) is Conversation conversation && !conversation.IsMovingToTarget())
				{
					tileObject = (conversation.IsListening() ? conversation.GetTargetCharacter() : playerRecord.PlayerCharacter);
					flag3 = true;
				}
			}
			if (tileObject is Character character)
			{
				if (character.FindActiveGoal(GoalType.ListenToTarget) is ListenToTarget listenToTarget)
				{
					tileObject = listenToTarget.GetTargetCharacter();
					flag3 = true;
				}
				flag3 |= character.InCombat && character.IsOutdoors() && !character.IsFleeing();
				flag3 |= character.Speaking != null;
				if (!flag3)
				{
					flag3 = character.FindActiveGoal(GoalType.Conversation) != null;
				}
			}
			Character mostInterestingSpeaker = StoryManager.Instance.GetMostInterestingSpeaker(skipIfLowerPriorityThanLocalPlayerConversation: true, skipIfLowerPriorityThanAnyPlayerConversation: false);
			if (mostInterestingSpeaker != null)
			{
				tileObject = mostInterestingSpeaker;
				flag3 = true;
				flag2 = true;
			}
			else if (StoryManager.Instance.DramaticDeathCharacter != null && StoryManager.Instance.DramaticDeathCharacter.IsVisibleInFogOfWar())
			{
				tileObject = StoryManager.Instance.DramaticDeathCharacter;
				flag3 = true;
				flag2 = true;
			}
			if (!flag3 && instance2.LocalControlledCharacter != null)
			{
				Character character2 = null;
				float num = float.MinValue;
				foreach (Character member in instance.CommunityManager.PlayerCommunity.Members)
				{
					if (!member.InCombat || member.Zombie || !member.IsAwake || !member.IsOutdoors() || member.IsFleeing())
					{
						continue;
					}
					Goal goal = member.FindActiveGoal(GoalType.Attack);
					if (goal == null || goal.Target == null || !goal.Target.GetFlag(TargetFlags.Inaccessible))
					{
						float num2 = (member.PosXZ - instance2.LocalControlledCharacter.PosXZ).magnitude * 0.01f;
						num2 += member.BloodLoss * 1000f;
						if (member.IsBeingBittenOrChoked())
						{
							num2 += 1000f;
						}
						if (member.IsChokingSomeone())
						{
							num2 += 2000f;
						}
						if (member == FocusObject)
						{
							num2 += 100f;
						}
						if (member.Speaking != null)
						{
							num2 += 10000f;
						}
						if (num2 > num)
						{
							character2 = member;
							num = num2;
						}
					}
				}
				if (character2 != null)
				{
					if (character2.InteractionObject == instance2.LocalControlledCharacter)
					{
						character2 = instance2.LocalControlledCharacter;
					}
					tileObject = character2;
					flag3 = true;
					flag2 = true;
				}
			}
		}
		if (tileObject is Character { InsideBuilding: not null } character3 && !character3.IsOutdoors())
		{
			tileObject = character3.InsideBuilding;
		}
		bool flag4 = start;
		if (FocusObject != tileObject)
		{
			if (FocusObject is Character character4)
			{
				character4.GetPredictedOrElseThisCharacter().Unity.SetWantUpdateWhenOffscreen(v: false);
			}
			FocusObject = tileObject;
			flag4 = true;
		}
		FocusObjectIsInteresting = flag3;
		if (FocusObject != null && FocusObject.Deleted)
		{
			FocusObject = null;
		}
		TileObject focusObjectPredicted = FocusObjectPredicted;
		float grassRangeMin = 0f;
		if (focusObjectPredicted != null)
		{
			if (focusObjectPredicted is Character character5)
			{
				character5.Unity.SetWantUpdateWhenOffscreen(GraphicsDebugMenu.PipCommandBuffers && !GameImpl.Instance.Settings.PiPBackgroundEnabled);
				if ((bool)character5.Unity.UmaExpressionPlayer)
				{
					character5.Unity.UmaExpressionPlayer.WantExpensiveUpdate = true;
				}
				if ((FocusObject == instance2.LocalTargetObject || FocusObject == instance2.CursorTargetObject) && character5.IsControllableByPlayer())
				{
					flag2 = true;
				}
				if (flag2 && FocusObject != instance2.LocalControlledCharacter && (instance.FollowerCommandsEnabled || character5.IsControllableByPlayer()))
				{
					CanUseRightStickToGoToPipObject = true;
				}
				float target = 0f;
				float target2 = 0f;
				float pipFocusDist = character5.GetPipFocusDist();
				float defaultYOffset = DefaultYOffset;
				float num3 = 0f;
				float num4 = 0f;
				float target3 = 0f;
				float walkSpeed = character5.GetWalkSpeed();
				float runSpeed = character5.GetRunSpeed();
				float movementSpeed = character5.MovementSpeed;
				bool flag5 = character5.EquippedItem is Bow;
				Animal animal = character5 as Animal;
				if (character5.Sitting && animal == null && character5.CurrentActionAnim != ActionAnim.HandsUp)
				{
					defaultYOffset = SittingFocusYFrac * character5.Appearance.Height;
					pipFocusDist = SittingDistFromFocus;
				}
				else if (character5.IsCrouching() && character5.CurrentActionAnim != ActionAnim.HandsUp)
				{
					if (character5.UnityIsInAimingAnim())
					{
						if (movementSpeed > 0.01f)
						{
							num3 = (flag5 ? CrouchMovingAimingFocusXFracBow : CrouchMovingAimingFocusXFrac) * character5.Appearance.Height;
							defaultYOffset = (flag5 ? CrouchMovingAimingFocusYFracBow : CrouchMovingAimingFocusYFrac) * character5.Appearance.Height;
							pipFocusDist = CrouchMovingAimingDistFromFocus;
						}
						else
						{
							num3 = (flag5 ? CrouchAimingFocusXFracBow : CrouchAimingFocusXFrac) * character5.Appearance.Height;
							defaultYOffset = (flag5 ? CrouchAimingFocusYFracBow : CrouchAimingFocusYFrac) * character5.Appearance.Height;
							pipFocusDist = CrouchAimingDistFromFocus;
						}
					}
					else if (movementSpeed > 0.01f)
					{
						num3 = (flag5 ? CrouchMovingFocusXFracBow : CrouchMovingFocusXFrac) * character5.Appearance.Height;
						defaultYOffset = (flag5 ? CrouchMovingFocusYFracBow : CrouchMovingFocusYFrac) * character5.Appearance.Height;
						pipFocusDist = CrouchMovingDistFromFocus;
					}
					else
					{
						num3 = (flag5 ? CrouchFocusXFracBow : CrouchFocusXFrac) * character5.Appearance.Height;
						defaultYOffset = (flag5 ? CrouchFocusYFracBow : CrouchFocusYFrac) * character5.Appearance.Height;
						pipFocusDist = CrouchDistFromFocus;
					}
				}
				else
				{
					float num5 = WalkingDistFromFocus;
					float num6 = WalkingYOffset;
					float b = SprintDistFromFocus;
					float b2 = SprintingYOffset;
					if (character5.ShouldLimp())
					{
						num5 = WalkingDistFromFocusLimping;
						num6 = WalkingYOffsetLimping;
						b = SprintDistFromFocusLimping;
						b2 = SprintingYOffsetLimping;
					}
					if (animal != null)
					{
						num5 = pipFocusDist;
						num6 = 0f;
						b = pipFocusDist;
						b2 = 0f;
					}
					if (movementSpeed <= walkSpeed)
					{
						float num7 = movementSpeed / walkSpeed;
						target = num7 * WalkBounceFrequency;
						target2 = num7 * WalkBounceAmplitude;
						pipFocusDist = Mathf.Lerp(pipFocusDist, num5, Mathf.Clamp01(movementSpeed / walkSpeed));
						defaultYOffset = character5.Appearance.EyeHeight + Mathf.Lerp(DefaultYOffset, num6, Mathf.Clamp01(movementSpeed / walkSpeed));
					}
					else if (movementSpeed <= Character.JogSpeed)
					{
						float t = (movementSpeed - walkSpeed) / (Character.JogSpeed - walkSpeed);
						target = Mathf.Lerp(WalkBounceFrequency, JogBounceFrequency, t);
						target2 = Mathf.Lerp(WalkBounceAmplitude, JogBounceAmplitude, t);
						pipFocusDist = num5;
						defaultYOffset = character5.Appearance.EyeHeight + num6;
					}
					else
					{
						float t2 = (movementSpeed - Character.JogSpeed) / (runSpeed - Character.JogSpeed);
						target = Mathf.Lerp(JogBounceFrequency, RunBounceFrequency, t2);
						target2 = Mathf.Lerp(JogBounceAmplitude, RunBounceAmplitude, t2);
						pipFocusDist = Mathf.Lerp(num5, b, Mathf.Clamp01((movementSpeed - Character.JogSpeed) / (runSpeed - Character.JogSpeed)));
						defaultYOffset = character5.Appearance.EyeHeight + Mathf.Lerp(num6, b2, Mathf.Clamp01(movementSpeed - Character.JogSpeed) / (runSpeed - Character.JogSpeed));
					}
					if (character5.UnityIsInAimingAnim())
					{
						float num8 = (flag5 ? AimingFocusXFracBow : AimingFocusXFrac) * character5.Appearance.Height;
						float num9 = (flag5 ? AimingFocusYFracBow : AimingFocusYFrac) * character5.Appearance.Height;
						num8 *= ((character5.EquippedItem is AmmoWeapon) ? 1f : 0f);
						pipFocusDist = AimingDistFromFocus;
						defaultYOffset = num9;
						num3 = num8;
					}
				}
				if (character5.GetActionAnimPipView() == PipAnimView.LooseCloseUp || character5.GetActionAnimPipView() == PipAnimView.FullBodySide || character5.CarryingObject != null || character5.Zombie)
				{
					pipFocusDist = (character5.IsCrouching() ? LooseCloseUpCrouchingDistFromFocus : LooseCloseUpDistFromFocus);
					defaultYOffset = (character5.IsCrouching() ? (character5.Appearance.CrouchingEyeHeight + LooseCloseUpCrouchingYOffset) : (character5.Appearance.EyeHeight + LooseCloseUpYOffset));
				}
				if ((character5.Zombie && character5.InCombat) || (movementSpeed >= Character.JogSpeed - 0.1f && character5.EquippedItem is Bow))
				{
					pipFocusDist = MediumShotDistFromFocus;
					defaultYOffset = character5.Appearance.EyeHeight + MediumShotYOffset;
				}
				float deltaTime = Time.deltaTime;
				BounceFrequency = Delt(BounceFrequency, target, BounceFrequencyDelt, flag, flag4, deltaTime);
				BounceAmplitude = Delt(BounceAmplitude, target2, BounceAmplitudeDelt, flag, flag4, deltaTime);
				if (flag4)
				{
					BounceTime = 0f;
				}
				if (!flag)
				{
					BounceTime += BounceFrequency * (MathF.PI * 2f) * deltaTime;
				}
				float num10 = Mathf.Sin(BounceTime) * BounceAmplitude;
				if (character5.IsRagdollOrProneOrRecovering() || character5.IsPlayingDead() || !character5.Alive)
				{
					pipFocusDist = DeadDistFromFocus;
					target3 = DeadPitch;
					if (!flag)
					{
						Yaw += FocusBuildingCamRotSpeed * (MathF.PI * 2f) * deltaTime;
					}
				}
				else
				{
					Yaw = character5.GetPipYaw();
					if (animal != null)
					{
						num3 += animal.GetPipXOffset();
						defaultYOffset += animal.GetPipYOffset();
						num4 += animal.GetPipZOffset();
						grassRangeMin = BunnyGrassCullDist;
					}
				}
				DistFromFocusCharacter = LerpTowards(DistFromFocusCharacter, pipFocusDist, flag, flag4, deltaTime);
				YOffset = LerpTowards(YOffset, defaultYOffset, flag, flag4, deltaTime);
				XOffset = LerpTowards(XOffset, num3, flag, flag4, deltaTime);
				ZOffset = LerpTowards(ZOffset, num4, flag, flag4, deltaTime);
				Pitch = Delt(Pitch, target3, PitchDelt, flag, flag4, deltaTime);
				bool flag6 = character5.IsUnityObjectActive() && (character5.IsRagdollOrProneOrRecovering() || character5.GetActionAnimPipView() == PipAnimView.FullBodySide || !character5.Alive);
				CorpseTransition = Delt(CorpseTransition, flag6 ? 1f : 0f, CorpseTransitionSpeed, flag, flag4, deltaTime);
				Vector3 a = character5.Position + new Vector3(0f, YOffset + num10, 0f) + character5.World.Right() * XOffset + character5.World.Forward() * ZOffset;
				a = Vector3.Lerp(a, character5.IsUnityObjectActive() ? character5.GetUnityBoneTransform(character5.GetPipBoneToFocusOnWhenDead()).Translation() : character5.Position, CorpseTransition);
				Vector3 vector = (MathUtil.CreateRotationY(Yaw) * MathUtil.CreateFromAxisAngle(-character5.World.Right(), Pitch)).MultiplyVector(character5.Forward);
				Vector3 vector2 = a + vector * DistFromFocusCharacter;
				if (character5.IsBeingPinnedOrPinning())
				{
					a = character5.World.MultiplyPoint(PinnedTarget);
					vector2 = character5.World.MultiplyPoint(PinnedPos);
				}
				else if (character5.InteractionObject is Character)
				{
					a = character5.World.MultiplyPoint(KickTarget);
					vector2 = character5.World.MultiplyPoint(KickPos);
				}
				else if (character5.GetActionAnimPipView() == PipAnimView.ZombieEating)
				{
					a = character5.World.MultiplyPoint(ZombieEatingTarget);
					vector2 = character5.World.MultiplyPoint(ZombieEatingPos);
				}
				if (piPBackgroundEnabled)
				{
					float tileHeightAtPos = GameTerrain.Instance.GetTileHeightAtPos(vector2.x, vector2.z);
					vector2.y = Mathf.Max(vector2.y, tileHeightAtPos + 0.25f);
				}
				GameObject unityPipCameraObj = HudBehaviour.Instance.UnityPipCameraObj;
				unityPipCameraObj.transform.position = vector2;
				unityPipCameraObj.transform.rotation = Quaternion.LookRotation(a - vector2, Vector3.up);
				instance3.UnityPipDoFCameraBehaviour.focalPoint = (a - vector2).magnitude;
				instance3.UnityPipCamera.farClipPlane = 16f;
			}
			else
			{
				Vector3 vector3 = focusObjectPredicted.GetPipFocusPos();
				float pipFocusDist2 = focusObjectPredicted.GetPipFocusDist();
				if (focusObjectPredicted is Building building)
				{
					int externalInhabitantCount = building.GetExternalInhabitantCount();
					if (externalInhabitantCount > 0)
					{
						int num11 = Session.Instance.Frame / 600 % externalInhabitantCount;
						int i = 0;
						int num12 = -1;
						for (; i < building.Inhabitants.Length; i++)
						{
							if (building.Inhabitants[i] != null && building.GetInhabitantSlotDefs()[i].External)
							{
								num12++;
								if (num12 == num11)
								{
									vector3 = building.Inhabitants[i].GetBoundingBoxCentre();
									pipFocusDist2 = building.GetInhabitantSlotDefs()[i].PipFocusDist;
									break;
								}
							}
						}
					}
				}
				Matrix4x4 pipFocusWorldMatrix = focusObjectPredicted.GetPipFocusWorldMatrix();
				float angle = Time.realtimeSinceStartup * FocusBuildingCamRotSpeed * (MathF.PI * 2f);
				float angle2 = MathF.PI / 180f * FocusBuildingPitch;
				Vector3 vector4 = (MathUtil.CreateRotationY(angle) * MathUtil.CreateFromAxisAngle(pipFocusWorldMatrix.Right(), angle2)).MultiplyVector(pipFocusWorldMatrix.Forward());
				Vector3 vector5 = vector3 + vector4 * pipFocusDist2;
				if (piPBackgroundEnabled)
				{
					float tileHeightAtPos2 = GameTerrain.Instance.GetTileHeightAtPos(vector5.x, vector5.z);
					vector5.y = Mathf.Max(vector5.y, tileHeightAtPos2 + FocusBuildingMinHeight);
				}
				GameObject unityPipCameraObj2 = HudBehaviour.Instance.UnityPipCameraObj;
				unityPipCameraObj2.transform.position = vector5;
				unityPipCameraObj2.transform.rotation = Quaternion.LookRotation(vector3 - vector5, Vector3.up);
				instance3.UnityPipDoFCameraBehaviour.focalPoint = pipFocusDist2 + 0.5f;
				instance3.UnityPipCamera.farClipPlane = Math.Max(16f, pipFocusDist2 + focusObjectPredicted.GetPipClipDist());
			}
			instance3.UnityPipCameraBehaviour.UpdateFrustum();
		}
		instance3.UnityPipGrassCameraBehaviour.GrassRangeMin = grassRangeMin;
	}

	private static float Delt(float cur, float target, float speed, bool paused, bool snap, float frameTime)
	{
		if (!snap)
		{
			if (!paused)
			{
				return MathUtil.Delt(cur, target, speed * frameTime);
			}
			return cur;
		}
		return target;
	}

	private static float LerpTowards(float cur, float target, bool paused, bool snap, float frameTime)
	{
		if (!snap)
		{
			if (!paused)
			{
				return cur + (target - cur) * Mathf.Min(1f, 10f * frameTime);
			}
			return cur;
		}
		return target;
	}
}
