using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace Metaballs3D
{
    class Game : GameWindow
    {
        [STAThread]
        static void Main()
        {
            Game game = new Game();
            game.Run();
        }

        static int window_width = 800;
        static int window_height = 800;

        public Game() : base(window_width, window_height, GraphicsMode.Default, "metaballs")
        {
            VSync = VSyncMode.On;
        }

        protected override void OnResize(EventArgs E)
        {
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
        }

        int render_shader, compute_shader;
        int VAO, VBO;
        int metaballs_ssbo;

        protected override void OnLoad(EventArgs E)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            #region compile shaders
            render_shader = CompileShaders.Compile(
                new System.IO.StreamReader("frag_shader.glsl"), 
                new System.IO.StreamReader("vert_shader.glsl"), 
                new System.IO.StreamReader("geom_shader.glsl"));

            compute_shader = CompileShaders.CompileComputeShader(new System.IO.StreamReader("comp_shader.glsl"));
            #endregion

            #region uniforms in compute shader
            GL.UseProgram(compute_shader);
            GL.Uniform1(GL.GetUniformLocation(compute_shader, "threshold"), threshold);

            GL.Uniform1(GL.GetUniformLocation(compute_shader, "MarchingCubesStep"), MarchingCubesStep); 
            GL.Uniform3(GL.GetUniformLocation(compute_shader, "MarchingCubesMin"), MarchingCubesMin);

            GL.Uniform1(GL.GetUniformLocation(compute_shader, "default_vert_value"), DEFAULT_VERT_VALUE);
            #endregion
            
            int CubesSSBO = GL.GenBuffer();
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, CubesSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, Cubes.Table.Length * sizeof(int), Cubes.Table, BufferUsageHint.StaticDraw);
            
            #region create VAO & VBO
            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float), new float[] { 0 }, BufferUsageHint.StaticDraw);

                GL.VertexAttribPointer(0, 1, VertexAttribPointerType.Float, false, 1 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
            #endregion
            
            #region generate metaballs and put them into SSBO
            var Metaballs = new Metaball[metaball_count];

            Vector3[] possible_colors = new Vector3[]
            {
                new Vector3(0, 1, 0),
                new Vector3(1, 1, 0),
                new Vector3(1, 0, 0),
                new Vector3(0, 0, 1)
            };

            for (int i = 0; i < metaball_count; i++)
            {
                Vector3 pos = new Vector3(rand.Next(-100, 100) / 100f, rand.Next(-100, 100) / 100f, rand.Next(-100, 100) / 100f);
                Vector3 color = possible_colors[rand.Next(possible_colors.Length)];
                float charge = 1;

                Metaballs[i] = new Metaball()
                {
                    position = new Vector4(pos.X, pos.Y, pos.Z, 0),
                    color_charge = new Vector4(color, charge)
                };
            }

            metaballs_ssbo = GL.GenBuffer();
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, metaballs_ssbo);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, sizeof(float) * 8 * Metaballs.Length, Metaballs, BufferUsageHint.StaticDraw);
            #endregion

            #region meshSSBO
            int meshSSBO = GL.GenBuffer();
            int SSBOsize = sizeof(float) * 3 * 4 * 4 * MarchingCubesCount;
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, meshSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, SSBOsize, new float[0], BufferUsageHint.DynamicCopy);

            GL.UseProgram(compute_shader);
            GL.DispatchCompute(MarchingCubesCountX / 8, MarchingCubesCountY / 8, MarchingCubesCountZ / 8);

            var data = new float[SSBOsize];
            GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, new IntPtr(0), new IntPtr(SSBOsize), data);
            #endregion
        }

        const float DEFAULT_VERT_VALUE = -10000;

        Random rand = new Random();

        struct Metaball
        {
            public Vector4 position;
            public Vector4 color_charge;
        }

        const int metaball_count = 5;
        const float threshold = 8f;

        Vector3 MarchingCubesMin = new Vector3(-2);
        float MarchingCubesStep = 4f / 32f; 
        const int MarchingCubesCountX = 32;
        const int MarchingCubesCountY = 32;
        const int MarchingCubesCountZ = 32;
        const int MarchingCubesCount = MarchingCubesCountX * MarchingCubesCountY * MarchingCubesCountZ;

        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, window_width / (float)window_height, 0.01f, 100);
        Matrix4 model = Matrix4.Identity;

        Camera camera = new Camera(new Vector3(0, 0, 4), 0, -(float)Math.PI / 2);

        protected override void OnRenderFrame(FrameEventArgs E)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(render_shader);

            Matrix4 transform_mat = model * camera.Matrix * projection;
            GL.UniformMatrix4(GL.GetUniformLocation(render_shader, "transform_mat"), false, ref transform_mat);

            GL.DrawArraysInstanced(PrimitiveType.Points, 0, 1, MarchingCubesCount);

            SwapBuffers();

            camera.Update(0.01f);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Environment.Exit(1);

            Camera.MouseEvents(e);
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            Camera.MouseEvents(e);
        }
    }
}