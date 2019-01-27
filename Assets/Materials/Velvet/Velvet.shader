Shader "Custom/Velvet" {
    Properties {
      _Color ("Main Color", Color) = (1,1,1,1)
      _MainTex ("Texture", 2D) = "white" {}
      _BumpMap ("Bumpmap", 2D) = "bump" {}
      _BumpMapIntensity ("Bumpmap intensity", Range(0, 2)) = 1.0
      _BumpMap2 ("Bumpmap2", 2D) = "bump" {}
      _BumpMap2Intensity ("Bumpmap2 intensity", Range(0, 2)) = 1.0
      _RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
      _RimPower ("Rim Power", Range(0.5,8.0)) = 3.0

    }
    SubShader {
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf Lambert
      struct Input {
          float2 uv_MainTex;
          float2 uv_BumpMap;
          float2 uv_BumpMap2;
          float3 viewDir;
      };
      sampler2D _MainTex;
      sampler2D _BumpMap;
      sampler2D _BumpMap2;
      float4 _RimColor;
      float _RimPower;
      float _BumpMapIntensity;
      float _BumpMap2Intensity;
      void surf (Input IN, inout SurfaceOutput o) {
          o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
          float3 normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap)) * _BumpMapIntensity;
          float3 normal2 = UnpackNormal (tex2D (_BumpMap2, IN.uv_BumpMap2)) * _BumpMap2Intensity;
          o.Normal = normal + normal2;
          o.Normal = normalize(o.Normal) * clamp(_BumpMapIntensity + _BumpMap2Intensity, 0, 1);
          half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
          o.Emission = _RimColor.rgb * pow (rim, _RimPower);
      }
      ENDCG
    } 
    Fallback "Diffuse"
  }