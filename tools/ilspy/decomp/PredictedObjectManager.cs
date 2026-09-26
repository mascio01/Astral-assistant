using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PredictedObjectManager
{
	private struct CharacterScore : IComparable<CharacterScore>
	{
		public Character Character;

		public float Score;

		public int CompareTo(CharacterScore other)
		{
			if (Score > other.Score)
			{
				return -1;
			}
			if (Score < other.Score)
			{
				return 1;
			}
			if (Character.Id < other.Character.Id)
			{
				return -1;
			}
			if (Character.Id > other.Character.Id)
			{
				return 1;
			}
			return 0;
		}
	}

	public class CopySessionStateTaskData : BaseTaskData
	{
		public TileObject Authoritative;

		public TileObject Predicted;

		public CustomBinaryWriterToMemory Serialiser;

		public CustomBinaryReaderFromMemory Deserialiser;

		public ManualResetEvent FinishedEvent = new ManualResetEvent(initialState: true);
	}

	public static PredictedObjectManager Instance;

	public int PredictedInputFrame = -1;

	public int FrontMostPredictedInputFrame = -1;

	public bool IsPredictingFrames;

	public bool HasPredictedAnyFramesSinceLastNonDeterministicUpdate;

	public int PredictedFrame;

	public TimeSpan PredictedTime;

	public PlaySpeed PredictedPlaySpeed = PlaySpeed.Normal;

	public List<PlayerRecord> PredictedPlayerRecords = new List<PlayerRecord>();

	public List<TileObject> PredictedObjects = new List<TileObject>();

	public int PredictedNextFreeInjuryId;

	public CollisionManager PredictedCollisionManager = new CollisionManager();

	private List<CharacterScore> CharactersThatWantPrediction = new List<CharacterScore>();

	private List<TileObject> ObjectsThatWantPrediction = new List<TileObject>();

	private static GameProfiler CopySessionStateTimer = new GameProfiler("Update.Prediction.CopySessionState");

	private static GameProfiler StartCopySessionStateFromObjectTimer = new GameProfiler("Update.Prediction.CopySessionState.StartCopySessionStateFromObject");

	private static GameProfiler WaitCopySessionStateFromObjectTimer = new GameProfiler("Update.Prediction.CopySessionState.WaitCopySessionStateFromObject");

	private static GameProfiler FinishCopySessionStateFromObjectTimer = new GameProfiler("Update.Prediction.CopySessionState.FinishCopySessionStateFromObject");

	private static GameProfiler PredictFramesTimer = new GameProfiler("Update.Prediction.PredictFrames");

	public static bool PredictionEnabled = true;

	private int MaxPredictedCharacters = 8;

	private List<CopySessionStateTaskData> TaskData = new List<CopySessionStateTaskData>();

	private int CurTaskDataIndex;

	private TaskFunc CopyCharacterStateOnThreadFunc;

	private static GameProfiler PredictFrameTimer = new GameProfiler("Update.Prediction.PredictFrames.PredictFrame");

	public PredictedObjectManager()
	{
		Instance = this;
		CopyCharacterStateOnThreadFunc = CopySessionStateFromObjectOnThread;
		MaxPredictedCharacters = MathUtil.Clamp(Environment.ProcessorCount, 2, 8);
	}

	public void Unload()
	{
		ObjectsThatWantPrediction.Clear();
		CharactersThatWantPrediction.Clear();
		PredictedObjects.Clear();
		Instance = null;
	}

	public void OnFinish()
	{
		foreach (TileObject predictedObject in PredictedObjects)
		{
			predictedObject.UnityDelete();
		}
		PredictedObjects.Clear();
	}

	public PlayerRecord GetPredictedPlayerRecord(PlayerID playerID)
	{
		foreach (PlayerRecord predictedPlayerRecord in PredictedPlayerRecords)
		{
			if (predictedPlayerRecord.PlayerID == playerID)
			{
				return predictedPlayerRecord;
			}
		}
		return null;
	}

	public PlayerRecord GetPredictedPlayerControllingCharacter(Character character)
	{
		foreach (PlayerRecord predictedPlayerRecord in PredictedPlayerRecords)
		{
			if (predictedPlayerRecord.PlayerCharacter == character && predictedPlayerRecord.PlayerMode == PlayerMode.Controlling)
			{
				return predictedPlayerRecord;
			}
		}
		return null;
	}

	public TileObject FindPredictedObjectById(int id)
	{
		foreach (TileObject predictedObject in PredictedObjects)
		{
			if (predictedObject.Id == id)
			{
				return predictedObject;
			}
		}
		return null;
	}

	public Projectile FindProjectile(Character source, TileObject target, TimeSpan startTime)
	{
		foreach (TileObject predictedObject in PredictedObjects)
		{
			if (predictedObject is Projectile projectile && projectile._startTime == startTime && projectile._source.GetAuthoritativeOrElseThisCharacter() == source.GetAuthoritativeOrElseThisCharacter() && ((projectile._target != null) ? projectile._target.GetAuthoritativeOrElseThis() : null) == target?.GetAuthoritativeOrElseThis())
			{
				return projectile;
			}
		}
		return null;
	}

	public Projectile CreateProjectile(BaseObjectType objectType, Character source, TileObject target, bool predicted)
	{
		TimeSpan startTime = (predicted ? PredictedTime : Session.Instance.PlayTime);
		Projectile projectile = FindProjectile(source, target, startTime);
		Projectile projectile2 = null;
		if (predicted)
		{
			if (projectile != null)
			{
				projectile2 = projectile;
			}
			else
			{
				projectile2 = BaseObjectManager.Create(objectType) as Projectile;
				PredictedObjects.Add(projectile2);
			}
			projectile2.SetPredictedFreshFrame(PredictedInputFrame);
		}
		else
		{
			projectile2 = BaseObjectManager.Create(objectType) as Projectile;
			projectile2.AssignId(BaseObjectManager.Instance.Assign(projectile2));
			projectile2.SetPredicted(projectile);
			if (projectile != null)
			{
				projectile.SetAuthoritative(projectile2);
				projectile.AssignId(projectile2.Id);
			}
		}
		projectile2._startTime = startTime;
		projectile2._source = source;
		projectile2._target = target;
		return projectile2;
	}

	public void GetPredictedObjectsInSphere(BoundingSphere sphere, List<TileObject> objectsInArea)
	{
		foreach (TileObject predictedObject in PredictedObjects)
		{
			if (predictedObject.GetBoundingBox().Intersects(sphere))
			{
				objectsInArea.Add(predictedObject);
			}
		}
	}

	public void SetupPredictedPlayerRecords()
	{
		foreach (PlayerRecord playerRecord2 in Session.Instance.PlayerRecords)
		{
			PlayerRecord playerRecord = new PlayerRecord();
			playerRecord.PlayerID = playerRecord2.PlayerID;
			playerRecord.PlayerName = playerRecord2.PlayerName;
			playerRecord.IsLocal = playerRecord2.IsLocal;
			playerRecord.IsPartyLeader = playerRecord2.IsPartyLeader;
			PredictedPlayerRecords.Add(playerRecord);
		}
	}

	public void CopyCurrentSessionState()
	{
		using (new ProfileMarker(CopySessionStateTimer))
		{
			Session instance = Session.Instance;
			Hud instance2 = Hud.Instance;
			HasPredictedAnyFramesSinceLastNonDeterministicUpdate = true;
			PredictedInputFrame = instance.InputFrame;
			PredictedTime = instance.PlayTime;
			PredictedFrame = instance.Frame;
			PredictedPlaySpeed = instance.PlaySpeed;
			PredictedNextFreeInjuryId = instance.NextFreeInjuryId;
			PredictedCollisionManager.ClearAllMovers();
			CharactersThatWantPrediction.Clear();
			ObjectsThatWantPrediction.Clear();
			foreach (TileObject item2 in Session.Instance.MovableUnityObjectsVisibleInMainView)
			{
				if (!item2.WantPrediction() || !PredictionEnabled || item2.GetPredictedFreshFrame() == PredictedInputFrame || !item2.CanUnityObjectBeActivated())
				{
					continue;
				}
				if (item2 is Character character)
				{
					if (!character.HasUnityObject() && !character.GetPredictedOrElseThisCharacter().HasUnityObject())
					{
						Debug.LogWarning(character.GetDisplayNameString() + " wants to be predicted but has no Unity Object.  Disappeared: " + character.Disappeared + ", Deleted: " + character.Deleted + ", Alive: " + character.Alive + ", Player: " + character.IsControllableByPlayer() + ", Exists: " + ((BaseObjectManager.Instance.FindBaseObjectByID(character.Id) == character) ? "true" : "false"));
						continue;
					}
					float num = MathUtil.ToXZ(item2.Pos - instance.GameCamera.Focus).magnitude;
					if (item2 == instance2.LocalControlledCharacter || item2 == instance2.LocalTargetObject || item2 == instance2.CursorTargetObject || item2 == instance2.Pip.FocusObject || item2.IsBeingCarried() || character.CarryingObject != null)
					{
						num = 0f;
					}
					if (item2.IsPredicted())
					{
						num *= 0.9f;
					}
					if (!character.DisablePrediction || !(num > 0f))
					{
						CharacterScore item = new CharacterScore
						{
							Character = character,
							Score = num
						};
						CharactersThatWantPrediction.Add(item);
					}
				}
				else
				{
					ObjectsThatWantPrediction.Add(item2);
				}
			}
			foreach (EnterableVehicle movingVehicle in Session.Instance.PropManager.MovingVehicles)
			{
				if (movingVehicle.WantPrediction() && PredictionEnabled && !ObjectsThatWantPrediction.Contains(movingVehicle))
				{
					ObjectsThatWantPrediction.Add(movingVehicle);
				}
			}
			if (CharactersThatWantPrediction.Count >= MaxPredictedCharacters)
			{
				CharactersThatWantPrediction.Sort();
			}
			CurTaskDataIndex = 0;
			for (int i = 0; i < CharactersThatWantPrediction.Count && (i < MaxPredictedCharacters || !(CharactersThatWantPrediction[i].Score > 0f)); i++)
			{
				Character character2 = CharactersThatWantPrediction[i].Character;
				StartCopyingSessionStateFromObject(character2);
			}
			foreach (TileObject item3 in ObjectsThatWantPrediction)
			{
				StartCopyingSessionStateFromObject(item3);
			}
			for (int j = 0; j < CurTaskDataIndex; j++)
			{
				FinishCopyingSessionStateFromObject(TaskData[j]);
			}
			CharactersThatWantPrediction.Clear();
			ObjectsThatWantPrediction.Clear();
			for (int num2 = PredictedObjects.Count - 1; num2 >= 0; num2--)
			{
				TileObject tileObject = PredictedObjects[num2];
				if (tileObject.GetPredictedFreshFrame() < PredictedInputFrame && !tileObject.PredictedWantContinueAfterAuthoritativeHasBeenDeleted())
				{
					tileObject.GetAuthoritative()?.SetPredicted(null);
					tileObject.DeletePredicted();
					PredictedObjects.RemoveAt(num2);
				}
			}
			foreach (TileObject predictedObject in PredictedObjects)
			{
				predictedObject.PredictedFixup();
			}
			for (int k = 0; k < instance.PlayerRecords.Count; k++)
			{
				PlayerRecord playerRecord = instance.PlayerRecords[k];
				PlayerRecord playerRecord2 = PredictedPlayerRecords[k];
				playerRecord2.PlayerMode = playerRecord.PlayerMode;
				playerRecord2.PlayerCharacter = ((playerRecord.PlayerCharacter != null) ? playerRecord.PlayerCharacter.GetPredictedOrElseThisCharacter() : null);
				playerRecord2.FlyMode = playerRecord.FlyMode;
				playerRecord2.TargetObject = ((playerRecord.TargetObject != null) ? playerRecord.TargetObject.GetPredictedOrElseThis() : null);
				playerRecord2.WantLockOnTarget = playerRecord.WantLockOnTarget;
				playerRecord2.TargetBodyLocationToAimFor = playerRecord.TargetBodyLocationToAimFor;
				playerRecord2.TargetPos = playerRecord.TargetPos;
				playerRecord2.ThrowAngle = playerRecord.ThrowAngle;
				playerRecord2.ThrowSpeed = playerRecord.ThrowSpeed;
			}
		}
	}

	private void StartCopyingSessionStateFromObject(TileObject authoritative)
	{
		using (new ProfileMarker(StartCopySessionStateFromObjectTimer))
		{
			TileObject tileObject = authoritative.GetPredicted();
			if (tileObject == null)
			{
				tileObject = BaseObjectManager.Create(authoritative.GetBaseObjectType()) as TileObject;
				tileObject.AssignId(authoritative.Id);
				authoritative.SetPredicted(tileObject);
				tileObject.SetAuthoritative(authoritative);
				PredictedObjects.Add(tileObject);
			}
			tileObject.SetPredictedFreshFrame(PredictedInputFrame);
			if (CurTaskDataIndex >= TaskData.Count)
			{
				CopySessionStateTaskData copySessionStateTaskData = new CopySessionStateTaskData();
				copySessionStateTaskData.Serialiser = new CustomBinaryWriterToMemory(65536);
				copySessionStateTaskData.Deserialiser = new CustomBinaryReaderFromMemory(copySessionStateTaskData.Serialiser._buffer, copySessionStateTaskData.Serialiser._buffer.Length);
				copySessionStateTaskData.Serialiser.IsDoingPrediction = true;
				copySessionStateTaskData.Deserialiser.IsDoingPrediction = true;
				TaskData.Add(copySessionStateTaskData);
			}
			CopySessionStateTaskData copySessionStateTaskData2 = TaskData[CurTaskDataIndex];
			copySessionStateTaskData2.Authoritative = authoritative;
			copySessionStateTaskData2.Predicted = tileObject;
			copySessionStateTaskData2.FinishedEvent.Reset();
			GameImpl.Instance.UpdateThreadPool.AddTask(CopyCharacterStateOnThreadFunc, null, copySessionStateTaskData2, TaskPriority.High);
			CurTaskDataIndex++;
		}
	}

	private void CopySessionStateFromObjectOnThread(BaseTaskData data)
	{
		CopySessionStateTaskData copySessionStateTaskData = (CopySessionStateTaskData)data;
		try
		{
			copySessionStateTaskData.Serialiser.ResetIndex();
			copySessionStateTaskData.Authoritative.ReflectEarly(copySessionStateTaskData.Serialiser);
			copySessionStateTaskData.Authoritative.Reflect(copySessionStateTaskData.Serialiser);
			copySessionStateTaskData.Deserialiser.SetBuffer(copySessionStateTaskData.Serialiser._buffer, copySessionStateTaskData.Serialiser._index);
			copySessionStateTaskData.Predicted.ReflectEarly(copySessionStateTaskData.Deserialiser);
			copySessionStateTaskData.Predicted.Reflect(copySessionStateTaskData.Deserialiser);
		}
		catch (Exception ex)
		{
			Debug.Log("Error in CopySessionStateFromObjectOnThread: " + ex.Message + ex.StackTrace);
		}
		copySessionStateTaskData.FinishedEvent.Set();
	}

	private void FinishCopyingSessionStateFromObject(CopySessionStateTaskData taskData)
	{
		using (new ProfileMarker(WaitCopySessionStateFromObjectTimer))
		{
			taskData.FinishedEvent.WaitOne();
		}
		using (new ProfileMarker(FinishCopySessionStateFromObjectTimer))
		{
			taskData.Predicted.InitPredicted();
			if (taskData.Predicted.HasUnityObject() && !taskData.Predicted.IsUnityObjectActive())
			{
				taskData.Predicted.UnityActivate();
			}
		}
	}

	public static void ConvertToPredictedCharacterIfItExists(ref TileObject obj)
	{
		if (obj != null)
		{
			obj = obj.GetPredictedOrElseThis();
		}
	}

	public int GetPredictedFramesPerFrame()
	{
		return PredictedPlaySpeed switch
		{
			PlaySpeed.Paused => 0, 
			PlaySpeed.Normal => 1, 
			PlaySpeed.FastForwardx2 => 2, 
			PlaySpeed.FastForwardx4 => 4, 
			_ => 0, 
		};
	}

	public int GetCurrentFrame(bool predicted)
	{
		if (!predicted)
		{
			return Session.Instance.Frame;
		}
		return PredictedFrame;
	}

	public TimeSpan GetCurrentTime(bool predicted)
	{
		if (!predicted)
		{
			return Session.Instance.PlayTime;
		}
		return PredictedTime;
	}

	public PlaySpeed GetCurrentPlaySpeed(bool predicted)
	{
		if (!predicted)
		{
			return Session.Instance.PlaySpeed;
		}
		return PredictedPlaySpeed;
	}

	public int GetCurrentFramesPerFrame(bool predicted)
	{
		if (!predicted)
		{
			return Session.Instance.GetFramesPerFrame();
		}
		return GetPredictedFramesPerFrame();
	}

	public bool IsFrontmostPrediction()
	{
		return PredictedInputFrame > FrontMostPredictedInputFrame;
	}

	public void PredictFrames(int targetFrame)
	{
		IsPredictingFrames = true;
		while (PredictedInputFrame < targetFrame)
		{
			PredictedInputFrame++;
			PredictFrame();
			FrontMostPredictedInputFrame = Math.Max(PredictedInputFrame, FrontMostPredictedInputFrame);
			HasPredictedAnyFramesSinceLastNonDeterministicUpdate = true;
		}
		IsPredictingFrames = false;
	}

	public void PredictFrame()
	{
		using (new ProfileMarker(PredictFramesTimer))
		{
			Session instance = Session.Instance;
			int predictedFramesPerFrame = GetPredictedFramesPerFrame();
			if (predictedFramesPerFrame != 0)
			{
				foreach (PlayerRecord predictedPlayerRecord in PredictedPlayerRecords)
				{
					if (predictedPlayerRecord.PlayerCharacter != null && predictedPlayerRecord.PlayerCharacter.IsPredicted() && predictedPlayerRecord.PlayerMode == PlayerMode.Controlling)
					{
						predictedPlayerRecord.PlayerCharacter.UpdatePreProcessInputFrame(predictedPlayerRecord);
					}
				}
			}
			for (int i = 0; i < instance.NetworkPlayerIDs.Count; i++)
			{
				PartyMember partyMemberByID = OnlineParty.Instance.GetPartyMemberByID(instance.NetworkPlayerIDs[i]);
				if (partyMemberByID != null)
				{
					if (PredictedInputFrame <= partyMemberByID.ReceivedInputFrame)
					{
						ProcessInputFrame(partyMemberByID, partyMemberByID.GetInputFrame(PredictedInputFrame), extrapolating: false);
					}
					else if (partyMemberByID.ReceivedInputFrame >= 0)
					{
						ProcessInputFrame(partyMemberByID, partyMemberByID.GetInputFrame(partyMemberByID.ReceivedInputFrame), extrapolating: true);
					}
				}
			}
			if (predictedFramesPerFrame != 0)
			{
				foreach (PlayerRecord predictedPlayerRecord2 in PredictedPlayerRecords)
				{
					if (predictedPlayerRecord2.PlayerCharacter != null && predictedPlayerRecord2.PlayerCharacter.IsPredicted() && predictedPlayerRecord2.PlayerMode == PlayerMode.Controlling)
					{
						predictedPlayerRecord2.PlayerCharacter.UpdatePostProcessInputFrame(predictedPlayerRecord2);
					}
				}
			}
			TimeSpan timeSpan = MathUtil.FromSeconds(1f / 60f * (float)predictedFramesPerFrame);
			PredictedTime += timeSpan;
			PredictedFrame += predictedFramesPerFrame;
			for (int j = 0; j < PredictedObjects.Count; j++)
			{
				TileObject tileObject = PredictedObjects[j];
				tileObject.PredictedUpdate(timeSpan);
				if (tileObject.PredictedWantDelete())
				{
					tileObject.GetAuthoritative()?.SetPredicted(null);
					tileObject.DeletePredicted();
					PredictedObjects.RemoveAt(j);
					j--;
				}
			}
			for (int k = 0; k < predictedFramesPerFrame; k++)
			{
				PredictedCollisionManager.ProcessMovers(10, predicted: true);
			}
		}
	}

	public void RemovePredictedObject(TileObject predicted)
	{
		int num = PredictedObjects.IndexOf(predicted);
		if (num >= 0)
		{
			predicted.GetAuthoritative()?.SetPredicted(null);
			predicted.DeletePredicted();
			PredictedObjects.RemoveAt(num);
		}
	}

	public void ProcessInputFrame(PartyMember partyMember, InputFrame inputFrame, bool extrapolating)
	{
		PlayerRecord predictedPlayerRecord = GetPredictedPlayerRecord(partyMember.PlayerID);
		if (!inputFrame.HasActionsWhichYouMustLetGoOfToFireAgain())
		{
			predictedPlayerRecord.MustLetGoToFireAgain = false;
		}
		for (int i = 0; i < inputFrame.Actions.Count; i++)
		{
			ProcessInputAction(predictedPlayerRecord, inputFrame.Actions[i], extrapolating);
		}
	}

	public void ProcessInputAction(PlayerRecord predictedPlayerRecord, InputAction action, bool extrapolating)
	{
		if (extrapolating && action.Type != InputActionType.Move)
		{
			return;
		}
		switch (action.Type)
		{
		case InputActionType.Move:
			if (predictedPlayerRecord.IsPlayerMoveableAndPredicted())
			{
				predictedPlayerRecord.PlayerCharacter.OnProcessMoveAction(predictedPlayerRecord, action.Dir);
			}
			else if (predictedPlayerRecord.PlayerCharacter != null && predictedPlayerRecord.PlayerCharacter.IsAuthoritative() && predictedPlayerRecord.PlayerCharacter.CanReceiveUserInputs() && predictedPlayerRecord.PlayerCharacter.InsideBuilding is EnterableVehicle enterableVehicle3 && enterableVehicle3.GetPredicted() != null && enterableVehicle3.Predicted.IsMoving && enterableVehicle3.GetDriverAuthoritative() == predictedPlayerRecord.PlayerCharacter.GetAuthoritativeOrElseThisCharacter())
			{
				enterableVehicle3.Predicted.OnProcessMoveAction(action.Dir);
			}
			break;
		case InputActionType.EnableSprint:
		case InputActionType.EnableSprintAutomatic:
			if (predictedPlayerRecord.IsPlayerControllableAndPredicted())
			{
				predictedPlayerRecord.PlayerCharacter.OnProcessSprintAction(enable: true, action.Type == InputActionType.EnableSprintAutomatic);
			}
			break;
		case InputActionType.DisableSprint:
			if (predictedPlayerRecord.IsPlayerControllableAndPredicted())
			{
				predictedPlayerRecord.PlayerCharacter.OnProcessSprintAction(enable: false, automatic: false);
			}
			break;
		case InputActionType.Kick:
			if (predictedPlayerRecord.IsPlayerControllableAndPredicted())
			{
				predictedPlayerRecord.PlayerCharacter.OnProcessKickAction(predictedPlayerRecord);
			}
			break;
		case InputActionType.Parry:
		{
			Character attacker = action.GetObject(Session.Instance) as Character;
			if (predictedPlayerRecord.IsPlayerControllableAndPredicted())
			{
				predictedPlayerRecord.PlayerCharacter.OnProcessParryAction(predictedPlayerRecord, attacker, action.ActionAnim);
			}
			break;
		}
		case InputActionType.Escape:
		case InputActionType.EscapeHeld:
			if (predictedPlayerRecord.IsPlayerControllableAndPredicted())
			{
				predictedPlayerRecord.PlayerCharacter.OnProcessEscapeAction(predictedPlayerRecord, action.Type == InputActionType.EscapeHeld, action.IntAmount);
			}
			break;
		case InputActionType.Choke:
		case InputActionType.ChokeHeld:
			if (predictedPlayerRecord.IsPlayerControllableAndPredicted())
			{
				predictedPlayerRecord.PlayerCharacter.OnProcessChokeAction(predictedPlayerRecord, action.Type == InputActionType.ChokeHeld, action.IntAmount);
			}
			break;
		case InputActionType.Fire:
			if (predictedPlayerRecord.IsPlayerControllableAndPredicted())
			{
				predictedPlayerRecord.PlayerCharacter.OnProcessFireAction(predictedPlayerRecord);
			}
			break;
		case InputActionType.Reload:
			if (predictedPlayerRecord.IsPlayerControllableAndPredicted())
			{
				predictedPlayerRecord.PlayerCharacter.OnProcessReloadAction();
			}
			break;
		case InputActionType.SetDesiredWeapon:
			if (predictedPlayerRecord.IsPlayerControllableAndPredicted())
			{
				predictedPlayerRecord.PlayerCharacter.OnProcessSetDesiredWeaponAction(predictedPlayerRecord, action.GetObject(Session.Instance) as Equipment, action.Prototype, action.InfectionType);
			}
			break;
		case InputActionType.Crouch:
			if (predictedPlayerRecord.IsPlayerControllableAndPredicted())
			{
				predictedPlayerRecord.PlayerCharacter.OnProcessCrouchAction();
			}
			break;
		case InputActionType.Drop:
			if (predictedPlayerRecord.IsPlayerControllableAndPredicted())
			{
				predictedPlayerRecord.PlayerCharacter.OnProcessDropAction();
			}
			break;
		case InputActionType.SetDirectControlled:
			if (predictedPlayerRecord.IsPlayerControllableAndPredicted())
			{
				predictedPlayerRecord.PlayerCharacter.DirectControlled = true;
			}
			break;
		case InputActionType.SetTargetObject:
			if (predictedPlayerRecord.IsPlayerControllableAndPredicted())
			{
				predictedPlayerRecord.SetTargetObject(action.GetObject(Session.Instance) as TileObject, action.TargettableBodyLocation);
			}
			break;
		case InputActionType.SetTargetPos:
			if (predictedPlayerRecord.IsPlayerControllableAndPredicted())
			{
				predictedPlayerRecord.TargetPos = action.HalfPos.ToVector3();
			}
			break;
		case InputActionType.SetThrowAngle:
			if (predictedPlayerRecord.IsPlayerControllableAndPredicted())
			{
				predictedPlayerRecord.ThrowAngle = action.Angle;
				predictedPlayerRecord.ThrowSpeed = action.FloatAmount;
			}
			break;
		case InputActionType.SetWantLockOnTarget:
			predictedPlayerRecord.WantLockOnTarget = true;
			break;
		case InputActionType.ClearWantLockOnTarget:
			predictedPlayerRecord.WantLockOnTarget = false;
			break;
		case InputActionType.SetTargetBodyLocationToAimFor:
			if (predictedPlayerRecord.IsPlayerControllableAndPredicted())
			{
				predictedPlayerRecord.SetTargetBodyLocationToAimFor(action.TargettableBodyLocation);
			}
			break;
		case InputActionType.RagdollMove:
			if (FindPredictedObjectById(action.ObjectId) is Character character)
			{
				character.OnRagdollMove(action.PosXZ, action.Angle);
			}
			break;
		case InputActionType.RagdollStopMoving:
			if (FindPredictedObjectById(action.ObjectId) is Character character2)
			{
				character2.OnRagdollStopMoving(action.PosXZ, action.Value, action.Angle);
			}
			break;
		case InputActionType.RigidBodyMove:
			FindPredictedObjectById(action.ObjectId)?.OnRigidBodyMove(action.Pos, action.Rot, 0f);
			break;
		case InputActionType.VehicleMove:
			FindPredictedObjectById(action.ObjectId)?.OnRigidBodyMove(action.Pos, action.Rot, action.FloatAmount);
			break;
		case InputActionType.RigidBodyStopMoving:
			FindPredictedObjectById(action.ObjectId)?.OnRigidBodyStopMoving(action.Pos, action.Rot);
			break;
		case InputActionType.VehicleCollision:
		{
			EnterableVehicle enterableVehicle4 = action.GetFrom(Session.Instance) as EnterableVehicle;
			BaseObject to = action.GetTo(Session.Instance);
			if (enterableVehicle4 != null && to != null)
			{
				TileObject tileObject = to as TileObject;
				if (enterableVehicle4 != null && enterableVehicle4.IsSusceptibleToVehicleCollisions(juggernaut: false) && enterableVehicle4.IsPredicted())
				{
					enterableVehicle4.GetPredicted().OnVehicleCollision(to, action.RelativeVelocity, action.Pos, predicted: true);
				}
				if (tileObject != null && tileObject.IsSusceptibleToVehicleCollisions(enterableVehicle4.IsJuggernaut()) && tileObject.IsPredicted())
				{
					tileObject.GetPredicted().OnVehicleCollision(enterableVehicle4, -action.RelativeVelocity, action.Pos, predicted: true);
				}
			}
			break;
		}
		case InputActionType.Horn:
			if (predictedPlayerRecord.IsPlayerControllable() && predictedPlayerRecord.PlayerCharacter.InsideBuilding is EnterableVehicle { IsDriveable: not false } enterableVehicle2 && enterableVehicle2.GetPredicted() != null)
			{
				enterableVehicle2.Predicted.OnHorn();
			}
			break;
		case InputActionType.HandBrake:
			if (predictedPlayerRecord.IsPlayerControllable() && predictedPlayerRecord.PlayerCharacter.InsideBuilding is EnterableVehicle { IsDriveable: not false } enterableVehicle && enterableVehicle.GetPredicted() != null)
			{
				enterableVehicle.Predicted.OnHandBrake();
			}
			break;
		}
	}

	public void NonDeterministicUpdatePredictedObjects()
	{
		HasPredictedAnyFramesSinceLastNonDeterministicUpdate = false;
		foreach (TileObject predictedObject in PredictedObjects)
		{
			predictedObject.UnityUpdate();
		}
	}
}
