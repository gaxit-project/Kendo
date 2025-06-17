Shader "Custom/DitherFade"
{
    Properties
    {
        // Imageコンポーネントからテクスチャを受け取るためのプロパティ。インスペクターにはほぼ表示されません。
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        // ディザリングのしきい値を調整するプロパティ
        [Range(0.0, 1.0)]
        _DitherThreshold ("Dither Threshold", Float) = 0.5
    }
    SubShader
    {
        // UI用のタグ設定
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // 通常のUIと同じアルファブレンディング
            Cull Off
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;      // ★ ImageコンポーネントのColorプロパティを受け取る
                float2 uv       : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;      // ★ フラグメントシェーダーへ頂点カラーを渡す
                float2 uv       : TEXCOORD0;
                float4 screenPos: TEXCOORD1;
            };

            sampler2D _MainTex;
            float _DitherThreshold;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                
                o.uv = v.uv;
                o.color = v.color; // ★ 頂点カラーをそのままフラグメントシェーダーへ
                return o;
            }
            
            // Interleaved Gradient Noise を計算する関数
            float interleavedGradientNoise(float2 screenPos)
            {
                float magic = 0.06711056 * screenPos.x + 0.00583715 * screenPos.y;
                return frac(52.9829189 * frac(magic));
            }

            fixed4 frag (v2f i) : SV_TARGET
            {
                // --- ディザリング処理（変更なし） ---
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float2 screenPixelPos = screenUV * _ScreenParams.xy;
                float noise = interleavedGradientNoise(screenPixelPos);
                clip(noise - _DitherThreshold);

                // --- ★色の計算方法を修正 ---
                // Imageのテクスチャ色と、ImageのColorプロパティ(i.color)を乗算する
                // これにより、Unityの標準UIと同じ色の付き方になります。
                fixed4 finalColor = tex2D(_MainTex, i.uv) * i.color;
                
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
}