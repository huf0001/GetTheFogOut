// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TechArt/Buildings/Buildings"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_Albedo("Albedo", 2D) = "white" {}
		[HDR]_Tint("Tint", Color) = (1,1,1,0)
		_Normals("Normals", 2D) = "white" {}
		_NormalStrength("Normal Strength", Float) = 1
		_Metallic("Metallic", 2D) = "white" {}
		_MetallicStrength("Metallic Strength", Float) = 0
		_Roughness("Roughness", 2D) = "white" {}
		_RoughnessStrength("Roughness Strength", Float) = 0
		_Emission("Emission", 2D) = "white" {}
		_EmmisionStrength("Emmision Strength", Float) = 0
		[HDR]_GlowColour("Glow Colour", Color) = (4,0.879581,0,0)
		_Teleport("Teleport", Range( -20 , 20)) = -1
		[Toggle]_Reverse("Reverse", Float) = 0
		_Tiling("Tiling", Vector) = (5,5,0,0)
		_Speed("Speed", Float) = 1
		_VertOffsetStrength("Vert Offset Strength", Range( 0 , 1)) = 0
		_NoiseDirection("Noise Direction", Vector) = (0,-1,0,0)
		_EdgeWidth("Edge Width", Range( 0 , 5)) = 3
		_PulseSpeed("Pulse Speed", Float) = 20
		[HDR]_OutlineColour("Outline Colour", Color) = (2,1.686275,0,0)
		[Toggle]_Overclock("Overclock", Float) = 0
		[HDR]_UpgradeColour("Upgrade Colour", Color) = (0,1,0,0)
		_FresnelPower("Fresnel Power", Float) = 0.63
		[Toggle]_Upgraded("Upgraded", Float) = 0
		[Toggle]_Damaged("Damaged", Float) = 0
		_DanageTimescale("Danage Timescale", Float) = 5
		[HDR]_DamageColour("Damage Colour", Color) = (1,0,0,0)
		_PartyTime("Party Time", Float) = 2
		_PartyPower("Party Power", Range( 0 , 10)) = 0.5
		[Toggle]_Party("Party", Float) = 0
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
		void outlineVertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float mulTime111 = _Time.y * _PulseSpeed;
			float outlineVar = (0.0 + (( lerp(0.0,_EdgeWidth,_Overclock) * (0.0 + (sin( mulTime111 ) - -1.0) * (1.0 - 0.0) / (1.0 - -1.0)) ) - 0.0) * (0.0005 - 0.0) / (1.0 - 0.0));
			v.vertex.xyz += ( v.normal * outlineVar );
		}
		inline half4 LightingOutline( SurfaceOutput s, half3 lightDir, half atten ) { return half4 ( 0,0,0, s.Alpha); }
		void outlineSurf( Input i, inout SurfaceOutput o )
		{
			o.Emission = _OutlineColour.rgb;
		}
		ENDCG
		

		Tags{ "RenderType" = "Opaque"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Off
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform float _Teleport;
		uniform float _Reverse;
		uniform float _VertOffsetStrength;
		uniform float2 _Tiling;
		uniform float _Speed;
		uniform float2 _NoiseDirection;
		uniform sampler2D _Normals;
		uniform float4 _Normals_ST;
		uniform float _NormalStrength;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float4 _Tint;
		uniform float4 _GlowColour;
		uniform sampler2D _Emission;
		uniform float4 _Emission_ST;
		uniform float _EmmisionStrength;
		uniform float _Upgraded;
		uniform float4 _UpgradeColour;
		uniform float _FresnelPower;
		uniform float _Damaged;
		uniform float4 _DamageColour;
		uniform float _DanageTimescale;
		uniform float _Party;
		uniform float _PartyTime;
		uniform float _PartyPower;
		uniform sampler2D _Metallic;
		uniform float4 _Metallic_ST;
		uniform float _MetallicStrength;
		uniform sampler2D _Roughness;
		uniform float4 _Roughness_ST;
		uniform float _RoughnessStrength;
		uniform float _Cutoff = 0.5;
		uniform float _Overclock;
		uniform float _EdgeWidth;
		uniform float _PulseSpeed;
		uniform float4 _OutlineColour;


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


		struct Gradient
		{
			int type;
			int colorsLength;
			int alphasLength;
			float4 colors[8];
			float2 alphas[8];
		};


		Gradient NewGradient(int type, int colorsLength, int alphasLength, 
		float4 colors0, float4 colors1, float4 colors2, float4 colors3, float4 colors4, float4 colors5, float4 colors6, float4 colors7,
		float2 alphas0, float2 alphas1, float2 alphas2, float2 alphas3, float2 alphas4, float2 alphas5, float2 alphas6, float2 alphas7)
		{
			Gradient g;
			g.type = type;
			g.colorsLength = colorsLength;
			g.alphasLength = alphasLength;
			g.colors[ 0 ] = colors0;
			g.colors[ 1 ] = colors1;
			g.colors[ 2 ] = colors2;
			g.colors[ 3 ] = colors3;
			g.colors[ 4 ] = colors4;
			g.colors[ 5 ] = colors5;
			g.colors[ 6 ] = colors6;
			g.colors[ 7 ] = colors7;
			g.alphas[ 0 ] = alphas0;
			g.alphas[ 1 ] = alphas1;
			g.alphas[ 2 ] = alphas2;
			g.alphas[ 3 ] = alphas3;
			g.alphas[ 4 ] = alphas4;
			g.alphas[ 5 ] = alphas5;
			g.alphas[ 6 ] = alphas6;
			g.alphas[ 7 ] = alphas7;
			return g;
		}


		float4 SampleGradient( Gradient gradient, float time )
		{
			float3 color = gradient.colors[0].rgb;
			UNITY_UNROLL
			for (int c = 1; c < 8; c++)
			{
			float colorPos = saturate((time - gradient.colors[c-1].w) / (gradient.colors[c].w - gradient.colors[c-1].w)) * step(c, (float)gradient.colorsLength-1);
			color = lerp(color, gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), gradient.type));
			}
			#ifndef UNITY_COLORSPACE_GAMMA
			color = half3(GammaToLinearSpaceExact(color.r), GammaToLinearSpaceExact(color.g), GammaToLinearSpaceExact(color.b));
			#endif
			float alpha = gradient.alphas[0].x;
			UNITY_UNROLL
			for (int a = 1; a < 8; a++)
			{
			float alphaPos = saturate((time - gradient.alphas[a-1].y) / (gradient.alphas[a].y - gradient.alphas[a-1].y)) * step(a, (float)gradient.alphasLength-1);
			alpha = lerp(alpha, gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), gradient.type));
			}
			return float4(color, alpha);
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 Outline122 = 0;
			float3 ase_vertex3Pos = v.vertex.xyz;
			float4 transform72 = mul(unity_ObjectToWorld,float4( ase_vertex3Pos , 0.0 ));
			float YGradient70 = saturate( ( ( transform72.y + _Teleport ) / lerp(5.0,-5.0,_Reverse) ) );
			float mulTime56 = _Time.y * _Speed;
			float2 panner55 = ( mulTime56 * _NoiseDirection + float2( 0,0 ));
			float2 uv_TexCoord46 = v.texcoord.xy * _Tiling + panner55;
			float simplePerlin2D47 = snoise( uv_TexCoord46 );
			float Noise61 = ( simplePerlin2D47 + 1.0 );
			float3 VertOffset101 = ( ( ( ase_vertex3Pos * YGradient70 ) * _VertOffsetStrength ) * Noise61 );
			v.vertex.xyz += ( Outline122 + VertOffset101 );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normals = i.uv_texcoord * _Normals_ST.xy + _Normals_ST.zw;
			o.Normal = ( tex2D( _Normals, uv_Normals ) * _NormalStrength ).rgb;
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			o.Albedo = ( tex2D( _Albedo, uv_Albedo ) * _Tint ).rgb;
			float mulTime56 = _Time.y * _Speed;
			float2 panner55 = ( mulTime56 * _NoiseDirection + float2( 0,0 ));
			float2 uv_TexCoord46 = i.uv_texcoord * _Tiling + panner55;
			float simplePerlin2D47 = snoise( uv_TexCoord46 );
			float Noise61 = ( simplePerlin2D47 + 1.0 );
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float4 transform72 = mul(unity_ObjectToWorld,float4( ase_vertex3Pos , 0.0 ));
			float YGradient70 = saturate( ( ( transform72.y + _Teleport ) / lerp(5.0,-5.0,_Reverse) ) );
			float4 Emission91 = ( _GlowColour * ( Noise61 * YGradient70 ) );
			float2 uv_Emission = i.uv_texcoord * _Emission_ST.xy + _Emission_ST.zw;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float fresnelNdotV133 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode133 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV133, _FresnelPower ) );
			float4 Upgrade139 = lerp(float4( 0,0,0,0 ),( _UpgradeColour * fresnelNode133 ),_Upgraded);
			float mulTime151 = _Time.y * _DanageTimescale;
			float fresnelNdotV145 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode145 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV145, (10.0 + (sin( mulTime151 ) - -1.0) * (0.0 - 10.0) / (1.0 - -1.0)) ) );
			float4 Damaged141 = lerp(float4( 0,0,0,0 ),( _DamageColour * fresnelNode145 ),_Damaged);
			Gradient gradient161 = NewGradient( 0, 6, 2, float4( 1, 0.08846778, 0, 0 ), float4( 1, 0.6136441, 0, 0.1660029 ), float4( 0.9293728, 1, 0, 0.4979934 ), float4( 0.1565347, 1, 0, 0.6639963 ), float4( 0, 0.5080171, 1, 0.8299992 ), float4( 1, 0, 0.9203696, 1 ), 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
			float mulTime157 = _Time.y * _PartyTime;
			float fresnelNdotV163 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode163 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV163, _PartyPower ) );
			float4 Party166 = lerp(float4( 0,0,0,0 ),( SampleGradient( gradient161, (0.0 + (sin( mulTime157 ) - -1.0) * (1.0 - 0.0) / (1.0 - -1.0)) ) * fresnelNode163 ),_Party);
			o.Emission = ( Emission91 + ( tex2D( _Emission, uv_Emission ) * _EmmisionStrength ) + Upgrade139 + Damaged141 + Party166 ).rgb;
			float2 uv_Metallic = i.uv_texcoord * _Metallic_ST.xy + _Metallic_ST.zw;
			o.Metallic = ( tex2D( _Metallic, uv_Metallic ) * _MetallicStrength ).r;
			float2 uv_Roughness = i.uv_texcoord * _Roughness_ST.xy + _Roughness_ST.zw;
			o.Smoothness = ( tex2D( _Roughness, uv_Roughness ) * _RoughnessStrength ).r;
			o.Alpha = 1;
			float temp_output_83_0 = ( YGradient70 * 1.0 );
			float OpacityMask81 = ( ( ( ( 1.0 - YGradient70 ) * Noise61 ) - temp_output_83_0 ) + ( 1.0 - temp_output_83_0 ) );
			clip( OpacityMask81 - _Cutoff );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows vertex:vertexDataFunc 

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
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
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
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
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
0;1;3826;1011;1339.724;99.43372;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;76;-3268.99,46.91545;Float;False;1246.294;472.2013;Comment;10;108;74;71;72;67;68;73;75;70;109;Y Gradient;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;64;-3556.591,-399.8689;Float;False;1534.618;388.9829;Noise;10;46;56;57;54;55;58;47;59;60;61;Noise;1,1,1,1;0;0
Node;AmplifyShaderEditor.PosVertexDataNode;71;-3216.99,96.91553;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;57;-3506.591,-161.369;Float;False;Property;_Speed;Speed;15;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;108;-2967.252,415.8927;Float;False;Constant;_PositiveNumber;Positive Number;17;0;Create;True;0;0;False;0;5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;68;-3115.593,263.317;Float;False;Property;_Teleport;Teleport;12;0;Create;True;0;0;False;0;-1;-20;-20;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.ObjectToWorldTransfNode;72;-3025.892,96.91581;Float;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;74;-2975.598,343.0169;Float;False;Constant;_NegativeNumber;Negative Number;17;0;Create;True;0;0;False;0;-5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;58;-3387.627,-291.889;Float;False;Property;_NoiseDirection;Noise Direction;17;0;Create;True;0;0;False;0;0,-1;0,-1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;56;-3359.689,-162.669;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;168;-3266.159,3581.95;Float;False;1865.824;561;Comment;11;157;159;160;161;162;158;163;164;165;167;166;Party Time;1,1,1,1;0;0
Node;AmplifyShaderEditor.ToggleSwitchNode;109;-2767.813,386.1032;Float;False;Property;_Reverse;Reverse;13;0;Create;True;0;0;False;0;0;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;127;-3209.091,1870.536;Float;False;1508.071;471.9238;Comment;13;122;120;118;119;117;116;115;113;114;112;111;110;128;Outline;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;67;-2797.091,134.616;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;55;-3193.296,-208.1691;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,-1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;155;-3323.99,2957.08;Float;False;1869.883;514.2207;Comment;9;151;152;145;146;147;142;141;149;153;Damage;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;54;-3172.489,-338.169;Float;False;Property;_Tiling;Tiling;14;0;Create;True;0;0;False;0;5,5;5,5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;46;-2968.378,-339.63;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;110;-3159.091,2150.879;Float;False;Property;_PulseSpeed;Pulse Speed;19;0;Create;True;0;0;False;0;20;20;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;73;-2573.491,180.1167;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;153;-3273.99,3263.837;Float;False;Property;_DanageTimescale;Danage Timescale;26;0;Create;True;0;0;False;0;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;158;-3216.159,3708.772;Float;False;Property;_PartyTime;Party Time;28;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;47;-2733.09,-349.8689;Float;True;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;111;-2980.093,2154.879;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;157;-3050.272,3711.646;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;151;-3052.742,3269.3;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;60;-2673.17,-125.8863;Float;False;Constant;_Booster;Booster;15;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;75;-2438.292,185.3167;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;70;-2265.696,180.1165;Float;False;YGradient;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;159;-2871.567,3710.95;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;140;-2968.903,2403.983;Float;False;1149.498;512.3003;Comment;6;133;135;134;137;139;136;Upgrade;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;59;-2495.07,-309.1859;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;107;-3065.582,1426.236;Float;False;1048.128;388.5007;Comment;8;98;99;100;103;105;101;106;104;Vert Offset;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;114;-3092.219,1983.459;Float;False;Property;_EdgeWidth;Edge Width;18;0;Create;True;0;0;False;0;3;3;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;87;-3176.411,549.1596;Float;False;1157.092;348.4108;Comment;10;78;80;77;83;82;79;84;85;86;81;Opacity Mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.SinOpNode;112;-2815.094,2156.879;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;152;-2873.83,3267.934;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;164;-2839.567,3976.95;Float;False;Property;_PartyPower;Party Power;29;0;Create;True;0;0;False;0;0.5;0;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.GradientNode;161;-2745.567,3631.95;Float;False;0;6;2;1,0.08846778,0,0;1,0.6136441,0,0.1660029;0.9293728,1,0,0.4979934;0.1565347,1,0,0.6639963;0,0.5080171,1,0.8299992;1,0,0.9203696,1;1,0;1,1;0;1;OBJECT;0
Node;AmplifyShaderEditor.TFHCRemapNode;160;-2722.567,3711.95;Float;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;128;-2787.846,1997.588;Float;False;Property;_Overclock;Overclock;21;0;Create;True;0;0;False;0;0;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;96;-2920.465,946.4398;Float;False;903.301;425.0621;Comment;6;88;89;90;91;95;94;Emission;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;99;-3006.973,1624.794;Float;False;70;YGradient;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;113;-2696.093,2153.879;Float;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;136;-2918.903,2737.383;Float;False;Property;_FresnelPower;Fresnel Power;23;0;Create;True;0;0;False;0;0.63;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;77;-3126.411,599.1597;Float;False;70;YGradient;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;149;-2746.816,3269.301;Float;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;10;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;61;-2264.974,-305.2858;Float;False;Noise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;98;-3015.582,1476.236;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;147;-2494.155,3007.08;Float;False;Property;_DamageColour;Damage Colour;27;1;[HDR];Create;True;0;0;False;0;1,0,0,0;4,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GradientSampleNode;162;-2487.567,3680.95;Float;True;2;0;OBJECT;0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FresnelNode;163;-2517.567,3889.95;Float;True;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;89;-2870.465,1256.504;Float;False;70;YGradient;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;82;-2894.693,745.5705;Float;False;70;YGradient;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;116;-2486.518,2175.459;Float;False;Constant;_Float1;Float 1;2;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;88;-2865.119,1171.023;Float;False;61;Noise;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;78;-2915.892,604.8303;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;133;-2742.103,2663.283;Float;True;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;104;-2877.083,1699.737;Float;False;Property;_VertOffsetStrength;Vert Offset Strength;16;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;117;-2523.092,2084.879;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;80;-2916.418,675.6437;Float;False;61;Noise;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;115;-2520.518,2251.459;Float;False;Constant;_Float0;Float 0;2;0;Create;True;0;0;False;0;0.0005;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;135;-2651.103,2453.983;Float;False;Property;_UpgradeColour;Upgrade Colour;22;1;[HDR];Create;True;0;0;False;0;0,1,0,0;1.06425,0,2,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FresnelNode;145;-2524.2,3176.431;Float;False;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;100;-2794.454,1567.767;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;165;-2119.566,3758.95;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;103;-2601.363,1578.178;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;-2710.219,645.6431;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;119;-2346.517,2106.459;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;118;-2372.507,1920.536;Float;False;Property;_OutlineColour;Outline Colour;20;1;[HDR];Create;True;0;0;False;0;2,1.686275,0,0;2,1.686275,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;90;-2645.465,1196.503;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;146;-2231.932,3105.412;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;134;-2430.103,2616.483;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;-2708.693,748.5705;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;106;-2589.559,1687.97;Float;False;61;Noise;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;94;-2687.845,996.4406;Float;False;Property;_GlowColour;Glow Colour;11;1;[HDR];Create;True;0;0;False;0;4,0.879581,0,0;4,0.5654449,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;167;-1888.169,3728.873;Float;False;Property;_Party;Party;30;0;Create;True;0;0;False;0;0;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ToggleSwitchNode;137;-2268.903,2606.083;Float;False;Property;_Upgraded;Upgraded;24;0;Create;True;0;0;False;0;0;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;85;-2560.693,787.5705;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;95;-2462.818,1085.515;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;25;-456.5293,513.7897;Float;False;669.7966;388.7854;Emmision;3;24;23;17;;0.09477007,1,0,1;0;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;84;-2563.693,684.5704;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OutlineNode;120;-2136.257,2057.031;Float;False;0;True;None;0;0;Front;3;0;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;105;-2411.559,1579.97;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ToggleSwitchNode;142;-1903.334,3073.758;Float;False;Property;_Damaged;Damaged;25;0;Create;True;0;0;False;0;0;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;86;-2400.693,705.5704;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;166;-1643.335,3729.498;Float;False;Party;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;4;-956.6689,-1060.596;Float;False;668.8;487.2802;Albedo;3;2;3;1;;0.09477007,1,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;8;-211.4022,-1020.927;Float;False;541.7106;360.1877;Normals;3;7;5;6;;0.09477007,1,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;12;376.7612,-1029.104;Float;False;554.4001;357.3993;Metallic;3;11;9;10;;0.09477007,1,0,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;139;-2062.405,2611.51;Float;False;Upgrade;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;91;-2260.165,1188.492;Float;False;Emission;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;17;-406.5295,563.7897;Float;True;Property;_Emission;Emission;9;0;Create;True;0;0;False;0;None;669c44c6c2548b547acf5c714d40269e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;16;965.8558,-1015.472;Float;False;519.3003;354.7999;Roughness;3;15;13;14;;0.09477007,1,0,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-341.7328,759.5751;Float;False;Property;_EmmisionStrength;Emmision Strength;10;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;122;-1917.02,2054.89;Float;False;Outline;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;101;-2260.454,1574.767;Float;False;VertOffset;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;141;-1697.108,3105.17;Float;False;Damaged;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;154;-65.60034,221.7648;Float;False;141;Damaged;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;138;-62.80298,145.1329;Float;False;139;Upgrade;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;93;-58.43878,68.8231;Float;False;91;Emission;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;124;222.0054,320.3933;Float;False;122;Outline;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-89.73312,677.5751;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;13;1015.856,-965.4718;Float;True;Property;_Roughness;Roughness;7;0;Create;True;0;0;False;0;None;ec7380790c516074fa910e839a3d6774;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;6;-57.30219,-775.74;Float;False;Property;_NormalStrength;Normal Strength;4;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;81;-2262.319,702.5428;Float;False;OpacityMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;1074.355,-775.6719;Float;False;Property;_RoughnessStrength;Roughness Strength;8;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;102;211.2172,393.0399;Float;False;101;VertOffset;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;5;-161.4023,-970.9274;Float;True;Property;_Normals;Normals;3;0;Create;True;0;0;False;0;None;29c3497a14aa1304fb7f57fe60b59e85;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;9;426.7611,-979.1043;Float;True;Property;_Metallic;Metallic;5;0;Create;True;0;0;False;0;None;364891edc9fb3ed49b5c2dccc7a4fa0e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-906.6689,-1010.596;Float;True;Property;_Albedo;Albedo;1;0;Create;True;0;0;False;0;None;42b9c5c3ce329e445a8d76c21c239fc4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;169;-56.72351,297.5663;Float;False;166;Party;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;10;561.9612,-786.7051;Float;False;Property;_MetallicStrength;Metallic Strength;6;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;3;-843.1031,-778.4174;Float;False;Property;_Tint;Tint;2;1;[HDR];Create;True;0;0;False;0;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;-523.2825,-945.5884;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;161.3086,-836.8986;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;97;299.8869,202.7004;Float;False;81;OpacityMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;762.1618,-854.3048;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;1316.156,-835.471;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;92;174.9767,152.1353;Float;False;5;5;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;126;404.8075,340.7068;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;508,-81;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;TechArt/Buildings/Buildings;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Opaque;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;72;0;71;0
WireConnection;56;0;57;0
WireConnection;109;0;108;0
WireConnection;109;1;74;0
WireConnection;67;0;72;2
WireConnection;67;1;68;0
WireConnection;55;2;58;0
WireConnection;55;1;56;0
WireConnection;46;0;54;0
WireConnection;46;1;55;0
WireConnection;73;0;67;0
WireConnection;73;1;109;0
WireConnection;47;0;46;0
WireConnection;111;0;110;0
WireConnection;157;0;158;0
WireConnection;151;0;153;0
WireConnection;75;0;73;0
WireConnection;70;0;75;0
WireConnection;159;0;157;0
WireConnection;59;0;47;0
WireConnection;59;1;60;0
WireConnection;112;0;111;0
WireConnection;152;0;151;0
WireConnection;160;0;159;0
WireConnection;128;1;114;0
WireConnection;113;0;112;0
WireConnection;149;0;152;0
WireConnection;61;0;59;0
WireConnection;162;0;161;0
WireConnection;162;1;160;0
WireConnection;163;3;164;0
WireConnection;78;0;77;0
WireConnection;133;3;136;0
WireConnection;117;0;128;0
WireConnection;117;1;113;0
WireConnection;145;3;149;0
WireConnection;100;0;98;0
WireConnection;100;1;99;0
WireConnection;165;0;162;0
WireConnection;165;1;163;0
WireConnection;103;0;100;0
WireConnection;103;1;104;0
WireConnection;79;0;78;0
WireConnection;79;1;80;0
WireConnection;119;0;117;0
WireConnection;119;3;116;0
WireConnection;119;4;115;0
WireConnection;90;0;88;0
WireConnection;90;1;89;0
WireConnection;146;0;147;0
WireConnection;146;1;145;0
WireConnection;134;0;135;0
WireConnection;134;1;133;0
WireConnection;83;0;82;0
WireConnection;167;1;165;0
WireConnection;137;1;134;0
WireConnection;85;0;83;0
WireConnection;95;0;94;0
WireConnection;95;1;90;0
WireConnection;84;0;79;0
WireConnection;84;1;83;0
WireConnection;120;0;118;0
WireConnection;120;1;119;0
WireConnection;105;0;103;0
WireConnection;105;1;106;0
WireConnection;142;1;146;0
WireConnection;86;0;84;0
WireConnection;86;1;85;0
WireConnection;166;0;167;0
WireConnection;139;0;137;0
WireConnection;91;0;95;0
WireConnection;122;0;120;0
WireConnection;101;0;105;0
WireConnection;141;0;142;0
WireConnection;24;0;17;0
WireConnection;24;1;23;0
WireConnection;81;0;86;0
WireConnection;2;0;1;0
WireConnection;2;1;3;0
WireConnection;7;0;5;0
WireConnection;7;1;6;0
WireConnection;11;0;9;0
WireConnection;11;1;10;0
WireConnection;15;0;13;0
WireConnection;15;1;14;0
WireConnection;92;0;93;0
WireConnection;92;1;24;0
WireConnection;92;2;138;0
WireConnection;92;3;154;0
WireConnection;92;4;169;0
WireConnection;126;0;124;0
WireConnection;126;1;102;0
WireConnection;0;0;2;0
WireConnection;0;1;7;0
WireConnection;0;2;92;0
WireConnection;0;3;11;0
WireConnection;0;4;15;0
WireConnection;0;10;97;0
WireConnection;0;11;126;0
ASEEND*/
//CHKSM=0FA6A9F533B9F3164290E3210611874BA13768F7