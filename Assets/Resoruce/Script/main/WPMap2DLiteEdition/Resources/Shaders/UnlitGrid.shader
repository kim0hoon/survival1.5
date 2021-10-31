Shader "World Political Map/Unlit Grid" {
 
Properties {
    _Color ("Color", Color) = (1,1,1)
}
 
SubShader {
    Color [_Color]
        Tags {
        "Queue"="Transparent"
        "RenderType"="Transparent"
    }
    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha
    Offset -1, -1
    Pass {
    }
}
 
}
