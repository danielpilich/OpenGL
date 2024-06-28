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
        //static Sphere sphere = new Sphere();

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

            // Load the .obj file
            objLoader.Load("Models/sphere.obj");
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

            GL.Uniform4(shader.U("color"), 0.1f, 0.1f, 0.9f, 1f);

            GL.EnableVertexAttribArray(0); // Vertices
            GL.EnableVertexAttribArray(1); // Normals
            GL.EnableVertexAttribArray(2); // TexCoords

            // Bind the .obj data using indexed drawing
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // Unbind any previously bound VBO

            // Vertex positions
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, objLoader.Vertices.ToArray());

            // Normals
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, objLoader.Normals.ToArray());

            // Texture coordinates
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 0, objLoader.TexCoords.ToArray());

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            // Draw using indexed vertices
            GL.DrawElements(PrimitiveType.Triangles, objLoader.VertexIndices.Count, DrawElementsType.UnsignedInt, objLoader.VertexIndices.ToArray());

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);

            Glfw.SwapBuffers(window);
        }


        static void Main(string[] args)
        {
            Glfw.Init();

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