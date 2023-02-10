using System.Numerics;

namespace MinimalJSim {
    public class Motion {
        public Vector3 angular;
        public Property p, q, r; // angular velocity <p(roll), q(pitch), r(yaw)> for axis <x, y, z>
        public Property roll;
        public Property alt, terrainAlt, height; // sea-level geometric altitude, terrain elevation;

        public Motion(DynamicsModel model) {
            p = model.GetProperty("velocities/p-aero-1?");
            q = model.GetProperty("velocities/q-aero-1?");
            r = model.GetProperty("velocities/r-aero-1?");

            roll = model.GetProperty("attitude/roll-1?");
            alt = model.GetProperty("position/h-sl-1?");
            terrainAlt = model.GetProperty("position/terrain-elevation-asl-1?");
            height = model.GetProperty("position/h-agl-1?");
        }

        public void UpdateProperty(DynamicsModel _) {
            (p.Val, q.Val, r.Val) = (angular.X, angular.Y, angular.Z);
            height.Val = alt.Val - terrainAlt.Val;
        }
    }
}