using UnityEngine;
using System.Collections;



[CreateAssetMenu(fileName = "NPCProfile", menuName = "Adventure Corp/NPC Profile")]
public class NPCProfile : ScriptableObject
{
	public string profileName = "New NPC";

	public Statistics statistics;
	public Patrol patrol;
	public Attack attack;
	public Awareness awareness;


	[System.Serializable]
	public class Statistics
	{
		public NPCBrain.Rank rank = NPCBrain.Rank.Soldier;
		public int healthStarting = 100;
		public int healthMax = 100;			
	}


	[System.Serializable]
	public class Patrol
	{
		[Tooltip("Bias to the exact position to consider as arrived")]
		public float destinationBias = 1f;
		public float waitAtDestinationTime = 2f;
		public Probability waitAtDetination = Probability.EqualChance;		
		public Probability followSuperiors = Probability.Never;
	}


	[System.Serializable]
	public class Awareness
	{
		public float radial = 1f;
		public float coneRadius = 5f;
		[Range(5,360)]
		public float coneAngle = 90;
	}


	[System.Serializable]
	public class Attack
	{
		[Range(0,1)]
		[Tooltip("At what ratio of health should the NPC be more cautios with attacks")]
		public float cautiousHealthRatio = 0.5f;

		[Tooltip("What distance to the target is considered as close range?")]
		public float closeRangeDistance = 5f;

		[Range(0,1)]
		[Tooltip("When in close range to target, scale the speed by this amount")]
		public float closeRangeMoveSpeedRatio = 0.5f;

		[Range(0,1)]
		[Tooltip("When health is below this ratio the NPC will retreat")]
		public float retreatHealthRatio = 0.1f;

		[Tooltip("If hit by an NPC, should this NPC hit it back?")]
		public Probability attackNPCsIfHit = Probability.Never;
	}
}
