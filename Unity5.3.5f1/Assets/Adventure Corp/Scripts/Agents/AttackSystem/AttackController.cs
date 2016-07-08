using UnityEngine;
using System.Collections;


/// <summary>
/// Main controller that holds the attack descriptors
/// </summary>
public class AttackController : MonoBehaviour
{    
    public Animation animatedObject;
    public Agent agent;
    public EventorSchedule[] schedules;
    public Damager[] damagers;

    public AttackDescriptor[] attacks;

     
    void Start()
    {
        if (agent == null)
            agent = GetComponent<Agent>();
        if (animatedObject == null)
            animatedObject = agent.animatedGameObject;

        damagers = AttackVolumeCollection.CreateDamageCollidersForTransform(animatedObject.transform, agent.properties.GetComponent<AttackVolumeCollection>());
    }


    void AttackWithDescriptor(AttackDescriptor d)
    {
        StopCoroutine(RunJobRoutine(d));
        StartCoroutine(RunJobRoutine(d));        
    }


    // Istantiate an EventorSchedule for use as an 
    // attack.   Setup needed properties and run!
    void AttackWithEventor(EventorSchedule attack)
    {
        attack = Instantiate(attack) as EventorSchedule;
        Helpers.ParentAndCenterOnTransform(attack.transform, this.transform);
        attack.isDestroyOnComplete = true;
        EventorAttackDescriptor attackDesc;
        for (int i = 0; i < attack.jobs.Count; i++)
        {
            if (attack.jobs[i].GetType() == typeof(EventorAttackDescriptor))
            {
                attackDesc = attack.jobs[i] as EventorAttackDescriptor;
                attackDesc.animatedObject = animatedObject;
                attackDesc.controller = this;
            }
        }
        attack.Run();
    }



    // Apply the appropriate Damage object to the volumes    
    public void SetDamageToDamageVolumes(Damage damage)
    {
        for (int i = 0; i < damagers.Length; i++)
            damagers[i].damage = damage;
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



    IEnumerator RunJobRoutine(AttackDescriptor attack)
    {
        // Setup volumes
        SetDamageToDamageVolumes(attack.damage);
        ActivateDamageVolumes(false, attack.volumeIndices);

        float t;
        // Play animation and activate volumes during damage range
        if (animatedObject != null && attack != null)
        {
            animatedObject.CrossFade(attack.clipProperties.clip.name, attack.clipProperties.blendTime, attack.clipProperties.playMode);
            while (animatedObject.IsPlaying(attack.clipProperties.clip.name))
            {
                t = animatedObject[attack.clipProperties.clip.name].normalizedTime;
                ActivateDamageVolumes(t > attack.validDamageRange.x && t < attack.validDamageRange.y, attack.volumeIndices);
                yield return null;
            }
        }
        else
            Debug.LogWarning("Attack had no animated object or attack descriptor");

        // Deactivate
        ActivateDamageVolumes(false, attack.volumeIndices);
    }







    void OnGUI()
    {
        if (schedules == null || attacks == null)
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
