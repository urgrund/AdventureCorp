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
        public Material fullBodyPose;
        public Material fullBodyLost;
        public Material fullBodyWon;
    }

    [System.Serializable]
    public class Stats
    {
        [Range(0, 10)]
        public int vitality = 3;
        [Range(0, 10)]
        public int strength = 3;
        [Range(0, 10)]
        public int agility = 3;
    }

    public Material m;
    public string explorerName = "new explorer";
    public Visuals visuals;
    public Stats statistics; 
}
