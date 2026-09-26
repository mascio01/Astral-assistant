using System.Collections.Generic;
using UnityEngine;

public class CollisionManager
{
	protected List<Character> _movers = new List<Character>();

	protected List<Character> _moversProcessing = new List<Character>();

	public List<Character> UpdatedThisFrame = new List<Character>();

	private List<TileObject> _nearbyObjects = new List<TileObject>();

	private static GameProfiler ProcessMoversTimer = new GameProfiler("Update.ProcessMovers");

	public void AddToMovers(Character character)
	{
		if (!_movers.Contains(character))
		{
			_movers.Add(character);
		}
	}

	public void RemoveFromMovers(Character character)
	{
		_movers.Remove(character);
	}

	public void ClearAllMovers()
	{
		_movers.Clear();
	}

	public static Vector2 TraceCollisionsIfLargeStep(Character character, Vector3 pos, Vector2 velXZ, out TileObject hitObject)
	{
		Vector2 result = MathUtil.ToXZ(pos) + velXZ;
		hitObject = null;
		float sqrMagnitude = velXZ.sqrMagnitude;
		if (sqrMagnitude >= 0.20249999f)
		{
			float num = Mathf.Sqrt(sqrMagnitude);
			int options = 0x100 | ((character.CurrentActionAnim == ActionAnim.Vault) ? 512 : 0);
			result = MathUtil.ToXZ(GameTerrain.Instance.TraceCollisions(new Ray(pos, MathUtil.ToX0Y(velXZ / num)), num, options, character, character.RagdollIgnoreBuilding, out hitObject));
		}
		return result;
	}

	public void ProcessMovers(int maxIterations, bool predicted)
	{
		using (new ProfileMarker(ProcessMoversTimer))
		{
			GameTerrain instance = GameTerrain.Instance;
			Vector3 vector = new Vector3(Character.HumanRadius, 0f, Character.HumanRadius) * 2f;
			foreach (Character item in UpdatedThisFrame)
			{
				item.VelocityXZBeforeCollisionDetection = item.VelocityXZ;
				item.OldPosition = item.Position;
			}
			UpdatedThisFrame.Clear();
			for (int i = 0; i < maxIterations; i++)
			{
				List<Character> moversProcessing = _moversProcessing;
				_moversProcessing = _movers;
				_movers = moversProcessing;
				foreach (Character item2 in _moversProcessing)
				{
					TileObject hitObject;
					Vector2 vector2 = TraceCollisionsIfLargeStep(item2, item2.Position, item2.VelocityXZ, out hitObject);
					item2.ClearVelocityXZ();
					item2.SetPosition(vector2.x, vector2.y);
					item2.OnMovedByCollisionManager();
					if (hitObject != null)
					{
						item2.OnCollisionWithProp(hitObject);
					}
				}
				foreach (Character item3 in _moversProcessing)
				{
					if (item3.WantKilledByPitTrap)
					{
						continue;
					}
					Vector2 posXZ = item3.PosXZ;
					TerrainCoord tileCoordForPos = instance.GetTileCoordForPos(item3.Position - vector);
					TerrainCoord tileCoordForPos2 = instance.GetTileCoordForPos(item3.Position + vector);
					if (item3.IsAwake && item3.CarriedBy == null)
					{
						if (predicted)
						{
							foreach (TileObject predictedObject in PredictedObjectManager.Instance.PredictedObjects)
							{
								if (predictedObject is Character otherCharacter)
								{
									RepulseCharacter(item3, otherCharacter, ref posXZ);
								}
							}
						}
						else
						{
							instance.GetObjectsOfTypeInRect(tileCoordForPos, tileCoordForPos2, _nearbyObjects, typeof(Character));
							foreach (TileObject nearbyObject in _nearbyObjects)
							{
								Character otherCharacter2 = nearbyObject as Character;
								RepulseCharacter(item3, otherCharacter2, ref posXZ);
							}
						}
					}
					TerrainCoord terrainCoord = TerrainCoord.Invalid;
					Vector2 vector3 = Vector2.zero;
					float num = 0f;
					for (int j = tileCoordForPos.x; j <= tileCoordForPos2.x; j++)
					{
						for (int k = tileCoordForPos.y; k <= tileCoordForPos2.y; k++)
						{
							bool isRound = false;
							int num2 = 8448;
							if (item3.CurrentActionAnim == ActionAnim.Vault)
							{
								num2 |= 0x4200;
							}
							if (instance.IsImpassable(j, k, num2, item3, item3.RagdollIgnoreBuilding, out isRound))
							{
								TerrainCoord terrainCoord2 = new TerrainCoord(j, k);
								Vector2 tileCentreXZ = instance.GetTileCentreXZ(terrainCoord2);
								Vector2 normal;
								float penetration;
								if (isRound)
								{
									Vector2 vector4 = posXZ - tileCentreXZ;
									float magnitude = vector4.magnitude;
									normal = ((magnitude > 0.0001f) ? (vector4 / magnitude) : MathUtil.ToXZ(-item3.Forward));
									penetration = item3.Radius + TreeProp.Radius - magnitude;
									if (penetration > num)
									{
										terrainCoord = terrainCoord2;
										vector3 = normal;
										num = penetration;
									}
								}
								else if (MathUtil.DoesCircleIntersectRect(posXZ, item3.Radius, tileCentreXZ - new Vector2(0.5f, 0.5f), tileCentreXZ + new Vector2(0.5f, 0.5f), out normal, out penetration) && penetration > num)
								{
									terrainCoord = terrainCoord2;
									vector3 = normal;
									num = penetration;
								}
							}
							else
							{
								if (!instance.IsSlope(j, k) || item3.CurrentActionAnim == ActionAnim.Slide)
								{
									continue;
								}
								TerrainCoord terrainCoord3 = new TerrainCoord(j, k);
								Vector2 tileCentreXZ2 = instance.GetTileCentreXZ(terrainCoord3);
								if (MathUtil.DoesCircleIntersectRect(posXZ, item3.Radius, tileCentreXZ2 - new Vector2(0.5f, 0.5f), tileCentreXZ2 + new Vector2(0.5f, 0.5f), out var normal2, out var penetration2) && penetration2 > num)
								{
									Vector2 vector5 = posXZ - normal2 * item3.Radius;
									if (Vector2.Dot(MathUtil.ToXZ(instance.GetNormalAtPos(vector5.x, vector5.y)), item3.VelocityXZBeforeCollisionDetection) <= 0f)
									{
										terrainCoord = terrainCoord3;
										vector3 = normal2;
										num = penetration2;
									}
								}
							}
						}
					}
					if (terrainCoord != TerrainCoord.Invalid)
					{
						item3.AddVelocityXZ(vector3 * num * 1.001f, setFollowMeDir: false);
						TileObject fixedObjectOnTile = instance.GetFixedObjectOnTile(terrainCoord.x, terrainCoord.y);
						if (fixedObjectOnTile != null)
						{
							item3.OnCollisionWithProp(fixedObjectOnTile);
						}
					}
				}
				_moversProcessing.Clear();
				if (_movers.Count == 0)
				{
					break;
				}
			}
			while (_movers.Count > 0)
			{
				_movers[0].ClearVelocityXZ();
			}
			_nearbyObjects.Clear();
		}
	}

	private void RepulseCharacter(Character character, Character otherCharacter, ref Vector2 posXZ)
	{
		if (otherCharacter == character || !otherCharacter.IsAwake || otherCharacter.CarriedBy != null || (character.InteractionObject != null && character.InteractionObject.GetAuthoritativeOrElseThis() == otherCharacter.GetAuthoritativeOrElseThis()) || (character.CurrentActionAnim == ActionAnim.Vault && otherCharacter.CurrentActionAnim == ActionAnim.Vault) || ((character.IsDodging() || character.CanAttackJumpingZombie != CanAttackState.None) && otherCharacter.IsZombieJumping()) || ((otherCharacter.IsDodging() || otherCharacter.CanAttackJumpingZombie != CanAttackState.None) && character.IsZombieJumping()))
		{
			return;
		}
		Vector2 vector = otherCharacter.PosXZ - posXZ;
		float sqrMagnitude = vector.sqrMagnitude;
		float num = character.Radius + otherCharacter.Radius;
		if (sqrMagnitude < num * num)
		{
			float num2 = Mathf.Sqrt(sqrMagnitude);
			Vector2 vector2 = ((num2 > 0.001f) ? (vector / num2) : MathUtil.ToXZ(character.Forward));
			float num3 = (num - num2) * 1.001f;
			if (_moversProcessing.Contains(otherCharacter))
			{
				character.AddVelocityXZ(-vector2 * num3 * 0.5f, setFollowMeDir: false);
			}
			else
			{
				character.AddVelocityXZ(-vector2 * num3, setFollowMeDir: false);
			}
			character.OnCollisionWithCharacter(otherCharacter);
		}
	}
}
