// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TechArt/Effects/PickupPulse2"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
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
		[Toggle]_Toggle("Toggle", Float) = 1
		_Lerp("Lerp", Range( 0 , 1)) = 0
		_NoiseScale("Noise Scale", Float) = 0
		[HDR]_NoiseEmission("Noise Emission", Color) = (0,16,15.56078,0)
		_WorldMulty("World Multy", Range( 0 , 1)) = 0
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
		

		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
		};

		uniform float _NoiseScale;
		uniform float _Lerp;
		uniform float _WorldMulty;
		uniform sampler2D _Normals;
		uniform float4 _Normals_ST;
		uniform float _NormalStrength;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float4 _Tint;
		uniform sampler2D _Emission;
		uniform float4 _Emission_ST;
		uniform float _EmmisionStrength;
		uniform float4 _NoiseEmission;
		uniform sampler2D _Metallic;
		uniform float4 _Metallic_ST;
		uniform float _MetallicStrength;
		uniform sampler2D _Roughness;
		uniform float4 _Roughness_ST;
		uniform float _RoughnessStrength;
		uniform float _Cutoff = 0.5;


		//https://www.shadertoy.com/view/XdXGW8
		float2 GradientNoiseDir( float2 x )
		{
			const float2 k = float2( 0.3183099, 0.3678794 );
			x = x * k + k.yx;
			return -1.0 + 2.0 * frac( 16.0 * k * frac( x.x * x.y * ( x.x + x.y ) ) );
		}
		
		float GradientNoise( float2 UV, float Scale )
		{
			float2 p = UV * Scale;
			float2 i = floor( p );
			float2 f = frac( p );
			float2 u = f * f * ( 3.0 - 2.0 * f );
			return lerp( lerp( dot( GradientNoiseDir( i + float2( 0.0, 0.0 ) ), f - float2( 0.0, 0.0 ) ),
					dot( GradientNoiseDir( i + float2( 1.0, 0.0 ) ), f - float2( 1.0, 0.0 ) ), u.x ),
					lerp( dot( GradientNoiseDir( i + float2( 0.0, 1.0 ) ), f - float2( 0.0, 1.0 ) ),
					dot( GradientNoiseDir( i + float2( 1.0, 1.0 ) ), f - float2( 1.0, 1.0 ) ), u.x ), u.y );
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 Outline49 = 0;
			float3 ase_vertex3Pos = v.vertex.xyz;
			float gradientNoise56 = GradientNoise(v.texcoord.xy,_NoiseScale);
			gradientNoise56 = gradientNoise56*0.5 + 0.5;
			float Lerp54 = _Lerp;
			float OpacityMask63 = ( gradientNoise56 + (-1.0 + (( 1.0 - Lerp54 ) - 0.0) * (1.0 - -1.0) / (1.0 - 0.0)) );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float4 appendResult94 = (float4(ase_worldPos.y , ase_worldPos.z , 0.0 , 0.0));
			float4 VertOffset83 = ( ( float4( ( ase_vertex3Pos * ( ( 1.0 - OpacityMask63 ) + 0.6 ) ) , 0.0 ) * ( appendResult94 * _WorldMulty ) ) * (0.0 + (Lerp54 - 0.0) * (10.0 - 0.0) / (1.0 - 0.0)) );
			v.vertex.xyz += ( float4( Outline49 , 0.0 ) + VertOffset83 ).xyz;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normals = i.uv_texcoord * _Normals_ST.xy + _Normals_ST.zw;
			o.Normal = ( tex2D( _Normals, uv_Normals ) * _NormalStrength ).rgb;
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			o.Albedo = ( tex2D( _Albedo, uv_Albedo ) * _Tint ).rgb;
			float2 uv_Emission = i.uv_texcoord * _Emission_ST.xy + _Emission_ST.zw;
			float gradientNoise56 = GradientNoise(i.uv_texcoord,_NoiseScale);
			gradientNoise56 = gradientNoise56*0.5 + 0.5;
			float Lerp54 = _Lerp;
			float OpacityMask63 = ( gradientNoise56 + (-1.0 + (( 1.0 - Lerp54 ) - 0.0) * (1.0 - -1.0) / (1.0 - 0.0)) );
			float4 EmissionEgde72 = ( ( 1.0 - OpacityMask63 ) * _NoiseEmission );
			o.Emission = ( ( tex2D( _Emission, uv_Emission ) * _EmmisionStrength ) + EmissionEgde72 ).rgb;
			float2 uv_Metallic = i.uv_texcoord * _Metallic_ST.xy + _Metallic_ST.zw;
			o.Metallic = ( tex2D( _Metallic, uv_Metallic ) * _MetallicStrength ).r;
			float2 uv_Roughness = i.uv_texcoord * _Roughness_ST.xy + _Roughness_ST.zw;
			o.Smoothness = ( tex2D( _Roughness, uv_Roughness ) * _RoughnessStrength ).r;
			o.Alpha = 1;
			clip( OpacityMask63 - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17000
384;81;1140;544;3014.519;-1865.959;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;55;-2407.98,309.4389;Float;False;572.1822;170.4575;Comment;2;53;54;Lerp;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;53;-2357.98,364.8963;Float;False;Property;_Lerp;Lerp;17;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;65;-2866.344,638.6394;Float;False;1132.451;561.8245;Comment;8;60;58;57;61;62;56;59;63;Opacity Mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;54;-2078.798,359.4388;Float;False;Lerp;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;60;-2816.344,996.5068;Float;False;54;Lerp;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;58;-2670.222,858.2394;Float;False;Property;_NoiseScale;Noise Scale;18;0;Create;True;0;0;False;0;0;25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;57;-2732.622,688.6394;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;61;-2617.45,1002.154;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;56;-2471.822,763.8394;Float;True;Gradient;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;62;-2431.446,998.4639;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;52;-3081.421,-415.8382;Float;False;1587.928;507.428;Comment;13;39;40;43;41;42;48;44;46;45;47;37;38;49;Outline;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;59;-2212.621,903.0394;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;39;-3031.421,-125.9899;Float;False;Property;_EdgeSpeed;Edge Speed;13;0;Create;True;0;0;False;0;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;63;-1976.893,897.7734;Float;False;OpacityMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;78;-3013.23,2009.201;Float;False;63;OpacityMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;40;-2861.424,-119.9899;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;81;-2640.337,2114.268;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;87;-2800.017,2092.648;Float;False;Constant;_Adds;Adds;20;0;Create;True;0;0;False;0;0.6;0.6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-2907.734,-224.3316;Float;False;Property;_EdgeWidth;Edge Width;15;0;Create;True;0;0;False;0;1;150;0;1000;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;41;-2696.425,-117.9899;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;79;-2806.112,2010.844;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;93;-2760.519,2269.959;Float;False;Property;_WorldMulty;World Multy;20;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;86;-2637.565,1997.74;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;94;-2448.519,2111.959;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;74;-2662.48,1321.391;Float;False;895.2076;337.6197;Comment;5;66;69;67;68;72;Noise Emission;1,1,1,1;0;0
Node;AmplifyShaderEditor.TFHCRemapNode;42;-2577.425,-120.9899;Float;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;48;-2629.81,-239.2913;Float;False;Property;_Toggle;Toggle;16;0;Create;True;0;0;False;0;1;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;75;-2784.928,1850.593;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;45;-2367.85,-99.41002;Float;False;Constant;_Float2;Float 2;2;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;44;-2401.851,-23.41012;Float;False;Constant;_Float1;Float 1;2;0;Create;True;0;0;False;0;0.0005;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;76;-2506.698,1890.702;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;90;-2530.219,2498.597;Float;False;54;Lerp;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;66;-2612.48,1371.391;Float;False;63;OpacityMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;92;-2277.643,2140.066;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;-2404.424,-189.9899;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;91;-2271.017,2427.427;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;37;-2259.592,-365.8382;Float;False;Property;_EdgeColour;Edge Colour;14;1;[HDR];Create;True;0;0;False;0;1,0,0,0;0.07450981,1,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;80;-2301.222,1900.565;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ColorNode;69;-2397.928,1452.011;Float;False;Property;_NoiseEmission;Noise Emission;19;1;[HDR];Create;True;0;0;False;0;0,16,15.56078,0;0,4,3.895288,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;47;-2227.85,-168.4099;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;25;-1443.769,-1270.716;Float;False;3511.502;501.9753;Standard Surface;6;1;2;3;4;5;6;;1,1,1,1;0;0
Node;AmplifyShaderEditor.OneMinusNode;67;-2367.928,1372.011;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;1;915.2271,-1147.554;Float;False;535.7966;360.7854;Emmision;3;20;19;10;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;89;-2077.946,1910.424;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.OutlineNode;38;-2017.591,-217.8383;Float;False;1;True;None;0;0;Front;3;0;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;-2176.928,1377.011;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;10;965.2274,-1097.554;Float;True;Property;_Emission;Emission;9;0;Create;True;0;0;False;0;None;c7efa54cd6456994ea68f7c20b336679;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;49;-1736.494,-203.3817;Float;False;Outline;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;72;-2010.273,1386.203;Float;False;EmissionEgde;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;4;-821.056,-1197.208;Float;False;541.7106;360.1877;Normals;3;22;17;12;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;5;-1393.769,-1220.716;Float;False;529.9496;451.9731;Albedo;3;24;16;11;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;2;377.6733,-1144.189;Float;False;519.3003;354.7999;Roughness;3;23;18;15;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;3;-210.8494,-1198.491;Float;False;554.4001;357.3993;Metallic;3;21;14;13;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;83;-1838.945,1890.288;Float;False;VertOffset;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;19;1030.024,-901.7695;Float;False;Property;_EmmisionStrength;Emmision Strength;12;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;16;-1343.769,-1170.715;Float;True;Property;_Albedo;Albedo;1;0;Create;True;0;0;False;0;None;956c8abea3473c14e8c1b723548e987e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;12;-771.0562,-1147.209;Float;True;Property;_Normals;Normals;3;0;Create;True;0;0;False;0;None;20bd5e9c22adb8445900364d84b994af;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;6;1469.132,-1156.698;Float;False;548.6011;355.4004;Opacity;3;9;8;7;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;73;-371.942,-184.9234;Float;False;72;EmissionEgde;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;14;-160.8494,-1148.491;Float;True;Property;_Metallic;Metallic;5;0;Create;True;0;0;False;0;None;c2d7c3fe9cac2504c80b150301808ac7;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;85;-46.40808,74.92752;Float;False;83;VertOffset;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;50;18.11601,-36.84397;Float;False;49;Outline;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-25.64908,-956.0923;Float;False;Property;_MetallicStrength;Metallic Strength;6;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;18;427.6734,-1094.189;Float;True;Property;_Roughness;Roughness;7;0;Create;True;0;0;False;0;None;0312635f67dda5845b2e5bf572432c7a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;1282.024,-983.7695;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;11;-1260.538,-975.7407;Float;False;Property;_Tint;Tint;2;0;Create;True;0;0;False;0;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;15;486.1731,-904.3896;Float;False;Property;_RoughnessStrength;Roughness Strength;8;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-666.9557,-952.0227;Float;False;Property;_NormalStrength;Normal Strength;4;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;1848.734,-972.2993;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;71;-139.9424,-212.1234;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;174.5512,-1023.692;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;9;1519.132,-1106.699;Float;True;Property;_OpacityMap;Opacity Map;10;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-1032.819,-1023.887;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-448.3444,-1013.181;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;7;1659.933,-916.2983;Float;False;Property;_Opacity;Opacity;11;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;64;184.1608,-114.2491;Float;False;63;OpacityMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;727.9736,-964.189;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;51;227.4838,-24.05822;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;433,-300;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;TechArt/Effects/PickupPulse2;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;54;0;53;0
WireConnection;61;0;60;0
WireConnection;56;0;57;0
WireConnection;56;1;58;0
WireConnection;62;0;61;0
WireConnection;59;0;56;0
WireConnection;59;1;62;0
WireConnection;63;0;59;0
WireConnection;40;0;39;0
WireConnection;41;0;40;0
WireConnection;79;0;78;0
WireConnection;86;0;79;0
WireConnection;86;1;87;0
WireConnection;94;0;81;2
WireConnection;94;1;81;3
WireConnection;42;0;41;0
WireConnection;48;0;43;0
WireConnection;76;0;75;0
WireConnection;76;1;86;0
WireConnection;92;0;94;0
WireConnection;92;1;93;0
WireConnection;46;0;48;0
WireConnection;46;1;42;0
WireConnection;91;0;90;0
WireConnection;80;0;76;0
WireConnection;80;1;92;0
WireConnection;47;0;46;0
WireConnection;47;3;45;0
WireConnection;47;4;44;0
WireConnection;67;0;66;0
WireConnection;89;0;80;0
WireConnection;89;1;91;0
WireConnection;38;0;37;0
WireConnection;38;1;47;0
WireConnection;68;0;67;0
WireConnection;68;1;69;0
WireConnection;49;0;38;0
WireConnection;72;0;68;0
WireConnection;83;0;89;0
WireConnection;20;0;10;0
WireConnection;20;1;19;0
WireConnection;8;0;9;0
WireConnection;8;1;7;0
WireConnection;71;0;20;0
WireConnection;71;1;73;0
WireConnection;21;0;14;0
WireConnection;21;1;13;0
WireConnection;24;0;16;0
WireConnection;24;1;11;0
WireConnection;22;0;12;0
WireConnection;22;1;17;0
WireConnection;23;0;18;0
WireConnection;23;1;15;0
WireConnection;51;0;50;0
WireConnection;51;1;85;0
WireConnection;0;0;24;0
WireConnection;0;1;22;0
WireConnection;0;2;71;0
WireConnection;0;3;21;0
WireConnection;0;4;23;0
WireConnection;0;10;64;0
WireConnection;0;11;51;0
ASEEND*/
//CHKSM=6935CF35C3738205EE4E061E1F5C83A7DB964CF5