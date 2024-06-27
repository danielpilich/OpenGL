using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

public class ObjLoader
{
    public List<float> Vertices { get; private set; } = new List<float>();
    public List<float> Normals { get; private set; } = new List<float>();
    public List<float> TexCoords { get; private set; } = new List<float>();

    public void Load(string path)
    {
        using (StreamReader sr = new StreamReader(path))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.StartsWith("v "))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    Vertices.Add(float.Parse(parts[1], CultureInfo.InvariantCulture));
                    Vertices.Add(float.Parse(parts[2], CultureInfo.InvariantCulture));
                    Vertices.Add(float.Parse(parts[3], CultureInfo.InvariantCulture));
                }
                else if (line.StartsWith("vn "))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    Normals.Add(float.Parse(parts[1], CultureInfo.InvariantCulture));
                    Normals.Add(float.Parse(parts[2], CultureInfo.InvariantCulture));
                    Normals.Add(float.Parse(parts[3], CultureInfo.InvariantCulture));
                }
                else if (line.StartsWith("vt "))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    TexCoords.Add(float.Parse(parts[1], CultureInfo.InvariantCulture));
                    TexCoords.Add(float.Parse(parts[2], CultureInfo.InvariantCulture));
                }
            }
        }
    }
}