using System;
using System.Collections.Generic;

namespace MinimalJSim {
    public enum FuncType {
        abs,
        acos,
        and,
        asin,
        atan,
        atan2,
        avg,
        cos,
        difference,
        eq,
        fraction,
        ge,
        gt,
        ifthen,
        integer,
        interpolate1d,
        le,
        lt,
        max,
        min,
        mod,
        not,
        nq,
        or,
        pi,
        pow,
        product,
        property,
        quotient,
        random,
        sin,
        sum,
        switch_fn,
        table,
        tan,
        urandom,
        value,
    }

    public enum AxisDimension {
        Drag = 0,
        Side = 1,
        Lift = 2,
        Roll = 3,
        Pitch = 4,
        Yaw = 5,
        Dummy = 6,
    }

    public abstract class Function {
        public FuncType fType;
        public string identifier, description;

        public Function(FuncType type) { fType = type; }
        public abstract float Eval();
        public abstract Function[] Dependency();
        public List<string> DependProps() {
            Function[] empty = { };
            var props = new List<string>();
            Queue<Function> q = new Queue<Function>(); // bfs
            foreach (var dep in Dependency() ?? empty) {
                q.Enqueue(dep);
            }
            while (q.Count > 0) {
                var f = q.Dequeue();
                if (f is PropOrValue pv) {
                    if (!pv.isValue) {
                        props.Add(pv.prop.identifier);
                    }
                }
                foreach (var dep in f.Dependency() ?? empty) {
                    q.Enqueue(dep);
                }
            }
            return props;
        }
    }

    class PropOrValue : Function {
        public float value;
        public Property prop;
        public bool isValue;

        public PropOrValue(float value) : base(FuncType.property) {
            this.value = value;
            isValue = true;
        }

        public PropOrValue(Property prop) : base(FuncType.property) {
            this.prop = prop;
            isValue = false;
        }

        public override float Eval() {
            return isValue ? value : prop.Val;
        }

        public override Function[] Dependency() {
            return null;
        }
    }

    class CommutativeOperator : Function {
        public Function[] functions;
        public float initial;
        int n;

        public CommutativeOperator(FuncType typ, List<Function> fs) : base(typ) {
            initial = Zero(fs);
            n = fs.Count;

            int size = 0;
            foreach (var f in fs) {
                if (f is PropOrValue pv && pv.isValue) {
                    size++;
                }
            }
            int i = 0;
            functions = new Function[fs.Count - size];
            foreach (var f in fs) {
                if (f is PropOrValue pv && pv.isValue) {
                    initial = Operate(initial, pv.value);
                } else {
                    functions[i++] = f;
                }
            }
        }

        float Zero(List<Function> fs) {
            switch (fType) {
                case FuncType.product:
                    return 1;
                default:
                    return 0;
            }
        }

        float Operate(float a, float b) {
            switch (fType) {
                case FuncType.product:
                    return a * b;
                case FuncType.sum:
                    return a + b;
                case FuncType.avg:
                    return a + b / (float)n;
                default:
                    return 0;
            }
        }

        public override float Eval() {
            float v = initial;
            foreach (Function func in functions) {
                v = Operate(v, func.Eval());
            }
            return v;
        }

        public override Function[] Dependency() {
            return functions;
        }
    }

    class Table1 : Function {
        public Property var;
        public float[] row, value;
        float[] rRowDiff;

        public Table1() : base(FuncType.table) { }

        public Table1(in float[] _row, in float[] _value) : base(FuncType.table) {
            row = _row;
            value = _value;
        }

        public void Init(float scale) {
            MathJ.ScaleArray(row, scale);
            rRowDiff = MathJ.ReciprocalSeqDiff(row);
        }

        public override float Eval() {
            return Eval(var.Val);
        }

        public float Eval(float v) {
            int i = MathJ.SearchOrdered(row, v);
            switch (i) {
                case -1:
                    return value[0];
                case -2:
                    return value[row.Length - 1];
            }
            float alpha = (v - row[i]) * rRowDiff[i];
            // linear interpolation
            return value[i] + (value[i + 1] - value[i]) * alpha;
        }

        public override Function[] Dependency() {
            return new Function[] { new PropOrValue(var) };
        }
    }

    class Table2 : Function {
        public Property varRow, varCol;
        public float[] row, col;
        public float[,] value;
        float[] rRowDiff, rColDiff;

        public Table2() : base(FuncType.table) { }

        public void Init(float rScale, float cScale) {
            MathJ.ScaleArray(row, rScale);
            MathJ.ScaleArray(col, cScale);
            rRowDiff = MathJ.ReciprocalSeqDiff(row);
            rColDiff = MathJ.ReciprocalSeqDiff(col);
        }

        public override float Eval() {
            float vR = varRow.Val;
            float vC = varCol.Val;
            int i = MathJ.SearchOrdered(row, vR);
            int j = MathJ.SearchOrdered(col, vC);
            switch (i, j) {
                case (-1, -1):
                    return value[0, 0];
                case (-1, -2):
                    return value[0, col.Length - 1];
                case (-2, -1):
                    return value[row.Length - 1, 0];
                case (-2, -2):
                    return value[row.Length - 1, col.Length - 1];
            }
            if (i == -1 || i == -2) {
                int idx = (i == -1) ? 0 : row.Length - 1;
                float a = (vC - col[j]) * rColDiff[j];
                return value[idx, j] + (value[idx, j + 1] - value[idx, j]) * a;
            }
            if (j == -1 || j == -2) {
                int idx = (j == -1) ? 0 : col.Length - 1;
                float a = (vR - row[i]) * rRowDiff[i];
                return value[i, idx] + (value[i + 1, idx] - value[i, idx]) * a;
            }
            float alpha = (vR - row[i]) * rRowDiff[i];
            float beta = (vC - col[j]) * rColDiff[j];
            float a10 = value[i + 1, j] - value[i, j];
            float a01 = value[i, j + 1] - value[i, j];
            float a11 = value[i + 1, j + 1] + value[i, j] - (value[i + 1, j] + value[i, j + 1]);
            // bilinear interpolation
            return value[i, j] + a10 * alpha + a01 * beta + a11 * alpha * beta;
        }

        public override Function[] Dependency() {
            return new Function[] { new PropOrValue(varRow), new PropOrValue(varCol) };
        }
    }

    class BoolOperator : Function {
        public Function first, second;

        public BoolOperator(FuncType t, Function l, Function r) : base(t) {
            first = l;
            second = r;
        }

        bool Operate(float l, float r) {
            switch (fType) {
                case FuncType.lt:
                    return l < r;
                case FuncType.le:
                    return l <= r;
                case FuncType.gt:
                    return l > r;
                case FuncType.ge:
                    return l >= r;
                case FuncType.eq:
                    return Math.Abs(l - r) < 1e-6;
                case FuncType.not:
                    return l <= 0;
                default:
                    return false;
            }
        }

        public override float Eval() {
            return Operate(first.Eval(), second.Eval()) ? 1 : 0;
        }

        public override Function[] Dependency() {
            return new Function[] { first, second };
        }
    }

    class BinaryOperator : Function {
        public Function first, second;

        public BinaryOperator(FuncType t, Function l, Function r) : base(t) {
            first = l;
            second = r;
        }

        public static float Zero(FuncType t) {
            switch (t) {
                case FuncType.quotient:
                    return 1;
                case FuncType.difference:
                default:
                    return 0;
            }
        }

        float Operate(float l, float r) {
            switch (fType) {
                case FuncType.pow:
                    return (float)Math.Pow(l, r);
                case FuncType.quotient:
                    return l / r;
                case FuncType.difference:
                    return l - r;
                default:
                    return 0;
            }
        }

        public override float Eval() {
            return Operate(first.Eval(), second.Eval());
        }

        public override Function[] Dependency() {
            return new Function[] { first, second };
        }
    }

    class UnaryOperator : Function {
        public Function f;

        public UnaryOperator(FuncType t, Function f) : base(t) {
            this.f = f;
        }

        float Operate(float v) {
            switch (fType) {
                case FuncType.abs:
                    return (float)Math.Abs(v);
                case FuncType.acos:
                    return (float)Math.Acos(v);
                case FuncType.asin:
                    return (float)Math.Asin(v);
                case FuncType.atan:
                    return (float)Math.Atan(v);
                case FuncType.cos:
                    return (float)Math.Cos(v);
                case FuncType.fraction:
                    return v % 1f;
                case FuncType.integer:
                    return (int)v;
                case FuncType.sin:
                    return (float)Math.Sin(v);
                case FuncType.tan:
                    return (float)Math.Tan(v);
                default:
                    return 0;
            }
        }

        public override float Eval() {
            return Operate(f.Eval());
        }

        public override Function[] Dependency() {
            return new Function[] { f };
        }
    }

    class IfThen : Function {
        public Function[] func;

        public IfThen(Function a, Function b, Function c) : base(FuncType.product) {
            func = new Function[] { a, b, c };
        }

        public override float Eval() {
            return func[0].Eval() > 0 ? func[1].Eval() : func[2].Eval();
        }

        public override Function[] Dependency() {
            return func;
        }
    }
}
