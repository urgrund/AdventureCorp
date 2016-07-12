using UnityEngine;
using System.Collections;

public class EventorInstantiate : EventorJob
{
	public bool isInstantiateAtTarget = false;
	public bool isChildToTarget = false;
	public Transform objectToSpawn;
	public Transform parentTarget = null;

	public Vector3 randomOffset = Vector3.zero;

	public EventorJobObjectDestroyDetails destroyObject;
	public EventorJobRepeatDetails repeat;

	private Transform currentTransformSpawned = null;

	private void InstantiateObject()
	{
		if(objectToSpawn != null)
		{
			Vector3 position;
			Quaternion rotation;

			if(isInstantiateAtTarget)
			{
				position = parentTarget.position;
				rotation = parentTarget.rotation;
			}
			else
			{
				position = objectToSpawn.position;
				rotation = objectToSpawn.rotation;
			}

            if (randomOffset != Vector3.zero)
            {
                position += new Vector3(Random.Range(-Mathf.Abs(randomOffset.x), Mathf.Abs(randomOffset.x)),
                                        Random.Range(-Mathf.Abs(randomOffset.y), Mathf.Abs(randomOffset.y)),
                                        Random.Range(-Mathf.Abs(randomOffset.z), Mathf.Abs(randomOffset.z)));
            }		

			if(destroyObject.isActive)
				currentTransformSpawned = destroyObject.SpawnAndDestroy(objectToSpawn, position, rotation);
			else
				currentTransformSpawned = Instantiate(objectToSpawn, position, rotation) as Transform;

			if(isChildToTarget && parentTarget != null)
				currentTransformSpawned.parent = parentTarget;
		}
	}


	protected override IEnumerator RunJobRoutine ()
	{
		if(repeat.isActive)
		{
			for(int i=0; i< repeat.count; i++)
			{
				InstantiateObject();

				// Only wait if this ISN'T the last time
				if(!(i == repeat.count - 1))					
					yield return new WaitForSeconds(repeat.randomRepeatFromDelay);
			}
		}
		else
			InstantiateObject();
	}


	void OnDrawGizmos()
	{
		Color a = new Color(1, 1, 1, 0.1f);        

		// Get position depending if 
		// local to the job or the object
		Vector3 p = transform.position;
		if(!isInstantiateAtTarget)
		{
			if(objectToSpawn != null)
				p = objectToSpawn.position;
		}
		else
		{
			if(parentTarget != null)
				p = parentTarget.position;
		}

		// Setup to draw 
		Gizmos.matrix = transform.localToWorldMatrix;
		Vector3 pos = transform.InverseTransformPoint(p);

		if (isActive)
		{
			Gizmos.color = Color.green * a;
			Gizmos.DrawCube(pos, randomOffset * 2f);
			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(pos, randomOffset * 2f);
		}
		else
		{
			Gizmos.color = Color.grey * a;
			Gizmos.DrawWireCube(pos, randomOffset * 2f);
		}
	}
}























