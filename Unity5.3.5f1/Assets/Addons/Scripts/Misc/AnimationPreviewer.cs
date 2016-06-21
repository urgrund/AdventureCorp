using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationPreviewer : MonoBehaviour
{
    public Animation animationComponent;

    void OnGUI()
    {
        if (animationComponent == null)
            return;

 //        for (var state : AnimationState in anim) {
 //    // add name to tmpList
 //    tmpList.Add(state.name);
 //}
        int columns = 3;

        List<AnimationState> states =  new List<AnimationState>();
        foreach (AnimationState s in animationComponent)
        {
            states.Add(s);
        }


        if(states.Count == 0)
            return;

        for(int i=0; i<states.Count;i++)
        {
			int stride = i * columns;
            GUILayout.BeginHorizontal();
            for (int j = 0; j < columns; j++)
            {				
				if (stride + j < states.Count)
                {
					if (GUILayout.Button(states[stride + j].name, GUILayout.Width(100)))
						animationComponent.Play(states[stride + j].name, PlayMode.StopAll);
					
				}
            }
            GUILayout.EndHorizontal();            
        }

        //foreach(AnimationClip c in animationComponent)
        //{
        //    if (GUILayout.Button(c.name))
        //        animationComponent.Play(c.name, PlayMode.StopAll);
        //}
    }		
}
