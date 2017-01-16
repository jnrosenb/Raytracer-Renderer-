using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ejemplo2
{

    public struct tuple_3
    {
        public float x;
        public float y;
        public float z;

        //Override simbolo suma (+).
        public static tuple_3 operator +(tuple_3 A, tuple_3 B)
        {
            return new tuple_3 { x = A.x + B.x, y = A.y + B.y, z = A.z + B.z };
        }

        //Override simbolo resta (-).
        public static tuple_3 operator -(tuple_3 A, tuple_3 B)
        {
            return new tuple_3 { x = A.x - B.x, y = A.y - B.y, z = A.z - B.z };
        }

        //Override simbolo mult (*).
        public static tuple_3 operator *(float a, tuple_3 B)
        {
            return new tuple_3 { x = a * B.x, y = a * B.y, z = a * B.z };
        }

        //Override simbolo mult (*).
        public static tuple_3 operator *(tuple_3 A, tuple_3 B)
        {
            return new tuple_3 { x = A.x * B.x, y = A.y * B.y, z = A.z * B.z };
        }

        //Override simbolo div (/).
        public static tuple_3 operator /(tuple_3 A, float b)
        {
            if (b != 0.0f)
                return new tuple_3 { x = A.x / b, y = A.y / b, z = A.z / b };
            else
                return new tuple_3();
        }
        
        //Override simbolo eq (==).
        public static bool operator ==(tuple_3 A, tuple_3 B)
        {
            return (A.x == B.x && A.y == B.y && A.z == B.z );
        }
        
        //Override simbolo not eq (!=).
        public static bool operator !=(tuple_3 A, tuple_3 B)
        {
            return (A.x != B.x || A.y != B.y || A.z != B.z);
        }
    }

    public struct tuple_4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        //Override simbolo suma (+).
        public static tuple_4 operator +(tuple_4 A, tuple_4 B)
        {
            return new tuple_4 { x = A.x + B.x, y = A.y + B.y, z = A.z + B.z, w = A.w + B.w };
        }

        //Override simbolo resta (-).
        public static tuple_4 operator -(tuple_4 A, tuple_4 B)
        {
            return new tuple_4 { x = A.x - B.x, y = A.y - B.y, z = A.z - B.z, w = A.w - B.w };
        }

        //Override simbolo mult (*).
        public static tuple_4 operator *(float a, tuple_4 B)
        {
            return new tuple_4 { x = a * B.x, y = a * B.y, z = a * B.z, w = a * B.w };
        }

        //Override simbolo mult (*).
        public static tuple_4 operator *(tuple_4 A, tuple_4 B)
        {
            return new tuple_4 { x = A.x * B.x, y = A.y * B.y, z = A.z * B.z, w = A.w * B.w };
        }

        //Override simbolo div (/).
        public static tuple_4 operator /(tuple_4 A, float b)
        {
            if (b != 0.0f)
                return new tuple_4 { x = A.x / b, y = A.y / b, z = A.z / b, w = A.w / b };
            return new tuple_4();
        }
    }

    public struct color_3
    {
        public float R;
        public float G;
        public float B;
    }
     

    public static class Vectores
    {
        //Suma de vectores  (x1, y1, z1) + (x2, y2, z2) = (x1 + x2, y1 + y2, z1 + z2)
        public static tuple_3 Sum(tuple_3 A, tuple_3 B) 
        {
            return new tuple_3{x = A.x + B.x, y = A.y + B.y, z = A.z + B.z};
        }

        //Resta de vectores (x1, y1, z1) - (x2, y2, z2) = (x1 - x2, y1 - y2, z1 - z2) 
        public static tuple_3 Sub(tuple_3 A, tuple_3 B)
        {
            return new tuple_3 { x = A.x - B.x, y = A.y - B.y, z = A.z - B.z };
        }

        //Ponderacion escalar por vector s * (x1, y1, z1) = (s*x1, s*y1, s*z1)
        public static tuple_3 ScalarMult(float s, tuple_3 A)
        {
            return new tuple_3 { x = s * A.x, y = s * A.y, z = s * A.z };
        }

        //Division vector por escalar (x1, y1, z1)/s = (x1/s, y1/s, z1/s)
        public static tuple_3 ScalarDiv(float s, tuple_3 A)
        {
            if (s != 0.0f)
                return new tuple_3 { x = A.x / s, y = A.y / s, z = A.z / s };
            return new tuple_3();
        }


        //Multiplicacion de vectores  (x1, y1, z1) * (x2, y2, z2) = (x1 * x2, y1 * y2, z1 * z1) 
        public static tuple_3 Mult(tuple_3 A, tuple_3 B)
        {
            return new tuple_3 { x = A.x * B.x, y = A.y * B.y, z = A.z * B.z };
        }

        //Magnitud de un vector mag((x1, y1, z1)) = sqrt(x1 * x1 + y1 * y1 + z1 * z1)
        public static float Module(tuple_3 A)
        {
            return (float)Math.Sqrt((float)Math.Pow(A.x, 2) + (float)Math.Pow(A.y, 2) + (float)Math.Pow(A.z, 2));
        }

        //Normalizar un vector norm((x1, y1, z1)) = (x1, y1, z1) / mag((x1, y1, z1))
        public static tuple_3 Normalize(tuple_3 A)
        {
            float magnitude = Module(A);
            if (magnitude == 0)
                return new tuple_3 { x = 0.0f, y = 0.0f, z = 0.0f };
            return new tuple_3 { x = A.x / magnitude, y = A.y / magnitude, z = A.z / magnitude };
        }

        //Producto cruz (x1, y1, z1) x (x2, y2, z2) = (y1*z2 - y2*z1, x2*z1 - x1*z2, x1*y2 - x2*y1)
        public static tuple_3 CrossProduct(tuple_3 A, tuple_3 B)
        { 
            return new tuple_3 { x = A.y * B.z - B.y * A.z, 
                                 y = B.x * A.z - A.x * B.z, 
                                 z = A.x * B.y - B.x * A.y };
        }

        //Producto punto  (x1, y1, z1) . (x2, y2, z2) = x1 * x2 + y1 * y2 + z1 * z2)
        public static float PointProduct(tuple_3 A, tuple_3 B)
        {
            return  (A.x * B.x) + (A.y * B.y) + (A.z * B.z);
        }
        
        //Imprime el vector.
        public static void Print(tuple_3 A) 
        {
            Console.WriteLine("({0}, {1}, {2})", A.x, A.y, A.z);
        }
    }

    public static class Vectores2
    {
        //Suma de vectores  (x1, y1, z1) + (x2, y2, z2) = (x1 + x2, y1 + y2, z1 + z2)
        public static List<float> vectorSum(List<float> A, List<float> B)
        {
            return new List<float> { A[0] + B[0], A[1] + B[1], A[2] + B[2] };
        }

        //Resta de vectores (x1, y1, z1) - (x2, y2, z2) = (x1 - x2, y1 - y2, z1 - z2) 
        public static List<float> vectorSub(List<float> A, List<float> B)
        {
            return new List<float> { A[0] - B[0], A[1] - B[1], A[2] - B[2] };
        }

        //Ponderacion escalar por vector s * (x1, y1, z1) = (s*x1, s*y1, s*z1)
        public static List<float> vectorEscMult(float s, List<float> A)
        {
            return new List<float> { s * A[0], s * A[1], s * A[2] };
        }

        //Division vector por escalar (x1, y1, z1)/s = (x1/s, y1/s, z1/s)
        public static List<float> vectorEscDiv(float s, List<float> A)
        {
            if (s != 0.0f)
                return new List<float> { A[0] / s, A[1] / s, A[2] / s };
            return new List<float>();
        }

        //Multiplicacion de vectores  (x1, y1, z1) * (x2, y2, z2) = (x1 * x2, y1 * y2, z1 * z1) 
        public static List<float> vectorMult(List<float> A, List<float> B)
        {
            return new List<float> { A[0] * B[0], A[1] * B[1], A[2] * B[2] };
        }

        //Magnitud de un vector mag((x1, y1, z1)) = sqrt(x1 * x1 + y1 * y1 + z1 * z1)
        public static float vectorMagnitude(List<float> A)
        {
            return (float)Math.Sqrt(Math.Pow(A[0], 2) + Math.Pow(A[1], 2) + Math.Pow(A[2], 2));
        }

        //Normalizar un vector norm((x1, y1, z1)) = (x1, y1, z1) / mag((x1, y1, z1))
        public static List<float> vectorNormalize(List<float> A)
        {
            float magnitude = vectorMagnitude(A);
            return new List<float> { A[0] / magnitude, A[1] / magnitude, A[2] / magnitude };
        }

        //Producto punto  (x1, y1, z1) . (x2, y2, z2) = x1 * x2 + y1 * y2 + z1 * z2) 
        public static float vectorPointProduct(List<float> A, List<float> B)
        {
            return (A[0] * B[0]) + (A[1] * B[1]) + (A[2] * B[2]);
        }

        //Producto cruz (x1, y1, z1) x (x2, y2, z2) = (y1*z2 - y2*z1, x2*z1 - x1*z2, x1*y2 - x2*y1)
        public static List<float>  vectorCrossProduct(List<float> A, List<float> B)
        {
            return new List<float> { A[1] * B[2] - B[1] * A[2], B[0] * A[2] - A[0] * B[2], A[0] * B[1] - B[0] * A[1] };
        }

        //Imprime el vector.
        public static void vectorPrint(List<float> A)
        {
            Console.WriteLine("({0}, {1}, {2})", A[0], A[1], A[2]);
        }
    }
}
