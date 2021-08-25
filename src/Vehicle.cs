using System.Numerics;
namespace MinimalJSim {
    public class Vehicle {
        // metrics
        public Property WingArea, WingSpan, WingIncidence, Chord;
        public Property HTailArea, HTailArm, VTailArea, VTailArm;
        public Location[] locations;

        // inertia
        public Matrix4x4 inertiaMatrix;
        public Vector3 inertiaTensor;
        public Quaternion massFrame;
        public Vector3 centerOfMass;
        public Property emptyWeight;
    }

    public class Location {
        public string name;
        public Vector3 loc;
        public Location(string _name, float x, float y, float z) { name = _name; loc = new Vector3(x, y, z); }
    }
}