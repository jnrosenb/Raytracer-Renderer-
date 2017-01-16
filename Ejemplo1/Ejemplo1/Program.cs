using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ejemplo2;
using raytracer;

namespace Ejemplo2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Ingresar path de archivo con extension .json (debe estar en la carpeta json): ");
            string path = Console.ReadLine();
            Scene.LoadScene("..\\..\\Json\\" + path);   
        }
    }
}
