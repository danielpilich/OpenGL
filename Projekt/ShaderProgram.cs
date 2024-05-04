using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;

namespace Shaders
{
    
    //Obiekt klasy ShaderProgram reprezentuje program cieniujący.
    //Konstruktor wczytuje źródła poszczególnych shaderów wchodzących w skład programu cieniującego, kompiluje je, linkuje i umieszcza gotowy program w pamięci karty graficznej.
    //Dodatkowe metody ułatwiają korzystanie z programu cieniujacego.
    public class ShaderProgram
    {
        int shaderProgram;


        //vert - nazwa pliku z kodem vertex shadera        
        //frag - nazwa pliku z kodem fragment shadera
        public ShaderProgram(string vert, string frag)
        {
            Construct(vert, "", frag);            
        }

        //vert - nazwa pliku z kodem vertex shadera
        //geom - nazwa pliku z kodem geometry shadera (opcjonalna), pomijane jeżeli geom==""
        //frag - nazwa pliku z kodem fragment shadera
        public ShaderProgram(string vert, string geom, string frag)
        {
            Construct(vert, geom, frag);
        }

        //vert - nazwa pliku z kodem vertex shadera
        //geom - nazwa pliku z kodem geometry shadera (opcjonalna), pomijane jeżeli geom==""
        //frag - nazwa pliku z kodem fragment shadera
        public void Construct(string vert, string geom, string frag)
        {
            int vertexShader; //Uchwyt na obiekt vertex shadera
            int geometryShader; //Uchwyt na obiekt geometry shadera
            int fragmentShader; //Uchwyt na obiekt fragment shadera

            Console.WriteLine("Loading vertex shader...");

            //Utwórz obiekt ze skompilowanym vertex shaderem na podstawie źródła w pliku o nazwie w vert
            vertexShader = LoadShader(ShaderType.VertexShader, File.ReadAllText(vert));

            //Opcjonalnie utwórz obiekt ze skompilowanym vertex shaderem na podstawie źródła w pliku o nazwie w vert
            geometryShader = 0x7fffffff;
            if (geom != "")
            {
                Console.WriteLine("Loading geometry shader...");
                geometryShader = LoadShader(ShaderType.GeometryShader, File.ReadAllText(geom));
            }


            Console.WriteLine("Loading fragment shader...");
            //Utwórz obiekt ze skompilowanym fragment shaderem na podstawie źródła w pliku o nazwie w vert
            fragmentShader = LoadShader(ShaderType.FragmentShader, File.ReadAllText(frag));


            Console.WriteLine("Linking shader program\n");
            //Utwórz obiekt programu cieniującego
            shaderProgram = GL.CreateProgram();

            //Podłącz obiekt vertex shadera
            GL.AttachShader(shaderProgram, vertexShader);
            if (geometryShader == 0x7fffffff)
            {
                //Opcjonalnie podłącz obiekt geometry shadera
                GL.AttachShader(shaderProgram, geometryShader);
            }
            //Podłącz obiekt fragment shadera
            GL.AttachShader(shaderProgram, fragmentShader);

            //Linkuj program cieniujący
            GL.LinkProgram(shaderProgram);

            //Pobierz log błędów linkowania i wyświetl
            var log = GL.GetProgramInfoLog(shaderProgram);
           
            if (log.Length > 1) Console.WriteLine(log);

            //Po linkowaniu obiekty poszczeólnych shaderów nie są potrzebne, także je odłącz od obiektu programu cieniującego...
            GL.DetachShader(shaderProgram, fragmentShader);
            if (geometryShader != 0x7fffffff) GL.DetachShader(shaderProgram, geometryShader);
            GL.DetachShader(shaderProgram, vertexShader);

            //... i wykasuj
            GL.DeleteShader(fragmentShader);
            if (geometryShader != 0x7fffffff) GL.DeleteShader(geometryShader);
            GL.DeleteShader(vertexShader);
        }

        ~ShaderProgram()
        {
            //Usuń z pamięci karty graficznej obiekt programu cieniującego
            GL.DeleteProgram(shaderProgram);
        }


        //Aktywuj program cineiujący
        public void Use()
        {
            GL.UseProgram(shaderProgram);
        }
        

        //Zwraca identyfikator zmiennej jednorodnej o podanej nazwie
        public int U(string name)
        {
            //Pobierz identyfikator zmiennej jednorodnej
            return GL.GetUniformLocation(shaderProgram, name);
        }

        //Zwraca identyfikator atrybutu o podanej nazwie
        public int A(string name)
        {
            //Pobierz identyfikator atrybutu
            return GL.GetAttribLocation(shaderProgram, name);
        }

        //Kompiluje kod shadera określonego typu i zwraca obiekt OpenGL
        private static int LoadShader(ShaderType type, string source)
        {
            source=System.Text.RegularExpressions.Regex.Replace(source, @"[^\u0000-\u007F]+", string.Empty);//Usuń wszystkie znaki UTF - Błąd w shadersource najprawdopobniej bierze długość łańcucha jako długość źródła a nie liczbę bajtów, więc się później sypie.
            //Utwórz obiekt shadera
            var shader = GL.CreateShader(type);
            //Dodaj kod źródłowy shadera do obiektu
            GL.ShaderSource(shader,source);
            //Kompiluj shader
            GL.CompileShader(shader);

            //Opcjonalnie wyświetl log błędów na konsoli
            var log=GL.GetShaderInfoLog(shader);            
            if (log.Length>1) Console.WriteLine(log);

            return shader;
        }
    
    }
    
}

