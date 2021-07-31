namespace MinimalJSim {
    public class Aero {
        public Property alpha, beta;
        public Property bi2vel, ci2vel; // span / (2*vel), chord / (2*vel)
        public Property kCLge, hbMac; // ground effect coefficient, height of MAC above ground over mean-air-chord
        public Property qbar;

        float hbGround; // height above ground;
        public Function fnKCLge;

        public void UpdateProperty(DynamicsModel model) {
            float v = model.velocities.vel.Length();
            bi2vel.value = model.metrics.WingSpan.value / 2 / v;
            ci2vel.value = model.metrics.Chord.value / 2 / v;
            qbar.value = model.atmosphere.rho.value * model.velocities.vel.LengthSquared() / 2;
            hbMac.value = hbGround / model.metrics.Chord.value;
            kCLge.value = fnKCLge.Eval();
        }
    }
}