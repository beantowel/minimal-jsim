using System;
using System.Collections.Generic;
using System.Numerics;

namespace MinimalJSim {
    public static class Parser {
        public static DModel Parse(fdm_config conf) {
            Logger.Debug("start parse model");

            var model = new DModel();
            // functions
            foreach (var func in conf.aerodynamics.function ?? new function[] { }) {
                // dummy axis
                Function f = ParseFunction(model, func);
                model.AddFunction(f, MinimalJSim.AxisDimension.Dummy);
            }
            foreach (var axis in conf.aerodynamics.axis) {
                AxisDimension i = AxisDimension(axis.name);
                foreach (var func in axis.function) {
                    Function f = ParseFunction(model, func);
                    model.AddFunction(f, i);
                }
            }
            // inertia, metrics, aero, velocities
            model.vehicle = new Vehicle(model, conf.metrics, conf.mass_balance);
            model.motion = new Motion(model);
            model.fcs = new FlightControlSys(model);
            model.aero = new Aero(model);
            return model;
        }

        static AxisDimension AxisDimension(string name) {
            switch (name) {
                case "DRAG":
                    return MinimalJSim.AxisDimension.Drag;
                case "SIDE":
                    return MinimalJSim.AxisDimension.Side;
                case "LIFT":
                    return MinimalJSim.AxisDimension.Lift;
                case "ROLL":
                    return MinimalJSim.AxisDimension.Roll;
                case "PITCH":
                    return MinimalJSim.AxisDimension.Pitch;
                case "YAW":
                    return MinimalJSim.AxisDimension.Yaw;
                default:
                    Logger.Error($"invalid axis name={name}");
                    return MinimalJSim.AxisDimension.Dummy;
            }
        }

        static Function ParseFunction(DModel model, function func) {
            Function f = ParseFunctionGroup(model, func.Item);
            if (f != null) {
                f.identifier = func.name;
                f.description = func.description;
            }
            return f;
        }

        static Function ParseFunctionGroup(DModel model, object obj) {
            switch (obj) {
                case product o:
                    return ParseCommutative(model, FuncType.product, o.Items);
                case sum o:
                    return ParseCommutative(model, FuncType.sum, o.Items);
                case avg o:
                    return ParseCommutative(model, FuncType.avg, o.Items);
                case table o:
                    return ParseTable(model, o);
                case abs o:
                    return ParseUnary(model, FuncType.abs, o.Item);
                case cos o:
                    return ParseUnary(model, FuncType.cos, o.Item);
                case quotient o:
                    return ParseBinary(model, FuncType.quotient, o.Items);
                case lt o:
                    return ParseBool(model, FuncType.lt, o.Items);
                case property o:
                    return ParseProp(model, o);
                case double o:
                    return new PropOrValue((float)o);
                case ifthen o:
                    return new IfThen(
                        ParseFunctionGroup(model, o.Items[0]),
                        ParseFunctionGroup(model, o.Items[1]),
                        ParseFunctionGroup(model, o.Items[2]));
                default:
                    Logger.ErrorObj($"unknown function type={obj.GetType()}", obj);
                    return null;
            }
        }

        static Function ParseProp(DModel model, property p) {
            var prop = model.GetProperty(p.Value);
            if (p.valueSpecified) {
                model.SetProperty(p.Value, (float)p.value);
            }
            return new PropOrValue(prop);
        }

        static Function ParseUnary(DModel model, FuncType typ, object obj) {
            return new UnaryOperator(typ, ParseFunctionGroup(model, obj));
        }

        static Function ParseBinary(DModel model, FuncType typ, object[] objs) {
            Function l, r;
            if (objs.Length == 1) {
                l = new PropOrValue(BinaryOperator.Zero(typ));
                r = ParseFunctionGroup(model, objs[0]);
            } else {
                l = ParseFunctionGroup(model, objs[0]);
                r = ParseFunctionGroup(model, objs[1]);
            }
            return new BinaryOperator(typ, l, r);
        }

        static Function ParseBool(DModel model, FuncType typ, object[] objs) {
            return new BoolOperator(typ,
                ParseFunctionGroup(model, objs[0]), ParseFunctionGroup(model, objs[1]));
        }

        static Function ParseCommutative(DModel model, FuncType typ, object[] objs) {
            List<Function> functions = new List<Function>();
            foreach (var obj in objs) {
                Function func = ParseFunctionGroup(model, obj);
                if (func != null) {
                    functions.Add(func);
                }
            }
            return new CommutativeOperator(typ, functions);
        }

        static Function ParseTable(DModel model, table obj) {
            switch (obj.independentVar.Length) {
                case 1:
                    Table1 t1 = new Table1();
                    var name = PropName.Parse(obj.independentVar[0].Text[0]);
                    t1.var = model.GetProperty(name);
                    ParseTableData(obj.tableData[0].Value, out t1.row, out t1.value);
                    t1.Init(Units.ToMetric(name.unit));
                    return t1;
                case 2:
                    Table2 t2 = new Table2();
                    PropName rName, cName;
                    ParseTableVar(model, obj.independentVar, out rName, out cName);
                    t2.varRow = model.GetProperty(rName);
                    t2.varCol = model.GetProperty(cName);
                    ParseTableData(obj.tableData[0].Value, out t2.row, out t2.col, out t2.value);
                    t2.Init(Units.ToMetric(rName.unit), Units.ToMetric(cName.unit));
                    return t2;
                default:
                    Logger.Error($"unknown table type, len(vars)={obj.independentVar.Length}");
                    break;
            }
            return null;
        }

        static void ParseTableVar(DModel model, independentVar[] vars, out PropName row, out PropName col) {
            row = PropName.Parse("not_found");
            col = PropName.Parse("not_found");
            foreach (var v in vars) {
                switch (v.lookup) {
                    case "row":
                        row = PropName.Parse(v.Text[0]);
                        break;
                    case "column":
                        col = PropName.Parse(v.Text[0]);
                        break;
                    default:
                        Logger.Error($"unknown table lookup type={v.lookup}");
                        break;
                }
            }
        }

        static void ParseTableData(string data, out float[] row, out float[] v) {
            string[] lines = data.Trim().Split('\n');
            row = new float[lines.Length];
            v = new float[lines.Length];
            char[] sep = { ' ', '\t' };
            for (int i = 0; i < lines.Length; i++) {
                string[] arr = lines[i].Trim().Split(sep, StringSplitOptions.RemoveEmptyEntries);
                row[i] = float.Parse(arr[0]);
                v[i] = float.Parse(arr[1]);
            }
        }

        static void ParseTableData(string data, out float[] row, out float[] col, out float[,] v) {
            string[] lines = data.Trim().Split('\n');
            char[] sep = { ' ', '\t' };
            string[] arr = lines[0].Trim().Split(sep, StringSplitOptions.RemoveEmptyEntries);
            int n = lines.Length - 1;
            int m = arr.Length;
            row = new float[n];
            col = new float[m];
            v = new float[n, m];
            for (int j = 0; j < m; j++) {
                col[j] = float.Parse(arr[j]);
            }
            for (int i = 0; i < n; i++) {
                arr = lines[i + 1].Trim().Split(sep, StringSplitOptions.RemoveEmptyEntries);
                row[i] = float.Parse(arr[0]);
                for (int j = 0; j < m; j++) {
                    v[i, j] = float.Parse(arr[j + 1]);
                }
            }
        }
    }
}
