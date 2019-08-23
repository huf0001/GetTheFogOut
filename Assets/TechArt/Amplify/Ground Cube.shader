// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TechArt/GroundCube"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_Tint("Tint", Color) = (1,1,1,0)
		_AO("AO", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_Metallic("Metallic", 2D) = "white" {}
		_Roughness("Roughness", 2D) = "white" {}
		_Shiny("Shiny", Range( 0 , 1)) = 0.7
		_Tile("Tile", Float) = 1
		_World("World", Float) = 1
		_Test1("Test1", Float) = 0
		_Test2("Test2", Float) = 0
		_Test3("Test3", Float) = 1
		_Test4("Test4", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform sampler2D _Normal;
		uniform float _World;
		uniform float _Tile;
		uniform float4 _Tint;
		uniform sampler2D _Albedo;
		uniform float _Test1;
		uniform float _Test2;
		uniform float _Test3;
		uniform float _Test4;
		uniform sampler2D _Metallic;
		uniform sampler2D _Roughness;
		uniform float _Shiny;
		uniform sampler2D _AO;


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


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TexCoord14 = i.uv_texcoord * float2( 0.1,0.1 ) + float2( -5,-5 );
			float3 ase_worldPos = i.worldPos;
			float4 appendResult25 = (float4(ase_worldPos.x , ase_worldPos.z , 0.0 , 0.0));
			float4 WORLDUV34 = ( float4( uv_TexCoord14, 0.0 , 0.0 ) * ( ( appendResult25 * _World ) * _Tile ) );
			o.Normal = UnpackNormal( tex2D( _Normal, WORLDUV34.xy ) );
			Gradient gradient45 = NewGradient( 0, 2, 2, float4( 0, 0, 0, 0 ), float4( 0.01524997, 1, 0, 1 ), 0, 0, 0, 0, 0, 0, float2( 0, 0 ), float2( 1, 0.6441138 ), 0, 0, 0, 0, 0, 0 );
			o.Albedo = ( ( _Tint * tex2D( _Albedo, WORLDUV34.xy ) ) + ( SampleGradient( gradient45, ( ase_worldPos.x + _Test1 ) ) + SampleGradient( gradient45, ( ase_worldPos.z + _Test2 ) ) + SampleGradient( gradient45, ( 1.0 - ( ase_worldPos.x + _Test3 ) ) ) + SampleGradient( gradient45, ( 1.0 - ( ase_worldPos.z + _Test4 ) ) ) ) ).rgb;
			o.Metallic = tex2D( _Metallic, WORLDUV34.xy ).r;
			o.Smoothness = ( tex2D( _Roughness, WORLDUV34.xy ) * _Shiny ).r;
			o.Occlusion = tex2D( _AO, WORLDUV34.xy ).r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16900
379;73;1150;554;1859.261;17.25931;3.003156;True;False
Node;AmplifyShaderEditor.CommentaryNode;33;-1255.478,48.48308;Float;False;928.0014;499.5453;Comment;8;24;25;32;26;30;28;14;31;World Space UV's;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;24;-1205.478,222.0284;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;25;-984.4784,227.0284;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;32;-1008.478,423.0284;Float;False;Property;_World;World;9;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-808.4781,282.0284;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-812.4781,433.0284;Float;False;Property;_Tile;Tile;8;0;Create;True;0;0;False;0;1;0.01;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;-578.4769,341.0284;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;14;-748.4462,98.48308;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;0.1,0.1;False;1;FLOAT2;-5,-5;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;128;-725.2394,1375.039;Float;False;Property;_Test4;Test4;13;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;125;-773.3394,1196.939;Float;False;Property;_Test3;Test3;12;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-496.4766,167.0284;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WorldPosInputsNode;107;-726.5388,865.4384;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;34;-210.952,165.3074;Float;False;WORLDUV;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;119;-453.5395,938.2385;Float;False;Property;_Test1;Test1;10;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;120;-579.6395,1070.838;Float;False;Property;_Test2;Test2;11;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;127;-538.0395,1368.539;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;124;-582.2398,1181.339;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;35;422.57,190.426;Float;False;34;WORLDUV;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;109;-411.9389,832.9383;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;118;-408.0395,1042.238;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;123;-413.2394,1183.939;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GradientNode;45;-494.6515,747.9699;Float;False;0;2;2;0,0,0,0;0.01524997,1,0,1;0,0;1,0.6441138;0;1;OBJECT;0
Node;AmplifyShaderEditor.OneMinusNode;126;-414.5395,1363.339;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GradientSampleNode;121;-219.5394,1125.439;Float;True;2;0;OBJECT;0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GradientSampleNode;129;-220.8394,1313.939;Float;True;2;0;OBJECT;0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GradientSampleNode;46;-219.0996,751.5057;Float;True;2;0;OBJECT;0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GradientSampleNode;111;-226.0391,936.9382;Float;True;2;0;OBJECT;0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;42;874.6908,-334.9365;Float;False;Property;_Tint;Tint;1;0;Create;True;0;0;False;0;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;7;790.7092,-165.6735;Float;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;False;0;None;434ae09004e9a1148a2d43d91538f808;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;113;290.0611,878.4382;Float;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;1122.691,-149.9365;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;39;801.4119,571.9543;Float;True;Property;_Roughness;Roughness;6;0;Create;True;0;0;False;0;None;ef74598915b53424f9477c65e47a4266;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;41;819.4731,761.6644;Float;False;Property;_Shiny;Shiny;7;0;Create;True;0;0;False;0;0.7;0.7;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;1115.199,651.1383;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;130;1249.005,-2.243558;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;37;791.9794,17.95902;Float;True;Property;_Normal;Normal;4;0;Create;True;0;0;False;0;None;9f8bc9eb44e7bce449d4fc3f742413dd;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;38;794.1963,200.8486;Float;True;Property;_Metallic;Metallic;5;0;Create;True;0;0;False;0;None;23c933ad690063e498a3217ae1b5e82c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;36;796.5217,388.4293;Float;True;Property;_AO;AO;3;0;Create;True;0;0;False;0;None;144e6ec18932ef347a637d4cae9bb43a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1343.731,117.5834;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;TechArt/GroundCube;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;2;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;25;0;24;1
WireConnection;25;1;24;3
WireConnection;26;0;25;0
WireConnection;26;1;32;0
WireConnection;28;0;26;0
WireConnection;28;1;30;0
WireConnection;31;0;14;0
WireConnection;31;1;28;0
WireConnection;34;0;31;0
WireConnection;127;0;107;3
WireConnection;127;1;128;0
WireConnection;124;0;107;1
WireConnection;124;1;125;0
WireConnection;109;0;107;1
WireConnection;109;1;119;0
WireConnection;118;0;107;3
WireConnection;118;1;120;0
WireConnection;123;0;124;0
WireConnection;126;0;127;0
WireConnection;121;0;45;0
WireConnection;121;1;123;0
WireConnection;129;0;45;0
WireConnection;129;1;126;0
WireConnection;46;0;45;0
WireConnection;46;1;109;0
WireConnection;111;0;45;0
WireConnection;111;1;118;0
WireConnection;7;1;35;0
WireConnection;113;0;46;0
WireConnection;113;1;111;0
WireConnection;113;2;121;0
WireConnection;113;3;129;0
WireConnection;43;0;42;0
WireConnection;43;1;7;0
WireConnection;39;1;35;0
WireConnection;40;0;39;0
WireConnection;40;1;41;0
WireConnection;130;0;43;0
WireConnection;130;1;113;0
WireConnection;37;1;35;0
WireConnection;38;1;35;0
WireConnection;36;1;35;0
WireConnection;0;0;130;0
WireConnection;0;1;37;0
WireConnection;0;3;38;0
WireConnection;0;4;40;0
WireConnection;0;5;36;0
ASEEND*/
//CHKSM=3DA30A412E02025BF49B312DF4937AEE0B5B896F