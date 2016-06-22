using UnityEngine;
using System.Collections;

public class LogicCounter : TriggerLogicBase    
{
    public int count = 1;
    private int _count = 0;
    
    public Trigger[] sourceTriggers;

    void Awake()
    {
        if (sourceTriggers == null)
            return;

        if (sourceTriggers.Length == 0)
            return;

        // Gather all the source triggers 
        for (int i = 0; i < sourceTriggers.Length; i++)
        {
            sourceTriggers[i].onTriggerEntered += OnSourceTriggerd;     
        }
    }

    void OnSourceTriggerd()
    {
        if (_isValid)
            return;

        _count++;
        Debug.Log("counter tick = " + _count.ToString() + "/" + count.ToString());
    }

    public override void Validate()
    {
        if(!_isValid)
            _isValid = _count == count;
        base.Validate();
    }

    void OnDrawGizmos()
    {
        if (sourceTriggers != null)
        {
            Gizmos.color = Color.yellow;
            // TODO - check if the transform is null
            for (int i = 0; i < sourceTriggers.Length; i++)
                Gizmos.DrawLine(transform.position, sourceTriggers[i].transform.position);
        }
    }
}
