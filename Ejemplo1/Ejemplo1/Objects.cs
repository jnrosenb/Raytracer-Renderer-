using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Ejemplo2
{

    class Objects
    {
        public List<Material> materials;
    }

    class Mesh : Objects 
    {
        public string path;
        public bool cvn;

        //Para bounding box:
        private List<float> xbounds = new List<float>();
        private List<float> ybounds = new List<float>();
        private List<float> zbounds = new List<float>();
        public tuple_3[] cube_vertex = new tuple_3[9];
        public List<tuple_4> bound_faces = new List<tuple_4>();

        public Dictionary<int, tuple_3> vertex = new Dictionary<int, tuple_3>();
        public Dictionary<int, tuple_3> vertex_normals = new Dictionary<int, tuple_3>();
        public Dictionary<int, tuple_3> vt_dic = new Dictionary<int, tuple_3>();
        public Dictionary<int, tuple_3> vn_dic = new Dictionary<int, tuple_3>();
        public List<tuple_3[]> faces = new List<tuple_3[]>();

        public Mesh(string path, bool cvn, List<Material> materials) 
        {
            this.path = path;
            this.cvn = cvn;
            this.materials = materials;

            load_mesh();

            set_bounding_planes();

            if (cvn)
                compute_vertex_normals();
        }

        private void load_mesh()
        {
            //Line guarda cada linea del texto
            string line;
            //Index es para saber a que vertice se refieren las caras.
            int index = 1;
            int vt_index = 1;
            int vn_index = 1;

            System.IO.StreamReader file = new System.IO.StreamReader("..\\..\\meshes\\" + path);
            while ((line = file.ReadLine()) != null) 
            {
                string[] frag = line.Split(' ');

                if (frag[0] == "v") 
                {
                    save_vertex(frag[1], frag[2], frag[3], index);
                    index++;
                }
                else if (frag[0] == "#") { }
                else if (frag[0] == "f")
                {
                    save_face(frag[1], frag[2], frag[3], cvn);
                }
                else if (frag[0] == "vt")
                {
                    save_vt(frag[1], frag[2], "0.0", vt_index);
                    vt_index++;
                }
                else if (frag[0] == "vn")
                {
                    save_vn(frag[1], frag[2], frag[3], vn_index);
                    vn_index++;
                }
                else { }
            }
            file.Close();    
        }
        
        //Guarda vertices en diccionario que los relaciona  aun indice.
        private void save_vertex(string a, string b, string c, int index)
        {
            float x = float.Parse(a);
            float y = float.Parse(b);
            float z = float.Parse(c);
            vertex[index] = new tuple_3 { x = x, y = y, z = z };

            set_max_min(x, y, z);
        }

        //Guarda caras en listas. Mira primero si debe o no guardar vn y vt.
        private void save_face(string a, string b, string c, bool cvn)
        {
            tuple_3[] tuple_array = new tuple_3[3];
            if (a.Contains("/"))
            {
                string[] f1 = a.Split('/');
                string[] f2 = b.Split('/');
                string[] f3 = c.Split('/');

                for (int i = 0; i < 3; i++)
                {
                    if (f1[i] == "") f1[i] = "0.0";
                    if (f2[i] == "") f2[i] = "0.0";
                    if (f3[i] == "") f3[i] = "0.0";
                }

                tuple_array[0] = new tuple_3 { x = float.Parse(f1[0]), y = float.Parse(f2[0]), z = float.Parse(f3[0]) };
                tuple_array[1] = new tuple_3 { x = float.Parse(f1[1]), y = float.Parse(f2[1]), z = float.Parse(f3[1]) };
                tuple_array[2] = new tuple_3 { x = float.Parse(f1[2]), y = float.Parse(f2[2]), z = float.Parse(f3[2]) };
                faces.Add(tuple_array);
            }
            else
            {
                float x = float.Parse(a);
                float y = float.Parse(b);
                float z = float.Parse(c);
                tuple_array[0] = new tuple_3 { x = x, y = y, z = z };
                faces.Add(tuple_array);
            }
        }

        //Guarda los valores de las texturas.
        private void save_vt(string a, string b, string c, int index)
        {
            float x = float.Parse(a);
            float y = float.Parse(b);
            float z = float.Parse(c);
            vt_dic[index] = new tuple_3 { x = x, y = y, z = z };
        }

        //Guarda los valores de las normales.
        private void save_vn(string a, string b, string c, int index)
        {
            float x = float.Parse(a);
            float y = float.Parse(b);
            float z = float.Parse(c);
            vn_dic[index] = new tuple_3 { x = x, y = y, z = z };
        }
        
        //Para cada triangulo, computa su normal y luego guarda la info de la normal de los vertices.
        private void compute_vertex_normals()
        {
            foreach (tuple_3[] face in faces) 
            {
                tuple_3 v1 = vertex[(int)face[0].x];
                tuple_3 v2 = vertex[(int)face[0].y];
                tuple_3 v3 = vertex[(int)face[0].z];

                //Saco la normal de la cara, y la sumo a la normal asignada a cada vector en su diccionario.
                tuple_3 normal = Vectores.Normalize(Vectores.CrossProduct((v2 - v1), (v3 - v1)));

                if (!vertex_normals.ContainsKey((int)face[0].x))
                    vertex_normals.Add((int)face[0].x, normal);
                else if (vertex_normals.ContainsKey((int)face[0].x))
                    vertex_normals[(int)face[0].x] = vertex_normals[(int)face[0].x] + normal;
                    
                if (!vertex_normals.ContainsKey((int)face[0].y))
                    vertex_normals.Add((int)face[0].y, normal);
                else if (vertex_normals.ContainsKey((int)face[0].y))
                    vertex_normals[(int)face[0].y] = vertex_normals[(int)face[0].y] + normal;
                
                if (!vertex_normals.ContainsKey((int)face[0].z))
                    vertex_normals.Add((int)face[0].z, normal);
                else if (vertex_normals.ContainsKey((int)face[0].z))
                    vertex_normals[(int)face[0].z] = vertex_normals[(int)face[0].z] + normal;
            }   
        }

        //Define datos de max y min de cada coord.
        private void set_max_min(float x, float y, float z) 
        {
            if (xbounds.Count == 0)
            {
                xbounds.Add(x);
                xbounds.Add(x);
                ybounds.Add(y);
                ybounds.Add(y);
                zbounds.Add(z);
                zbounds.Add(z);
            }
            else
            {
                if (x > xbounds[0])
                    xbounds[0] = x;
                if (x < xbounds[1])
                    xbounds[1] = x;
                if (y > ybounds[0])
                    ybounds[0] = y;
                if (y < ybounds[1])
                    ybounds[1] = y;
                if (z > zbounds[0])
                    zbounds[0] = z;
                if (z < zbounds[1])
                    zbounds[1] = z;
            }
        }

        //Mediante datos de max y min de cada coord, define 6 caras.
        private void set_bounding_planes()
        {
            if (xbounds[0] == xbounds[1])
                xbounds[0] += 0.001f;
            if (ybounds[0] == ybounds[1])
                ybounds[0] += 0.001f;
            if (zbounds[0] == zbounds[1])
                zbounds[0] += 0.001f;

            //Primero guarda las coordenadas en arreglo. ignora el primer elemento por comodidad:
            cube_vertex[1] = new tuple_3 { x = xbounds[0], y = ybounds[0], z = zbounds[0] };
            cube_vertex[2] = new tuple_3 { x = xbounds[0], y = ybounds[0], z = zbounds[1] };
            cube_vertex[3] = new tuple_3 { x = xbounds[0], y = ybounds[1], z = zbounds[0] };
            cube_vertex[4] = new tuple_3 { x = xbounds[0], y = ybounds[1], z = zbounds[1] };
            cube_vertex[5] = new tuple_3 { x = xbounds[1], y = ybounds[0], z = zbounds[0] };
            cube_vertex[6] = new tuple_3 { x = xbounds[1], y = ybounds[0], z = zbounds[1] };
            cube_vertex[7] = new tuple_3 { x = xbounds[1], y = ybounds[1], z = zbounds[0] };
            cube_vertex[8] = new tuple_3 { x = xbounds[1], y = ybounds[1], z = zbounds[1] };

            /*
            for (int i = 1; i < 9; i++)
                Vectores.Print(cube_vertex[i]);//*/

            //Ahora genera las caras del cubo:
            bound_faces.Add(new tuple_4 { x = 1, y = 2, z = 3, w = 4 });
            bound_faces.Add(new tuple_4 { x = 5, y = 6, z = 7, w = 8 });
            bound_faces.Add(new tuple_4 { x = 5, y = 6, z = 2, w = 1 });
            bound_faces.Add(new tuple_4 { x = 7, y = 8, z = 4, w = 3 });
            bound_faces.Add(new tuple_4 { x = 8, y = 6, z = 2, w = 4 });
            bound_faces.Add(new tuple_4 { x = 7, y = 5, z = 1, w = 3 });
        }
    }


    class Sphere : Objects
    {
        public float rad;
        public tuple_3 center;

        public Sphere(float rad, tuple_3 center, List<Material> materials)
        {
            this.rad = rad;
            this.center = center;
            this.materials = materials;

        }
    }


    class Cylinder : Objects
    {
        public float rad;
        public tuple_3 center;
        public tuple_3 direction;

        public Cylinder(float rad, tuple_3 center, tuple_3 direction, List<Material> materials)
        {
            this.rad = rad;
            this.center = center;
            this.direction = direction;
            this.materials = materials;
        }
    }
}
