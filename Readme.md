
#Renderer por simulacion (Raytracer)

**Programa al que se le entrega el nombre de un archivo json que contiene detalles de una escena, y retorna una imagen con el render de tal escena. A continuacion instrucciones de datos a entregar:**

Input para el programa: 
	
	int Width (se puede introducir un entero positivo cualquiera. Mientras mas grande, mas se demorara el rendering)
	int Height (se puede introducir un entero positivo cualquiera. Mientras mas grande, mas se demorara el rendering)
	string nombre_imagen_output (Aqui se debe introducir un nombre para la imagen, como por ejemplo: img1.json)

Los archivos json que se deseen cargar deben estar en carpeta **\Ejemplo1\Ejemplo1\Json\**

Al final, el programa pide un nombre (el string que aparece arriba, **nombre_imagen_final**) y se lo asigna a la imagen resultante. Esta se encuentra en la carpeta: **Ejemplo1\Ejemplo1\bin\Debug\**

Acciones que soporta:

	1- Materials Lambert / blinn-Phong / Reflective	
	2- Spheres and Meshes(.obj)
	3- Reflections, Shadows
	4- Point Light, Ambient Light
	5- Antialiasing, soft shadow, Depth of field
	
