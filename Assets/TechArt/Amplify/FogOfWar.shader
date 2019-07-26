// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TechArt/FogOfWar"
{
	Properties
	{
		_TextureSheet("Texture Sheet", 2D) = "white" {}
		_Normals("Normals", 2D) = "white" {}
		[HDR]_Colour("Colour", Color) = (1,1,1,0)
		_FPS("FPS", Float) = 16
		_Alpha("Alpha", Float) = 0.65
		_Offset("Offset", Float) = 0
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

		uniform sampler2D _TextureSheet;
		uniform float _FPS;
		uniform float _Offset;
		uniform sampler2D _Normals;
		uniform float4 _Colour;
		uniform float _Alpha;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertexNormal = v.normal.xyz;
			// *** BEGIN Flipbook UV Animation vars ***
			// Total tiles of Flipbook Texture
			float fbtotaltiles3 = 8.0 * 8.0;
			// Offsets for cols and rows of Flipbook Texture
			float fbcolsoffset3 = 1.0f / 8.0;
			float fbrowsoffset3 = 1.0f / 8.0;
			// Speed of animation
			float fbspeed3 = _Time.y * _FPS;
			// UV Tiling (col and row offset)
			float2 fbtiling3 = float2(fbcolsoffset3, fbrowsoffset3);
			// UV Offset - calculate current tile linear index, and convert it to (X * coloffset, Y * rowoffset)
			// Calculate current tile linear index
			float fbcurrenttileindex3 = round( fmod( fbspeed3 + 0.0, fbtotaltiles3) );
			fbcurrenttileindex3 += ( fbcurrenttileindex3 < 0) ? fbtotaltiles3 : 0;
			// Obtain Offset X coordinate from current tile linear index
			float fblinearindextox3 = round ( fmod ( fbcurrenttileindex3, 8.0 ) );
			// Multiply Offset X by coloffset
			float fboffsetx3 = fblinearindextox3 * fbcolsoffset3;
			// Obtain Offset Y coordinate from current tile linear index
			float fblinearindextoy3 = round( fmod( ( fbcurrenttileindex3 - fblinearindextox3 ) / 8.0, 8.0 ) );
			// Reverse Y to get tiles from Top to Bottom
			fblinearindextoy3 = (int)(8.0-1) - fblinearindextoy3;
			// Multiply Offset Y by rowoffset
			float fboffsety3 = fblinearindextoy3 * fbrowsoffset3;
			// UV Offset
			float2 fboffset3 = float2(fboffsetx3, fboffsety3);
			// Flipbook UV
			half2 fbuv3 = v.texcoord.xy * fbtiling3 + fboffset3;
			// *** END Flipbook UV Animation vars ***
			float4 tex2DNode1 = tex2Dlod( _TextureSheet, float4( fbuv3, 0, 0.0) );
			v.vertex.xyz += ( ( ase_vertexNormal * tex2DNode1.a ) * _Offset );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			// *** BEGIN Flipbook UV Animation vars ***
			// Total tiles of Flipbook Texture
			float fbtotaltiles3 = 8.0 * 8.0;
			// Offsets for cols and rows of Flipbook Texture
			float fbcolsoffset3 = 1.0f / 8.0;
			float fbrowsoffset3 = 1.0f / 8.0;
			// Speed of animation
			float fbspeed3 = _Time.y * _FPS;
			// UV Tiling (col and row offset)
			float2 fbtiling3 = float2(fbcolsoffset3, fbrowsoffset3);
			// UV Offset - calculate current tile linear index, and convert it to (X * coloffset, Y * rowoffset)
			// Calculate current tile linear index
			float fbcurrenttileindex3 = round( fmod( fbspeed3 + 0.0, fbtotaltiles3) );
			fbcurrenttileindex3 += ( fbcurrenttileindex3 < 0) ? fbtotaltiles3 : 0;
			// Obtain Offset X coordinate from current tile linear index
			float fblinearindextox3 = round ( fmod ( fbcurrenttileindex3, 8.0 ) );
			// Multiply Offset X by coloffset
			float fboffsetx3 = fblinearindextox3 * fbcolsoffset3;
			// Obtain Offset Y coordinate from current tile linear index
			float fblinearindextoy3 = round( fmod( ( fbcurrenttileindex3 - fblinearindextox3 ) / 8.0, 8.0 ) );
			// Reverse Y to get tiles from Top to Bottom
			fblinearindextoy3 = (int)(8.0-1) - fblinearindextoy3;
			// Multiply Offset Y by rowoffset
			float fboffsety3 = fblinearindextoy3 * fbrowsoffset3;
			// UV Offset
			float2 fboffset3 = float2(fboffsetx3, fboffsety3);
			// Flipbook UV
			half2 fbuv3 = i.uv_texcoord * fbtiling3 + fboffset3;
			// *** END Flipbook UV Animation vars ***
			o.Normal = tex2D( _Normals, fbuv3 ).rgb;
			float4 tex2DNode1 = tex2D( _TextureSheet, fbuv3 );
			float4 temp_output_8_0 = ( _Colour * tex2DNode1 );
			o.Albedo = temp_output_8_0.rgb;
			o.Emission = temp_output_8_0.rgb;
			o.Alpha = ( tex2DNode1.a * _Alpha );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows vertex:vertexDataFunc 

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
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
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
Version=16800
1855;73;1983;648;-1271.341;-124.056;1;True;False
Node;AmplifyShaderEditor.TextureCoordinatesNode;4;903,56;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;7;976,313;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;993,242;Float;False;Property;_FPS;FPS;3;0;Create;True;0;0;False;0;16;16;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;5;995,172;Float;False;Constant;_Float0;Float 0;2;0;Create;True;0;0;False;0;8;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCFlipBookUVAnimation;3;1156,138;Float;False;0;0;6;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SamplerNode;1;1534,50;Float;True;Property;_TextureSheet;Texture Sheet;0;0;Create;True;0;0;False;0;d34cf0b1a6163ad4abc469a5ed4d036f;d34cf0b1a6163ad4abc469a5ed4d036f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalVertexDataNode;14;1649.341,470.056;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;13;1816.838,300.5442;Float;False;Property;_Alpha;Alpha;4;0;Create;True;0;0;False;0;0.65;0.65;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;9;1838,-117;Float;False;Property;_Colour;Colour;2;1;[HDR];Create;True;0;0;False;0;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;2047.341,587.056;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;18;2011.341,690.056;Float;False;Property;_Offset;Offset;5;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;1979.838,153.5442;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;2047,49;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;2;1535,237;Float;True;Property;_Normals;Normals;1;0;Create;True;0;0;False;0;c559870912e037b4384a2dcb95a56c29;c559870912e037b4384a2dcb95a56c29;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;2220.341,567.056;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2369,176;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;TechArt/FogOfWar;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;3;0;4;0
WireConnection;3;1;5;0
WireConnection;3;2;5;0
WireConnection;3;3;6;0
WireConnection;3;5;7;0
WireConnection;1;1;3;0
WireConnection;16;0;14;0
WireConnection;16;1;1;4
WireConnection;12;0;1;4
WireConnection;12;1;13;0
WireConnection;8;0;9;0
WireConnection;8;1;1;0
WireConnection;2;1;3;0
WireConnection;17;0;16;0
WireConnection;17;1;18;0
WireConnection;0;0;8;0
WireConnection;0;1;2;0
WireConnection;0;2;8;0
WireConnection;0;9;12;0
WireConnection;0;11;17;0
ASEEND*/
//CHKSM=73A36442A02941AC61B197963FC49E21DE90A0A0