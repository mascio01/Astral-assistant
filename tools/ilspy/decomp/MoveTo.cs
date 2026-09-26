using System;
using UnityEngine;

public class MoveTo : StateMachineGoal
{
	public TerrainCoord DestTile;

	protected TerrainCoord _requestedDestTile;

	protected int RequestedExitIndex;

	public MovementType MovementType;

	public AStarPriority AStarPriority = AStarPriority.Unknown;

	public bool _dontOpenOurGates;

	public bool CanUnlockGatesFromInsideWithKey;

	public bool AvoidHostileBases;

	public int DontAvoidCommunityId;

	public bool RespectMovementZone = true;

	public bool MoveToCentreOfTile;

	protected AStarRequester _requester;

	protected bool _requesting;

	protected bool _requested;

	protected bool _allowedMultipleAttempts;

	public PropPrototype MustBeWithinPlantingZone;

	protected int _failedAttempts;

	protected Vector2 _stuckPosXZ;

	protected int _stuckUpdate;

	protected Gate UnlockedGate;

	public bool Success;

	private static int StuckGiveUpFrames = 30;

	public MoveTo()
	{
	}

	public MoveTo(MovementType movementType)
	{
		MovementType = movementType;
	}

	public MoveTo(MovementType movementType, TerrainCoord destTile)
	{
		MovementType = movementType;
		DestTile = GameTerrain.Instance.ClampTileWithinBounds(destTile);
	}

	public MoveTo(MovementType movementType, TerrainCoord destTile, bool dontOpenOurGates)
	{
		MovementType = movementType;
		DestTile = GameTerrain.Instance.ClampTileWithinBounds(destTile);
		_dontOpenOurGates = dontOpenOurGates;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref DestTile);
		reflector.Add(ref MovementType);
		reflector.Add(ref AStarPriority);
		reflector.Add(ref Success);
		reflector.Add(ref _dontOpenOurGates);
		reflector.AddAfter(ref CanUnlockGatesFromInsideWithKey, 549);
		reflector.AddAfter(ref AvoidHostileBases, 339);
		reflector.AddAfter(ref DontAvoidCommunityId, 411);
		reflector.AddAfter(ref RespectMovementZone, 207);
		reflector.Add(ref MoveToCentreOfTile);
		reflector.AddAfter(ref _requesting, 511);
		reflector.Add(ref _requested);
		reflector.Add(ref _requestedDestTile);
		reflector.AddAfter(ref RequestedExitIndex, 138);
		reflector.Add(ref _stuckPosXZ);
		reflector.AddAfter(ref UnlockedGate, 547);
		if (reflector.Version >= 489)
		{
			reflector.Add(ref _stuckUpdate);
		}
		else
		{
			TimeSpan value = TimeSpan.Zero;
			reflector.Add(ref value);
		}
		reflector.Add(ref _allowedMultipleAttempts);
		if (reflector.Version < 218)
		{
			BaseObjectType value2 = BaseObjectType.Invalid;
			reflector.AddAfter(ref value2, 133);
			if (value2 != BaseObjectType.Invalid)
			{
				MustBeWithinPlantingZone = GameImpl.Instance.FindPropPrototypeByName(value2.ToString());
			}
		}
		else
		{
			reflector.Add(ref MustBeWithinPlantingZone);
		}
		reflector.Add(ref _failedAttempts);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveTo;
	}

	public TerrainCoord GetStartTile(Character character)
	{
		RequestedExitIndex = 0;
		if (character.InsideBuilding != null)
		{
			RequestedExitIndex = GetBuildingExitIndex(character);
			return character.GetBuildingExitPos(RequestedExitIndex);
		}
		return character.Tile;
	}

	public virtual void StartAStarRequest(Character character, Goal parent)
	{
		_requester.StartRequest(GetStartTile(character), DestTile, character, null, AStarPriority, _dontOpenOurGates, RespectMovementZone, AvoidHostileBases, DontAvoidCommunityId);
	}

	public virtual bool ShouldReRequestPath(Character character, Goal parent)
	{
		return DestTile != _requestedDestTile;
	}

	public virtual MoveToResult HasReachedDestination(Character character, Goal parent)
	{
		if (!(character.Tile == DestTile))
		{
			return MoveToResult.Working;
		}
		return MoveToResult.Success;
	}

	public virtual void UpdateDestination(Character character, Goal parent)
	{
		TerrainCoord destTile = DestTile;
		GameTerrain instance = GameTerrain.Instance;
		for (int i = 0; i < 10; i++)
		{
			if (!instance.IsImpassable(destTile.x, destTile.y, 0x801 | (_dontOpenOurGates ? 4 : 0), character, null) && (MustBeWithinPlantingZone == null || (Session.Instance.CropsManager.IsInPatchOfType(destTile, character.GetCommunityId(), MustBeWithinPlantingZone) && GameCursor.CanPlantHere(character, destTile, checkCharacters: false) == CursorActionDisabledReason.Enabled)))
			{
				DestTile = destTile;
				break;
			}
			while (true)
			{
				TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
				destTile.x += MathUtil.RandomInt((int)currentTime.Ticks, -1, 2);
				destTile.y += MathUtil.RandomInt((int)currentTime.Ticks, -1, 2);
				if (!instance.IsTileOutsideBounds(destTile.x, destTile.y) && !instance.IsImpassable(destTile.x, destTile.y, _dontOpenOurGates ? 4 : 0, character, null))
				{
					break;
				}
				destTile = DestTile;
				i++;
				if (i >= 10)
				{
					return;
				}
			}
		}
	}

	public virtual int GetBuildingExitIndex(Character character)
	{
		return character.InsideBuilding.GetClosestEntranceTo(DestTile, character, mustBeUnblocked: true);
	}

	public override bool WantDisableSleep()
	{
		return true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		_stuckPosXZ = character.PosXZ;
		_stuckUpdate = character.NumUpdateFrames;
		_requested = false;
		if (Aiming)
		{
			character.GoalTarget = Target;
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		if (UnlockedGate != null && character.IsAuthoritative())
		{
			UnlockedGate.Lock();
			UnlockedGate = null;
		}
		if (_requester != null)
		{
			_requester.CancelRequest();
			_requester = null;
			_requesting = false;
		}
		character.HaltMovement();
		character.GoalTarget = null;
		base.OnDeactivate(character, parent);
	}

	public override void OnEncounterGate(Character character, Goal parent, Gate gate)
	{
		if (gate.CanOpenMeReason(character, fromAI: true, _dontOpenOurGates, CanUnlockGatesFromInsideWithKey) == CursorActionDisabledReason.Enabled)
		{
			if (SubGoal == null)
			{
				if (gate.Community == Session.Instance.CommunityManager.PlayerCommunity && character.Community != Session.Instance.CommunityManager.PlayerCommunity && character.CanOpenPlayerGates == CanOpenGates.Unknown && gate.IsInFrontOfGate(character.PosXZ))
				{
					Squad squad = character.GetSquad();
					bool flag = squad?.CanForceOpenPlayerGates() ?? false;
					TimeSpan timeSpan = TimeSpan.FromSeconds(flag ? 15f : 180f);
					if (character.SquadLeader == null && character.IsAuthoritative())
					{
						if (character.HasSpokenSpeechForSituationToAnyoneRecently(SpeechSituation.LetMeIn, timeSpan, out var spokenTime))
						{
							timeSpan -= Session.Instance.PlayTime - spokenTime;
						}
						else
						{
							Character character2 = GetTargetCharacter();
							if (flag && character2 == null)
							{
								character2 = squad.GoalCharacter;
							}
							if (character2 != null && (!character2.AliveAndNotZombie || character2.GetBaseObjectType() != BaseObjectType.Human || character2.Community != gate.Community))
							{
								character2 = null;
							}
							Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, character2, gate, SpeechSituation.LetMeIn);
							if (speechForSituation != null)
							{
								SetSubGoal(character, parent, new Conversation(character, character2, gate, speechForSituation, controlledByPlayer: false));
							}
						}
					}
					if (SubGoal == null)
					{
						SetSubGoal(character, parent, new Wait(timeSpan));
					}
				}
				else
				{
					if (gate.GateState == GateState.Locked && character.IsAuthoritative())
					{
						if (UnlockedGate != null)
						{
							UnlockedGate.Lock();
						}
						UnlockedGate = gate;
					}
					if (character.CarryingObject != null)
					{
						if (character.IsAuthoritative())
						{
							gate.Open(character);
						}
						return;
					}
					SetSubGoal(character, parent, new OpenGateGoal(character, gate));
				}
			}
		}
		else if (_requester == null)
		{
			UpdateDestination(character, parent);
			if (character.IsAuthoritative())
			{
				RequestPath(character, parent);
			}
		}
		character.SetMovementType(MovementType.None);
	}

	public override void OnEncounterWaistHighWall(Character character, Goal parent, TileObject prop)
	{
		if (character is Human && character.CarryingObject == null && (!(prop is Gate gate) || !gate.CanOpenMe(character, fromAI: true, _dontOpenOurGates)))
		{
			SetSubGoal(character, parent, new VaultWaistHighWall(character, prop));
		}
	}

	public override void OnCollisionWithCharacter(Character character, Goal parent, Character other)
	{
		if (other == GetTargetCharacter() || SubGoal != null || character.WantAvoidance)
		{
			return;
		}
		character.WantAvoidance = true;
		_allowedMultipleAttempts = true;
		if (!other.IsFinishedRoute() && character.GetRouteNextNode() == other.GetRouteNextNode())
		{
			Vector2 tileCentreXZ = GameTerrain.Instance.GetTileCentreXZ(character.GetRouteNextNode());
			float sqrMagnitude = (tileCentreXZ - other.PosXZ).sqrMagnitude;
			float sqrMagnitude2 = (tileCentreXZ - character.PosXZ).sqrMagnitude;
			if (sqrMagnitude < sqrMagnitude2 || (sqrMagnitude == sqrMagnitude2 && other.Id < character.Id))
			{
				character.SetMovementType(MovementType.None);
				SetSubGoal(character, parent, new Wait(TimeSpan.FromSeconds((MovementType == MovementType.Walk) ? 2f : 0.5f)));
			}
			else if (character.GetRouteCount() >= 2 && character.GetRouteNextNode().IsWithinBounds(character.Tile - new TerrainCoord(1, 1), character.Tile + new TerrainCoord(1, 1)))
			{
				character.PopNextRouteNode();
			}
		}
		else if (other.IsFinishedRoute() && other.Tile == character.GetRouteNextNode() && character.GetRouteCount() >= 2 && character.GetRouteNextNode().IsWithinBounds(character.Tile - new TerrainCoord(1, 1), character.Tile + new TerrainCoord(1, 1)))
		{
			character.PopNextRouteNode();
		}
	}

	protected override bool FinishOnNullSubGoal()
	{
		return false;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is Wait)
		{
			if (character.IsAuthoritative() && character.Community != null && character.Community.CanOpenPlayerGates == CanOpenGates.Unknown)
			{
				character.Community.CanOpenPlayerGates = CanOpenGates.No;
			}
			return null;
		}
		_stuckPosXZ = character.PosXZ;
		_stuckUpdate = character.NumUpdateFrames;
		if (_requested && _requester == null)
		{
			character.SetMovementType(MovementType);
		}
		return base.GetNextSubGoal(character, parent);
	}

	private void RequestPath(Character character, Goal parent)
	{
		if (_requester != null)
		{
			_requester.CancelRequest();
			_requester = null;
			_requesting = false;
		}
		_requester = new AStarRequester();
		StartAStarRequest(character, parent);
		_requestedDestTile = DestTile;
		_requested = true;
		_requesting = true;
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (Finished)
		{
			return;
		}
		if (SubGoal != null)
		{
			_stuckPosXZ = character.PosXZ;
			_stuckUpdate = character.NumUpdateFrames;
			if (SubGoal is VaultWaistHighWall && character.IsFinishedRoute() && _requested && !_requesting && parent is MoveToAndEnterBuilding && HasReachedDestination(character, parent) == MoveToResult.Success)
			{
				Success = true;
				Finished = true;
			}
			return;
		}
		UpdateDestination(character, parent);
		if (character.IsAuthoritative() && (!_requested || (_requesting && _requester == null) || ShouldReRequestPath(character, parent)))
		{
			RequestPath(character, parent);
		}
		if (_requester != null)
		{
			_stuckPosXZ = character.PosXZ;
			_stuckUpdate = character.NumUpdateFrames;
			switch (_requester.Result)
			{
			case AStarResult.Success:
				if (character.InsideBuilding != null)
				{
					character.RemoveFailedExitBuildingAttempt(character.InsideBuilding, RequestedExitIndex);
					SetSubGoal(character, parent, new LeaveBuilding(RequestedExitIndex));
				}
				else if (character.IsSitting())
				{
					SetSubGoal(character, parent, new StopSittingGoal());
				}
				else if (character.CrouchingTransition > 0f && !character.IsCrouching())
				{
					SetSubGoal(character, parent, new UncrouchAnim());
				}
				else if (character.CrouchingTransition < 1f && character.IsCrouching())
				{
					SetSubGoal(character, parent, new CrouchAnim());
				}
				if (_requester.Route.Count > 0)
				{
					character.MoveTo(MovementType, _requester.Route);
				}
				else if (MoveToCentreOfTile)
				{
					character.MoveToCurrentTile(MovementType);
				}
				else if (HasReachedDestination(character, parent) == MoveToResult.Fail)
				{
					character.MoveToCurrentTile(MovementType);
				}
				RequestedExitIndex = 0;
				_requester = null;
				_requesting = false;
				break;
			case AStarResult.Fail:
				if (character.InsideBuilding != null)
				{
					character.AddFailedExitBuildingAttempt(character.InsideBuilding, RequestedExitIndex);
				}
				RequestedExitIndex = 0;
				_requester = null;
				_requesting = false;
				Success = false;
				Finished = true;
				break;
			}
		}
		if (_requested && _requester == null && !Finished)
		{
			if (character.IsFinishedRoute())
			{
				MoveToResult moveToResult = HasReachedDestination(character, parent);
				if (moveToResult == MoveToResult.Working)
				{
					if (character.IsAuthoritative())
					{
						RequestPath(character, parent);
					}
				}
				else
				{
					Success = moveToResult == MoveToResult.Success;
					Finished = true;
				}
			}
			else
			{
				float num = 4f * character.PickAIMovementSpeed(MovementType, character.Tired) * (1f / 60f);
				Vector2 posXZ = character.PosXZ;
				if ((posXZ - _stuckPosXZ).sqrMagnitude >= num * num || character.GetCurrentActionPriority() > ActionPriority.None || character.IsRagdollOrProneOrRecovering())
				{
					_stuckPosXZ = posXZ;
					_stuckUpdate = character.NumUpdateFrames;
				}
				if (character.NumUpdateFrames - _stuckUpdate >= StuckGiveUpFrames)
				{
					if (_allowedMultipleAttempts && _failedAttempts < 3)
					{
						if (character.IsAuthoritative())
						{
							_allowedMultipleAttempts = false;
							_failedAttempts++;
							character.SetMovementType(MovementType.None);
							UpdateDestination(character, parent);
							RequestPath(character, parent);
						}
					}
					else
					{
						Success = false;
						Finished = true;
					}
				}
			}
		}
		if (Aiming && SubGoal == null)
		{
			character.GoalTarget = Target;
		}
	}

	public override void SetMovementType(Character character, MovementType movementType)
	{
		MovementType = movementType;
		if (Active && _requested && _requester == null && SubGoal == null)
		{
			character.SetMovementType(MovementType);
		}
	}

	public override MovementType GetMovementType()
	{
		return MovementType;
	}
}
