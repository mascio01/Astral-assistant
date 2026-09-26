using System;
using System.Collections.Generic;

public class PropManager : IReflectable
{
	public enum Bucket
	{
		EveryFrame,
		Rare,
		Count
	}

	public List<Prop> AllProps = new List<Prop>();

	public List<BaseObject>[] ObjectsThatNeedUpdating = new List<BaseObject>[2];

	public int[] Iter = new int[2];

	public List<TriggeredTrap> TriggeredTraps = new List<TriggeredTrap>();

	public List<EnterableVehicle> MovingVehicles = new List<EnterableVehicle>();

	public List<Projectile> Projectiles = new List<Projectile>();

	public PropManager()
	{
		for (int i = 0; i < ObjectsThatNeedUpdating.Length; i++)
		{
			ObjectsThatNeedUpdating[i] = new List<BaseObject>();
		}
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref TriggeredTraps);
	}

	public void Add(Prop prop)
	{
		AllProps.Add(prop);
	}

	public void Remove(Prop prop)
	{
		RemoveFromObjectsThatNeedUpdating(prop);
		AllProps.Remove(prop);
	}

	public void AddToObjectsThatNeedUpdating(BaseObject obj)
	{
		AddToObjectsThatNeedUpdating(obj, Bucket.EveryFrame);
	}

	public void AddToObjectsThatNeedUpdating(BaseObject obj, Bucket bucket)
	{
		if (!ObjectsThatNeedUpdating[(int)bucket].Contains(obj))
		{
			ObjectsThatNeedUpdating[(int)bucket].Add(obj);
		}
	}

	public void RemoveFromObjectsThatNeedUpdating(BaseObject obj)
	{
		RemoveFromObjectsThatNeedUpdating(obj, Bucket.EveryFrame);
	}

	public void RemoveFromObjectsThatNeedUpdating(BaseObject obj, Bucket bucket)
	{
		ObjectsThatNeedUpdating[(int)bucket].Remove(obj);
	}

	public bool HasTriggeredTrap(Character character)
	{
		for (int i = 0; i < TriggeredTraps.Count; i++)
		{
			if (TriggeredTraps[i].Triggerer == character)
			{
				return true;
			}
		}
		return false;
	}

	public void AddTriggeredTrap(Character triggerer, TileObject trap)
	{
		TriggeredTrap item = new TriggeredTrap
		{
			Triggerer = triggerer,
			Trap = trap
		};
		TriggeredTraps.Add(item);
	}

	public void Update(TimeSpan dt)
	{
		for (int i = 0; i < TriggeredTraps.Count; i++)
		{
			if (TriggeredTraps[i].Trap is ITrap trap)
			{
				trap.TriggerTrap(TriggeredTraps[i].Triggerer);
			}
		}
		TriggeredTraps.Clear();
		for (int j = 0; j < ObjectsThatNeedUpdating.Length; j++)
		{
			if (ObjectsThatNeedUpdating[j].Count == 0)
			{
				continue;
			}
			Bucket bucket = (Bucket)j;
			int num = Math.Min(ObjectsThatNeedUpdating[j].Count, (bucket != Bucket.EveryFrame) ? 1 : int.MaxValue);
			int num2 = 0;
			Iter[j] %= ObjectsThatNeedUpdating[j].Count;
			while (num2 < num)
			{
				num2++;
				BaseObject baseObject = ObjectsThatNeedUpdating[j][Iter[j]];
				bool stillNeedUpdating = false;
				if (bucket == Bucket.Rare)
				{
					baseObject.PropUpdateRare(ref stillNeedUpdating);
				}
				else
				{
					baseObject.PropUpdate(dt, ref stillNeedUpdating);
				}
				if (baseObject.PropWantDelete())
				{
					if (baseObject is Equipment equipment)
					{
						equipment.InventoryOwner.GetInventory().Remove(equipment.InventoryOwner, equipment);
					}
					baseObject.Delete();
					if (ObjectsThatNeedUpdating[j].Count == 0)
					{
						break;
					}
				}
				else if (!stillNeedUpdating)
				{
					RemoveFromObjectsThatNeedUpdating(baseObject, bucket);
					if (ObjectsThatNeedUpdating[j].Count == 0)
					{
						break;
					}
				}
				else
				{
					Iter[j]++;
				}
				Iter[j] %= ObjectsThatNeedUpdating[j].Count;
			}
		}
	}
}
