using Stride.Engine;
using Stride.Rendering.ProceduralModels;
using Stride.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Physics;
using Stride.Core.Mathematics;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public class ActorProjectionMesh : StartupScript
    {
        public Material Mat;
        public Material FadedMat;
        public Actor Actor;
        public Vector3 Size;
        public int FadeHour;
        public int KillHour;

        Model Mod;
        public override void Start()
        {
            base.Start();
            // The model classes
            var myModel = new ProjectionMesh();
            // myModel.Spawn(InteractionProbe.S.GetSimulation(), Transform.WorldPosition, Transform.Up);
            myModel.Size = Size;
            myModel.Sim = InteractionProbe.S.GetSimulation();
            myModel.WorldZero = Transform.WorldMatrix.TranslationVector;
            Transform.GetWorldRotation(true);
            myModel.M = Matrix.RotationQuaternion(Actor.State.GetProjectionMeshOrientation()) * Transform.WorldMatrix;
            myModel.Normal = myModel.M.Forward;
            var model = new Model();
            var modelComponent = new ModelComponent(model);

            // Generate the procedual model
            myModel.Generate(Services, model);

            // Add a meterial
            //var material = Content.Load<Material>("MyModel Material");
            // model.Add(material);
            model.Add(Mat);
            modelComponent.IsShadowCaster = false;
            FadeHour = Actor.State.GetVar<int>("fadehour");
            KillHour = Actor.State.GetVar<int>("killhour");
            // Add everything to the entity
            Entity.Add(modelComponent);

            Mod = model;
            Clock.S.OnHour += UpdateRollover;
        }

        public override void Cancel()
        {
            base.Cancel();
            Clock.S.OnHour -= UpdateRollover;
        }

        void UpdateRollover()
        {
            if (Clock.S.HoursAndDays >=  KillHour)
            {
                Actor.State.Destroy();
            }
            else if (Clock.S.HoursAndDays >= FadeHour)
            {
                Mod.Materials[0] = FadedMat;
            }
        }
    }
}
