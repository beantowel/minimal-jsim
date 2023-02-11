using System;
using System.Diagnostics;
using System.Numerics;

namespace MinimalJSim {
    public class Matrix3x3 {
        public float M11;
        public float M21;
        public float M31;
        public float M12;
        public float M22;
        public float M32;
        public float M13;
        public float M23;
        public float M33;

        public Matrix3x3() { }

        public Matrix3x3(Matrix3x3 m) {
            M13 = m.M13; M12 = m.M12; M11 = m.M11;
            M23 = m.M23; M22 = m.M22; M21 = m.M21;
            M33 = m.M33; M32 = m.M32; M31 = m.M31;
        }

        public Matrix3x3(float m11, float m12, float m13,
            float m21, float m22, float m23,
            float m31, float m32, float m33) {
            M13 = m13; M12 = m12; M11 = m11;
            M23 = m23; M22 = m22; M21 = m21;
            M33 = m33; M32 = m32; M31 = m31;
        }

        public Matrix3x3(Quaternion q) {
            float x = q.X;
            float y = q.Y;
            float z = q.Z;
            float w = q.W;

            float x2 = x + x;
            float y2 = y + y;
            float z2 = z + z;

            float xx = x2 * x;
            float yy = y2 * y;
            float zz = z2 * z;

            float xy = x2 * y;
            float xz = x2 * z;
            float xw = x2 * w;

            float yz = y2 * z;
            float yw = y2 * w;
            float zw = z2 * w;

            (M11, M21, M31) = (1.0f - yy - zz, xy + zw, xz - yw);
            (M12, M22, M32) = (xy - zw, 1.0f - xx - zz, yz + xw);
            (M13, M23, M33) = (xz + yw, yz - xw, 1.0f - xx - yy);
        }

        public static Matrix3x3 Transpose(Matrix3x3 m) {
            return new Matrix3x3(
                m.M11, m.M21, m.M31,
                m.M12, m.M22, m.M32,
                m.M13, m.M23, m.M33
            );
        }

        public override string ToString() {
            return $@"MinimalJSim.Matrix3x3{{
                {M11}, {M21}, {M31},
                {M12}, {M22}, {M32},
                {M13}, {M23}, {M33},
            }}";
        }

        public static Matrix3x3 operator *(Matrix3x3 m, float x) {
            return new Matrix3x3(
                m.M11 * x, m.M11 * x, m.M13 * x,
                m.M21 * x, m.M22 * x, m.M23 * x,
                m.M31 * x, m.M32 * x, m.M33 * x
            );
        }

        public static Vector3 operator *(Matrix3x3 m, Vector3 v) {
            return new Vector3(
                Vector3.Dot(v, m.Row(0)),
                Vector3.Dot(v, m.Row(1)),
                Vector3.Dot(v, m.Row(2))
            );
        }

        public static Matrix3x3 operator *(Matrix3x3 l, Matrix3x3 r) {
            float v(UInt32 i, UInt32 j) {
                return Vector3.Dot(l.Row(i), r.Col(j));
            }
            return new Matrix3x3(
                v(0, 0), v(0, 1), v(0, 2),
                v(1, 0), v(1, 1), v(1, 2),
                v(2, 0), v(2, 1), v(2, 2)
            );
        }

        public float this[UInt32 i, UInt32 j] {
            get {
                switch (i, j) {
                    case (0, 0):
                        return M11;
                    case (0, 1):
                        return M12;
                    case (0, 2):
                        return M13;
                    case (1, 0):
                        return M21;
                    case (1, 1):
                        return M22;
                    case (1, 2):
                        return M23;
                    case (2, 0):
                        return M31;
                    case (2, 1):
                        return M32;
                    case (2, 2):
                        return M33;
                    default:
                        Logger.Error("invalid index");
                        return 0;
                }
            }
        }

        public Vector3 Row(UInt32 i) {
            switch (i) {
                case 0:
                    return new Vector3(M11, M12, M13);
                case 1:
                    return new Vector3(M21, M22, M23);
                case 2:
                default:
                    return new Vector3(M31, M32, M33);
            }
        }

        public Vector3 Col(UInt32 i) {
            switch (i) {
                case 0:
                    return new Vector3(M11, M21, M31);
                case 1:
                    return new Vector3(M12, M22, M32);
                case 2:
                default:
                    return new Vector3(M13, M23, M33);
            }
        }
    }

    static class MathJ {
        public static (Vector3, Quaternion) Diagonalize(in Matrix3x3 m) {
            // jacobi rotation using quaternions (from an idea of Stan Melax, with fix for precision issues)
            Quaternion q = Quaternion.Identity;
            const UInt32 MAX_ITERS = 24;

            UInt32 GetNextIndex3(UInt32 x) {
                return (x + 1) % 3;
            }

            Quaternion IndexedRotation(UInt32 axis, float s, float c) {
                float[] v = { 0, 0, 0 };
                v[axis] = s;
                return new Quaternion(v[0], v[1], v[2], c);
            }

            Matrix3x3 d = new Matrix3x3();
            for (UInt32 i = 0; i < MAX_ITERS; i++) {
                Matrix3x3 axes = new Matrix3x3(q);
                d = new Matrix3x3(Matrix3x3.Transpose(axes) * m * axes);

                float d0 = Math.Abs(d[1, 2]), d1 = Math.Abs(d[0, 2]), d2 = Math.Abs(d[0, 1]);
                UInt32 a = (UInt32)(d0 > d1 && d0 > d2 ? 0 : d1 > d2 ? 1 : 2); // rotation axis index, from largest Off-diagonal element

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

            return (new Vector3(d.M11, d.M22, d.M33), q);
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
            Logger.Error($"search ordered fail, seq={seq}, v={v}");
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