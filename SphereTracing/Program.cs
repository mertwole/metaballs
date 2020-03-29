using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace SphereTracing
{
    class Game : GameWindow
    {
        [STAThread]
        static void Main(){
            Game game = new Game();
            game.Run();
        }

        static int window_width = 1200;
        static int window_height = 900;
        
        public Game() : base(window_width, window_height, GraphicsMode.Default, "metaballs"){
            VSync = VSyncMode.On;
        }

        protected override void OnResize(EventArgs E){
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
        }

        int render_shader;

        protected override void OnLoad(EventArgs E){
            render_shader = new ShaderProgram().addVertexShader("vert_shader.glsl").addFragmentShader("frag_shader.glsl").Compile(out string rend_log);
            GL.UseProgram(render_shader);

            float[] vertices ={
                -1,-1,  -1,1,  1,1,  1,-1
            };

            int VBO = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            FillMetaballsSSBO();
        }

        struct Metaball
        {
            public Vector4 pos;
            public Vector4 color_charge;
        }

        const int metaballs_count = 4;

        void FillMetaballsSSBO()
        {     
            Metaball[] metaballs = new Metaball[metaballs_count];
            Random rand = new Random();
            for(int i = 0; i < metaballs_count; i++)
            {
                float r = (float)rand.Next(1024) / 1024;
                float g = (float)rand.Next(1024) / 1024;
                float b = (float)rand.Next(1024) / 1024;
                metaballs[i].color_charge = new Vector4(r, g, b, 1);
                float x = (float)rand.Next(-1024, 1024) / 1024;
                float y = (float)rand.Next(-1024, 1024) / 1024;
                float z = (float)rand.Next(-1024, 1024) / 1024;
                metaballs[i].pos = new Vector4(x * 3, y * 3, z * 3, 0);
            }

            int metaballs_SSBO = GL.GenBuffer();
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, metaballs_SSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, metaballs.Length * sizeof(float) * 8, metaballs, BufferUsageHint.StaticDraw);
        }

        Camera camera = new Camera(new Vector3(0, 0, 10), 0, -(float)Math.PI / 2);
        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, (float)window_width / (float)window_height, 0.1f, 100);

        protected override void OnRenderFrame(FrameEventArgs E)
        {
            Matrix4 transform = camera.Matrix * projection;
            transform.Invert();
            GL.UniformMatrix4(GL.GetUniformLocation(render_shader, "transform_mat"), false, ref transform);
            GL.Uniform3(GL.GetUniformLocation(render_shader, "cam_pos"), camera.Pos);

            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            camera.Update(0.05f);
            SwapBuffers();
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            camera.KeyboardEvents(e);

            if (e.Keyboard.IsKeyDown(Key.R))
                FillMetaballsSSBO();
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            camera.KeyboardEvents(e);
        }
    }
}
