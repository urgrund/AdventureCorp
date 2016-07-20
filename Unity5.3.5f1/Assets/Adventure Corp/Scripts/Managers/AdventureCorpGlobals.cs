using UnityEngine;
using System.Collections;

public static class AdventureCorpGlobals
{
    public static class Editor
    {
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

    public class Agent
    {
        public static readonly float skinWidth = 0.025f;
        public static readonly float stepOffset = 0.4f;
        public static readonly float gravity = 32f;
    }
}
