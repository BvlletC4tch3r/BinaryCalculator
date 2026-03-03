#define DEBUG_PRINT

using Operations;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BinaryCalculator;

namespace C__Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            NormalCalculator();
        }

        static void NormalCalculator()
        {
            while (true)
            {
                int x = Calculator.ReadValue();
                var a = Calculator.IntToBit(x);
                Console.WriteLine(a.ToString());

                int y = Calculator.ReadValue();
                var b = Calculator.IntToBit(y);
                Console.WriteLine(b.ToString());


                Calculator.ReadOperation(a, b);
            }
        }
    }
}
