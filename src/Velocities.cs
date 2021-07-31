using System.Numerics;

namespace MinimalJSim {
    public class Velocities {
        // angular velocity <p(roll), q(pitch), r(yaw)> for axis <x, y, z>
        public Property p, q, r, mach;
        public Vector3 vel;
    }
}