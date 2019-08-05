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

        public Game() : base(window_width, window_height, GraphicsMode.Default, "Sample")
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
            #region compile shaders
            render_shader = CompileShaders.Compile(
                new System.IO.StreamReader("frag_shader.glsl"), 
                new System.IO.StreamReader("vert_shader.glsl"), 
                new System.IO.StreamReader("geom_shader.glsl"));

            //compute_shader = CompileShaders.CompileComputeShader(new System.IO.StreamReader("comp_shader.glsl"));
            #endregion

            #region uniforms in render shader
            GL.UseProgram(render_shader);
            GL.Uniform1(GL.GetUniformLocation(render_shader, "threshold"), threshold);

            GL.Uniform3(GL.GetUniformLocation(render_shader, "MarchingCubesCount"), MarchingCubesCountX, MarchingCubesCountY, MarchingCubesCountZ);
            GL.Uniform1(GL.GetUniformLocation(render_shader, "MarchingCubesStep"), MarchingCubesStep); 
            GL.Uniform3(GL.GetUniformLocation(render_shader, "MarchingCubesMin"), MarchingCubesMin);
            #endregion

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
                Vector3 pos = new Vector3(rand.Next(-50, 50) / 100f, rand.Next(-50, 50) / 100f, rand.Next(-50, 50) / 100f);
                Vector3 color = possible_colors[rand.Next(possible_colors.Length)];
                float charge = 1;

                Metaballs[i] = new Metaball()
                {
                    position = new Vector4(pos.X, pos.Y, pos.Z, 0),
                    color_charge = new Vector4(color, charge)
                };
            }

            Metaballs = new Metaball[]
            {
                new Metaball(){color_charge = new Vector4(1, 0, 0, 1), position = new Vector4(0, 0, 0, 0)}
            };

            metaballs_ssbo = GL.GenBuffer();
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, metaballs_ssbo);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, sizeof(float) * 8 * Metaballs.Length, Metaballs, BufferUsageHint.StaticDraw);
            #endregion
        }

        Random rand = new Random();

        struct Metaball
        {
            public Vector4 position;
            public Vector4 color_charge;
        }

        const int metaball_count = 1;
        const float threshold = 2;

        Vector3 MarchingCubesMin = new Vector3(-1);
        float MarchingCubesStep = 2f / 32f; 
        const int MarchingCubesCountX = 32;
        const int MarchingCubesCountY = 32;
        const int MarchingCubesCountZ = 32;
        const int MarchingCubesCount = MarchingCubesCountX * MarchingCubesCountY * MarchingCubesCountZ;

        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, window_width / (float)window_height, 0.01f, 100);
        Matrix4 model = Matrix4.Identity;

        Camera camera = new Camera(new Vector3(0, 0, 2), 0, -(float)Math.PI / 2);

        protected override void OnRenderFrame(FrameEventArgs E)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

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