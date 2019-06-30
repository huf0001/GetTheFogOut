// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TechArt/Effects/NormalRefraction"
{
	Properties
	{
		_NormalMap("Normal Map", 2D) = "white" {}
		_NormalScale("Normal Scale", Range( 0 , 1)) = 0.1
		[Header(Refraction)]
		_ChromaticAberration("Chromatic Aberration", Range( 0 , 0.3)) = 0.1
		_CubeMap("Cube Map", CUBE) = "white" {}
		_Opacity("Opacity", Range( 0 , 1)) = 0
		_IndexofRefraction("Index of Refraction", Range( -3 , 4)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Back
		GrabPass{ }
		CGPROGRAM
		#pragma target 3.0
		#pragma multi_compile _ALPHAPREMULTIPLY_ON
		#pragma surface surf Standard alpha:fade keepalpha finalcolor:RefractionF noshadow exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldRefl;
			INTERNAL_DATA
			float4 screenPos;
			float3 worldPos;
		};

		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform float _NormalScale;
		uniform samplerCUBE _CubeMap;
		uniform float _Opacity;
		uniform sampler2D _GrabTexture;
		uniform float _ChromaticAberration;
		uniform float _IndexofRefraction;

		inline float4 Refraction( Input i, SurfaceOutputStandard o, float indexOfRefraction, float chomaticAberration ) {
			float3 worldNormal = o.Normal;
			float4 screenPos = i.screenPos;
			#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
			#else
				float scale = 1.0;
			#endif
			float halfPosW = screenPos.w * 0.5;
			screenPos.y = ( screenPos.y - halfPosW ) * _ProjectionParams.x * scale + halfPosW;
			#if SHADER_API_D3D9 || SHADER_API_D3D11
				screenPos.w += 0.00000000001;
			#endif
			float2 projScreenPos = ( screenPos / screenPos.w ).xy;
			float3 worldViewDir = normalize( UnityWorldSpaceViewDir( i.worldPos ) );
			float3 refractionOffset = ( ( ( ( indexOfRefraction - 1.0 ) * mul( UNITY_MATRIX_V, float4( worldNormal, 0.0 ) ) ) * ( 1.0 / ( screenPos.z + 1.0 ) ) ) * ( 1.0 - dot( worldNormal, worldViewDir ) ) );
			float2 cameraRefraction = float2( refractionOffset.x, -( refractionOffset.y * _ProjectionParams.x ) );
			float4 redAlpha = tex2D( _GrabTexture, ( projScreenPos + cameraRefraction ) );
			float green = tex2D( _GrabTexture, ( projScreenPos + ( cameraRefraction * ( 1.0 - chomaticAberration ) ) ) ).g;
			float blue = tex2D( _GrabTexture, ( projScreenPos + ( cameraRefraction * ( 1.0 + chomaticAberration ) ) ) ).b;
			return float4( redAlpha.r, green, blue, redAlpha.a );
		}

		void RefractionF( Input i, SurfaceOutputStandard o, inout half4 color )
		{
			#ifdef UNITY_PASS_FORWARDBASE
			color.rgb = color.rgb + Refraction( i, o, _IndexofRefraction, _ChromaticAberration ) * ( 1 - color.a );
			color.a = 1;
			#endif
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Normal = float3(0,0,1);
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			float4 tex2DNode54 = tex2D( _NormalMap, uv_NormalMap );
			o.Normal = ( tex2DNode54 * _NormalScale ).rgb;
			o.Albedo = ( texCUBE( _CubeMap, WorldReflectionVector( i , tex2DNode54.rgb ) ) * 5 ).rgb;
			o.Alpha = _Opacity;
			o.Normal = o.Normal + 0.00001 * i.screenPos * i.worldPos;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16800
1421;73;1609;653;1074.323;361.2438;1.505066;True;True
Node;AmplifyShaderEditor.SamplerNode;54;-646.7961,72.9934;Float;True;Property;_NormalMap;Normal Map;0;0;Create;True;0;0;False;0;a599dcdec4400194ca793909c2ea2216;a599dcdec4400194ca793909c2ea2216;True;0;True;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldReflectionVector;46;-268.58,-82.80111;Float;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;55;-623.6902,281.84;Float;False;Property;_NormalScale;Normal Scale;1;0;Create;True;0;0;False;0;0.1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;45;-23.07985,-140.8011;Float;True;Property;_CubeMap;Cube Map;4;0;Create;True;0;0;False;0;ef7513b54a0670140b9b967af7620563;ef7513b54a0670140b9b967af7620563;True;0;False;white;LockedToCube;False;Object;-1;Auto;Cube;6;0;SAMPLER2D;;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;52;304.7198,-183.0011;Float;False;293.1999;207.6;Multiplier to make it POP;1;53;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WireNode;57;-281.99,176.72;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-194.68,199.6993;Float;False;Property;_IndexofRefraction;Index of Refraction;6;0;Create;True;0;0;False;0;1;0;-3;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;42;-200.1801,296.399;Float;False;Property;_Opacity;Opacity;5;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleNode;53;379.1033,-99.10673;Float;False;5;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;56;-75.98999,76.71997;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;696.3,49.80001;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;TechArt/Effects/NormalRefraction;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;3;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;1;False;-1;1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;2;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;46;0;54;0
WireConnection;45;1;46;0
WireConnection;57;0;55;0
WireConnection;53;0;45;0
WireConnection;56;0;54;0
WireConnection;56;1;57;0
WireConnection;0;0;53;0
WireConnection;0;1;56;0
WireConnection;0;8;41;0
WireConnection;0;9;42;0
ASEEND*/
//CHKSM=F5DD31C8E690CD376E6E421F870F5F17126DAF11