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

        static int window_width = 512;
        static int window_height = 512;

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
            render_shader = new ShaderProgram()
                .addVertexShader(new System.IO.StreamReader("vert_shader.glsl"))
                .addFragmentShader(new System.IO.StreamReader("frag_shader.glsl"))
                .Compile();

            compute_shader = new ShaderProgram()
                .addComputeShader(new System.IO.StreamReader("comp_shader.glsl")).Compile();
            #endregion

            #region uniforms in compute shader
            GL.UseProgram(compute_shader);
            GL.Uniform1(GL.GetUniformLocation(compute_shader, "threshold"), threshold);
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

            #region create VAO & VBO
            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);
            {
                float[] screenQuad = new float[]
                {
                    -1, -1, 
                    -1, 1, 
                    1, 1,
                    1, -1
                };

                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 8, screenQuad, BufferUsageHint.StaticDraw);

                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
            #endregion
        }

        Random rand = new Random();

        struct Metaball
        {
            public Vector4 position;
            public Vector4 color_charge;
        }

        const int metaball_count = 5;
        const float threshold = 8f;

        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, window_width / (float)window_height, 0.01f, 100);
        Matrix4 model = Matrix4.Identity;

        Camera camera = new Camera(new Vector3(0, 0, 4), 0, -(float)Math.PI / 2);

        protected override void OnRenderFrame(FrameEventArgs E)
        {
            Matrix4 transform_mat = model * camera.Matrix * projection;

            GL.UseProgram(render_shader);

            GL.BindVertexArray(VAO);
            {
                
                
            }
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