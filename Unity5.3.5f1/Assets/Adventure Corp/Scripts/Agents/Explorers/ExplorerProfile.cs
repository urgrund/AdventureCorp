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
		public int parryPelletReGenPerSecond = 2;

		public float dashSpeed = 16f;
		public float dashDuration = 0.15f;
		public float dashCoolDown = 1f;
    }


    /// <summary>
	/// String representation of the name of the Explorer
	/// </summary>
    public string explorerName = "new explorer";

	/// <summary>
	/// Container for all visuals for this explorer for use in HUD's, menu's... etc
	/// </summary>
    public Visuals visuals;

	/// <summary>
	/// Stats of the explorer 
	/// </summary>
    public Stats statistics; 
}
