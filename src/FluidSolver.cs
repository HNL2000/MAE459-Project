using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAE459_Project.src
{
    static class FluidSolver
    {
        private static string PATH = "properties.txt"; 

        public static int numCells;
        private static Chemical H2;
        private static Fluid fluid;

        private static List<double> convergence = new List<double>();

        private static Input input;
        private static double maxWallTemp;
        private static double heatFlux;
        private static double convectiveTransfer;
        private static Nozzle nozzle;
        private static double ambientPressure;

        private static void SetUpChemicals()
        {
            // Sourced from NIST database, more accurate than average estimate suggested by project

            H2 = new Chemical(new double[] { 298,       1000,      2500}, 
                              new double[] { 33.066178, 18.563083, 43.413560 }, 
                              new double[] {-11.363417, 12.257357, -4.293079 }, 
                              new double[] { 11.432816, -2.859786,  1.272428 }, 
                              new double[] { -2.772874,  0.268238, -0.096876 },
                              new double[] { -0.158558,  1.977990,-20.533862 },
                              2);
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Setting up environment...");
            
            // Load properties file
            Dictionary<string, string> properties = new Dictionary<string, string>();
            StreamReader sr = new StreamReader(PATH);
            while (!sr.EndOfStream) 
            {
                string[] temp = sr.ReadLine().Split('=');
                // no error checking so just type stuff right please
                properties.Add(temp[0].Trim(), temp[1].Trim());
            }
            // Decipher properties file
            FluidSolver.numCells = Int32.Parse(properties["numCells"]);
            FluidSolver.heatFlux = Double.Parse(properties["heatFlux"]);
            FluidSolver.maxWallTemp = Double.Parse(properties["maxWallTemp"]);
            REACTOR_EQUATION reactorEquation = (REACTOR_EQUATION)Enum.Parse(typeof(REACTOR_EQUATION), properties["reactorEquation"].ToUpper());
            Console.WriteLine("Reactor: " + properties["reactorEquation"][0].ToString().ToUpper() + properties["reactorEquation"].Substring(1).ToLower());
            nozzle = new Nozzle(Double.Parse(properties["nozzleExitDia"]));
            decimal reactorLength = Decimal.Parse(properties["reactorLength"]);
            double circumference = Double.Parse(properties["circumference"]);
            ambientPressure = Double.Parse(properties["ambientPressure"]);

            // Problem setup
            SetUpChemicals();

            Chemical[] components = { H2 }; // enter fluid components (set up during chemical setup)
            double[] massFractions = { 1 }; // mass fractions corresponding to components
            fluid = new Fluid("\"H2\"", components, massFractions);
            
            double inPressure = Double.Parse(properties["inPressure"]);
            double inTemperature = Double.Parse(properties["inTemperature"]);
            double inMassFlow = Double.Parse(properties["inMassFlow"]);
            double area = Double.Parse(properties["area"]);
            input = new Input(inPressure, inTemperature, inMassFlow, area, fluid);
            convectiveTransfer = heatFlux * circumference / input.massFlow;

            sr.Close();
            
            // find max reactor length, or go with input number if > 0
            if (reactorLength <= 0) {
                Console.WriteLine("Calculating maximum reactor length...");
                reactorLength = FindMaxReactorLength(reactorEquation, 3);
                Console.WriteLine("Reactor max length: " + reactorLength + " meters");
            }

            Console.WriteLine("Running simulation...");
            // make reactor
            Reactor reactor = new Reactor(convectiveTransfer, (double)reactorLength);

            // construct cells from back to front
            LinkedList<Cell> cells = new LinkedList<Cell>();
            Cell last = null;
            for (int i = 0; i <= numCells; i++)
            {
                cells.AddFirst(new Cell(last, fluid, input.area, (double)reactorLength * (1 - (double)i / numCells), (double)reactorLength / numCells, 28*2*(0.15+1.2)));
                last = cells.First.Value;
            }

            // add differential heat to all cells
            reactor.AddHeat(cells, reactorEquation);
            
            // init first cell, update second
            cells.First.Value.InitialSetup(input.pressure, input.temperature, input.velocity, input.density);
            cells.ElementAt(1).Update(cells.First.Value);
            
            double maxWallTemp = -1;
            foreach (Cell cell in cells) {
                if (cell.TotalTemperature > maxWallTemp) {
                    maxWallTemp = cell.TotalTemperature;
                }
            }
            Console.WriteLine("Maximum Wall Temperature: " + String.Format("{0:0.000}", maxWallTemp) + " K");

            // solve nozzle flow using final cell
            double[] performance = nozzle.NozzleFlow(input.massFlow, cells.Last.Value, ambientPressure);
            Console.WriteLine("----------- Performance -----------");
            Console.WriteLine("c*: " + String.Format("{0:0.000}", performance[0]) + " m/s");
            Console.WriteLine("CF: " + String.Format("{0:0.000}", performance[1]));
            Console.WriteLine("F: " + String.Format("{0:0.000}", performance[2]) + " N");
            Console.WriteLine("Isp: " + String.Format("{0:0.000}", performance[3]) + " seconds");

            // collect data into file;
            StreamWriter sw = new StreamWriter("output.txt");
            sw.WriteLine("c* (m/s): " + performance[0]);
            sw.WriteLine("CF: " + performance[1]);
            sw.WriteLine("F (N): " + performance[2]);
            sw.WriteLine("Isp (s): " + performance[3]);
            sw.WriteLine("------------------------------------");
            sw.WriteLine("x(m)    T(K)    Tt(K)   P(Pa)   Pt(Pa)  M");
            sw.WriteLine("------------------------------------");
            foreach (Cell cell in cells) {
                sw.Write(cell.position + "    ");
                sw.Write(cell.temperature + "    ");
                sw.Write(cell.TotalTemperature + "    ");
                sw.Write(cell.pressure + "    ");
                sw.Write(cell.TotalPressure + "    ");
                sw.Write(cell.getMachNumber() + "    ");
                sw.WriteLine();
            }
            
            sw.Flush();
            sw.Close();
            Console.ReadKey();
        }

        public static decimal FindMaxReactorLength(REACTOR_EQUATION reactorEquation, double precision) {
            decimal increment = 1;
            int currentPrecision = 0; // precision to 10^-n (3 means 0.001)
            decimal reactorLength = 0m;

            while (currentPrecision <= precision) 
            {
                reactorLength += increment;

                // make reactor
                Reactor reactor = new Reactor(convectiveTransfer, (double)reactorLength);

                LinkedList<Cell> cells = new LinkedList<Cell>();
                Cell last = null;
                // construct cells from back to front
                for (int i = 0; i <= numCells; i++)
                {
                    cells.AddFirst(new Cell(last, fluid, input.area, (double)reactorLength * (1 - (double)i / numCells), (double)reactorLength / numCells, 28*2*(0.15+1.2)));
                    last = cells.First.Value;
                }

                // add differential heat to all cells
                reactor.AddHeat(cells, reactorEquation);
                
                // init first cell, update second
                cells.First.Value.InitialSetup(input.pressure, input.temperature, input.velocity, input.density);
                cells.ElementAt(1).Update(cells.First.Value);

                // after running sim
                Boolean exceeded = false;
                foreach (Cell cell in cells) {
                    if (cell.TotalTemperature > maxWallTemp) {
                        exceeded = true;
                        break;
                    }
                }

                // if temp exceeded (i.e., too long), step back and refine (or escape while loop)
                // worst case is 9.999.. which requires 10*n trials to get max length (n being precision)
                // e.g., if the reactor would be 9.999 long, requires 40 simulations to be run 
                if (exceeded) {
                    reactorLength -= increment;
                    currentPrecision += 1;
                    increment /= 10m;
                }
            }

            return reactorLength;
        }
    }
}
