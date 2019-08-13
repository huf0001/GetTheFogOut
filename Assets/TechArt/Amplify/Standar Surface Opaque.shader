// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TechArt/Standard Surface Opaque"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_Tint("Tint", Color) = (1,0.4063355,0,0)
		_Tint2("Tint 2", Color) = (0.7830189,0.6805335,0.06278926,0)
		_Normal("Normal", 2D) = "white" {}
		_NormalStrength("Normal Strength", Range( 0 , 1)) = 0
		_Metallic("Metallic", 2D) = "white" {}
		_MetallicStrength("Metallic Strength", Range( 0 , 1)) = 0
		_Smoothness("Smoothness", 2D) = "white" {}
		_SmoothnessStrength("Smoothness Strength", Range( 0 , 1)) = 0
		_Emission("Emission", 2D) = "white" {}
		_EmissionStrength("Emission Strength", Float) = 0
		[HDR]_EmissionTint("Emission Tint", Color) = (1,1,1,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform float _NormalStrength;
		uniform float4 _Tint;
		uniform float4 _Tint2;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform sampler2D _Emission;
		uniform float4 _Emission_ST;
		uniform float4 _EmissionTint;
		uniform float _EmissionStrength;
		uniform sampler2D _Metallic;
		uniform float4 _Metallic_ST;
		uniform float _MetallicStrength;
		uniform sampler2D _Smoothness;
		uniform float4 _Smoothness_ST;
		uniform float _SmoothnessStrength;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = ( UnpackNormal( tex2D( _Normal, uv_Normal ) ) * _NormalStrength );
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 tex2DNode27 = tex2D( _Albedo, uv_Albedo );
			float4 lerpResult29 = lerp( _Tint , _Tint2 , tex2DNode27);
			o.Albedo = ( lerpResult29 * tex2DNode27 ).rgb;
			float2 uv_Emission = i.uv_texcoord * _Emission_ST.xy + _Emission_ST.zw;
			o.Emission = ( tex2D( _Emission, uv_Emission ) * _EmissionTint * _EmissionStrength ).rgb;
			float2 uv_Metallic = i.uv_texcoord * _Metallic_ST.xy + _Metallic_ST.zw;
			o.Metallic = ( tex2D( _Metallic, uv_Metallic ) * _MetallicStrength ).r;
			float2 uv_Smoothness = i.uv_texcoord * _Smoothness_ST.xy + _Smoothness_ST.zw;
			o.Smoothness = ( tex2D( _Smoothness, uv_Smoothness ) * _SmoothnessStrength ).r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16900
1804;73;1132;656;2630.483;1521.403;3.540228;True;False
Node;AmplifyShaderEditor.CommentaryNode;25;-1549.093,-1159.742;Float;False;646;629.2994;Comment;5;29;28;27;26;30;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;28;-1447.093,-1111.742;Float;False;Property;_Tint;Tint;1;0;Create;True;0;0;False;0;1,0.4063355,0,0;0,1,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;26;-1465.769,-942.048;Float;False;Property;_Tint2;Tint 2;2;0;Create;True;0;0;False;0;0.7830189,0.6805335,0.06278926,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;17;-247,-1067;Float;False;539.866;360.41;Comment;3;3;16;15;Metallic;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;27;-1512.093,-771.7441;Float;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;False;0;None;04c97974a2dbc5e47beacee39afe9106;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;20;305,-1036;Float;False;523.866;355.41;Comment;3;4;19;18;Smoothness;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;14;-823,-1104;Float;False;538.866;357.41;Comment;3;2;12;13;Normals;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;21;893.0235,-1084.849;Float;False;597;518;Comment;4;5;9;10;8;Emission;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;2;-778,-1061;Float;True;Property;_Normal;Normal;3;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;9;1030.023,-845.8493;Float;False;Property;_EmissionTint;Emission Tint;11;1;[HDR];Create;True;0;0;False;0;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;19;375.866,-795.59;Float;False;Property;_SmoothnessStrength;Smoothness Strength;8;0;Create;True;0;0;False;0;0;0.408;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;4;355,-986;Float;True;Property;_Smoothness;Smoothness;7;0;Create;True;0;0;False;0;None;04c97974a2dbc5e47beacee39afe9106;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;13;-753.134,-861.59;Float;False;Property;_NormalStrength;Normal Strength;4;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;3;-197,-1017;Float;True;Property;_Metallic;Metallic;5;0;Create;True;0;0;False;0;None;04c97974a2dbc5e47beacee39afe9106;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;10;1020.023,-681.8492;Float;False;Property;_EmissionStrength;Emission Strength;10;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;5;943.0235,-1034.849;Float;True;Property;_Emission;Emission;9;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;29;-1145.769,-1084.046;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-170.134,-821.59;Float;False;Property;_MetallicStrength;Metallic Strength;6;0;Create;True;0;0;False;0;0;0.35;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-453.134,-880.59;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;1321.023,-996.8493;Float;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;123.866,-912.59;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;659.866,-895.59;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-1079.835,-839.3706;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;156,-126;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;TechArt/Standard Surface Opaque;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;29;0;28;0
WireConnection;29;1;26;0
WireConnection;29;2;27;0
WireConnection;12;0;2;0
WireConnection;12;1;13;0
WireConnection;8;0;5;0
WireConnection;8;1;9;0
WireConnection;8;2;10;0
WireConnection;15;0;3;0
WireConnection;15;1;16;0
WireConnection;18;0;4;0
WireConnection;18;1;19;0
WireConnection;30;0;29;0
WireConnection;30;1;27;0
WireConnection;0;0;30;0
WireConnection;0;1;12;0
WireConnection;0;2;8;0
WireConnection;0;3;15;0
WireConnection;0;4;18;0
ASEEND*/
//CHKSM=F7C11D46CF0EEA0EB0EB6C01EB89E6650FC26598