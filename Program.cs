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

        static int window_width = 600;
        static int window_height = 600;

        public Game() : base(window_width, window_height, GraphicsMode.Default, "Sample")
        {
            VSync = VSyncMode.On;
        }

        protected override void OnResize(EventArgs E)
        {
            base.OnResize(E);
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
        }

        int render_shader;
        int VAO, VBO;
        int metaballs_ssbo;

        protected override void OnLoad(EventArgs E)
        {
            base.OnLoad(E);

            render_shader = CompileShaders.Compile(new System.IO.StreamReader("frag_shader.glsl"), new System.IO.StreamReader("vert_shader.glsl"));
            GL.UseProgram(render_shader);
            GL.Uniform1(GL.GetUniformLocation(render_shader, "threshold"), threshold);

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

            var Metaballs = new Metaball[]
            {
                new Metaball{ pos = new Vector2(0.5f, -0.5f), charge = 1, color = new Vector3(1, 0, 0)},
                new Metaball{ pos = new Vector2(-0.5f, -0.5f), charge = 1, color = new Vector3(0, 1, 0)},
                new Metaball{ pos = new Vector2(0, 0.5f), charge = 1, color = new Vector3(0, 0, 1)}
            };

            metaballs_ssbo = GL.GenBuffer();
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, metaballs_ssbo);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, sizeof(float) * 8 * Metaballs.Length, Metaballs, BufferUsageHint.StaticDraw);
        }

        struct Metaball
        {
            public Vector2 pos;
            public float charge;

            public float align_0;

            public Vector3 color;

            public float align_1;
        }

        const float threshold = 5;

        protected override void OnRenderFrame(FrameEventArgs E)
        {
            base.OnRenderFrame(E);

            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            SwapBuffers();
        }
    }
}