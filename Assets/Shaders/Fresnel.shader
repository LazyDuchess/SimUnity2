// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Tutorial/012_Fresnel" {
	//show values to edit in inspector
	Properties{
		_WaterClip("Water Clipping", float) = 0.0
		_WaterClipMult("Water Clip Multiplier", float) = 1.0
		_BumpMap("Water Normal Map", 2D) = "bump" {}
		_Color("Tint", Color) = (0, 0, 0, 1)
		_MainTex("Texture", 2D) = "white" {}
		_FlowSpeed("Flow Speed", float) = 1.0
		_NormalStrength("Normal Map Strength", Range(0,1)) = 1
		_Smoothness("Smoothness", Range(0, 1)) = 0
		_Metallic("Metalness", Range(0, 1)) = 0
		_WaterMask("Water Mask", 2D) = "black" {}
		_Reflection("Reflection Map", 2D) = "white" {}
		_Refraction("Refraction Amount", float) = 1.0
			_Distortion("Distortion Amount", float) = 1.0
		[HDR] _Emission("Emission", color) = (0,0,0)
		_FresnelColor("Fresnel Color", Color) = (1,1,1,1)
		[PowerSlider(4)] _FresnelExponent("Fresnel Exponent", Range(0, 40)) = 1
			_WaveA("Wave A (dir, steepness, wavelength)", Vector) = (1,0,0.5,10)
			_WaveB("Wave B", Vector) = (0,1,0.25,20)
			_WaveC("Wave C", Vector) = (1,1,0.15,10)
			_SpecColor("Specular Color", Color) = (0, 0, 0, 1)
	}
		SubShader{
			//the material is completely non-transparent and is rendered at the same time as the other opaque geometry
			Tags{ "RenderType" = "Opaque" "Queue" = "Geometry"}

			CGPROGRAM

			//the shader is a surface shader, meaning that it will be extended by unity in the background to have fancy lighting and other features
			//our surface shader function is called surf and we use the standard lighting model, which means PBR lighting
			//fullforwardshadows makes sure unity adds the shadow passes the shader might need
			#pragma surface surf BlinnPhong fullforwardshadows vertex:vert addshadow
			#pragma target 3.0

			float _Distortion;
			float _Refraction;
			float _NormalStrength;
			float _FlowSpeed;
			sampler2D _BumpMap;
			sampler2D _MainTex;
		sampler2D _WaterMask;
		sampler2D _Reflection;
			fixed4 _Color;

			half _Smoothness;
			half _Metallic;
			half3 _Emission;
			float _WaterClip;
			float _WaterClipMult;

			float3 _FresnelColor;
			float _FresnelExponent;
			//float _Steepness, _Wavelength;
		//float2 _Direction;
			float4 _WaveA, _WaveB, _WaveC;

			//input struct which is automatically filled by unity
			struct Input {
				float2 uv_MainTex;
				float2 uv_BumpMap;
				float3 viewDir;
				float4 screenPos;
				//float3 wNormal;

				INTERNAL_DATA
			};


			float3 GerstnerWave(
				float4 wave, float3 p, inout float3 tangent, inout float3 binormal
			) {
				float steepness = wave.z;
				float wavelength = wave.w;
				float k = 2 * UNITY_PI / wavelength;
				float c = sqrt(9.8 / k);
				float2 d = normalize(wave.xy);
				float f = k * (dot(d, p.xz) - c * _Time.y);
				float a = steepness / k;

				//p.x += d.x * (a * cos(f));
				//p.y = a * sin(f);
				//p.z += d.y * (a * cos(f));

				tangent += float3(
					-d.x * d.x * (steepness * sin(f)),
					d.x * (steepness * cos(f)),
					-d.x * d.y * (steepness * sin(f))
					);
				binormal += float3(
					-d.x * d.y * (steepness * sin(f)),
					d.y * (steepness * cos(f)),
					-d.y * d.y * (steepness * sin(f))
					);
				return float3(
					d.x * (a * cos(f)),
					a * sin(f),
					d.y * (a * cos(f))
					);
			}
			//the surface shader function which sets parameters the lighting function then uses
			void vert(inout appdata_full v, out Input data)
			{
				
				UNITY_INITIALIZE_OUTPUT(Input, data);
				float4 ammo = tex2Dlod(_WaterMask, float4(v.vertex.x/200, v.vertex.z/200, 0, 0));
				float3 gridPoint = v.vertex.xyz;
				float3 tangent = float3(1, 0, 0);
				float3 binormal = float3(0, 0, 1);
				float3 p = gridPoint;
				p += GerstnerWave(_WaveA, gridPoint, tangent, binormal);
				p += GerstnerWave(_WaveB, gridPoint, tangent, binormal);
				p += GerstnerWave(_WaveC, gridPoint, tangent, binormal);
				float3 normal = normalize(cross(binormal, tangent));
				v.vertex.xyz = lerp(v.vertex.xyz, p.xyz, ammo);
				v.vertex.y += p.y*ammo;
				v.normal = lerp(v.normal,normal,ammo);
				
				//data.color = ammo;
				//v.vertex = mul(unity_WorldToObject, wpos);
			}

			half4 LightingSimpleSpecular(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
				half3 h = normalize(lightDir + viewDir);

				half diff = max(0, dot(s.Normal, lightDir));

				float nh = max(0, dot(s.Normal, h));
				float spec = pow(nh, 48.0);

				half4 c;
				c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * atten;
				c.a = s.Alpha;
				return c;
			}

			void surf(Input i, inout SurfaceOutput o) {
				float water = tex2D(_WaterMask, i.uv_MainTex).r;
				//float water = 0;
				//o.Normal = lerp(o.Normal,UnpackNormal(tex2D(_BumpMap, i.uv_BumpMap+(_Time.y*_FlowSpeed))),water*_NormalStrength);
				//sample and tint albedo texture
				
				
				
				//just apply the values for metalness and smoothness
				//o.Metallic = _Metallic;
				//o.Smoothness = water;
				//float3 worldInterpolatedNormalVector = WorldNormalVector(i, float3(0, 0, 1));
				//get the dot product between the normal and the view direction
				clip((water*_WaterClipMult)+_WaterClip);
				o.Specular = max(0.01,water*2);
				o.Gloss = max(0.0,water);
				o.Normal = lerp(o.Normal, UnpackNormal(tex2D(_BumpMap, i.uv_BumpMap + (_Time.y*_FlowSpeed))), water*_NormalStrength);
				fixed4 col = tex2D(_MainTex, i.uv_MainTex + (o.Normal.xy*_Refraction));
				float fresnel = pow(abs(1 - dot(i.viewDir, float3(0, 0, 1))), _FresnelExponent);
				//invert the fresnel so the big values are on the outside
				//fresnel = saturate(1 - fresnel);
				//raise the fresnel value to the exponents power to be able to adjust it
				//fresnel = pow(fresnel, _FresnelExponent);
				fresnel = lerp(0.0, fresnel, water);
				o.Albedo = lerp(col,float4(0.0,0.0,0.0,0.0),fresnel);
				//combine the fresnel value with a color
				float2 coords = (i.screenPos.xy / i.screenPos.w)+(o.Normal.xy*_Distortion);
				float3 fresnelColor = fresnel * tex2D(_Reflection, coords);
				//apply the fresnel value to the emission
				o.Emission = fresnel * fresnelColor;
				
			}
			ENDCG
		}
			FallBack "Standard"
}