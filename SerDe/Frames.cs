using System;
using System.Numerics;
namespace MinimalJSim {
    public static class Frames {
        // vel in unity object space
        public static Vector2 WindRot(Vector3 vel) {
            vel = Obj2Body(vel);
            float alpha = (float)Math.Atan2(vel.Z, vel.X); // pitch up
            float beta = (float)Math.Atan2(vel.Y, vel.X); // right slide (yaw left)
            return new Vector2(alpha, beta);
        }

        public static Matrix3x3 Wind2Body(Vector2 rotation) {
            var (alpha, beta) = (rotation.X, rotation.Y);
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

        public static Matrix3x3 Body2Wind(Vector2 rotation) {
            return Matrix3x3.Transpose(Wind2Body(rotation));
        }

        public static Matrix3x3 Body2Wind(Matrix3x3 w2b) {
            return Matrix3x3.Transpose(w2b);
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