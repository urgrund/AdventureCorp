Shader "Custom/FireTest2" {

	Properties{
		_Color("Main Color", Color) = (1,1,1,0)
		_Test("Test", float) = 0
		_Test2("Test2", float) = 0
		_Test3("Test3", float) = 0
	}


	SubShader{
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry+1" }

		ColorMask 0
		//ZWrite off

		Stencil
		{
			Ref 1
			Comp always
			Pass replace
		}


		CGINCLUDE
#include "UnityCG.cginc"

		struct appdata {
			float4 vertex : POSITION;
		};

		struct v2f {
			float4 pos : SV_POSITION;
		};

		v2f vert(appdata v) {
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			return o;
		}

		half4 frag(v2f i) : SV_Target{
			return half4(1,1,0,1);
		}
		ENDCG


		Pass {
			Cull Front
			ZTest Less

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}


		Pass {
			Cull Back
			ZTest Greater

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}



		Pass
		{
			Tags{ "RenderType" = "Opaque" "Queue" = "Geometry+2" }

				//Blend SrcAlpha OneMinusSrcAlpha

				ColorMask RGB
				//Cull Front				
				ZTest Always
				//Lighting Off
				Stencil
				{
					Ref 1
					Comp notequal
				}

				CGPROGRAM
#pragma vertex vert2
#pragma fragment frag2
#include "UnityCG.cginc"

				fixed4 _Color;
			fixed _Test;
			fixed _Test2;

			sampler2D _CameraGBufferTexture0;
			sampler2D _CameraGBufferTexture1;
			sampler2D _CameraGBufferTexture2;
			sampler2D _CameraGBufferTexture3;


			sampler2D_float _CameraDepthTexture;

			struct v2f2
			{
				float4 pos : SV_POSITION;
				float4 vert : TEXCOORD0;
				float4 projPos : TEXCOORD1; //Screen position of pos
			};

			v2f2 vert2(appdata v) {
				v2f2 o;
				o.vert = v.vertex;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.projPos = ComputeScreenPos(o.pos);
				return o;
			}

			half4 frag2(v2f2 i) : SV_Target{

				return fixed4(1,1,1,1);

				fixed4 projUV = UNITY_PROJ_COORD(i.projPos);
				float4 dfse = tex2Dproj(_CameraGBufferTexture0, projUV);
				float4 spec = tex2Dproj(_CameraGBufferTexture1,projUV);
				float4 nrm = tex2Dproj(_CameraGBufferTexture2,projUV);
				float4 light = tex2Dproj(_CameraGBufferTexture3, projUV);
				
				fixed4 c = fixed4(0,0,0,0);
				c = _Color;//i.vert;
				//clip(0-(nrm.y < 0.25));

				float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
				//float nonLinScene = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)).r;
				float partZ = i.projPos.z;
				float fade = saturate(_Test * (sceneZ - partZ));
				c.a = 1;// fade;

				//if (nrm.y > 0.5)
					//c.r = 1;

				//return light;
				return c;
			}


			ENDCG
		}



	}
}