using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "ep_ExplorerProfile", menuName = "Adventure Corp/Explorer Profile")]
public class ExplorerProfile : ScriptableObject
{    
    [System.Serializable]
    public class Visuals
    {
        public Material HUD;
        public Material headImageHD;
        public Material fullBody;        
    }

    [System.Serializable]
    public class Stats
    {
        [Range(0, 10)]
        public int vitality = 5;
        [Range(0, 10)]
        public int strength = 5;
        [Range(0, 10)]
        public int agility = 5;

		public int parryPellets = 5;
		public float parryPelletReGenInterval = 0.5f;
		public float parryBlockMovementScale = 0.25f;

		public float dashSpeed = 16f;
		public float dashDuration = 0.15f;
		public float dashCoolDown = 1f;

		public float rangedTimeToFocus = 60f;
		public float rangedStartAngle = 45f;

		public float staggerDmgReceiveWindow = 1f;
		public float staggerDmgThreshold = 30;
	}


    /// <summary>
	/// String representation of the name of the Explorer
	/// </summary>
    public string explorerName = "new explorer";

	/// <summary>
	/// Container for all visuals for this explorer for use in HUD's, menu's... etc
	/// </summary>
    public Visuals visuals = new Visuals();

	/// <summary>
	/// Stats of the explorer 
	/// </summary>
    public Stats statistics = new Stats(); 
}
