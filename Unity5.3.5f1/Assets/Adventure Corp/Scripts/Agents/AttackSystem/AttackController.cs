using UnityEngine;
using System.Collections;


/// <summary>
/// Main controller that holds the attack descriptors
/// </summary>
public class AttackController : MonoBehaviour
{
    public bool showDebugGUI = false;
    public AgentAnimationController animationController;
    public Agent agent;
    //public EventorSchedule[] schedules;
    //[HideInInspector]
    public Damager[] damagers;

    public AttackDescriptor[] attacks;
    public AttackDescriptor _currentAttack;

    private bool _isAttacking = false;
    public bool isAttacking { get { return _isAttacking; } }

    private bool _isControllingAgentVelocity = false;
    public bool isControllingAgentVelocity { get { return _isControllingAgentVelocity; } }

    void Awake()
    {
        if (agent == null)
            agent = GetComponent<Agent>();
        if (animationController == null)        
            animationController = GetComponent<AgentAnimationController>();
        if (agent == null || animationController == null)
            Debug.LogError("No agent or animated object on " + this.name);

        damagers = AttackVolumeCollection.CreateDamageCollidersForTransform(animationController.animatedGameObject.transform, agent.properties.GetComponent<AttackVolumeCollection>());
    }


    public bool isPastYieldControlTime
    {
        get
        {
            if (isAttacking)
                return animationController.animatedGameObject[_currentAttack.clipProperties.clip.name].normalizedTime > _currentAttack.yieldControlRatio;
            else
                return true;
        }
    }

    public void AttackWithDescriptor(AttackDescriptor d)
    {
        if (isPastYieldControlTime)
        {
            StopAllCoroutines();
            StartCoroutine(RunJobRoutine(d));
            if (d.eventor != null)
                EventorSchedule.RunAtTransformAsChild(d.eventor, this.transform);
        }
    }
    

    // Apply the appropriate Damage object to the volumes    
    public void SetDamageToDamageVolumes(Damage damage)
    {
        for (int i = 0; i < damagers.Length; i++)
            damagers[i].damage = damage;
    }


    
    public void SetOwnerHealthToDamageVolumes(Health health)
    {        
        for (int i = 0; i < damagers.Length; i++)
            damagers[i].owner = health;        
    }

    // This assumes the attack indices match the NPC's damager indices
    // If there's a mistmatch it's most likely that the attack descriptor
    // was never opened in the editor and initialized
    public void ActivateDamageVolumes(bool isEnabled, bool[] volumeIndices)
    {
        for (int i = 0; i < damagers.Length; i++)
        {
            if (isEnabled)
                damagers[i].enabled = volumeIndices[i];
            else
                damagers[i].enabled = false;
            damagers[i].sphereCollider.enabled = damagers[i].enabled;
        }
    }

    public void YieldControlFromAttack() { YieldControlFromAttack(false); }
    public void YieldControlFromAttack(bool force)
    {
        if (force)
        {
            ReleaseAttack(_currentAttack);
        }
        else
        {
            if (isAttacking)
            {
                if (isPastYieldControlTime)
                    ReleaseAttack(_currentAttack);
            }
        }
    }

    void ReleaseAttack(AttackDescriptor attack)
    {
        // Deactivate
        ActivateDamageVolumes(false, attack.volumeIndices);
        agent.isAllowedMovement = true;
        agent.isApplyGravity = true;
        _isAttacking = false;
        _isControllingAgentVelocity = false;
        
        animationController.overrideCountDown = -1;
    }


    IEnumerator RunJobRoutine(AttackDescriptor attack)
    {
        // Setup volumes
        SetDamageToDamageVolumes(attack.damage);
        ActivateDamageVolumes(false, attack.volumeIndices);
        _currentAttack = attack;

        if (attack.controllerLock == AttackDescriptor.Lock.Curves)
            curveLastPosition = GetPositionOnCurve(_currentAttack, 0f);

        float t;
        // Play animation and activate volumes during damage range
        if (animationController.animatedGameObject != null && _currentAttack != null)
        {            
            animationController.Play(_currentAttack.clipProperties);
            while (animationController.animatedGameObject.IsPlaying(_currentAttack.clipProperties.clip.name))
            {
                // Activate volumes during the attack
                _isAttacking = true;
                t = animationController.animatedGameObject[_currentAttack.clipProperties.clip.name].normalizedTime;
                ActivateDamageVolumes(t > _currentAttack.validDamageRange.x && t < _currentAttack.validDamageRange.y, _currentAttack.volumeIndices);


                // Control over agent if needed
                if (attack.controllerLock != AttackDescriptor.Lock.None)
                {
                    _isControllingAgentVelocity = true;
                    switch (attack.controllerLock)
                    {
                        case AttackDescriptor.Lock.StopAllMovement: agent.isAllowedMovement = false; break;
                        case AttackDescriptor.Lock.ScaleMovementHalf: agent.SetVelocityScaleThisFrame(0.50f); break;
                        case AttackDescriptor.Lock.ScaleMovementQuarter: agent.SetVelocityScaleThisFrame(0.25f); break;
                        case AttackDescriptor.Lock.Curves: MoveAgentWithAttackCurves(_currentAttack, t); break;
                    }
                }
            
                yield return null;
            }         
        }
        else
            Debug.LogWarning("Attack had no animated object or attack descriptor");

        ReleaseAttack(_currentAttack);
    }


    // Declare here so don't need to declare each frame
    private Vector3 curvePosition;
    private Vector3 curveLastPosition;
    private Vector3 curveDirection;
    void MoveAgentWithAttackCurves(AttackDescriptor attack, float normalizedTime)
    {
        agent.isApplyGravity = false;
        curvePosition = GetPositionOnCurve(attack, normalizedTime);
        curveDirection = agent.transform.TransformDirection(curvePosition - curveLastPosition);

        if(curveDirection.magnitude > 0)
            agent.OverrideMove(curveDirection);

        curveLastPosition = curvePosition;
        //Debug.Log("  dir:" + curveDirection+ "   lP:" + curveLastPosition + "  cP:" + curvePosition);
    }

    Vector3 GetPositionOnCurve(AttackDescriptor attack, float normalizedTime)
    {
        curvePosition = Vector3.zero;
        curvePosition.x = attack.curveX.Evaluate(normalizedTime);
        curvePosition.y = attack.curveY.Evaluate(normalizedTime);
        curvePosition.z = attack.curveZ.Evaluate(normalizedTime);
        return ( curvePosition );
    }




    void OnGUI()
    {
        if (!showDebugGUI)
            return;

        //if (schedules == null || attacks == null)
        //  return;

        if (attacks == null)
            return;

        foreach (AttackDescriptor s in attacks)
            if (GUILayout.Button(s.name))
                AttackWithDescriptor(s);

        //foreach (EventorSchedule s in schedules)
        //    if (GUILayout.Button(s.name))
        //        AttackWithEventor(s);
    }

    void OnDrawGizmos()
    {
        if (damagers == null)
            return;

        foreach (Damager d in damagers)
        {
            if (d.GetComponent<SphereCollider>())
            {                
                SphereCollider c = d.GetComponent<SphereCollider>();
                Gizmos.color = d.enabled ? Color.red : Color.grey;
                Gizmos.DrawWireSphere(d.transform.TransformPoint(c.center), c.radius);
                //Gizmos.DrawCube(d.transform.TransformPoint(c.center), Vector3.one * c.radius * 0.5f);
            }
        }
    }
}
