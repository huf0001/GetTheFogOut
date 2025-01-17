// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TechArt/MinimapTiles"
{
	Properties
	{
		[PerRendererData]_BaseColor("BaseColor", Color) = (0,1,0,1)
		[PerRendererData]_FogColour("Fog Colour", Color) = (0.6037736,0.6037736,0.6037736,0)
		[PerRendererData]_Foggyness("Foggyness", Range( 0 , 1)) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha noshadow 
		struct Input
		{
			half filler;
		};

		uniform float4 _BaseColor;
		uniform float4 _FogColour;
		uniform float _Foggyness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 lerpResult3 = lerp( _BaseColor , _FogColour , (0.0 + (_Foggyness - 0.0) * (0.7 - 0.0) / (1.0 - 0.0)));
			o.Albedo = lerpResult3.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16900
7;75;3826;944;2057.712;210.4922;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;4;-789.3934,243.0289;Float;False;Property;_Foggyness;Foggyness;2;1;[PerRendererData];Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;6;-488.7124,193.0078;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;0.7;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1;-729.0514,-90.93006;Float;False;Property;_BaseColor;BaseColor;0;1;[PerRendererData];Create;True;0;0;False;0;0,1,0,1;0.1662067,0.7830188,0.2721795,0.3137255;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;2;-738.6239,76.01767;Float;False;Property;_FogColour;Fog Colour;1;1;[PerRendererData];Create;True;0;0;False;0;0.6037736,0.6037736,0.6037736,0;0.6037736,0.6037736,0.6037736,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;3;-292.3704,43.95147;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;TechArt/MinimapTiles;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;6;0;4;0
WireConnection;3;0;1;0
WireConnection;3;1;2;0
WireConnection;3;2;6;0
WireConnection;0;0;3;0
ASEEND*/
//CHKSM=7624DC3D04E519E0ED5270FD1240280A71F4854E