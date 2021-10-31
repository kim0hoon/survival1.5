Shader "World Political Map/Unlit Single Texture"{

Properties { _MainTex ("Texture", 2D) = "" }
SubShader {
	    Tags {
        "Queue"="Geometry"
        "RenderType"="Opaque"
    }
    Offset 20, 20
	Pass {
		SetTexture[_MainTex]
		} 
	}
}