using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAE459_Project.src
{
    class Cell
    {
        private Cell next;
        public Fluid fluid { get; private set; }
        public double pressure { get; private set; }
        public double TotalPressure => pressure + 0.5 * density * velocity * velocity;
        public double temperature { get; private set; }
        public double TotalTemperature => temperature + velocity * velocity / (2 * fluid.GetCp(temperature));
        public double velocity { get; private set; }
        public double density { get; private set; }
        public double area {get; private set; }
        public double position { get; private set; }
        public double length { get; private set; }
        public double circumference { get; private set; }
        public double Cp => fluid.GetCp(temperature);
        public double Gamma => Cp / (Cp - fluid.GasConstant);
        private double heat;

        public Cell(Cell next, Fluid fluid, double area, double position, double length, double circumference)
        {
            this.next = next;
            this.fluid = fluid;
            this.area = area;
            this.position = position;
            this.length = length;
            this.circumference = circumference;
        }
        public void InitialSetup(double pressure, double temperature, double velocity, double density) {
            this.pressure = pressure;
            this.temperature = temperature;
            this.velocity = velocity;
            this.density = density;
        }

        public void AddHeat(double heat) { this.heat = heat; }

        public void Update(Cell lastCell)
        {
            if (lastCell.pressure < 0 || lastCell.temperature < 0 || lastCell.velocity < 0 || lastCell.density < 0) {
                throw new Exception("Negative property in cell at position " + lastCell.position);
            }
            
            LinearizedFormulas(lastCell);

            next?.Update(this);
        }

        private void LinearizedFormulas(Cell lastCell) {
            double rho = lastCell.density;
            double P = lastCell.pressure;
            double T = lastCell.temperature;
            double u = lastCell.velocity;
            double A = lastCell.area;
            double Astar = this.area;
            double Cp = lastCell.Cp;
            //Console.WriteLine(Cp);
            // TODO: use correct wall shear!
            double tau = 0.05*0.5*rho*u*u; // ?
            double c = lastCell.circumference;
            double dx = lastCell.length;
            double q = this.heat;

            double lambda = -1*tau*c*dx/(rho*A);

            // density
            double rhostar = lambda*rho*rho/P;
            rhostar += rho*u*u*(1/(Cp*T) - (Astar-A)/(Cp*A*T)) + rho - rho*q/(Cp*T) - rho*rho*u*u*(1/P - (Astar-A)/(A*P));
            double div = 1 - rho*rho*u*u/P + u*u/(Cp*T);
            rhostar /= div;
            this.density = rhostar;

            // velocity
            double ustar = (rho - rhostar)/rho - (Astar - A)/A;
            ustar *= u;
            ustar += u;
            this.velocity = ustar;

            // pressure
            double Pstar = lambda - u*(ustar - u);
            Pstar *= rho;
            Pstar += P;
            this.pressure = Pstar;

            // temperature
            double Tstar = q - u*(ustar-u);
            Tstar /= Cp;
            Tstar += T;
            this.temperature = Tstar;
        }
    }
}
