using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;

namespace OpenTK_Sample
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

        int render_shader, compute_shader;

        protected override void OnLoad(EventArgs E)
        {
            base.OnLoad(E);

            GL.ClearColor(Color.Black);
            //**************shaders****************
            render_shader = CompileShaders.Compile(new System.IO.StreamReader("frag_shader.glsl"), new System.IO.StreamReader("vert_shader.glsl"));

            compute_shader = CompileShaders.CompileComputeShader(new System.IO.StreamReader("compute_shader.glsl"));
            //*************************************
        }

        protected override void OnRenderFrame(FrameEventArgs E)
        {
            base.OnRenderFrame(E);

            SwapBuffers();
        }
    }
}