using System;
using System.Collections.Generic;
using System.Numerics;

namespace MinimalJSim {
    public struct PropName {
        public string identifier;
        public string[] nodes;
        public string unit;
        public bool isArray;
        public int index;

        public string Key => string.Join("/", nodes) + indexer;
        public string MetricIdentifier => string.Join("/", nodes) + "-1?" + indexer;
        string indexer => isArray ? $"[{index}]" : "";
    }

    public class Property {
        public string identifier;
        float value;
        public float Val {
            get => value;
            set => this.value = value;
        }

        public Property(PropName name) {
            this.identifier = name.identifier;
            value = name.nodes[0] == "tune" ? 1 : 0;
        }

        public PropName ParseName() {
            return ParseName(identifier);
        }

        public override string ToString() {
            return $"MinimalJSim.Property({identifier}, {value})";
        }

        public static PropName ParseName(string identifier) {
            var s = identifier;
            int j = s.LastIndexOf('['); // array
            int index = (j != -1) ? Int32.Parse(s.Substring(j + 1).TrimEnd(']')) : 0;
            s = (j != -1) ? s.Substring(0, j) : s;

            var nodes = s.Split('/');
            string endNode = nodes[nodes.Length - 1];
            int i = endNode.LastIndexOf('-');
            var unit = (i != -1) ? endNode.Substring(i + 1) : "";
            endNode = (i != -1) ? endNode.Substring(0, i) : endNode;
            nodes[nodes.Length - 1] = endNode;
            return new PropName {
                identifier = identifier,
                nodes = nodes,
                unit = unit,
                isArray = (j != -1),
                index = index,
            };
        }
    }


    public struct Axis {
        public List<Function> functions;
    }

    public class DynamicsModel {
        public SortedDictionary<string, Property> properties; // property's key should not contain unit
        public SortedDictionary<string, Function> functions;
        public Axis[] axes;
        public Vehicle vehicle;
        public Motion motion;
        public FlightControlSys fcs;
        public Aero aero;

        public DynamicsModel() {
            properties = new SortedDictionary<string, Property>();
            functions = new SortedDictionary<string, Function>();
            axes = new Axis[7];
            for (int i = 0; i < 7; i++) {
                axes[i] = new Axis { functions = new List<Function>() };
            }
        }

        public (Vector3 force, Vector3 torque) Eval() {
            var force = new Vector3(
                EvalAxis(AxisDimension.Drag),
                EvalAxis(AxisDimension.Side),
                EvalAxis(AxisDimension.Lift));
            var torque = new Vector3(
                EvalAxis(AxisDimension.Roll),
                EvalAxis(AxisDimension.Pitch),
                EvalAxis(AxisDimension.Yaw));
            return (force, torque);
        }


        public float EvalAxis(AxisDimension d) {
            var axis = axes[(int)d];
            float x = 0;
            foreach (Function f in axis.functions) {
                var v = f.Eval();
                // Logger.Debug($"axis={d}, f={f.identifier}, v={v}");
                // foreach (var name in f.DependProps()) {
                //     Logger.Debug($"prop={GetProperty(name)}");
                // }
                x += v;
            }
            return x;
        }

        public Property GetProperty(property prop) {
            var p = GetProperty(prop.Value);
            if (prop.valueSpecified) {
                SetProperty(p.identifier, (float)prop.value);
            }
            return p;
        }

        public Property GetProperty(string identifier) {
            return GetProperty(Property.ParseName(identifier));
        }

        public Property GetProperty(PropName name) {
            Property p;
            if (properties.TryGetValue(name.Key, out p)) {
                return p;
            }
            p = new Property(name);
            properties.Add(name.Key, p);
            return p;
        }

        public void SetProperty(string identifier, float value) {
            var name = Property.ParseName(identifier);
            if (!properties.ContainsKey(name.Key)) {
                Logger.Warn($"prop={identifier} not found");
                return;
            }
            properties[name.Key].Val = value;
        }

        public void UpdateProperty(float deltaT) {
            motion.UpdateProperty(this);
            aero.UpdateProperty(this, deltaT);
        }

        public void AddFunction(Function f, AxisDimension axis) {
            axes[(int)axis].functions.Add(f);
            functions[f.identifier] = f;
        }

        public Function GetFunction(string identifier) {
            if (!functions.ContainsKey(identifier)) {
                Logger.Warn($"function={identifier} not found");
                return null;
            }
            return functions[identifier];
        }
    }
}
