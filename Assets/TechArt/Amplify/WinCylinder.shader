// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TechArt/Effects/WinCylinder"
{
	Properties
	{
		_NoiseTile1("Noise Tile 1", Vector) = (5,1,0,0)
		[HDR]_Colour1("Colour 1", Color) = (0.08098996,1,0,0)
		_NoiseTile2("Noise Tile 2", Vector) = (5,2.5,0,0)
		[HDR]_Colour2("Colour 2", Color) = (0,0.7830722,1,0)
		_NoiseTile3("Noise Tile 3", Vector) = (5,1.63,0,0)
		[HDR]_Colour3("Colour 3", Color) = (0.69838,0,1,0)
		_Alpha("Alpha", Range( 0 , 1)) = 0
		_TimeScale("Time Scale", Float) = 1
		_Test("Test", Range( 1 , 5)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float2 _NoiseTile1;
		uniform float _TimeScale;
		uniform float4 _Colour1;
		uniform float2 _NoiseTile2;
		uniform float4 _Colour2;
		uniform float2 _NoiseTile3;
		uniform float4 _Colour3;
		uniform float _Test;
		uniform float _Alpha;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float mulTime11 = _Time.y * _TimeScale;
			float2 temp_cast_0 = (( mulTime11 * 1.0 )).xx;
			float2 uv_TexCoord2 = i.uv_texcoord * _NoiseTile1 + temp_cast_0;
			float simplePerlin2D1 = snoise( uv_TexCoord2 );
			float2 temp_cast_1 = (( mulTime11 * 0.73 )).xx;
			float2 uv_TexCoord7 = i.uv_texcoord * _NoiseTile2 + temp_cast_1;
			float simplePerlin2D6 = snoise( uv_TexCoord7 );
			float2 temp_cast_2 = (( mulTime11 * 0.29 )).xx;
			float2 uv_TexCoord8 = i.uv_texcoord * _NoiseTile3 + temp_cast_2;
			float simplePerlin2D4 = snoise( uv_TexCoord8 );
			float4 temp_output_5_0 = ( ( simplePerlin2D1 * _Colour1 ) + ( simplePerlin2D6 * _Colour2 ) + ( simplePerlin2D4 * _Colour3 ) );
			float4 temp_output_34_0 = ( temp_output_5_0 * _Test );
			o.Albedo = temp_output_34_0.rgb;
			o.Emission = temp_output_34_0.rgb;
			o.Alpha = ( _Alpha * temp_output_5_0 ).r;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17000
1500;73;1355;651;1756.121;85.71444;1.3;True;False
Node;AmplifyShaderEditor.RangedFloatNode;28;-2795.278,344.0863;Float;False;Property;_TimeScale;Time Scale;7;0;Create;True;0;0;False;0;1;50;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;11;-2613.462,347.6136;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;24;-2341.518,-202.8901;Float;False;Property;_NoiseTile1;Noise Tile 1;0;0;Create;True;0;0;False;0;5,1;5,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-2330.072,-59.99477;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-2341.288,357.1333;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.73;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;25;-2373.761,229.9588;Float;False;Property;_NoiseTile2;Noise Tile 2;2;0;Create;True;0;0;False;0;5,2.5;5,2.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;26;-2334.072,613.5375;Float;False;Property;_NoiseTile3;Noise Tile 3;4;0;Create;True;0;0;False;0;5,1.63;5,1.63;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-2313.153,752.7576;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.29;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;7;-2143.769,321.0275;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;5,5;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-2155.146,-115.154;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;3,3;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;8;-2140.48,724.8009;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;5,5;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;19;-1850.794,543.2659;Float;False;Property;_Colour2;Colour 2;3;1;[HDR];Create;True;0;0;False;0;0,0.7830722,1,0;0,0.5882353,0.7490196,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;18;-1848.568,946.092;Float;False;Property;_Colour3;Colour 3;5;1;[HDR];Create;True;0;0;False;0;0.69838,0,1,0;0.5254902,0,0.7490196,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;6;-1859.272,329.2501;Float;True;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;1;-1865.713,-62.52983;Float;True;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;20;-1863.111,153.5232;Float;False;Property;_Colour1;Colour 1;1;1;[HDR];Create;True;0;0;False;0;0.08098996,1,0,0;0.0627451,0.7490196,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;4;-1855.982,721.5119;Float;True;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-1393.327,26.20111;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-1616.081,458.435;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-1616.082,836.4902;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;5;-800.7997,94.9124;Float;True;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-505.5768,449.8426;Float;False;Property;_Alpha;Alpha;6;0;Create;True;0;0;False;0;0;0.97;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-838.3218,321.1856;Float;False;Property;_Test;Test;8;0;Create;True;0;0;False;0;0;2;1;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-470.4216,80.6856;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-206.483,350.3885;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;TechArt/Effects/WinCylinder;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;11;0;28;0
WireConnection;12;0;11;0
WireConnection;13;0;11;0
WireConnection;14;0;11;0
WireConnection;7;0;25;0
WireConnection;7;1;13;0
WireConnection;2;0;24;0
WireConnection;2;1;12;0
WireConnection;8;0;26;0
WireConnection;8;1;14;0
WireConnection;6;0;7;0
WireConnection;1;0;2;0
WireConnection;4;0;8;0
WireConnection;15;0;1;0
WireConnection;15;1;20;0
WireConnection;16;0;6;0
WireConnection;16;1;19;0
WireConnection;17;0;4;0
WireConnection;17;1;18;0
WireConnection;5;0;15;0
WireConnection;5;1;16;0
WireConnection;5;2;17;0
WireConnection;34;0;5;0
WireConnection;34;1;35;0
WireConnection;33;0;27;0
WireConnection;33;1;5;0
WireConnection;0;0;34;0
WireConnection;0;2;34;0
WireConnection;0;9;33;0
ASEEND*/
//CHKSM=D61209370A74D3B2174FAFE7B4E801D6BA10E99D