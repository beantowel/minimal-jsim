using System;
using System.Numerics;

namespace MinimalJSim {
    public class Metrics {
        public Property WingArea, WingSpan, WingIncidence, Chord;
        public Property HTailArea, HTailArm, VTailArea, VTailArm;
        public Location[] locations;
    }

    public class Location {
        public string name;
        public Vector3 loc;

        public Location(string _name, float x, float y, float z) { name = _name; loc = new Vector3(x, y, z); }
    }
}