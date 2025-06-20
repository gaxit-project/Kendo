Shader "Custom/DitherFade"
{
    // --- インスペクターに表示されるプロパティを定義 ---
    Properties
    {
        // UnityのUI(Imageなど)からテクスチャ情報を受け取るための標準的なプロパティ
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        // C#スクリプトから操作するための、ディザリングの閾値（透明度）を調整するプロパティ
        [Range(0.0, 1.0)]
        _DitherThreshold ("Dither Threshold", Float) = 0.5
    }

    // --- シェーダー本体 ---
    SubShader
    {
        // --- レンダリング方法に関する設定 ---
        Tags
        {
            "Queue"="Transparent"      // 描画順序を透明オブジェクトとして扱う
            "RenderType"="Transparent" // Unityにこのシェーダーが透過処理を行うものであると伝える
            "IgnoreProjector"="True"   // プロジェクター（特殊なライティング）の影響を受けないようにする
        }

        // --- 描画パス ---
        Pass
        {
            // --- レンダーステートの設定 ---
            Blend SrcAlpha OneMinusSrcAlpha // 通常のUIで使われる、ごく一般的なアルファブレンディング設定
            Cull Off                       // ポリゴンの裏面を描画する（UIでは通常Off）
            ZWrite Off                     // 深度バッファへの書き込みをしない（UIでは通常Off）

            // --- ここからがシェーダープログラム本体 ---
            CGPROGRAM
            // 使用するシェーダー関数を定義
            #pragma vertex vert   // 頂点シェーダーとしてvert関数を使用
            #pragma fragment frag // フラグメントシェーダーとしてfrag関数を使用
            
            // Unityの標準的なシェーダー変数や関数を定義したファイルを読み込む
            #include "UnityCG.cginc"

            // --- 構造体の定義 ---

            // 頂点シェーダーへの入力データ構造
            struct appdata
            {
                float4 vertex   : POSITION;  // 頂点座標
                float4 color    : COLOR;     // ImageコンポーネントのColorプロパティ
                float2 uv       : TEXCOORD0; // テクスチャのUV座標
            };

            // 頂点シェーダーからフラグメントシェーダーへ渡すデータ構造
            struct v2f
            {
                float4 vertex   : SV_POSITION; // スクリーン上の頂点座標
                float4 color    : COLOR;       // 補間された頂点カラー
                float2 uv       : TEXCOORD0;   // 補間されたUV座標
                float4 screenPos: TEXCOORD1;   // ディザリング計算用のスクリーン座標
            };

            // --- グローバル変数 ---
            sampler2D _MainTex;       // Propertiesで定義したテクスチャ
            float _DitherThreshold;   // Propertiesで定義したディザの閾値

            // --- 頂点シェーダー ---
            // メッシュの各頂点に対して呼ばれる関数
            v2f vert (appdata v)
            {
                v2f o; // 出力用の構造体を準備
                // 頂点座標を3D空間からスクリーン上の2D座標に変換
                o.vertex = UnityObjectToClipPos(v.vertex);
                // ディザリング計算用に、スクリーン座標を別途計算して保持
                o.screenPos = ComputeScreenPos(o.vertex);
                
                // UV座標とカラーをフラグメントシェーダーにそのまま渡す
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }
            
            //  Interleaved Gradient Noise関数 
            // スクリーン座標を元に、高品質なノイズ値を生成する
            float interleavedGradientNoise(float2 screenPos)
            {
                float magic = 0.06711056 * screenPos.x + 0.00583715 * screenPos.y;
                return frac(52.9829189 * frac(magic));
            }

            // --- フラグメントシェーダー ---
            // スクリーン上の各ピクセルに対して呼ばれる関数
            fixed4 frag (v2f i) : SV_TARGET
            {
                // Imageに設定されたテクスチャの色と、Colorプロパティの色を乗算する
                fixed4 finalColor = tex2D(_MainTex, i.uv) * i.color;
                
                // ノイズ値を計算
                // スクリーン座標を取得
                float2 screenPixelPos = i.screenPos.xy / i.screenPos.w * _ScreenParams.xy;
                // ノイズ関数を呼び出し、0.0～1.0の範囲のノイズ値を取得
                float noise = interleavedGradientNoise(screenPixelPos);

                // 大きければ、このピクセルは表示(alpha=1.0)、そうでなければ非表示(alpha=0.0)
                float ditherAlpha = _DitherThreshold > noise ? 1.0 : 0.0;

                // 計算したディザのアルファ値を乗算する
                finalColor.a *= ditherAlpha;
                
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
}