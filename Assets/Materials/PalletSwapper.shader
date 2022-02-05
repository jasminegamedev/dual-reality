Shader "Custom/PalletSwapper"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Darkest("Darkest", color) = (0.0588235, 0.21961, 0.0588235)
        _Dark("Dark", color) = (0.188235, 0.38431, 0.188235)
        _Ligt("Light", color) = (0.545098, 0.6745098, 0.0588235)
        _Ligtest("Lightest", color) = (0.607843, 0.7372549, 0.0588235)
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _Darkest, _Dark, _Ligt, _Ligtest;


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
              float4 col = tex2D(_MainTex, i.uv);

              if (col.r <= 0.24)
              {
                  col = _Darkest;
              }
              else if (col.r > 0.24 && col.r <= 0.49)
              {
                  col = _Dark;
              }
              else if (col.r > 0.49 && col.r <= 0.76)
              {
                  col = _Ligt;
              }
              else if (col.r > 0.76)
              {
                  col = _Ligtest;
              }

              return col;
            }
            ENDCG
        }
    }
}
