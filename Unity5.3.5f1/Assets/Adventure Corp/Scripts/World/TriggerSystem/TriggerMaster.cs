using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Trigger Master
/// Holds referecnes to a number of TriggerLogic components 
/// and will fire off events when all components are valid 
/// </summary>
public class TriggerMaster : MonoBehaviour
{
    public EventorSchedule eventToRunOnValid;
    public EventorSchedule eventToInstantiateOnValid;

    public List<TriggerLogicBase> triggerConditions = new List<TriggerLogicBase>();
    public bool _isAllConditionsMet = false;
    public bool isAllConditionsMet { get { return _isAllConditionsMet; } }

    void Awake()
    {
        triggerConditions = new List<TriggerLogicBase>(GetComponents<TriggerLogicBase>());
        StartCoroutine(CheckUpdate());
    }

    bool CheckConditionsMet()
    {
        return triggerConditions.TrueForAll(e => e.isValid);
    }

    IEnumerator CheckUpdate()
    {

        // COntinue to check if all components 
        // are valid.  This could be periodic (once per second?)
        _isAllConditionsMet = false;
        while (!CheckConditionsMet())
        {
            foreach (TriggerLogicBase t in triggerConditions)
                t.Validate();
            yield return null;
        }


        // All valid, fire off events and we're done
        _isAllConditionsMet = true;

        if (eventToRunOnValid != null)
            eventToRunOnValid.Run();

        if (eventToInstantiateOnValid != null)
            EventorSchedule.Run(eventToInstantiateOnValid);

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(transform.position, Vector3.one * 0.5f);

        Gizmos.color = Color.cyan;
        if (eventToRunOnValid != null)
            Gizmos.DrawLine(transform.position, eventToRunOnValid.transform.position);

    }
}
