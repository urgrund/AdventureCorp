using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;


public class EventorSlider
{
	public const float kNodeSize = 130.0f;	
	public const float kNodeHeight = 16f;
	
	static EventorSlider selection = null;
	
	public const float lastRectOffset = 0;//10f;
	
	Vector2 position;
	Rect nodeRect;		
	
	public EventorJob job;
	public Rect lastRect;
	public EventorScheduleEditorWindow window;
	
	public EventorSlider (EventorJob job)
	{
		//Position = position;
		this.job = job;
	}
	
	
	public static EventorSlider Selection
	{
		get
		{
			return selection;
		}
		set
		{
			selection = value;
		}
	}
	
	public void SetPositionFromLastRect()
	{
		FixXPositionToSlider ();
		Position = position;
	}
	
	void FixXPositionToSlider()
	{
		// Get from the job the x pos
		float jX = job.initialDelay * EventorScheduleEditorWindow.pixelsPerSecond;
		position.x = lastRect.xMax + lastRectOffset + jX;
		
		// Clamp to not slide too far!
		if(position.x < lastRect.xMax + lastRectOffset)
			position.x = lastRect.xMax + lastRectOffset;
	}
	
	public Vector2 Position
	{
		get
		{
			return position;
		}
		set
		{
			position = value;
			
			// Clamp to not slide too far!
			if(position.x < lastRect.xMax + lastRectOffset)
				position.x = lastRect.xMax + lastRectOffset;
			
			nodeRect = new Rect (
				position.x, 
				lastRect.yMax,
				kNodeSize,
				kNodeHeight
				);
		}
	}
	
	
	float deltaMouseDown = 0f;
	public void OnGUI ()
	{
		switch (Event.current.type)
		{
		case EventType.mouseDown:
			if (nodeRect.Contains (Event.current.mousePosition))
				// Select this node if we clicked it
			{
				selection = this;	
				deltaMouseDown = Event.current.mousePosition.x - nodeRect.x;
				//Debug.Log("Selected!" + job.GetType());
				Event.current.Use ();
			}
			break;
			
		case EventType.mouseUp:
			
			// If we released the mouse button...
			if (selection == null)
				// ... with no active selection, ignore the event
			{
				break;
			}
			else if (selection == this)
				// ... while this node was active selection...
			{
				//Debug.Log("released" + job.GetType());
				Selection = null;
				Event.current.Use ();
				
			}
			
			break;
			
		case EventType.mouseDrag:
			
			// If doing a mouse drag with this component selected...
			if (selection == this)
			{	
				
				Vector2 p;
				
				// Snap if holding CTRL									
				if(Event.current.control)
				{
					p = Event.current.mousePosition;
					p.y = Position.y;
					
					float snap = EventorScheduleEditorWindow.pixelsPerSecond / 10f;
					float t = Mathf.Round((p.x - EventorScheduleEditorWindow.inspectorWidth - deltaMouseDown) / snap) / 10f;
					job.initialDelay = t;										
					
					Position = p;
				}
				else
				{
					p = Event.current.delta;
					p.y = 0;
					Position += p; 
					job.initialDelay = (position.x - lastRect.xMax - lastRectOffset) / EventorScheduleEditorWindow.pixelsPerSecond;
				}
				
				job.initialDelay = Mathf.Max(job.initialDelay, 0);
				
				
				Event.current.Use ();			
			}
			break;
			
		case EventType.repaint:
			
			DrawSlider();
			
			break;
		}
	}
	
	void DrawRepeats(EventorJobRepeatDetails r)
	{
		if(r == null)
			return;
		
		if(!r.isActive)
			return;
		
		float avg = Mathf.Abs((r.repeatDelay.x + r.repeatDelay.y) * 0.5f) * EventorScheduleEditorWindow.pixelsPerSecond;
		float earliest = r.repeatDelay.x * r.count * EventorScheduleEditorWindow.pixelsPerSecond;
		float latest = r.repeatDelay.y * r.count * EventorScheduleEditorWindow.pixelsPerSecond;
		float endingSpace = Mathf.Abs(latest - earliest);
		
		Color c = GUI.color;
		GUI.color = Color.red;
		
		Rect avgEndRect = new Rect(nodeRect);
		avgEndRect.xMin += earliest;
		//		avgEndRect.xMax += latest;
		avgEndRect.width = endingSpace;
		//		avgEndRect.center += new Vector2(endingSpace, 0);
		//GUI.Box(avgEndRect, "Repeat End", EditorStyles.miniLabel);
		GUI.Box(avgEndRect, "");
		
		Handles.BeginGUI();
		
		for(int i=0;i<r.count;i++)
		{
			Handles.color = Color.red * new Color(1,1,1,0.3f);
			float x = nodeRect.xMin + ((i+1) * avg);// * EventorScheduleEditorWindow.pixelsPerSecond);
			Handles.DrawLine(
				new Vector3(x, nodeRect.yMin),
				new Vector3(x, nodeRect.yMax)
				);
		}
		
		Handles.EndGUI();
		
		GUI.color = c;
	}
	
	void DrawSlider()
	{		
		Color c = GUI.contentColor;
		float s = 0.3f;
		if(!job.isActive) GUI.contentColor = Color.red * new Color(s,s,s,1);
		GUI.Button(nodeRect, job.jobName,  EditorStyles.miniButtonRight);
		GUI.Label(nodeRect, job.initialDelay.ToString("F2")+"s", EditorStyles.whiteMiniLabel);
		GUI.contentColor = c;
		
		
		Handles.BeginGUI();
		Handles.color = (job.isActive ? Color.blue : Color.grey) * new Color(1,1,1,0.5f);
		Handles.DrawLine(
			new Vector3(nodeRect.xMin, 0),
			new Vector3(nodeRect.xMin,
		            window.position.yMax)); //EventorScheduleEditorWindow.lastRectOfLayout.yMin + 20));
		
		Handles.color = Color.gray * new Color(1,1,1,0.5f);
		float lH = 4f;
		Handles.DrawLine(
			new Vector3(EventorScheduleEditorWindow.inspectorWidth, lastRect.yMax - lH),
			new Vector3(window.position.width,  lastRect.yMax - lH));
		
		Handles.EndGUI();
		
		
		// Use reflection to find particular Job 
		// classes and draw them
		foreach(FieldInfo i in job.GetType().GetFields())
		{
			if(i.FieldType == typeof(EventorJobRepeatDetails))
			{
				DrawRepeats(i.GetValue(job) as EventorJobRepeatDetails);
				break;
			}
		}
		
		
		
	}
	
}
















































