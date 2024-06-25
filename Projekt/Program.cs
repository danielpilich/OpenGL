﻿using GLFW;
using GlmSharp;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Shaders;
using System.Drawing;

namespace PMLabs
{
    //Implementacja interfejsu dostosowującego metodę biblioteki Glfw służącą do pozyskiwania adresów funkcji i procedur OpenGL do współpracy z OpenTK.
    public class BC : IBindingsContext
    {
        public IntPtr GetProcAddress(string procName)
        {
            return Glfw.GetProcAddress(procName);
        }
    }

    class Program
    {
        static ShaderProgram shader;
        static float speed_y;
        static float speed_x;

        static List<float> vertices = new List<float>();
        static List<float> vertexNormals = new List<float>();
        static List<float> texCoords = new List<float>();
        static List<int> vertexIndices = new List<int>();
        static List<int> normalIndices = new List<int>();
        static List<int> texCoordIndices = new List<int>();

        static KeyCallback kc = KeyProcessor;
        public static void KeyProcessor(System.IntPtr window, Keys key, int scanCode, InputState state, ModifierKeys mods)
        {
            if (state == InputState.Press)
            {
                if (key == Keys.Left) speed_y = -3.14f;
                if (key == Keys.Right) speed_y = 3.14f;
                if (key == Keys.Up) speed_x = -3.14f;
                if (key == Keys.Down) speed_x = 3.14f;
            }
            if (state == InputState.Release)
            {
                if (key == Keys.Left) speed_y = 0;
                if (key == Keys.Right) speed_y = 0;
                if (key == Keys.Up) speed_x = 0;
                if (key == Keys.Down) speed_x = 0;
            }
        }

        public static void InitOpenGLProgram(Window window)
        {
            GL.ClearColor(0, 0, 0, 1);
            shader = new ShaderProgram("vertex_shader.glsl", "fragment_shader.glsl");
            Glfw.SetKeyCallback(window, kc);
            GL.Enable(EnableCap.DepthTest);

            string path = "../../../sphere.obj";

            using (StreamReader reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("v "))
                    {
                        var parts = line.Split(' ');
                        vertices.Add(float.Parse(parts[1])); // x
                        vertices.Add(float.Parse(parts[2])); // y
                        vertices.Add(float.Parse(parts[3])); // z
                        vertices.Add(1.0f);
                    }
                    else if (line.StartsWith("vn "))
                    {
                        var parts = line.Split(' ');
                        vertexNormals.Add(float.Parse(parts[1])); // x
                        vertexNormals.Add(float.Parse(parts[2])); // y
                        vertexNormals.Add(float.Parse(parts[3])); // z
                        vertexNormals.Add(1.0f);
                    }
                    else if (line.StartsWith("vt "))
                    {
                        var parts = line.Split(' ');
                        texCoords.Add(float.Parse(parts[1]));
                        texCoords.Add(float.Parse(parts[2]));
                    }
                    else if (line.StartsWith("f "))
                    {
                        var parts = line.Split(' ');
                        for (int i = 1; i < parts.Length; i++)
                        {
                            var indices = parts[i].Split('/');
                            vertexIndices.Add(int.Parse(indices[0]) - 1);
                            if (indices.Length > 1 && indices[1] != "")
                                texCoordIndices.Add(int.Parse(indices[1]) - 1);
                            if (indices.Length > 2 && indices[2] != "")
                                normalIndices.Add(int.Parse(indices[2]) - 1);
                        }
                    }
                }
            }
        }

        public static void FreeOpenGLProgram(Window window)
        {

        }

        //MODYFIKACJA. Ta wersja funkcji pozwala łatwo wczytać teksturę do innej jednostki teksturującej - należy ją podać jako argument.
        public static int ReadTexture(string filename, TextureUnit textureUnit = TextureUnit.Texture0)
        {
            var tex = GL.GenTexture();
            GL.ActiveTexture(textureUnit);
            GL.BindTexture(TextureTarget.Texture2D, tex);

            Bitmap bitmap = new Bitmap(filename);
            System.Drawing.Imaging.BitmapData data = bitmap.LockBits(
              new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
              System.Drawing.Imaging.ImageLockMode.ReadOnly,
              System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width,
              data.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);
            bitmap.Dispose();

            GL.TexParameter(TextureTarget.Texture2D,
              TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D,
              TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            return tex;
        }

        public static void DrawScene(Window window, float angle_x, float angle_y)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            mat4 P = mat4.Perspective(glm.Radians(50.0f), 1, 1, 50);
            mat4 V = mat4.LookAt(new vec3(0, 0, -3), new vec3(0, 0, 0), new vec3(0, 1, 0));

            shader.Use();
            GL.UniformMatrix4(shader.U("P"), 1, false, P.Values1D);
            GL.UniformMatrix4(shader.U("V"), 1, false, V.Values1D);

            mat4 M = mat4.Rotate(angle_y, new vec3(0, 1, 0)) * mat4.Rotate(angle_x, new vec3(1, 0, 0));
            GL.UniformMatrix4(shader.U("M"), 1, false, M.Values1D);

            GL.EnableVertexAttribArray(shader.A("vertex"));
            GL.EnableVertexAttribArray(shader.A("normal"));
            GL.EnableVertexAttribArray(shader.A("texCoord"));

            vertices = new List<float>();
            vertexNormals = new List<float>();
            texCoords = new List<float>();
            vertices.Add(0.000000f);
            vertices.Add(0.707107f);
            vertices.Add(-0.707107f);
            vertices.Add(1.0f);

            vertexNormals.Add(0.0464f);
            vertexNormals.Add(0.8810f);
            vertexNormals.Add(-0.4709f);
            vertexNormals.Add(0.0f);

            texCoords.Add(0.750000f);
            texCoords.Add(0.187500f);

            GL.VertexAttribPointer(shader.A("vertex"), 4, VertexAttribPointerType.Float, false, 0, vertices.ToArray());
            GL.VertexAttribPointer(shader.A("normal"), 4, VertexAttribPointerType.Float, false, 0, vertexNormals.ToArray());
            GL.VertexAttribPointer(shader.A("texCoord"), 2, VertexAttribPointerType.Float, false, 0, texCoords.ToArray());

            GL.DrawArrays(PrimitiveType.Lines, 0, vertices.Count / 4);

            GL.DisableVertexAttribArray(shader.A("vertex"));
            GL.DisableVertexAttribArray(shader.A("normal"));
            GL.DisableVertexAttribArray(shader.A("texCoord"));

            Glfw.SwapBuffers(window);
        }


        static void Main(string[] args)
        {
            Glfw.Init();//Zainicjuj bibliotekę GLFW

            Window window = Glfw.CreateWindow(500, 500, "OpenGL", GLFW.Monitor.None, Window.None);

            Glfw.MakeContextCurrent(window);
            Glfw.SwapInterval(1);

            GL.LoadBindings(new BC());

            float angle_x = 0;
            float angle_y = 0;

            InitOpenGLProgram(window);

            Glfw.Time = 0;

            while (!Glfw.WindowShouldClose(window))
            {
                angle_x += speed_x * (float)Glfw.Time;
                angle_y += speed_y * (float)Glfw.Time;
                Glfw.Time = 0;
                DrawScene(window, angle_x, angle_y);

                Glfw.PollEvents();
            }


            FreeOpenGLProgram(window);

            Glfw.Terminate();
        }


    }
}