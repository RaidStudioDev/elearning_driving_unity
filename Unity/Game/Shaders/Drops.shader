Shader "Unlit/Drops"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Size("Size", float) = 1
		_T("Time", float) = 1
		_Distortion("Distortion", range(-5, 5)) = 1
		_Blur("Blur", range(0, 1)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Transparent" }
		LOD 100

		GrabPass { "_GrabTexture" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#define S(a, b, t) smoothstep(a, b, t)

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 grabUv : TEXCOORD1;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			static const float PI = 3.14159;

			sampler2D _MainTex, _GrabTexture;
			float4 _MainTex_ST;
			float _Size, _T, _Distortion, _Blur;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.grabUv = UNITY_PROJ_COORD(ComputeGrabScreenPos(o.vertex));

				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			float N21(float2 p) {
				p = frac(p * float2(123.4, 345.45));
				p += dot(p, p + 34.345);
				return frac(p.x * p.y);
			}

			float3 Layer(float2 UV, float t)
			{
				float2 aspect = float2(2, 1);
				float2 uv = UV * _Size * aspect;
				uv.y += t * 0.25;
				float2 gv = frac(uv) - 0.5;
				float2 id = floor(uv);

				float n = N21(id); // 0 - 1
				t += n * 6.2831;

				float w = UV.y * 10;
				float x = (n - .5) * 0.8; // -0.4 to 0.4 
				x += (0.4 - abs(x)) * sin(3 * w) * pow(sin(w), 6) * 0.45;
				float y = -sin(t + sin(t + sin(t) * .5)) * 0.45;
				y -= (gv.x - x) * (gv.x - x);

				float2 dropPos = (gv - float2(x, y)) / aspect;
				float drop = S(0.05, 0.03, length(dropPos));

				float2 trailPos = (gv - float2(x, t * 0.25)) / aspect;
				trailPos.y = (frac(trailPos.y * 8) - 0.5) / 8;
				float trail = S(0.03, 0.01, length(trailPos));
				float fogTrail = S(-0.05, 0.05, dropPos.y);
				fogTrail *= S(0.5, y, gv.y);
				trail *= fogTrail;
				fogTrail *= S(0.05, 0.04, abs(dropPos.x));

				float2 offset = drop * dropPos + trail * trailPos;

				return float3(offset, fogTrail);
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// float t = _Time.y * 0 + _T;
				// we wrap the time to control the size of time
				float t = fmod(_Time.y + _T, 7200);

				float4 col = 0;

				float3 drops = Layer(i.uv, t);
				drops += Layer(i.uv * 1.23 + 7.54, t);
				drops += Layer(i.uv * 1.35 + 1.54, t);
				drops += Layer(i.uv * 1.60 - 7.54, t);

				float fade = 1 - saturate(fwidth(i.uv) * 50);
				float blur = _Blur * 7 * (1 - drops.z * fade);

				float2 projUv = i.grabUv.xy / i.grabUv.w;
				projUv += drops.xy * _Distortion * fade;
				blur *= .01;

				const float numSamples = 32;
				float angle = N21(i.uv) * (PI * 4);
				for (float i = 0; i < numSamples; i++)
				{
					float offset = float2(sin(angle), cos(angle)) * blur;
					float d = frac(sin((i + 1) * 5.) * 5.);
					// d = sqrt(d);
					offset *= d;
					col += tex2D(_GrabTexture, projUv + offset);
					angle++;
				}
				col /= numSamples;

				return col;
			}
			ENDCG
		}
	}
}
