using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAE459_Project.src
{
    
    public class Fluid
    {
        private Chemical[] components;
        private double[] moleFractions;
        public double molecularWeight  {get; private set; }
        public double GasConstant => 8314.5 / molecularWeight;
        public string name { get; private set; }

        public Fluid(string name, Chemical[] components, double[] moleFractions)
        {
            if (components.Length != moleFractions.Length)
            {
                throw new SystemException("Invalid number of components and moleFractions");
            }
            double sum = 0;
            foreach (double d in moleFractions)
            {
                sum += d;
            }
            if (Math.Abs(1 - sum) > 0.01)
            {
                throw new SystemException("Mole fractions do not add to 1");
            }

            this.name = name;
            this.components = components;
            this.moleFractions = moleFractions;

            this.molecularWeight = 0;
            for (int i = 0; i < components.Count(); i++) {
                this.molecularWeight += moleFractions[i] / components[i].molecularWeight;
            }
            this.molecularWeight = 1 / this.molecularWeight;

            Console.WriteLine("Fluid " + name + " created");
        }

        // J/mol.K
        public double GetCp(double temperature)
        {
            double Cp = 0;
            for (int i = 0; i < components.Length; i++)
            {
                Cp += components[i].GetCp(temperature) * moleFractions[i];
            }
            return Cp*1000/molecularWeight;
        }
    }
}
