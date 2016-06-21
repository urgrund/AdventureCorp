using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Linq;


public class EventorJobSlider
{
	public EventorJob job;
	public EventorSlider node;
	public Editor jobEditor;
}




public class EventorScheduleEditorWindow : EditorWindow
{
	private EventorSchedule e;
	List<EventorJobSlider> eventorJobs = new List<EventorJobSlider>();
	public static Rect lastRectOfLayout;
	public static float pixelsPerSecond = 100;
	//public static float timeLineOffset = 0;
	public static float inspectorWidth = 400;
	public static float inspectorTop = 0f;
	public Vector2 scrollPosition = Vector2.zero;
	static public Color guiContentColor;
	


	// -----------------------------------------------------------------------------------------------------------------------------

	
	public void Init(EventorSchedule e)
	{
		this.Focus();
		this.titleContent.text = "Eventor Editor";
		EventorSlider.Selection = null;
		GetAllJobs();
	}
	

	// -----------------------------------------------------------------------------------------------------------------------------
	
	bool needToUpdateEditors = true;
	
	void GetAllJobs()	
	{
		if(!GetEventorObject())
			return;
		
		eventorJobs.Clear();
		foreach(EventorJob j in e.GetComponentsInChildren<EventorJob>())
		{
			// Dunno why this happens sometimes
			if(j == null)
				continue;
			
			EventorJobSlider slider = new EventorJobSlider();
			slider.job = j;
			slider.node = new EventorSlider(j);
			slider.node.window = this;
			
			//if(needToUpdateEditors)
			{
				//needToUpdateEditors = false;
				//slider.jobEditor = Editor.CreateEditor(j);
			}
			
			eventorJobs.Add(slider);	
		}
	}
	
	bool GetEventorObject()
	{
		if(e == null)
			if(Selection.activeGameObject != null)
				if(Selection.activeGameObject.GetComponent<EventorSchedule>())
					e = Selection.activeGameObject.GetComponent<EventorSchedule>();
		
		return (e != null);
	}
	
	void OnFocus()
	{
		needToUpdateEditors = true;
		if(GetEventorObject())
			GetAllJobs();
	}
	
	void OnEnable()
	{
		needToUpdateEditors = true;
		if(GetEventorObject())
			GetAllJobs();
	}



	// -----------------------------------------------------------------------------------------------------------------------------
	
	void OnGUI()
	{
		if(!Application.isPlaying)
			if(e == null)
				return;
		
		guiContentColor = GUI.contentColor;
		DrawTopBanner();
		
		// Check for updates on the source object
		foreach(EventorJobSlider s in eventorJobs)
		{
			if(s == null)
			{
				GetAllJobs();
				break;
			}
		}
		
		// Start Drawing the main stuff
		GUILayout.BeginHorizontal();
		
		// Draw the jobs including their inspectors
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(inspectorWidth));
		foreach(EventorJobSlider s in eventorJobs)
		{
			DrawEventorJob(s);		
		}
		GUILayout.EndScrollView();
		
		// The time line
		DrawTimeLine();
		
		GUI.BeginGroup(new Rect(0, inspectorTop, this.position.width, this.position.height));
		foreach(EventorJobSlider s in eventorJobs)
		{
			s.node.OnGUI();           
		}
		GUI.EndGroup();
		
		GUILayout.EndHorizontal();
		
		
		
		// If we have a selection, we're doing an operation which requires an update each mouse move		
		switch (Event.current.type)
		{
		case EventType.mouseUp:
			// If we had a mouse up event which was not handled by the nodes, clear our selection
			EventorSlider.Selection = null;
			Event.current.Use ();
			break;
		}
		Repaint();
	}







	
	// -----------------------------------------------------------------------------------------------------------------------------

	
	
	void DrawTimeLine()
	{
		Rect r = new Rect(inspectorWidth, 0, (this.position.width - lastRectOfLayout.xMax), inspectorTop);
		Rect b = new Rect(0, 0, pixelsPerSecond, inspectorTop);
		
		int divs = (int)(r.width / pixelsPerSecond);
		
		int smallLineCount = 10;
		float lineDiv = pixelsPerSecond / smallLineCount;
		
		GUI.BeginGroup(r);
		
		//GUI.Box(r, "");
		
		for(int i=0; i<divs+1; i++)
		{
			b.x = i * pixelsPerSecond;
			
			EditorGUI.LabelField(b, i.ToString() + "s", EditorStyles.miniLabel);
			
			// draw lines
			Handles.BeginGUI( );
			for(int j=0; j < smallLineCount; j++)
			{
				float y = b.yMax * 0.25f;
				if(j==5) y = b.height * 0.5f;
				if(j==0) y = b.height;
				
				Handles.color = j==0 ? Color.white : Color.grey;
				Handles.DrawLine( 
				                 new Vector3(b.x + j*lineDiv, y),
				                 new Vector3(b.x + j*lineDiv, b.yMin));
			}
			Handles.EndGUI();
		}
		
		GUI.EndGroup();
		
		if( Event.current.type == EventType.mouseDrag && r.Contains (Event.current.mousePosition))		
		{
			pixelsPerSecond += Event.current.delta.x * 0.5f;
			pixelsPerSecond = Mathf.Clamp(pixelsPerSecond, 40, 300);
		}
	}
	








	// -----------------------------------------------------------------------------------------------------------------------------
	
	float tsBtnW = 25;
	float tsActiveW = 45;
	float tsTypeW = 110;
	float tsDelayW = 30;	
	float scrollbarwidth = 100;
	
	bool lastAllActive = true;
	bool lastAllShow = true;
	bool lastSortByDelay = true;
	
	bool showScheduleDetails = false;
	
	void DrawTopBanner()
	{
		if(e == null)
			return;

		showScheduleDetails = EditorGUILayout.Foldout(showScheduleDetails, e.name.ToString());//, EditorStyles.whiteLargeLabel);
		
		if(showScheduleDetails)
		{

			GUILayout.BeginHorizontal(GUILayout.Width(inspectorWidth - 3));
			e.isDestroyOnAllCompelte = GUILayout.Toggle(e.isDestroyOnAllCompelte, "Destroy On Complete", EditorStyles.miniButtonMid);
			GUILayout.EndHorizontal();

			//GUILayout.Label(e.name, EditorStyles.whiteLargeLabel);
			GUILayout.BeginHorizontal(GUILayout.Width(inspectorWidth - 3));
			e.isActive = GUILayout.Toggle(e.isActive, "Active", EditorStyles.miniButtonMid);
			e.isStartOnAwake = GUILayout.Toggle(e.isStartOnAwake, "Start On Awake", EditorStyles.miniButtonMid);
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal(GUILayout.Width(inspectorWidth - 3));
			e.repeat.isActive = GUILayout.Toggle(e.repeat.isActive, "Repeat", EditorStyles.miniButtonMid);
			e.isRepeatInfinite = GUILayout.Toggle(e.isRepeatInfinite, "Never End", EditorStyles.miniButtonMid);
			GUILayout.EndHorizontal();
			
			
			//GUILayout.BeginHorizontal(GUILayout.Width(inspectorWidth - 3));
			e.repeat.count = EditorGUILayout.IntField("Repeat Count", e.repeat.count, GUILayout.Width(inspectorWidth - 3));
			e.repeat.repeatDelay.x = EditorGUILayout.FloatField("Min", e.repeat.repeatDelay.x, GUILayout.Width(inspectorWidth - 3));
			e.repeat.repeatDelay.y = EditorGUILayout.FloatField("Max", e.repeat.repeatDelay.y, GUILayout.Width(inspectorWidth - 3));
			//GUILayout.EndHorizontal();
			
			e.repeat.repeatDelay.x = Mathf.Max (0, e.repeat.repeatDelay.x);
			e.repeat.repeatDelay.y = Mathf.Max (0, e.repeat.repeatDelay.y);
		}
		
		
		
		GUIStyle s = EditorStyles.toolbarButton;
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("i", s, GUILayout.Width(tsBtnW))) {
			eventorJobs.ForEach(x => x.job.isExpandedInEditor = lastAllShow); lastAllShow = !lastAllShow;
		}
		if(GUILayout.Button("Active", s, GUILayout.Width(tsActiveW))){
			eventorJobs.ForEach(x => x.job.isActive = lastAllActive); lastAllActive = !lastAllActive;
		}
		GUILayout.Label("Class Type", s, GUILayout.Width(tsTypeW));
		GUILayout.Label("Job Name", s, GUILayout.Width(175));
		if(GUILayout.Button("t", s, GUILayout.Width(tsDelayW))){
			if(lastSortByDelay)
				eventorJobs.Sort((x,y) => x.job.initialDelay.CompareTo(y.job.initialDelay));
			else
				GetAllJobs();
			lastSortByDelay = !lastSortByDelay;
		}
		GUILayout.Label("", s, GUILayout.ExpandWidth(true));
		GUILayout.EndHorizontal();
		
		inspectorTop = GUILayoutUtility.GetLastRect().yMax;		
	}






	// -----------------------------------------------------------------------------------------------------------------------------

	void DrawJobToolStrip(EventorJobSlider s)
	{
		GUILayout.BeginHorizontal();//GUILayout.Width(inspectorWidth));	
		
		if(GUILayout.Button((s.job.isExpandedInEditor ? "v" : ">"), EditorStyles.toolbarButton, GUILayout.Width(tsBtnW) ))
			s.job.isExpandedInEditor = !s.job.isExpandedInEditor;
		
		GUI.contentColor = s.job.isActive ? Color.green:Color.red;
		if(GUILayout.Button((s.job.isActive ? "active" : "off"), EditorStyles.toolbarButton, GUILayout.Width(tsActiveW) ))
			s.job.isActive = !s.job.isActive;
		GUI.contentColor = guiContentColor;
		
		GUILayout.Label("("+s.job.GetType().ToString()+")", EditorStyles.miniLabel, GUILayout.Width(tsTypeW));
		
		s.job.jobName = EditorGUILayout.TextField(s.job.jobName, EditorStyles.miniTextField, GUILayout.ExpandWidth(true));
		s.job.initialDelay = EditorGUILayout.FloatField("", s.job.initialDelay, EditorStyles.miniTextField, GUILayout.Width(tsDelayW));
		
		GUILayout.EndHorizontal();
	}






	// -----------------------------------------------------------------------------------------------------------------------------
	
	void DrawEventorJob(EventorJobSlider s)
	{
		
		FillBox(1f);
		
		DrawJobToolStrip(s);
		
		lastRectOfLayout = GUILayoutUtility.GetLastRect();
		
		if( Event.current.type == EventType.Repaint )		
		{
			//s.node.lastRect = GUILayoutUtility.GetLastRect();
			
			lastRectOfLayout.xMax = inspectorWidth;
			lastRectOfLayout.center -= new Vector2(0, scrollPosition.y + EventorSlider.kNodeHeight);
			
			s.node.lastRect = lastRectOfLayout;
			s.node.SetPositionFromLastRect();
			//lastRectOfLayout = s.node.lastRect;
		}
		
		if(s.job.isExpandedInEditor)
		{	
			if(!s.job.isActive)
				GUI.contentColor = Color.gray;

			s.jobEditor = Editor.CreateEditor(s.job);
			s.jobEditor.DrawDefaultInspector();
			GUI.contentColor = guiContentColor;
		}     
		
		lastRectOfLayout = GUILayoutUtility.GetLastRect();
	}
	
	
	public static void FillBox(float s)
	{
		GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(s)});
	}
}


















