using System;
using System.Numerics;
using System.Collections.Generic;

namespace MinimalJSim {
    public partial class DynamicsModel {
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
            // functions
            foreach (var func in conf.aerodynamics.function) {
                // dummy axis
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
            // inertia, metrics, aero, velocities
            model.vehicle = ParseVehicle(conf, model);
            model.motion = ParseMotion(model);
            model.fcs = ParseFCS(model);
            model.aero = ParseAero(model);
            return model;
        }





        static Vehicle ParseVehicle(fdm_config conf, DynamicsModel model) {
            Vehicle v = new Vehicle();
            metrics cm = conf.metrics;

            if (cm.wingarea != null) {
                v.WingArea = model.GetDefaultProperty("metrics/Sw-1?");
                v.WingArea.Value = (float)cm.wingarea.Value * Units.ToMetric(cm.wingarea.unit);
            }
            if (cm.wingspan != null) {
                v.WingSpan = model.GetDefaultProperty("metrics/bw-1?");
                v.WingSpan.Value = (float)cm.wingspan.Value * Units.ToMetric(cm.wingspan.unit);
            }
            if (cm.wing_incidence != null) {
                v.WingIncidence = model.GetDefaultProperty("metrics/iw-deg");
                v.WingIncidence.Value = (float)cm.wing_incidence.Value * Units.ToMetric(cm.wing_incidence.unit);
            }
            if (cm.chord != null) {
                v.Chord = model.GetDefaultProperty("metrics/cbarw-1?");
                v.Chord.Value = (float)cm.chord.Value * Units.ToMetric(cm.chord.unit);
            }
            if (cm.htailarea != null) {
                v.HTailArea = model.GetDefaultProperty("metrics/Sh-1?");
                v.HTailArea.Value = (float)cm.htailarea.Value * Units.ToMetric(cm.htailarea.unit);
            }
            if (cm.htailarm != null) {
                v.HTailArm = model.GetDefaultProperty("metrics/lh-1?");
                v.HTailArm.Value = (float)cm.htailarm.Value * Units.ToMetric(cm.htailarm.unit);
            }
            if (cm.vtailarea != null) {
                v.VTailArea = model.GetDefaultProperty("metrics/Sv-1?");
                v.VTailArea.Value = (float)cm.vtailarea.Value * Units.ToMetric(cm.vtailarea.unit);
            }
            if (cm.vtailarm != null) {
                v.VTailArm = model.GetDefaultProperty("metrics/lv-1?");
                v.VTailArm.Value = (float)cm.vtailarm.Value * Units.ToMetric(cm.vtailarm.unit);
            }

            v.locations = new Location[cm.location.Length];
            for (int i = 0; i < cm.location.Length; i++) {
                location loc = cm.location[i];
                v.locations[i] = new Location(loc.name, (float)loc.x, (float)loc.y, (float)loc.z);
                v.locations[i].loc *= Units.ToMetric(loc.unit);
            }

            mass_balance m = conf.mass_balance;
            Matrix4x4 mat = new Matrix4x4(
                (float)m.ixx.Value, (float)m.ixy.Value, (float)m.ixz.Value, 0,
                (float)m.ixy.Value, (float)m.iyy.Value, (float)m.iyz.Value, 0,
                (float)m.ixz.Value, (float)m.iyz.Value, (float)m.izz.Value, 0,
                0, 0, 0, 0);
            v.emptyWeight = model.GetDefaultProperty("inertia/empty-weight-1?");
            v.emptyWeight.Value = (float)m.emptywt.Value * Units.ToMetric(m.emptywt.unit);
            v.centerOfMass = new Vector3((float)m.location.x, (float)m.location.y, (float)m.location.z);
            v.centerOfMass *= Units.ToMetric(m.location.unit);
            v.inertiaMatrix = mat * Units.ToMetric(m.ixx.unit);
            (v.inertiaTensor, v.massFrame) = MathUtil.Diagonalize(v.inertiaMatrix);
            return v;
        }

        static Aero ParseAero(DynamicsModel model) {
            Aero aero = new Aero();
            aero.alpha = model.GetDefaultProperty("aero/alpha-1?");
            aero.beta = model.GetDefaultProperty("aero/beta-1?");
            aero.bi2vel = model.GetDefaultProperty("aero/bi2vel-1?");
            aero.ci2vel = model.GetDefaultProperty("aero/ci2vel-1?");
            aero.kCLge = model.GetDefaultProperty("aero/function/kCLge-1?");
            aero.hbMac = model.GetDefaultProperty("aero/h_b-mac-1?");
            aero.qbar = model.GetDefaultProperty("aero/qbar-1?");
            aero.fnKCLge = model.GetFunction("aero/function/kCLge");
            aero.rho = model.GetDefaultProperty("atmosphere/rho-1?");
            aero.pressure = model.GetDefaultProperty("atmosphere/pressure-1?");
            aero.temperature = model.GetDefaultProperty("atmosphere/T-1?");
            aero.mach = model.GetDefaultProperty("velocities/mach-1?");
            return aero;
        }

        static Motion ParseMotion(DynamicsModel model) {
            Motion v = new Motion();
            v.p = model.GetDefaultProperty("velocities/p-aero-1?");
            v.q = model.GetDefaultProperty("velocities/q-aero-1?");
            v.r = model.GetDefaultProperty("velocities/r-aero-1?");

            v.alt = model.GetDefaultProperty("position/h-sl-1?");
            v.terrainAlt = model.GetDefaultProperty("position/terrain-elevation-asl-1?");
            return v;
        }

        static FlightControlSys ParseFCS(DynamicsModel model) {
            FlightControlSys f = new FlightControlSys();
            f.aileronPos = model.GetDefaultProperty("fcs/aileron-pos-1?");
            f.elevatorPos = model.GetDefaultProperty("fcs/elevator-pos-1?");
            f.rudderPos = model.GetDefaultProperty("fcs/rudder-pos-1?");
            f.leadEdgeFlapPos = model.GetDefaultProperty("fcs/lef-pos-1?");
            f.flaperonMix = model.GetDefaultProperty("fcs/flaperon-mix-1?");
            f.speedBrakePos = model.GetDefaultProperty("fcs/speedbrake-pos-1?");
            f.gearPos = model.GetDefaultProperty("gear/gear-pos-1?");
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
            if (f != null) {
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
                        properties.Add(model.GetDefaultProperty(s));
                        break;
                    default:
                        Function func = ParseFunctionGroup(model, item);
                        if (func != null) {
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
                    t1.var = model.GetDefaultProperty(obj.independentVar[0].Text[0]);
                    ParseTableData(obj.tableData[0].Value, out t1.row, out t1.value);
                    t1.Init();
                    return t1;
                case 2:
                    Table2 t2 = new Table2();
                    foreach (independentVar v in obj.independentVar) {
                        switch (v.lookup) {
                            case "row":
                                t2.varRow = model.GetDefaultProperty(v.Text[0]);
                                break;
                            case "column":
                                t2.varCol = model.GetDefaultProperty(v.Text[0]);
                                break;
                            default:
                                Logger.Error("unkown table lookup type", v.lookup);
                                break;
                        }
                    }
                    ParseTableData(obj.tableData[0].Value, out t2.row, out t2.col, out t2.value);
                    t2.Init();
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