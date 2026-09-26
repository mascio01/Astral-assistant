using UnityEngine;

public struct RaycastResult
{
	public Ray Ray;

	public BaseObject HitObject;

	public float HitDist;

	public Vector3 Normal;

	public TerrainCoord Tile;

	public Bone Bone;

	public Vector3 PosInBoneSpace;

	public float PlantCover;

	public RaycastResult(Ray ray, BaseObject hitObject, float hitDist, Vector3 normal, TerrainCoord tile, Bone bone, Vector3 posInBoneSpace, float plantCover)
	{
		Ray = ray;
		HitObject = hitObject;
		HitDist = hitDist;
		Normal = normal;
		Tile = tile;
		Bone = bone;
		PosInBoneSpace = posInBoneSpace;
		PlantCover = plantCover;
	}

	public Vector3 GetHitPosition()
	{
		return Ray.origin + Ray.direction * HitDist;
	}
}
