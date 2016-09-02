﻿Shader "Custom/Hole" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,0)
	}

	
	SubShader
	{
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry+2" }

		ColorMask RGB
		Cull Front
		ZTest Always
	
		Stencil
		{
			Ref 1
			Comp notequal
		}

		Pass
	{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"
		struct appdata {
			float4 vertex : POSITION;
		};

		struct v2f {
			float4 pos : SV_POSITION;
		};

		v2f vert(appdata v) {
			v2f o;
			//UnityObjectToClipPos
			o.pos = UnityObjectToClipPos(v.vertex);
			return o;
		}

		half4 frag(v2f i) : SV_Target{
			return half4(1,0,0,1);
		}
		ENDCG
			}
	}
}