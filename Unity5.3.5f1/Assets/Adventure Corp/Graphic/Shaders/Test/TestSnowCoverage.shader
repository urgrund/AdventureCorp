// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/TestSnowCoverage" {
	Properties {
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo", 2D) = "white" {}
		_MetallicGlossMap("Metallic", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" {}


		[Enum(World,0,Model,1)] _CovereageUVSource("Covereage UV Source", Float) = 0
		_cA("Coverage (RGB) ", 2D) = "white" {}
		_cN("Covereage Normal", 2D) = "bump"{}
		_CoveragePower("Coverage Power", Range(0.1, 8)) = 0.15
		_CoverageBias("Coverage Bias", Range(-1,1)) = -0.2
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows 
		#include "UnityCG.cginc"	 

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		sampler2D _MetallicGlossMap;
		sampler2D _BumpMap;

		sampler2D _cA;
		sampler2D _cN;
		
		fixed _CoveragePower;
		fixed _CoverageBias;
		
		struct Input {
			float2 uv_MainTex;
			float3 worldNormal;
			float3 worldPos;
			INTERNAL_DATA
		};	


		void surf (Input IN, inout SurfaceOutputStandard o) {
			
			fixed4 albedo = tex2D (_MainTex, IN.uv_MainTex) * _Color;			
			fixed4 m = tex2D (_MetallicGlossMap, IN.uv_MainTex);
			fixed3 n = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));

			fixed2 coverageUV = IN.worldPos.xz*0.1;
			fixed4 cA = tex2D(_cA, coverageUV);
			fixed3 cN = UnpackNormal(tex2D(_cN, coverageUV));

			// Write normal
			o.Normal = n;	

			// Get the blend based on world up
			fixed3 worldNormal = WorldNormalVector(IN, o.Normal);
			fixed NdW = dot(worldNormal, fixed3(0, 1, 0));		
			fixed d = pow(saturate(dot(worldNormal, fixed3(0, 1, 0)) + _CoverageBias), _CoveragePower);

			o.Albedo = lerp(albedo, cA, d).rgb;
			o.Normal = normalize(lerp(n, cN, d));

			o.Metallic = lerp(m.r,0, d);
			o.Smoothness = lerp(m.a,0,d);
			o.Alpha = albedo.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
