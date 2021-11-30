namespace MAE459_Project.src {

    class Input {
        public double pressure;
        public double temperature;
        public double massFlow;
        public double density;
        public double area;
        public double velocity;

        public Input(double pressure, double temperature, double massFlow, double area, Fluid fluid) {
            this.pressure = pressure;
            this.temperature = temperature;
            this.massFlow = massFlow;
            this.area = area;
            this.density = pressure/(fluid.GasConstant*temperature);
            this.velocity = massFlow/(this.density*area);
        }
    }
}