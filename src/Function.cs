using System;
using System.Collections.Generic;

namespace MinimalJSim {
    public enum FuncType {
        Table,
        Product,
        Difference,
        Sum,
        Quotient,
        Pow,
        Abs,
        Sin,
        Cos,
        Tan,
        Asin,
        Acos,
        Atan,
        Atan2,
        Min,
        Max,
        Avg,
        Fraction,
        Integer,
        Mod,
        Random,
    }

    enum ItemType {
        Value,
        Property,
        Function,
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
        public abstract List<string> Dependency();
    }

    class Product : Function {
        public Property[] properties;
        public Function[] functions;
        public float value;

        public Product() : base(FuncType.Product) {
            value = 1;
        }

        public override float Eval() {
            float v = value;
            foreach (Function func in functions) {
                v *= func.Eval();
            }
            foreach (Property prop in properties) {
                v *= prop.Value;
            }
            return v;
        }

        public override List<string> Dependency() {
            List<string> d = new List<string>();
            foreach (Property p in properties) {
                d.Add(p.identifier);
            }
            foreach (Function f in functions) {
                d.AddRange(f.Dependency());
            }
            return d;
        }
    }

    class Table1 : Function {
        public Property var;
        public float[] row, value;
        float[] rRowDiff;

        public Table1() : base(FuncType.Table) { }

        public Table1(in float[] _row, in float[] _value) : base(FuncType.Table) {
            row = _row;
            value = _value;
            Init();
        }

        public void Init() {
            MathUtil.ScaleArray(row, Units.ToMetric(var));
            rRowDiff = MathUtil.ReciprocalSeqDiff(row);
        }

        public override float Eval() {
            return Eval(var.Value);
        }

        public float Eval(float v) {
            int i = MathUtil.SearchOrdered(row, v);
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

        public override List<string> Dependency() {
            return new List<string>() { var.identifier };
        }
    }

    class Table2 : Function {
        public Property varRow, varCol;
        public float[] row, col;
        public float[,] value;
        float[] rRowDiff, rColDiff;

        public Table2() : base(FuncType.Table) { }

        public void Init() {
            MathUtil.ScaleArray(row, Units.ToMetric(varRow));
            MathUtil.ScaleArray(col, Units.ToMetric(varCol));
            rRowDiff = MathUtil.ReciprocalSeqDiff(row);
            rColDiff = MathUtil.ReciprocalSeqDiff(col);
        }

        public override float Eval() {
            float vR = varRow.Value;
            float vC = varCol.Value;
            int i = MathUtil.SearchOrdered(row, vR);
            int j = MathUtil.SearchOrdered(col, vC);
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

        public override List<string> Dependency() {
            return new List<string>() { varRow.identifier, varCol.identifier };
        }
    }

    // class UnaryOperator : Function {
    //     public ItemType itemType;
    //     public float value;
    //     public Property property;
    //     public Function f;

    //     public UnaryOperator(FuncType t) : base(t) { }

    //     public float Operate(float v) {
    //         switch (fType) {
    //             case FuncType.Abs:
    //                 return (float)Math.Abs(v);
    //             case FuncType.Acos:
    //                 return (float)Math.Acos(v);
    //             case FuncType.Asin:
    //                 return (float)Math.Asin(v);
    //             case FuncType.Atan:
    //                 return (float)Math.Atan(v);
    //             case FuncType.Cos:
    //                 return (float)Math.Cos(v);
    //             case FuncType.Fraction:
    //                 return 1 / v;
    //             case FuncType.Integer:
    //                 return (int)v;
    //             case FuncType.Sin:
    //                 return (float)Math.Sin(v);
    //             case FuncType.Tan:
    //                 return (float)Math.Tan(v);
    //             default:
    //                 return 0;
    //         }
    //     }

    //     public override float Eval() {
    //         switch (itemType) {
    //             case ItemType.Value:
    //                 return value;
    //             case ItemType.Property:
    //                 return Operate(property.Value);
    //             case ItemType.Function:
    //                 return Operate(f.Eval());
    //             default:
    //                 return 0;
    //         }
    //     }

    //     public override List<string> Dependency() {
    //         switch (itemType) {
    //             case ItemType.Property:
    //                 return new List<string>() { property.identifier };
    //             case ItemType.Function:
    //                 return f.Dependency();
    //             case ItemType.Value:
    //             default:
    //                 return new List<string>();
    //         }
    //     }
    // }
}
