using System;
using System.Collections.Generic;
using UnityEngine;

public class VehicleBehaviour : MonoBehaviour
{
	public struct CollisionInfo
	{
		public int Id;

		public int RefCount;

		public Vector3 RelativeVelocity;

		public Vector3 ContactPoint;

		public bool Sent;
	}

	private EnterableVehicle OwnerAuthoritative;

	private PrefabResource Prefab;

	private GameObject UnityWheelGroup;

	public List<WheelCollider> WheelColliders = new List<WheelCollider>();

	public List<GameObject> Wheels = new List<GameObject>();

	public List<int> WheelHitObjectId = new List<int>();

	public List<GroundType> WheelTerrainType = new List<GroundType>();

	public AudioSource UnityEngineIdleSound;

	public AudioSource UnityEngineRunningSound;

	public AudioSource UnityEngineReversingSound;

	public AudioSource UnityBrakeSound;

	private static string WheelGroupStr = "WheelGroup";

	public static float ForceAppPointDist = 0.75f;

	public static float ForwardFrictionExtremumSlip = 1f;

	public static float ForwardFrictionExtremumValue = 3f;

	public static float ForwardFrictionAsymptoteSlip = 2f;

	public static float ForwardFrictionAsymptoteValue = 1.5f;

	public static float ForwardFrictionStiffness = 1f;

	public static float SidewaysFrictionExtremumSlip = 0.25f;

	public static float SidewaysFrictionExtremumValue = 1.5f;

	public static float SidewaysFrictionAsymptoteSlip = 1f;

	public static float SidewaysFrictionAsymptoteValue = 1f;

	public static float SidewaysFrictionStiffness = 1f;

	public static float[] TerrainExtremumFactor = new float[3] { 1f, 0.75f, 0.75f };

	public static float[] TerrainExtremumFactorInSnow = new float[3] { 0.75f, 0.5f, 0.25f };

	public static float[] TerrainAsymptoteFactor = new float[3] { 1f, 1f, 1f };

	public static float[] TerrainAsymptoteFactorInSnow = new float[3] { 0.75f, 0.5f, 0.25f };

	public static float EngineVolume = 1f;

	private float Fade;

	private bool OnSoundPlayed;

	private bool OffSoundPlayed;

	private float SnowFade;

	private static List<TileObject> JuggernautContacts;

	private static List<TileObject> JuggernautOldContacts;

	public static float JuggernautFakeCollisionLookahead = 5f;

	private float CurBrakingVolume;

	private float TargetBrakingVolume;

	private static float HandBrakePowerFactor = 10f;

	private static float AllWheelDriveTorqFactor = 0.5f;

	public List<CollisionInfo> CurrentCollisions = new List<CollisionInfo>();

	private float NextImpactSound = -1000f;

	public static float CarCrashSoundVolume = 0.75f;

	public static void CalculateWheelPos(GameObject unityObj, ref Vector2 wheelPos, ref Vector2 wheelOffset, ref float extraFrontWheelSeparation, ref float wheelRadius)
	{
		GameObject gameObject = unityObj.FindChildWithNameContaining("Wheel_F_R");
		GameObject gameObject2 = unityObj.FindChildWithNameContaining("Wheel_F_L");
		GameObject gameObject3 = unityObj.FindChildWithNameContaining("Wheel_R_R");
		GameObject gameObject4 = unityObj.FindChildWithNameContaining("Wheel_R_L");
		if (gameObject != null && gameObject2 != null && gameObject3 != null && gameObject4 != null)
		{
			MeshFilter component = gameObject.GetComponent<MeshFilter>();
			if (component != null && component.mesh != null)
			{
				wheelRadius = component.mesh.bounds.extents.y * gameObject.transform.lossyScale.y;
			}
			Vector3 vector = unityObj.transform.localToWorldMatrix.inverse.MultiplyPoint(gameObject.transform.position);
			Vector3 vector2 = unityObj.transform.localToWorldMatrix.inverse.MultiplyPoint(gameObject2.transform.position);
			Vector3 vector3 = unityObj.transform.localToWorldMatrix.inverse.MultiplyPoint(gameObject3.transform.position);
			Vector3 vector4 = unityObj.transform.localToWorldMatrix.inverse.MultiplyPoint(gameObject4.transform.position);
			Vector3 a = Vector3.Lerp(vector, vector2, 0.5f);
			Vector3 b = Vector3.Lerp(vector3, vector4, 0.5f);
			Vector3 vector5 = Vector3.Lerp(a, b, 0.5f);
			wheelOffset = MathUtil.ToXZ(vector5);
			wheelPos = MathUtil.ToXZ(vector4 - vector5);
			extraFrontWheelSeparation = ((vector - vector2).magnitude - (vector3 - vector4).magnitude) * 0.5f;
		}
		else
		{
			Debug.LogWarning("Couldn't find objects named Wheel_F_R, Wheel_F_L, Wheel_R_R, Wheel_R_L");
		}
	}

	public void UpdateWheelFriction()
	{
		Session instance = Session.Instance;
		float t = Mathf.Clamp01(instance.Weather.TemperatureInCelsius / Weather.WaterCompletelyFrozenTemperatureInCelsius);
		for (int i = 0; i < WheelColliders.Count; i++)
		{
			WheelCollider wheelCollider = WheelColliders[i];
			GroundType groundType = WheelTerrainType[i];
			float a = TerrainExtremumFactor[(int)groundType];
			float b = TerrainExtremumFactorInSnow[(int)groundType];
			float a2 = TerrainAsymptoteFactor[(int)groundType];
			float b2 = TerrainAsymptoteFactorInSnow[(int)groundType];
			float num = 1f;
			float num2 = 1f;
			if (!OwnerAuthoritative.IsJuggernaut())
			{
				switch (groundType)
				{
				case GroundType.Road:
					num = Mathf.Lerp(a, b, instance.Weather.SnowOnGroundAmount);
					num2 = Mathf.Lerp(a2, b2, instance.Weather.SnowOnGroundAmount);
					break;
				case GroundType.Grass:
				{
					float t2 = Math.Max(instance.Weather.SnowOnGroundAmount, instance.Weather.GroundMuddiness);
					num = Mathf.Lerp(a, b, t2);
					num2 = Mathf.Lerp(a2, b2, t2);
					break;
				}
				case GroundType.River:
					num = Mathf.Lerp(a, b, t);
					num2 = Mathf.Lerp(a2, b2, t);
					break;
				}
			}
			wheelCollider.forwardFriction = new WheelFrictionCurve
			{
				extremumSlip = ForwardFrictionExtremumSlip,
				extremumValue = ForwardFrictionExtremumValue * num,
				asymptoteSlip = ForwardFrictionAsymptoteSlip,
				asymptoteValue = ForwardFrictionAsymptoteValue * num2,
				stiffness = ForwardFrictionStiffness
			};
			wheelCollider.sidewaysFriction = new WheelFrictionCurve
			{
				extremumSlip = SidewaysFrictionExtremumSlip,
				extremumValue = SidewaysFrictionExtremumValue * num,
				asymptoteSlip = SidewaysFrictionAsymptoteSlip,
				asymptoteValue = SidewaysFrictionAsymptoteValue * num2,
				stiffness = SidewaysFrictionStiffness
			};
		}
	}

	public static void OnFrictionSettingsChanged()
	{
		foreach (EnterableVehicle movingVehicle in Session.Instance.PropManager.MovingVehicles)
		{
			if (movingVehicle.UnityObj != null)
			{
				VehicleBehaviour component = movingVehicle.UnityObj.GetComponent<VehicleBehaviour>();
				if (component != null)
				{
					component.UpdateWheelFriction();
				}
			}
		}
	}

	public void Init(EnterableVehicle owner, PrefabResource prefab)
	{
		OwnerAuthoritative = owner;
		Prefab = prefab;
		if (JuggernautContacts != null)
		{
			JuggernautContacts = null;
		}
		if (JuggernautOldContacts != null)
		{
			JuggernautOldContacts = null;
		}
		PropPrototype prototype = owner.Prototype;
		if (UnityWheelGroup == null)
		{
			SnowFade = Session.Instance.Weather.SnowOnGroundAmount;
			GameObject gameObject = new GameObject();
			gameObject.layer = base.gameObject.layer;
			gameObject.name = WheelGroupStr;
			gameObject.transform.parent = base.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			UnityWheelGroup = gameObject;
			Vector2 wheelPos = prototype.WheelPos;
			Vector2 wheelOffset = prototype.WheelOffset;
			for (int i = -1; i <= 1; i += 2)
			{
				float num = ((i == -1) ? prototype.ExtraFrontWheelSeparation : 0f);
				for (int j = -1; j <= 1; j += 2)
				{
					GameObject obj = new GameObject();
					obj.layer = base.gameObject.layer;
					obj.name = ((i == -1) ? "Front" : "Rear") + ((j == -1) ? "Right" : "Left");
					obj.transform.parent = gameObject.transform;
					WheelCollider wheelCollider = obj.AddComponent<WheelCollider>();
					wheelCollider.mass = prototype.Mass / 100f;
					wheelCollider.radius = prototype.VehicleWheelRadius;
					wheelCollider.wheelDampingRate = 0.25f;
					wheelCollider.suspensionDistance = prototype.VehicleWheelRadius * prototype.VehicleSuspensionBounce;
					wheelCollider.forceAppPointDistance = prefab.IdentityBounds.extents.y * ForceAppPointDist;
					wheelCollider.forwardFriction = new WheelFrictionCurve
					{
						extremumSlip = ForwardFrictionExtremumSlip,
						extremumValue = ForwardFrictionExtremumValue,
						asymptoteSlip = ForwardFrictionAsymptoteSlip,
						asymptoteValue = ForwardFrictionAsymptoteValue,
						stiffness = ForwardFrictionStiffness
					};
					wheelCollider.sidewaysFriction = new WheelFrictionCurve
					{
						extremumSlip = SidewaysFrictionExtremumSlip,
						extremumValue = SidewaysFrictionExtremumValue,
						asymptoteSlip = SidewaysFrictionAsymptoteSlip,
						asymptoteValue = SidewaysFrictionAsymptoteValue,
						stiffness = SidewaysFrictionStiffness
					};
					WheelColliders.Add(wheelCollider);
					obj.transform.localPosition = new Vector3(wheelOffset.x + (wheelPos.x + num) * (float)j, prototype.VehicleWheelRadius + wheelCollider.suspensionDistance * wheelCollider.suspensionSpring.targetPosition, wheelOffset.y + wheelPos.y * (float)i);
					obj.transform.localRotation = Quaternion.identity;
					string str = "Wheel_" + ((i == -1) ? "F" : "R") + "_" + ((j == -1) ? "R" : "L");
					GameObject item = base.gameObject.FindChildWithNameContaining(str);
					Wheels.Add(item);
					WheelHitObjectId.Add(0);
					if (WheelTerrainType.Count < Wheels.Count)
					{
						WheelTerrainType.Add(GroundType.Road);
					}
				}
			}
		}
		if (UnityEngineIdleSound == null)
		{
			UnityEngineIdleSound = base.gameObject.AddComponent<AudioSource>();
			UnityEngineIdleSound.clip = SoundManager.EngineRunningSound;
			UnityEngineIdleSound.loop = true;
			UnityEngineIdleSound.spatialBlend = 1f;
			UnityEngineIdleSound.minDistance = SoundManager.AudioRolloffMinDist;
			UnityEngineIdleSound.maxDistance = SoundManager.AudioRolloffMaxDist;
			UnityEngineIdleSound.RealisticRolloff();
			UnityEngineIdleSound.playOnAwake = false;
		}
		if (UnityEngineRunningSound == null)
		{
			UnityEngineRunningSound = base.gameObject.AddComponent<AudioSource>();
			UnityEngineRunningSound.clip = SoundManager.EngineRunningSound;
			UnityEngineRunningSound.loop = true;
			UnityEngineRunningSound.spatialBlend = 1f;
			UnityEngineRunningSound.minDistance = SoundManager.AudioRolloffMinDist;
			UnityEngineRunningSound.maxDistance = SoundManager.AudioRolloffMaxDist;
			UnityEngineRunningSound.RealisticRolloff();
			UnityEngineRunningSound.playOnAwake = false;
		}
		if (UnityEngineReversingSound == null)
		{
			UnityEngineReversingSound = base.gameObject.AddComponent<AudioSource>();
			UnityEngineReversingSound.clip = SoundManager.EngineRunningSound;
			UnityEngineReversingSound.loop = true;
			UnityEngineReversingSound.spatialBlend = 1f;
			UnityEngineReversingSound.minDistance = SoundManager.AudioRolloffMinDist;
			UnityEngineReversingSound.maxDistance = SoundManager.AudioRolloffMaxDist;
			UnityEngineReversingSound.RealisticRolloff();
			UnityEngineReversingSound.playOnAwake = false;
		}
		if (UnityBrakeSound == null)
		{
			UnityBrakeSound = base.gameObject.AddComponent<AudioSource>();
			UnityBrakeSound.clip = SoundManager.BrakingSound;
			UnityBrakeSound.loop = true;
			UnityBrakeSound.spatialBlend = 1f;
			UnityBrakeSound.minDistance = SoundManager.AudioRolloffMinDist;
			UnityBrakeSound.maxDistance = SoundManager.AudioRolloffMaxDist;
			UnityBrakeSound.RealisticRolloff();
			UnityBrakeSound.playOnAwake = false;
		}
	}

	public void OnStopMoving()
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = true;
		}
		if (UnityWheelGroup != null)
		{
			UnityEngine.Object.Destroy(UnityWheelGroup);
			UnityWheelGroup = null;
			WheelColliders.Clear();
			Wheels.Clear();
			WheelHitObjectId.Clear();
		}
	}

	private void Update()
	{
		GameImpl instance = GameImpl.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		EnterableVehicle predictedOrElseThisVehicle = OwnerAuthoritative.GetPredictedOrElseThisVehicle();
		Rigidbody component = GetComponent<Rigidbody>();
		if (component != null && component.isKinematic)
		{
			Vector3 position = (base.transform.position = predictedOrElseThisVehicle.CentreOfMass);
			component.position = position;
		}
		float num = Mathf.Clamp01(predictedOrElseThisVehicle.CurrentRPM / predictedOrElseThisVehicle.Prototype.VehicleMaxRPM);
		if (predictedOrElseThisVehicle.GearState >= GearState.StartEngine)
		{
			if (!UnityEngineIdleSound.isPlaying && !instance.IsMenuOpen())
			{
				UnityEngineIdleSound.Play();
				UnityEngineRunningSound.Play();
				UnityEngineReversingSound.Play();
			}
			Fade = Math.Min(1f, Fade + Time.deltaTime);
			OffSoundPlayed = false;
			if (!OnSoundPlayed)
			{
				SoundManager.PlaySound3D(SoundManager.EngineStartSound, predictedOrElseThisVehicle.CentreOfMass, EngineVolume);
				OnSoundPlayed = true;
			}
			if (OwnerAuthoritative.IsJuggernaut() && component != null && component.linearVelocity.y >= 1f)
			{
				component.linearVelocity = new Vector3(component.linearVelocity.x, 1f, component.linearVelocity.z);
			}
			if (Prefab != null && Prefab.InitialWheelRot != null)
			{
				for (int i = 0; i < Wheels.Count; i++)
				{
					GameObject gameObject = Wheels[i];
					if (!(gameObject != null))
					{
						continue;
					}
					WheelColliders[i].GetWorldPose(out var pos, out var quat);
					gameObject.transform.position = pos;
					gameObject.transform.rotation = quat * Prefab.InitialWheelRot[i];
					GroundType value = GroundType.Road;
					int num2 = 0;
					if (WheelColliders[i].GetGroundHit(out var hit) && hit.collider != null)
					{
						num2 = IdBehaviour.GetIdFromUnityObject(hit.collider.gameObject);
						if (num2 == instance2.Id)
						{
							TerrainCoord tileCoordForPos = instance2.GetTileCoordForPos(hit.point);
							TerrainType tileTerrainType = instance2.GetTileTerrainType(tileCoordForPos);
							value = (((uint)(tileTerrainType - 2) > 2u) ? ((!instance2.IsTileRiver(tileCoordForPos.x, tileCoordForPos.y)) ? GroundType.Grass : GroundType.River) : GroundType.Road);
						}
					}
					WheelTerrainType[i] = value;
					if (OwnerAuthoritative.IsJuggernaut() && WheelHitObjectId[i] != num2)
					{
						if (WheelHitObjectId[i] != 0)
						{
							RemoveFromCurrentCollisions(WheelHitObjectId[i]);
						}
						WheelHitObjectId[i] = num2;
						if (num2 != 0 && num2 != instance2.Id)
						{
							AddToCurrentCollisions(num2, hit.force * hit.forwardDir, hit.point, trigger: false);
						}
					}
				}
				UpdateWheelFriction();
			}
			if (OwnerAuthoritative.IsJuggernaut() && component != null && component.linearVelocity.sqrMagnitude > 0.0001f)
			{
				if (JuggernautContacts == null)
				{
					JuggernautContacts = new List<TileObject>();
					JuggernautOldContacts = new List<TileObject>();
				}
				PrefabResource unityModel = predictedOrElseThisVehicle.GetUnityModel();
				Bounds obb = MathUtil.CreateBoundsCentreExtents(unityModel.IdentityBounds.center, unityModel.IdentityBounds.extents);
				Matrix4x4 matrix4x = Matrix4x4.TRS(predictedOrElseThisVehicle.CentreOfMass + component.linearVelocity * (1f / 60f) * JuggernautFakeCollisionLookahead, predictedOrElseThisVehicle.Rot, unityModel.LocalScale);
				if (unityModel != null)
				{
					Vector3 lhs = matrix4x.MultiplyPoint(obb.center + new Vector3(obb.extents.x, 0f, obb.extents.z));
					Vector3 rhs = matrix4x.MultiplyPoint(obb.center + new Vector3(0f - obb.extents.x, 0f, obb.extents.z));
					Vector3 lhs2 = matrix4x.MultiplyPoint(obb.center + new Vector3(obb.extents.x, 0f, 0f - obb.extents.z));
					Vector3 rhs2 = matrix4x.MultiplyPoint(obb.center + new Vector3(0f - obb.extents.x, 0f, 0f - obb.extents.z));
					TerrainCoord tileCoordForPos2 = instance2.GetTileCoordForPos(Vector3.Min(Vector3.Min(lhs, rhs), Vector3.Min(lhs2, rhs2)));
					TerrainCoord tileCoordForPos3 = instance2.GetTileCoordForPos(Vector3.Max(Vector3.Max(lhs, rhs), Vector3.Max(lhs2, rhs2)));
					instance2.GetObjectsInRect(tileCoordForPos2, tileCoordForPos3, JuggernautContacts);
					foreach (TileObject juggernautOldContact in JuggernautOldContacts)
					{
						if (!JuggernautContacts.Contains(juggernautOldContact))
						{
							RemoveFromCurrentCollisions(juggernautOldContact.Id);
						}
					}
					JuggernautOldContacts.Clear();
					foreach (TileObject juggernautContact in JuggernautContacts)
					{
						if (juggernautContact != OwnerAuthoritative && !JuggernautOldContacts.Contains(juggernautContact) && !(juggernautContact is Character) && (juggernautContact.IsSusceptibleToVehicleCollisions(juggernaut: true) || juggernautContact is TreeProp) && juggernautContact.GetBoundingBox().IntersectsOBB(ref matrix4x, obb))
						{
							Vector3 boundingBoxCentre = juggernautContact.GetBoundingBoxCentre();
							boundingBoxCentre += MathUtil.SafeNormalize(predictedOrElseThisVehicle.GetBoundingBoxCentre() - boundingBoxCentre, Vector3.zero) * juggernautContact.GetBoundingBox().extents.magnitude;
							AddToCurrentCollisions(juggernautContact.Id, -component.linearVelocity, boundingBoxCentre, trigger: false);
							JuggernautOldContacts.Add(juggernautContact);
						}
					}
					JuggernautContacts.Clear();
				}
			}
		}
		else
		{
			Fade = Math.Max(0f, Fade - Time.deltaTime * 2f);
			OnSoundPlayed = false;
			if (!OffSoundPlayed)
			{
				SoundManager.PlaySound3D(SoundManager.EngineStopSound, predictedOrElseThisVehicle.CentreOfMass, EngineVolume);
				OffSoundPlayed = true;
			}
			if (UnityEngineIdleSound.isPlaying && Fade == 0f)
			{
				UnityEngineIdleSound.Stop();
				UnityEngineRunningSound.Stop();
				UnityEngineReversingSound.Stop();
			}
		}
		if (instance.IsMenuOpen() && UnityEngineIdleSound.isPlaying)
		{
			UnityEngineIdleSound.Stop();
			UnityEngineRunningSound.Stop();
			UnityEngineReversingSound.Stop();
		}
		float snowFade = SnowFade;
		if (predictedOrElseThisVehicle.GearState == GearState.Off)
		{
			Weather weather = Session.Instance.Weather;
			if (weather.TemperatureInCelsius <= Weather.SnowSettleTempCelcius)
			{
				SnowFade = Mathf.Min(1f, SnowFade + weather.PrecipitationAmount * Weather.SnowOnGroundBuildupRate * Time.deltaTime);
			}
			if (weather.TemperatureInCelsius >= 0f)
			{
				SnowFade = Mathf.Max(0f, SnowFade - weather.TemperatureInCelsius * Weather.SnowOnGroundMeltRate * Time.deltaTime);
			}
		}
		else
		{
			SnowFade = Mathf.Max(0f, SnowFade - Time.deltaTime);
		}
		if (SnowFade != snowFade)
		{
			Prop.UnitySetSnowFade(base.gameObject, SnowFade);
		}
		float worldSoundVolume = SoundManager.WorldSoundVolume;
		if (UnityEngineIdleSound.isPlaying)
		{
			UnityEngineIdleSound.volume = Mathf.Lerp(0.5f, 1f, num) * EngineVolume * Fade * worldSoundVolume;
			if (num > 0f)
			{
				UnityEngineReversingSound.volume = 0f;
				UnityEngineRunningSound.volume = Mathf.Lerp(0.5f, 1f, num) * EngineVolume * Fade * worldSoundVolume;
				UnityEngineRunningSound.pitch = Mathf.Lerp(0.3f, 2f, num);
			}
			else
			{
				UnityEngineRunningSound.volume = 0f;
				UnityEngineReversingSound.volume = Mathf.Lerp(0f, 1f, num) * EngineVolume * Fade * worldSoundVolume;
				UnityEngineReversingSound.pitch = Mathf.Lerp(0.2f, 1f, num);
			}
		}
		CurBrakingVolume = Mathf.Lerp(CurBrakingVolume, TargetBrakingVolume, Time.deltaTime * 4f);
		if (CurBrakingVolume > 0.0001f)
		{
			if (!UnityBrakeSound.isPlaying)
			{
				UnityBrakeSound.Play();
			}
			UnityBrakeSound.volume = Mathf.Clamp01(CurBrakingVolume * CurBrakingVolume) * worldSoundVolume;
		}
		else if (CurBrakingVolume <= 0.0001f && UnityBrakeSound.isPlaying)
		{
			UnityBrakeSound.Stop();
		}
	}

	public void WheelSteer(float steeringAngle)
	{
		if (WheelColliders.Count >= 2)
		{
			WheelColliders[0].steerAngle = steeringAngle;
			WheelColliders[1].steerAngle = steeringAngle;
		}
	}

	public void AddBrakeTorq(float brake, float handbrake)
	{
		float brakeTorque = brake * OwnerAuthoritative.Prototype.VehicleBrakePower;
		float num = handbrake * HandBrakePowerFactor * OwnerAuthoritative.Prototype.VehicleBrakePower;
		for (int i = 0; i < WheelColliders.Count; i++)
		{
			WheelColliders[i].brakeTorque = 0f;
		}
		switch (OwnerAuthoritative.Prototype.VehicleBrakeType)
		{
		case VehicleTractionType.AllWheelDrive:
		{
			for (int j = 0; j < WheelColliders.Count; j++)
			{
				WheelColliders[j].brakeTorque = brakeTorque;
			}
			break;
		}
		case VehicleTractionType.FrontWheelDrive:
		{
			for (int k = 0; k < Math.Min(2, WheelColliders.Count); k++)
			{
				WheelColliders[k].brakeTorque = brakeTorque;
			}
			break;
		}
		case VehicleTractionType.RearWheelDrive:
			if (WheelColliders.Count >= 2)
			{
				WheelColliders[WheelColliders.Count - 1].brakeTorque = brakeTorque;
				WheelColliders[WheelColliders.Count - 2].brakeTorque = brakeTorque;
			}
			break;
		}
		if (WheelColliders.Count >= 2)
		{
			WheelColliders[WheelColliders.Count - 1].brakeTorque += num;
			WheelColliders[WheelColliders.Count - 2].brakeTorque += num;
		}
		float num2 = Mathf.Abs(OwnerAuthoritative.GetPredictedOrElseThisVehicle().GetSpeedInMph());
		TargetBrakingVolume = (brake + handbrake) * Mathf.Clamp01(num2 / 30f);
	}

	public void AddTorq(float torq)
	{
		for (int i = 0; i < WheelColliders.Count; i++)
		{
			WheelColliders[i].motorTorque = 0f;
		}
		switch (OwnerAuthoritative.Prototype.VehicleTractionType)
		{
		case VehicleTractionType.AllWheelDrive:
		{
			for (int j = 0; j < WheelColliders.Count; j++)
			{
				WheelColliders[j].motorTorque = (0f - torq) * AllWheelDriveTorqFactor;
			}
			break;
		}
		case VehicleTractionType.FrontWheelDrive:
		{
			for (int k = 0; k < Math.Min(2, WheelColliders.Count); k++)
			{
				WheelColliders[k].motorTorque = 0f - torq;
			}
			break;
		}
		case VehicleTractionType.RearWheelDrive:
			if (WheelColliders.Count >= 2)
			{
				WheelColliders[WheelColliders.Count - 1].motorTorque = 0f - torq;
				WheelColliders[WheelColliders.Count - 2].motorTorque = 0f - torq;
			}
			break;
		}
	}

	public float GetWheelRPM()
	{
		float num = 0f;
		float num2 = 0f;
		switch (OwnerAuthoritative.Prototype.VehicleTractionType)
		{
		case VehicleTractionType.AllWheelDrive:
		{
			for (int i = 0; i < WheelColliders.Count; i++)
			{
				num += WheelColliders[i].rpm;
			}
			num2 = WheelColliders.Count;
			break;
		}
		case VehicleTractionType.FrontWheelDrive:
		{
			for (int j = 0; j < Math.Min(2, WheelColliders.Count); j++)
			{
				num += WheelColliders[j].rpm;
			}
			num2 = 2f;
			break;
		}
		case VehicleTractionType.RearWheelDrive:
			if (WheelColliders.Count >= 2)
			{
				num += WheelColliders[WheelColliders.Count - 1].rpm;
				num += WheelColliders[WheelColliders.Count - 2].rpm;
			}
			num2 = 2f;
			break;
		}
		return (0f - num) / num2;
	}

	private int FindCollisionInfo(int id)
	{
		for (int i = 0; i < CurrentCollisions.Count; i++)
		{
			if (CurrentCollisions[i].Id == id)
			{
				return i;
			}
		}
		return -1;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.contactCount > 0)
		{
			AddToCurrentCollisions(collision.gameObject, collision.relativeVelocity, collision.GetContact(0).point, trigger: false);
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		RemoveFromCurrentCollisions(collision.gameObject);
	}

	private void OnTriggerEnter(Collider other)
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if (component != null)
		{
			AddToCurrentCollisions(other.gameObject, -component.linearVelocity, other.ClosestPoint(component.centerOfMass), trigger: true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		RemoveFromCurrentCollisions(other.gameObject);
	}

	private void AddToCurrentCollisions(GameObject unityObj, Vector3 relativeVelocity, Vector3 contactPoint, bool trigger)
	{
		int idFromUnityObject = IdBehaviour.GetIdFromUnityObject(unityObj);
		if (idFromUnityObject != 0)
		{
			AddToCurrentCollisions(idFromUnityObject, relativeVelocity, contactPoint, trigger);
		}
	}

	private void AddToCurrentCollisions(int id, Vector3 relativeVelocity, Vector3 contactPoint, bool trigger)
	{
		int num = FindCollisionInfo(id);
		if (num == -1)
		{
			BaseObject baseObject = BaseObjectManager.Instance.FindBaseObjectByID(id);
			if (baseObject != null)
			{
				Character character = baseObject as Character;
				if (character != null && !character.InTerrain)
				{
					return;
				}
				if (Time.time >= NextImpactSound)
				{
					float num2 = relativeVelocity.magnitude * 2.2369418f;
					float num3 = 0.5f + Mathf.Clamp01(num2 / 20f) * 0.5f;
					if (character != null)
					{
						if (trigger)
						{
							OwnerAuthoritative.GetPredictedOrElseThisVehicle().PlaySoundOneShotFromList(SoundManager.CarHitBodySounds);
							NextImpactSound = Time.time + Mathf.Lerp(0.1f, 0.5f, MathUtil.NonDeterministicRand.RandomFloat());
						}
					}
					else if (baseObject is GameTerrain)
					{
						if (GameTerrain.Instance.GetTreeFromCollisionPoint(contactPoint) != null)
						{
							OwnerAuthoritative.GetPredictedOrElseThisVehicle().PlaySoundOneShotFromList(SoundManager.CarCrashSounds, num3 * CarCrashSoundVolume);
							NextImpactSound = Time.time + Mathf.Lerp(0.1f, 0.5f, MathUtil.NonDeterministicRand.RandomFloat());
						}
					}
					else
					{
						OwnerAuthoritative.GetPredictedOrElseThisVehicle().PlaySoundOneShotFromList(SoundManager.CarCrashSounds, num3 * CarCrashSoundVolume);
						NextImpactSound = Time.time + Mathf.Lerp(0.1f, 0.5f, MathUtil.NonDeterministicRand.RandomFloat());
					}
				}
			}
			CollisionInfo item = new CollisionInfo
			{
				Id = id,
				RefCount = 1,
				RelativeVelocity = relativeVelocity,
				ContactPoint = contactPoint
			};
			CurrentCollisions.Add(item);
		}
		else
		{
			CollisionInfo value = CurrentCollisions[num];
			value.RefCount++;
			CurrentCollisions[num] = value;
		}
	}

	private void RemoveFromCurrentCollisions(GameObject unityObj)
	{
		int idFromUnityObject = IdBehaviour.GetIdFromUnityObject(unityObj);
		if (idFromUnityObject != 0)
		{
			RemoveFromCurrentCollisions(idFromUnityObject);
		}
	}

	private void RemoveFromCurrentCollisions(int id)
	{
		int num = FindCollisionInfo(id);
		if (num != -1)
		{
			CurrentCollisions.RemoveAt(num);
		}
	}
}
