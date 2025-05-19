using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Externals;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace My_First_OPENGL_application
{
    class ModelTest
    {
        public Model model;
        public Shader shader;
        public Texture texture;
        public ModelTest(string pathToObj, string pathToVert, string pathToFrag, string pathToTexture = "../../../Objects/container.png")
        {
            model = new Model(pathToObj);
            shader = new Shader(pathToVert, pathToFrag);
            texture = Texture.LoadFromFile(pathToTexture);
        }
    }
}
