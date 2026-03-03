using C__Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinaryCalculator;

namespace Operations
{
    public abstract class Operation
    {
        #region Members
        protected BitCollection operationResult = new BitCollection(new byte[] { 0 });
        #endregion

        #region Methods
        public abstract void Execute(BitCollection a, BitCollection b);
        public void AdjustLengths(BitCollection a, BitCollection b, bool keepSigns)
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
        public BitCollection GetResult
        {
            get
            {
                return operationResult;
            }
        }
        #endregion

        #region Logic Gates 
        protected static bool Not(bool a) => !a;
        protected static bool And(bool a, bool b) => a && b;
        protected static bool Nand(bool a, bool b) => !And(a, b);
        protected static bool Or(bool a, bool b) => a | b;
        protected static bool Nor(bool a, bool b) => !Or(a, b);
        protected static bool Xor(bool a, bool b) => a ^ b;
        protected static bool Xnor(bool a, bool b) => !Xor(a, b);
        #endregion
    }

    public class Sum : Operation
    {
        public Sum() { }
        public Sum(BitCollection a, BitCollection b)
        {
            Execute(a, b);
        }

        public Sum(BitCollection a, BitCollection b, bool keepSign)
        {
            Execute(a, b, keepSign);
        }

        public override void Execute(BitCollection a, BitCollection b)
        {
            if (a.Length != b.Length)
                AdjustLengths(a, b, true);

            var minLen = a.Length <= b.Length ? a.Length : b.Length;
            List<bool> result = new List<bool>();

            bool carry = false;
            for (int i = minLen - 1; i >= 0; i--)
            {
                bool x = a[i];
                bool y = b[i];

                if (carry)
                {
                    bool tmp = carry;
                    carry = And(tmp, x);
                    x = Xor(tmp, x);

                    result.Add(Xor(x, y));

                    if (And(x, y))
                        carry = true;

                    continue;
                }

                result.Add(Xor(x, y));
                carry = And(x, y);
            }

            operationResult = DetectOverflow(a[0], b[0], result[result.Count - 1], result);
        }

        public void Execute(BitCollection a, BitCollection b, bool keepSign)
        {
            if (a.Length != b.Length)
                AdjustLengths(a, b, keepSign);

            var minLen = a.Length <= b.Length ? a.Length : b.Length;
            List<bool> result = new List<bool>();

            bool carry = false;
            for (int i = minLen - 1; i >= 0; i--)
            {
                bool x = a[i];
                bool y = b[i];

                if (carry)
                {
                    bool tmp = carry;
                    carry = And(tmp, x);
                    x = Xor(tmp, x);

                    result.Add(Xor(x, y));

                    if (And(x, y))
                        carry = true;

                    continue;
                }

                result.Add(Xor(x, y));
                carry = And(x, y);
            }

            operationResult = DetectOverflow(a[0], b[0], result[result.Count - 1], result);
        }

        BitCollection DetectOverflow(bool aSign, bool bSign, bool carry, List<bool> bits)
        {
            // Overflow
            if(And(Nor(aSign, bSign), carry))
            {
                bits.Add(aSign);
                bits.Reverse();
                return new BitCollection(bits.ToArray());
            }
            // Underflow
            else if(And(And(aSign, bSign), Not(carry)))
            {
                bits.Add(aSign);
                bits.Reverse();
                return new BitCollection(bits.ToArray());
            }

            // No overflow
            bits.Reverse();
            return new BitCollection(bits.ToArray());
        }
    }

    public class Subtraction : Operation
    {
        public Subtraction() { }
        public Subtraction(BitCollection a, BitCollection b)
        {
            Execute(a, b);
        }

        public override void Execute(BitCollection a, BitCollection b)
        {
            if (b.isSigned)
            {
                b = Calculator.NegativeToPositive(b);
                Sum sum = new Sum(a, b);
                operationResult = sum.GetResult;
                return;
            }

            b = Calculator.PositiveToNegative(b);
            Sum op = new Sum(a, b);
            operationResult = op.GetResult;
        }
    }

    public class Multiplication : Operation
    {
        public Multiplication() { }
        public Multiplication(BitCollection a, BitCollection b)
        {
            Execute(a, b);
        }
        public override void Execute(BitCollection a, BitCollection b)
        {
            if (a.Length != b.Length)
                AdjustLengths(a, b, true);

            bool Qn = false;
            bool Qplus = false;

            BitCollection AC = new BitCollection(new byte[b.Length]);
            BitCollection QR = new BitCollection(b);
            for (int counter = 0; counter < b.Length; counter++)
            {
                Qn = QR[QR.Length - 1];

                // Sum
                if (And(Not(Qn), Qplus))
                {
                    Sum tmp = new Sum(AC, a);
                    AC = tmp.GetResult;
                }
                //Subtraction
                else if (And(Qn, Not(Qplus)))
                {
                    Subtraction tmp = new Subtraction(AC, a);
                    AC = tmp.GetResult;
                }

                // Right Shift
                bool ACBIT = AC[AC.Length - 1];
                AC = AC >> 1;
                QR.RemoveLSB();
                QR.BitToLeft(ACBIT);

                Qplus = Qn;
            }

            operationResult = AC + QR;
        }
    }

    public class Division : Operation
    {
        public Division(BitCollection a, BitCollection b)
        {
            Execute(a, b);
        }

        public override void Execute(BitCollection a, BitCollection b)
        {
            if(b > a || b == BitCollection.Zero)
            {
                operationResult = new BitCollection(false);
                return;
            }

            var minLen = a.Length <= b.Length ? a.Length : b.Length;

            BitCollection quotient = new BitCollection(false);
            var aCopy = new BitCollection(a);
            var bCopy = new BitCollection(b);
            for (int i = a.Length - 1; i >= 0; i--)
            {
                var tmp = aCopy >> i;
                if (bCopy.Length != tmp.Length)
                    AdjustLengths(bCopy, tmp, true);

                if(bCopy <= tmp)
                {
                    aCopy = new Subtraction(aCopy, bCopy << i).GetResult;
                    quotient.BitToRight(true);
                    continue;
                }

                quotient.BitToRight(false);
            }

            operationResult = quotient;
        }
    }

    public class NewDivision : Operation
    {
        public NewDivision(BitCollection a, BitCollection b)
        {
            Execute(a, b);
        }

        public override void Execute(BitCollection a, BitCollection b)
        {
            if(b > a)
            {
                operationResult = BitCollection.Zero;
                return;
            }

            // A 1111_0011 = -13
            // B 0000_0011 = 3
            // EXP -13/3 = -4 R = -1

            var d = b << 5;

            throw new NotImplementedException();
        }
    }

    public class LongMultiplication : Operation
    {
        public LongMultiplication(BitCollection a, BitCollection b)
        {
            Execute(a, b);
        }

        public override void Execute(BitCollection a, BitCollection b)
        {
            BitCollection result = new BitCollection(new bool[0]);
            for (int i = b.Length - 1, pad = 0; i >= 0; i--, pad++)
            {
                var tmp = PartialProduct(a, b[i], pad);

                if (result.Length != tmp.Length)
                    AdjustLengths(result, tmp, true);

                result = new Sum(result, tmp).GetResult;
            }

            operationResult = result;
        }

        public static BitCollection PartialProduct(BitCollection a, bool multiplier, int padding)
        {
            List<bool> result = new List<bool>();
            for (int i = a.Length - 1; i >= 0; i--)
            {
                bool x = a[i];
                result.Add(And(x, multiplier));
            }

            result.Reverse();
            BitCollection bit = new BitCollection(result.ToArray());
            bit.PadRight(padding);
            return bit;
        }
    }

    public class RegularSubtraction : Operation
    {
        public RegularSubtraction(BitCollection a, BitCollection b)
        {
            Execute(a, b);
        }

        public override void Execute(BitCollection a, BitCollection b)
        {
            var minLen = a.Length <= b.Length ? a.Length : b.Length;
            List<bool> result = new List<bool>();

            bool borrow = false;
            for (int i = minLen - 1; i >= 1; i--)
            {
                bool x = a[i];
                bool y = b[i];

                if (borrow)
                {
                    borrow = Nand(borrow, x);
                    result.Add(Xor(borrow, y));
                    continue;
                }

                result.Add(Xor(x, y));
                borrow = And(Not(x), y);
            }

            result.Reverse();
            operationResult = new BitCollection(result.ToArray());
        }
    }
}
