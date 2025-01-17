// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TechArt/Abilities/Shield"
{
	Properties
	{
		[HDR]_EdgeColour("Edge Colour", Color) = (1,0.6078432,0,0)
		[HDR]_EdgeColour2("Edge Colour2", Color) = (1,0,0,0)
		_LERP("LERP", Range( 0.1 , 1)) = 0.1
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_HexTex("HexTex", 2D) = "white" {}
		_HexTiling("Hex Tiling", Vector) = (3,3,0,0)
		_ScrollSpeed("Scroll Speed", Float) = 0.05
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Custom"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		
		AlphaToMask On
		CGPROGRAM
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha noshadow 
		struct Input
		{
			float4 screenPos;
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
		};

		uniform float4 _EdgeColour2;
		uniform float4 _EdgeColour;
		uniform float _LERP;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform sampler2D _HexTex;
		uniform float2 _HexTiling;
		uniform float _ScrollSpeed;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float LERP22 = _LERP;
			float4 lerpResult19 = lerp( _EdgeColour2 , _EdgeColour , LERP22);
			o.Emission = lerpResult19.rgb;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth2 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,UNITY_PROJ_COORD( ase_screenPos )));
			float distanceDepth2 = abs( ( screenDepth2 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( ( 1.0 - (0.0 + (LERP22 - 0.0) * (10.0 - 0.0) / (1.0 - 0.0)) ) ) );
			float clampResult3 = clamp( distanceDepth2 , 0.0 , 1.0 );
			float mulTime17 = _Time.y * _ScrollSpeed;
			float2 temp_cast_1 = (mulTime17).xx;
			float2 uv_TexCoord14 = i.uv_texcoord * _HexTiling + temp_cast_1;
			float4 tex2DNode7 = tex2D( _HexTex, uv_TexCoord14 );
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV40 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode40 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV40, (0.0 + (LERP22 - 0.1) * (0.35 - 0.0) / (1.0 - 0.1)) ) );
			o.Alpha = ( ( ( ( 1.0 - clampResult3 ) * 1.0 ) + 0.0 ) * tex2DNode7 * fresnelNode40 ).r;
			clip( tex2DNode7.a - _Cutoff );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16800
1676;73;1029;656;1634.467;179.7159;1.594522;True;False
Node;AmplifyShaderEditor.RangedFloatNode;21;-1763.875,120.981;Float;False;Property;_LERP;LERP;2;0;Create;True;0;0;False;0;0.1;0;0.1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;22;-1467.121,120.5821;Float;False;LERP;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;34;-1208.808,103.5447;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;35;-1028.096,77.54375;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;2;-878.1229,57.43546;Float;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;3;-636.2213,69.13546;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-934.526,339.7498;Float;False;Property;_ScrollSpeed;Scroll Speed;6;0;Create;True;0;0;False;0;0.05;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;4;-488.1954,84.70373;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;15;-807.5261,173.7492;Float;False;Property;_HexTiling;Hex Tiling;5;0;Create;True;0;0;False;0;3,3;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;17;-769.5261,343.7498;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;43;-725.5895,505.9278;Float;False;22;LERP;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;45;-489.599,483.6048;Float;False;5;0;FLOAT;0;False;1;FLOAT;0.1;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;0.35;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-308.2092,77.34947;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;14;-604.236,270.2727;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;7;-329.9839,212.3961;Float;True;Property;_HexTex;HexTex;4;0;Create;True;0;0;False;0;decdb5d510cb58844aa1e65da66a1d38;decdb5d510cb58844aa1e65da66a1d38;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;5;-453.9697,-197.0222;Float;False;Property;_EdgeColour;Edge Colour;0;1;[HDR];Create;True;0;0;False;0;1,0.6078432,0,0;0.4196078,0,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;20;-454.9955,-365.1609;Float;False;Property;_EdgeColour2;Edge Colour2;1;1;[HDR];Create;True;0;0;False;0;1,0,0,0;1,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;23;-76.32357,-114.5171;Float;False;22;LERP;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;8;-151.866,77.71764;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;40;-270.8691,401.5349;Float;False;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;19;125.1265,-191.519;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-11.64087,73.44778;Float;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;359,-129;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;TechArt/Abilities/Shield;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;1;Custom;0.5;True;False;0;True;Custom;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;1;False;-1;1;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;3;-1;-1;-1;0;True;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;22;0;21;0
WireConnection;34;0;22;0
WireConnection;35;0;34;0
WireConnection;2;0;35;0
WireConnection;3;0;2;0
WireConnection;4;0;3;0
WireConnection;17;0;18;0
WireConnection;45;0;43;0
WireConnection;6;0;4;0
WireConnection;14;0;15;0
WireConnection;14;1;17;0
WireConnection;7;1;14;0
WireConnection;8;0;6;0
WireConnection;40;3;45;0
WireConnection;19;0;20;0
WireConnection;19;1;5;0
WireConnection;19;2;23;0
WireConnection;12;0;8;0
WireConnection;12;1;7;0
WireConnection;12;2;40;0
WireConnection;0;2;19;0
WireConnection;0;9;12;0
WireConnection;0;10;7;4
ASEEND*/
//CHKSM=F9A62C06DA4DD83F114B13EEC9971ADD03F1EDF2