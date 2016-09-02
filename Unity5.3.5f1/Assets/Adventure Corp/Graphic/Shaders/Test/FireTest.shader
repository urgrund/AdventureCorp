Shader "Custom/FireTest"
{
	Properties{
		_Color("Main Color", Color) = (1,1,1,0)
		_Test("Test", float) = 0
		_Test2("Test2", float) = 0
		_Test3("Test3", float) = 0
	}
	
	SubShader
	{
		//Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry+2" }
		Lighting Off

		//Blend SrcAlpha OneMinusSrcAlpha		
		//ColorMask RGB		
		//Cull Front		
		
		//ZWrite Off
		//ZTest Greater
		//ZTest Always


		ColorMask RGB
		Cull Front
		ZTest LEqual
		Stencil
		{
			Ref 1
			Comp notequal
		}


		Pass
		{	

				CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#define UNITY_PASS_FORWARDBASE
			#include "UnityCG.cginc"

				fixed4 _Color;
			fixed _Test;
			fixed _Test2;
			fixed _Test3;

			sampler2D_float _CameraDepthTexture;

			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 projPos : TEXCOORD1; //Screen position of pos
			};

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.projPos = ComputeScreenPos(o.vertex);
				COMPUTE_EYEDEPTH(o.projPos.z);

				return o;
			}

			half4 frag(v2f i) : COLOR
			{
			//Grab the depth value from the depth texture
			//Linear01Depth restricts this value to [0, 1]
				float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
			float nonLinScene = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)).r;
				float partZ = i.projPos.z;
				float fade = saturate(_Test3 * (sceneZ - partZ));
				
			

				fixed nonL = saturate(pow(nonLinScene * _Test2, 4));
				fixed pix = saturate(pow(partZ * _Test, 4));

				fixed4 c = fixed4(0, 0, 0, 0);// _Color;
				//c.r = pow( partZ * _Test, 4);
				//c.g = sceneZ * _Test2;
				//c.b = 1-pow( nonLinScene * _Test3, 4);
				
				c.r = pix;
				//c.g = fade;				

				
				
				c.a = 1;// fade;
				return c;
			}
			ENDCG
		}	
	}
}