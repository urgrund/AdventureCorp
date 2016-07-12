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

    void Awake()
    {
        if (agent == null)
            agent = GetComponent<Agent>();
        if (animationController == null)        
            if (GetComponent<AgentAnimationController>())
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
        _isAttacking = false;
        animationController.overrideCountDown = -1;
    }


    IEnumerator RunJobRoutine(AttackDescriptor attack)
    {
        // Setup volumes
        SetDamageToDamageVolumes(attack.damage);
        ActivateDamageVolumes(false, attack.volumeIndices);
        _currentAttack = attack;

        float t;
        // Play animation and activate volumes during damage range
        if (animationController.animatedGameObject != null && _currentAttack != null)
        {            
            animationController.Play(_currentAttack.clipProperties);
            while (animationController.animatedGameObject.IsPlaying(_currentAttack.clipProperties.clip.name))
            {
                _isAttacking = true;
                t = animationController.animatedGameObject[_currentAttack.clipProperties.clip.name].normalizedTime;
                ActivateDamageVolumes(t > _currentAttack.validDamageRange.x && t < _currentAttack.validDamageRange.y, _currentAttack.volumeIndices);
                agent.isAllowedMovement = !_currentAttack.lockController;
                yield return null;
            }         
        }
        else
            Debug.LogWarning("Attack had no animated object or attack descriptor");

        ReleaseAttack(_currentAttack);
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
                Gizmos.DrawCube(d.transform.TransformPoint(c.center), Vector3.one * c.radius * 0.5f);
            }
        }
    }
}
