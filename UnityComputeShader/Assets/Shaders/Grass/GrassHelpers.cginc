// 简单的 Shader 随机数功能, 参考 http://answers.unity.com/answers/624136/view.html
// 更多的讨论可以参考如下链接:
// https://forum.unity.com/threads/am-i-over-complicating-this-random-function.454887/#post-2949326
// 返回值是 [0, 1].
float rand(float3 co)
{
    return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
}

// 构建绕任意轴旋转的矩阵, 参考 https://gist.github.com/keijiro/ee439d5e7388f3aafc5296005c8c3f33
float3x3 AngleAxis3x3(float angle, float3 axis)
{
    float c, s;
    sincos(angle, s, c);

    float t = 1 - c;
    float x = axis.x;
    float y = axis.y;
    float z = axis.z;

    return float3x3
    (
        t * x * x + c, t * x * y - s * z, t * x * z + s * y,
        t * x * y + s * z, t * y * y + c, t * y * z - s * x,
        t * x * z - s * y, t * y * z + s * x, t * z * z + c
    );
}

// 获取三角形的中心点.
float3 GetTriangleCenter(float3 a, float3 b, float3 c)
{
    return (a + b + c) / 3.0;
}
float2 GetTriangleCenter(float2 a, float2 b, float2 c)
{
    return (a + b + c) / 3.0;
}

void GetTriangleNormalAndTangentToWorldMatrix(float3 a, float3 b, float3 c,
    out float3 normalWS, out float3x3 tangentToWorld)
{
    float3 tangentWS = normalize(b - a);
    normalWS = normalize(cross(tangentWS, c - a));
    float3 bitangentWS = normalize(cross(tangentWS, normalWS));
    tangentToWorld = transpose(float3x3(tangentWS, bitangentWS, normalWS));
}