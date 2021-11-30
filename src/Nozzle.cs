using System;

namespace MAE459_Project.src {

    class Nozzle {

        private double exitArea = -1;
        private double throatArea = -1;
        

        public Nozzle(double exitDiameter) {
            this.exitArea = Math.PI * Math.Pow(exitDiameter/2, 2);
        }

        private double ComputeThroatArea(double massFlow, double totalTemperature, double totalPressure, double gamma, double R) {
            // choked flow assumption
            this.throatArea = (massFlow / totalPressure) * Math.Sqrt(R/gamma) * Math.Sqrt(totalTemperature);
            double temp = Math.Pow(1 + (gamma-1)/2, -1*(gamma + 1)/(2*gamma -2));
            this.throatArea /= temp;
            return this.throatArea;
        }

        public double[] NozzleFlow(double massFlow, Cell input, double ambientPressure) {
            double[] results = new double[4]; // cstar, CF, F, Isp
            ComputeThroatArea(massFlow, input.TotalTemperature, input.TotalPressure, input.Gamma, input.fluid.GasConstant);
            double exitMach = ExitMach(this.exitArea/this.throatArea, input.Gamma);
            double exitPressure = input.pressure / Math.Pow(1 + exitMach*exitMach*(input.Gamma -1)/2, input.Gamma / (input.Gamma - 1));

            results[0] = input.TotalPressure * this.throatArea / massFlow; // cstar
            results[1] = CalculateCF(input.Gamma, exitPressure, input.pressure, ambientPressure); // CF
            results[2] = results[1] * input.pressure * this.throatArea; // F
            results[3] = results[0] * results[1] / 9.81; // Isp
            return results;
        }

        // determines exit mach number assuming choked throat with a given expansion ratio and specific heat ratio
        public static double ExitMach(double expansionRatio, double gamma) {
            double step = 0.001;
            double closestDistance = -1;
            double exitMach = -1;
            for (double M = 1; M <= 5; M += step) {
                double distance = Math.Abs(expansionRatio - (1/M)*Math.Pow(2/(gamma + 1)*(1 + M*M*(gamma - 1)/2), (gamma + 1)/(2*gamma - 2)));
                if (distance < closestDistance || closestDistance < 0 || exitMach < 0) {
                    closestDistance = distance;
                    exitMach = M;
                }
            }
            return exitMach;
        }

        public static double CalculateCF(double gamma, double exitPressure, double chamberPressure, double ambientPressure) 
        {
            double GAMMA = Math.Pow(2/(gamma + 1), (gamma + 1)/(2*gamma - 2))*Math.Sqrt(gamma);

            double CF = exitPressure/chamberPressure - ambientPressure/chamberPressure;
            CF *= Math.Pow(chamberPressure/exitPressure, 1/gamma);
            CF *= (gamma - 1)/(2*gamma);
            CF /= 1 - Math.Pow(exitPressure/chamberPressure, (gamma - 1)/gamma);
            CF += 1;
            CF *= Math.Sqrt(1 - Math.Pow(exitPressure/chamberPressure, (gamma - 1)/gamma));
            CF *= GAMMA;
            CF *= Math.Sqrt(2*gamma/(gamma - 1));
            return CF;
        }
    }

}