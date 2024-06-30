using GLFW;
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

        static ObjLoader objLoader = new ObjLoader();

        static int tex;
        static int tex2;
        static int tex3;

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

            tex = ReadTexture("Texture/earth.jpg", TextureUnit.Texture0);
            tex2 = ReadTexture("Texture/earthDark.jpg", TextureUnit.Texture1);
            tex3 = ReadTexture("Texture/earthLighting.png", TextureUnit.Texture2);

            Glfw.SetKeyCallback(window, kc);
            GL.Enable(EnableCap.DepthTest);

            objLoader.Load("Models/sphere.obj");
        }

        public static void FreeOpenGLProgram(Window window)
        {
            GL.DeleteTexture(tex);
            GL.DeleteTexture(tex2);
            GL.DeleteTexture(tex3);
        }

        //MODYFIKACJA. Ta wersja funkcji pozwala łatwo wczytać teksturę do innej jednostki teksturującej - należy ją podać jako argument.
        public static int ReadTexture(string filename, TextureUnit textureUnit)
        {
            var tex = GL.GenTexture();
            GL.ActiveTexture(textureUnit);
            GL.BindTexture(TextureTarget.Texture2D, tex);

            Bitmap bitmap = new Bitmap(filename);

            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

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

            mat4 V = mat4.LookAt(new vec3(2, 2, -3), new vec3(0, 0, 0), new vec3(0, 1, 0)) *
                     mat4.Rotate(angle_y, new vec3(0, 1, 0)) *
                     mat4.Rotate(angle_x, new vec3(1, 0, 0));

            shader.Use();
            GL.UniformMatrix4(shader.U("P"), 1, false, P.Values1D);
            GL.UniformMatrix4(shader.U("V"), 1, false, V.Values1D);

            float earthTiltAngle = glm.Radians(23.5f);
            mat4 tiltMatrix = mat4.Rotate(earthTiltAngle, new vec3(1, 0, 0));

            mat4 selfRotationMatrix = mat4.Rotate(selfRotationAngle, new vec3(0, 1, 0));

            mat4 M = tiltMatrix * selfRotationMatrix;
            GL.UniformMatrix4(shader.U("M"), 1, false, M.Values1D);

            GL.Uniform1(shader.U("tex"), 0);
            GL.Uniform1(shader.U("tex2"), 1);
            GL.Uniform1(shader.U("tex3"), 2);

            // Generowanie Ziemi
            GL.EnableVertexAttribArray(shader.A("vertex"));
            GL.EnableVertexAttribArray(shader.A("normal"));
            GL.EnableVertexAttribArray(shader.A("texCoord"));

            GL.VertexAttribPointer(shader.A("vertex"), 4, VertexAttribPointerType.Float, false, 0, objLoader.Vertices.ToArray());
            GL.VertexAttribPointer(shader.A("normal"), 4, VertexAttribPointerType.Float, false, 0, objLoader.Normals.ToArray());
            GL.VertexAttribPointer(shader.A("texCoord"), 2, VertexAttribPointerType.Float, false, 0, objLoader.TexCoords.ToArray());

            GL.DrawArrays(PrimitiveType.Triangles, 0, objLoader.Vertices.Count / 4);

            GL.DisableVertexAttribArray(shader.A("vertex"));
            GL.DisableVertexAttribArray(shader.A("normal"));
            GL.DisableVertexAttribArray(shader.A("texCoord"));

            Glfw.SwapBuffers(window);
        }

        static void Main(string[] args)
        {
            Glfw.Init();

            Window window = Glfw.CreateWindow(500, 500, "OpenGL", GLFW.Monitor.None, Window.None);

            Glfw.MakeContextCurrent(window);
            Glfw.SwapInterval(1);

            GL.LoadBindings(new BC());

            float angle_x = 0.4f;
            float angle_y = 0.0f;
            float selfRotationAngle = 0;

            InitOpenGLProgram(window);

            Glfw.Time = 0;

            while (!Glfw.WindowShouldClose(window))
            {
                float time = (float)Glfw.Time;
                angle_x += speed_x * time;
                angle_y += speed_y * time;
                selfRotationAngle += 1.0f * time;
                Glfw.Time = 0;
                DrawScene(window, angle_x, angle_y, selfRotationAngle);

                Glfw.PollEvents();
            }
            FreeOpenGLProgram(window);

            Glfw.Terminate();
        }
    }
}