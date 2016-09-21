using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Trigger : MonoBehaviour
{
	[Header("Register Collider Hits")]
    public bool onEnter;
    public bool onStay;
    public bool onExit;

    public LayerMask mask;	

	void Reset()
	{
		if (GetComponent<Collider>() == null)
		{
			this.gameObject.AddComponent<BoxCollider>();
			GetComponent<BoxCollider>().isTrigger = true;
		}
	}

	public enum CollideEvent
	{
		Enter,
		Exit,
		Stay
	}

	[Header("Colliders to Ignore")]
	public List<Transform> collidersToIgnore;


	[Header("Eventors To Fire")]
	public List<EventorSchedule> onEnterSchedules;
    public List<EventorSchedule> onStaySchedules;
    public List<EventorSchedule> onExitSchedules;

    public delegate void OnTriggerEvent();
    public event OnTriggerEvent onTriggerEntered;
    public event OnTriggerEvent onTriggerExit;

    public delegate void OnColliderCountChanged(int currentCount);
    public event OnColliderCountChanged colliderCountChanged;

	int _collidersInsideVolume = 0;
	List<Collider> _colliderComponentInsideVolume = new List<Collider>();
    public int collidersInsideVolume { get { return _collidersInsideVolume; } }


	void Awake()
	{
		foreach (Transform t in collidersToIgnore)
		{
			if(t.GetComponent<Collider>() != null && GetComponent<Collider>() !=null)
				Physics.IgnoreCollision(this.GetComponent<Collider>(), t.GetComponent<Collider>());
			foreach(Collider c in t.GetComponentsInChildren<Collider>())
				Physics.IgnoreCollision(this.GetComponent<Collider>(), c);
		}
	}

    private void AddCollidersInsideVolume(int count)
    {
		_collidersInsideVolume += count;
		if (colliderCountChanged != null)
            colliderCountChanged(_collidersInsideVolume);
    }


    private void FireEvents(List<EventorSchedule> messages)
    {        
        if (messages == null)
            return;        

        if (messages.Count > 0)
        {
            for (int i = 0; i < messages.Count; i++)
                if (messages[i] != null)
                    messages[i].Run();
        }
    }

    bool IsSameLayerAsMask(Collider other) { return ((mask.value & 1 << other.gameObject.layer) == 1 << other.gameObject.layer); }

    void OnTriggerEnter(Collider other)
    {
		if (other.transform == this)
			return;	

        if (onEnter && IsSameLayerAsMask(other))
        {
			FireEvents(onEnterSchedules);

            if (onTriggerEntered != null)
                onTriggerEntered();

			_colliderComponentInsideVolume.Add(other);

			AddCollidersInsideVolume(1);
        }
    }

    void OnTriggerStay(Collider other)
    {
		if (other.transform == this)
			return;

		if (other.transform.parent == this)
			return;

		if (onStay && IsSameLayerAsMask(other))
        {
            FireEvents(onStaySchedules);
        }
    }

    void OnTriggerExit(Collider other)
    {
		if (other.transform == this)
			return;

		if (other.transform.parent == this)
			return;

		if (onExit && IsSameLayerAsMask(other))
        {
            FireEvents(onExitSchedules);

            if (onTriggerExit != null)
                onTriggerExit();

			_colliderComponentInsideVolume.Remove(other);

			AddCollidersInsideVolume(-1);
        }
    }


    void DrawSchedules(List<EventorSchedule> messages, Color color, float alpha)
    {
        if (messages == null)
            return;

        Gizmos.color = new Color(color.r, color.g, color.b, alpha);
        foreach (EventorSchedule e in messages)
            if(e != null)
                Gizmos.DrawLine(transform.position, e.transform.position);
    }



    

    void OnDrawGizmosSelected()
    {
        DrawSchedules(onEnterSchedules, Color.yellow, 0.25f);
    }

}
