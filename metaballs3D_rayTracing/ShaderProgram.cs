using OpenTK.Graphics.OpenGL;
using System.IO;

namespace Metaballs3D
{
    public class ShaderProgram
    {
        int shader_program;

        public ShaderProgram()
        {
            shader_program = GL.CreateProgram();
        }

        public int Compile()
        {
            GL.LinkProgram(shader_program);

            string log = GL.GetProgramInfoLog(shader_program);
            if (log != "")
                throw new System.Exception(log);

            return shader_program;
        }

        public ShaderProgram addFragmentShader(StreamReader source)
        {
            string code = source.ReadToEnd();
            int shader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(shader, code);
            GL.CompileShader(shader);

            string log = GL.GetShaderInfoLog(shader);
            if (log != "")
                throw new System.Exception(log);

            GL.AttachShader(shader_program, shader);

            return this;
        }

        public ShaderProgram addVertexShader(StreamReader source)
        {
            string code = source.ReadToEnd();
            int shader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(shader, code);
            GL.CompileShader(shader);

            string log = GL.GetShaderInfoLog(shader);
            if (log != "")
                throw new System.Exception(log);

            GL.AttachShader(shader_program, shader);

            return this;
        }

        public ShaderProgram addGeometryShader(StreamReader source)
        {
            string code = source.ReadToEnd();
            int shader = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(shader, code);
            GL.CompileShader(shader);

            string log = GL.GetShaderInfoLog(shader);
            if (log != "")
                throw new System.Exception(log);

            GL.AttachShader(shader_program, shader);

            return this;
        }

        public ShaderProgram addComputeShader(StreamReader source)
        {
            string code = source.ReadToEnd();
            int shader = GL.CreateShader(ShaderType.ComputeShader);
            GL.ShaderSource(shader, code);
            GL.CompileShader(shader);

            string log = GL.GetShaderInfoLog(shader);
            if (log != "")
                throw new System.Exception(log);

            GL.AttachShader(shader_program, shader);

            return this;
        }

        public ShaderProgram addTessControlShader(StreamReader source)
        {
            string code = source.ReadToEnd();
            int shader = GL.CreateShader(ShaderType.TessControlShader);
            GL.ShaderSource(shader, code);
            GL.CompileShader(shader);

            string log = GL.GetShaderInfoLog(shader);
            if (log != "")
                throw new System.Exception(log);

            GL.AttachShader(shader_program, shader);

            return this;
        }

        public ShaderProgram addTessEvaluationShader(StreamReader source)
        {
            string code = source.ReadToEnd();
            int shader = GL.CreateShader(ShaderType.TessEvaluationShader);
            GL.ShaderSource(shader, code);
            GL.CompileShader(shader);

            string log = GL.GetShaderInfoLog(shader);
            if (log != "")
                throw new System.Exception(log);

            GL.AttachShader(shader_program, shader);

            return this;
        }
    }
}