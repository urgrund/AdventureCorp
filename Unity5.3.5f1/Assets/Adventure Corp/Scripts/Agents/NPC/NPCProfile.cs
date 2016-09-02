using UnityEngine;
using System.Collections;



[CreateAssetMenu(fileName = "NPCProfile", menuName = "Adventure Corp/NPC Profile")]
public class NPCProfile : ScriptableObject
{
	public string profileName = "New NPC";
	
	public Statistics statistics;
	public Patrol patrol;	
	public Awareness awareness;
	public Attack attack;
	public BaseAttackCollection attackCollection;
	public Actions actions;




	[System.Serializable]
	public class Statistics
	{
		public NPCBrain.Rank rank = NPCBrain.Rank.Soldier;
		public int startingHealth = 100;
		public int maxHealth = 100;
	}


	[System.Serializable]
	public class Patrol
	{
		[Range(1, 5)]
		[Tooltip("Bias to the exact position to consider as arrived")]
		public float destinationBias = 1f;

		[Range(0, 1)]
		public float patrolMoveSpeedRatio = 0.33f;
		public float waitAtDestinationTime = 2f;		
		public Probability waitAtDestination = Probability.EqualChance;

		[Tooltip("Whether to venture to other, linked, patrol zones or stay only within the starting assigned zone")]
		public Probability stayWithinAssignedZone = Probability.EqualChance;

		[Tooltip("If near an NPC with a higher rank, should we follow them?")]
		public Probability followSuperiors = Probability.Never;

		[Tooltip("(NOT IMPLEMENTED)  The formation to use whilst following")]
		public NPCBrain.PatrolFormation followingFormation = NPCBrain.PatrolFormation.Column;

		[Tooltip("If subordinates follow this NPC, what is the max it wants to manage?")]
		public int maxPatrolSubordinates = 5;


		public Probability avoidOtherAgents = Probability.Always;
	}


	[System.Serializable]
	public class Awareness
	{
		[Tooltip("Radius for awareness")]
		public float radial = 2f;
		public float coneRadius = 8f;
		[Range(5,180)]
		public float coneAngle = 80;

		[Tooltip("Personal space for avoidance and positioning")]
		public float personalSpace = 1.5f;

		public bool isUsePreferredStateOnAware = true;
		[Tooltip("The preferred state to enter after being aware of another agent")]
		public NPCBrain.State preferredStateOnAware = NPCBrain.State.Attack; 
	}


	[System.Serializable]
	public class Attack
	{


		[Header("Offense")]

		[Range(0,1)]
		[Tooltip("At what ratio of health should the NPC be more cautios with attacks")]
		public float cautiousHealthRatio = 0.5f;

		[Tooltip("What distance to the target is considered as close range?")]
		public float closeRangeDistance = 4f;

		[Tooltip("What distance to the target to consider far and start running")]
		public float farRangeDistance = 6f;

		[Range(0,1)]
		[Tooltip("When in close range to target, scale the speed by this amount")]
		public float closeRangeMoveSpeedRatio = 0.5f;


		[Tooltip("If hit by an NPC, should this NPC hit it back?")]
		public Probability attackNPCsIfHit = Probability.Never;

		[Range(3,50)]
		[Tooltip("Stop attacking if this far from the place we started attacking")]
		public float stopAttackFromStartDistance = 25;

		[Range(3, 50)]
		[Tooltip("Stop attacking if this far from the target")]
		public float stopAttackFromTargetDistance = 15f;

		[Tooltip("Prefer to move into close range distance before attacking")]
		public Probability preferCloseCombat = Probability.EqualChance;

		[Range(5, 45)]
		[Tooltip("If in close combat mode, how long to require being in close range distance")]
		public float closeCombatDuration = 5;


		public Probability parryAccuracy = Probability.Low;


		[Header("Defense")]


		[Range(0, 1)]
		[Tooltip("When health is below this ratio the NPC will retreat")]
		public float retreatHealthRatio = 0.1f;

		[Tooltip("If hit, jump away from the attacker to create distance")]
		public Probability jumpAwayIfHit = Probability.Low;

		[Tooltip("Anticiapte the attacker and jump away before they attack")]
		public Probability jumpAwayToAvoid = Probability.Low;

		[Tooltip("Should the NPC try to avoid a projectile?")]
		public Probability attemptProjectileAvoidance = Probability.Low;

		public Probability useShieldWhenAttacked = Probability.EqualChance;
		public Probability preferShieldOverAvoid = Probability.EqualChance;
		

	}


	[System.Serializable]
	public class Actions
	{
		
		public AnimationClipProperties roar;
		public AttackDescriptor jumpAway;
		public AttackDescriptor jumpLeft;
		public AttackDescriptor jumpRight;
		
		public AnimationClipProperties shieldBlock;
	}
}
