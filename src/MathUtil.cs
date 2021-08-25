using System;
using System.Diagnostics;
using System.Numerics;

namespace MinimalJSim {
    class Matrix33 {
        public Matrix4x4 m;

        public Matrix33(Matrix4x4 m) {
            this.m = m;
        }

        public float this[UInt32 i, UInt32 j] {
            get { return Elem(i, j); }
        }

        float Elem(UInt32 i, UInt32 j) {
            switch (i, j) {
                case (0, 0):
                    return m.M11;
                case (0, 1):
                    return m.M12;
                case (0, 2):
                    return m.M13;
                case (1, 0):
                    return m.M21;
                case (1, 1):
                    return m.M22;
                case (1, 2):
                    return m.M23;
                case (2, 0):
                    return m.M31;
                case (2, 1):
                    return m.M32;
                case (2, 2):
                    return m.M33;
                default:
                    Logger.Error("invalid index");
                    return 0;
            }
        }
    }

    class MathUtil {
        static UInt32 GetNextIndex3(UInt32 x) {
            return (x + 1) % 3;
        }

        static Quaternion IndexedRotation(UInt32 axis, float s, float c) {
            float[] v = { 0, 0, 0 };
            v[axis] = s;
            return new Quaternion(v[0], v[1], v[2], c);
        }

        public static (Vector3, Quaternion) Diagonalize(in Matrix4x4 m) {
            // jacobi rotation using quaternions (from an idea of Stan Melax, with fix for precision issues)

            const UInt32 MAX_ITERS = 24;

            Quaternion q = Quaternion.Identity;

            Matrix33 d = new Matrix33(new Matrix4x4());
            for (UInt32 i = 0; i < MAX_ITERS; i++) {
                Matrix4x4 axes = Matrix4x4.CreateFromQuaternion(q);
                d = new Matrix33(Matrix4x4.Transpose(axes) * m * axes);

                float d0 = Math.Abs(d[1, 2]), d1 = Math.Abs(d[0, 2]), d2 = Math.Abs(d[0, 1]);
                UInt32 a = (UInt32)(d0 > d1 && d0 > d2 ? 0 : d1 > d2 ? 1 : 2); // rotation axis index, from largest off-diagonal element

                UInt32 a1 = GetNextIndex3(a), a2 = GetNextIndex3(a1);
                if (d[a1, a2] == 0.0f || Math.Abs(d[a1, a1] - d[a2, a2]) > 2e6f * Math.Abs(2.0f * d[a1, a2]))
                    break;

                float w = (d[a1, a1] - d[a2, a2]) / (2.0f * d[a1, a2]); // cot(2 * phi), where phi is the rotation angle
                float absw = Math.Abs(w);

                Quaternion r;
                if (absw > 1000)
                    r = IndexedRotation(a, 1 / (4 * w), 1f); // h will be very close to 1, so use small angle approx instead
                else {
                    float t = 1 / (absw + (float)Math.Sqrt(w * w + 1)); // absolute value of tan phi
                    float h = 1 / (float)Math.Sqrt(t * t + 1);          // absolute value of cos phi

                    Debug.Assert(h != 1); // |w|<1000 guarantees this with typical IEEE754 machine eps (approx 6e-8)
                    r = IndexedRotation(a, (float)Math.Sqrt((1 - h) / 2) * Math.Sign(w), (float)Math.Sqrt((1 + h) / 2));
                }

                q = Quaternion.Normalize(q * r);
            }

            return (new Vector3(d.m.M11, d.m.M22, d.m.M33), q);
        }



        /// SearchOrdered returns i so that seq[i] <= v < seq[i+1].
        /// if v < seq[0], return i=-1.
        /// if v >= seq[-1], return i=-2.
        public static int SearchOrdered(float[] seq, float v) {
            if (v < seq[0]) {
                return -1;
            }
            if (v >= seq[seq.Length - 1]) {
                return -2;
            }
            if (seq.Length <= 10) {
                for (int i = 0; i < seq.Length - 1; i++) {
                    if (v < seq[i + 1]) {
                        return i;
                    }
                }
            } else {
                int i = Array.BinarySearch(seq, v);
                i = (i >= 0) ? i : ~i - 1;
                return i;
            }
            Logger.Error("search ordered fail, seq={0}, v={1}", seq, v);
            return -3;
        }

        public static float[] ReciprocalSeqDiff(float[] seq) {
            float[] x = new float[seq.Length - 1];
            for (int i = 0; i < seq.Length - 1; i++) {
                x[i] = 1 / (seq[i + 1] - seq[i]);
            }
            return x;
        }

        public static void ScaleArray(float[] a, float scale) {
            for (int i = 0; i < a.Length; i++) {
                a[i] = a[i] * scale;
            }
        }
    }
}