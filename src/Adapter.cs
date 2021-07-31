using System;
using System.Numerics;
using System.Collections.Generic;

namespace MinimalJSim {
    public partial class Adapter {
        public static fdm_config Deserialize(string path) {
            System.Xml.Serialization.XmlSerializer reader =
                new System.Xml.Serialization.XmlSerializer(typeof(fdm_config));
            System.IO.StreamReader file = new System.IO.StreamReader(path);
            fdm_config config = (fdm_config)reader.Deserialize(file);
            file.Close();
            return config;
        }

        public static DynamicsModel Parse(fdm_config conf) {
            Logger.Debug("start parse model");

            var model = new DynamicsModel();
            // inertia tensor
            model.inertiaTensor = InertiaTensor(InertiaMatrix(conf), out model.massFrame);
            // functions
            foreach (var func in conf.aerodynamics.function) {
                Function f = ParseFunction(model, func);
                model.AddFunction(f, AxisDimension.Dummy);
            }
            foreach (var axis in conf.aerodynamics.axis) {
                AxisDimension i = ParseAxis(axis.name);
                foreach (var func in axis.function) {
                    Function f = ParseFunction(model, func);
                    model.AddFunction(f, i);
                }
            }
            // metrics, aero, velocities
            model.metrics = ParseMetrics(conf, model);
            model.aero = ParseAero(model);
            model.atmosphere = ParseAtmosphere(model);
            model.velocities = ParseVelocities(model);
            model.fcs = ParseFCS(model);
            // debug
            foreach (Function func in model.functions.Values) {
                Logger.Debug("func={0}, {1}", func.identifier, func.description);
                Logger.DebugObj("dependency", func.Dependency());
            }
            foreach (Property prop in model.properties.Values) {
                Logger.Debug("prop={0}, root={1}, end={2}, sub={3}, unit={4}, val={5}",
                prop.identifier, prop.RootNode(), prop.EndNode(), prop.SubNodes(), prop.Unit(), prop.value);
            }
            return model;
        }

        public static Matrix4x4 InertiaMatrix(fdm_config conf) {
            mass_balance m = conf.mass_balance;
            Matrix4x4 mat = new Matrix4x4(
                (float)m.ixx.Value, (float)m.ixy.Value, (float)m.ixz.Value, 0,
                (float)m.ixy.Value, (float)m.iyy.Value, (float)m.iyz.Value, 0,
                (float)m.ixz.Value, (float)m.iyz.Value, (float)m.izz.Value, 0,
                0, 0, 0, 0);
            return mat * Units.ToMetric(m.ixx.unit);
        }

        public static Vector3 InertiaTensor(in Matrix4x4 inertiaMat, out Quaternion massFrame) {
            return MathUtil.Diagonalize(inertiaMat, out massFrame);
        }

        static Metrics ParseMetrics(fdm_config conf, DynamicsModel model) {
            metrics cm = conf.metrics;
            Metrics m = new Metrics();
            if (cm.wingarea is not null) {
                m.WingArea = model.GetProperty("metrics/Sw-1?");
                m.WingArea.value = (float)cm.wingarea.Value * Units.ToMetric(cm.wingarea.unit);
            }
            if (cm.wingspan is not null) {
                m.WingSpan = model.GetProperty("metrics/bw-1?");
                m.WingSpan.value = (float)cm.wingspan.Value * Units.ToMetric(cm.wingspan.unit);
            }
            // if (cm.wing_incidence is not null) {
            //     m.WingArea = model.GetProperty("metrics/bw");
            //     m.WingIncidence = (float)cm.wing_incidence.Value * Units.ToMetric(cm.wing_incidence.unit);
            // }
            if (cm.chord is not null) {
                m.Chord = model.GetProperty("metrics/cbarw-1?");
                m.Chord.value = (float)cm.chord.Value * Units.ToMetric(cm.chord.unit);
            }
            if (cm.htailarea is not null) {
                m.HTailArea = model.GetProperty("Sh-1?");
                m.HTailArea.value = (float)cm.htailarea.Value * Units.ToMetric(cm.htailarea.unit);
            }
            if (cm.htailarm is not null) {
                m.HTailArm = model.GetProperty("lh-1?");
                m.HTailArm.value = (float)cm.htailarm.Value * Units.ToMetric(cm.htailarm.unit);
            }
            if (cm.vtailarea is not null) {
                m.VTailArea = model.GetProperty("Sv-1?");
                m.VTailArea.value = (float)cm.vtailarea.Value * Units.ToMetric(cm.vtailarea.unit);
            }
            if (cm.vtailarm is not null) {
                m.VTailArm = model.GetProperty("lv-1?");
                m.VTailArm.value = (float)cm.vtailarm.Value * Units.ToMetric(cm.vtailarm.unit);
            }

            m.locations = new Location[cm.location.Length];
            for (int i = 0; i < cm.location.Length; i++) {
                location loc = cm.location[i];
                float a = Units.ToMetric(loc.unit);
                m.locations[i] = new Location(loc.name, (float)loc.x * a, (float)loc.y * a, (float)loc.z * a);
            }
            return m;
        }

        static Aero ParseAero(DynamicsModel model) {
            Aero aero = new Aero();
            aero.alpha = model.GetProperty("aero/alpha-1?");
            aero.beta = model.GetProperty("aero/beta-1?");
            aero.bi2vel = model.GetProperty("aero/bi2vel-1?");
            aero.ci2vel = model.GetProperty("aero/ci2vel-1?");
            aero.kCLge = model.GetProperty("aero/function/kCLge-1?");
            aero.hbMac = model.GetProperty("aero/h_b-mac-1?");
            aero.qbar = model.GetProperty("aero/qbar-1?");
            aero.fnKCLge = model.GetFunction("aero/function/kCLge");
            return aero;
        }

        static Atmosphere ParseAtmosphere(DynamicsModel model) {
            Atmosphere a = new Atmosphere();
            a.rho = model.GetProperty("atmosphere/rho-1?");
            return a;
        }

        static Velocities ParseVelocities(DynamicsModel model) {
            Velocities v = new Velocities();
            v.mach = model.GetProperty("velocities/mach-1?");
            v.p = model.GetProperty("velocities/p-aero-1?");
            v.q = model.GetProperty("velocities/q-aero-1?");
            v.r = model.GetProperty("velocities/r-aero-1?");
            return v;
        }

        static FlightControlSys ParseFCS(DynamicsModel model) {
            FlightControlSys f = new FlightControlSys();
            f.aileronPos = model.GetProperty("fcs/aileron-pos-1?");
            f.elevatorPos = model.GetProperty("fcs/elevator-pos-1?");
            f.rudderPos = model.GetProperty("fcs/rudder-pos-1?");
            f.lefPos = model.GetProperty("fcs/lef-pos-1?");
            f.flaperonMix = model.GetProperty("fcs/flaperon-mix-1?");
            f.speedBrakePos = model.GetProperty("fcs/speedbrake-pos-1?");
            f.gearPos = model.GetProperty("gear/gear-pos-1?");
            return f;
        }

        static AxisDimension ParseAxis(string name) {
            switch (name) {
                case "DRAG":
                    return AxisDimension.Drag;
                case "SIDE":
                    return AxisDimension.Side;
                case "LIFT":
                    return AxisDimension.Lift;
                case "ROLL":
                    return AxisDimension.Roll;
                case "PITCH":
                    return AxisDimension.Pitch;
                case "YAW":
                    return AxisDimension.Yaw;
                default:
                    Logger.Error("invalid axis name={0}", name);
                    return AxisDimension.Dummy;
            }
        }

        static Function ParseFunction(DynamicsModel model, function func) {
            Function f = ParseFunctionGroup(model, func.Item);
            if (f is not null) {
                f.identifier = func.name;
                f.description = func.description;
            }
            return f;
        }

        static Function ParseFunctionGroup(DynamicsModel model, object obj) {
            switch (obj) {
                case product o:
                    return ParseFunctionGroup(model, o);
                case table o:
                    return ParseFunctionGroup(model, o);
                default:
                    Logger.Error("unknown function type", obj);
                    return null;
            }
        }

        static Function ParseFunctionGroup(DynamicsModel model, product obj) {
            Product p = new Product();
            List<Property> properties = new List<Property>();
            List<Function> functions = new List<Function>();
            foreach (var item in obj.Items) {
                switch (item) {
                    case double d:
                        p.value *= (float)d;
                        break;
                    case string s:
                        properties.Add(model.GetProperty(s));
                        break;
                    default:
                        Function func = ParseFunctionGroup(model, item);
                        if (func is not null) {
                            functions.Add(func);
                        }
                        break;
                }
            }
            p.properties = properties.ToArray();
            p.functions = functions.ToArray();
            return p;
        }

        static Function ParseFunctionGroup(DynamicsModel model, table obj) {
            switch (obj.independentVar.Length) {
                case 1:
                    Table1 t1 = new Table1();
                    t1.var = model.GetProperty(obj.independentVar[0].Text[0]);
                    ParseTableData(obj.tableData[0].Value, out t1.row, out t1.value);
                    t1.InitDiff();
                    return t1;
                case 2:
                    Table2 t2 = new Table2();
                    foreach (independentVar v in obj.independentVar) {
                        switch (v.lookup) {
                            case "row":
                                t2.varRow = model.GetProperty(v.Text[0]);
                                break;
                            case "column":
                                t2.varCol = model.GetProperty(v.Text[0]);
                                break;
                            default:
                                Logger.Error("unkown table lookup type", v.lookup);
                                break;
                        }
                    }
                    ParseTableData(obj.tableData[0].Value, out t2.row, out t2.col, out t2.value);
                    t2.InitDiff();
                    return t2;
                default:
                    Logger.Error("unkown table type, len(vars)={0}", obj.independentVar.Length);
                    break;
            }
            return null;
        }

        static void ParseTableData(string data, out float[] row, out float[] v) {
            string[] lines = data.Trim().Split('\n');
            row = new float[lines.Length];
            v = new float[lines.Length];
            for (int i = 0; i < lines.Length; i++) {
                string[] arr = lines[i].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                row[i] = float.Parse(arr[0]);
                v[i] = float.Parse(arr[1]);
            }
        }

        static void ParseTableData(string data, out float[] row, out float[] col, out float[,] v) {
            string[] lines = data.Trim().Split('\n');
            string[] arr = lines[0].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int n = lines.Length - 1;
            int m = arr.Length;
            row = new float[n];
            col = new float[m];
            v = new float[n, m];
            for (int j = 0; j < m; j++) {
                col[j] = float.Parse(arr[j]);
            }
            for (int i = 0; i < n; i++) {
                arr = lines[i + 1].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                row[i] = float.Parse(arr[0]);
                for (int j = 0; j < m; j++) {
                    v[i, j] = float.Parse(arr[j + 1]);
                }
            }
        }
    }

}