using UnityEngine;

public class AutoDestroyWhenEmptyBehaviour : MonoBehaviour
{
	private void Update()
	{
		if (base.transform.childCount == 0)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
