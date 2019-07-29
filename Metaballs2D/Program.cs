using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace Metaballs2D
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
            base.OnResize(E);
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
        }

        int render_shader, compute_shader;
        int VAO, VBO;
        int metaballs_ssbo;

        protected override void OnLoad(EventArgs E)
        {
            base.OnLoad(E);

            render_shader = CompileShaders.Compile(new System.IO.StreamReader("frag_shader.glsl"), new System.IO.StreamReader("vert_shader.glsl"));
            GL.UseProgram(render_shader);
            GL.Uniform1(GL.GetUniformLocation(render_shader, "threshold"), threshold);

            compute_shader = CompileShaders.CompileComputeShader(new System.IO.StreamReader("comp_shader.glsl"));

            float[] vertices =
            {
                -1, -1,
                -1, 1,
                1, 1,
                1, -1
            };

            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }

            var Metaballs = new Metaball[metaball_count];

            Vector3[] possible_colors = new Vector3[]
            {
                new Vector3(0, 1, 0)
            };

            for(int i = 0; i < metaball_count; i++)
            {
                Vector2 pos = new Vector2(rand.Next(-100, 100) / 100f, rand.Next(-100, 100) / 100f);
                Vector2 vel = new Vector2(rand.Next(-100, 100) / 10000f, rand.Next(-100, 100) / 10000f);
                Vector3 color = possible_colors[rand.Next(possible_colors.Length)];
                float charge = rand.Next(50) / 100 + 0.5f;

                Metaballs[i] = new Metaball()
                {
                    pos_vel = new Vector4(pos.X, pos.Y, vel.X, vel.Y),
                    color_charge = new Vector4(color, charge)
                };
            }

            metaballs_ssbo = GL.GenBuffer();
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, metaballs_ssbo);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, sizeof(float) * 8 * Metaballs.Length, Metaballs, BufferUsageHint.StaticDraw);
        }

        Random rand = new Random();

        struct Metaball
        {
            public Vector4 pos_vel;
            public Vector4 color_charge;
        }

        const int metaball_count = 32;
        const float threshold = 50;

        protected override void OnRenderFrame(FrameEventArgs E)
        {
            base.OnRenderFrame(E);

            GL.UseProgram(compute_shader);
            GL.DispatchCompute(metaball_count / 32, 1, 1);

            GL.UseProgram(render_shader);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            SwapBuffers();
        }
    }
}