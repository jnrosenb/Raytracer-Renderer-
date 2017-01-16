using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Ejemplo2;

namespace raytracer
{

  class Scene
  {
    public Dictionary<string, object> Parameters { get; set; }
    public Camera Camera { get; set; }
    public List<Objects> Objects { get; set; }
    public List<Light> Lights { get; set; }
    public AmbientLight ambient_light { get; set; }

    public static Random rand = new Random(DateTime.Now.Millisecond);
    public static int ray_count; 

    //Constructor de Scene.
    Scene(Dictionary<string, object> parameters, Camera camera, List<Objects> objects, List<Light> lights, AmbientLight ambient)//Dictionary<string, Light> lights)
    {
        Parameters = parameters;
        Camera = camera;
        Objects = objects;
        Lights = lights;
        ambient_light = ambient;
    }


    //Metodo recursivo que saca todos los datos de la escena y los guarda en scene.
    private static object ObjectHook(JToken token)
    {
      switch (token.Type)
      {
        case JTokenType.Object:

          var children = token.Children<JProperty>();
          var dic = children.ToDictionary(prop => prop.Name, prop => ObjectHook(prop.Value));
          
          if (dic.ContainsKey("__type__"))
          {
            if (dic["__type__"].ToString() == "scene")
            {
                AmbientLight ambient = null;
                var camera = (Camera)dic["camera"];
                var Lights = ((List<Object>)dic["lights"]).ConvertAll(x => (Light)x);
                foreach (object l in (List<Object>)dic["lights"])
                {
                    Light light = (Light)l;
                    if (light.name == "ambient_light")
                    {
                        ambient = (AmbientLight)light;
                        break;
                    }
                }
                var Objects = ((List<Object>)dic["objects"]).ConvertAll(x => (Objects)x);
                return new Scene((Dictionary<string, object>)dic["params"], camera, Objects, Lights, ambient);
            }
            else if (dic["__type__"].ToString() == "camera")
            {
              var fov = Convert.ToSingle(dic["fov"]);
              var position = ((List<object>)dic["position"]).Select(Convert.ToSingle).ToList();
              var up = ((List<object>)dic["up"]).Select(Convert.ToSingle).ToList();
              var target = ((List<object>)dic["target"]).Select(Convert.ToSingle).ToList();

              var near = -1.0f;
              if (dic.ContainsKey("near"))
                  near = Convert.ToSingle(dic["near"]);
              var lens_size = 0.0f;
              if (dic.ContainsKey("lensSize"))
                  lens_size = Convert.ToSingle(dic["lensSize"]);

              tuple_3 pos = new tuple_3 { x = position[0], y = position[1], z = position[2]};
              tuple_3 cup = new tuple_3 { x = up[0], y = up[1], z = up[2] };
              tuple_3 tgt = new tuple_3 { x = target[0], y = target[1], z = target[2] };

              return new Camera(fov, pos, cup, tgt, near, lens_size);
            }
            else if (dic["__type__"].ToString() == "mesh")
            {
                var path = (string)dic["file_path"];
                
                var cvn = false;
                if (dic.ContainsKey("compute_vertex_normals"))
                    cvn = (bool)dic["compute_vertex_normals"];

                var names = ((List<Object>)dic["materials"]).ConvertAll(x => (string)x);
                List<Material> materials = new List<Material>();
                foreach (string name in names)
                {
                    materials.Add(Resources.materials[name]);
                }

                return new Mesh(path, cvn, materials);
            }
            else if (dic["__type__"].ToString() == "sphere")
            {
                var radius = Convert.ToSingle(dic["radius"]);
                var position = ((List<object>)dic["position"]).Select(Convert.ToSingle).ToList();
                
                var names = ((List<Object>)dic["materials"]).ConvertAll(x => (string)x);
                List<Material> materials = new List<Material>(); 
                foreach (string name in names)
                {
                    materials.Add(Resources.materials[name]);
                }

                tuple_3 pos = new tuple_3 { x = position[0], y = position[1], z = position[2] };

                return new Sphere(radius, pos, materials);
            }
            else if (dic["__type__"].ToString() == "point_light")
            {
                var position = ((List<object>)dic["position"]).Select(Convert.ToSingle).ToList();
                var color = ((List<object>)dic["color"]).Select(Convert.ToSingle).ToList();
                tuple_3 pos = new tuple_3 { x = position[0], y = position[1], z = position[2] };

                var lightSize = -1.0f;
                if (dic.ContainsKey("lightSize"))
                    lightSize = Convert.ToSingle(dic["lightSize"]);
                List<float> lightNormal = null;
                if (dic.ContainsKey("lightNormal"))
                    lightNormal = ((List<object>)dic["lightNormal"]).Select(Convert.ToSingle).ToList();

                return new PointLight(pos, color, lightSize, lightNormal);
            }
            else if (dic["__type__"].ToString() == "ambient_light")
            {
                var color = ((List<object>)dic["color"]).Select(Convert.ToSingle).ToList();
                return new AmbientLight(color);
            }
          }
          return dic;

        case JTokenType.Array:
          return token.Select(ObjectHook).ToList();

        default:
          return ((JValue)token).Value;
      }
    }


    //Carga la escena completa y deja los pixeles listos para pintar.
    public static void LoadScene(string fileName)
    {
        try
        {
            //Recupero el json y guardo todos los valores en el objeto scene.
            var jsonString1 = File.ReadAllText(fileName);
            Resources.load();
            var scene1 = (Scene)ObjectHook(JToken.Parse(jsonString1));

            //Defino el width, height y near de la escena. Representa ancho y alto en espacio imagen.
            Console.Write("Ingrese width imagen: ");
            int width = int.Parse(Console.ReadLine());
            Console.Write("Ingrese height imagen: ");
            int height = int.Parse(Console.ReadLine());
            float near = 0.0f;
            if (scene1.Camera.near == -1.0f)
            {
                Console.Write("Ingrese distancia near: ");
                near = float.Parse(Console.ReadLine());
            }
            else near = scene1.Camera.near;
            tuple_3 e = scene1.Camera.position;
            tuple_3 t = scene1.Camera.target;

            //Se obtienen valores top, bottom right y left a partir de los otros.
            float top = near * (float)Math.Tan(DegreeToRadian(scene1.Camera.FOV / 2.0f));
            float bottom = -top;
            float right = ((float)width / height) * (top);
            float left = -right;
            float pix_width  = (float)Math.Abs((right - left) / width);
            float pix_height = (float)Math.Abs((top - bottom) / height);

            //Ahora se obtienen los vectores unitarios u,v,w.
            tuple_3 w = Vectores.Normalize(e - t);
            tuple_3 u = Vectores.Normalize(Vectores.CrossProduct(scene1.Camera.up, w));
            tuple_3 v = Vectores.Normalize(Vectores.CrossProduct(w, u));

            //Este sera el arreglo final con los datos de pixeles y colores.
            List<float>[,] imageData = new List<float>[width, height];

            //For en el que se ira transformando cada par ordenado de espacio imagen a espacio mundo.
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    //Primero paso todo a espacio camara (u,w,v) -> (x,y,z)
                    float i_u = (i + 0.5f) * (pix_width) - ((right - left) / 2.0f);
                    float j_v = (j + 0.5f) * (pix_height) - ((top - bottom) / 2.0f);
                    float k_w = -near;

                    //MULTIPLE-RAYS: Define los puntos por los que pasaran los rayos:
                    if (scene1.Parameters.ContainsKey("samplesPerPixel"))
                        ray_count = (int)Math.Sqrt(Convert.ToSingle(scene1.Parameters["samplesPerPixel"]));
                    else
                        ray_count = 1;
                    tuple_3[] ray_array = new tuple_3[ray_count * ray_count];
                    float i_u2, j_v2 = 0.0f;
                    int index = 0;
                    for (int i1 = 1; i1 <= ray_count; i1++)
                    {
                        j_v2 = (j_v - pix_height / 2.0f) + i1 * (pix_height / (ray_count + 1.0f));
                        for (int j1 = 1; j1 <= ray_count; j1++)
                        {
                            i_u2 = (i_u - pix_width / 2.0f) + j1 * (pix_width / (ray_count + 1.0f));
                            tuple_3 pixelPos  = e + (i_u2 * u) + (j_v2 * v) + (k_w * w);
                            ray_array[index++] = pixelPos;
                        }
                    }

                    //Multiples rayos. Por cada uno, vera con que objetos chocan:
                    List<float>[] rayImgData = new List<float>[ray_count * ray_count];
                    for (int n_ray = 0; n_ray < (ray_count * ray_count); n_ray++)
                    {
                        //Define origen de forma aleatoria, y usando define la d que corresponda segun el arreglo y el origen:
                        float arg = scene1.Camera.lensSize; /// 2.0f;

                        //Este se compara contra t, y va a decidir que objeto se pinta y cual no para un mismo pixel.
                        float distance_ray = 0.0f;

                        //Obtengo el nuevo origen y, de acuerdo a esto, el nuevo d:
                        tuple_3 curr_e = e + (next_float(-0.5f, 0.5f) * arg) * u + (next_float(-0.5f, 0.5f) * arg) * v;
                        tuple_3 curr_d = Vectores.Normalize(ray_array[n_ray] - curr_e);

                        //Console.WriteLine("({0}, {1}, {2})", curr_e.x, curr_e.y, curr_e.z);

                        //Ahora, para cada objeto, debere ver si el rayo intersecta:
                        foreach (Objects obj in scene1.Objects)
                        {
                            if (obj.GetType() == typeof(Sphere))
                                paint_sphere(imageData, obj, scene1, curr_d, curr_e, ref distance_ray, i, j, n_ray,ref rayImgData);
                            else if (obj.GetType() == typeof(Mesh))
                                paint_mesh(imageData, obj, scene1, curr_d, curr_e, ref distance_ray, i, j, n_ray, ref rayImgData);
                        }
                    }

                    //Promedia los rayos y lo guarda en image_data:
                    imageData[i,j] = new List<float>{0.0f, 0.0f, 0.0f};
                    float nullCount = 0;
                    for (int i2 = 0; i2 < ray_count * ray_count; i2++)
                    {
                        if (rayImgData[i2] != null)
                            imageData[i, j] = new List<float> { imageData[i, j][0] + rayImgData[i2][0], imageData[i, j][1] + rayImgData[i2][1], imageData[i, j][2] + rayImgData[i2][2] };
                        else
                            nullCount++;
                    }
                    float total = ray_count * ray_count - nullCount;
                    imageData[i, j] = new List<float> { imageData[i, j][0] / (total), imageData[i, j][1] / (total), imageData[i, j][2] / (total) };
                }
            }
            Display.GenerateImage(imageData, width, height);
        }
        catch (IOException)
        {
            Console.WriteLine("Error, archivo no existe!");
            Console.Read();
        }
    }


    //Metodo que pinta los meshes.
    private static void paint_mesh(List<float>[,] imageData, Objects obj, Scene scene, tuple_3 d, tuple_3 e, ref float distance_ray, int i, int j, int n_ray, ref List<float>[] rayImgData)
    {
        bool collisioned_bound_box = true; //Poner esto en false para probar la bounding box.
        Mesh mesh = (Mesh)obj;
        float t = 0.0f;
        tuple_3 normal = new tuple_3 { x = 0.0f, y = 0.0f, z = 0.0f };

        #region BOUNDING BOX
        /*//Primero vera si intersecta alguna de las caras del cubo:
        foreach (tuple_4 face in mesh.bound_faces) 
        {
            //Elementos de la ecuacion de interseccion con plano:
            tuple_3 v1 = mesh.cube_vertex[(int)face.x];
            tuple_3 v2 = mesh.cube_vertex[(int)face.y];
            tuple_3 v3 = mesh.cube_vertex[(int)face.z];

            tuple_3 tb = v2 - v1;
            tuple_3 tg = v3 - v1;
            tuple_3 aux = e - v1;
            float t = 0; float beta = 0; float gamma = 0;

            //Se calculan los valores de determinantes para luego usarlos:
            float det_A = dt(-d.x, tb.y, tg.y, tb.z, tg.z) + dt(-tb.x, -d.y, tg.y, -d.z, tg.z)+ dt(tg.x, -d.y, tb.y, -d.z, tb.z);
            float det_A1 = dt(aux.x, tb.y, tg.y, tb.z, tg.z) + dt(-tb.x, aux.y, tg.y, aux.z, tg.z)+ dt(tg.x, aux.y, tb.y, aux.z, tb.z);
            float det_A2 = dt(-d.x, aux.y, tg.y, aux.z, tg.z)+ dt(-aux.x, -d.y, tg.y, -d.z, tg.z)+ dt(tg.x, -d.y, aux.y, -d.z, aux.z);
            float det_A3 = dt(-d.x, tb.y, aux.y, tb.z, aux.z)+ dt(-tb.x, -d.y, aux.y, -d.z, aux.z)+ dt(aux.x, -d.y, tb.y, -d.z, tb.z);

            //Ahora con los determinantes, sacamos alfa beta y gama:
            if (det_A != 0)
            {
                t = det_A1 / det_A;
                beta = det_A2 / det_A;
                gamma = det_A3 / det_A;
            }

            //Caso que colisiona.
            if (beta >= 0.0f && gamma >= 0.0f && beta <= 1.0f && gamma <= 1.0f)
            {
                collisioned_bound_box = true;
                //break;
            }
        }//*/
        #endregion

        if (collisioned_bound_box)
        {
            bool intersect = intersects(scene, e, d, mesh, ref t, ref normal);
            
            //Caso que se pinta el fondo (por ahora que hay solo un objeto).
            if (!intersect)
            {
                List<float> bgc = ((List<object>)scene.Parameters["background_color"]).Select(Convert.ToSingle).ToList();
                if (rayImgData[n_ray] == null)
                {
                    rayImgData[n_ray] = new List<float> { bgc[0], bgc[1], bgc[2] };
                }
            }
            //Caso que se pinta.
            else 
            {
                //Si distance_ray es cero, toma el valor de t1 directamente.
                if (distance_ray == 0.0f)
                    distance_ray = t;

                //Posicion en coordenada mundo del punto que estamos pintando.
                tuple_3 obj_point = e + t * d;

                //Si esta mas cerca o igual de cerca, pinta sobre el otro objeto.
                if (t <= distance_ray)
                {
                    distance_ray = t;
                    List<float> obj_color = material_management(scene, mesh, e, d, obj_point, normal, -1);
                    rayImgData[n_ray] = new List<float> { obj_color[0], obj_color[1], obj_color[2] };
                }
            }
        }
        #region BOUNDING BOX
        /*//Como no colisiono contra la caja, pinta el background.
        else
        {
            List<float> bgc = ((List<object>)scene.Parameters["background_color"]).Select(Convert.ToSingle).ToList();
            if (imageData[i, j] == null)
                imageData[i, j] = new List<float> { bgc[0], bgc[1], bgc[2] };
        }//*/
        #endregion 
    }


    //Metodo que pinta las esferas.
    private static void paint_sphere(List<float>[,] imageData, Objects obj, Scene scene, tuple_3 d, tuple_3 e, ref float distance_ray, int i, int j, int n_ray, ref List<float>[] rayImgData)
    {
        Sphere sphere = (Sphere)obj;
        float t = 0.0f;
        tuple_3 normal = new tuple_3 { x = 0.0f, y = 0.0f, z = 0.0f };

        //Bool que ve si intersecta a esfera:
        bool intersect = intersects(scene, e, d, sphere, ref t, ref normal);

        if (!intersect)
        {
            List<float> bgc = ((List<object>)scene.Parameters["background_color"]).Select(Convert.ToSingle).ToList();
            if (rayImgData[n_ray] == null)//imageData[i, j] == null)
            {
                rayImgData[n_ray] = new List<float> { bgc[0], bgc[1], bgc[2] };
            }
        }
        else
        {
            //Si distance_ray es cero, toma el valor de t1 directamente.
            if (distance_ray == 0.0f)
                distance_ray = t;

            //Posicion en coordenada mundo del punto que estamos pintando.
            tuple_3 obj_point = e + t * d;

            //Si esta mas cerca o igual de cerca, pinta sobre el otro objeto.
            if (t <= distance_ray)
            {
                distance_ray = t;
                List<float> obj_color = material_management(scene, sphere, e, d, obj_point, normal, -1);
                rayImgData[n_ray] = new List<float> { obj_color[0], obj_color[1], obj_color[2] };
            }
        }
    }


    //Se encarga de las luces.
    private static List<float> material_management(Scene scene, Objects obj, tuple_3 e, tuple_3 d, tuple_3 obj_point, tuple_3 normal, float reflex_recursions)
    {
        //Vector de direccion desde punto (obj_point) hasta la camara (e).
        tuple_3 vision_dir = Vectores.Normalize(e - obj_point);

        //Se dejan en variables y definidos los distintos materiales que pueda tener el objeto:
        Material_brdf lambert = null;
        Material_brdf blinnPhong = null;
        Material_reflective reflective = null;
        Material_dielectric dielectric = null;
        
        //Se asignan los distintos materiales a sus variables (Por ahora, solo 1 de cada tipo):
        foreach (Material m in obj.materials)
        {
            if (m.GetType() == typeof(Material_brdf))
            {
                Material_brdf material = (Material_brdf)m;
                if (m.material_type == "lambert")
                    lambert = material;
                if (m.material_type == "blinnPhong")
                    blinnPhong = material;
            } 
            else if (m.GetType() == typeof(Material_reflective))
                reflective = (Material_reflective)m;
            else if (m.GetType() == typeof(Material_dielectric))
                dielectric = (Material_dielectric)m;
        }

        //Se declara el color difuso y especular del objeto.
        List<float> obj_difuse_color = new List<float> { 0.0f, 0.0f, 0.0f }; 
        List<float> obj_specular_color = new List<float> { 0.0f, 0.0f, 0.0f }; 
        List<float> obj_reflection_color = new List<float> { 0.0f, 0.0f, 0.0f };
        
        //Asigna los colores especulares y difusos si existen los materiales que correspondan:
        if (lambert != null)    obj_difuse_color = lambert.color;
        if (blinnPhong != null) obj_specular_color = blinnPhong.color;

        //Se definen las sumas de los colores difusos y especulares que guardaran infos de todas las luces que llegan a ese punto.
        tuple_3 difuse_color_sum = new tuple_3 { x = 0.0f, y = 0.0f, z = 0.0f };
        tuple_3 specular_color_sum = new tuple_3 { x = 0.0f, y = 0.0f, z = 0.0f };

        //Aqui define lo referente a la luz ambiente de la escena:
        List<float> obj_ambient_color = new List<float> { 0.0f, 0.0f, 0.0f }; //new List<float>(); //###
        if (scene.ambient_light != null) 
        {
            //Si tiene definido un color difuso, y ademas este es afectado por lus ambiente, entonces determina la luz de ambiente.
            AmbientLight ambient = scene.ambient_light;
            if (lambert != null && lambert.use_for_ambient)
                obj_ambient_color = new List<float> { ambient.color[0] * obj_difuse_color[0], ambient.color[1] * obj_difuse_color[1], ambient.color[2] * obj_difuse_color[2] };
        }

        //Aqui se encarga de las luces, sombras y los brillos difusos y especular:
        light_shade_manager(scene, obj_point, vision_dir, normal, obj, lambert, blinnPhong, ref difuse_color_sum, ref specular_color_sum);
        if (lambert != null)
            obj_difuse_color = new List<float> { obj_difuse_color[0] * difuse_color_sum.x, obj_difuse_color[1] * difuse_color_sum.y, obj_difuse_color[2] * difuse_color_sum.z };
        if (blinnPhong != null)
            obj_specular_color = new List<float> { obj_specular_color[0] * specular_color_sum.x, obj_specular_color[1] * specular_color_sum.y, obj_specular_color[2] * specular_color_sum.z };

        //Aca se maneja el tema de la reflexion:
        if (reflective != null)
            reflection(scene, obj, obj_point, normal, d, reflex_recursions, ref obj_reflection_color, reflective);

        #region DIELECTRIC (REFRACCION) NO IMPLEMENTADO
        /*Aca se maneja el tema de la refraccion:---------------------------------------------------------------------------------------------------------------------
        if (dielectric != null)
        {
            //Indice de refraccion de la escena por ahora:
            float I_escena = 1.0f;
            tuple_3 k = new tuple_3 { x = (float)Math.Exp(dielectric.attenuation[0]), y = (float)Math.Exp(dielectric.attenuation[0]), z = (float)Math.Exp(dielectric.attenuation[0]) };

            //Args para refleccion en materiales dielectricos (Material reflective dara null, ver que hacer con eso).
            //reflection(scene, obj, obj_point, normal, d, reflex_recursions, ref obj_reflection_color, reflective);

            tuple_3 r_refrac = new tuple_3();
            if ((1 - (Math.Pow(I_escena, 2.0f) / Math.Pow(dielectric.ref_index, 2.0f)) * (1 - Math.Pow(Vectores.PointProduct(d, normal), 2.0f))) >= 0.0f)
                r_refrac = Vectores.Normalize(((I_escena / dielectric.ref_index) * (d - (Vectores.PointProduct(d, normal)) * normal)) - ((float)Math.Sqrt(1 - (Math.Pow(I_escena, 2.0f) / Math.Pow(dielectric.ref_index, 2.0f)) * (1 - Math.Pow(Vectores.PointProduct(d, normal), 2.0f)))) * normal);
            tuple_3 q_refrac = obj_point + 0.0000001f * r_refrac;

            float R0 = (float)Math.Pow((dielectric.ref_index - 1) / (dielectric.ref_index + 1), 2.0f);
            float R_theta = R0 + (1.0f - R0)*((float)Math.Pow((1 - Vectores.PointProduct(d, normal)), 5.0f));

            //Rayo esta entrando:
            if (Vectores.PointProduct(d, normal) < 0.0f) 
            {

            }
            //Rayo esta saliendo:
            else if (Vectores.PointProduct(d, normal) >= 0.0f) 
            {

            }
        }//------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        #endregion

        //Color final que tendra el punto en cuestion:
        List<float> obj_color = new List<float> { 0.0f, 0.0f, 0.0f };
       
        if (lambert != null)
            obj_color = Vectores2.vectorSum(obj_color, obj_difuse_color);
        if (blinnPhong != null)
            obj_color = Vectores2.vectorSum(obj_color, obj_specular_color);
        if (scene.ambient_light != null)
            obj_color = Vectores2.vectorSum(obj_color, obj_ambient_color);
        if (reflective != null)
            obj_color = Vectores2.vectorSum(obj_color, obj_reflection_color);
   
        return obj_color;
    }


    //Metodo de la reflexion. Devuelve color de reflexion.
    private static void reflection(Scene scene, Objects obj, tuple_3 obj_point, tuple_3 normal, tuple_3 d, float reflex_recursions, ref List<float> obj_reflection_color, Material_reflective reflective) 
    {
        //R_reflex sera el vector direccion del rayo que se usara para revisar si existe reflejo o no.
        tuple_3 r_reflex = Vectores.Normalize(d - 2.0f * Vectores.PointProduct(d, normal) * normal);
        tuple_3 new_obj_normal = new tuple_3 { x = 0.0f, y = 0.0f, z = 0.0f };
        tuple_3 q_reflex = obj_point + 0.0000001f * r_reflex;
        Objects closest_obj = null;
        bool intersect = false;
        float closest_t = 0.0f;
        float t = 0.0f;

        //Para cada objeto, debere ver si el rayo intersecta y se produce sombra:
        foreach (Objects current_object in scene.Objects)
        {
            //Si es el mismo objeto, sigue con el proximo.
            if (current_object == obj) continue;

            intersect = intersects(scene, q_reflex, r_reflex, current_object, ref t, ref new_obj_normal);
            if (closest_t == 0.0f) closest_t = t;

            //Se queda con el t mas cercano.
            if (intersect && t <= closest_t)
            {
                closest_t = t;
                closest_obj = current_object;
            }
        }

        int ref_rec = (int)Convert.ToSingle(scene.Parameters["maxReflectionRecursions"]);
        if (closest_obj != null && (reflex_recursions < 0 || reflex_recursions > 0))
        {
            tuple_3 new_d = Vectores.Normalize(d + r_reflex);
            tuple_3 new_obj_point = q_reflex + closest_t * r_reflex;

            if (reflex_recursions < 0)
                obj_reflection_color = material_management(scene, closest_obj, q_reflex, r_reflex, new_obj_point, new_obj_normal, ref_rec - 1);
            else
                obj_reflection_color = material_management(scene, closest_obj, q_reflex, r_reflex, new_obj_point, new_obj_normal, reflex_recursions - 1);

            obj_reflection_color = new List<float> { obj_reflection_color[0] * reflective.color[0], obj_reflection_color[1] * reflective.color[1], obj_reflection_color[2] * reflective.color[2] };
        }
        else
        {
            if (obj.GetType() == typeof(Sphere))
            {
                int a = 0;
            }
            //Este seria el caso en que no choca con nada, por lo que refleja la luz de fondo:
            List<float> bgc = ((List<object>)scene.Parameters["background_color"]).Select(Convert.ToSingle).ToList();
            obj_reflection_color = new List<float> { bgc[0] * reflective.color[0], bgc[1] * reflective.color[1], bgc[2] * reflective.color[2] };
        }
    }


    //Metodo que se encarga de las luces y sombras.
    private static void light_shade_manager(Scene scene, tuple_3 obj_point, tuple_3 vision_dir, tuple_3 normal, Objects obj, Material_brdf lambert, Material_brdf blinnPhong, ref tuple_3 difuse_color_sum, ref tuple_3 specular_color_sum) 
    {
        //Aqui se encarga de las luces y los brillos difusos y especular:
        foreach (Light l in scene.Lights)
        {
            if (l.name == "point_light")
            {
                //Declaro todos los vectores unitarios que me interesan:
                PointLight light = (PointLight)l;
                tuple_3 light_n = new tuple_3();
                tuple_3 light_dir = new tuple_3();
                tuple_3 light_pos = new tuple_3();

                if (light.lightNormal != null)
                {
                    light_n = new tuple_3 { x = light.lightNormal[0], y = light.lightNormal[1], z = light.lightNormal[2] };

                    //Aqui puede quedar una normal cero, cuidado! (arreglar)
                    tuple_3 aux = new tuple_3();
                    if (light_n.x == light_n.y && light_n.y == light_n.z)   
                        aux = new tuple_3 { x = 0.0f, y = 0.0f, z = 1.0f };
                    else                                                    
                        aux = new tuple_3 { x = light_n.y, y = light_n.z, z = light_n.x };
                    
                    tuple_3 a = Vectores.Normalize(Vectores.CrossProduct(aux, light_n));
                    tuple_3 b = Vectores.Normalize(Vectores.CrossProduct(a, light_n));

                    light_pos = light.position + (next_float(-0.5f, 0.5f) * light.lightSize) * a + (next_float(-0.5f, 0.5f) * light.lightSize) * b;
                    light_dir = Vectores.Normalize(light_pos - obj_point);//light.position - obj_point);
                }
                else
                {
                    light_pos = light.position;
                    light_dir = Vectores.Normalize(light.position - obj_point);
                }

                //Esta parte se encarga de las sombras:
                bool shadowed = false;
                tuple_3 q = obj_point + 0.000001f * light_dir;

                //Para cada objeto, debere ver si el rayo intersecta y se produce sombra:
                foreach (Objects current_object in scene.Objects)
                {
                    float t = 0.0f;
                    tuple_3 normal2 = new tuple_3 { x = 0.0f, y = 0.0f, z = 0.0f };
                    //Si es el mismo objeto, sigue con el proximo.
                    if (current_object == obj) continue;

                    //Shadowed toma el valor que corresponda a si se intersecta:
                    shadowed = intersects(scene, q, light_dir, current_object, ref t, ref normal2);
                    if (shadowed && t >= 0.0f && t < Vectores.Module(light_pos - q))//light.position - q))
                        break;
                    //Deja shadowed en false por si termina el loop sin cumplir condicion.
                    shadowed = false;
                }

                //Brillo difuso con metodo lambert:
                if (lambert != null && !shadowed)
                {
                    //Ahora calculo el coseno entre la normal y el vector de luz:
                    float cos_theta = Vectores.PointProduct(normal, light_dir);
                    float f_difuse = Math.Max(0.0f, cos_theta);
                    tuple_3 difuse_color = new tuple_3 { x = f_difuse * light.color[0], y = f_difuse * light.color[1], z = f_difuse * light.color[2] };
                    difuse_color_sum += difuse_color;
                }

                //Brillo especular con metodo blinnphon:
                if (blinnPhong != null && !shadowed)
                {
                    tuple_3 h = Vectores.Normalize((light_dir + vision_dir) / 2);
                    float cos_theta2 = Vectores.PointProduct(normal, h);
                    float f_specular = (float)Math.Pow(Math.Max(0.0f, cos_theta2), Convert.ToInt32(blinnPhong.brdfParams["shininess"]));
                    tuple_3 specular_color = new tuple_3 { x = f_specular * light.color[0], y = f_specular * light.color[1], z = f_specular * light.color[2] };
                    specular_color_sum += specular_color;
                }
            }
        }
    }


    //Metodo que revisara si al tirar un rayo desde una posicion, se intersecta a algun objeto. Deja seteado t y normal.
    private static bool intersects(Scene scene, tuple_3 origen, tuple_3 d, Objects obj, ref float t, ref tuple_3 normal)
    {
        if (obj.GetType() == typeof(Sphere))
        {
            Sphere sphere = (Sphere)obj;
            tuple_3 c = sphere.center;

            //Elementos de la ecuacion cuadratica (A,B,C):
            float A = Vectores.PointProduct(d, d);
            float B = Vectores.PointProduct(2.0f * d, origen - c);
            float C = Vectores.PointProduct(origen - c, origen - c) - (float)Math.Pow(sphere.rad, 2.0f);
            float discr = (float)Math.Pow(B, 2) - 4.0f * C * A;

            //Caso que se pinta.
            if (discr >= 0.0f && A != 0.0f)
            {
                float temp = (float)Math.Sqrt(discr);
                float t1 = (-B - temp) / 2.0f * A;
                float t2 = (-B + temp) / 2.0f * A;

                //Temporalmente sera asi, despues cambiar.
                if (t1 < t2 && t1 >= 0) t = t1;
                else                    t = t2;

                //Posicion en coordenada mundo del punto que estamos pintando.
                tuple_3 obj_point = origen + t * d;
                normal = Vectores.Normalize(obj_point - sphere.center);
                return true;
            }
            return false;
        }
        else if (obj.GetType() == typeof(Mesh))
        {
            Mesh mesh = (Mesh)obj;
            foreach (tuple_3[] face in mesh.faces)
            {
                //Elementos de la ecuacion de interseccion con plano:
                tuple_3 v1 = mesh.vertex[(int)face[0].x];
                tuple_3 v2 = mesh.vertex[(int)face[0].y];
                tuple_3 v3 = mesh.vertex[(int)face[0].z];

                tuple_3 tb = v2 - v1;
                tuple_3 tg = v3 - v1;
                tuple_3 aux = origen - v1;
                float beta = 0; float gamma = 0; float alfa = 0;

                //Se calculan los valores de determinantes para luego usarlos:
                float det_A = dt(-d.x, tb.y, tg.y, tb.z, tg.z) + dt(-tb.x, -d.y, tg.y, -d.z, tg.z) + dt(tg.x, -d.y, tb.y, -d.z, tb.z);
                float det_A1 = dt(aux.x, tb.y, tg.y, tb.z, tg.z) + dt(-tb.x, aux.y, tg.y, aux.z, tg.z) + dt(tg.x, aux.y, tb.y, aux.z, tb.z);
                float det_A2 = dt(-d.x, aux.y, tg.y, aux.z, tg.z) + dt(-aux.x, -d.y, tg.y, -d.z, tg.z) + dt(tg.x, -d.y, aux.y, -d.z, aux.z);
                float det_A3 = dt(-d.x, tb.y, aux.y, tb.z, aux.z) + dt(-tb.x, -d.y, aux.y, -d.z, aux.z) + dt(aux.x, -d.y, tb.y, -d.z, tb.z);

                //Ahora con los determinantes, sacamos alfa beta y gama:
                if (det_A != 0)
                {
                    beta = det_A2 / det_A;
                    gamma = det_A3 / det_A;
                    alfa = 1 - beta - gamma;
                }

                //Caso rayo de luz intersecta a objeto. Esta en sombra y se sale del loop.
                if (det_A != 0 && alfa >= 0.0f && beta >= 0.0f && gamma >= 0.0f)
                {
                    //Deja seteada la distancia t:
                    t = det_A1 / det_A;

                    //Aca obtiene la normal del punto usando algun metodo:
                    tuple_3 n1 = new tuple_3(); tuple_3 n2 = new tuple_3(); tuple_3 n3 = new tuple_3();
                    if (mesh.cvn)
                    {
                        n1 = Vectores.Normalize(mesh.vertex_normals[(int)face[0].x]);
                        n2 = Vectores.Normalize(mesh.vertex_normals[(int)face[0].y]);
                        n3 = Vectores.Normalize(mesh.vertex_normals[(int)face[0].z]);
                        normal = Vectores.Normalize(alfa * n1 + beta * n2 + gamma * n3);
                    }
                    else if (mesh.vn_dic.Count == 0)
                        normal = Vectores.Normalize(Vectores.CrossProduct((v2 - v1), (v3 - v1)));
                    else
                    {
                        n1 = mesh.vn_dic[(int)face[2].x];
                        n2 = mesh.vn_dic[(int)face[2].y];
                        n3 = mesh.vn_dic[(int)face[2].z];
                        normal = Vectores.Normalize(alfa * n1 + beta * n2 + gamma * n3);
                    }
                    
                    return true;
                }
            }
            return false;
        }
        else
            return false;
    }


    //Convierte de Angulo a Radianes.
    public static float DegreeToRadian(float angle)
    {
        return (float)Math.PI * angle / 180.0f;
    }


    //No obtiene determinante, sino uno de los elementos para sacarlo.
    public static float dt(float pond, float a, float b, float c, float d) 
    {
        float det_A = pond * (a * d - b * c);
        return det_A;
    }


    //Retorna float aleatorio entre ambos valores.
    public static float next_float(float min, float max)
    {
        float r = (float)rand.NextDouble();
        r *= (max - min);
        r += min;
        return r;
    }
  
  }
}
