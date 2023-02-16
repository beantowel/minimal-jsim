using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;

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

        public static PropName Parse(string identifier) {
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

    public class Property {
        float value;
        public float Val {
            get => value;
            set => this.value = value;
        }

        public Property() { }

        public Property(PropName name) {
            value = name.nodes[0] == "tune" ? 1 : 0;
        }

        public override string ToString() {
            return $"MinimalJSim.Property({value})";
        }
    }


    public struct Axis {
        public List<Function> functions;
    }

    public class DModel {
        public SortedDictionary<string, Property> properties;
        public SortedDictionary<string, Function> functions;
        public Axis[] axes;
        public Vehicle vehicle;
        public Motion motion;
        public FlightControlSys fcs;
        public Aero aero;

        [JsonIgnore]
        Dictionary<Property, string> prop2Name; // debug

        public DModel() {
            properties = new SortedDictionary<string, Property>();
            functions = new SortedDictionary<string, Function>();
            const int n = (int)AxisDimension.Dummy + 1;
            axes = new Axis[n];
            for (int i = 0; i < n; i++) {
                axes[i] = new Axis { functions = new List<Function>() };
            }
        }

        public (Vector3 force, Vector3 torque) Eval() {
            var force = new Vector3(
                -EvalAxis(AxisDimension.Drag),
                EvalAxis(AxisDimension.Side),
                -EvalAxis(AxisDimension.Lift));
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
                x += v;
                // if (d == AxisDimension.Side) {
                //     Logger.Debug($"axis={d}, f={f.identifier}, v={v}");
                //     foreach (var p in f.DependProps()) {
                //         Logger.Debug($"{prop2Name[p]}={p.Val}");
                //     }
                // }
            }
            return x;
        }

        public Property GetProperty(string identifier) {
            return GetProperty(PropName.Parse(identifier));
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
            var name = PropName.Parse(identifier);
            if (!properties.ContainsKey(name.Key)) {
                Logger.Warn($"prop={identifier} not found");
                return;
            }
            properties[name.Key].Val = value;
        }

        public void UpdateProperty() {
            motion.UpdateProperty(this);
            aero.UpdateProperty(this);
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

        public void BuildLUT() {
            prop2Name = new Dictionary<Property, string>(properties.Count);
            foreach (var kv in properties) {
                prop2Name[kv.Value] = kv.Key;
            }
        }
    }
}
