using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ejemplo2;

namespace Ejemplo2
{
    class Camera
    {

        public float FOV { get; set; }
        public float near { get; set; }
        public float lensSize { get; set; }
        public tuple_3 position { get; set; }
        public tuple_3 up { get; set; }
        public tuple_3 target { get; set; }

        public Camera(float fov, tuple_3 position, tuple_3 up, tuple_3 target, float near, float lensSize) 
        {
            this.FOV = fov;
            this.position = position;
            this.up = up;
            this.target = target;

            this.near = near;
            this.lensSize = lensSize;
        }

    }
}
