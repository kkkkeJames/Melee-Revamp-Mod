sampler2D uImage0 : register(s0);
float2 uSource;

float4 main(float2 fragCoord : TEXCOORD0) : COLOR0
{
    float2 coord = fragCoord * exp2(-4.0);

    float3 color = tex2D(uImage0, coord).rgb;
    
    return float4(color, 1.0);
}

technique Technique1
{
    pass MagnifyPass
    {
        PixelShader = compile ps_3_0 main();
    }
}