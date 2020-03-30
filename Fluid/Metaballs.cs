using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Fluid
{
    struct Metaball
    {
        public Vector4 pos;
        public Vector4 velocity;
        public Vector4 color_charge;

        public const int size = sizeof(float) * 12;
    }
    
    static class Metaballs
    {
        public static int metaballs_count = 64;

        static int compute_shader, metaballs_shader, bounding_shader;
        static int quad_VAO, sphere_VAO;
        public static void Init(int window_width, int window_height)
        {
            #region shaders
            metaballs_shader = new ShaderProgram()
                .addVertexShader(new System.IO.StreamReader("shaders/metaballs.vert"))
                .addFragmentShader(new System.IO.StreamReader("shaders/metaballs.frag"))
                .Compile();

            bounding_shader = new ShaderProgram()
                .addVertexShader(new System.IO.StreamReader("shaders/metaballs_bounding.vert"))
                .addFragmentShader(new System.IO.StreamReader("shaders/metaballs_bounding.frag"))
                .addTessEvaluationShader(new System.IO.StreamReader("shaders/metaballs_bounding.tese"))
                .Compile();

            compute_shader = new ShaderProgram()
                .addComputeShader(new System.IO.StreamReader("shaders/compute_movement.comp"))
                .Compile();
            #endregion

            #region quad
            float[] quad_vertices = { -1,-1,  -1,1,  1,1,  1,-1 };

            quad_VAO = GL.GenVertexArray();
            GL.BindVertexArray(quad_VAO);
            {
                int quad_VBO = GL.GenBuffer();

                GL.BindBuffer(BufferTarget.ArrayBuffer, quad_VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, quad_vertices.Length * sizeof(float), quad_vertices, BufferUsageHint.StaticDraw);

                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);
            }
            #endregion

            #region sphere
            sphere_VAO = GL.GenVertexArray();
            GL.BindVertexArray(sphere_VAO);
            {
                int sphere_VBO = GL.GenBuffer();

                GL.BindBuffer(BufferTarget.ArrayBuffer, sphere_VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, 0, new float[0], BufferUsageHint.StaticDraw);
            }

            GL.PatchParameter(PatchParameterInt.PatchVertices, 1);
            GL.PatchParameter(PatchParameterFloat.PatchDefaultInnerLevel, new float[2] { 20.0f, 20.0f });
            GL.PatchParameter(PatchParameterFloat.PatchDefaultOuterLevel, new float[4] { 20.0f, 20.0f, 20.0f, 20.0f });
            #endregion

            projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, (float)window_width / (float)window_height, 0.1f, 100);

            int metaballs_SSBO = GL.GenBuffer();
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, metaballs_SSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, metaballs_count * Metaball.size, IntPtr.Zero, BufferUsageHint.StaticDraw);

            GL.Enable(EnableCap.StencilTest);
            GL.StencilOp(StencilOp.Keep, StencilOp.Replace, StencilOp.Replace);

            GL.UseProgram(bounding_shader);
            GL.Uniform1(GL.GetUniformLocation(bounding_shader, "cutoff_radius_k"), 6.0f);
            GL.UseProgram(metaballs_shader);
            GL.Uniform1(GL.GetUniformLocation(metaballs_shader, "cutoff_radius_k"), 20.0f);
        }

        public static void ComputeMovement()
        {
            GL.UseProgram(compute_shader);
            GL.DispatchCompute((metaballs_count - 1) / 32 + 1 , 1, 1);
        }

        static Random rand = new Random();
        static Vector3 RandVec3((float, float) xyz_range)
        {
            float rand_in_range((float, float) range)
            {
                float rand_norm = rand.Next(10000) / 10000f;
                return range.Item1 + (range.Item2 - range.Item1) * rand_norm;
            }
            return new Vector3(rand_in_range(xyz_range), rand_in_range(xyz_range), rand_in_range(xyz_range));
        }

        static int push_metaball_id = 0;

        public static void PushMetaball()
        {
            Metaball metaball = new Metaball();
            metaball.color_charge = new Vector4(0.2f, 0.4f, 0.8f, 0.5f);
            metaball.pos = new Vector4(RandVec3((-0.3f, 0.3f)));
            float vel = rand.Next(100) + 400f;
            metaball.velocity = new Vector4(vel, 0, 0, 0);

            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, (IntPtr)(push_metaball_id * Metaball.size), Metaball.size, ref metaball);

            push_metaball_id++;
            if (push_metaball_id == metaballs_count) push_metaball_id = 0;
        }

        static Matrix4 projection;

        public static void Draw(Camera camera)
        {
            Matrix4 transform = camera.Matrix * projection;

            GL.StencilMask(0xFF);
            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.StencilBufferBit);                       

            GL.BindVertexArray(sphere_VAO);
            {
                GL.UseProgram(bounding_shader);
                GL.UniformMatrix4(GL.GetUniformLocation(bounding_shader, "transform_mat"), false, ref transform);
                GL.DrawArraysInstanced(PrimitiveType.Patches, 0, 1, metaballs_count);
            }

            GL.StencilMask(0x00);
            GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
            
            // Inversed transform required by sphere tracing
            transform.Invert();
            
            GL.BindVertexArray(quad_VAO);
            {
                GL.UseProgram(metaballs_shader);             
                GL.UniformMatrix4(GL.GetUniformLocation(metaballs_shader, "transform_mat"), false, ref transform);
                GL.Uniform3(GL.GetUniformLocation(metaballs_shader, "cam_pos"), camera.Pos);

                GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            }
        }
    }
}
