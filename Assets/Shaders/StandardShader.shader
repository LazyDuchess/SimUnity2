// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/reflect" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		_BumpMap("Bump Map", 2D) = "gray" {}
		_Reflectivity("Reflectivity", Range(0,1)) = 0.0
			_ReflectionBlur("Reflection Blur", Range(0,3)) = 0.0
			_SpecularAmount("Specular Amount", Range(0,1)) = 0.0
			_Cutout("Cutout", Range(0,1)) = 0.0
		_BumpOrNormal("BumpOrNormal", Range(0,1)) = 0.0
	}
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Reflect fullforwardshadows addshadow

	float4 tint0;
		float _ReflectionBlur;
		float _Reflectivity;
		sampler2D _MainTex;
		sampler2D _BumpMap;
		float _BumpOrNormal;
		float _SpecularAmount;
		float _Cutout;
		uniform float4 _BumpMap_TexelSize;

	struct Input {
		float2 uv_MainTex;
		float3 worldRefl;
		float3 viewDir;
		//float3 worldPos;
		//float3 worldNormal;
		INTERNAL_DATA
	};
	/*
	v2f vert(float4 vertex : POSITION, float3 normal : NORMAL)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(vertex);
		o.worldPos = mul(unity_ObjectToWorld, vertex).xyz;
		o.worldNormal = UnityObjectToWorldNormal(normal);
		return o;
	}*/

	void surf(Input IN, inout SurfaceOutput o) {
		
		float3 graynorm = float3(0, 0, 1);
		float heightSampleCenter = tex2D(_BumpMap, IN.uv_MainTex).r;
		float heightSampleRight = tex2D(_BumpMap, IN.uv_MainTex + float2(_BumpMap_TexelSize.x, 0)).r;
		float heightSampleUp = tex2D(_BumpMap, IN.uv_MainTex + float2(0, _BumpMap_TexelSize.y)).r;
		float sampleDeltaRight = heightSampleRight - heightSampleCenter;
		float sampleDeltaUp = heightSampleUp - heightSampleCenter;
		graynorm = cross(
			float3(1, 0, sampleDeltaRight),
			float3(0, 1, sampleDeltaUp));

		float3 bumpNormal = normalize(graynorm);
		float3 normalNormal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
		o.Normal = lerp(bumpNormal, normalNormal, _BumpOrNormal);
		float4 c = tex2D(_MainTex, IN.uv_MainTex);
		clip(c.a-_Cutout);
		//half3 worldNormal;
		//worldNormal.x = dot(i.tspace0, tnormal);
		//worldNormal.y = dot(i.tspace1, tnormal);
		//worldNormal.z = dot(i.tspace2, tnormal);
		float4 hdrReflection = 1.0;
		float3 reflectedDir = WorldReflectionVector(IN, o.Normal);
		//half3 worldViewDir = normalize(UnityWorldSpaceViewDir(IN.worldPos));
		//half3 worldRefl = reflect(-worldViewDir, IN.worldNormal);
		float4 reflection = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectedDir, _ReflectionBlur);
		hdrReflection.rgb = DecodeHDR(reflection, unity_SpecCube0_HDR);
		//hdrReflection.rgb = tex2D(_EnvMap, reflectedDir);
		hdrReflection.a = 1.0;
		float InverseReflect = (-_Reflectivity) + 1.0;
		o.Albedo = c.rgb*InverseReflect;
		o.Emission = hdrReflection*_Reflectivity;
		//c.a = 1.0;
	}

	inline fixed4 LightingReflect(SurfaceOutput s, fixed3 lightDir, half3 halfasview, fixed atten)
	{
		half3 h = normalize(lightDir + halfasview);

		half diff = max(0, dot(s.Normal, lightDir));

		float nh = max(0, dot(s.Normal, h));
		float spec = pow(nh, 48.0) * _SpecularAmount;

		half4 c;
		c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * atten;
		c.a = s.Alpha;
		

		return c;
	}

	ENDCG
	}
		FallBack "Diffuse"
}
