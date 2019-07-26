using OpenTK.Graphics.OpenGL;
using System.IO;

namespace Metaballs2D
{
    class CompileShaders
    {
        public static int Compile(StreamReader fragment_shader, StreamReader vertex_shader)
        {
            string vertex_shader_code = vertex_shader.ReadToEnd();
            string fragment_shader_code = fragment_shader.ReadToEnd();

            int shader_program;

            int vert_shader = GL.CreateShader(ShaderType.VertexShader);
            int frag_shader = GL.CreateShader(ShaderType.FragmentShader);
            
            GL.ShaderSource(frag_shader, fragment_shader_code);
            GL.CompileShader(frag_shader);

            GL.ShaderSource(vert_shader, vertex_shader_code);
            GL.CompileShader(vert_shader);

            shader_program = GL.CreateProgram();

            GL.AttachShader(shader_program, vert_shader);
            GL.AttachShader(shader_program, frag_shader);

            GL.LinkProgram(shader_program);

            GL.DeleteShader(vert_shader);
            GL.DeleteShader(frag_shader);

            return shader_program;
        }
    }
}
