// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TechArt/Abilities/Shield"
{
	Properties
	{
		[HDR]_EdgeColour("Edge Colour", Color) = (1,0.6078432,0,0)
		[HDR]_EdgeColour2("Edge Colour2", Color) = (1,0,0,0)
		_LERP("LERP", Range( 0 , 1)) = 0
		_HexTex("HexTex", 2D) = "white" {}
		_HexTiling("Hex Tiling", Vector) = (3,3,0,0)
		_FresnelPower("Fresnel Power", Range( 1 , 7)) = 3
		_ScrollSpeed("Scroll Speed", Float) = 0.05
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha noshadow 
		struct Input
		{
			float4 screenPos;
			float3 worldPos;
			float3 worldNormal;
			float2 uv_texcoord;
		};

		uniform float4 _EdgeColour;
		uniform float4 _EdgeColour2;
		uniform float _LERP;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _FresnelPower;
		uniform sampler2D _HexTex;
		uniform float2 _HexTiling;
		uniform float _ScrollSpeed;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float LERP22 = _LERP;
			float4 lerpResult19 = lerp( _EdgeColour , _EdgeColour2 , LERP22);
			o.Emission = lerpResult19.rgb;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth2 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,UNITY_PROJ_COORD( ase_screenPos )));
			float distanceDepth2 = abs( ( screenDepth2 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( 0.5 ) );
			float clampResult3 = clamp( distanceDepth2 , 0.0 , 1.0 );
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV9 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode9 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV9, _FresnelPower ) );
			float mulTime17 = _Time.y * _ScrollSpeed;
			float2 temp_cast_1 = (mulTime17).xx;
			float2 uv_TexCoord14 = i.uv_texcoord * _HexTiling + temp_cast_1;
			o.Alpha = ( ( ( ( 1.0 - clampResult3 ) * 1.0 ) + fresnelNode9 ) * tex2D( _HexTex, uv_TexCoord14 ) ).r;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16800
204;81;1409;642;2994.507;775.6874;2.844653;True;True
Node;AmplifyShaderEditor.RangedFloatNode;33;-989.4553,-32.61368;Float;False;Constant;_Float0;Float 0;7;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;2;-840.4231,-51.7645;Float;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;3;-598.5215,-40.06452;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-1058.233,557.1236;Float;False;Property;_ScrollSpeed;Scroll Speed;6;0;Create;True;0;0;False;0;0.05;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;4;-450.4956,-24.49629;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;17;-893.2329,561.1236;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-1363.473,-246.9192;Float;False;Property;_LERP;LERP;2;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;15;-931.2329,391.1236;Float;False;Property;_HexTiling;Hex Tiling;4;0;Create;True;0;0;False;0;3,3;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;10;-762.5447,379.7043;Float;False;Property;_FresnelPower;Fresnel Power;5;0;Create;True;0;0;False;0;3;0;1;7;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;9;-484.1157,180.7849;Float;True;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;14;-727.9429,487.6466;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-308.2092,77.34947;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;22;-1066.719,-247.3181;Float;False;LERP;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;7;-453.6908,429.7705;Float;True;Property;_HexTex;HexTex;3;0;Create;True;0;0;False;0;decdb5d510cb58844aa1e65da66a1d38;decdb5d510cb58844aa1e65da66a1d38;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;8;-151.866,77.71764;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;23;-76.32357,-114.5171;Float;False;22;LERP;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;5;-453.9697,-197.0222;Float;False;Property;_EdgeColour;Edge Colour;0;1;[HDR];Create;True;0;0;False;0;1,0.6078432,0,0;0.4196078,0,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;20;-418.8735,-363.519;Float;False;Property;_EdgeColour2;Edge Colour2;1;1;[HDR];Create;True;0;0;False;0;1,0,0,0;1,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-11.64087,73.44778;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;19;125.1265,-191.519;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;359,-129;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;TechArt/Abilities/Shield;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;True;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;2;0;33;0
WireConnection;3;0;2;0
WireConnection;4;0;3;0
WireConnection;17;0;18;0
WireConnection;9;3;10;0
WireConnection;14;0;15;0
WireConnection;14;1;17;0
WireConnection;6;0;4;0
WireConnection;22;0;21;0
WireConnection;7;1;14;0
WireConnection;8;0;6;0
WireConnection;8;1;9;0
WireConnection;12;0;8;0
WireConnection;12;1;7;0
WireConnection;19;0;5;0
WireConnection;19;1;20;0
WireConnection;19;2;23;0
WireConnection;0;2;19;0
WireConnection;0;9;12;0
ASEEND*/
//CHKSM=C6F73E06824AA612DC365B22A70DABB70476B29B