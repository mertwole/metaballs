using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace Metaballs3D
{
    class Game : GameWindow
    {
        [STAThread]
        static void Main(){
            Game game = new Game();
            game.Run();
        }

        static int window_width = 320;
        static int window_height = 180;
        const int workgroup_size = 10;//max 32
        
        public Game() : base(window_width, window_height, GraphicsMode.Default, "metaballs"){
            VSync = VSyncMode.On;
        }

        protected override void OnResize(EventArgs E){
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
        }

        int VAO;
        int render_shader, compute_shader;

        protected override void OnLoad(EventArgs E){
            float[] vertices ={
                -1,-1,  -1,1,  1,1,  1,-1
            };

            VAO = GL.GenVertexArray();
            int VBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);{
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }GL.BindVertexArray(0);

            render_shader = new ShaderProgram().addVertexShader("vert_shader.glsl").addFragmentShader("frag_shader.glsl").Compile(out string rend_log);
            compute_shader = new ShaderProgram().addComputeShader("comp_shader.glsl").Compile(out string comp_log);

            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexStorage2D(TextureTarget2d.Texture2D, 1, SizedInternalFormat.Rgba8, window_width, window_height);
            GL.BindImageTexture(0, texture, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba8);

            GL.UseProgram(compute_shader);
            GL.Uniform2(GL.GetUniformLocation(compute_shader, "resolution"), new Vector2(window_width, window_height));
            GL.DispatchCompute(window_width / workgroup_size, window_height / workgroup_size, 1);

            GL.UseProgram(render_shader);            
        }

        protected override void OnRenderFrame(FrameEventArgs E){
            GL.BindVertexArray(VAO);{
                GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            }
            GL.BindVertexArray(0);

            SwapBuffers();
        }
    }
}
