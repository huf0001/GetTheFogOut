// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TechArt/Effects/AbilityRange"
{
	Properties
	{
		[HDR]_Colour("Colour", Color) = (0,0.1323529,0.007843138,0)
		_IntersectIntensity("Intersect Intensity", Range( 0 , 5)) = 0.2
		_Alpha("Alpha", Float) = 0
		_Emission("Emission", Range( 0 , 1)) = 0.5
		_FresnelPower("Fresnel Power", Float) = 1
		[HDR]_FresnelColour("Fresnel Colour", Color) = (1.335078,1.335078,0,0)
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+10" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha noshadow exclude_path:deferred 
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			float4 screenPos;
		};

		uniform float4 _Colour;
		uniform float _Emission;
		uniform float4 _FresnelColour;
		uniform float _FresnelPower;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _IntersectIntensity;
		uniform float _Alpha;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = ( _Colour * _Emission ).rgb;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV52 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode52 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV52, _FresnelPower ) );
			o.Emission = ( _FresnelColour * fresnelNode52 ).rgb;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth4 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth4 = abs( ( screenDepth4 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _IntersectIntensity ) );
			float clampResult5 = clamp( distanceDepth4 , 0.0 , 1.0 );
			o.Alpha = ( ( 1.0 - clampResult5 ) * _Alpha );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17000
1825;73;1255;656;1870.494;1528.272;1.3;True;False
Node;AmplifyShaderEditor.RangedFloatNode;3;-1336.992,87.27348;Float;False;Property;_IntersectIntensity;Intersect Intensity;1;0;Create;True;0;0;False;0;0.2;5;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;4;-1043.883,82.40861;Float;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;55;-943.0748,-191.2544;Float;False;Property;_FresnelPower;Fresnel Power;4;0;Create;True;0;0;False;0;1;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;5;-801.9815,94.10854;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-620.0276,211.5887;Float;False;Property;_Alpha;Alpha;2;0;Create;True;0;0;False;0;0;0.75;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;52;-709.4023,-200.3315;Float;True;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;6;-625.8816,139.5081;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-1164.823,-1319.017;Float;False;Property;_Emission;Emission;3;0;Create;True;0;0;False;0;0.5;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1;-1100.199,-1499.156;Float;False;Property;_Colour;Colour;0;1;[HDR];Create;True;0;0;False;0;0,0.1323529,0.007843138,0;0.7490196,0.1176471,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;56;-853.0328,-470.2637;Float;False;Property;_FresnelColour;Fresnel Colour;5;1;[HDR];Create;True;0;0;False;0;1.335078,1.335078,0,0;1.335078,1.335078,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LengthOpNode;14;-2962.825,1078.86;Float;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;26;-2488.505,1127.869;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;42;-2038.84,1310.817;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;37;-2310.656,1143.505;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-3274.844,1332.987;Float;False;Constant;_Float0;Float 0;6;0;Create;True;0;0;False;0;0.8114148;0.8114148;-2;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;29;-2489.505,1349.869;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;54;-330.5493,-243.9428;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-441.2309,119.558;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;15;-3203.031,1078.425;Float;False;5;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;1,1;False;3;FLOAT2;-1,-1;False;4;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;11;-3492.728,1055.195;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;50;-2028.868,1539.65;Float;False;Constant;_Color0;Color 0;4;1;[HDR];Create;True;0;0;False;0;21.36125,21.36125,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;-1692.75,1335.044;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;31;-2927.505,1349.569;Float;False;5;0;FLOAT;0;False;1;FLOAT;-2;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;24;-2731.505,1132.869;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;28;-2729.505,1349.869;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-814.9052,-1379.974;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;25.96349,-9.565496;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;TechArt/Effects/AbilityRange;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;10;False;Transparent;;Transparent;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;4;0;3;0
WireConnection;5;0;4;0
WireConnection;52;3;55;0
WireConnection;6;0;5;0
WireConnection;14;0;15;0
WireConnection;26;0;24;0
WireConnection;42;0;37;0
WireConnection;42;1;29;0
WireConnection;37;1;26;0
WireConnection;29;0;28;0
WireConnection;54;0;56;0
WireConnection;54;1;52;0
WireConnection;7;0;6;0
WireConnection;7;1;8;0
WireConnection;15;0;11;0
WireConnection;49;0;42;0
WireConnection;49;1;50;0
WireConnection;31;0;25;0
WireConnection;24;0;14;0
WireConnection;24;1;25;0
WireConnection;28;0;14;0
WireConnection;28;1;31;0
WireConnection;9;0;1;0
WireConnection;9;1;10;0
WireConnection;0;0;9;0
WireConnection;0;2;54;0
WireConnection;0;9;7;0
ASEEND*/
//CHKSM=13F41BC1754DFAF888227E60A7E10B8198925A70