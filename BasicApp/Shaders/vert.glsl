#version 430

// Uniform constants.
uniform mat4 Model;
uniform mat4 ModelNormal;
uniform mat4 View;
uniform mat4 Projection;

// In data attributes.
// These are defined in the vertex layout.
layout(location = 0) in vec3 VertexPosition;
/*
in vec3 VertexNormal;
in vec2 VertexTexCoords;

// Out data attributes.
// These must match between vertex and fragment shader.
out vec3 FragmentPosition;
out vec3 FragmentNormal;
out vec2 FragmentTexCoords;
*/

// Entry point for vertex shader.
void main()
{
    vec4 v = vec4(VertexPosition, 1.0);
    v = Model * v;
    v = View * v;
    v = Projection * v;
    gl_Position = v;

    /*
    // Translate the position from world-space to screen-space.
    vec4 vertexPosition = Model * vec4(VertexPosition, 1.f);
    gl_Position = ViewProj * vertexPosition;

    // Pass data to fragment shader.
    // Rotate and scale normal vector.
    FragmentPosition = VertexPosition;
    FragmentNormal = (ModelNormal * vec4(VertexNormal, 1.f)).xyz;
    FragmentTexCoords = VertexTexCoords;
    */
}
