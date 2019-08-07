// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TechArt/Effects/PickupPulse2"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_Tint("Tint", Color) = (1,1,1,0)
		_Normals("Normals", 2D) = "white" {}
		_NormalStrength("Normal Strength", Float) = 1
		_Metallic("Metallic", 2D) = "white" {}
		_MetallicStrength("Metallic Strength", Float) = 0
		_Roughness("Roughness", 2D) = "white" {}
		_RoughnessStrength("Roughness Strength", Float) = 0
		_Emission("Emission", 2D) = "white" {}
		_EmmisionStrength("Emmision Strength", Float) = 0
		_EdgeSpeed("Edge Speed", Float) = 5
		[HDR]_EdgeColour("Edge Colour", Color) = (1,0,0,0)
		_EdgeWidth("Edge Width", Range( 0 , 1000)) = 1
		[Toggle]_Toggle("Toggle", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ }
		Cull Front
		CGPROGRAM
		#pragma target 3.0
		#pragma surface outlineSurf Outline nofog  keepalpha noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap nometa noforwardadd vertex:outlineVertexDataFunc 
		
		#include "UnityShaderVariables.cginc"
		
		
		struct Input
		{
			half filler;
		};
		uniform float _Toggle;
		uniform float _EdgeWidth;
		uniform float _EdgeSpeed;
		uniform float4 _EdgeColour;
		
		void outlineVertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float mulTime40 = _Time.y * _EdgeSpeed;
			float outlineVar = (0.0 + (( lerp(_EdgeWidth,0.0,_Toggle) * (0.0 + (sin( mulTime40 ) - -1.0) * (1.0 - 0.0) / (1.0 - -1.0)) ) - 0.0) * (0.0005 - 0.0) / (1.0 - 0.0));
			v.vertex.xyz *= ( 1 + outlineVar);
		}
		inline half4 LightingOutline( SurfaceOutput s, half3 lightDir, half atten ) { return half4 ( 0,0,0, s.Alpha); }
		void outlineSurf( Input i, inout SurfaceOutput o )
		{
			o.Emission = _EdgeColour.rgb;
		}
		ENDCG
		

		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Normals;
		uniform float4 _Normals_ST;
		uniform float _NormalStrength;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float4 _Tint;
		uniform sampler2D _Emission;
		uniform float4 _Emission_ST;
		uniform float _EmmisionStrength;
		uniform sampler2D _Metallic;
		uniform float4 _Metallic_ST;
		uniform float _MetallicStrength;
		uniform sampler2D _Roughness;
		uniform float4 _Roughness_ST;
		uniform float _RoughnessStrength;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			v.vertex.xyz += 0;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normals = i.uv_texcoord * _Normals_ST.xy + _Normals_ST.zw;
			o.Normal = ( tex2D( _Normals, uv_Normals ) * _NormalStrength ).rgb;
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			o.Albedo = ( tex2D( _Albedo, uv_Albedo ) * _Tint ).rgb;
			float2 uv_Emission = i.uv_texcoord * _Emission_ST.xy + _Emission_ST.zw;
			o.Emission = ( tex2D( _Emission, uv_Emission ) * _EmmisionStrength ).rgb;
			float2 uv_Metallic = i.uv_texcoord * _Metallic_ST.xy + _Metallic_ST.zw;
			o.Metallic = ( tex2D( _Metallic, uv_Metallic ) * _MetallicStrength ).r;
			float2 uv_Roughness = i.uv_texcoord * _Roughness_ST.xy + _Roughness_ST.zw;
			o.Smoothness = ( tex2D( _Roughness, uv_Roughness ) * _RoughnessStrength ).r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16800
1676;73;1341;649;1397.151;60.85199;1.211092;True;False
Node;AmplifyShaderEditor.RangedFloatNode;39;-1032.283,392.1579;Float;False;Property;_EdgeSpeed;Edge Speed;12;0;Create;True;0;0;False;0;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;40;-862.2857,398.1579;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-908.5953,293.8162;Float;False;Property;_EdgeWidth;Edge Width;14;0;Create;True;0;0;False;0;1;100;0;1000;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;41;-697.2863,400.1579;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;42;-578.2862,397.1579;Float;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;48;-630.6719,278.8565;Float;False;Property;_Toggle;Toggle;15;0;Create;True;0;0;False;0;0;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;25;-1588.765,-1194.922;Float;False;3511.502;501.9753;Standard Surface;6;1;2;3;4;5;6;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;4;-966.0521,-1121.415;Float;False;541.7106;360.1877;Normals;3;22;17;12;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;44;-402.7122,494.7377;Float;False;Constant;_Float1;Float 1;2;0;Create;True;0;0;False;0;0.0005;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;5;-1538.765,-1144.922;Float;False;529.9496;451.9731;Albedo;3;24;16;11;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;2;232.6771,-1068.396;Float;False;519.3003;354.7999;Roughness;3;23;18;15;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;-405.2858,328.1579;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;3;-355.8456,-1122.698;Float;False;554.4001;357.3993;Metallic;3;21;14;13;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-368.7118,418.7378;Float;False;Constant;_Float2;Float 2;2;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;1;770.2312,-1071.761;Float;False;535.7966;360.7854;Emmision;3;20;19;10;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;19;885.0279,-825.9761;Float;False;Property;_EmmisionStrength;Emmision Strength;11;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;47;-228.7113,349.7379;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;16;-1488.765,-1094.922;Float;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;False;0;None;956c8abea3473c14e8c1b723548e987e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;14;-305.8456,-1072.698;Float;True;Property;_Metallic;Metallic;4;0;Create;True;0;0;False;0;None;c2d7c3fe9cac2504c80b150301808ac7;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;15;341.177,-828.5961;Float;False;Property;_RoughnessStrength;Roughness Strength;7;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;12;-916.0522,-1071.416;Float;True;Property;_Normals;Normals;2;0;Create;True;0;0;False;0;None;20bd5e9c22adb8445900364d84b994af;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;18;282.6772,-1018.396;Float;True;Property;_Roughness;Roughness;6;0;Create;True;0;0;False;0;None;0312635f67dda5845b2e5bf572432c7a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;10;820.2313,-1021.761;Float;True;Property;_Emission;Emission;8;0;Create;True;0;0;False;0;None;c7efa54cd6456994ea68f7c20b336679;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;37;-260.4532,152.3096;Float;False;Property;_EdgeColour;Edge Colour;13;1;[HDR];Create;True;0;0;False;0;1,0,0,0;0.07450981,1,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;6;1324.136,-1080.905;Float;False;548.6011;355.4004;Opacity;3;9;8;7;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;11;-1405.534,-899.9472;Float;False;Property;_Tint;Tint;1;0;Create;True;0;0;False;0;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;17;-811.9518,-876.2292;Float;False;Property;_NormalStrength;Normal Strength;3;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-170.6452,-880.2989;Float;False;Property;_MetallicStrength;Metallic Strength;5;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;1703.738,-896.5058;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-593.3405,-937.3878;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;29.55506,-947.8987;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;582.9775,-888.3954;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OutlineNode;38;-18.45258,300.3095;Float;False;1;True;None;0;0;Front;3;0;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;9;1374.136,-1030.906;Float;True;Property;_OpacityMap;Opacity Map;9;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;7;1514.937,-840.5048;Float;False;Property;_Opacity;Opacity;10;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-1177.815,-948.0938;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;1137.028,-907.976;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;433,-300;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;TechArt/Effects/PickupPulse2;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;40;0;39;0
WireConnection;41;0;40;0
WireConnection;42;0;41;0
WireConnection;48;0;43;0
WireConnection;46;0;48;0
WireConnection;46;1;42;0
WireConnection;47;0;46;0
WireConnection;47;3;45;0
WireConnection;47;4;44;0
WireConnection;8;0;9;0
WireConnection;8;1;7;0
WireConnection;22;0;12;0
WireConnection;22;1;17;0
WireConnection;21;0;14;0
WireConnection;21;1;13;0
WireConnection;23;0;18;0
WireConnection;23;1;15;0
WireConnection;38;0;37;0
WireConnection;38;1;47;0
WireConnection;24;0;16;0
WireConnection;24;1;11;0
WireConnection;20;0;10;0
WireConnection;20;1;19;0
WireConnection;0;0;24;0
WireConnection;0;1;22;0
WireConnection;0;2;20;0
WireConnection;0;3;21;0
WireConnection;0;4;23;0
WireConnection;0;11;38;0
ASEEND*/
//CHKSM=FBE0E01F181329C7B5D91E27167F473E52069463