using SharpDX.DXGI;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Physics;
using Stride.Rendering.ProceduralModels;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    [DataContract("ProjectionMesh")]
    [Display("ProjectionMesh")] // This name shows up in the procedural model dropdown list
    public class ProjectionMesh : PrimitiveProceduralModelBase
    {
        // A custom property that shows up in Game Studio
        /// <summary>
        /// Gets or sets the size of the model.
        /// </summary>
        public Vector3 Size { get; set; } = Vector3.One;

        public Vector3 Normal;

        public Vector3 WorldZero;
        public Simulation Sim;

        public void Spawn(Simulation sim, Vector3 worldZero, Vector3 normal)
        {
            Sim = sim;
            Normal = normal;
            WorldZero = worldZero;

            M.TranslationVector = worldZero;
            M = Matrix.RotationQuaternion(
                Quaternion.LookRotation(in Vector3.forward, in normal)) * M;
            /*

            var projectionStart = worldZero + normal * 1f;
            var hit = sim.Raycast(worldZero + normal * 1f, worldZero - normal * 1f);

            var rotation = Quaternion.LookRotation(Vector3.forward, normal);*/
            
        }

        public Matrix M = Matrix.Identity;
        Vector3 BuildPoint(float x, float z)
        {
            var pos = WorldZero + M.Right * x + M.Up * z;
            var hit = Sim.Raycast(pos - Normal * 1f, pos + Normal * 1f);
            if (hit.Succeeded)
                return hit.Point - WorldZero - Normal * 0.1f;
            else
            {
                return (pos - WorldZero) / 3 - Normal * 0.1f;//pos - WorldZero;
            }
        }
        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            // First generate the arrays for vertices and indices with the correct size
            var vertexCount = 9;
            var indexCount = 24;//6
            var vertices = new VertexPositionNormalTexture[vertexCount];
            var indices = new int[indexCount];

            // Create custom vertices, in this case just a quad facing in Y direction
            //  var normal = Vector3.UnitZ;
            var normal = - Normal;
            /*
   vertices[0] = new VertexPositionNormalTexture(new Vector3(-0.5f, 0.5f, 0) * Size, normal, new Vector2(0, 0));
   vertices[1] = new VertexPositionNormalTexture(new Vector3(0.5f, 0.5f, 0) * Size, normal, new Vector2(1, 0));
   vertices[2] = new VertexPositionNormalTexture(new Vector3(-0.5f, -0.5f, 0) * Size, normal, new Vector2(0, 1));
   vertices[3] = new VertexPositionNormalTexture(new Vector3(0.5f, -0.5f, 0) * Size, normal, new Vector2(1, 1));

    * */
        /*
            vertices[0] = new VertexPositionNormalTexture(BuildPoint(-0.5f, 0.5f) * Size, normal, new Vector2(0, 0));
            vertices[1] = new VertexPositionNormalTexture(BuildPoint(0.5f, 0.5f) * Size, normal, new Vector2(1, 0));
            vertices[2] = new VertexPositionNormalTexture(BuildPoint(-0.5f, -0.5f) * Size, normal, new Vector2(0, 1));
            vertices[3] = new VertexPositionNormalTexture(BuildPoint(0.5f, -0.5f) * Size, normal, new Vector2(1, 1));
        */

            vertices[0] = new VertexPositionNormalTexture(BuildPoint(-0.5f, 0.5f) * Size, normal, new Vector2(0, 0));
            vertices[1] = new VertexPositionNormalTexture(BuildPoint(0, 0.5f) * Size, normal, new Vector2(0.5f, 0));
            vertices[2] = new VertexPositionNormalTexture(BuildPoint(-0.5f, 0) * Size, normal, new Vector2(0, 0.5f));
            vertices[3] = new VertexPositionNormalTexture(BuildPoint(0, 0) * Size, normal, new Vector2(0.5f, 0.5f));

            // Create custom indices
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 1;
            indices[4] = 3;
            indices[5] = 2;


            vertices[4] = new VertexPositionNormalTexture(BuildPoint(0.5f, 0.5f) * Size, normal, new Vector2(1, 0));

            indices[6] = 1;
            indices[7] = 4;
            indices[8] = 3;

            vertices[5] = new VertexPositionNormalTexture(BuildPoint(0.5f, 0) * Size, normal, new Vector2(1, 0.5f));

            indices[9] = 4;
            indices[10] = 5;
            indices[11] = 3;

            vertices[6] = new VertexPositionNormalTexture(BuildPoint(0.5f, -0.5f) * Size, normal, new Vector2(1, 1));

            indices[12] = 5;
            indices[13] = 6;
            indices[14] = 3;

            indices[15] = 3;
            indices[16] = 6;
            indices[17] = 7;

            vertices[7] = new VertexPositionNormalTexture(BuildPoint(0f, -0.5f) * Size, normal, new Vector2(0.5f, 1));



            vertices[8] = new VertexPositionNormalTexture(BuildPoint(-0.5f, -0.5f) * Size, normal, new Vector2(0, 1));

            indices[18] = 3;
            indices[19] = 7;
            indices[20] = 8;


            indices[21] = 3;
            indices[22] = 8;
            indices[23] = 2;

            // Create the primitive object for further processing by the base class
            return new GeometricMeshData<VertexPositionNormalTexture>(vertices, indices, isLeftHanded: false) { Name = "MyModel" };
        }


    }
}