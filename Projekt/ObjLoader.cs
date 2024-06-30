using System.Globalization;

public class ObjLoader
{
    public List<float> Vertices { get; private set; } = new List<float>();
    public List<float> Normals { get; private set; } = new List<float>();
    public List<float> TexCoords { get; private set; } = new List<float>();

    public List<float> TempVertices { get; private set; } = new List<float>();
    public List<float> TempNormals { get; private set; } = new List<float>();
    public List<float> TempTexCoords { get; private set; } = new List<float>();
    public List<int> VertexIndices { get; private set; } = new List<int>();
    public List<int> NormalIndices { get; private set; } = new List<int>();
    public List<int> TexCoordIndices { get; private set; } = new List<int>();

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
                    TempVertices.Add(float.Parse(parts[1], CultureInfo.InvariantCulture));
                    TempVertices.Add(float.Parse(parts[2], CultureInfo.InvariantCulture));
                    TempVertices.Add(float.Parse(parts[3], CultureInfo.InvariantCulture));
                    TempVertices.Add(1.0f);
                }
                else if (line.StartsWith("vn "))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    TempNormals.Add(float.Parse(parts[1], CultureInfo.InvariantCulture));
                    TempNormals.Add(float.Parse(parts[2], CultureInfo.InvariantCulture));
                    TempNormals.Add(float.Parse(parts[3], CultureInfo.InvariantCulture));
                    TempNormals.Add(0.0f);
                }
                else if (line.StartsWith("vt "))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    TempTexCoords.Add(float.Parse(parts[1], CultureInfo.InvariantCulture));
                    TempTexCoords.Add(float.Parse(parts[2], CultureInfo.InvariantCulture));
                }
                else if (line.StartsWith("f "))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 1; i < parts.Length; i++)
                    {
                        var indices = parts[i].Split('/');
                        VertexIndices.Add(int.Parse(indices[0]) - 1);
                        if (indices.Length > 1 && indices[1].Length > 0)
                        {
                            TexCoordIndices.Add(int.Parse(indices[1]) - 1);
                        }
                        if (indices.Length > 2 && indices[2].Length > 0)
                        {
                            NormalIndices.Add(int.Parse(indices[2]) - 1);
                        }
                    }
                }
            }
        }

        foreach (var index in VertexIndices)
        {
            var test = index * 4;
            Vertices.Add(TempVertices[test]); // x
            Vertices.Add(TempVertices[test + 1]); // y
            Vertices.Add(TempVertices[test + 2]); // z
            Vertices.Add(TempVertices[test + 3]); // w
        }

        foreach (var index in NormalIndices)
        {
            var test = index * 4;
            Normals.Add(TempNormals[test]);
            Normals.Add(TempNormals[test + 1]);
            Normals.Add(TempNormals[test + 2]);
            Normals.Add(TempNormals[test + 3]);
        }

        foreach (var index in TexCoordIndices)
        {
            var test = index * 2;
            TexCoords.Add(TempTexCoords[test]);
            TexCoords.Add(TempTexCoords[test + 1]);
        }
    }
}

