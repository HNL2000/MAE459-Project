using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAE459_Project.src
{
    public class Chemical
    {
        private double[] A;
        private double[] B;
        private double[] C;
        private double[] D;
        private double[] E;
        private double[] temperatureRanges;
        public double molecularWeight { get; private set; }

        public Chemical(double[] temperatureRanges, double[] A, double[] B, double[] C, double[] D, double[] E, double molecularWeight)
        {
            int[] lengths = { A.Length, B.Length, C.Length, D.Length, E.Length };
            foreach (int i in lengths)
            {
                if (i != lengths[0])
                {
                    throw new SystemException("A-E must have the same number of entries.");
                }
            }
            if (temperatureRanges.Length != A.Length)
            {
                throw new SystemException("Incompatible temperature ranges.");
            }

            this.A = A;
            this.B = B;
            this.C = C;
            this.D = D;
            this.E = E;
            this.temperatureRanges = temperatureRanges;
            this.molecularWeight = molecularWeight;
        } 

        // J/mol.K
        public double GetCp(double temperature, Fluid fluid)
        {
            int counter = -1;
            foreach (int T in temperatureRanges)
            {
                if (T >= temperature)
                {
                    break;
                }
                counter++;
            }
            if (counter < 0)
            {
                throw new SystemException("Temperature below lowest range in database");
            }

            double t = temperature / 1000;
            double t2 = Math.Pow(t, 2); // slight optimization

            return (A[counter] + B[counter]*t + C[counter]*t2 + D[counter]*Math.Pow(t, 3) + E[counter] / t2)/fluid.molecularWeight; // Shomate Equation      
        }
    }
}
