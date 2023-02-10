using System;
using System.Numerics;
namespace MinimalJSim {
    public class Aero {
        public Property alpha, beta, alphaDot, betaDot;
        public Property bi2vel, ci2vel; // span / (2*vel), chord / (2*vel)
        public Property kCLge, hbMac; // ground effect coefficient, height of MAC above ground over mean-air-chord
        public Property rho, pressure, temperature, qbar, clSquare;
        public Property mach, Re;
        public Function fnKCLge;
        public Vector3 vel, force;

        public float IAS => (float)Math.Sqrt(2 * qbar.Val / Atmosphere.Density(0));

        public Vector3 forceWind => (body2Wind * force) * new Vector3(-1, 1, -1);

        Matrix3x3 body2Wind {
            get {
                float ca = (float)Math.Cos(alpha.Val);
                float sa = (float)Math.Sin(alpha.Val);
                float cb = (float)Math.Sin(beta.Val);
                float sb = (float)Math.Sin(beta.Val);
                var m = new Matrix3x3(
                    ca * cb, -ca * sb, -sa,
                    sb, cb, 0,
                    sa * cb, -sa * sb, ca
                );
                return Matrix3x3.Transpose(m);
            }
        }

        public Aero(DynamicsModel model) {
            // bind properties
            alpha = model.GetProperty("aero/alpha-1?");
            beta = model.GetProperty("aero/beta-1?");
            alphaDot = model.GetProperty("aero/alphadot-1?");
            betaDot = model.GetProperty("aero/betadot-1?");
            bi2vel = model.GetProperty("aero/bi2vel-1?");
            ci2vel = model.GetProperty("aero/ci2vel-1?");
            kCLge = model.GetProperty("aero/function/kCLge-1?");
            hbMac = model.GetProperty("aero/h_b-mac-1?");
            qbar = model.GetProperty("aero/qbar-1?");
            clSquare = model.GetProperty("aero/cl-squared-1?");
            Re = model.GetProperty("aero/Re-1?");
            fnKCLge = model.GetFunction("aero/function/kCLge");
            rho = model.GetProperty("atmosphere/rho-1?");
            pressure = model.GetProperty("atmosphere/pressure-1?");
            temperature = model.GetProperty("atmosphere/T-1?");
            mach = model.GetProperty("velocities/mach-1?");
        }

        public void UpdateProperty(DynamicsModel model, float deltaT) {
            var motion = model.motion;
            var vehicle = model.vehicle;
            float twoVel = vel.Length() * 2;
            twoVel = (twoVel == 0) ? 1 : twoVel;
            deltaT = (deltaT == 0) ? 1 : deltaT;

            float alpha0 = (float)Math.Atan2(vel.Z, vel.X);
            float beta0 = (float)Math.Atan2(-vel.Y, vel.X);
            alphaDot.Val = (alpha0 - alpha.Val) / deltaT;
            betaDot.Val = (beta0 - beta.Val) / deltaT;
            alpha.Val = alpha0;
            beta.Val = beta0;
            bi2vel.Val = vehicle.WingSpan.Val / twoVel;
            ci2vel.Val = vehicle.WingChord.Val / twoVel;

            (pressure.Val, temperature.Val) = Atmosphere.GetPressureTemp(motion.alt.Val);
            rho.Val = Atmosphere.Density(pressure.Val, temperature.Val);
            qbar.Val = rho.Val * vel.LengthSquared() / 2;
            clSquare.Val = qbar.Val > 1 ? forceWind.Z / (vehicle.WingArea.Val * qbar.Val) : clSquare.Val;
            mach.Val = (float)vel.Length() / Atmosphere.SoundSpeed(temperature.Val);
            float kinematicViscosity = Atmosphere.Viscosity(temperature.Val) / rho.Val;
            Re.Val = vel.Length() * vehicle.WingChord.Val / kinematicViscosity;

            hbMac.Val = (motion.alt.Val - motion.terrainAlt.Val) / vehicle.WingChord.Val;
            kCLge.Val = fnKCLge?.Eval() ?? 0;
        }
    }
}