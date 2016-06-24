Shader "Custom/MattTestShader" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_AlphaCutoff("Alpha Cutoff", Range(0,1)) = 0.5
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
		SubShader{
			//Tags { "RenderType" = "Opaque" }
			Tags{ "Queue" = "AlphaTest" "RenderType" = "TransparentCutout" "IgnoreProjector" = "True" }

			LOD 200
			//Cull Off
			AlphaToMask On
			CGPROGRAM
			#pragma surface surf WrapLambert

			half4 LightingWrapLambert(SurfaceOutput s, half3 lightDir, half atten)
			{				
				half NdotL = dot(s.Normal, lightDir);
				half diff = NdotL * 0.5 + 0.5;
				half4 c;
				c.rgb = s.Albedo * _LightColor0.rgb * (diff * atten);
				c.a = s.Alpha;
				return c;
			}

			struct Input {
				float2 uv_MainTex;
			};

			
			sampler2D _MainTex;
			float4 _Color;
			float _AlphaCutoff;

			void surf(Input IN, inout SurfaceOutput o) {				
				fixed4 diff = tex2D(_MainTex, IN.uv_MainTex);
				clip(-(1-diff.a)+(1-_AlphaCutoff));
				o.Albedo = diff.rgb;
			}
			ENDCG
		}
		FallBack "Diffuse"
}
