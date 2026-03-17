// Based on Buffer-pass Bloom implementation by robobo1221: https://www.shadertoy.com/view/lsBfRc
// Adapted for the RT2D postprocessing pipeline by myself.
// Focused on Gaussian Blur & HDR Buffer extraction. The multipass part is in RT2D rendering.
// The Gaussian weights are pre-calculated based on a standard normal distribution to handle the Bloom blurkernel for the purpose of optimization.

sampler2D uImage0 : register(s0);
float2 uSource = float2(1920, 1080);
static const float weight[10][10] =
{
    { 0.0000, 0.0001, 0.0006, 0.0017, 0.0033, 0.0045, 0.0033, 0.0017, 0.0006, 0.0001 },
    { 0.0001, 0.0011, 0.0068, 0.0198, 0.0370, 0.0491, 0.0370, 0.0198, 0.0068, 0.0011 },
    { 0.0006, 0.0068, 0.0355, 0.0913, 0.1574, 0.1995, 0.1574, 0.0913, 0.0355, 0.0068 },
    { 0.0017, 0.0198, 0.0913, 0.2124, 0.3349, 0.4019, 0.3349, 0.2124, 0.0913, 0.0198 },
    { 0.0033, 0.0370, 0.1574, 0.3349, 0.4903, 0.5613, 0.4903, 0.3349, 0.1574, 0.0370 },
    { 0.0045, 0.0491, 0.1995, 0.4019, 0.5613, 1.0000, 0.5613, 0.4019, 0.1995, 0.0491 },
    { 0.0033, 0.0370, 0.1574, 0.3349, 0.4903, 0.5613, 0.4903, 0.3349, 0.1574, 0.0370 },
    { 0.0017, 0.0198, 0.0913, 0.2124, 0.3349, 0.4019, 0.3349, 0.2124, 0.0913, 0.0198 },
    { 0.0006, 0.0068, 0.0355, 0.0913, 0.1574, 0.1995, 0.1574, 0.0913, 0.0355, 0.0068 },
    { 0.0001, 0.0011, 0.0068, 0.0198, 0.0370, 0.0491, 0.0370, 0.0198, 0.0068, 0.0011 }
};

float3 GammaCorrect(float3 c, float gamma)
{
    return pow(abs(c), float3(gamma, gamma, gamma));
}

float3 makeBloom(sampler2D tex, float lod, float2 uv)
{
    float2 pixelSize = 1.0 / uSource;
    float offset = pixelSize;

    float lodFactor = pow(2.0, lod);
    float2 scale = lodFactor * pixelSize;
    float2 coord = (uv - offset) * lodFactor;
    
    if (abs(coord.x - 0.5) >= scale.x + 0.5 || abs(coord.y - 0.5) >= scale.y + 0.5)
        return float3(0, 0, 0);

    float3 bloom = float3(0, 0, 0);
    float totalWeight = 0.0;

    for (int i = -5; i < 5; i++)
    {
        for (int j = -5; j < 5; j++)
        {
            float2 offsetCoord = float2(i, j) * scale + coord + lodFactor * pixelSize;
            float temp = weight[i + 5][j + 5];
            
            float3 sample = GammaCorrect(tex2Dlod(tex, float4(offsetCoord, 0, lod)).rgb, 2.2);
            bloom += sample * temp;
            totalWeight += temp;
        }
    }
    bloom /= (totalWeight != 0 ? totalWeight : 1);
    return bloom;
}

float4 main(float2 fragCoord : TEXCOORD0) : COLOR0
{
    float3 blur = makeBloom(uImage0, 4.0, fragCoord);

    return float4(GammaCorrect(blur, 1.0 / 2.2), 1.0);
}

technique Technique1
{
    pass PrecalcGaussBlurPass
    {
        PixelShader = compile ps_3_0 main();
    }
}