using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace Fluid
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

        public Game() : base(window_width, window_height, new GraphicsMode(new ColorFormat(32), 24, 8), "fluid")
        {
            VSync = VSyncMode.On;
        }

        protected override void OnResize(EventArgs E)
        {
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
        }

        protected override void OnLoad(EventArgs E)
        {
            Metaballs.Init(window_width, window_height);
        }   

        Camera camera = new Camera(new Vector3(13, 0, 45), 0, -(float)Math.PI / 2);

        int time = 0;
        protected override void OnRenderFrame(FrameEventArgs E)
        {
            Metaballs.PushMetaball();       
            Metaballs.ComputeMovement();
            Metaballs.Draw(camera);
            camera.Update(0.05f);
            SwapBuffers();
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            camera.KeyboardEvents(e);
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            camera.KeyboardEvents(e);
        }
    }
}
