#version 430

// Uniform constants.
uniform vec3 CameraPosition;
uniform vec3 LightDirection;
uniform vec3 LightColor;

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
    float ambient = 0.025f;
    float diffuse = max(0.f, dot(FragmentNormal, -LightDirection));
    float specular = max(0.f, dot(FragmentNormal, normalize(normalize(CameraPosition - FragmentPosition) - LightDirection)));

    // Prevent bleeding.
    if (diffuse == 0.f)
    {
        specular = 0.f;
    }

    OutColor = vec4(ambient + LightColor * (diffuse + pow(specular, 3.f)), 1.0f);
}
