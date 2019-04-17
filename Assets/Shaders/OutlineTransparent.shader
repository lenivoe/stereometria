Shader "Unlit/OutlineTransparent" {
	Properties {
		_Color("Base Color", Color) = (1,1,1,1)
		_OutlineColor("Outline Color", Color) = (0.5,0.5,0.5,1.0)
		_OutlineWidth("Outline width", Float) = 0.01

	}

	SubShader {
		Tags{ "QUEUE" = "Transparent" "RenderType" = "Transparent" }
		LOD 250
		Lighting Off
		Fog{ Mode Off }
		Cull Front
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			Name "first"
			
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl_no_auto_normalization
			#pragma vertex vert
			#pragma fragment frag

			struct appdata_t {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};
			
			struct v2f {
				float4 pos : SV_POSITION;
			};


			fixed _OutlineWidth;

			v2f vert(appdata_t v) {
				v2f o;
				o.pos = v.vertex;
				o.pos.xyz += normalize(v.normal.xyz) * _OutlineWidth * 0.01;
				o.pos = UnityObjectToClipPos(o.pos);
				return o;
			}


			fixed4 _OutlineColor;

			fixed4 frag(v2f i) :COLOR {
				return _OutlineColor;
			}
			ENDCG
		}


		Pass {
			Name "second"

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl_no_auto_normalization
			#pragma vertex vert
			#pragma fragment frag

			struct appdata_t {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f {
				float4 pos : SV_POSITION;
			};


			fixed _OutlineWidth;

			v2f vert(appdata_t v) {
				v2f o;
				o.pos = v.vertex;
				o.pos = UnityObjectToClipPos(o.pos);
				return o;
			}


			fixed4 _Color;

			fixed4 frag(v2f i) :COLOR {
				return _Color;
			}
			ENDCG
		}
	}
	Fallback "Legacy Shaders/Diffuse"
}