using UnityEngine;
using System.Collections;

[System.Serializable]
public class EventorJobObjectDestroyDetails : EventorJobDetailsBase
{
	public float destroyDelay = 1f;
	
	public Transform SpawnAndDestroy(Transform t) { return SpawnAndDestroy(t, t.position, t.rotation); }
	public Transform SpawnAndDestroy(Transform t, Vector3 p, Quaternion q)
	{
		Transform tf = (GameObject.Instantiate(t, p, q) as Transform);
		GameObject.Destroy( tf.gameObject, destroyDelay);
		return tf;
	}
}

[System.Serializable]
public class EventorJobRepeatDetails : EventorJobDetailsBase
{
	public int count = 1;
	public Vector2 repeatDelay = new Vector2(1,2);
	public float randomRepeatFromDelay { get { return Random.Range (repeatDelay.x, repeatDelay.y); } }
}

[System.Serializable]
public class EventorJobReactivateDetails : EventorJobDetailsBase
{
	public int count;
}

public class EventorJobDetailsBase
{
	public bool isActive;
}
