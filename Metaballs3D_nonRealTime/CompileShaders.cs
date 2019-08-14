using OpenTK.Graphics.OpenGL;
using System.IO;

namespace Metaballs3D
{
    class CompileShaders
    {
        public static int Compile(StreamReader fragment_shader, StreamReader vertex_shader, StreamReader geometry_shader)
        {
            string vertex_shader_code = vertex_shader.ReadToEnd();
            string fragment_shader_code = fragment_shader.ReadToEnd();
            string geometry_shader_code = geometry_shader.ReadToEnd();

            int shader_program;

            int vert_shader = GL.CreateShader(ShaderType.VertexShader);
            int frag_shader = GL.CreateShader(ShaderType.FragmentShader);
            int geom_shader = GL.CreateShader(ShaderType.GeometryShader);

            GL.ShaderSource(frag_shader, fragment_shader_code);
            GL.CompileShader(frag_shader);

            GL.ShaderSource(vert_shader, vertex_shader_code);
            GL.CompileShader(vert_shader);

            GL.ShaderSource(geom_shader, geometry_shader_code);
            GL.CompileShader(geom_shader);

            shader_program = GL.CreateProgram();

            GL.AttachShader(shader_program, vert_shader);
            GL.AttachShader(shader_program, frag_shader);
            GL.AttachShader(shader_program, geom_shader);

            GL.LinkProgram(shader_program);

            GL.DeleteShader(vert_shader);
            GL.DeleteShader(frag_shader);
            GL.DeleteShader(geom_shader);

            return shader_program;
        }

        public static int CompileComputeShader(StreamReader compute_shader)
        {
            string compute_shader_code = compute_shader.ReadToEnd();

            int comp_shader = GL.CreateShader(ShaderType.ComputeShader);
            GL.ShaderSource(comp_shader, compute_shader_code);
            GL.CompileShader(comp_shader);

            int shader_program = GL.CreateProgram();

            GL.AttachShader(shader_program, comp_shader);
            GL.LinkProgram(shader_program);

            GL.DeleteShader(comp_shader);

            return shader_program;
        }
    }
}
