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
    }
    
    class Game : GameWindow
    {
        private Vector3[] cameraPos;
        private Vector3 viewVector;
        private Vector3 lampPos;
        private ModelTest modeltest1;
        private Object house;
        private Object lamp;
        private Object moon;
        private Object earth;
        private Vector4 lampColor;
        private  const bool POINT_LIGHT = false;
        private float curColor;
        private float ambientColor;
        private bool isNight;
        private bool mouseFirstMove;
        private Vector2 mouseLastPos;
        private const float cameraSpeed = 1.5f;
        private const float sensitivity = 0.2f;
        private int CUR_CAMERA = 0;

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
            cameraPos = new Vector3[2];
            cameraPos[0] = new Vector3(0, 1, 2);
            cameraPos[1] = new Vector3(0, 0.3f, 2);
            lampPos = new Vector3(0.0f, 100.0f, 0.0f); // 2.0f y
            ambientColor = curColor = 0.8f;
            isNight = false;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        protected override void OnLoad()
        {
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
                -0.5f, 0.0f,  0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, 1.0f,
                 0.5f, 0.0f,  0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, 1.0f,
                 0.0f, 0.5f,  0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, 1.0f,

                -0.5f, 0.0f, -0.5f, 1.0f, 0.0f, 0.0f, 1.0f, -2.0f, 1.0f, 1.0f,
                 0.5f, 0.0f, -0.5f, 1.0f, 0.0f, 0.0f, 1.0f, -2.0f, 1.0f, 1.0f,
                 0.0f, 0.5f,  0.0f, 1.0f, 0.0f, 0.0f, 1.0f, -2.0f, 1.0f, 1.0f,

                -0.5f, 0.0f,  0.5f, 1.0f, 0.0f, 0.0f, 1.0f, -1.0f, 1.0f, 0.0f,
                -0.5f, 0.0f, -0.5f, 1.0f, 0.0f, 0.0f, 1.0f, -1.0f, 1.0f, 0.0f,
                 0.0f, 0.5f,  0.0f, 1.0f, 0.0f, 0.0f, 1.0f, -1.0f, 1.0f, 0.0f,

                 0.5f, 0.0f,  0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f, 0.0f,
                 0.5f, 0.0f, -0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f, 0.0f,
                 0.0f, 0.5f,  0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f, 0.0f
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

            float fluffy = 256.0f;
            float[] vertices3 = {
                    -0.5f, 0.0f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                    -0.5f, 0.0f, 0.5f, 0.0f, 1.0f, 0.0f, 0.0f, fluffy,
                    0.5f, 0.0f, -0.5f, 0.0f, 1.0f, 0.0f, fluffy, 0.0f,
                    0.5f, 0.0f, -0.5f, 0.0f, 1.0f, 0.0f, fluffy, 0.0f,
                    -0.5f, 0.0f, 0.5f, 0.0f, 1.0f, 0.0f, 0.0f, fluffy,
                    0.5f, 0.0f, 0.5f, 0.0f, 1.0f, 0.0f, fluffy, fluffy
                };
            string houseVertexShaderPath = "../../../Shaders/house_shader.vert";
            string houseFragmentShaderPointPath = "../../../Shaders/house_shader_point.frag";
            string houseFragmentShaderDirPath = "../../../Shaders/house_shader_direct.frag";
            string lightVertexShaderPath = "../../../Shaders/light_shader.vert";
            string lightFragmentShaderPath = "../../../Shaders/light_shader.frag";
            int[] attr_size1 = { 3, 4, 3 };
            // XYZ -> COLOR RGBA -> NORMALS
            int[] attr_size2 = { 3 };
            // XYZ
            int[] attr_size3 = { 3, 3, 2 };
            // XYZ -> NORMALS -> TEX
            if (POINT_LIGHT)
            {
                house = new Object(vertices1, houseVertexShaderPath, houseFragmentShaderPointPath, attr_size1);
            }
            else
            {
                house = new Object(vertices1, houseVertexShaderPath, houseFragmentShaderDirPath, attr_size1);
            }
            lamp = new Object(vertices2, lightVertexShaderPath, lightFragmentShaderPath, attr_size2);
            moon = new Object(vertices2, lightVertexShaderPath, lightFragmentShaderPath, attr_size2);
            earth = new Object(vertices3, "../../../Shaders/ground_shader.vert", "../../../Shaders/ground_shader_direct.frag", attr_size3);
            earth.setTexture("../../../Objects/grass.png", TextureUnit.Texture1);
            modeltest1 = new ModelTest();
            base.OnLoad();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            var input = KeyboardState;
            if (input.IsKeyPressed(Keys.Space)) {
                CUR_CAMERA = 1 - CUR_CAMERA;
            }
            if (input.IsKeyDown(Keys.Equal))
            {
                PointManager.Translation(ref cameraPos[0], new Vector3(0.0f, 0.001f, 0.0f));
            }
            if (input.IsKeyDown(Keys.Minus))
            {
                PointManager.Translation(ref cameraPos[0], new Vector3(0.0f, -0.001f, 0.0f));
            }
            if (input.IsKeyDown(Keys.W))
            {
                PointManager.Scale(ref cameraPos[0], 0.999f);
                PointManager.Translation(ref cameraPos[1], Vector3.Normalize(new Vector3(viewVector.X, 0, viewVector.Z)) * cameraSpeed * (float)args.Time);
            }
            if (input.IsKeyDown(Keys.S))
            {
                PointManager.Scale(ref cameraPos[0], 1.001f);
                PointManager.Translation(ref cameraPos[1], Vector3.Normalize(new Vector3(-viewVector.X, 0, -viewVector.Z)) * cameraSpeed * (float)args.Time);
            }
            if (input.IsKeyDown(Keys.A))
            {
                PointManager.RotateY(ref cameraPos[0], 0.1f);
                PointManager.Translation(ref cameraPos[1], Vector3.Normalize(new Vector3(viewVector.Z, 0, -viewVector.X)) * cameraSpeed * (float)args.Time);
            }
            if (input.IsKeyDown(Keys.D))
            {
                PointManager.RotateY(ref cameraPos[0], -0.1f);
                PointManager.Translation(ref cameraPos[1], Vector3.Normalize(new Vector3(-viewVector.Z, 0, viewVector.X)) * cameraSpeed * (float)args.Time);
            }

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


            PointManager.RotateX(ref lampPos, 0.05f);
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
            curColor = Math.Max(Vector3.Dot(Vector3.Normalize(lampPos), new Vector3(0.0f, 1.0f, 0.0f)) * ambientColor, 0.0f);
            if (!isNight)
            {
                GL.ClearColor(0.0f, curColor, curColor, 1.0f);
            }
            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            float k = 25.0f;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 houseModel = Matrix4.CreateTranslation(-k, 0.5f, -k);
            Matrix4 lampModel = Matrix4.CreateScale(1.5f);
            lampModel *= Matrix4.CreateTranslation(isNight ? -lampPos : lampPos);
            Matrix4 moonModel = Matrix4.CreateScale(1.5f);
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
            // our obj1
            for (int j = 0; j < k; j++)
            {
                for (int i = 0; i < k; i++)
                {
                    house.objShader.SetMatrix4("model", houseModel);
                    house.objShader.SetMatrix4("view", view);
                    house.objShader.SetMatrix4("projection", projection);
                    house.objShader.SetVector3("viewPos", cameraPos[CUR_CAMERA]);
                    house.objShader.SetVector3("lightColor", lampColor.Xyz);
                    if (POINT_LIGHT)
                    {
                        house.objShader.SetVector3("lightPos", lampPos);
                    }
                    else
                    {
                        house.objShader.SetVector3("dirLight.direction", -lampPos);
                        house.objShader.SetFloat("dirLight.ambient", curColor + 0.1f);
                        house.objShader.SetFloat("dirLight.diffuse", curColor / ambientColor);
                        house.objShader.SetFloat("dirLight.specular", 0.0f);
                    }
                    //house.Render();
                    houseModel *= Matrix4.CreateTranslation(2.0f, 0.0f, 0.0f);
                }
                houseModel *= Matrix4.CreateTranslation(-2.0f*k, 0.0f, 2.0f);
            }
            // model from file
            foreach (Mesh mesh in modeltest1.model.meshes)
            {
                modeltest1.shader.Use();
                modeltest1.shader.SetMatrix4("model", Matrix4.CreateScale(16.0f));
                modeltest1.shader.SetMatrix4("view", view);
                modeltest1.shader.SetMatrix4("projection", projection);
                modeltest1.shader.SetVector3("lightColor", lampColor.Xyz);
                modeltest1.shader.SetVector3("dirLight.direction", -lampPos);
                modeltest1.shader.SetFloat("dirLight.ambient", curColor + 0.1f);
                modeltest1.shader.SetFloat("dirLight.diffuse", curColor / ambientColor);
                modeltest1.shader.SetFloat("dirLight.specular", 0.0f);
                modeltest1.shader.SetVector3("viewPos", cameraPos[CUR_CAMERA]);
                //modeltest1.texture.Use(TextureUnit.Texture0);
                //modeltest1.shader.SetInt("texture0", 0);
                GL.BindVertexArray(mesh.VAO);
                GL.DrawElements(PrimitiveType.Triangles, mesh.indicesCount, DrawElementsType.UnsignedInt, 0);
            }
            // light source (sun)
            lamp.objShader.SetMatrix4("model", lampModel * Matrix4.CreateTranslation(cameraPos[CUR_CAMERA]));
            lamp.objShader.SetMatrix4("view", view);
            lamp.objShader.SetMatrix4("projection", projection);
            lamp.objShader.SetVector4("lightColor", lampColor);
            lamp.Render();
            // light source (moon)
            moon.objShader.SetMatrix4("model", moonModel * Matrix4.CreateTranslation(cameraPos[CUR_CAMERA]));
            moon.objShader.SetMatrix4("view", view);
            moon.objShader.SetMatrix4("projection", projection);
            moon.objShader.SetVector4("lightColor", lampColor);
            moon.Render();
            // grass
            earth.objShader.SetMatrix4("model", Matrix4.CreateScale(256.0f));
            earth.objShader.SetMatrix4("view", view);
            earth.objShader.SetMatrix4("projection", projection);
            earth.objShader.SetVector3("lightColor", lampColor.Xyz);
            earth.objShader.SetVector3("viewPos", cameraPos[CUR_CAMERA]);
            earth.objShader.SetVector3("dirLight.direction", -lampPos);
            earth.objShader.SetFloat("dirLight.ambient", curColor);
            earth.objShader.SetFloat("dirLight.diffuse", curColor / ambientColor);
            earth.objShader.SetFloat("dirLight.specular", 0.0f);
            earth.objShader.SetInt("texture0", 1);
            earth.Render();
            Context.SwapBuffers();
            base.OnRenderFrame(args);
        }
    }
}
