using System;
using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Generic;
using Externals;

namespace My_First_OPENGL_application
{
    class Shadow
    {
        public readonly int depthMapHandle;
        public readonly int depthMapTextureHandle;
        public readonly int SHADOW_WIDTH = 8192;
        public readonly int SHADOW_HEIGHT = 8192;
        public Shadow()
        {
            // Init shadow texture, framebuffer
            depthMapHandle = GL.GenFramebuffer();
            depthMapTextureHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, depthMapTextureHandle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, SHADOW_WIDTH, SHADOW_HEIGHT, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapHandle);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthMapTextureHandle, 0);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    }
    class PointManager
    {
        public static void RotateX(ref Vector3 point, float angle)
        {
            Vector4 point4 = new Vector4(point, 1.0f);
            point4 *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(angle));
            point = point4.Xyz;
        }
        public static void RotateY(ref Vector3 point, float angle)
        {
            Vector4 point4 = new Vector4(point, 1.0f);
            point4 *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(angle));
            point = point4.Xyz;
        }
        public static void RotateZ(ref Vector3 point, float angle)
        {
            Vector4 point4 = new Vector4(point, 1.0f);
            point4 *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(angle));
            point = point4.Xyz;
        }
        public static void RotateAxis(Vector3 axis, ref Vector3 point, float angle)
        {
            Vector4 point4 = new Vector4(point, 1.0f);
            point4 *= Matrix4.CreateFromAxisAngle(axis, angle);
            point = point4.Xyz;
        }
        public static void Translation(ref Vector3 point, Vector3 offset)
        {
            point += offset;
        }
        public static void Scale(ref Vector3 point, float factor)
        {
            point *= factor;
        }
    }

    class Object
    {
        public readonly int vertexBufferHandle;
        public readonly int vertexArrayHandle;
        public readonly int elementBufferHandle;
        public TextureUnit objUnit;
        public Texture objTexture;
        public Shader objShader;
        private int vertexCount;
        private bool EBO;
        private bool textureLoaded;

        public void setTexture(string path, TextureUnit unit)
        {
            objTexture = Texture.LoadFromFile(path);
            textureLoaded = true;
            objUnit = unit;
        }
        public Object(float[] vertices, string vertPath, string fragPath, int[] vao_attrib_sizes)
        {
            textureLoaded = false;
            objShader = new Shader(vertPath, fragPath);
            // VBO
            vertexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            // VAO
            vertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayHandle);
            // VAO structure
            int curOffset = 0;
            int vertexSize = 0;
            for (int i = 0; i < vao_attrib_sizes.Length; i++)
            {
                vertexSize += vao_attrib_sizes[i];
            }
            for (int i = 0; i < vao_attrib_sizes.Length; i++)
            {
                GL.VertexAttribPointer(i, vao_attrib_sizes[i], VertexAttribPointerType.Float, false, vertexSize * sizeof(float), curOffset);
                GL.EnableVertexAttribArray(i);
                curOffset += vao_attrib_sizes[i] * sizeof(float);
            }
            if (vertexSize == 0)
            {
                throw new Exception("Vertices size is 0");
            }
            if (vertices.Length % vertexSize != 0)
            {
                throw new Exception("Vertex Size is incorrect");
            }
            vertexCount = vertices.Length / vertexSize;
            EBO = false;
        }

        public Object(float[] vertices, uint[] indices, string vertPath, string fragPath, int[] vao_attrib_sizes)
        {
            textureLoaded = false;
            objShader = new Shader(vertPath, fragPath);
            // VBO
            vertexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            // EBO
            elementBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
            // VAO
            vertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayHandle);
            // VAO structure
            int curOffset = 0;
            int vertexSize = 0;
            for (int i = 0; i < vao_attrib_sizes.Length; i++)
            {
                vertexSize += vao_attrib_sizes[i];
            }
            for (int i = 0; i < vao_attrib_sizes.Length; i++)
            {
                GL.VertexAttribPointer(i, vao_attrib_sizes[i], VertexAttribPointerType.Float, false, vertexSize * sizeof(float), curOffset);
                GL.EnableVertexAttribArray(i);
                curOffset += vao_attrib_sizes[i] * sizeof(float);
            }
            if (vertexSize == 0)
            {
                throw new Exception("Vertices size is 0");
            }
            vertexCount = indices.Length;
            EBO = true;
        }

        public void Render()
        {
            GL.BindVertexArray(vertexArrayHandle);
            objShader.Use();
            if (textureLoaded)
            {
                objTexture.Use(objUnit);
            }
            if (!EBO)
            {
                GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);
            }
            else
            {
                GL.DrawElements(PrimitiveType.Triangles, vertexCount, DrawElementsType.UnsignedInt, 0);
            }
        }

        public void Render(ref Shader shader)
        {
            GL.BindVertexArray(vertexArrayHandle);
            if (shader == null)
            {
                objShader.Use();
            }
            else
            {
                // hard coded model uniform assignment
                float[] temp = new float[16];
                GL.GetUniform(objShader.Handle, GL.GetUniformLocation(objShader.Handle, "model"), temp);
                shader.SetMatrix4("model", temp);
                //
                shader.Use();
            }
            if (textureLoaded)
            {
                objTexture.Use(objUnit);
            }
            if (!EBO)
            {
                GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);
            }
            else
            {
                GL.DrawElements(PrimitiveType.Triangles, vertexCount, DrawElementsType.UnsignedInt, 0);
            }
        }
    }
    
    class Game : GameWindow
    {
        private Shadow shadow;
        private Shader shadowShader;
        private Matrix4 lightSpaceMatrix;
        private Vector2 mouseLastPos;
        private Vector3[] cameraPos;
        private Vector3[] grass1info;
        private Vector3 viewVector;
        private Vector3 lampPos;
        private Vector4 lampColor;
        private ModelTest modeltest1;
        private ModelTest lamp1;
        private ModelTest moon1;
        private ModelTest grass1;
        private Object house;
        private Object earth;
        private int CUR_CAMERA;
        private float curColor;
        private float ambientColor;
        private bool isNight;
        private bool mouseFirstMove;
        private const int grassCount = 8192;
        private const float cameraSpeed = 1.5f;
        private const float sensitivity = 0.2f;
        private const float specular_all = 0.5f;
        private const bool POINT_LIGHT = false;

        public Game(string title = "OPENGL APP", int width = 1280, int height = 720) : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            Title = title,
            Size = new Vector2i(width, height),
            WindowBorder = WindowBorder.Resizable,
            StartVisible = false,
            StartFocused = true,
            API = ContextAPI.OpenGL,
            Profile = ContextProfile.Core,
            APIVersion = new Version(3, 3)
        })
        {
            this.CenterWindow();
            // init essential values
            cameraPos = new Vector3[2];
            grass1info = new Vector3[grassCount];
            cameraPos[0] = new Vector3(0, 1, 2);
            cameraPos[1] = new Vector3(0, 0.3f, 2);
            if (POINT_LIGHT)
            {
                lampPos = new Vector3(0.0f, 5.0f, 0.0f);
            }
            else
            {
                lampPos = new Vector3(0.0f, 100.0f, 0.0f);
            }
            ambientColor = curColor = 0.8f;
            isNight = false;
            CUR_CAMERA = 1;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        protected override void OnLoad()
        {
            Random rand = new Random(DateTime.UtcNow.Millisecond);
            double range0 = -20;
            double range1 = 20;
            for (int i = 0; i < grassCount; i++)
            {
                grass1info[i].X = (float)(rand.NextDouble() * (range1 - range0) - (range1 - range0) / 2);
                grass1info[i].Y = (float)(rand.NextDouble() * (range1 - range0) - (range1 - range0) / 2);
                grass1info[i].Z = (float)(rand.NextDouble() * 360);
            }
            this.CursorState = CursorState.Grabbed;
            this.IsVisible = true;
            GL.ClearColor(0.0f, curColor, curColor, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            viewVector = Vector3.Normalize(new Vector3(-cameraPos[CUR_CAMERA]));
            lampColor = new Vector4(1.0f, 1.0f, 0.7f, 1.0f);

            float[] vertices1 = {
                // Основание домика (куб)
                // Передняя грань
                -0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f,
                 0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f,
                 0.5f,  0.0f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f,
                -0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f,
                 0.5f,  0.0f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f,
                -0.5f,  0.0f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f,

                // Задняя грань
                -0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, -1.0f,
                 0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, -1.0f,
                 0.5f,  0.0f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, -1.0f,
                -0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, -1.0f,
                 0.5f,  0.0f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, -1.0f,
                -0.5f,  0.0f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, -1.0f,

                // Верхняя грань
                -0.5f,  0.0f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f, 0.0f,
                 0.5f,  0.0f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f, 0.0f,
                 0.5f,  0.0f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f, 0.0f,
                -0.5f,  0.0f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f, 0.0f,
                 0.5f,  0.0f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f, 0.0f,
                -0.5f,  0.0f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f, 0.0f,

                // Нижняя грань
                -0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, -1.0f, 0.0f,
                 0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, -1.0f, 0.0f,
                 0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, -1.0f, 0.0f,
                -0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, -1.0f, 0.0f,
                 0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, -1.0f, 0.0f,
                -0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, -1.0f, 0.0f,

                // Левая грань
                -0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, -1.0f, 0.0f, 0.0f,
                -0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, -1.0f, 0.0f, 0.0f,
                -0.5f,  0.0f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, -1.0f, 0.0f, 0.0f,
                -0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, -1.0f, 0.0f, 0.0f,
                -0.5f,  0.0f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, -1.0f, 0.0f, 0.0f,
                -0.5f,  0.0f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, -1.0f, 0.0f, 0.0f,

                // Правая грань
                 0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f,
                 0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f,
                 0.5f,  0.0f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f,
                 0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f,
                 0.5f,  0.0f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f,
                 0.5f,  0.0f,  0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f,
                 // Крыша
                -0.5f, 0.0f,  0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, -1.0f, -1.0f,
                 0.5f, 0.0f,  0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, -1.0f, -1.0f,
                 0.0f, 0.5f,  0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, -1.0f, -1.0f,

                -0.5f, 0.0f, -0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 2.0f, -1.0f, -1.0f,
                 0.5f, 0.0f, -0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 2.0f, -1.0f, -1.0f,
                 0.0f, 0.5f,  0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 2.0f, -1.0f, -1.0f,

                -0.5f, 0.0f,  0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f, -1.0f, 0.0f,
                -0.5f, 0.0f, -0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f, -1.0f, 0.0f,
                 0.0f, 0.5f,  0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f, -1.0f, 0.0f,

                 0.5f, 0.0f,  0.5f, 1.0f, 0.0f, 0.0f, 1.0f, -1.0f, -1.0f, 0.0f,
                 0.5f, 0.0f, -0.5f, 1.0f, 0.0f, 0.0f, 1.0f, -1.0f, -1.0f, 0.0f,
                 0.0f, 0.5f,  0.0f, 1.0f, 0.0f, 0.0f, 1.0f, -1.0f, -1.0f, 0.0f
            };

            float[] vertices2 = {
                -0.5f, -0.5f, -0.5f,
                 0.5f, -0.5f, -0.5f,
                 0.5f,  0.5f, -0.5f,
                 0.5f,  0.5f, -0.5f,
                -0.5f,  0.5f, -0.5f,
                -0.5f, -0.5f, -0.5f,

                -0.5f, -0.5f,  0.5f,
                 0.5f, -0.5f,  0.5f,
                 0.5f,  0.5f,  0.5f,
                 0.5f,  0.5f,  0.5f,
                -0.5f,  0.5f,  0.5f,
                -0.5f, -0.5f,  0.5f,

                -0.5f,  0.5f,  0.5f,
                -0.5f,  0.5f, -0.5f,
                -0.5f, -0.5f, -0.5f,
                -0.5f, -0.5f, -0.5f,
                -0.5f, -0.5f,  0.5f,
                -0.5f,  0.5f,  0.5f,

                 0.5f,  0.5f,  0.5f,
                 0.5f,  0.5f, -0.5f,
                 0.5f, -0.5f, -0.5f,
                 0.5f, -0.5f, -0.5f,
                 0.5f, -0.5f,  0.5f,
                 0.5f,  0.5f,  0.5f,

                -0.5f, -0.5f, -0.5f,
                 0.5f, -0.5f, -0.5f,
                 0.5f, -0.5f,  0.5f,
                 0.5f, -0.5f,  0.5f,
                -0.5f, -0.5f,  0.5f,
                -0.5f, -0.5f, -0.5f,

                -0.5f,  0.5f, -0.5f,
                 0.5f,  0.5f, -0.5f,
                 0.5f,  0.5f,  0.5f,
                 0.5f,  0.5f,  0.5f,
                -0.5f,  0.5f,  0.5f,
                -0.5f,  0.5f, -0.5f
            };

            float grassScale = 256.0f;
            float[] vertices3 = {
                    -0.5f, 0.0f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                    -0.5f, 0.0f, 0.5f, 0.0f, 1.0f, 0.0f, 0.0f, grassScale,
                    0.5f, 0.0f, -0.5f, 0.0f, 1.0f, 0.0f, grassScale, 0.0f,
                    0.5f, 0.0f, -0.5f, 0.0f, 1.0f, 0.0f, grassScale, 0.0f,
                    -0.5f, 0.0f, 0.5f, 0.0f, 1.0f, 0.0f, 0.0f, grassScale,
                    0.5f, 0.0f, 0.5f, 0.0f, 1.0f, 0.0f, grassScale, grassScale
                };
            string houseVertexShaderPath = "../../../Shaders/house_shader.vert";
            string houseFragmentShaderPointPath = "../../../Shaders/house_shader_point.frag";
            string houseFragmentShaderDirPath = "../../../Shaders/house_shader_direct.frag";
            string lightVertexShaderPath = "../../../Shaders/light_shader.vert";
            string lightFragmentShaderPath = "../../../Shaders/light_shader.frag";
            string house1VertexShaderPath = "../../../Shaders/house_shader1.vert";
            string house1FragmentShaderPointPath = "../../../Shaders/house_shader_point1.frag";
            string house1FragmentShaderDirPath = "../../../Shaders/house_shader_direct1.frag";
            string groundVertexShaderPath = "../../../Shaders/ground_shader.vert";
            string groundFragmentShaderPointPath = "../../../Shaders/ground_shader_point.frag";
            string groundFragmentShaderDirPath = "../../../Shaders/ground_shader_direct.frag";
            int[] attr_size1 = { 3, 4, 3 };
            // XYZ -> COLOR RGBA -> NORMALS
            int[] attr_size2 = { 3 };
            // XYZ
            int[] attr_size3 = { 3, 3, 2 };
            // XYZ -> NORMALS -> TEX
            if (POINT_LIGHT)
            {
                house = new Object(vertices1, houseVertexShaderPath, houseFragmentShaderPointPath, attr_size1);
                earth = new Object(vertices3, groundVertexShaderPath, groundFragmentShaderPointPath, attr_size3);
                modeltest1 = new ModelTest("../../../Objects/GraceField_House.obj", house1VertexShaderPath, house1FragmentShaderPointPath);
                grass1 = new ModelTest("../../../Objects/highqualitygrass.obj", house1VertexShaderPath, house1FragmentShaderPointPath);
            }
            else
            {
                house = new Object(vertices1, houseVertexShaderPath, houseFragmentShaderDirPath, attr_size1);
                earth = new Object(vertices3, groundVertexShaderPath, groundFragmentShaderDirPath, attr_size3);
                modeltest1 = new ModelTest("../../../Objects/GraceField_House.obj", house1VertexShaderPath, house1FragmentShaderDirPath);
                grass1 = new ModelTest("../../../Objects/highqualitygrass.obj", house1VertexShaderPath, house1FragmentShaderDirPath);
            }
            //lamp = new Object(vertices2, lightVertexShaderPath, lightFragmentShaderPath, attr_size2);
            lamp1 = new ModelTest("../../../Objects/sun.obj", lightVertexShaderPath, lightFragmentShaderPath);
            //moon = new Object(vertices2, lightVertexShaderPath, lightFragmentShaderPath, attr_size2);
            moon1 = new ModelTest("../../../Objects/sun.obj", lightVertexShaderPath, lightFragmentShaderPath);
            earth.setTexture("../../../Objects/grass.png", TextureUnit.Texture0);
            shadow = new Shadow();
            shadowShader = new Shader("../../../Shaders/shadow_simple_shader.vert", "../../../Shaders/shadow_simple_shader.frag");
            base.OnLoad();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            KeyDetection(ref args);

            MouseDetection();

            // Move the sun and the moon by 3 degrees / sec.
            PointManager.RotateX(ref lampPos, 0.05f * (float)args.Time * 60);
            // sun&moon behaviour
            if (lampPos.Y < 0.0f)
            {
                lampPos = -lampPos;
                isNight = !isNight;
                if (isNight) {
                    lampColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    ambientColor = 0.2f;
                }
                else
                {
                    lampColor = new Vector4(1.0f, 1.0f, 0.7f, 1.0f);
                    ambientColor = 0.8f;
                }
            }
            // simulate dark effect during night
            curColor = Math.Max(Vector3.Dot(Vector3.Normalize(lampPos), new Vector3(0.0f, 1.0f, 0.0f)) * ambientColor, 0.0f);
            if (!isNight)
            {
                GL.ClearColor(0.0f, curColor, curColor, 1.0f);
            }
            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // setup view from light's perspective
            //Matrix4 lightProjection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), (float)shadow.SHADOW_WIDTH / shadow.SHADOW_HEIGHT, 0.1f, 200.0f);
            Matrix4 lightProjection = Matrix4.CreateOrthographic(20.0f, 20.0f, 0.1f, 200.0f);
            Matrix4 lightView = Matrix4.LookAt(lampPos, Vector3.Zero, Vector3.UnitY);
            lightSpaceMatrix = lightView * lightProjection;
            shadowShader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
            GL.Viewport(0, 0, shadow.SHADOW_WIDTH, shadow.SHADOW_HEIGHT);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadow.depthMapHandle);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            // render from light's perspective
            GL.CullFace(TriangleFace.Front);
            RenderObjects(shadowShader);
            // back to our perspective
            GL.CullFace(TriangleFace.Back);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, shadow.depthMapTextureHandle);
            RenderObjects();
            Context.SwapBuffers();
            base.OnRenderFrame(args);
        }

        private void RenderObjects(Shader shader = null)
        {
            Matrix4 lampModel = Matrix4.CreateScale(POINT_LIGHT ? 256f : 1152f);
            lampModel *= Matrix4.CreateTranslation(isNight ? -lampPos : lampPos);
            Matrix4 moonModel = Matrix4.CreateScale(POINT_LIGHT ? 256f : 1152f);
            moonModel *= Matrix4.CreateTranslation(isNight ? lampPos : -lampPos);
            Matrix4 view;
            if (CUR_CAMERA == 1)
            {
                view = Matrix4.LookAt(cameraPos[CUR_CAMERA], cameraPos[CUR_CAMERA] + viewVector, Vector3.UnitY);
            }
            else
            {
                view = Matrix4.LookAt(cameraPos[CUR_CAMERA], Vector3.Zero, Vector3.UnitY);
            }
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), (float)ClientSize.X / ClientSize.Y, 0.1f, 100.0f);
            // our houses
            //RenderHouses(ref view, ref projection, shader);
            // model from file
            if (shader != null)
            {
                shader.Use();
            }
            else
            {
                modeltest1.shader.Use();
            }
            foreach (Mesh mesh in modeltest1.model.meshes)
            {
                modeltest1.shader.SetMatrix4("model", Matrix4.CreateScale(32.0f));
                modeltest1.shader.SetMatrix4("view", view);
                modeltest1.shader.SetMatrix4("projection", projection);
                modeltest1.shader.SetVector3("light.color", lampColor.Xyz);
                modeltest1.shader.SetVector3("viewPos", cameraPos[CUR_CAMERA]);
                modeltest1.shader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
                modeltest1.shader.SetFloat("light.ambient", curColor);
                modeltest1.shader.SetFloat("light.diffuse", curColor / ambientColor);
                modeltest1.shader.SetFloat("light.specular", specular_all);
                modeltest1.shader.SetInt("shadowMap", 1);
                if (POINT_LIGHT)
                {
                    modeltest1.shader.SetVector3("light.pos", lampPos);
                }
                else
                {
                    modeltest1.shader.SetVector3("light.direction", -lampPos);
                }
                //modeltest1.texture.Use(TextureUnit.Texture0);
                //modeltest1.shader.SetInt("texture0", 0);
                modeltest1.Render(ref shader, mesh);
            }
            grass1.shader.SetMatrix4("view", view);
            grass1.shader.SetMatrix4("projection", projection);
            grass1.shader.SetVector3("light.color", lampColor.Xyz);
            grass1.shader.SetVector3("viewPos", cameraPos[CUR_CAMERA]);
            grass1.shader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
            grass1.shader.SetFloat("light.ambient", curColor);
            grass1.shader.SetFloat("light.diffuse", curColor / ambientColor);
            grass1.shader.SetFloat("light.specular", specular_all);
            grass1.shader.SetInt("shadowMap", 1);
            if (POINT_LIGHT)
            {
                grass1.shader.SetVector3("light.pos", lampPos);
            }
            else
            {
                grass1.shader.SetVector3("light.direction", -lampPos);
            }
            Matrix4 curGrassModel;
            if (shader != null)
            {
                shader.Use();
            }
            else
            {
                grass1.shader.Use();
            }
            for (int i = 0; i < grassCount; i++)
            {
                curGrassModel = Matrix4.CreateRotationY(grass1info[i].Z) * Matrix4.CreateScale(256.0f) * Matrix4.CreateTranslation(grass1info[i].X, 0.0f, grass1info[i].Y);
                foreach (Mesh mesh in grass1.model.meshes)
                {
                    grass1.shader.SetMatrix4("model", curGrassModel);
                    grass1.Render(ref shader, mesh);
                }
            }
            // light source (sun)
            foreach (Mesh mesh in lamp1.model.meshes) {
                if (POINT_LIGHT)
                {
                    lamp1.shader.SetMatrix4("model", lampModel);
                }
                else
                {
                    lamp1.shader.SetMatrix4("model", lampModel * Matrix4.CreateTranslation(cameraPos[CUR_CAMERA]));
                }
                lamp1.shader.SetMatrix4("view", view);
                lamp1.shader.SetMatrix4("projection", projection);
                lamp1.shader.SetVector4("lightColor", lampColor);
                if (shader == null)
                {
                    lamp1.Render(mesh);
                }
            }
            // light source (moon)
            foreach (Mesh mesh in lamp1.model.meshes)
            {
                if (POINT_LIGHT)
                {
                    moon1.shader.SetMatrix4("model", moonModel);
                }
                else
                {
                    moon1.shader.SetMatrix4("model", moonModel * Matrix4.CreateTranslation(cameraPos[CUR_CAMERA]));
                }
                moon1.shader.SetMatrix4("view", view);
                moon1.shader.SetMatrix4("projection", projection);
                moon1.shader.SetVector4("lightColor", lampColor);
                if (shader == null)
                {
                    moon1.Render(mesh);
                }
            }
            // grass
            earth.objShader.SetMatrix4("model", Matrix4.CreateScale(256.0f) * Matrix4.CreateTranslation(0.0f, -1e-4f, 0.0f));
            earth.objShader.SetMatrix4("view", view);
            earth.objShader.SetMatrix4("projection", projection);
            earth.objShader.SetVector3("viewPos", cameraPos[CUR_CAMERA]);
            earth.objShader.SetVector3("light.color", lampColor.Xyz);
            earth.objShader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
            earth.objShader.SetInt("shadowMap", 1);
            earth.objShader.SetFloat("light.ambient", curColor);
            earth.objShader.SetFloat("light.diffuse", curColor / ambientColor);
            earth.objShader.SetFloat("light.specular", 0.0f);
            earth.objShader.SetInt("texture0", 0);
            if (POINT_LIGHT)
            {
                earth.objShader.SetVector3("light.pos", lampPos);
            }
            else
            {
                earth.objShader.SetVector3("light.direction", -lampPos);
            }
            earth.Render(ref shader);
        }

        private void RenderHouses(ref Matrix4 view, ref Matrix4 projection, Shader shader)
        {
            float k = 25.0f;
            Matrix4 houseModel = Matrix4.CreateTranslation(-k, 0.5f, -k);
            house.objShader.SetMatrix4("view", view);
            house.objShader.SetMatrix4("projection", projection);
            house.objShader.SetVector3("viewPos", cameraPos[CUR_CAMERA]);
            house.objShader.SetVector3("light.color", lampColor.Xyz);
            house.objShader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
            house.objShader.SetInt("shadowMap", 1);
            house.objShader.SetFloat("light.ambient", curColor + 0.1f);
            house.objShader.SetFloat("light.diffuse", curColor / ambientColor);
            house.objShader.SetFloat("light.specular", specular_all);
            if (POINT_LIGHT)
            {
                house.objShader.SetVector3("light.pos", lampPos);
            }
            else
            {
                house.objShader.SetVector3("light.direction", -lampPos);
            }
            for (int j = 0; j < k; j++)
            {
                for (int i = 0; i < k; i++)
                {
                    house.objShader.SetMatrix4("model", houseModel);
                    house.Render(ref shader);
                    houseModel *= Matrix4.CreateTranslation(5.0f, 0.0f, 0.0f);
                }
                houseModel *= Matrix4.CreateTranslation(-5.0f * k, 0.0f, 5.0f);
            }
        }

        private void KeyDetection(ref FrameEventArgs args)
        {
            var input = KeyboardState;
            if (input.IsKeyPressed(Keys.Space))
            {
                CUR_CAMERA = 1 - CUR_CAMERA;
            }
            if (input.IsKeyDown(Keys.Equal))
            {
                PointManager.Translation(ref cameraPos[0], new Vector3(0.0f, (float)args.Time, 0.0f));
            }
            if (input.IsKeyDown(Keys.Minus))
            {
                PointManager.Translation(ref cameraPos[0], new Vector3(0.0f, -(float)args.Time, 0.0f));
            }
            if (input.IsKeyDown(Keys.W))
            {
                PointManager.Scale(ref cameraPos[0], 1.0f - (float)args.Time);
                PointManager.Translation(ref cameraPos[1], Vector3.Normalize(new Vector3(viewVector.X, 0, viewVector.Z)) * cameraSpeed * (float)args.Time);
            }
            if (input.IsKeyDown(Keys.S))
            {
                PointManager.Scale(ref cameraPos[0], 1.0f + (float)args.Time);
                PointManager.Translation(ref cameraPos[1], Vector3.Normalize(new Vector3(-viewVector.X, 0, -viewVector.Z)) * cameraSpeed * (float)args.Time);
            }
            if (input.IsKeyDown(Keys.A))
            {
                PointManager.RotateY(ref cameraPos[0], 60 * (float)args.Time);
                PointManager.Translation(ref cameraPos[1], Vector3.Normalize(new Vector3(viewVector.Z, 0, -viewVector.X)) * cameraSpeed * (float)args.Time);
            }
            if (input.IsKeyDown(Keys.D))
            {
                PointManager.RotateY(ref cameraPos[0], -60 * (float)args.Time);
                PointManager.Translation(ref cameraPos[1], Vector3.Normalize(new Vector3(-viewVector.Z, 0, viewVector.X)) * cameraSpeed * (float)args.Time);
            }
        }

        private void MouseDetection()
        {
            var mouse = MouseState;

            if (mouseFirstMove) // This bool variable is initially set to true.
            {
                mouseLastPos = new Vector2(mouse.X, mouse.Y);
                mouseFirstMove = false;
            }
            else
            {
                // Calculate the offset of the mouse position
                var deltaX = mouse.X - mouseLastPos.X;
                var deltaY = mouse.Y - mouseLastPos.Y;
                mouseLastPos = new Vector2(mouse.X, mouse.Y);
                PointManager.RotateY(ref viewVector, -deltaX * sensitivity);
                if (!((deltaY > 0 && -1.0f + 1e-2 > viewVector.Y && viewVector.Y > -1.0f) || (deltaY < 0 && 1.0f - 1e-2 < viewVector.Y && viewVector.Y < 1.0f)))
                {
                    PointManager.RotateAxis(Vector3.Normalize(new Vector3(-viewVector.Z, 0.0f, viewVector.X)), ref viewVector, -deltaY * sensitivity / 100);
                }
            }
        }
    }


}
