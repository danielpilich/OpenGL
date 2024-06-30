#version 330

//Zmienne jednorodne
uniform mat4 P;
uniform mat4 V;
uniform mat4 M;
uniform sampler2D tex;
uniform sampler2D tex2;
uniform sampler2D tex3;

in vec4 i_c;
in vec2 i_tc;
in vec4 vertex_f;
in vec4 normal_f;
vec4 lightSource;
vec4 l;
vec4 n;
vec4 r;
vec4 v;

vec4 earth;
vec4 lightning;

out vec4 pixelColor; //Zmienna wyjsciowa fragment shadera. Zapisuje sie do niej ostateczny (prawie) kolor piksela

void main(void) {

    vec4 color = texture2D(tex,i_tc);
    vec4 color2 = texture2D(tex2,i_tc);
    vec4 color3 = texture2D(tex3,i_tc);

	lightSource = vec4(0,0,-6,1);
    l = normalize(V*lightSource - V*M*vertex_f);
    n = normalize(V*M*normal_f);
    r = reflect(-l,n);
    v = normalize(vec4(0,0,0,1) - V*M*vertex_f);
    float rv = clamp(dot(r,v),0,1);
    rv = pow(rv,100);
    earth=vec4(0,0,0,1)*color+vec4(1,1,1,1)*color*dot(l,n)+color2*vec4(1,1,1,1)*rv;
    lightning=vec4(0.5,0.5,0.5,1)*color3+vec4(1,1,1,1)*color3*dot(-l,n);
    pixelColor = earth + lightning;
}
