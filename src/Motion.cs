using System.Numerics;

namespace MinimalJSim {
    public class Motion {
        // velocities
        public Property p, q, r; // angular velocity <p(roll), q(pitch), r(yaw)> for axis <x, y, z>
        public Vector3 vel, angular;
        public Quaternion rotation;
        // positions
        public Property alt, terrainAlt; // sea-level geometric altitude, terrain elevation;

        public void UpdateProperty(DynamicsModel model) {
            (p.Value, q.Value, r.Value) = (angular.X, angular.Y, angular.Z);
        }
    }
}