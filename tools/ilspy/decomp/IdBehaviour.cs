using UnityEngine;

public class IdBehaviour : MonoBehaviour
{
	public int Id;

	public static int GetIdFromUnityObject(GameObject unityObj)
	{
		while (unityObj != null)
		{
			IdBehaviour component = unityObj.GetComponent<IdBehaviour>();
			if (component != null)
			{
				return component.Id;
			}
			unityObj = ((unityObj.transform.parent != null) ? unityObj.transform.parent.gameObject : null);
		}
		return 0;
	}
}
