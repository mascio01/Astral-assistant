using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseThrownProjectile : Projectile
{
	public EquipmentPrototype Proto;

	public InfectionType InfectedWith;

	public Vector3 _startPos;

	public Vector3 _targetPos;

	public Vector3 Position;

	public float _throwAngle;

	public float _speed;

	public float _spinSpeed;

	public float _maxRange;

	public float Fuel;

	public InjuryLocation InjuryLocation;

	public float HitDamage;

	public SkillType DamageSkillType;

	public float DamageRadius;

	public bool Assassinate;

	public SecrecyMode Secret;

	public bool Landed;

	public bool IsFromAI;

	public static float NormalSpeed = 16f;

	public static float FastZombieSpeed = 32f;

	public static float MaxThrowSpeed = 16f;

	public static float MaxThrowAngleDeg = 75f;

	public static float Gravity = 9.81f;

	public static float ThrowAngleIncrement = 15f;

	public static float MinSpinSpeed = 60f;

	public static float MaxSpinSpeed = 120f;

	public static string PickUnblockedAngleAndSpeedStr = "PickUnblockedAngleAndSpeed";

	public static bool DrawThrowingArcRaycasts = false;

	private static List<Character> TempCharacters = new List<Character>();

	private static float InitialAngle = 0f;

	private const int DefaultRayCastFlags = 4649;

	private const int ThrowAtFeetRayCastFlags = 4136;

	private static string ParabolicRayCastStr = "ParabolicRayCast";

	public BaseThrownProjectile()
	{
	}

	public override PrefabResource GetUnityModel()
	{
		if (Proto.EquippedModelProperties == null)
		{
			return null;
		}
		return Proto.EquippedModelProperties.Prefab;
	}

	public static float CalcThrowSpeed(Character source, TileObject target)
	{
		float result = NormalSpeed;
		Character character = target as Character;
		if (!source.IsGuarding() && character != null && character.Zombie && !character.ShouldLimp())
		{
			result = FastZombieSpeed;
		}
		return result;
	}

	public static float CalcThrowAngle(Vector3 startPos, Vector3 targetPos, float speed)
	{
		float magnitude = MathUtil.ToXZ(targetPos - startPos).magnitude;
		float num = targetPos.y - startPos.y;
		float gravity = Gravity;
		float num2 = speed * speed;
		float num3 = magnitude * magnitude;
		float result = MathF.PI / 4f;
		float num4 = num2 * num2 - gravity * (gravity * num3 + 2f * num * num2);
		if (num4 >= 0f)
		{
			float num5 = (float)Math.Sqrt(num4);
			float val = (float)Math.Atan((num2 + num5) / (gravity * magnitude));
			float val2 = (float)Math.Atan((num2 - num5) / (gravity * magnitude));
			result = Math.Min(val, val2);
		}
		return result;
	}

	public static float CalcThrowSpeedFromAngle(Vector3 startPos, Vector3 targetPos, float angle)
	{
		float magnitude = MathUtil.ToXZ(targetPos - startPos).magnitude;
		float num = targetPos.y - startPos.y;
		return Mathf.Sqrt(Gravity * magnitude * magnitude / (magnitude * Mathf.Sin(2f * angle) - 2f * num * MathUtil.Squared(Mathf.Cos(angle))));
	}

	public static void PickUnblockedAngleAndSpeed(Character source, TileObject target, Vector3 startPos, Vector3 targetPos, float damageRadius, bool predicted, bool ai, out float throwAngle, out float throwSpeed, out float t, out bool blocked, out Vector3 hitPos)
	{
		using (new UnityProfileMarker(PickUnblockedAngleAndSpeedStr))
		{
			Vector2 horizDir = MathUtil.SafeNormalize(MathUtil.ToXZ(targetPos - startPos), MathUtil.ToXZ(source.Forward));
			float num = (throwSpeed = CalcThrowSpeed(source, target));
			float num2 = (throwAngle = CalcThrowAngle(source.ThrowPosition, targetPos, throwSpeed));
			float num3 = float.MinValue;
			Vector3 vector = targetPos;
			int rayCastFlags = GetRayCastFlags(damageRadius);
			while (true)
			{
				RaycastResult raycastResult = ParabolicRayCast(source, target, startPos, horizDir, throwAngle, throwSpeed, rayCastFlags, predicted, DrawThrowingArcRaycasts, out t);
				hitPos = raycastResult.GetHitPosition();
				if (num3 < 0f)
				{
					num3 = t;
					vector = hitPos;
				}
				if (raycastResult.HitObject is TileObject && raycastResult.HitObject != target && !source.IsEnemy((TileObject)raycastResult.HitObject))
				{
					if (throwAngle >= MaxThrowAngleDeg * (MathF.PI / 180f) - 0.001f)
					{
						blocked = true;
						break;
					}
					throwAngle = Math.Max(0.17453292f, throwAngle + ThrowAngleIncrement * (MathF.PI / 180f));
					throwAngle = Math.Min(throwAngle, MaxThrowAngleDeg * (MathF.PI / 180f));
					throwSpeed = CalcThrowSpeedFromAngle(source.ThrowPosition, targetPos, throwAngle);
					if (!ai)
					{
						throwSpeed = Math.Min(throwSpeed, MaxThrowSpeed);
					}
					continue;
				}
				blocked = (hitPos - targetPos).sqrMagnitude >= 1f;
				break;
			}
			if (blocked && !ai)
			{
				throwSpeed = num;
				throwAngle = num2;
				t = num3;
				hitPos = vector;
			}
		}
	}

	private static bool CanCharacterDodgeSplashDamage(Character character, Character source, Vector3 targetPos, float damageRadius)
	{
		if (!character.Zombie && character.IsAwake && character.InsideBuilding == null && character.GetBaseObjectType() == BaseObjectType.Human && (character.Pos - targetPos).magnitude < damageRadius && !character.IsDodging() && !character.DirectControlled)
		{
			if (character.Community == source.Community)
			{
				return true;
			}
			Target target = character.GetTarget(source);
			if (target != null && target.FullyTracked && target.Visible)
			{
				return true;
			}
		}
		return false;
	}

	public static BaseThrownProjectile Spawn(BaseObjectType projectileType, EquipmentPrototype proto, InfectionType infectedWith, Character source, TileObject target, InjuryLocation injuryLocation, Vector3 startPos, Vector3 targetPos, float throwAngle, float throwSpeed, float maxRange, float fuel, float hitDamage, SkillType damageSkillType, float damageRadius, bool assassinate, SecrecyMode secret, GameObject unityObj, bool fromAI)
	{
		bool flag = source.IsPredicted();
		if (throwSpeed == 0f && throwAngle == 0f)
		{
			PickUnblockedAngleAndSpeed(source, target, startPos, targetPos, damageRadius, flag, ai: true, out throwAngle, out throwSpeed, out var _, out var _, out var _);
		}
		throwSpeed = Math.Max(throwSpeed, 0.0001f);
		if (source.CheckFrontmostPrediction(PredictedEventType.ThrowSound))
		{
			SoundManager.PlaySound3DFromList(SoundManager.ThrowSounds, startPos);
		}
		BaseThrownProjectile baseThrownProjectile = PredictedObjectManager.Instance.CreateProjectile(projectileType, source, target, flag) as BaseThrownProjectile;
		baseThrownProjectile.Proto = proto;
		baseThrownProjectile.InfectedWith = infectedWith;
		baseThrownProjectile._startPos = startPos;
		baseThrownProjectile._targetPos = targetPos;
		baseThrownProjectile._speed = throwSpeed;
		baseThrownProjectile._throwAngle = throwAngle;
		baseThrownProjectile._spinSpeed = Mathf.Lerp(MinSpinSpeed, MaxSpinSpeed, MathUtil.RandomFloat((float)baseThrownProjectile._startTime.TotalMilliseconds));
		baseThrownProjectile._maxRange = maxRange;
		baseThrownProjectile.Fuel = fuel;
		baseThrownProjectile.InjuryLocation = injuryLocation;
		baseThrownProjectile.HitDamage = hitDamage;
		baseThrownProjectile.DamageSkillType = damageSkillType;
		baseThrownProjectile.DamageRadius = damageRadius;
		baseThrownProjectile.Position = startPos;
		baseThrownProjectile.Assassinate = assassinate;
		baseThrownProjectile.Secret = secret;
		baseThrownProjectile.Landed = false;
		baseThrownProjectile.IsFromAI = fromAI;
		baseThrownProjectile.UpdateBoundingBox();
		if (!flag)
		{
			baseThrownProjectile.Init();
		}
		if (!baseThrownProjectile.IsBeingPredicted())
		{
			if (unityObj != null)
			{
				baseThrownProjectile.UnityObj = unityObj;
				baseThrownProjectile.UnityActivate();
				baseThrownProjectile.AddToActiveMovingUnityObjects();
			}
			else
			{
				baseThrownProjectile.UnityInit();
			}
		}
		return baseThrownProjectile;
	}

	public override bool PropWantDelete()
	{
		return Landed;
	}

	public Vector3 GetPosAtTime(float t)
	{
		Vector2 horizDir = MathUtil.SafeNormalize(MathUtil.ToXZ(_targetPos - _startPos), Vector2.zero);
		return GetPosAtTime(t, _speed, _throwAngle, _startPos, horizDir);
	}

	public static Vector3 GetPosAtTime(float t, float speed, float throwAngle, Vector3 startPos, Vector2 horizDir)
	{
		float num = speed * (float)Math.Cos(throwAngle);
		float num2 = num * t;
		float y = startPos.y + num2 * (float)Math.Tan(throwAngle) - Gravity * num2 * num2 / (2f * num * num);
		return new Vector3(startPos.x + horizDir.x * num2, y, startPos.z + horizDir.y * num2);
	}

	public Quaternion GetRotAtTime(float t)
	{
		Vector3 axis = Vector3.Cross(MathUtil.SafeNormalize(MathUtil.ToX0Y(MathUtil.ToXZ(_targetPos - _startPos)), (_source != null) ? _source.Forward : Vector3.forward), Vector3.up);
		return Quaternion.AngleAxis(InitialAngle - _spinSpeed * t, axis);
	}

	private void UpdateBoundingBox()
	{
		float num = 0.4f;
		Bounds boundingBox = MathUtil.CreateBoundsMinMax(Position - new Vector3(num, num, num), Position + new Vector3(num, num, num));
		SetBoundingBox(boundingBox);
	}

	public static int GetRayCastFlags(float damageRadius)
	{
		if (!(damageRadius > 0f))
		{
			return 4649;
		}
		return 4136;
	}

	public override void ProjectileUpdate(TimeSpan dt)
	{
		if (!Landed)
		{
			float num = (float)(PredictedObjectManager.Instance.GetCurrentTime(IsPredicted()) - _startTime).TotalSeconds;
			Vector3 vector = GetPosAtTime(num) - Position;
			float magnitude = vector.magnitude;
			if (magnitude > 0.0001f)
			{
				vector /= magnitude;
				Ray ray = new Ray(Position, vector);
				int rayCastFlags = GetRayCastFlags(DamageRadius);
				RaycastResult raycastResult = GameTerrain.Instance.RayCast(ray, magnitude, rayCastFlags, _source.InsideBuilding, _source, _target, IsPredicted());
				Position = raycastResult.GetHitPosition();
				UpdateBoundingBox();
				if (raycastResult.HitObject != null || num >= 10f)
				{
					Land(raycastResult.HitObject as TileObject, vector, raycastResult.Normal);
				}
			}
		}
		if (DamageRadius > 0f)
		{
			float num2 = DamageRadius + 1f;
			Vector3 vector2 = (Landed ? Position : _targetPos);
			GameTerrain instance = GameTerrain.Instance;
			TerrainCoord tileCoordForPos = instance.GetTileCoordForPos(vector2);
			int num3 = Mathf.CeilToInt(num2);
			if (IsPredicted())
			{
				foreach (TileObject predictedObject in PredictedObjectManager.Instance.PredictedObjects)
				{
					if (predictedObject is Character character && CanCharacterDodgeSplashDamage(character, _source, vector2, num2))
					{
						character.OnChanceToDodgeSplashDamage(_source, _startPos, vector2, this);
					}
				}
			}
			else
			{
				instance.CharacterMapWho.GetObjectsInRect(tileCoordForPos - new TerrainCoord(num3, num3), tileCoordForPos + new TerrainCoord(num3, num3), TempCharacters);
				foreach (Character tempCharacter in TempCharacters)
				{
					if (CanCharacterDodgeSplashDamage(tempCharacter, _source, vector2, num2))
					{
						tempCharacter.OnChanceToDodgeSplashDamage(_source, _startPos, vector2, this);
					}
				}
			}
			TempCharacters.Clear();
		}
		base.ProjectileUpdate(dt);
	}

	public static RaycastResult ParabolicRayCast(Character source, TileObject target, Vector3 startPos, Vector3 targetPos, int options, bool predicted, bool debugDraw)
	{
		float speed = CalcThrowSpeed(source, target);
		float throwAngle = CalcThrowAngle(startPos, targetPos, speed);
		Vector2 horizDir = MathUtil.SafeNormalize(MathUtil.ToXZ(targetPos - startPos), Vector2.zero);
		float t;
		return ParabolicRayCast(source, target, startPos, horizDir, throwAngle, speed, options, predicted, debugDraw, out t);
	}

	public static RaycastResult ParabolicRayCast(Character source, TileObject target, Vector3 startPos, Vector2 horizDir, float throwAngle, float speed, int options, bool predicted, bool debugDraw, out float t)
	{
		using (new UnityProfileMarker(ParabolicRayCastStr))
		{
			Vector3 direction = GetPosAtTime(2f, speed, throwAngle, startPos, horizDir) - startPos;
			float maxHeight = GameTerrain.Instance.GetMaxHeight(new Ray(startPos, direction), direction.magnitude);
			Vector3 vector = startPos;
			t = 0f;
			RaycastResult result = default(RaycastResult);
			while (vector.y > 0f && t < 2f)
			{
				float num = t + 1f / 6f;
				Vector3 posAtTime = GetPosAtTime(num, speed, throwAngle, startPos, horizDir);
				direction = posAtTime - vector;
				float magnitude = direction.magnitude;
				if (magnitude <= 0.0001f)
				{
					return new RaycastResult(new Ray(vector, direction), null, magnitude, Vector3.zero, TerrainCoord.Invalid, Bone.Invalid, Vector3.zero, 0f);
				}
				direction /= magnitude;
				int num2 = options;
				if (Math.Min(vector.y, posAtTime.y) > maxHeight)
				{
					num2 |= 0x80000;
				}
				result = GameTerrain.Instance.RayCast(new Ray(vector, direction), magnitude, num2, source.InsideBuilding, source, target, predicted);
				if (debugDraw)
				{
					DebugGraphics.AddPersistentLine(vector, result.GetHitPosition(), Color.yellow);
				}
				if (result.HitObject != null)
				{
					t = Mathf.Lerp(t, num, result.HitDist / magnitude);
					return result;
				}
				t = num;
				vector = posAtTime;
			}
			return result;
		}
	}

	public virtual void Land(TileObject hitObject, Vector3 dir, Vector3 hitNormal)
	{
		Landed = true;
		if (IsPredicted() && Authoritative != null)
		{
			Authoritative.PredictedHasLanded = true;
		}
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref Proto);
		reflector.AddAfter(ref InfectedWith, 154);
		reflector.Add(ref _startPos);
		reflector.Add(ref _targetPos);
		reflector.Add(ref Position);
		reflector.Add(ref _speed);
		reflector.Add(ref _throwAngle);
		reflector.Add(ref _maxRange);
		reflector.Add(ref Fuel);
		reflector.Add(ref InjuryLocation);
		reflector.Add(ref HitDamage);
		reflector.Add(ref DamageSkillType);
		reflector.AddAfter(ref DamageRadius, 264);
		reflector.Add(ref Assassinate);
		reflector.Add(ref Secret);
		reflector.Add(ref Landed);
		reflector.AddAfter(ref IsFromAI, 322);
	}

	public override void UpdateUnityTransform()
	{
		if (UnityObj != null)
		{
			float t = (float)(PredictedObjectManager.Instance.GetCurrentTime(IsPredicted()) - _startTime).TotalSeconds;
			UnityObj.transform.position = GetPosAtTime(t);
			UnityObj.transform.rotation = GetRotAtTime(t);
			UnityObj.transform.localScale = Vector3.one * Proto.EquippedModelProperties.LocalScale;
		}
	}

	protected void OnHitObjectNonExplosive(TileObject hitObject, Vector3 dir, float hitDamage, Vector3 hitNormal)
	{
		if (hitObject != null && IsPredicted() == hitObject.IsPredicted())
		{
			ApplyHitDamageToObject(hitObject, dir, hitDamage);
		}
		if (!(DamageRadius > 0f) || InfectedWith == InfectionType.None)
		{
			return;
		}
		Vector3 vector = Position + hitNormal * 0.1f;
		List<TileObject> list = new List<TileObject>();
		if (IsPredicted())
		{
			PredictedObjectManager.Instance.GetPredictedObjectsInSphere(new BoundingSphere(vector, DamageRadius), list);
		}
		else
		{
			GameTerrain.Instance.GetObjectsInSphere(new BoundingSphere(vector, DamageRadius), list);
		}
		foreach (TileObject item in list)
		{
			Vector3 boundingBoxCentre = item.GetBoundingBoxCentre();
			Vector3 dir2 = MathUtil.SafeNormalize(boundingBoxCentre - vector, dir);
			if (item == hitObject || IsPredicted() != item.IsPredicted() || (Fuel == 0f && !(item is Character)))
			{
				continue;
			}
			RaycastResult raycastResult = GameTerrain.Instance.RayCast(vector, boundingBoxCentre, 40);
			if (Character.DrawExplosionRaycasts)
			{
				if (raycastResult.HitObject != null && raycastResult.HitObject != item)
				{
					DebugGraphics.AddPersistentLine(vector, raycastResult.GetHitPosition(), Color.yellow);
					DebugGraphics.AddPersistentLine(raycastResult.GetHitPosition(), boundingBoxCentre, Color.red);
				}
				else
				{
					DebugGraphics.AddPersistentLine(vector, boundingBoxCentre, Color.green);
				}
			}
			if (raycastResult.HitObject == null)
			{
				ApplyHitDamageToObject(item, dir2, hitDamage);
			}
		}
	}

	private bool ShouldIgnoreSnowball(Character hitCharacter)
	{
		if (Proto == EquipmentPrototype.Snowball)
		{
			TimeSpan time = Session.Instance.PlayTime - TimeSpan.FromSeconds(_source.DirectControlled ? 10f : 60f);
			if (hitCharacter.HasMemoryAfter(MemoryPrototype.WonSnowballFight, _source, hitCharacter, time) || hitCharacter.HasMemoryAfter(MemoryPrototype.LostSnowballFight, _source, hitCharacter, time))
			{
				return true;
			}
		}
		return false;
	}

	private void ApplyHitDamageToObject(TileObject hitObject, Vector3 dir, float hitDamage)
	{
		if (hitObject is Character character)
		{
			if (hitDamage > 0f)
			{
				float damage = hitDamage;
				character.PickRandomHitPos(InjuryLocation, Id, Pos, out var bone, out var hitPosInBoneSpace);
				character.OnDamaged(_source, _target, Proto.InjuryType, InjuryLocation, InfectedWith, Proto, DamageSkillType, ref damage, Character.BottleHitRadius, Pos, Character.BottleHitForce * dir, bone, hitPosInBoneSpace, dontReact: false, Assassinate, Secret);
			}
			else if (_source != character)
			{
				if (IsAuthoritative() && character is Human && character.IsAwake && !character.Zombie && _source != null)
				{
					Target orCreateTarget = character.GetOrCreateTarget(_source);
					if (_source.IsEnemy(character))
					{
						orCreateTarget.OnAttackedMe(character, _source.Position, forceVisible: true);
					}
					else
					{
						orCreateTarget.UpdateLastKnownInfo(character, _source.Position);
						if (!ShouldIgnoreSnowball(character))
						{
							if (orCreateTarget.FullyTracked)
							{
								Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, _source, this, SpeechSituation.ThrownAt);
								if (speechForSituation != null)
								{
									character.Speak(speechForSituation, _source);
								}
							}
							else
							{
								Speech speechForSituation2 = StoryManager.Instance.GetSpeechForSituation(character, _source, null, SpeechSituation.Scared);
								if (speechForSituation2 != null)
								{
									character.Speak(speechForSituation2, _source);
								}
							}
						}
					}
				}
				character.SetRecentActivity(RecentActivityType.ThrownAt, _source);
				if (_source != null && character.SparringPartner == _source && IsAuthoritative())
				{
					_source.SparringSnowballHits++;
				}
			}
		}
		hitObject.AddFuel(_source, Fuel);
	}
}
