using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ejemplo2
{
    public class Material
    {
        public string name { get; set; }
        public List<float> color { get; set; }
        public string material_type { get; set; }
    }

    public class Material_brdf: Material
    {
        public bool use_for_ambient{ get; set; }
        public Dictionary<string, object> brdfParams{ get; set; }

        public Material_brdf(string name, List<float> color, string type, Dictionary<string, object> param, bool ambient = false) 
        {
            this.name = name;
            this.color = color;
            this.material_type = type;
            this.use_for_ambient = ambient;
            this.brdfParams = param;
        }
    }

    public class Material_reflective : Material
    {
        public Material_reflective(string name, List<float> color, string type)
        {
            this.name = name;
            this.color = color;
            this.material_type = type;
        }
    }

    public class Material_dielectric : Material
    {
        public List<float> attenuation { get; set; }
        public float ref_index { get; set; }

        public Material_dielectric(string name, List<float> color, string type, List<float> attenuation, float ref_index)
        {
            this.name = name;
            this.color = color;
            this.material_type = type;
            this.attenuation = attenuation;
            this.ref_index = ref_index;
        }
    }
}
