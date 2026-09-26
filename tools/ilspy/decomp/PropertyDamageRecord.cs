using System;

public struct PropertyDamageRecord : IReflectable
{
	public int PropertyDamageRecordId;

	public TerrainRect Region;

	public TimeSpan LastDamagedTime;

	public float MaxDamagedHeight;

	public bool Investigated;

	public bool HasAssignedBlame;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref PropertyDamageRecordId);
		Region.Reflect(reflector);
		reflector.Add(ref LastDamagedTime);
		reflector.AddAfter(ref MaxDamagedHeight, 494);
		reflector.Add(ref Investigated);
		reflector.Add(ref HasAssignedBlame);
		if (reflector.Version < 323)
		{
			HasAssignedBlame = true;
		}
	}

	public float GetVisibleFromDist(float sightRange)
	{
		return sightRange * MaxDamagedHeight / 8f;
	}
}
