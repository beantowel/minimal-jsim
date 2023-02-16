using System;
using System.Numerics;
namespace MinimalJSim {
    public static class Frames {
        // vel in unity object space
        public static Vector4 WindRot(Vector3 vel, Vector3 velDot) {
            vel = Obj2Body(vel);
            velDot = Obj2Body(velDot);
            var (u, v, w) = (vel.X, vel.Y, vel.Z);
            var (du, dv, dw) = (velDot.X, velDot.Y, velDot.Z);
            float uw = u * u + w * w;
            float vt = vel.Length();

            float alpha = 0, beta = 0;
            float aDot = 0, bDot = 0;
            if (vt > 1e-3) {
                // yaw left (right slide)
                beta = (float)Math.Atan2(v, Math.Sqrt(uw));
                if (uw >= 1e-6) {
                    // pitch up
                    alpha = (float)Math.Atan2(w, u);
                    aDot = (u * dw - w * du) / uw; // d/dx (atan(w/u))
                    float dvt = (u * du + v * dv + w * dw) / vt; // sqrt(u**2+v**2+w**2)
                    bDot = (dv * vt - v * dvt) / (vt * (float)Math.Sqrt(uw));
                }
            }
            return new Vector4(alpha, beta, aDot, bDot);
        }

        public static Matrix3x3 Wind2Body(float alpha, float beta) {
            float ca = (float)Math.Cos(alpha);
            float sa = (float)Math.Sin(alpha);
            float cb = (float)Math.Cos(beta);
            float sb = (float)Math.Sin(beta);
            return new Matrix3x3(
                ca * cb, -ca * sb, -sa,
                sb, cb, 0,
                sa * cb, -sa * sb, ca
            );
        }

        public static Vector3 Cons2Body(Vector3 centerOfMass, Vector3 v) {
            var flip = new Vector3(-1, 1, -1);
            return (v - centerOfMass) * flip;
        }

        public static Vector3 Body2Obj(Vector3 v) {
            return new Vector3(v.Y, -v.Z, v.X);
        }

        public static Vector3 Obj2Body(Vector3 v) {
            return new Vector3(v.Z, v.X, -v.Y);
        }

        public static Quaternion Cons2Body(Quaternion q) {
            return new Quaternion(-q.X, q.Y, -q.Z, q.W);
        }

        public static Quaternion Body2Obj(Quaternion q) {
            return new Quaternion(q.Y, -q.Z, q.X, q.W);
        }

        public static Vector3 FlipHandedness(Vector3 v) {
            // holy moly
            return -v;
        }
    }
}