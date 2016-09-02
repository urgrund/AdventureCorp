using UnityEngine;
using System.Collections;

/// <summary>
/// Adventure Corp Global properites
/// (c) 2011-2016 Flame Shark
/// </summary>
public static class AdventureCorpGlobals
{
    public static class Editor
    {
		public static class Measurements
		{
			public static readonly float patrolPointSpacing = 3f;
			public static readonly float coverPointSpacing = 4.5f;
		}

        public static class Prefixes
        {
            public static readonly string AttackDescriptor = "atk_";
            public static readonly string AnimationProperties = "ap_";
        }

        public static class NavMesh
        {
            public static readonly float agentRadius = 1f;
            public static readonly float agentHeight = 2f;
            public static readonly float maxSlope = 35f;
            public static readonly float stepHeight = 0.4f;
            public static readonly float dropHeight = 3f;
            public static readonly float jumpDistance = 2f;
        }
    }

    public static class Agent
    {
        public static readonly float skinWidth = 0.025f;
        public static readonly float stepOffset = 0.4f;
        public static readonly float gravity = 32f;
    }

	public static class Input
	{
		public static class Player
		{
			public static readonly string MELEE = "Melee";
			public static readonly string RANGED = "Ranged";
			public static readonly string SHIELD = "Shield";
			public static readonly string DASH = "Dash";
			public static readonly string MOVE_HORIZONTAL = "Move Horizontal";
			public static readonly string MOVE_VERTICAL = "Move Vertical";
			public static readonly string AIM_HORIZONTAL = "Aim Horizontal";
			public static readonly string AIM_VERTICAL = "Aim Vertical";
		}

		public static class UI
		{
		}
	}
}


public static class AdventureCorpExtensions
{
	public static bool Evaluate(this Probability e)
	{
		if (e == Probability.Always)
			return true;
		if (e == Probability.Never)
			return false;
		return (Random.value < (float)((float)e / 100f));
	}
}
