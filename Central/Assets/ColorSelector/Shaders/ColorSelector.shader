// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "CodeArtist.mx/ColorSelector" {
Properties {
    _Color ("Selected Color", Color) = (1,1,1,0.5)
 
   _MainTex ("Texture", 2D) = "white" { }
   
      
}
SubShader {
	Tags {"Queue" = "Transparent" }
    Pass {
    Blend SrcAlpha OneMinusSrcAlpha     
	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag	
	#pragma target 3.0
	#include "UnityCG.cginc"	
	float4 _Color;
	sampler2D _MainTex;
	
	struct v2f {
	    float4  pos : SV_POSITION;
	    float2  uv : TEXCOORD0;
	};	
	float4 _MainTex_ST;	
	
	v2f vert (appdata_base v){
	    v2f o;
	    o.pos = UnityObjectToClipPos (v.vertex);
	    o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
	    return o;
	}	
	
	half3 hsv2rgb(half3 c){
	    half4 K = half4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	    half3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
	    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
	}
	half3 rgb2hsv(half3 c){
	    half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	    half4 p = lerp(half4(c.bg, K.wz), half4(c.gb, K.xy), step(c.b, c.g));
	    half4 q = lerp(half4(p.xyw, c.r), half4(c.r, p.yzx), step(p.x, c.r));
	    float d = q.x - min(q.w, q.y);
	    float e = 1.0e-10;
	    return half3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
	}
	half4 DrawHSVCircle(half2 pos){
		half3 paintColor=_Color;
		float alpha=0.0f;
		half2 center= half2(0.5,0.5);//vec2(resolution.x/2.0,resolution.y/2.0);		
		float dist=distance(pos,center);
		float innerRadius=0.4,outerRadius=0.5;		
		half2 delta=half2(pos.x-center.x,pos.y-center.y);
		float angle= ((atan2(delta.x, delta.y) )*0.16);		
				
		//float angle= ((atan2(delta.x, delta.y) ));		
		if(dist<outerRadius && dist>innerRadius){
			half3 hsvValues = half3(angle,1.0,1.0);
			paintColor=  hsv2rgb(hsvValues);
			alpha=1.0f;
		}				
		return half4 (paintColor,alpha);		
	}
	
	//half3 SubstractCircle(half2 pos,half3 originalColor){
		//half3 paintColor=originalColor;
		//half2 center= half2(0.5,0.5);
		//float dist=distance(pos,center);
		//float radius=0.4;
		//if(dist<radius){
			//paintColor=_BgColor;
		//}
		//return paintColor;
	//}
	half4 DrawTriangle(half3 triangleColor,half4 originalColor,half2 pos){
		half2 verts[3];
		verts[0].x = 0.5;
		verts[0].y = 0.9;	
		verts[1].x = 0.85;
		verts[1].y = 0.3;	
		verts[2].x = 0.15;
		verts[2].y = 0.3;	
		half2 uv = pos;//gl_FragCoord.xy / resolution.xy;
		half4 col = originalColor;
		half3 bcoord;	
		bcoord.x = ((verts[1].y - verts[2].y)*(uv.x - verts[2].x) + (verts[2].x - verts[1].x)*(uv.y - verts[2].y)) /
			   ((verts[1].y - verts[2].y)*(verts[0].x - verts[2].x) + (verts[2].x - verts[1].x)*(verts[0].y - verts[2].y)) ;
		
		bcoord.y = ((verts[2].y - verts[0].y)*(uv.x - verts[2].x) + (verts[0].x - verts[2].x)*(uv.y - verts[2].y)) /
			   ((verts[1].y - verts[2].y)*(verts[0].x - verts[2].x) + (verts[2].x - verts[1].x)*(verts[0].y - verts[2].y)) ;	
		bcoord.z = 1.0 - bcoord.x - bcoord.y;	
		if( bcoord.x > 0.0 && bcoord.x < 1.0 && bcoord.y > 0.0 && bcoord.y < 1.0 &&  bcoord.z > 0.0 && bcoord.z < 1.0 )	{
			col.xyz = bcoord.x * triangleColor + bcoord.y * half3(0.0,0.0,0.0) + bcoord.z *half3(1.0,1.0,1.0); 								
			col.a=1.0f;
		}
		return col;
	}
	half4 frag (v2f i) : COLOR{		
		half4 texcol;			   
		half2 position =half2(i.uv.x,i.uv.y); 
		half4 circleColor=DrawHSVCircle(position);			
		circleColor=DrawTriangle(_Color,circleColor,i.uv);
		
		texcol = circleColor;

	    return texcol;
	}
	ENDCG

    }
}
Fallback "VertexLit"
} 