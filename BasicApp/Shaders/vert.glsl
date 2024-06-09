#version 430

// Uniform constants.
uniform mat4 Model;
uniform mat4 ModelNormal;
uniform mat4 View;
uniform mat4 Projection;

// In data attributes.
// These are defined in the vertex layout.
in vec3 VertexPosition;
in vec2 VertexTexCoords;
in vec3 VertexNormal;

// Out data attributes.
// These must match between vertex and fragment shader.
out vec3 FragmentPosition;
out vec2 FragmentTexCoords;
out vec3 FragmentNormal;

// Entry point for vertex shader.
void main()
{
    // Position.
    vec4 v = vec4(VertexPosition, 1.f);
    v = Model * v;
    FragmentPosition = v.xyz;
    v = View * v;
    v = Projection * v;
    gl_Position = v;

    // Texture coordinates.
    FragmentTexCoords = VertexTexCoords;

    // Normal.
    vec4 n = vec4(VertexNormal, 1.f);
    n = ModelNormal * n;
    FragmentNormal = normalize(n.xyz);
}
