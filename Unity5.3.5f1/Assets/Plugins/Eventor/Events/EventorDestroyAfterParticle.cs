using UnityEngine;
using System.Collections;

public class EventorDestroyAfterParticle : EventorJob
{
    public new ParticleSystem particleSystem;

    protected override IEnumerator RunJobRoutine()
    {
        if (particleSystem)
            if (particleSystem.IsAlive())
                Destroy(gameObject);
        yield return null;
    }
}
