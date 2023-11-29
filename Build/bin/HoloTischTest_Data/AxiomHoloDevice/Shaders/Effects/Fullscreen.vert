#version 430

//Input Format
layout(location = 0) in vec3 position0;
layout(location = 2) in vec2 texcoord0;

//Output Format
out vec2 passTexcoord0;

void main()
{
  gl_Position = vec4(position0, 1);
  passTexcoord0 = texcoord0;
}
