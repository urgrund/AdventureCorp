Shader "Custom/DebugSurfaceAngles" {
	
	Properties{
		_IllegalSlopeDegrees("Illegal Slope Degrees", Range(0,90)) = 45
	}


		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		fixed _IllegalSlopeDegrees;

		struct Input {
			float2 uv_MainTex;
			float3 worldNormal;
			INTERNAL_DATA
		};



		void surf (Input IN, inout SurfaceOutputStandard o) {	

			fixed3 up = fixed3(0, 1, 0);

			fixed a = 1-(_IllegalSlopeDegrees / 90);

			// Here in the surface function I assume that you already calculated the normals.
			// This is your normals converted into world space.
			float3 worldNormal = WorldNormalVector(IN, o.Normal);

			// This is the "angle" between a vector (in your case it can be half3(0, 1, 0)) and the world normals.
			half w = dot(worldNormal, up);
			w = w * 2 - 1;

			w = step(a,w);

			// Here is the blending between red and green according to the dot product which is a between 0 and 1.
			half3 color = lerp(half3(1, 0, 0), half3(0, 1, 0), w);

			o.Albedo = color;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
