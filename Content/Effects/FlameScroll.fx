sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float uDissolveRate;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;
float2 tex2Scale;
float timer;
float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float y = frac(coords.y + timer);
    float topwidth = ((1 - coords.y) * (1 - coords.y) * (1 - coords.y)) / 3.0f;
    float sway = (1 - coords.y) * sin(timer * 5) * 0.06f;
    float4 color = tex2D(uImage0, float2(coords.x, y));
    float4 color1 = tex2D(uImage1, coords / tex2Scale);
    float alpha = 0.0f;
    
    if (color.r < color1.r && (coords.x >= topwidth + sway && coords.x <= (1.0f - topwidth + sway)))
    {
        alpha = 1.0f;
    }
    if (coords.y < 0.05f) 
        return float4(0.0f, 0.0f, 0.0f, 0.0f);
    return float4(1.0f, 0.9f - (1.0f - color1.g) * 0.7f, 0.5f - (1.0f - color1.b) * 0.4f, alpha);
}
technique Technique1
{
    pass FlameScrollPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}