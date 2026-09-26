using System;
using System.Collections.Generic;
using UnityEngine;

public class EnterableVehicle : TiltedBuilding
{
	private static PrefabResource[] Models = new PrefabResource[1]
	{
		new PrefabResource("Prefabs\\Vehicles\\DumpTruck")
	};

	public bool IsDriveable;

	public bool HasEverMoved;

	public bool IsMoving;

	public PlayerID PhysicsOwnerID;

	public float GasPedal;

	public float BrakePedal;

	public bool HandBrake;

	public float SteeringAngle;

	public GearState GearState;

	public int Gear;

	public float GearStateTimer;

	public float Clutch;

	public float CurrentRPM;

	public Vector3 CentreOfMass;

	public Quaternion Rot;

	public float WheelRPM;

	public float Speed;

	public Vector3 Velocity;

	public EnterableVehicle Predicted;

	public EnterableVehicle Authoritative;

	public int PredictedFreshFrame = -1;

	public byte[] PredictedActionCounter = new byte[34];

	public List<PredictedEvent> PredictedEvents;

	public bool SentRigidBodyStopMoving;

	public static float DrivingHideMenuSpeedMph = 10f;

	public static float DefaultGasFracMin = 0.05f;

	public static float DefaultGasFracMax = 0.1f;

	private static float LastPlayedCarStartFailSound = -1000f;

	private static float GearShiftSoundVolume = 0.15f;

	private static float ChangeDirSpeedThreshold = 5f;

	private static float GasUsageRateFactor = 0.5f;

	private static List<Character> TempCharacters = new List<Character>();

	private static float InterruptDodgeAnimFrac = 0.4f;

	private static float ExtraDodgeDist = 0.5f;

	private static float LookAheadTime = 2f;

	private static float GateLookAheadDist = 8f;

	private static float RPMLerpFactor = 3f;

	private static float GearIncreaseFactor = 0.6f;

	private static float GearDecreaseFactor = 0.2f;

	private static float[] GearChangeUpMinSpeed = new float[5] { 0f, 10f, 20f, 35f, 55f };

	private static float[] GearChangeDownMaxSpeed = new float[6] { 0f, 0f, 5f, 15f, 25f, 40f };

	private static float PhysicsStopMovingTolerance = 0.015f;

	private static float AngleFix = 0f;

	public static float HornRange = 32f;

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.EnterableVehicle;
	}

	public float GetMass()
	{
		if (Prototype == null)
		{
			return 0f;
		}
		return Prototype.Mass;
	}

	public bool IsJuggernaut()
	{
		if (Prototype != null)
		{
			return Prototype.Mass >= 100000f;
		}
		return false;
	}

	public float GetSpeedInKmh()
	{
		return Speed * 60f * 60f / 1000f;
	}

	public float GetSpeedInMph()
	{
		return Speed * 60f * 60f / 1609.34f;
	}

	public bool IsDrivingTooFastToShowMenu()
	{
		return Mathf.Abs(GetSpeedInMph()) >= DrivingHideMenuSpeedMph;
	}

	public override ImpactSusceptibility GetImpactSusceptibility()
	{
		if ((!IsDriveable || Destroyed || LiquidAmount <= 0f) && !IsMoving)
		{
			return ImpactSusceptibility.Invulnerable;
		}
		return base.GetImpactSusceptibility();
	}

	public override Vector3 GetUnityPos()
	{
		if (Predicted != null)
		{
			return Predicted.GetUnityPos();
		}
		return CentreOfMass;
	}

	public override float GetUnityOverheadIconYOffset()
	{
		return GetUnityModel()?.Height ?? base.GetUnityOverheadIconYOffset();
	}

	public override CursorActionDisabledReason CanBeFilled()
	{
		if (!MossCleanedAway)
		{
			return CursorActionDisabledReason.NeedToRepair;
		}
		return CursorActionDisabledReason.Enabled;
	}

	public override bool IsDriveableVehicle()
	{
		return IsDriveable;
	}

	public override bool IsMovingVehicle()
	{
		return IsMoving;
	}

	public override bool IsUnityObjectStatic()
	{
		return !IsMoving;
	}

	public override bool WantPrediction()
	{
		return IsMoving;
	}

	public override bool IsPredicted()
	{
		return PredictedFreshFrame != -1;
	}

	public override bool IsAuthoritative()
	{
		return PredictedFreshFrame == -1;
	}

	public override int GetPredictedFreshFrame()
	{
		return PredictedFreshFrame;
	}

	public override TileObject GetPredicted()
	{
		return Predicted;
	}

	public override TileObject GetAuthoritative()
	{
		return Authoritative;
	}

	public override void SetPredictedFreshFrame(int frame)
	{
		PredictedFreshFrame = frame;
	}

	public override void SetPredicted(TileObject obj)
	{
		Predicted = (EnterableVehicle)obj;
	}

	public override void SetAuthoritative(TileObject obj)
	{
		Authoritative = (EnterableVehicle)obj;
	}

	public EnterableVehicle GetAuthoritativeOrElseThisVehicle()
	{
		if (!IsAuthoritative())
		{
			return Authoritative;
		}
		return this;
	}

	public EnterableVehicle GetPredictedOrElseThisVehicle()
	{
		if (Predicted == null)
		{
			return this;
		}
		return Predicted;
	}

	public override byte[] GetPredictedActionCounter()
	{
		return PredictedActionCounter;
	}

	public override List<PredictedEvent> GetPredictedEvents()
	{
		if (PredictedEvents == null)
		{
			PredictedEvents = new List<PredictedEvent>();
		}
		return PredictedEvents;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.AddAfter(ref IsDriveable, 464);
		reflector.AddAfter(ref HasEverMoved, 461);
		reflector.AddAfter(ref IsMoving, 461);
		reflector.AddAfter(ref PhysicsOwnerID, 471);
		reflector.AddAfter(ref CentreOfMass, 461);
		reflector.AddAfter(ref Rot, 461);
		reflector.AddAfter(ref GasPedal, 461);
		reflector.AddAfter(ref BrakePedal, 463);
		reflector.AddAfter(ref HandBrake, 565);
		reflector.AddAfter(ref SteeringAngle, 461);
		reflector.AddAfter(ref Speed, 463);
		reflector.AddAfter(ref Velocity, 466);
		reflector.AddAfter(ref CurrentRPM, 463);
		reflector.AddAfter(ref WheelRPM, 463);
		reflector.AddAfter(ref Gear, 463);
		reflector.AddAfter(ref GearState, 463);
		reflector.AddAfter(ref GearStateTimer, 463);
		reflector.AddAfter(ref Clutch, 463);
		float value = 0f;
		reflector.AddAfter(ref value, 467);
		if (reflector.IsDoingPrediction)
		{
			reflector.AddByteArray(ref PredictedActionCounter);
		}
	}

	public override void OnSpawn()
	{
		base.OnSpawn();
		LiquidAmount = GetLiquidCapacity() * Mathf.Lerp(DefaultGasFracMin, DefaultGasFracMax, MathUtil.RandomFloat((float)Id + 39457f));
	}

	public override void Init()
	{
		base.Init();
		if (!HasEverMoved)
		{
			UpdateNeverMovedPosAndRot();
		}
		if (IsMoving)
		{
			Session.Instance.PropManager.MovingVehicles.Add(this);
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
			AddToActiveMovingUnityObjects();
		}
	}

	public override void Delete()
	{
		if (IsMoving)
		{
			RemoveFromActiveMovingUnityObjects();
			Session.Instance.PropManager.RemoveFromObjectsThatNeedUpdating(this);
			Session.Instance.PropManager.MovingVehicles.Remove(this);
		}
		base.Delete();
	}

	public void OnProcessMoveAction(Vector2 moveDir)
	{
		if (IsDriveable && LiquidAmount > 0f && !Destroyed)
		{
			SteeringAngle = moveDir.x;
			GasPedal = Math.Max(moveDir.y, 0f);
			BrakePedal = Math.Max(0f - moveDir.y, 0f);
			StartMoving();
		}
		else if (MossCleanedAway && Time.time - LastPlayedCarStartFailSound >= SoundManager.StartFailSound.GetAsset().length)
		{
			SoundManager.PlaySound3D(SoundManager.StartFailSound, CentreOfMass, VehicleBehaviour.EngineVolume);
			LastPlayedCarStartFailSound = Time.time;
		}
	}

	public void SetGearState(GearState gearState)
	{
		if (GearState > GearState.StartEngine)
		{
			if ((gearState == GearState.ChangingDown || (gearState == GearState.Neutral && Gear > 0 && !IsReversing())) && CheckFrontmostPrediction(PredictedEventType.GearShiftSound))
			{
				PlaySoundOneShotFromList(SoundManager.GearShiftDownSounds, GearShiftSoundVolume);
			}
			if ((gearState == GearState.ChangingUp || (gearState == GearState.Neutral && IsReversing())) && CheckFrontmostPrediction(PredictedEventType.GearShiftSound))
			{
				PlaySoundOneShotFromList(SoundManager.GearShiftUpSounds, GearShiftSoundVolume);
			}
		}
		GearState = gearState;
		GearStateTimer = 0f;
	}

	public void StartMoving()
	{
		if (!IsMoving)
		{
			if (!HasEverMoved)
			{
				UpdateNeverMovedPosAndRot();
			}
			IsMoving = true;
			HasEverMoved = true;
			Character driverAuthoritative = GetDriverAuthoritative();
			PlayerRecord playerRecord = Session.Instance.GetPlayerControllingCharacter(driverAuthoritative);
			if (playerRecord == null)
			{
				playerRecord = Session.Instance.GetPartyLeaderRecord();
			}
			PhysicsOwnerID = playerRecord.PlayerID;
			Session.Instance.PropManager.MovingVehicles.Add(this);
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
			AddToActiveMovingUnityObjects();
			UnityStartMoving();
			if (HasUnityObject() && !IsUnityObjectActive())
			{
				UnityActivate();
			}
			if (Predicted != null)
			{
				Predicted.IsMoving = true;
				Predicted.UnityStartMoving();
			}
		}
	}

	public void UnityStartMoving()
	{
		if (UnityObj != null)
		{
			PrefabResource unityModel = GetUnityModel();
			unityModel?.CacheWheelRotations();
			if (IsJuggernaut())
			{
				Prop.SetLayerRecursively(UnityObj, Character.JuggernautLayer);
			}
			Rigidbody rigidbody = UnityObj.GetComponent<Rigidbody>();
			if (rigidbody == null)
			{
				rigidbody = UnityObj.AddComponent<Rigidbody>();
			}
			rigidbody.mass = Prototype.Mass;
			rigidbody.linearDamping = 0.015f;
			rigidbody.angularDamping = 0.05f;
			rigidbody.isKinematic = false;
			if (IsJuggernaut())
			{
				rigidbody.maxDepenetrationVelocity = 1f;
				rigidbody.maxLinearVelocity = 25f;
			}
			VehicleBehaviour vehicleBehaviour = UnityObj.GetComponent<VehicleBehaviour>();
			if (vehicleBehaviour == null)
			{
				vehicleBehaviour = UnityObj.AddComponent<VehicleBehaviour>();
			}
			vehicleBehaviour.Init(this, unityModel);
		}
	}

	public override void UnityInit()
	{
		base.UnityInit();
		if (IsMoving)
		{
			UnityStartMoving();
		}
	}

	public bool IsValidPlaceToPark(TerrainCoord tile)
	{
		GameTerrain instance = GameTerrain.Instance;
		CalcMinMaxTile(tile, Orientation, out var minTile, out var maxTile);
		if (!instance.IsTileRectWithinBounds(minTile, maxTile))
		{
			return false;
		}
		for (int i = minTile.x; i <= maxTile.x; i++)
		{
			for (int j = minTile.y; j <= maxTile.y; j++)
			{
				if (instance.IsImpassable(i, j, 0, null, this) || instance.IsTileRiver(i, j))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void Park(CustomRandom rand)
	{
		RestoreNeverMoved();
		_ = Tile;
		for (int i = 0; i < GameTerrain.Instance.Size; i++)
		{
			TerrainCoord tile = Tile + rand.RandomTile(new TerrainCoord(-i, -i), new TerrainCoord(i, i));
			if (IsValidPlaceToPark(tile))
			{
				SetTile(tile);
				break;
			}
		}
	}

	public void RestoreNeverMoved()
	{
		if (HasEverMoved)
		{
			HasEverMoved = false;
			UpdateNeverMovedPosAndRot();
			UpdateWorldTransformAndBounds();
			UnityReinit();
		}
	}

	public override void SetTile(TerrainCoord tile)
	{
		base.SetTile(tile);
		RestoreNeverMoved();
	}

	public override void SetOrientationType(OrientationType orientation)
	{
		base.SetOrientationType(orientation);
		RestoreNeverMoved();
	}

	private void UpdateNeverMovedPosAndRot()
	{
		Matrix4x4 customModelTransform = GetCustomModelTransform();
		CentreOfMass = customModelTransform.Translation();
		Rot = customModelTransform.rotation;
		Velocity = Vector3.zero;
		Speed = 0f;
		WheelRPM = 0f;
	}

	public override void InitPredicted()
	{
		if (Authoritative != null && Authoritative.UnityObj != null)
		{
			UnityObj = Authoritative.UnityObj;
			Authoritative.UnityObj = null;
			int num = Session.Instance.ActiveMovingUnityObjects.IndexOf(Authoritative);
			if (num != -1)
			{
				Session.Instance.ActiveMovingUnityObjects[num] = this;
			}
		}
	}

	public override void DeletePredicted()
	{
		if (Authoritative == null || Authoritative.PropWantDelete())
		{
			UnityDelete();
			return;
		}
		Authoritative.UnityObj = UnityObj;
		UnityObj = null;
		int num = Session.Instance.ActiveMovingUnityObjects.IndexOf(this);
		if (num != -1)
		{
			Session.Instance.ActiveMovingUnityObjects[num] = Authoritative;
		}
	}

	public override void PredictedFixup()
	{
		base.PredictedFixup();
		for (int i = 0; i < Inhabitants.Length; i++)
		{
			if (Inhabitants[i] != null)
			{
				Inhabitants[i] = Inhabitants[i].GetPredictedOrElseThisCharacter();
			}
		}
	}

	public override void PredictedUpdate(TimeSpan dt)
	{
		VehicleUpdate(dt);
	}

	public override void PropUpdate(TimeSpan dt, ref bool stillNeedUpdating)
	{
		base.PropUpdate(dt, ref stillNeedUpdating);
		if (IsMoving)
		{
			VehicleUpdate(dt);
			stillNeedUpdating = true;
		}
	}

	private bool IsReversing()
	{
		return Gear == Prototype.VehicleGearRatios.Length - 1;
	}

	private void VehicleUpdate(TimeSpan dt)
	{
		float num = (float)dt.TotalSeconds;
		GearStateTimer += num;
		if (Destroyed || !IsDriveable || LiquidAmount <= 0f)
		{
			if (GearState != GearState.Off)
			{
				SetGearState(GearState.Off);
			}
		}
		else
		{
			switch (GearState)
			{
			case GearState.Off:
				if (IsMoving)
				{
					SetGearState(GearState.StartEngine);
				}
				break;
			case GearState.StartEngine:
				if (GearStateTimer >= 0.5f)
				{
					SetGearState(GearState.Neutral);
				}
				break;
			case GearState.Neutral:
				Clutch = 0f;
				Gear = 0;
				if (GasPedal > 0f && (WheelRPM > -50f || Speed > 0f - ChangeDirSpeedThreshold))
				{
					SetGearState(GearState.ChangingUp);
				}
				else if (BrakePedal > 0f && (WheelRPM < 50f || Speed < ChangeDirSpeedThreshold))
				{
					SetGearState(GearState.ChangingDown);
				}
				break;
			case GearState.Running:
			case GearState.CheckingChangeUp:
			case GearState.CheckingChangeDown:
				Clutch = Mathf.Lerp(Clutch, 1f, num);
				switch (GearState)
				{
				case GearState.Running:
					if (Gear < Prototype.VehicleGearRatios.Length - 2 && WantChangeGear(1))
					{
						SetGearState(GearState.CheckingChangeUp);
					}
					else if (Gear > 1 && Gear != Prototype.VehicleGearRatios.Length - 1 && WantChangeGear(-1))
					{
						SetGearState(GearState.CheckingChangeDown);
					}
					else if (CurrentRPM < Prototype.VehicleStartEngineRPM + 200f && Mathf.Abs(Speed) < ChangeDirSpeedThreshold)
					{
						if (IsReversing())
						{
							if (BrakePedal <= 0f)
							{
								SetGearState(GearState.Neutral);
							}
						}
						else if (GasPedal <= 0f && Gear == 1)
						{
							SetGearState(GearState.Neutral);
						}
					}
					else
					{
						if (!(Mathf.Abs(Speed) < ChangeDirSpeedThreshold))
						{
							break;
						}
						bool num2;
						if (!IsReversing())
						{
							if (!(BrakePedal > 0f))
							{
								break;
							}
							num2 = GasPedal <= 0f;
						}
						else
						{
							if (!(GasPedal > 0f))
							{
								break;
							}
							num2 = BrakePedal <= 0f;
						}
						if (num2)
						{
							SetGearState(GearState.Neutral);
						}
					}
					break;
				case GearState.CheckingChangeUp:
					if (!WantChangeGear(1))
					{
						SetGearState(GearState.Running);
					}
					else if (GearStateTimer >= 0.7f)
					{
						SetGearState(GearState.ChangingUp);
					}
					break;
				case GearState.CheckingChangeDown:
					if (!WantChangeGear(-1))
					{
						SetGearState(GearState.Running);
					}
					else if (GearStateTimer >= 0.1f)
					{
						SetGearState(GearState.ChangingDown);
					}
					break;
				}
				break;
			case GearState.ChangingUp:
			case GearState.ChangingDown:
				Mathf.Abs(Speed);
				_ = ChangeDirSpeedThreshold;
				Clutch = 0f;
				if (!(GearStateTimer >= 0.5f))
				{
					break;
				}
				if (GearState == GearState.ChangingDown)
				{
					if (Gear <= 0)
					{
						Gear = Prototype.VehicleGearRatios.Length - 1;
					}
					else
					{
						Gear--;
						while (Gear > 1 && WantChangeGear(-1))
						{
							Gear--;
						}
					}
				}
				else
				{
					Gear++;
				}
				SetGearState((Gear != 0) ? GearState.Running : GearState.Neutral);
				break;
			}
		}
		bool flag = IsReversing();
		float num3 = (flag ? BrakePedal : GasPedal);
		float brake = (flag ? GasPedal : BrakePedal);
		float handbrake = 0f;
		float num4 = 0f;
		if (GearState > GearState.StartEngine)
		{
			if (Clutch < 0.1f)
			{
				if (GearState == GearState.ChangingUp || GearState == GearState.ChangingDown)
				{
					num3 = 0f;
				}
				CurrentRPM = Mathf.Lerp(CurrentRPM, Mathf.Max(Prototype.VehicleStartEngineRPM, Prototype.VehicleMaxRPM * num3), num);
			}
			else
			{
				CurrentRPM = Mathf.Lerp(CurrentRPM, Mathf.Max(Prototype.VehicleStartEngineRPM - 100f, Mathf.Abs(WheelRPM) * Prototype.VehicleGearRatios[Gear] * Prototype.VehicleGearRatios[0]), num * RPMLerpFactor);
				num4 = EvaluatePowerCurve(CurrentRPM / Prototype.VehicleMaxRPM) * Prototype.VehicleHP / CurrentRPM * Prototype.VehicleGearRatios[Gear] * Prototype.VehicleGearRatios[0] * 5252f * Clutch * num3;
			}
		}
		else
		{
			CurrentRPM = 0f;
		}
		Character driver = GetDriver();
		if (driver == null && Mathf.Abs(GetSpeedInMph()) < 20f)
		{
			brake = 1f;
		}
		if (!Session.Instance.IsVisibleDeterministic(MathUtil.ToXZ(CentreOfMass)))
		{
			brake = 1f;
		}
		if (!IsDriveable || LiquidAmount <= 0f || Destroyed)
		{
			brake = 1f;
		}
		if (HandBrake)
		{
			handbrake = 1f;
		}
		if (UnityObj != null)
		{
			VehicleBehaviour component = UnityObj.GetComponent<VehicleBehaviour>();
			if (component != null)
			{
				component.AddTorq(flag ? (0f - num4) : num4);
				component.AddBrakeTorq(brake, handbrake);
				component.WheelSteer(SteeringAngle * Prototype.VehicleMaxTurningAngle);
			}
		}
		ExtractLiquid(Mathf.Clamp01(CurrentRPM / Prototype.VehicleMaxRPM) * (Prototype.VehicleHP / 100f) * GasUsageRateFactor * num);
		GasPedal = 0f;
		BrakePedal = 0f;
		HandBrake = false;
		SteeringAngle = 0f;
		float magnitude = Velocity.magnitude;
		Vector3 pos = CentreOfMass + LookAheadTime * 0.5f * Velocity;
		int num5 = Mathf.CeilToInt(LookAheadTime * 0.5f * magnitude);
		GameTerrain instance = GameTerrain.Instance;
		TerrainCoord tileCoordForPos = instance.GetTileCoordForPos(pos);
		Vector2 closestPointXZ;
		if (IsPredicted())
		{
			foreach (TileObject predictedObject in PredictedObjectManager.Instance.PredictedObjects)
			{
				if (predictedObject is Character character && CanCharacterDodgeVehicle(character, out closestPointXZ))
				{
					character.OnChanceToDodgeVehicle(this, closestPointXZ);
				}
			}
		}
		else
		{
			instance.CharacterMapWho.GetObjectsInRect(tileCoordForPos - new TerrainCoord(num5, num5), tileCoordForPos + new TerrainCoord(num5, num5), TempCharacters);
			foreach (Character tempCharacter in TempCharacters)
			{
				if (CanCharacterDodgeVehicle(tempCharacter, out closestPointXZ))
				{
					tempCharacter.OnChanceToDodgeVehicle(this, closestPointXZ);
				}
			}
			if (magnitude > 0.01f && driver != null && !IsJuggernaut() && Prototype != null)
			{
				Vector3 vector = Velocity / magnitude;
				float num6 = Prototype.ExtentsMax.y - Prototype.ExtentsMin.y;
				if (instance.FindImpassableObject(new Ray(CentreOfMass + vector * num6 * 0.5f, vector), GateLookAheadDist, this) is Gate { Community: not null } gate && gate.Community.HasAnyActiveMembers() && gate.CanOpenMe(driver, fromAI: false, dontOpenOurGates: false))
				{
					if (gate.GateState == GateState.Closed)
					{
						gate.Open(driver);
					}
					gate.DontCloseUntilTime = Session.Instance.PlayTime + TimeSpan.FromSeconds(2.0);
				}
			}
		}
		TempCharacters.Clear();
	}

	private bool CanCharacterDodgeVehicle(Character character, out Vector2 closestPointXZ)
	{
		closestPointXZ = Vector2.zero;
		if (!character.IsAwake)
		{
			return false;
		}
		if (character.InsideBuilding != null)
		{
			return false;
		}
		if (character.IsDodging() && character.GetActionAnimPlayedFrac() < InterruptDodgeAnimFrac)
		{
			return false;
		}
		if (character.DirectControlled)
		{
			return false;
		}
		if (character.GetBaseObjectType() != BaseObjectType.Human)
		{
			return false;
		}
		if (character.Zombie && character.ShouldLimp())
		{
			return false;
		}
		float num = GetUnityModel().IdentityBounds.size.x + ExtraDodgeDist;
		closestPointXZ = MathUtil.GetClosestPointOnSegmentToPoint(MathUtil.ToXZ(CentreOfMass), MathUtil.ToXZ(CentreOfMass + Velocity * LookAheadTime), character.PosXZ, out var t);
		if (t >= 1f)
		{
			return false;
		}
		if ((closestPointXZ - character.PosXZ).sqrMagnitude >= num)
		{
			return false;
		}
		Character driver = GetDriver();
		Community community = ((driver != null) ? driver.GetCommunity() : GetCommunity());
		if (character.Community != null && (character.Community == community || character.Community.CachedAllies.Contains(community)))
		{
			return true;
		}
		Target target = character.GetTarget(this);
		if (target != null && target.FullyTracked && target.Visible)
		{
			return true;
		}
		return false;
	}

	public override float GetPipFocusDist()
	{
		return GetUnityModel()?.IdentityBounds.size.z ?? base.GetPipFocusDist();
	}

	public override float GetPipClipDist()
	{
		return GetUnityModel()?.IdentityBounds.size.z ?? base.GetPipClipDist();
	}

	public override Vector3 GetPipFocusPos()
	{
		PrefabResource unityModel = GetUnityModel();
		if (unityModel != null)
		{
			return CentreOfMass + Vector3.up * unityModel.IdentityBounds.size.y * 0.5f;
		}
		return CentreOfMass;
	}

	public override Matrix4x4 GetPipFocusWorldMatrix()
	{
		return Matrix4x4.TRS(CentreOfMass, Rot, Vector3.one);
	}

	private bool WantChangeGear(int change)
	{
		int num = Mathf.RoundToInt((Prototype.VehicleMaxRPM - Prototype.VehicleStartEngineRPM) * GearIncreaseFactor);
		int num2 = Mathf.RoundToInt((Prototype.VehicleMaxRPM - Prototype.VehicleStartEngineRPM) * GearDecreaseFactor);
		float num3 = CurrentRPM * Prototype.VehicleGearRatios[Gear + change] / Prototype.VehicleGearRatios[Gear];
		float speedInMph = GetSpeedInMph();
		if (change == 1)
		{
			if (CurrentRPM > (float)num && num3 > (float)num2)
			{
				return speedInMph >= GearChangeUpMinSpeed[Math.Min(Gear, GearChangeUpMinSpeed.Length - 1)];
			}
			return false;
		}
		if (CurrentRPM < (float)num2 && num3 < (float)num)
		{
			return speedInMph <= GearChangeDownMaxSpeed[Math.Min(Gear, GearChangeDownMaxSpeed.Length - 1)];
		}
		return false;
	}

	private float EvaluatePowerCurve(float normalizedRpm)
	{
		return Prop.VehicleSettings.NormalGasolinePowerCurve.Evaluate(normalizedRpm);
	}

	private float CalculateCurrentTorq(int Gear, float rpm)
	{
		float num = 0f;
		if (Gear != 0)
		{
			return Prototype.VehicleGearRatios[0] * Prototype.VehicleGearRatios[Gear] * Mathf.SmoothStep(0f, 1000f, rpm / 1000f) * 150f;
		}
		return 0f;
	}

	public override void UnitySendPhysicsStateToClients(InputFrame inputFrame)
	{
		if (!IsMoving || SentRigidBodyStopMoving)
		{
			return;
		}
		PlayerRecord playerRecord = Session.Instance.GetPlayerRecord(PhysicsOwnerID);
		if (playerRecord == null || playerRecord.PlayerMode != PlayerMode.Controlling)
		{
			playerRecord = Session.Instance.GetPartyLeaderRecord();
		}
		if (Session.Instance.GetLocalPlayerRecord().PlayerID != playerRecord.PlayerID)
		{
			return;
		}
		EnterableVehicle predictedOrElseThisVehicle = GetPredictedOrElseThisVehicle();
		GameObject unityObj = predictedOrElseThisVehicle.UnityObj;
		if (!(unityObj != null))
		{
			return;
		}
		GameTerrain instance = GameTerrain.Instance;
		unityObj.transform.position = instance.ClampPosWithinBounds(unityObj.transform.position, 1f);
		float tileHeightAtPos = instance.GetTileHeightAtPos(unityObj.transform.position.x, unityObj.transform.position.z);
		if (unityObj.transform.position.y < tileHeightAtPos - 1f)
		{
			Debug.LogWarning(GetDisplayNameString() + " fell through terrain at: " + unityObj.transform.position.ToString());
			unityObj.transform.position = new Vector3(unityObj.transform.position.x, tileHeightAtPos + 0.01f, unityObj.transform.position.z);
		}
		float num = 0f;
		VehicleBehaviour component = unityObj.GetComponent<VehicleBehaviour>();
		if (component != null)
		{
			num = component.GetWheelRPM();
			for (int i = 0; i < component.CurrentCollisions.Count; i++)
			{
				if (!component.CurrentCollisions[i].Sent)
				{
					VehicleBehaviour.CollisionInfo value = component.CurrentCollisions[i];
					value.Sent = true;
					component.CurrentCollisions[i] = value;
					BaseObject other = BaseObjectManager.Instance.FindBaseObjectByID(value.Id);
					inputFrame.AddAction(InputAction.VehicleCollision(this, other, value.RelativeVelocity, value.ContactPoint));
				}
			}
		}
		Rigidbody component2 = unityObj.GetComponent<Rigidbody>();
		if (component2 == null || (component2.linearVelocity.sqrMagnitude < MathUtil.Squared(PhysicsStopMovingTolerance) && Mathf.Abs(num) < 10f && (predictedOrElseThisVehicle.GearState == GearState.Neutral || predictedOrElseThisVehicle.GearState == GearState.Off) && predictedOrElseThisVehicle.GearStateTimer >= 1f))
		{
			inputFrame.AddAction(InputAction.RigidBodyStopMoving(this, unityObj.transform.position, unityObj.transform.rotation));
			SentRigidBodyStopMoving = true;
		}
		else
		{
			inputFrame.AddAction(InputAction.VehicleMove(this, unityObj.transform.position, unityObj.transform.rotation, num));
		}
	}

	public override bool IsImpassable(Character requester, int options, TerrainCoord tile)
	{
		if ((options & 0x2000) != 0 && IsMoving)
		{
			return false;
		}
		return base.IsImpassable(requester, options, tile);
	}

	public override Matrix4x4 GetCustomModelTransform()
	{
		if (HasEverMoved)
		{
			PrefabResource unityModel = GetUnityModel();
			if (unityModel != null && unityModel.GetAsset() != null)
			{
				Vector3 centreOfMass = CentreOfMass;
				if (Destroyed && !IsMoving)
				{
					centreOfMass += new Vector3(DemolitionShakeOffset.x, DemolitionShakeOffset.y - DemolitionTransition, DemolitionShakeOffset.z);
				}
				return Matrix4x4.TRS(centreOfMass, Rot, unityModel.LocalScale);
			}
		}
		return base.GetCustomModelTransform();
	}

	public float GetActualFacingAngle()
	{
		return (Rot.eulerAngles.y + AngleFix) * (MathF.PI / 180f);
	}

	public void OnHandBrake()
	{
		HandBrake = true;
	}

	public void OnHorn()
	{
		if (CheckFrontmostPrediction(PredictedEventType.HornSound))
		{
			SoundManager.PlaySound3DFromList(SoundManager.HornSounds, CentreOfMass);
		}
		if (IsAuthoritative())
		{
			Session.Instance.AISoundManager.AddSound(new AISound(AISoundType.Radio, CentreOfMass, HornRange, 0f, this, GetDriver(), null, null));
		}
	}

	public override void OnRigidBodyMove(Vector3 pos, Quaternion rot, float wheelRPM)
	{
		TerrainCoord tile = Tile;
		TerrainRect tileRect = GetTileRect();
		int currentFramesPerFrame = PredictedObjectManager.Instance.GetCurrentFramesPerFrame(IsPredicted());
		if (currentFramesPerFrame > 0)
		{
			float num = 60f / (float)currentFramesPerFrame;
			Velocity = (pos - CentreOfMass) * num;
			Speed = Velocity.magnitude * Mathf.Sign(Vector3.Dot(Velocity, -Matrix4x4.Rotate(rot).Forward()));
		}
		WheelRPM = wheelRPM;
		CentreOfMass = pos;
		Rot = rot;
		Orientation = Prop.GetClosestOrientationTypeToAngle(GetActualFacingAngle());
		Tile = GameTerrain.Instance.GetTileCoordForPos(CentreOfMass - Matrix4x4.Rotate(Rot).MultiplyVector(ModelOffset));
		UpdateWorldTransformAndBounds();
		if (IsAuthoritative())
		{
			if (tileRect != GetTileRect() && IsImpassableProp())
			{
				GameTerrain.Instance.AStar.AddChange(AStarChange.RemovedFixedContents(this, tileRect));
				GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
			}
			if (tile != Tile && IsBurning())
			{
				GameTerrain.Instance.BurningMapWho.RemoveFromMapWho(this, tile);
				GameTerrain.Instance.BurningMapWho.AddToMapWho(this, Tile);
			}
		}
	}

	public override void OnRigidBodyStopMoving(Vector3 pos, Quaternion rot)
	{
		TerrainCoord tile = Tile;
		TerrainRect tileRect = GetTileRect();
		WheelRPM = 0f;
		Speed = 0f;
		Velocity = Vector3.zero;
		CentreOfMass = pos;
		Rot = rot;
		Orientation = Prop.GetClosestOrientationTypeToAngle(GetActualFacingAngle());
		Tile = GameTerrain.Instance.GetTileCoordForPos(CentreOfMass - Matrix4x4.Rotate(Rot).MultiplyVector(ModelOffset));
		IsMoving = false;
		SentRigidBodyStopMoving = false;
		UpdateWorldTransformAndBounds();
		if (UnityObj != null)
		{
			VehicleBehaviour component = UnityObj.GetComponent<VehicleBehaviour>();
			if (component != null)
			{
				component.OnStopMoving();
			}
		}
		RemoveFromActiveMovingUnityObjects();
		SetGearState(GearState.Off);
		if (IsAuthoritative())
		{
			if (!IsBurning() && !Destroyed)
			{
				Session.Instance.PropManager.RemoveFromObjectsThatNeedUpdating(this);
			}
			Session.Instance.PropManager.MovingVehicles.Remove(this);
			if (tileRect != GetTileRect() && IsImpassableProp())
			{
				GameTerrain.Instance.AStar.AddChange(AStarChange.RemovedFixedContents(this, tileRect));
				GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
			}
			if (tile != Tile && IsBurning())
			{
				GameTerrain.Instance.BurningMapWho.RemoveFromMapWho(this, tile);
				GameTerrain.Instance.BurningMapWho.AddToMapWho(this, Tile);
			}
		}
	}

	public TerrainRect GetAStarTileRect()
	{
		lock (GameTerrain.Instance.AStar)
		{
			if (GameTerrain.Instance.AStar.AStarTileContentsManager.AllMoveableObstacles.TryGetValue(Id, out var value))
			{
				return new TerrainRect(value.MinTile, value.MaxTile);
			}
		}
		return TerrainRect.Invalid;
	}

	public override void UpdateWorldTransformAndBounds()
	{
		if (HasEverMoved)
		{
			World = MathUtil.CreateTranslation(CalcWorldPos()) * MathUtil.CreateRotationY(GetFacingAngleRad());
			PrefabResource unityModel = GetUnityModel();
			if (unityModel != null && unityModel.GetAsset() != null)
			{
				Bounds identityBounds = unityModel.IdentityBounds;
				Matrix4x4 customModelTransform = GetCustomModelTransform();
				Vector3 lhs = customModelTransform.MultiplyPoint(identityBounds.min);
				Vector3 rhs = customModelTransform.MultiplyPoint(identityBounds.max);
				Vector3 lhs2 = customModelTransform.MultiplyPoint(new Vector3(identityBounds.min.x, identityBounds.min.y, identityBounds.max.z));
				Vector3 rhs2 = customModelTransform.MultiplyPoint(new Vector3(identityBounds.max.x, identityBounds.max.y, identityBounds.min.z));
				Vector3 min = Vector3.Min(Vector3.Min(lhs, rhs), Vector3.Min(lhs2, rhs2));
				Vector3 max = Vector3.Max(Vector3.Max(lhs, rhs), Vector3.Max(lhs2, rhs2));
				SetBoundingBox(MathUtil.CreateBoundsMinMax(min, max));
			}
			UpdateUnityTransform();
			for (int i = 0; i < Inhabitants.Length; i++)
			{
				if (Inhabitants[i] != null && Inhabitants[i].IsAuthoritative() == IsAuthoritative())
				{
					Vector3 slotPos = GetSlotPos(i);
					float slotAngleRad = GetSlotAngleRad(i);
					Inhabitants[i].SetPosition(slotPos);
					Inhabitants[i].SetFollowMePos(Vector2.zero);
					Inhabitants[i].SetFacingAngle(slotAngleRad);
				}
			}
		}
		else
		{
			base.UpdateWorldTransformAndBounds();
		}
	}

	public override Vector3 GetSlotPos(int i)
	{
		if (HasEverMoved)
		{
			return (Matrix4x4.TRS(CentreOfMass, Rot, Vector3.one) * Matrix4x4.Translate(-ModelOffset)).MultiplyPoint(GetInhabitantSlotDefs()[i].Pos);
		}
		return base.GetSlotPos(i);
	}

	public override float GetSlotAngleRad(int i)
	{
		if (HasEverMoved)
		{
			return GetActualFacingAngle() + MathF.PI;
		}
		return base.GetSlotAngleRad(i) + MathF.PI;
	}

	public override Vector3 GetEntrancePos(int i)
	{
		if (HasEverMoved)
		{
			TerrainCoord entranceOffset = GetEntranceDefs()[i].EntranceOffset;
			Vector2 dirFromAngle = MathUtil.GetDirFromAngle(GetEntranceDefs()[i].EntranceAngle);
			Matrix4x4 matrix4x = Matrix4x4.TRS(CentreOfMass, Rot, Vector3.one) * Matrix4x4.Translate(-ModelOffset);
			for (int j = 0; j < 100; j++)
			{
				Vector3 vector = matrix4x.MultiplyPoint(new Vector3(entranceOffset.x, 0f, entranceOffset.y) + MathUtil.ToX0Y(dirFromAngle) * j);
				TerrainCoord tileCoordForPos = GameTerrain.Instance.GetTileCoordForPos(vector);
				if (!GetTileRect().Contains(tileCoordForPos))
				{
					return vector;
				}
			}
		}
		return base.GetEntrancePos(i);
	}

	public override float GetEntranceAngle(int i)
	{
		if (HasEverMoved)
		{
			float entranceAngle = GetEntranceDefs()[i].EntranceAngle;
			return MathUtil.WrapAngle(GetActualFacingAngle() + entranceAngle);
		}
		return base.GetEntranceAngle(i);
	}

	public void SetDriveable(bool driveable)
	{
		IsDriveable = driveable;
		if (IsDriveable)
		{
			SetMossCleanedAway(mossCleanedAway: true);
		}
	}

	public override void AbandonBuilding()
	{
		base.AbandonBuilding();
		SetDriveable(driveable: false);
	}

	public override bool CaptureBuilding(float amount, Character capturedBy)
	{
		bool num = base.CaptureBuilding(amount, capturedBy);
		if (num)
		{
			SetDriveable(driveable: true);
		}
		return num;
	}

	public override void RepairDamage(float amount, Vector3 fromPos)
	{
		base.RepairDamage(amount, fromPos);
		if (MossCleanedAway)
		{
			SetDriveable(driveable: true);
		}
	}

	public override void OnDestroyed(Character source, bool from_impact, Vector3 hitPos)
	{
		if (from_impact)
		{
			SetDriveable(driveable: false);
		}
		else
		{
			base.OnDestroyed(source, from_impact, hitPos);
		}
	}

	public Character GetDriver()
	{
		Character[] inhabitants = Inhabitants;
		foreach (Character character in inhabitants)
		{
			if (character != null && character.DirectControlled && character.ConsciousAndNotZombie && character.GetBaseObjectType() == BaseObjectType.Human)
			{
				return character;
			}
		}
		inhabitants = Inhabitants;
		foreach (Character character2 in inhabitants)
		{
			if (character2 != null && character2.ConsciousAndNotZombie && character2.GetBaseObjectType() == BaseObjectType.Human)
			{
				return character2;
			}
		}
		return null;
	}

	public Character GetDriverAuthoritative()
	{
		return GetDriver()?.GetAuthoritativeOrElseThisCharacter();
	}

	public override float GetMassEstimate()
	{
		if (Prototype == null)
		{
			return base.GetMassEstimate();
		}
		return Prototype.Mass;
	}

	public override bool AddTalkToInhabitantActions(Character controlledCharacter, List<AvailableAction> actions)
	{
		if (IsDriveable && Session.Instance.GameCamera.FlyCam)
		{
			Character driver = GetDriver();
			if (driver != null)
			{
				actions.Add(new AvailableAction(CursorAction.ControlVehicle, driver, null, CursorActionDisabledReason.Enabled));
			}
		}
		return base.AddTalkToInhabitantActions(controlledCharacter, actions);
	}
}
