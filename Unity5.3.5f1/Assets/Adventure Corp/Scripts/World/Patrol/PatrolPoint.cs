using UnityEngine;
using System.Collections;

[System.Serializable]
public class PatrolPoint 
{
	private bool _isBusy = false;
	public bool isBusy { get { return _isBusy; } }
	private NPCBrain _occupier = null;

	private Vector3 _position;
	private PatrolZone _zone;
	public Vector3 position { get { return _position; } }
	public PatrolZone zone { get { return _zone; } }

	public PatrolPoint(Vector3 position, PatrolZone zone)
	{
		this._isBusy = false;
		this._position = position;
		this._zone = zone;
	}

	public void Occupy(NPCBrain occupier)
	{
		this._occupier = occupier;
		_isBusy = true;
	}

	public void Release()
	{
		_occupier = null;
		_isBusy = false;
	}
}
 