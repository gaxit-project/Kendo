Shader "Custom/DitherFade"
{
    // --- �C���X�y�N�^�[�ɕ\�������v���p�e�B���` ---
    Properties
    {
        // Unity��UI(Image�Ȃ�)����e�N�X�`�������󂯎�邽�߂̕W���I�ȃv���p�e�B
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        // C#�X�N���v�g���瑀�삷�邽�߂́A�f�B�U�����O��臒l�i�����x�j�𒲐�����v���p�e�B
        [Range(0.0, 1.0)]
        _DitherThreshold ("Dither Threshold", Float) = 0.5
    }

    // --- �V�F�[�_�[�{�� ---
    SubShader
    {
        // --- �����_�����O���@�Ɋւ���ݒ� ---
        Tags
        {
            "Queue"="Transparent"      // �`�揇���𓧖��I�u�W�F�N�g�Ƃ��Ĉ���
            "RenderType"="Transparent" // Unity�ɂ��̃V�F�[�_�[�����ߏ������s�����̂ł���Ɠ`����
            "IgnoreProjector"="True"   // �v���W�F�N�^�[�i����ȃ��C�e�B���O�j�̉e�����󂯂Ȃ��悤�ɂ���
        }

        // --- �`��p�X ---
        Pass
        {
            // --- �����_�[�X�e�[�g�̐ݒ� ---
            Blend SrcAlpha OneMinusSrcAlpha // �ʏ��UI�Ŏg����A������ʓI�ȃA���t�@�u�����f�B���O�ݒ�
            Cull Off                       // �|���S���̗��ʂ�`�悷��iUI�ł͒ʏ�Off�j
            ZWrite Off                     // �[�x�o�b�t�@�ւ̏������݂����Ȃ��iUI�ł͒ʏ�Off�j

            // --- �������炪�V�F�[�_�[�v���O�����{�� ---
            CGPROGRAM
            // �g�p����V�F�[�_�[�֐����`
            #pragma vertex vert   // ���_�V�F�[�_�[�Ƃ���vert�֐����g�p
            #pragma fragment frag // �t���O�����g�V�F�[�_�[�Ƃ���frag�֐����g�p
            
            // Unity�̕W���I�ȃV�F�[�_�[�ϐ���֐����`�����t�@�C����ǂݍ���
            #include "UnityCG.cginc"

            // --- �\���̂̒�` ---

            // ���_�V�F�[�_�[�ւ̓��̓f�[�^�\��
            struct appdata
            {
                float4 vertex   : POSITION;  // ���_���W
                float4 color    : COLOR;     // Image�R���|�[�l���g��Color�v���p�e�B
                float2 uv       : TEXCOORD0; // �e�N�X�`����UV���W
            };

            // ���_�V�F�[�_�[����t���O�����g�V�F�[�_�[�֓n���f�[�^�\��
            struct v2f
            {
                float4 vertex   : SV_POSITION; // �X�N���[����̒��_���W
                float4 color    : COLOR;       // ��Ԃ��ꂽ���_�J���[
                float2 uv       : TEXCOORD0;   // ��Ԃ��ꂽUV���W
                float4 screenPos: TEXCOORD1;   // �f�B�U�����O�v�Z�p�̃X�N���[�����W
            };

            // --- �O���[�o���ϐ� ---
            sampler2D _MainTex;       // Properties�Œ�`�����e�N�X�`��
            float _DitherThreshold;   // Properties�Œ�`�����f�B�U��臒l

            // --- ���_�V�F�[�_�[ ---
            // ���b�V���̊e���_�ɑ΂��ČĂ΂��֐�
            v2f vert (appdata v)
            {
                v2f o; // �o�͗p�̍\���̂�����
                // ���_���W��3D��Ԃ���X�N���[�����2D���W�ɕϊ�
                o.vertex = UnityObjectToClipPos(v.vertex);
                // �f�B�U�����O�v�Z�p�ɁA�X�N���[�����W��ʓr�v�Z���ĕێ�
                o.screenPos = ComputeScreenPos(o.vertex);
                
                // UV���W�ƃJ���[���t���O�����g�V�F�[�_�[�ɂ��̂܂ܓn��
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }
            
            //  Interleaved Gradient Noise�֐� 
            // �X�N���[�����W�����ɁA���i���ȃm�C�Y�l�𐶐�����
            float interleavedGradientNoise(float2 screenPos)
            {
                float magic = 0.06711056 * screenPos.x + 0.00583715 * screenPos.y;
                return frac(52.9829189 * frac(magic));
            }

            // --- �t���O�����g�V�F�[�_�[ ---
            // �X�N���[����̊e�s�N�Z���ɑ΂��ČĂ΂��֐�
            fixed4 frag (v2f i) : SV_TARGET
            {
                // Image�ɐݒ肳�ꂽ�e�N�X�`���̐F�ƁAColor�v���p�e�B�̐F����Z����
                fixed4 finalColor = tex2D(_MainTex, i.uv) * i.color;
                
                // �m�C�Y�l���v�Z
                // �X�N���[�����W���擾
                float2 screenPixelPos = i.screenPos.xy / i.screenPos.w * _ScreenParams.xy;
                // �m�C�Y�֐����Ăяo���A0.0�`1.0�͈̔͂̃m�C�Y�l���擾
                float noise = interleavedGradientNoise(screenPixelPos);

                // �傫����΁A���̃s�N�Z���͕\��(alpha=1.0)�A�����łȂ���Δ�\��(alpha=0.0)
                float ditherAlpha = _DitherThreshold > noise ? 1.0 : 0.0;

                // �v�Z�����f�B�U�̃A���t�@�l����Z����
                finalColor.a *= ditherAlpha;
                
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
}