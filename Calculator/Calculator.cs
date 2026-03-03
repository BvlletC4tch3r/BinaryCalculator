using Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryCalculator
{
    public static class Calculator
    {
        #region NumConvertions
        static Dictionary<int, BitCollection> NumToBinary = new Dictionary<int, BitCollection>()
        {
            [0] = new BitCollection(new byte[] { 0 }),
            [1] = new BitCollection(new byte[] { 0, 1 }),
            [2] = new BitCollection(new byte[] { 0, 1, 0 }),
            [3] = new BitCollection(new byte[] { 0, 1, 1 }),
            [4] = new BitCollection(new byte[] { 0, 1, 0, 0 }),
            [5] = new BitCollection(new byte[] { 0, 1, 0, 1 }),
            [6] = new BitCollection(new byte[] { 0, 1, 1, 0 }),
            [7] = new BitCollection(new byte[] { 0, 1, 1, 1 }),
            [8] = new BitCollection(new byte[] { 0, 1, 0, 0, 0 }),
            [9] = new BitCollection(new byte[] { 0, 1, 0, 0, 1 }),
        };
        #endregion

        #region Constants
        const double euler = 2.7182_8182_8459_0452_3536_0287_4713_527d;
        const double PI = 3.1415_9265_3589_7932_3846_2643_3832_795d;
        #endregion

        #region Members
        static int scale = 0;
        static Stack<Operation> operations = new Stack<Operation>();
        #endregion

        #region Methods
        public static int ReadValue()
        {
            try
            {
                string str = Console.ReadLine();
                return Convert.ToInt32(str);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public static void ReadOperation(BitCollection a, BitCollection b)
        {
            string str = Console.ReadLine();
            BitCollection result = null;
            Operation op = null;

            switch (str)
            {
                case "+":
                    op = new Sum(a, b);
                    break;
                case "-":
                    op = new Subtraction(a, b);
                    break;
                case "*":
                    op = new Multiplication(a, b);
                    break;
                case "/":
                    op = new Division(a, b);
                    break;
                default:

                    break;
            }

            var r = op.GetResult;

            if (r is not null)
            {
                Console.WriteLine();
                Console.WriteLine($"{a.ToString()} {str} {b.ToString()}:");
                Console.WriteLine($"{r.BitsToInt()}\t{r.ToString()}");
                Console.WriteLine();
            }

            operations.Push(op);
        }

        public static BitCollection ExecuteOperation(BitCollection a, BitCollection b, char operation)
        {
            BitCollection result = null;
            Operation op = null;
            switch(operation)
            {
                case '+':
                    op = new Sum(a, b);
                    break;
                case '-':
                    op = new Subtraction(a, b);
                    break;
                case 'x':
                    op = new Multiplication(a, b);
                    break;
                case '/':
                    op = new Division(a, b);
                    break;
                default:
                    return null;
            }

            return op.GetResult;
        }

        public static BitCollection IntToBit(int value)
        {
            if (!NumToBinary.ContainsKey(value))
                return IntConvertion(value);

            return NumToBinary[value];
        }

        public static BitCollection IntConvertion(int value)
        {
            List<bool> bits = new List<bool>();

            long v = Math.Abs(value);
            bool remainder = false;
            while (v != 0)
            {
                remainder = (v % 2) == 1 ? true : false;
                v = (int)v / 2;

                bits.Add(remainder);
            }

            bits.Add(false);
            bits.Reverse();
            if (int.IsNegative(value))
            {
                return PositiveToNegative(new BitCollection(bits.ToArray()));
            }

            var result = new BitCollection(bits.ToArray());
            return result;
        }

        public static BitCollection PositiveToNegative(BitCollection value)
        {
            if (value.isSigned)
                return null;

            List<bool> newBits = new List<bool>();

            for (int i = 0; i < value.Length; i++)
            {
                newBits.Add(!value[i]);
            }

            var neg = new BitCollection(newBits.ToArray());
            var sum = new BitCollection(true);

            int dif = neg.Length - sum.Length;
            sum.PadLeft(dif, false, false);

            var result = new Sum(neg, sum, false).GetResult;
            return result;
        }

        public static BitCollection NegativeToPositive(BitCollection value)
        {
            if (!value.isSigned)
                return null;

            var bitsLen = value.Length;
            bool[] newBits = new bool[bitsLen];
            newBits[0] = false;
            for (int i = 1; i < bitsLen; i++)
            {
                newBits[i] = !value[i];
            }

            var positive = new BitCollection(newBits);
            var sum = new BitCollection(true);
            sum.PadLeft(positive.Length - sum.Length, false, false);
            return new Sum(positive, sum, false).GetResult;
        }

        public static void AdjustLengths(BitCollection a, BitCollection b, bool keepSigns)
        {
            int aLen = a.Length;
            int bLen = b.Length;
            if (a.Length > b.Length)
            {
                int dif = aLen - bLen;
                b.PadLeft(dif, keepSigns);
            }
            else
            {
                int dif = bLen - aLen;
                a.PadLeft(dif, keepSigns);
            }
        }

        public static void AddPadding(BitCollection a, bool paddingValue)
        {
            var len = 64;
            while (a.Length < len)
            {
                int dif = len - a.Length;
                if (dif > 8)
                {
                    len -= 8;
                    continue;
                }

                a.PadLeft(dif, true, paddingValue);
                break;
            }
        }
        #endregion
    }
}
