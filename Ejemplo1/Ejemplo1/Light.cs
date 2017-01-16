using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ejemplo2;

namespace Ejemplo2
{

    public class Light 
    {
        public string name;
    }

    public class PointLight : Light
    {
        public float lightSize;
        public List<float> lightNormal;
        public tuple_3 position;
        public List<float> color;

        public PointLight(tuple_3 position, List<float> color, float lightSize, List<float> lightNormal) 
        {
            this.position = position;
            this.color = color;
            this.name = "point_light";

            this.lightSize = lightSize;
            this.lightNormal = lightNormal;
        }
    }

    public class AmbientLight : Light 
    {
        public List<float> color;
        public AmbientLight(List<float> amb_color)
        {
            this.name = "ambient_light";
            this.color = amb_color;
        }
    }
}
