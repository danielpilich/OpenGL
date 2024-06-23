#version 330

//Zmienne jednorodne
uniform mat4 P;
uniform mat4 V;
uniform mat4 M;

//Atrybuty
in vec4 vertex; //wspolrzedne wierzcholka w przestrzeni modelu
in vec4 normal; //wektor normalny w przestrzeni modelu
in vec4 color; //kolor skojarzony z wierzcho³kiem
in vec2 texCoord; //wspó³rzêdna teksturowana

out vec4 i_c;
out vec2 i_tc;
out vec4 vertex_f;
out vec4 normal_f;

void main(void) {
    vertex_f = vertex;
    normal_f = normal;
    i_c = color;
    i_tc = texCoord;
    gl_Position = P*V*M*vertex;
}