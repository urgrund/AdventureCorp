// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/Standard Character" {
	Properties {
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo", 2D) = "white" {}
		_MetallicGlossMap("Metallic", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" {}

		_RimColor ("Rim Color", Color) = (1,1,1,1)
		_RimPower ("Rim Power", Range(1,8)) = 2
		_RimBias("Rim Bias", Range(0,1)) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM


#pragma surface surf WrapLambert
			fixed _RimPower;
			fixed _RimBias;
			half4 LightingWrapLambert(SurfaceOutput s, half3 lightDir, half atten) {
			half NdotL = dot(s.Normal, lightDir);
			half diff = NdotL * _RimBias + _RimPower;
			half4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * (diff * atten);
			c.a = s.Alpha;
			return c;
		}

		struct Input {
			float2 uv_MainTex;
		};

		sampler2D _MainTex;
		void surf(Input IN, inout SurfaceOutput o) {
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
			o.Alpha = 1;
		}

		//// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf WrapLambert fullforwardshadows 
		//#include "UnityCG.cginc"	 

		//// Use shader model 3.0 target, to get nicer looking lighting
		//#pragma target 3.0

		//sampler2D _MainTex;
		//fixed4 _Color;
		//sampler2D _MetallicGlossMap;
		//sampler2D _BumpMap;
		//
		//fixed4 _RimColor;
		//fixed _RimPower;
		//fixed _RimBias;
		//
		//struct Input {
		//	float2 uv_MainTex;
		//	float3 worldNormal;
		//	float3 viewDir;
		//};	

		//void surf (Input IN, inout SurfaceOutputStandard o) {
		//	
		//	fixed4 albedo = tex2D (_MainTex, IN.uv_MainTex) * _Color;			
		//	fixed4 m = tex2D (_MetallicGlossMap, IN.uv_MainTex);
		//	fixed3 n = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));

		//	o.Albedo = albedo.rgb;
		//	o.Normal = n;

		//	o.Metallic = m.r;
		//	o.Smoothness = m.a;
		//	o.Alpha = albedo.a;

		//	//fixed d = 1;
		//	//fixed3 vD = IN.viewDir;
		//	//d = dot(vD, o.Normal);
		//	fixed d = 1-pow(saturate(dot(normalize(IN.viewDir.xyz), o.Normal.xyz) + _RimBias), _RimPower);
		//	o.Emission = lerp(_RimColor.rgb, albedo.rgb, _RimColor.a) * 2 * d;


		//}
		ENDCG
	}
	FallBack "Diffuse"
}
