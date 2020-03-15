using OpenTK.Graphics.OpenGL;
using System.IO;

namespace Metaballs3D
{
    public class ShaderProgram
    {
        int shader_program;
        string global_log = "";

        public ShaderProgram(){
            shader_program = GL.CreateProgram();
        }

        public int Compile(out string log){
            GL.LinkProgram(shader_program);

            global_log += "\nProgram log :\n" + GL.GetProgramInfoLog(shader_program);
            log = global_log;

            return shader_program;
        }

        public ShaderProgram addFragmentShader(StreamReader source){
            string code = source.ReadToEnd();
            int shader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(shader, code);
            GL.CompileShader(shader);

            global_log += "\nFragment shader log :\n" + GL.GetShaderInfoLog(shader);

            GL.AttachShader(shader_program, shader);

            return this;
        }      

        public ShaderProgram addVertexShader(StreamReader source){
            string code = source.ReadToEnd();
            int shader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(shader, code);
            GL.CompileShader(shader);

            global_log += "\nVertex shader log :\n" + GL.GetShaderInfoLog(shader);

            GL.AttachShader(shader_program, shader);

            return this;
        }

        public ShaderProgram addGeometryShader(StreamReader source){
            string code = source.ReadToEnd();
            int shader = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(shader, code);
            GL.CompileShader(shader);

            global_log += "\nGeometry shader log :\n" + GL.GetShaderInfoLog(shader);

            GL.AttachShader(shader_program, shader);

            return this;
        }

        public ShaderProgram addComputeShader(StreamReader source){
            string code = source.ReadToEnd();
            int shader = GL.CreateShader(ShaderType.ComputeShader);
            GL.ShaderSource(shader, code);
            GL.CompileShader(shader);

            global_log += "\nCompute shader log :\n" + GL.GetShaderInfoLog(shader);

            GL.AttachShader(shader_program, shader);

            return this;
        }

        public ShaderProgram addTessControlShader(StreamReader source){
            string code = source.ReadToEnd();
            int shader = GL.CreateShader(ShaderType.TessControlShader);
            GL.ShaderSource(shader, code);
            GL.CompileShader(shader);

            global_log += "\nTesselation control shader log :\n" + GL.GetShaderInfoLog(shader);

            GL.AttachShader(shader_program, shader);

            return this;
        }

        public ShaderProgram addTessEvaluationShader(StreamReader source){
            string code = source.ReadToEnd();
            int shader = GL.CreateShader(ShaderType.TessEvaluationShader);
            GL.ShaderSource(shader, code);
            GL.CompileShader(shader);

            global_log += "\nTesselation evaluation shader log :\n" + GL.GetShaderInfoLog(shader);

            GL.AttachShader(shader_program, shader);

            return this;
        }

        public ShaderProgram addFragmentShader(string filePath)
        {
            return addFragmentShader(new StreamReader(filePath));
        }

        public ShaderProgram addVertexShader(string filePath)
        {
            return addVertexShader(new StreamReader(filePath));
        }

        public ShaderProgram addGeometryShader(string filePath)
        {
            return addGeometryShader(new StreamReader(filePath));
        }

        public ShaderProgram addComputeShader(string filePath)
        {
            return addComputeShader(new StreamReader(filePath));
        }

        public ShaderProgram addTessControlShader(string filePath)
        {
            return addTessControlShader(new StreamReader(filePath));
        }

        public ShaderProgram addTessEvaluationShader(string filePath)
        {
            return addTessEvaluationShader(new StreamReader(filePath));
        }
    }
}
