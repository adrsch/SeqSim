using Stride.Core.Collections;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public enum NavmeshType
    {
        Default,
        Smash,
    }
    public class Navmeshes : StartupScript
    {
        public static Navmeshes S;

        public NavigationComponent DefaultNav;
        public NavigationComponent SmashNav;


        public NavigationQuerySettings QuerySettings = new NavigationQuerySettings
        {
            MaxPathPoints = 1024,
            FindNearestPolyExtent = new Vector3(4.0f, 6f, 4.0f),
            //FindNearestPolyExtent = new Vector3(2.0f, 4f, 2.0f),
        };

        public override void Start()
        {
            base.Start();
            S = this;

            var dynamicNavigationMeshSystem = Game.GameSystems.OfType<DynamicNavigationMeshSystem>().FirstOrDefault();

            // Wait for the dynamic navigation to be registered
            if (dynamicNavigationMeshSystem == null)
                Game.GameSystems.CollectionChanged += GameSystemsOnCollectionChanged;
            else
                dynamicNavigationMeshSystem.Enabled = true;
        }

        public override void Cancel()
        {
            Game.GameSystems.CollectionChanged -= GameSystemsOnCollectionChanged;
        }

        private void GameSystemsOnCollectionChanged(object sender, TrackingCollectionChangedEventArgs trackingCollectionChangedEventArgs)
        {
            if (trackingCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Add)
            {
                var dynamicNavigationMeshSystem = trackingCollectionChangedEventArgs.Item as DynamicNavigationMeshSystem;
                if (dynamicNavigationMeshSystem != null)
                {
                    SetupDynamicNav(dynamicNavigationMeshSystem);

                    // No longer need to listen to changes
                    Game.GameSystems.CollectionChanged -= GameSystemsOnCollectionChanged;
                }
            }
        }

        DynamicNavigationMeshSystem MeshSystem;

        void SetupDynamicNav(DynamicNavigationMeshSystem sys)
        {
            MeshSystem = sys;
            sys.Enabled = true;
            sys.AutomaticRebuild = false;
            sys.Rebuild();
        }

        public void Rebuild() => MeshSystem?.Rebuild();

        public bool TryFindPath(Vector3 start, Vector3 end, NavmeshType mesh, IList<Vector3> path)
        {
            switch (mesh)
            {
                default:
                case NavmeshType.Default:
                    return DefaultNav.TryFindPath(start, end, path, QuerySettings);

                case NavmeshType.Smash:
                    return SmashNav.TryFindPath(start, end, path, QuerySettings);

            }
        }
    }
}
