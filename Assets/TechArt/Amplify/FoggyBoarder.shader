// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TechArt/Effects/FoggyBoarder"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_Depth("Depth", Float) = 50
		_Float1("Float 1", Float) = 16
		[HDR]_FogColour("Fog Colour", Color) = (0.2358491,0.2358491,0.2358491,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha noshadow exclude_path:deferred 
		struct Input
		{
			float4 screenPos;
			float2 uv_texcoord;
		};

		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _Depth;
		uniform float4 _FogColour;
		uniform sampler2D _TextureSample0;
		uniform float _Float1;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth1 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,UNITY_PROJ_COORD( ase_screenPos )));
			float distanceDepth1 = abs( ( screenDepth1 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _Depth ) );
			// *** BEGIN Flipbook UV Animation vars ***
			// Total tiles of Flipbook Texture
			float fbtotaltiles20 = 8.0 * 8.0;
			// Offsets for cols and rows of Flipbook Texture
			float fbcolsoffset20 = 1.0f / 8.0;
			float fbrowsoffset20 = 1.0f / 8.0;
			// Speed of animation
			float fbspeed20 = _Time.y * _Float1;
			// UV Tiling (col and row offset)
			float2 fbtiling20 = float2(fbcolsoffset20, fbrowsoffset20);
			// UV Offset - calculate current tile linear index, and convert it to (X * coloffset, Y * rowoffset)
			// Calculate current tile linear index
			float fbcurrenttileindex20 = round( fmod( fbspeed20 + 0.0, fbtotaltiles20) );
			fbcurrenttileindex20 += ( fbcurrenttileindex20 < 0) ? fbtotaltiles20 : 0;
			// Obtain Offset X coordinate from current tile linear index
			float fblinearindextox20 = round ( fmod ( fbcurrenttileindex20, 8.0 ) );
			// Multiply Offset X by coloffset
			float fboffsetx20 = fblinearindextox20 * fbcolsoffset20;
			// Obtain Offset Y coordinate from current tile linear index
			float fblinearindextoy20 = round( fmod( ( fbcurrenttileindex20 - fblinearindextox20 ) / 8.0, 8.0 ) );
			// Reverse Y to get tiles from Top to Bottom
			fblinearindextoy20 = (int)(8.0-1) - fblinearindextoy20;
			// Multiply Offset Y by rowoffset
			float fboffsety20 = fblinearindextoy20 * fbrowsoffset20;
			// UV Offset
			float2 fboffset20 = float2(fboffsetx20, fboffsety20);
			// Flipbook UV
			half2 fbuv20 = i.uv_texcoord * fbtiling20 + fboffset20;
			// *** END Flipbook UV Animation vars ***
			o.Emission = ( ( 1.0 - distanceDepth1 ) * _FogColour * tex2D( _TextureSample0, fbuv20 ).a ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16800
1927;7;1906;1004;1362.834;391.1192;1.251994;True;True
Node;AmplifyShaderEditor.RangedFloatNode;2;-549,-76.5;Float;False;Property;_Depth;Depth;1;0;Create;True;0;0;False;0;50;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;16;-910.7493,281.6017;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;17;-837.7493,538.6013;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-820.7491,467.6015;Float;False;Property;_Float1;Float 1;2;0;Create;True;0;0;False;0;16;16;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-818.7491,397.6015;Float;False;Constant;_Float2;Float 2;2;0;Create;True;0;0;False;0;8;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;1;-344,-85.5;Float;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCFlipBookUVAnimation;20;-657.7492,363.6016;Float;False;0;0;6;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.ColorNode;3;-343,29.5;Float;False;Property;_FogColour;Fog Colour;3;1;[HDR];Create;True;0;0;False;0;0.2358491,0.2358491,0.2358491,0;0.2358491,0.2358491,0.2358491,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;5;-91,-70.5;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;21;-279.7495,275.6017;Float;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;False;0;d34cf0b1a6163ad4abc469a5ed4d036f;d34cf0b1a6163ad4abc469a5ed4d036f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;65,-51;Float;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;253,-163;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;TechArt/Effects/FoggyBoarder;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;0;0;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;1;0;2;0
WireConnection;20;0;16;0
WireConnection;20;1;19;0
WireConnection;20;2;19;0
WireConnection;20;3;18;0
WireConnection;20;5;17;0
WireConnection;5;0;1;0
WireConnection;21;1;20;0
WireConnection;4;0;5;0
WireConnection;4;1;3;0
WireConnection;4;2;21;4
WireConnection;0;2;4;0
ASEEND*/
//CHKSM=461BAA1D64BF2B32BDC341562D0F1325E13BC551