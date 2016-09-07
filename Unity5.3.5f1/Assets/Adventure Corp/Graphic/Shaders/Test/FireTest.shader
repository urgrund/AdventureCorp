// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/FireTest"
{
	Properties{
		_Color("Main Color", Color) = (1,1,1,0)
		_Test("Test", float) = 0
		_Test2("Test2", float) = 0
		_Test3("Test3", float) = 0
		_Fade("Fade", float) = 0

		//_ZFactor ("ZFactor", float)=1
		//_ZUnits("ZUnits", float) = 1
	}
	
	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }
	//	Tags{ "RenderType" = "Opaque" "Queue" = "Geometry+2" }
		Lighting Off

		Blend SrcAlpha OneMinusSrcAlpha		
		ColorMask RGB		
		//Cull Front		
		
		ZWrite On
		//ZTest Greater
		//ZTest Always


		//ColorMask RGB
		//Cull Front
		//ZTest Always
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
			fixed _Fade;

			sampler2D_float _CameraDepthTexture;
			sampler2D _CameraGBufferTexture0;
			sampler2D _CameraGBufferTexture1;
			sampler2D _CameraGBufferTexture2;
			sampler2D _CameraGBufferTexture3;


			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				fixed4 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 projPos : TEXCOORD0; //Screen position of pos
				float4 localVertex : TEXCOORD1;
				float3 worldPosition : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
			};

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.projPos = ComputeScreenPos(o.vertex);
				o.localVertex = v.vertex;
				COMPUTE_EYEDEPTH(o.projPos.z);
				o.worldPosition = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
				//o.worldNormal = mul(unity_ObjectToWorld, float4(v.normal.xyz, 0.0)).xyz;
				o.worldNormal = UnityObjectToWorldNormal(v.normal);

				return o;
			}

			half4 frag(v2f i) : COLOR
			{
			//Grab the depth value from the depth texture
			//Linear01Depth restricts this value to [0, 1]

				fixed4 projUV = UNITY_PROJ_COORD(i.projPos);

				float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, projUV));
				float nonLinScene = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, projUV).r;
				float partZ = i.projPos.z;
				
				float fade = saturate(_Fade * (sceneZ - partZ));
				float fade2 = saturate(_Test2 * (partZ - sceneZ));
				
				float4 dfse = tex2Dproj(_CameraGBufferTexture0, projUV);
				float4 spec = tex2Dproj(_CameraGBufferTexture1, projUV);
				float4 nrm = tex2Dproj(_CameraGBufferTexture2, projUV);
				float4 light = tex2Dproj(_CameraGBufferTexture3, projUV);
			

				fixed nonL = saturate(pow(nonLinScene * _Test2, 4));
				fixed pix = saturate(pow(partZ * _Test, 4));

				fixed4 c = fixed4(0, 0, 0, 1);// _Color;
				//c.r = pow( partZ * _Test, 4);
				//c.g = sceneZ * _Test2;
				//c.b = 1-pow( nonLinScene * _Test3, 4);
				
				


				//c.r = sceneZ * _Test;
				//c.b = partZ * _Test3;
				//c.g = fade;
				if (sceneZ-partZ > _Test2)
					c.a = 0;
				//c.g = (partZ >= sceneZ) ? 1 : 0;
				
				c.rgb = normalize(i.worldNormal); 
				
				//c.g = 1;// fade;

				//c.a = fade;
				
				
				
				//clip(-fade2);

				
				
				
				return c;
			}
			ENDCG
		}	
	}
}