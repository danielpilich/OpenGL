using GLFW;
using GlmSharp;

using Shaders;
using Models;

using OpenTK;
using OpenTK.Graphics.OpenGL4;

using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

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

        static int tex;
        static int tex2;

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

            // Textures generated using LeonardoAI
            tex = ReadTexture("earth.jpg", TextureUnit.Texture0);
            tex2 = ReadTexture("earthLight.jpg", TextureUnit.Texture1);
            Glfw.SetKeyCallback(window, kc);
            GL.Enable(EnableCap.DepthTest);

        }

        public static void FreeOpenGLProgram(Window window)
        {

        }

        //MODYFIKACJA. Ta wersja funkcji pozwala łatwo wczytać teksturę do innej jednostki teksturującej - należy ją podać jako argument.
        public static int ReadTexture(string filename, TextureUnit textureUnit)
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

        public static void DrawScene(Window window, float angle_x, float angle_y, float selfRotationAngle)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            mat4 P = mat4.Perspective(glm.Radians(50.0f), 1, 1, 50);
            mat4 V = mat4.LookAt(new vec3(0, 0, -5), new vec3(0, 0, 0), new vec3(0, 1, 0)) *
                 mat4.Rotate(angle_y, new vec3(0, 1, 0)) *
                 mat4.Rotate(angle_x, new vec3(1, 0, 0));

            shader.Use();
            GL.UniformMatrix4(shader.U("P"), 1, false, P.Values1D);
            GL.UniformMatrix4(shader.U("V"), 1, false, V.Values1D);

            // Nachylenie osi obrotu
            vec3 inclinedAxis = new vec3(1, 1, 0).Normalized;

            mat4 M = mat4.Rotate(angle_y, new vec3(0, 1, 0)) *
                     mat4.Rotate(angle_x, new vec3(1, 0, 0)) *
                     mat4.Rotate(selfRotationAngle, inclinedAxis);

            GL.UniformMatrix4(shader.U("M"), 1, false, M.Values1D);

            GL.Uniform1(shader.U("tex"), 0);
            GL.Uniform1(shader.U("tex2"), 1);

            GL.EnableVertexAttribArray(shader.A("vertex"));
            GL.EnableVertexAttribArray(shader.A("normal"));
            GL.EnableVertexAttribArray(shader.A("texCoord"));
            GL.EnableVertexAttribArray(shader.A("color"));

            GL.VertexAttribPointer(shader.A("vertex"), 4, VertexAttribPointerType.Float, false, 0, MyCube.vertices);
            GL.VertexAttribPointer(shader.A("normal"), 4, VertexAttribPointerType.Float, false, 0, MyCube.vertexNormals);
            GL.VertexAttribPointer(shader.A("texCoord"), 2, VertexAttribPointerType.Float, false, 0, MyCube.texCoords);
            GL.VertexAttribPointer(shader.A("color"), 4, VertexAttribPointerType.Float, false, 0, MyCube.colors);

            GL.DrawArrays(PrimitiveType.Triangles, 0, MyCube.vertexCount);

            GL.DisableVertexAttribArray(shader.A("vertex"));
            GL.DisableVertexAttribArray(shader.A("normal"));
            GL.DisableVertexAttribArray(shader.A("texCoord"));
            GL.DisableVertexAttribArray(shader.A("color"));

            Glfw.SwapBuffers(window);
        }

        static void Main(string[] args)
        {
            Glfw.Init(); // Zainicjuj bibliotekę GLFW

            Window window = Glfw.CreateWindow(500, 500, "OpenGL", GLFW.Monitor.None, Window.None);

            Glfw.MakeContextCurrent(window);
            Glfw.SwapInterval(1);

            GL.LoadBindings(new BC());

            float angle_x = 0;
            float angle_y = 0;
            float selfRotationAngle = 0;

            InitOpenGLProgram(window);

            Glfw.Time = 0;

            while (!Glfw.WindowShouldClose(window))
            {
                float time = (float)Glfw.Time;
                angle_x += speed_x * time;
                angle_y += speed_y * time;
                selfRotationAngle += 1.0f * time; // Prędkość obrotu wokół nachylonej osi
                Glfw.Time = 0;
                DrawScene(window, angle_x, angle_y, selfRotationAngle);

                Glfw.PollEvents();
            }

            FreeOpenGLProgram(window);

            Glfw.Terminate();
        }
    }
}