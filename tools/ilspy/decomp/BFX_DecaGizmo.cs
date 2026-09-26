using UnityEngine;

public class BFX_DecaGizmo : MonoBehaviour
{
	private Transform t;

	private void OnDrawGizmos()
	{
		if (t == null)
		{
			t = base.transform;
		}
		Gizmos.color = new Color(0.19215687f, 8f / 15f, 1f, 0.15f);
		Gizmos.matrix = Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);
		Gizmos.DrawCube(Vector3.zero, Vector3.one);
		Gizmos.color = new Color(0.19215687f, 8f / 15f, 1f, 0.95f);
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		Gizmos.matrix = Matrix4x4.identity;
		Gizmos.color = new Color(0.95f, 0.2f, 0.2f, 0.85f);
		Gizmos.DrawRay(t.position, t.up);
	}
}
