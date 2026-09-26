using System;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera
{
	public bool FlyCam;

	public Vector3 Focus;

	public Vector3 Pos;

	public Vector3 Right;

	public Vector3 Up;

	public Vector3 Forward;

	public Matrix4x4 ProjectionMatrix;

	public Matrix4x4 WorldToCameraMatrix;

	public float ZoomDist;

	private float Rotation;

	private float DirectControlMaxZoomDist;

	public float DirectControlPitch;

	public float FlyModeZoomDist = ZoomedOutDistMax;

	private bool HasSetDirectControlPitchRecently;

	private int TargetOrientation;

	private float OldRotation;

	private float OrientationTransition = 1f;

	public float FlyCamTransition;

	private float FocusYBeforeFlyCamTransition;

	public float RightStickTime;

	public bool WantSnapZoom;

	public bool WantTeleport;

	public Vector3 WantTeleportDest;

	public bool SentFlyCam;

	private Vector2 SentFocusXZ;

	private float SentRotation;

	private float SentFlyModeZoomDist;

	private int SendFlyModeZoomDistCountdown;

	private Vector3 ControlTargetStartPos;

	private float ControlTargetTransition = 1f;

	private float LockRotateStartAngle;

	private float LockRotateStartPitch;

	private float LockRotateTransition;

	private float LockRotateAngleDiff;

	private float LockOnCharacterRagdoll;

	private bool LockOnCharacterInBuilding;

	public static float DirectControlPitchOnFlatGround = 12f;

	public static float LockedOnExtraPitchToTarget = 15f;

	public static float LockedOnExtraPitchToTargetInBuildingNear = 45f;

	public static float LockedOnExtraPitchToTargetInBuildingFar = 15f;

	public static float LockedOnExtraPitchToTargetInBuildingDist = 16f;

	public static float FlyCamPitch = 68f;

	public static float ZoomedInDistUnarmed = 4f;

	public static float ZoomedInDistMelee = 8f;

	public static float ZoomedInDistRanged = 4f;

	public static float ZoomedInDistForest = 2f;

	public static float ZoomedInDistBuilding = 12f;

	public static float ZoomedOutDistMin = 15f;

	public static float ZoomedOutDistMax = 30f;

	public static float ZoomedOutDistEditorMin = 4f;

	public static float ZoomedOutDistEditorMax = 120f;

	public static float ZoomSpeed = 1f;

	public static float FlyModeZoomSpeed = 12f;

	public static float EditorZoomSpeed = 30f;

	public static float MinZoomedInDist = 0.5f;

	public static float FocusPointHeight = 1.4f;

	public static float RagdollLerpTime = 0.5f;

	public static float SideOffsetDist = -0.5f;

	private static float MoveSpeed = 10f;

	private static float FastScrollSpeed = 40f;

	private static float RotSpeed = 2f;

	public static float MaxPitch = MathF.PI / 3f;

	public static float MinPitch = -MathF.PI / 4f;

	private static float MinPitchInBuilding = MathF.PI / 12f;

	private static float MinPitchInWatchtower = 0f;

	private static float MaxPitchInWatchtower = MathF.PI * 89f / 180f;

	private static List<TileObject> Results = new List<TileObject>();

	private float MaxZoomDist;

	public bool SkipZoomDistRaycast;

	private static float SwaySpeed = 2f;

	private static float MaxSway = 60f;

	private Vector3 SwayPos;

	private float SwayTransition;

	private float SwayTime;

	private float ForcePushUp;

	private float SideOffset;

	private const float clipPlaneOffset = 0.07f;

	public void OnStart()
	{
		if (Session.Instance.Editor)
		{
			FlyCam = true;
			Focus = Vector3.zero;
			Rotation = 0f;
			FlyCamTransition = 1f;
			FocusYBeforeFlyCamTransition = 0f;
			TargetOrientation = 0;
			OldRotation = 0f;
			OrientationTransition = 0f;
		}
		else
		{
			PlayerRecord localPlayerRecord = Session.Instance.GetLocalPlayerRecord();
			FlyCam = localPlayerRecord.FlyMode;
			FlyCamTransition = (FlyCam ? 1f : 0f);
			Focus = MathUtil.ToX0Y(localPlayerRecord.SyncedCamFocusPosXZ);
			FocusYBeforeFlyCamTransition = 0f;
			Rotation = localPlayerRecord.SyncedCamAngle;
			FlyModeZoomDist = localPlayerRecord.SyncedFlyModeZoomDist;
			TargetOrientation = (int)(Rotation / (MathF.PI / 2f) + 0.5f) % 4;
			OldRotation = Rotation;
			OrientationTransition = 1f;
		}
		DirectControlPitch = DirectControlPitchOnFlatGround * (MathF.PI / 180f);
		ZoomDist = ZoomedInDistUnarmed;
		SentFlyCam = FlyCam;
		SentFocusXZ = MathUtil.ToXZ(Focus);
		SentRotation = Rotation;
		SentFlyModeZoomDist = FlyModeZoomDist;
		WantSnapZoom = true;
	}

	public void OnFinish()
	{
	}

	public void CalcMinMaxPitch(out float minPitch, out float maxPitch)
	{
		minPitch = MinPitch;
		maxPitch = MaxPitch;
		Character localControlledCharacter = Hud.Instance.LocalControlledCharacter;
		if (localControlledCharacter != null && localControlledCharacter.InsideBuilding != null)
		{
			minPitch = MinPitchInBuilding;
			if (!localControlledCharacter.InsideBuilding.HasAnyInternalSlots())
			{
				minPitch = MinPitchInWatchtower;
				maxPitch = MaxPitchInWatchtower;
			}
		}
	}

	public void HandleInput(InputFrame inputFrame)
	{
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		InputFunctionManager instance3 = InputFunctionManager.Instance;
		Hud instance4 = Hud.Instance;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		int buttonPromptHash = ((FlyCam && !instance4.Cursor.IsPlacingBuilding && instance4.Cursor.IsSettingCropPatch == null) ? ButtonPromptBarBehaviour.PROMPT_ExitCommandMode : 0);
		if (instance3.IsJustPressed(InputFunction.CommandMode, capture: true, buttonPromptHash))
		{
			if (instance.FollowerCommandsEnabled || FlyCam)
			{
				bool snap = false;
				SetFlyCamMode(!FlyCam, snap, inputFrame);
				HintManager.Instance.Hints[10].MarkPerformed();
			}
			else
			{
				HudBehaviour.Instance.SetStatusBarMsg(StringUtil.ApplyFormulae(GameImpl.Translate("HINT_CommandModeIsDisabled")));
			}
		}
		if (FlyCam && instance3.IsJustPressed(InputFunction.Back))
		{
			SetFlyCamMode(!FlyCam, snap: false, inputFrame);
		}
		if (FlyCam)
		{
			num = instance3.GetAxis(InputFunction.MoveHoriz);
			num2 = instance3.GetAxis(InputFunction.MoveVert);
			num5 = instance3.GetAxis(InputFunction.MapZoom, capture: true, ButtonPromptBarBehaviour.PROMPT_Zoom);
			if (Mathf.Abs(num) > 0f || Mathf.Abs(num2) > 0f || Mathf.Abs(num5) != 0f)
			{
				SelectableBehaviour.CurSelectionMode = SelectableBehaviour.SelectionMode.Buttons;
			}
			float axis = instance3.GetAxis(InputFunction.CommandModeRotateCamera, capture: true, ButtonPromptBarBehaviour.PROMPT_RotateCamera);
			if (axis < 0f && GameImpl.RealTimeSinceStartup - RightStickTime > 0.2f && RotateLeft())
			{
				RightStickTime = GameImpl.RealTimeSinceStartup;
			}
			if (axis > 0f && GameImpl.RealTimeSinceStartup - RightStickTime > 0.2f && RotateRight())
			{
				RightStickTime = GameImpl.RealTimeSinceStartup;
			}
		}
		else if (!Hud.Instance.LocalWantLockOnTarget || Hud.Instance.LocalTargetObject == null)
		{
			num3 += instance3.GetAxis(InputFunction.RotateCamera) * RotSpeed;
			num4 += instance3.GetAxis(InputFunction.PitchCamera) * RotSpeed;
		}
		if (FlyCam)
		{
			switch (Hud.Instance.Cursor.GetCursorState())
			{
			case CursorState.MoveLeft:
				num = -1f;
				break;
			case CursorState.MoveRight:
				num = 1f;
				break;
			case CursorState.MoveUp:
				num2 = 1f;
				break;
			case CursorState.MoveDown:
				num2 = -1f;
				break;
			case CursorState.MoveUpLeft:
				num = -1f;
				num2 = 1f;
				break;
			case CursorState.MoveUpRight:
				num = 1f;
				num2 = 1f;
				break;
			case CursorState.MoveDownLeft:
				num = -1f;
				num2 = -1f;
				break;
			case CursorState.MoveDownRight:
				num = 1f;
				num2 = -1f;
				break;
			}
			if (OrientationTransition < 1f)
			{
				float num6 = OldRotation;
				float num7 = (float)TargetOrientation * (MathF.PI / 2f);
				if (Math.Abs(num6 + MathF.PI * 2f - num7) < Math.Abs(num6 - num7))
				{
					num6 += MathF.PI * 2f;
				}
				if (Math.Abs(num6 - MathF.PI * 2f - num7) < Math.Abs(num6 - num7))
				{
					num6 -= MathF.PI * 2f;
				}
				OrientationTransition += RotSpeed * GameImpl.UnscaledDeltaTime;
				if (OrientationTransition >= 1f)
				{
					OrientationTransition = 1f;
					OldRotation = num7;
				}
				Rotation = MathUtil.ClampAngleBetween0AndTwoPi(Mathf.SmoothStep(num6, num7, OrientationTransition));
			}
			else
			{
				float rotation = (float)TargetOrientation * (MathF.PI / 2f);
				Rotation = rotation;
			}
		}
		else
		{
			Rotation = MathUtil.ClampAngleBetween0AndTwoPi(Rotation + num3);
			if (num4 != 0f)
			{
				CalcMinMaxPitch(out var minPitch, out var maxPitch);
				DirectControlPitch = Mathf.Clamp(DirectControlPitch + num4, minPitch, maxPitch);
				HasSetDirectControlPitchRecently = true;
			}
		}
		if (!FlyCam)
		{
			return;
		}
		bool flag = instance3.IsPressed(InputFunction.CommandModeFastScroll);
		float num8 = (flag ? FastScrollSpeed : MoveSpeed);
		if (num != 0f || num2 != 0f)
		{
			Matrix4x4 mat = MathUtil.CreateRotationY(MathF.PI + Rotation);
			Focus += mat.Right() * num * num8 * GameImpl.UnscaledDeltaTime;
			Focus += mat.Forward() * num2 * num8 * GameImpl.UnscaledDeltaTime;
			Focus.x = Mathf.Clamp(Focus.x, 0f - instance2.HalfSize + 1f, instance2.HalfSize - 1f);
			Focus.z = Mathf.Clamp(Focus.z, 0f - instance2.HalfSize + 1f, instance2.HalfSize - 1f);
			HintManager instance5 = HintManager.Instance;
			if (flag)
			{
				instance5.Hints[27].MarkPerformed();
			}
			else if (instance5.Hints[27].CanShowHint() && instance5.Hints[10].Performed && instance5.Hints[10].TimeSinceLastShown >= TimeSpan.FromSeconds(60.0))
			{
				instance5.Hints[27].StartShowing(GameImpl.Translate(HintManager.HINT_FastCamera));
			}
		}
		FlyModeZoomDist = Mathf.Clamp(FlyModeZoomDist - num5 * (instance.Editor ? EditorZoomSpeed : FlyModeZoomSpeed) * GameImpl.UnscaledDeltaTime, instance.Editor ? ZoomedOutDistEditorMin : ZoomedOutDistMin, instance.Editor ? ZoomedOutDistEditorMax : ZoomedOutDistMax);
		if (num5 != 0f)
		{
			SendFlyModeZoomDistCountdown = 60;
		}
	}

	public void PostHandleInput(InputFrame inputFrame)
	{
		if (FlyCam != SentFlyCam)
		{
			inputFrame.AddAction(new InputAction(FlyCam ? InputActionType.EnterFlyMode : InputActionType.LeaveFlyMode));
			SentFlyCam = FlyCam;
		}
		Vector2 vector = MathUtil.ToXZ(Focus);
		if (FlyCam && (vector - SentFocusXZ).sqrMagnitude >= 1f)
		{
			inputFrame.AddAction(InputAction.SetSyncedCamPosXZ(vector));
			SentFocusXZ = vector;
		}
		if (Session.Instance.InputFrame % 15 == 0 && Math.Abs(SentRotation - Rotation) >= MathF.PI / 8f)
		{
			inputFrame.AddAction(InputAction.SetSyncedCamAngle(Rotation));
			SentRotation = Rotation;
		}
		if (SendFlyModeZoomDistCountdown > 0)
		{
			SendFlyModeZoomDistCountdown--;
			if (SendFlyModeZoomDistCountdown == 0)
			{
				inputFrame.AddAction(InputAction.SetSyncedCamZoom(FlyModeZoomDist));
				SentFlyModeZoomDist = FlyModeZoomDist;
			}
		}
	}

	public void Teleport(Vector3 dest)
	{
		WantTeleport = true;
		WantTeleportDest = dest;
	}

	public void TeleportToObject(TileObject obj)
	{
		if (obj != null)
		{
			WantTeleport = true;
			WantTeleportDest = obj.GetBoundingBoxBottom();
		}
	}

	public void SetFlyCamMode(bool on, bool snap, InputFrame inputFrame)
	{
		if (on && !FlyCam)
		{
			FlyCam = true;
			TargetOrientation = (int)((Rotation / (MathF.PI / 2f) + 0.5f) % 4f);
			FocusYBeforeFlyCamTransition = Focus.y;
			if (snap)
			{
				OrientationTransition = 1f;
				FlyCamTransition = 1f;
			}
			else
			{
				OldRotation = Rotation;
				OrientationTransition = 0f;
			}
			Hud.Instance.Cursor.UpdateCursorState();
		}
		else if (!on && FlyCam)
		{
			FlyCam = false;
			Hud.Instance.ClearCursorTarget();
			if (snap)
			{
				FlyCamTransition = 0f;
				TeleportToObject(Session.Instance.GetLocalPlayerCharacterOrBuildingTheyAreIn());
			}
			else
			{
				ControlTargetStartPos = Focus;
				ControlTargetTransition = 0f;
			}
			Hud.Instance.Cursor.UpdateCursorState();
		}
	}

	public bool RotateLeft()
	{
		if (OrientationTransition < 1f)
		{
			return false;
		}
		TargetOrientation = (TargetOrientation + 4 - 1) % 4;
		OrientationTransition = 0f;
		return true;
	}

	public bool RotateRight()
	{
		if (OrientationTransition < 1f)
		{
			return false;
		}
		TargetOrientation = (TargetOrientation + 1) % 4;
		OrientationTransition = 0f;
		return true;
	}

	public void Update(float dts)
	{
		GameTerrain instance = GameTerrain.Instance;
		Hud instance2 = Hud.Instance;
		if (WantTeleport && FlyCam)
		{
			OrientationTransition = 1f;
			Rotation = (float)TargetOrientation * (MathF.PI / 2f);
		}
		Character character = instance2.LocalControlledCharacter;
		if (character != null)
		{
			character = character.GetPredictedOrElseThisCharacter();
		}
		bool flag = false;
		if (!FlyCam)
		{
			if (character != null)
			{
				if (instance2.LocalWantLockOnTarget && instance2.LocalTargetObject != null)
				{
					Character character2 = instance2.LocalTargetObject as Character;
					float b = MathUtil.GetAngleTo(instance2.LocalTargetObject.GetBoundingBoxBottom(), character.Position, 0f);
					if (character.IsBeingBittenOrChoked() || character.IsChokingSomeone())
					{
						b = MathUtil.ClampAngleBetween0AndTwoPi(character.FacingAngle + MathF.PI);
					}
					bool flag2 = character2.InsideBuilding != null;
					if (LockRotateTransition == 0f || LockOnCharacterInBuilding != flag2)
					{
						LockRotateTransition = 0f;
						LockRotateStartAngle = Rotation;
						LockRotateAngleDiff = MathUtil.AngleDiff(Rotation, b);
						LockRotateStartPitch = DirectControlPitch;
						LockOnCharacterRagdoll = ((character2.CurrentAnimState == AnimState.Animation) ? 0f : 1f);
						LockOnCharacterInBuilding = flag2;
					}
					LockRotateTransition = Math.Min(1f, LockRotateTransition + dts / (0.25f + 0.75f * LockRotateAngleDiff / MathF.PI));
					LockOnCharacterRagdoll = MathUtil.Delt(LockOnCharacterRagdoll, (character2.CurrentAnimState == AnimState.Animation) ? 0f : 1f, dts / RagdollLerpTime);
					float dest = MathUtil.AngleLerp(LockRotateStartAngle, b, Mathf.SmoothStep(0f, 1f, LockRotateTransition));
					Rotation = MathUtil.AngleStep(Rotation, dest, MathF.PI * dts * 4f);
					flag = true;
				}
				if (character.InsideBuilding is EnterableVehicle enterableVehicle && enterableVehicle.GetPredictedOrElseThisVehicle().UnityObj != null)
				{
					Focus = enterableVehicle.GetPredictedOrElseThisVehicle().UnityObj.transform.position;
				}
				else
				{
					Focus = character.Pos;
				}
				Focus.y = Math.Max(Focus.y + FocusPointHeight, instance.GetTileHeightAtPos(Focus.x, Focus.z));
				if (ControlTargetTransition < 1f)
				{
					Focus = ControlTargetStartPos + (Focus - ControlTargetStartPos) * ControlTargetTransition;
					ControlTargetTransition = Math.Min(1f, ControlTargetTransition + dts / 0.25f);
				}
			}
		}
		else
		{
			Focus.y = Mathf.Lerp(FocusYBeforeFlyCamTransition, instance.GetTileHeightAtPos(Focus.x, Focus.z) + FocusPointHeight, FlyCamTransition);
		}
		if (!flag)
		{
			LockRotateTransition = 0f;
		}
		if (WantTeleport)
		{
			Focus = WantTeleportDest;
			Focus.y = Math.Max(Focus.y + FocusPointHeight, instance.GetTileHeightAtPos(Focus.x, Focus.z));
			ControlTargetTransition = 1f;
			HasSetDirectControlPitchRecently = false;
			WantSnapZoom = true;
			WantTeleport = false;
		}
		FlyCamTransition = Mathf.Clamp(FlyCamTransition + (FlyCam ? 1f : (-1f)) * dts / 0.25f, 0f, 1f);
		CalcMinMaxPitch(out var minPitch, out var maxPitch);
		float value = Vector3.Dot(instance.GetInterpolatedNormalAtPos(MathUtil.ToXZ(Focus)), MathUtil.ToX0Y(-MathUtil.GetDirFromAngle(Rotation)));
		float f = MathF.PI / 2f - Mathf.Acos(Mathf.Clamp(value, -1f, 1f));
		float num = Mathf.Abs(f);
		f = Mathf.Clamp(Mathf.Sign(f) * (MathUtil.Squared(Mathf.Clamp01((num - 0f) / (MathF.PI / 4f))) * (MathF.PI / 4f) + Mathf.Max(0f, num - MathF.PI / 4f)), minPitch, maxPitch);
		float num2 = MathF.PI / 180f * DirectControlPitchOnFlatGround + f;
		if (FlyCam || WantTeleport)
		{
			DirectControlPitch = num2;
			HasSetDirectControlPitchRecently = false;
		}
		if (flag)
		{
			Character character3 = instance2.LocalTargetObject as Character;
			Vector3 boundingBoxBottom = instance2.LocalTargetObject.GetBoundingBoxBottom();
			if (character3 != null)
			{
				boundingBoxBottom.y = Mathf.Lerp(boundingBoxBottom.y, character3.GetUnityPosY(), Mathf.SmoothStep(0f, 1f, LockOnCharacterRagdoll));
			}
			float heightIgnoringCrouching = instance2.LocalTargetObject.GetHeightIgnoringCrouching();
			float magnitude = MathUtil.ToXZ(boundingBoxBottom - character.Position).magnitude;
			float num3 = (float)Math.Atan2(Focus.y - (boundingBoxBottom.y + heightIgnoringCrouching * 0.5f), magnitude);
			num3 = ((character.InsideBuilding != null) ? (num3 + MathF.PI / 180f * Mathf.Lerp(LockedOnExtraPitchToTargetInBuildingNear, LockedOnExtraPitchToTargetInBuildingFar, Mathf.Sqrt(Mathf.Clamp01(magnitude / LockedOnExtraPitchToTargetInBuildingDist)))) : (num3 + MathF.PI / 180f * LockedOnExtraPitchToTarget * Mathf.Sqrt(Mathf.Clamp01(ZoomDist / 4f))));
			float b2 = Mathf.Clamp(num3, minPitch, maxPitch);
			DirectControlPitch = Mathf.Lerp(LockRotateStartPitch, b2, Mathf.SmoothStep(0f, 1f, LockRotateTransition));
			HasSetDirectControlPitchRecently = false;
		}
		else if (!FlyCam && !flag && character != null && character.GetOldVelocity().sqrMagnitude > 0f)
		{
			if (!HasSetDirectControlPitchRecently)
			{
				DirectControlPitch = Mathf.Lerp(DirectControlPitch, num2, dts);
			}
		}
		else
		{
			HasSetDirectControlPitchRecently = false;
		}
		float num4 = (Session.Instance.Editor ? Mathf.Lerp(DirectControlPitchOnFlatGround, FlyCamPitch, Mathf.Clamp01((ZoomDist - ZoomedOutDistEditorMin) / (ZoomedOutDistMin - ZoomedOutDistEditorMin))) : FlyCamPitch);
		float f2 = Mathf.Lerp(DirectControlPitch, MathF.PI / 180f * num4, FlyCamTransition);
		Matrix4x4 mat = MathUtil.CreateRotationY(Rotation);
		Vector3 vector = mat.MultiplyPoint(new Vector3(0f, Mathf.Sin(f2), Mathf.Cos(f2)));
		float num5 = ((character != null && character.EquippedItem is MeleeWeapon) ? ZoomedInDistMelee : ((character != null && character.EquippedItem is Weapon) ? ZoomedInDistRanged : ZoomedInDistUnarmed));
		Vector2 vector2 = MathUtil.ToXZ(Focus + vector * num5);
		Vector2 vector3 = MathUtil.ToXZ(Focus);
		Rect rect = MathUtil.RectExpand(new Rect(vector3, Vector2.zero), new Rect(vector2, Vector2.zero));
		TerrainCoord tileCoordForPosXZ = instance.GetTileCoordForPosXZ(rect.min - 2f * Vector2.one);
		TerrainCoord tileCoordForPosXZ2 = instance.GetTileCoordForPosXZ(rect.max + 2f * Vector2.one);
		instance.GetObjectsOfTypeInRect(tileCoordForPosXZ, tileCoordForPosXZ2, Results, typeof(TreeProp));
		float t = 1f;
		foreach (TreeProp result in Results)
		{
			TerrainCoord tile = result.Tile;
			Vector2 vertexPosXZ = instance.GetVertexPosXZ(tile.x - 1, tile.y - 1);
			Vector2 vertexPosXZ2 = instance.GetVertexPosXZ(tile.x + 2, tile.y + 2);
			MathUtil.DoesSegmentIntersectRect(vector3, vector2, vertexPosXZ, vertexPosXZ2, ref t);
		}
		Results.Clear();
		num5 = Math.Max(ZoomedInDistForest, t * num5);
		DirectControlMaxZoomDist = (WantSnapZoom ? num5 : Mathf.Lerp(DirectControlMaxZoomDist, num5, dts));
		bool flag3 = character != null && character.InsideBuilding != null;
		if (flag3)
		{
			float camDist = character.InsideBuilding.GetInhabitantSlotDef(character).CamDist;
			if (camDist > 0f)
			{
				DirectControlMaxZoomDist = camDist;
			}
			else
			{
				Bounds boundingBox = character.InsideBuilding.GetBoundingBox();
				DirectControlMaxZoomDist = Math.Max(boundingBox.extents.x, boundingBox.extents.z) + ZoomedInDistBuilding;
			}
		}
		if (!SkipZoomDistRaycast || MaxZoomDist == 0f)
		{
			MaxZoomDist = Mathf.Lerp(DirectControlMaxZoomDist, FlyModeZoomDist, FlyCamTransition);
			if (!flag3 && FlyCamTransition < 1f)
			{
				int defaultLayer = Character.DefaultLayer;
				float num6 = (float)Math.Atan(HudBehaviour.Instance.UnityGameCamera.fieldOfView * 0.5f) * HudBehaviour.Instance.UnityGameCamera.nearClipPlane;
				float magnitude2 = new Vector3(num6 * HudBehaviour.Instance.UnityGameCamera.aspect, num6, HudBehaviour.Instance.UnityGameCamera.nearClipPlane).magnitude;
				MathUtil.CheckNaNorInfinity(Focus);
				MathUtil.CheckNaNorInfinity(magnitude2);
				MathUtil.CheckNaNorInfinity(vector);
				MathUtil.CheckNaNorInfinity(MaxZoomDist);
				if (Physics.SphereCast(Focus, magnitude2, vector, out var hitInfo, MaxZoomDist, 1 << defaultLayer))
				{
					MaxZoomDist = hitInfo.distance;
				}
				if (Physics.Raycast(Focus, vector, out hitInfo, MaxZoomDist, 1 << defaultLayer))
				{
					MaxZoomDist = Math.Min(MaxZoomDist, hitInfo.distance);
				}
				Vector3 vector4 = Focus + vector * MaxZoomDist;
				float num7 = Math.Min(MaxZoomDist, 2f);
				if (Physics.Raycast(vector4 - vector * num7, vector, out hitInfo, MaxZoomDist, 1 << Character.CharacterCapsuleLayer))
				{
					MaxZoomDist = Math.Min(MaxZoomDist, MaxZoomDist - num7 + hitInfo.distance);
				}
			}
			MaxZoomDist = Math.Max(MinZoomedInDist, MaxZoomDist);
			SkipZoomDistRaycast = true;
		}
		if (ZoomDist < MaxZoomDist && !WantSnapZoom && FlyCamTransition == 0f)
		{
			ZoomDist += (MaxZoomDist - ZoomDist) * ZoomSpeed * dts;
		}
		else
		{
			ZoomDist = MaxZoomDist;
		}
		WantSnapZoom = false;
		float num8 = 0f;
		if (instance2.LocalWantLockOnTarget && instance2.LocalControlledCharacter != null && instance2.LocalControlledCharacter.EquippedItem is Throwable)
		{
			num8 = 1f;
		}
		SideOffset += (num8 - SideOffset) * 0.25f;
		Vector3 vector5 = mat.Right() * SideOffset * SideOffsetDist;
		Vector3 vector6 = Focus + vector * ZoomDist + vector5;
		Vector3 vector7 = Focus + vector5;
		GameObject unityGameCameraObj = HudBehaviour.Instance.UnityGameCameraObj;
		Camera unityGameCamera = HudBehaviour.Instance.UnityGameCamera;
		Vector3 position = unityGameCameraObj.transform.position;
		float num9 = vector6.y;
		if (flag3 && FlyCamTransition < 1f)
		{
			TileObject ignore = ((character.InsideBuilding.GetInhabitantSlotDef(character).CamDist > 0f) ? character.InsideBuilding : null);
			RaycastResult raycastResult = instance.RayCast(new Ray(vector6 + Vector3.up * 64f, -Vector3.up), 128f, 20, ignore);
			num9 = Math.Max(vector6.y, raycastResult.GetHitPosition().y + 1f);
		}
		Vector3 vector8 = vector6 + ForcePushUp * Vector3.up;
		if (position.y > vector8.y && position.y - vector8.y > MathUtil.ToXZ(position - vector8).magnitude * 0.5f)
		{
			ForcePushUp = num9 - vector6.y;
		}
		else
		{
			ForcePushUp += (num9 - vector8.y) * 0.25f;
		}
		vector6.y += ForcePushUp;
		SwayTransition = MathUtil.Delt(SwayTransition, (!FlyCam && character != null) ? (character.GetBloodAlcoholConcentration() / Character.BACDeath) : 0f, dts * 4f);
		if (SwayTransition > 0f)
		{
			float num10 = character.GetBloodAlcoholConcentration() / Character.BACDeath;
			SwayTime += dts * SwaySpeed;
			float swayTime = SwayTime;
			SwayPos = vector6 + new Vector3(Mathf.Sin(swayTime * 0.6098f) + Mathf.Sin(swayTime * 0.3106f), Mathf.Sin(swayTime * 0.7194f) + Mathf.Sin(swayTime * 0.1203f), Mathf.Sin(swayTime * 0.2332f) + Mathf.Sin(swayTime * 0.8553f)) * dts * num10 * MaxSway;
		}
		else
		{
			SwayTime = 0f;
			SwayPos = vector6;
		}
		vector6 = Vector3.Lerp(vector6, SwayPos, SwayTransition);
		Transform transform = HudBehaviour.Instance.UnityDecalLayersCameraObj.transform;
		Vector3 position2 = (unityGameCameraObj.transform.position = (Pos = vector6));
		transform.position = position2;
		Transform transform2 = HudBehaviour.Instance.UnityDecalLayersCameraObj.transform;
		Quaternion rotation = (unityGameCameraObj.transform.rotation = Quaternion.LookRotation(vector7 - vector6, Vector3.up));
		transform2.rotation = rotation;
		Right = unityGameCameraObj.transform.right;
		Up = unityGameCameraObj.transform.up;
		Forward = unityGameCameraObj.transform.forward;
		WorldToCameraMatrix = unityGameCamera.worldToCameraMatrix;
		ProjectionMatrix = unityGameCamera.projectionMatrix;
		HudBehaviour.Instance.UnityGameCameraBehaviour.UpdateFrustum();
		SetupWaterReflectionCam();
	}

	public void SetFocusInFlyMode(Vector3 focus)
	{
		Vector3 vector = MathUtil.CreateRotationY(Rotation).MultiplyPoint(new Vector3(0f, Mathf.Sin(FlyCamPitch), Mathf.Cos(FlyCamPitch)));
		Focus = focus;
		Pos = Focus + vector * ZoomDist;
	}

	public RaycastResult RayCastFromPointOnScreen(Vector2 screenPos, int flags)
	{
		HudBehaviour instance = HudBehaviour.Instance;
		Ray rayFromPointOnScreen = GetRayFromPointOnScreen(screenPos);
		return GameTerrain.Instance.RayCast(rayFromPointOnScreen, instance.GameCameraFarClipDist, flags);
	}

	public Ray GetRayFromPointOnScreen(Vector2 screenPos)
	{
		HudBehaviour instance = HudBehaviour.Instance;
		Vector2 vector = new Vector2(screenPos.x / instance.MainPanelRect.width * 2f - 1f, screenPos.y / instance.MainPanelRect.height * 2f - 1f);
		float num = instance.GameCameraNearClipDist * Mathf.Tan(instance.GameCameraFOV * 0.5f * (MathF.PI / 180f));
		float num2 = num * instance.GameCameraAspect;
		return new Ray(Pos, (Forward * instance.GameCameraNearClipDist + Right * vector.x * num2 + Up * vector.y * num).normalized);
	}

	public Vector2 GetScreenPosFromPointInWorld(Vector3 worldPos)
	{
		HudBehaviour instance = HudBehaviour.Instance;
		Vector2 vector = MathUtil.ToXY(instance.UnityGameCamera.WorldToScreenPoint(worldPos));
		return new Vector2(vector.x * (instance.MainPanelRectTransform.rect.width / (float)instance.MainViewRenderTexture.width), vector.y * (instance.MainPanelRectTransform.rect.height / (float)instance.MainViewRenderTexture.height));
	}

	public Vector3 GetPos()
	{
		return Pos;
	}

	public Vector3 GetRight()
	{
		return Right;
	}

	public Vector3 GetUp()
	{
		return Up;
	}

	public Vector3 GetForward()
	{
		return Forward;
	}

	public void OnLockedTargetChanged()
	{
		LockRotateTransition = 0f;
	}

	public void SetupWaterReflectionCam()
	{
		Camera unityGameCamera = HudBehaviour.Instance.UnityGameCamera;
		Camera component = GameImpl.Instance.UnityReflectionCameraObj.GetComponent<Camera>();
		Camera component2 = GameImpl.Instance.UnityRefractionCameraObj.GetComponent<Camera>();
		Vector3 zero = Vector3.zero;
		Vector3 up = Vector3.up;
		Vector4 plane = new Vector4(w: 0f - Vector3.Dot(up, zero) - 0.07f, x: up.x, y: up.y, z: up.z);
		Matrix4x4 reflectionMat = Matrix4x4.zero;
		CalculateReflectionMatrix(ref reflectionMat, plane);
		Vector3 position = unityGameCamera.transform.position;
		Vector3 position2 = reflectionMat.MultiplyPoint(position);
		component.worldToCameraMatrix = unityGameCamera.worldToCameraMatrix * reflectionMat;
		Vector4 clipPlane = CameraSpacePlane(component, zero, up, 1f);
		component.projectionMatrix = unityGameCamera.CalculateObliqueMatrix(clipPlane);
		component.transform.position = position2;
		Vector3 eulerAngles = unityGameCamera.transform.eulerAngles;
		component.transform.eulerAngles = new Vector3(0f - eulerAngles.x, eulerAngles.y, eulerAngles.z);
		component2.worldToCameraMatrix = unityGameCamera.worldToCameraMatrix;
		Vector4 clipPlane2 = CameraSpacePlane(component2, zero, up, -1f);
		component2.projectionMatrix = unityGameCamera.CalculateObliqueMatrix(clipPlane2);
		component2.cullingMatrix = unityGameCamera.projectionMatrix * unityGameCamera.worldToCameraMatrix;
		component2.transform.position = unityGameCamera.transform.position;
		component2.transform.rotation = unityGameCamera.transform.rotation;
		Material unityWaterMaterial = GameTerrain.Instance.UnityWaterMaterial;
		unityWaterMaterial.SetFloat(ShaderHash._WaveTime, Time.time);
		unityWaterMaterial.SetFloat(value: Mathf.Clamp01(Session.Instance.Weather.TemperatureInCelsius / Weather.WaterCompletelyFrozenTemperatureInCelsius), nameID: ShaderHash._IceCover);
		unityWaterMaterial.SetFloat(ShaderHash._IceCoverSoftness, GameTerrain.IceCoverSoftness);
	}

	private static Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
	{
		Vector3 point = pos + normal * 0.07f;
		Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
		Vector3 lhs = worldToCameraMatrix.MultiplyPoint(point);
		Vector3 rhs = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
		return new Vector4(rhs.x, rhs.y, rhs.z, 0f - Vector3.Dot(lhs, rhs));
	}

	private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
	{
		reflectionMat.m00 = 1f - 2f * plane[0] * plane[0];
		reflectionMat.m01 = -2f * plane[0] * plane[1];
		reflectionMat.m02 = -2f * plane[0] * plane[2];
		reflectionMat.m03 = -2f * plane[3] * plane[0];
		reflectionMat.m10 = -2f * plane[1] * plane[0];
		reflectionMat.m11 = 1f - 2f * plane[1] * plane[1];
		reflectionMat.m12 = -2f * plane[1] * plane[2];
		reflectionMat.m13 = -2f * plane[3] * plane[1];
		reflectionMat.m20 = -2f * plane[2] * plane[0];
		reflectionMat.m21 = -2f * plane[2] * plane[1];
		reflectionMat.m22 = 1f - 2f * plane[2] * plane[2];
		reflectionMat.m23 = -2f * plane[3] * plane[2];
		reflectionMat.m30 = 0f;
		reflectionMat.m31 = 0f;
		reflectionMat.m32 = 0f;
		reflectionMat.m33 = 1f;
	}
}
