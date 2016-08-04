using UnityEngine;
using System.Collections;

public class TestSizeUp : NPCBrain
{

    protected override IEnumerator LogicRoutine()
    {
        if (state == State.SizeUp)
            yield return null;

        state = State.SizeUp;

        yield return null;
    }
}
