// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TechArt/GroundCube"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_Tile("Tile", Float) = 1
		_World("World", Float) = 1
		_AO("AO", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_Metallic("Metallic", 2D) = "white" {}
		_Roughness("Roughness", 2D) = "white" {}
		_Shiny("Shiny", Range( 0 , 1)) = 0.7
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
		uniform sampler2D _TextureSample0;
		uniform sampler2D _Metallic;
		uniform sampler2D _Roughness;
		uniform float _Shiny;
		uniform sampler2D _AO;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TexCoord14 = i.uv_texcoord * float2( 0.1,0.1 ) + float2( -5,-5 );
			float3 ase_worldPos = i.worldPos;
			float4 appendResult25 = (float4(ase_worldPos.x , ase_worldPos.z , 0.0 , 0.0));
			float4 WORLDUV34 = ( float4( uv_TexCoord14, 0.0 , 0.0 ) * ( ( appendResult25 * _World ) * _Tile ) );
			o.Normal = UnpackNormal( tex2D( _Normal, WORLDUV34.xy ) );
			o.Albedo = tex2D( _TextureSample0, WORLDUV34.xy ).rgb;
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
Version=16800
404;73;1737;648;31.60742;-244.2215;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;33;-1255.478,48.48308;Float;False;928.0014;499.5453;Comment;8;24;25;32;26;30;28;14;31;World Space UV's;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;24;-1205.478,222.0284;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;25;-984.4784,227.0284;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;32;-1008.478,423.0284;Float;False;Property;_World;World;3;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-808.4781,282.0284;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-812.4781,433.0284;Float;False;Property;_Tile;Tile;2;0;Create;True;0;0;False;0;1;0.01;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;-578.4769,341.0284;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;14;-748.4462,98.48308;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;0.1,0.1;False;1;FLOAT2;-5,-5;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-496.4766,167.0284;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;34;-210.952,165.3074;Float;False;WORLDUV;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;35;422.57,190.426;Float;False;34;WORLDUV;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;39;801.4119,571.9543;Float;True;Property;_Roughness;Roughness;7;0;Create;True;0;0;False;0;None;ef74598915b53424f9477c65e47a4266;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;41;819.4731,761.6644;Float;False;Property;_Shiny;Shiny;8;0;Create;True;0;0;False;0;0.7;0.7;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;7;790.7092,-165.6735;Float;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;False;0;None;434ae09004e9a1148a2d43d91538f808;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;37;791.9794,17.95902;Float;True;Property;_Normal;Normal;5;0;Create;True;0;0;False;0;None;9f8bc9eb44e7bce449d4fc3f742413dd;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;38;794.1963,200.8486;Float;True;Property;_Metallic;Metallic;6;0;Create;True;0;0;False;0;None;23c933ad690063e498a3217ae1b5e82c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;36;796.5217,388.4293;Float;True;Property;_AO;AO;4;0;Create;True;0;0;False;0;None;144e6ec18932ef347a637d4cae9bb43a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;1115.199,651.1383;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1343.731,117.5834;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;TechArt/GroundCube;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;25;0;24;1
WireConnection;25;1;24;3
WireConnection;26;0;25;0
WireConnection;26;1;32;0
WireConnection;28;0;26;0
WireConnection;28;1;30;0
WireConnection;31;0;14;0
WireConnection;31;1;28;0
WireConnection;34;0;31;0
WireConnection;39;1;35;0
WireConnection;7;1;35;0
WireConnection;37;1;35;0
WireConnection;38;1;35;0
WireConnection;36;1;35;0
WireConnection;40;0;39;0
WireConnection;40;1;41;0
WireConnection;0;0;7;0
WireConnection;0;1;37;0
WireConnection;0;3;38;0
WireConnection;0;4;40;0
WireConnection;0;5;36;0
ASEEND*/
//CHKSM=4B08F456A3DEDEBC770751E2D772AEF39EF6724A