using System.Numerics;
using System.Collections.Generic;

namespace MinimalJSim {
    public class Property {
        public string identifier;
        public float value;

        public Property(string _identifier) { identifier = _identifier; value = 0; }

        public string RootNode() {
            int i = identifier.IndexOf('/');
            return (i != -1) ? identifier.Substring(0, i) : identifier;
        }

        public string EndNode() {
            return identifier.Substring(identifier.LastIndexOf('/') + 1);
        }

        public static string SubNodes(string node) {
            return node.Substring(node.IndexOf('/') + 1);
        }

        public string SubNodes() {
            return SubNodes(identifier);
        }

        public static string WithoutUnit(string identifier) {
            int i = identifier.LastIndexOf('-');
            return (i != -1) ? identifier.Substring(0, i) : identifier;
        }

        public string WithoutUnit() {
            return WithoutUnit(identifier);
        }

        public string Unit() {
            int index = identifier.LastIndexOf('-');
            if (index == -1) {
                return "";
            }
            return identifier.Substring(index + 1);
        }
    }


    public class Axis {
        public AxisDimension axisDim;
        public List<Function> functions;

        public Axis(AxisDimension d) { axisDim = d; functions = new List<Function>(); }

        public float Eval() {
            float x = 0;
            foreach (Function f in functions) {
                x += f.Eval();
            }
            return x;
        }
    }

    public class DynamicsModel {
        public Vector3 inertiaTensor;
        public Quaternion massFrame;
        public SortedDictionary<string, Property> properties; // property's key should not contain unit
        public SortedDictionary<string, Function> functions;
        Axis[] axes;
        public Metrics metrics;
        public Aero aero;
        public Atmosphere atmosphere;
        public Velocities velocities;
        public FlightControlSys fcs;

        public DynamicsModel() {
            properties = new SortedDictionary<string, Property>();
            functions = new SortedDictionary<string, Function>();
            axes = new Axis[7];
            for (int i = 0; i < 7; i++) {
                axes[i] = new Axis((AxisDimension)i);
            }
        }

        public Property GetProperty(string identifer) {
            Property p;
            string uIdent = Property.WithoutUnit(identifer);
            if (properties.TryGetValue(uIdent, out p)) {
                return p;
            }
            p = new Property(identifer);
            properties.Add(uIdent, p);
            return p;
        }

        public void AddFunction(Function f, AxisDimension axis) {
            axes[(int)axis].functions.Add(f);
            functions[f.identifier] = f;
        }

        public Function GetFunction(string identifer) {
            return functions[identifer];
        }

        public void UpdateProperty() {
            aero.UpdateProperty(this);
        }
    }
}
