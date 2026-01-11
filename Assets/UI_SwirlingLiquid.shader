Shader "UI/SwirlingLiquid"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {} // C'est le masque circulaire
        _LiquidTex ("Liquid Texture", 2D) = "white" {} // C'est la texture qui tourne (lave/eau)
        _Color ("Tint", Color) = (1,1,1,1) // La couleur de l'émotion (Rouge/Bleu/Violet)
        _FillAmount ("Fill Amount", Range(0,1)) = 1.0
        _SwirlSpeed ("Swirl Speed", Range(-5,5)) = 1.0
        
        // Nécessaire pour l'UI Unity
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };
            
            sampler2D _MainTex; // Le masque circulaire
            sampler2D _LiquidTex; // La texture de lave
            fixed4 _Color;
            float _FillAmount;
            float _SwirlSpeed;
            float4 _MainTex_ST;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 1. Gestion du remplissage circulaire (Radial Fill)
                float2 uv = IN.texcoord - float2(0.5, 0.5);
                float angle = atan2(uv.y, uv.x); // Angle en radians (-PI à PI)
                float normalizedAngle = angle / (2.0 * 3.14159) + 0.5; // Convertir en 0 à 1
                
                // Si l'angle est au-dessus du FillAmount, on coupe (rend transparent)
                clip(_FillAmount - normalizedAngle);

                // 2. Gestion de la texture tourbillonnante
                float2 swirlUV = IN.texcoord;
                // Rotation simple basée sur le temps
                float sinX = sin(_Time.y * _SwirlSpeed);
                float cosX = cos(_Time.y * _SwirlSpeed);
                float2x2 rotationMatrix = float2x2(cosX, -sinX, sinX, cosX);
                swirlUV = mul(swirlUV - 0.5, rotationMatrix) + 0.5;

                fixed4 liquidColor = tex2D(_LiquidTex, swirlUV);
                
                // 3. Le masque circulaire final
                fixed4 maskColor = tex2D(_MainTex, IN.texcoord);
                
                // Combine tout : La couleur de l'émotion * la texture liquide * le masque rond
                return IN.color * liquidColor * maskColor.a;
            }
            ENDCG
        }
    }
}