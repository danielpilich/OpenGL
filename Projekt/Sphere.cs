using GlmSharp;

namespace Models
{
    public class Sphere
    {
        public int vertexCount; //Liczba wierzchołków modelu
        public float[] vertices; //Tablica wierzchołków
        public float[] normals; //Tablica wektorów normalnych ścian
        public float[] texCoords; //Tablica współrzędnych teksturowania

        public Sphere()
        {
            buildSphere(1, 12, 12);
        }

        public Sphere(float r, float divs1, float divs2)
        {
            buildSphere(r, divs1, divs2);
        }

        vec4 generateSpherePoint(float r, float alpha, float beta)
        {
            alpha = glm.Radians(alpha);
            beta = glm.Radians(beta);
            return new vec4((float)(r * Math.Cos(alpha) * Math.Cos(beta)), (float)(r * Math.Cos(alpha) * Math.Sin(beta)), (float)(r * Math.Sin(alpha)), 1.0f);
        }

        vec4 computeVertexNormal(float alpha, float beta)
        {
            alpha = glm.Radians(alpha);
            beta = glm.Radians(beta);
            return new vec4((float)(Math.Cos(alpha) * Math.Cos(beta)), (float)(Math.Cos(alpha) * Math.Sin(beta)), (float)Math.Sin(alpha), 0.0f);
        }

        vec4 computeFaceNormal(vec4[] face)
        {
            vec3 a = new vec3(face[1] - face[0]);
            vec3 b = new vec3(face[2] - face[0]);

            return (new vec4(vec3.Cross(b, a), 0.0f)).Normalized;
        }

        void generateSphereFace(vec4[] vertices, ref vec4 faceNormal, float r, float alpha, float beta, float step_alpha, float step_beta)
        {
            vertices[0] = generateSpherePoint(r, alpha, beta);
            vertices[1] = generateSpherePoint(r, alpha + step_alpha, beta);
            vertices[2] = generateSpherePoint(r, alpha + step_alpha, beta + step_beta);
            vertices[3] = generateSpherePoint(r, alpha, beta + step_beta);

            faceNormal = computeFaceNormal(vertices);
        }

        void AddVec4(List<float> target, vec4 value)
        {
            target.Add(value[0]);
            target.Add(value[1]);
            target.Add(value[2]);
            target.Add(value[3]);
        }

        void buildSphere(float r, float mainDivs, float tubeDivs)
        {
            vec4[] face = { new vec4(), new vec4(), new vec4(), new vec4() };
            vec4 normal = new vec4();

            List<float> internalVertices = new List<float>();
            List<float> internalFaceNormals = new List<float>();

            float mult_alpha = 180.0f / tubeDivs;
            float mult_beta = 360.0f / mainDivs;

            vec4 green = new vec4(0, 1, 0, 1);

            for (int alpha = 0; alpha < Math.Round(tubeDivs); alpha++)
            {
                for (int beta = 0; beta < Math.Round(mainDivs); beta++)
                {
                    generateSphereFace(face, ref normal, r, alpha * mult_alpha - 90.0f, beta * mult_beta, mult_alpha, mult_beta);

                    AddVec4(internalVertices, face[0]);
                    AddVec4(internalVertices, face[1]);
                    AddVec4(internalVertices, face[2]);

                    AddVec4(internalVertices, face[0]);
                    AddVec4(internalVertices, face[2]);
                    AddVec4(internalVertices, face[3]);

                    for (int i = 0; i < 6; i++) AddVec4(internalFaceNormals, normal);
                }
            }

            vertices = internalVertices.ToArray();
            normals = internalFaceNormals.ToArray();
            vertexCount = internalVertices.Count / 4;

            texCoords = new float[vertexCount * 2];
        }
    }
}
