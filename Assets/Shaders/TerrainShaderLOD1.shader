Shader "Custom/TerrainShaderLOD1"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_CliffTex("Cliff (RGB)", 2D) = "white" {}
		_ShoreTex("Shore (RGB)", 2D) = "white" {}
		_WaterLevel("Water Level", float) = 312.5
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
			_CliffIntensity("Cliff Exponent", float) = 1.0
			_ShoreFalloff("Shore Falloff", float) = 1.0
			_ShoreOffset("Shore Offset", float) = 0.0
			_CliffCutoff("Cliff Cutoff", Range(0,1)) = 0.0
			_CliffContrast("Cliff Contrast", float) = 1.0
			_CliffGlossiness("Cliff Smoothness", float) = 0.0
			_CliffMetallic("Cliff Metallic", Range(0,1)) = 0.0
			_Roughness("Roughness Texture", 2D) = "white" {}
		_RoughnessHeightMin("Roughness Height Minimum", float) = 0.0
			_RoughnessHeightMax("Roughness Height Maximum", float) = 1.0
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf SimpleSpecular fullforwardshadows

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0
				sampler2D _Roughness;
		float _RoughnessHeightMin;
		float _RoughnessHeightMax;
        sampler2D _MainTex;
		sampler2D _CliffTex;
		sampler2D _ShoreTex;

        struct Input
        {
            float2 uv_MainTex;
			float3 worldPos;
			float4 color : COLOR;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		float _WaterLevel;
		float _CliffIntensity;
		float _ShoreFalloff;
		float _ShoreOffset;
		float _CliffCutoff;
		float _CliffContrast;
		half _CliffGlossiness;
		half _CliffMetallic;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

		float3 blend(float3 texture1, float a1, float3 texture2, float a2)
		{
			float avg1 = (texture1.r + texture1.g + texture1.b) / 3.0;
			float avg2 = pow((texture2.r + texture2.g + texture2.b) / 3.0, _CliffContrast);
			float depth = 0.2;
			float ma = max(avg1 + a1, avg2 + a2) - depth;

			float b1 = max(avg1 + a1 - ma, 0);
			float b2 = max(avg2 + a2 - ma, 0);

			return (0.0 * b1 + 1.0 * b2) / (b1 + b2);
		}
		half4 LightingSimpleSpecular(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
			half3 h = normalize(lightDir + viewDir);

			half diff = max(0, dot(s.Normal, lightDir));

			float nh = max(0, dot(s.Normal, h));
			float spec = pow(nh, s.Gloss);

			half4 c;
			c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * (spec * s.Specular)) * atten;
			c.a = s.Alpha;
			return c;
		}
        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			fixed4 l = tex2D(_CliffTex, IN.uv_MainTex) * _Color;
			fixed4 s = tex2D(_ShoreTex, IN.uv_MainTex) * _Color;
			fixed4 r = tex2D(_Roughness, IN.uv_MainTex) * _Color;
			
			//float coeff = min(_ShoreFalloff, max(0.0, IN.worldPos.y - _WaterLevel - _ShoreOffset)) / _ShoreFalloff;
			//o.Albedo = lerp(s.rgb, c.rgb, coeff);
			o.Albedo = lerp(c.rgb, s.rgb, IN.color.r);
			o.Albedo = lerp(o.Albedo, r.rgb, min(1.0,max(0.0,(IN.worldPos.y-_RoughnessHeightMin))/_RoughnessHeightMax));
			float cliffAmount = pow(max(float3(0.0, 0.0, 0.0), dot(o.Normal, float3(0.0, 1.0, 0.0))), _CliffIntensity);
			cliffAmount = -cliffAmount + 1.0;
			/*
			if (-cliffAmount + 1.0 <= _CliffCutoff)
				cliffAmount = 1.0;*/
			cliffAmount = max(0.0,cliffAmount-_CliffCutoff)/(1.0-_CliffCutoff);
			float cliffAmount2 = blend(o.Albedo, 1.0 - cliffAmount, l.rgb, cliffAmount).rgb;
			o.Albedo.rgb = lerp(o.Albedo,l.rgb,blend(o.Albedo,1.0-cliffAmount,l.rgb, lerp(0,cliffAmount,cliffAmount2)).rgb);
			//o.Albedo = lerp(o.Albedo, l.rgb, cliffAmount);
			//o.Albedo *= IN.color.rgb;
				/*
			if (IN.worldPos.y <= _WaterLevel)
				o.Albedo = s.rgb;*/
            // Metallic and smoothness come from slider variables
            o.Specular = lerp(_Metallic,_CliffMetallic,cliffAmount);
            o.Gloss = lerp(_Glossiness,_CliffGlossiness,cliffAmount);
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
