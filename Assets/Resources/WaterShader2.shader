Shader "Custom/WaterShader2"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
	_BumpMap("Water Normal Map", 2D) = "bump" {}
	_FlowSpeed("Flow Speed", float) = 1.0
	    _Reflection("Reflection", 2D) = "white" {}
		_FresnelExponent("Fresnel Exponent", float) = 1.0
			_NormalIntensity("Normal Map Intensity", Range(0,1)) = 1.0
			_Distortion("Reflection Distortion", float) = 1.0
			_Refraction("Refraction", float) = 1.0
			_WaterFogColor("Water Fog Color", Color) = (0, 0, 0, 0)
		_WaterFogDensity("Water Fog Density", Range(0, 2)) = 0.1
    }
    SubShader
    {
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "ForceNoShadowCasting" = "True"}
        LOD 200
		GrabPass { "_WaterBackground" }
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf SimpleSpecular alpha finalcolor:ResetAlpha fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
		sampler2D _CameraDepthTexture, _WaterBackground;
		sampler2D _Reflection;
		float _FlowSpeed;
		float4 _CameraDepthTexture_TexelSize;
		sampler2D _BumpMap;
		float _NormalIntensity;
		float _Distortion;
		float3 _WaterFogColor;
		float _WaterFogDensity;
		float _Refraction;

	struct Input {
		float2 uv_MainTex;
		float2 uv_BumpMap;
		float3 viewDir;
		float4 screenPos;
		
		//float3 wNormal;

		INTERNAL_DATA
	};

		float _FresnelExponent;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

			half4 LightingSimpleSpecular(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
			half3 h = normalize(lightDir + viewDir);

			half diff = max(0, dot(s.Normal, lightDir));

			float nh = max(0, dot(s.Normal, h));
			float spec = pow(nh, 500.0);

			half4 c;
			c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * atten;
			c.a = s.Alpha;
			return c;
		}

		float2 AlignWithGrabTexel(float2 uv) {
			return
				(floor(uv * _CameraDepthTexture_TexelSize.zw) + 0.5) *
				abs(_CameraDepthTexture_TexelSize.xy);
		}


		float3 ColorBelowWater(float4 screenPos, float2 distortion) {
			float2 uv = AlignWithGrabTexel((screenPos.xy) / screenPos.w);
			float backgroundDepth =
				LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
			float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(screenPos.z);
			float depthDifference = backgroundDepth - surfaceDepth;
				distortion *= saturate(depthDifference);

			float3 backgroundColor = tex2D(_WaterBackground, uv+distortion).rgb;
			float fogFactor = exp2(-_WaterFogDensity * depthDifference);
			return lerp(_WaterFogColor, backgroundColor, fogFactor);
			//return depthDifference / 20;
		}

		float3 GetDepthDifference(float4 screenPos) {
			float2 uv = AlignWithGrabTexel((screenPos.xy) / screenPos.w);
			float backgroundDepth =
				LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
			float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(screenPos.z);
			return backgroundDepth - surfaceDepth;
		}

		void ResetAlpha(Input IN, SurfaceOutput o, inout fixed4 color) {
			color.a = 1;
		}

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            //o.Metallic = _Metallic;
            
            //o.Alpha = c.a;
			o.Normal = lerp(o.Normal,UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap + (_Time.y*_FlowSpeed))),_NormalIntensity);
			float fresnel = pow(abs(1 - dot(IN.viewDir, float3(0, 0, 1))), _FresnelExponent);
			//o.Smoothness = min(1.0,lerp(_Glossiness*2,0.0,fresnel));
			o.Albedo = lerp(c, float3(0.0, 0.0, 0.0), fresnel);
			o.Alpha = lerp(o.Alpha, 1.0, fresnel);
			float2 coords = (IN.screenPos.xy / IN.screenPos.w) + (o.Normal.xy*_Distortion);
			float3 fresnelColor = fresnel * tex2D(_Reflection, coords);
			float depdif = GetDepthDifference(IN.screenPos);
			o.Emission = lerp(ColorBelowWater(IN.screenPos, o.Normal.xy*(_Refraction*saturate(depdif))), float3(0.0, 0.0, 0.0), c.a);
			//o.Albedo = lerp(float3(0.0, 0.0, 0.0), o.Albedo, c.a);
			o.Alpha = 1.0;
			o.Emission = lerp(o.Emission, fresnelColor, fresnel);

        }
        ENDCG
    }
    FallBack "Diffuse"
}
