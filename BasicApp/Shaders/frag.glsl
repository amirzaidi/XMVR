#version 430

// Uniform constants.
uniform vec3 CameraPosition;
uniform vec3 LightDirection;
uniform vec3 LightColor;
uniform vec3 BackgroundColor;

// In data attributes.
// These must match between vertex and fragment shader.
in vec3 FragmentPosition;
in vec2 FragmentTexCoords;
in vec3 FragmentNormal;

// Out data attributes.
// The location determines the order for these.
// To-Do: Can we remove the location specifier?
layout(location = 0) out vec4 OutColor;

// Entry point for fragment shader.
void main()
{
    float ambient = 0.15f;
    float diffuse = 0.50f * max(0.f, dot(FragmentNormal, -LightDirection));
    float specular = 1.50f * pow(max(0.f, dot(FragmentNormal, normalize(normalize(CameraPosition - FragmentPosition) - LightDirection))), 10.f);
    if (diffuse == 0.f)
    {
        specular = 0.f;
    }

    OutColor = vec4(BackgroundColor * ambient + LightColor * (diffuse + specular), 1.0f);
}
