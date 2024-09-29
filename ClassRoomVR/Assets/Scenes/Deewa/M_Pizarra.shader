Shader "lit/PaintShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Coordinates("Coordinate", Vector) = (0, 0, 0, 0)
        _Color("Draw Color", Color) = (1, 0, 0, 1)
        _Size("Size", Range(.01,5000)) = 5000
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

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
            float4 _MainTex_ST;
            fixed4 _Coordinates, _Color;
            float _Size; // Definir _Size como una variable float

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                // Convertir las coordenadas de textura a partir de _Coordinates
                float2 distanceUV = abs(i.uv - _Coordinates.xy) * _MainTex_ST.xy;

                // Calcular la distancia euclidiana normalizada
                float draw = pow(saturate(1 - length(distanceUV)), 700); // Usar _Size

                fixed4 drawcol = _Color * ((draw * 9));
                return saturate(col + drawcol);
            }
            ENDCG
        }
    }
}
