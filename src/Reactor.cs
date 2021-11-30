using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAE459_Project.src
{
    public enum REACTOR_EQUATION { CONSTANT, SINE, PARAMETRIC };

    class Reactor
    {
        private double q;
        private double length;

        public Reactor(double q, double length)
        {
            this.q = q;
            this.length = length;
        }
        
        public void AddHeat(LinkedList<Cell> cells, REACTOR_EQUATION equation)
        {
            foreach (Cell c in cells)
            {
                c.AddHeat(GetHeat(equation, c));
            }
        }

        // returns differential heat given to a cell
        private double GetHeat(REACTOR_EQUATION equation, Cell cell)
        {
            double x = cell.length / 2 + cell.position;

            switch (equation) 
            {
                case REACTOR_EQUATION.CONSTANT:
                    return q*cell.length;
                case REACTOR_EQUATION.SINE:
                    return q*cell.length * Math.Sin(Math.PI * x / length);
                case REACTOR_EQUATION.PARAMETRIC:
                    if (cell.position < length / 2)
                    {
                        return q*cell.length;
                    } else
                    {
                        return q*cell.length * Math.Sin(Math.PI * x / length);
                    }
                default:
                    return -1;
            }

        }
    }
}
