using System;
namespace MinimalJSim {
    public class Aero {
        public Property alpha, beta;
        public Property bi2vel, ci2vel; // span / (2*vel), chord / (2*vel)
        public Property kCLge, hbMac; // ground effect coefficient, height of MAC above ground over mean-air-chord
        public Property rho, pressure, temperature, qbar;
        public Property mach;

        public Function fnKCLge;

        public void UpdateProperty(DynamicsModel model) {
            alpha.Value = (float)Math.Atan2(model.motion.vel.Z, model.motion.vel.X);
            beta.Value = (float)Math.Atan2(-model.motion.vel.Y, model.motion.vel.X);

            float twoVel = model.motion.vel.Length() * 2;
            if (twoVel != 0) {
                bi2vel.Value = model.vehicle.WingSpan.Value / twoVel;
                ci2vel.Value = model.vehicle.Chord.Value / twoVel;
            }

            (pressure.Value, rho.Value, temperature.Value) = Atmosphere.GetPressureDensityTemp(model.motion.alt.Value);
            qbar.Value = rho.Value * model.motion.vel.LengthSquared() / 2;
            mach.Value = (float)model.motion.vel.Length() / Atmosphere.GetSoundSpeed(model.aero.temperature.Value);

            hbMac.Value = (model.motion.alt.Value - model.motion.terrainAlt.Value) / model.vehicle.Chord.Value;
            kCLge.Value = fnKCLge.Eval();
        }
    }
}