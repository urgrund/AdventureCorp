using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TriggerMaster))]
public abstract class TriggerLogicBase : MonoBehaviour
{
    public bool _isValid = false;
    public bool isValid { get { return _isValid;} }
    public bool isNOT = false;

    public virtual void Validate() { } 
}
