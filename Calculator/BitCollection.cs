using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BinaryCalculator
{
    public class BitCollection
    {
        bool[] bits = new bool[0];
        int bitsSize = 0;

        #region Constructors

        public BitCollection(bool bit)
        {
            bits = new bool[] { bit };
            bitsSize = bits.Length;
        }

        public BitCollection(bool[] bits)
        {
            this.bits = bits;
            bitsSize = bits.Length;
        }

        public BitCollection(byte[] bits)
        {
            bitsSize = bits.Length;

            this.bits = new bool[bitsSize];
            for (int i = 0; i < bitsSize; i++)
            {
                this.bits[i] = NumericToBool(bits[i]);
            }
        }

        public BitCollection(BitCollection collection)
        {
            bitsSize = collection.bitsSize;
            this.bits = collection.bits;
        }

        #endregion

        #region Methods
        public void PadLeft(int padding, bool keepSign, bool paddingValue = false)
        {
            if (padding == 0)
                return;

            if(keepSign)
            {
                bool sign = bits[0];
                bool[] nBits = new bool[bitsSize + padding];
                Array.Copy(bits, 1, nBits, (nBits.Length - bits.Length + 1), bits.Length - 1);

                for (int i = 1; i < padding + 1; i++)
                {
                    nBits[i] = sign;
                }

                nBits[0] = sign;
                bitsSize = nBits.Length;
                bits = nBits;

                return;
            }

            bool[] newBits = new bool[bitsSize + padding];
            Array.Copy(bits, 0, newBits, padding, bits.Length);

            for (int i = 0; i < padding; i++)
            {
                newBits[i] = paddingValue;
            }

            bitsSize = newBits.Length;
            bits = newBits;
        }
        
        public void PadRight(int padding)
        {
            if (padding == 0)
                return;

            bool[] newBits = new bool[bitsSize + padding];
            Array.Copy(bits, newBits, bits.Length);

            for (int i = bits.Length; i < newBits.Length; i++)
            {
                newBits[i] = false;
            }

            bitsSize = newBits.Length;
            bits = newBits;
        }

        public void TrimLeft()
        {
            int count = 0;
            for (count = 0; count < bitsSize; count++)
            {
                if (bits[count])
                    break;
            }

            int bitsCount = bitsSize - count;
            if (int.IsNegative(bitsCount))
                throw new Exception("Trim exceed size!");

            bool[] newBits = new bool[bitsCount];
            Array.Copy(bits, count, newBits, 0, bitsSize);

            bitsSize = bitsCount;
            bits = newBits;
        }

        public void TrimRight()
        {
            int count = 0;
            for (int i = bitsSize - 1; i >= 0; i--, count++)
            {
                if (bits[i])
                    break;
            }

            int bitsCount = bitsSize - count;
            if (int.IsNegative(bitsCount))
                throw new Exception("Trim exceed size!");

            bool[] newBits = new bool[bitsCount];
            Array.Copy(bits, 0, newBits, 0, bitsCount);

            bitsSize = bitsCount;
            bits = newBits;
        }

        public void RemoveMSB()
        {
            if (bits.Length == 0)
                return;

            bitsSize -= 1;
            bool[] newBits = new bool[bitsSize];
            Array.Copy(bits, 1, newBits, 0, bitsSize);
            bits = newBits;
        }

        public void RemoveLSB()
        {
            if (bits.Length == 0)
                return;

            bitsSize -= 1;
            bool[] newBits = new bool[bitsSize];
            Array.Copy(bits, 0, newBits, 0, bitsSize);
            bits = newBits;
        }

        public int BitsToInt()
        {
            int result = 0, power = 1;
            for (int i = bitsSize - 1; i >= 1; i--, power *= 2)
            {
                if (bits[i])
                    result += (power);
            }

            if (bits[0])
            {
                result = power - result;
                result *= -1;
            }

            return result;
        }

        public void BitToRight(bool bit)
        {
            bitsSize += 1;
            Array.Resize(ref bits, bitsSize);
            bits[bitsSize - 1] = bit;
        }

        public void BitToLeft(bool bit)
        {
            bitsSize += 1;
            bool[] newBits = new bool[bitsSize];
            newBits[0] = bit;
            Array.Copy(bits, 0, newBits, 1, bitsSize - 1);
            bits = newBits;
        }

        bool NumericToBool(byte value) => value == 0 ? false : true;

        byte BoolToNumeric(bool value) => (byte)(value == false ? 0 : 1);

        #endregion

        #region Get/Set Definitions

        public int Length
        {
            get
            {
                return bitsSize;
            }
        }

        public bool isSigned
        {
            get
            {
                if (!(bitsSize > 0))
                    return false;
                return bits[0];
            }
        }

        public static BitCollection Zero
        {
            get
            {
                return new BitCollection(new byte[] { 0 });
            }
        }

        #endregion

        #region Operators

        public bool this[int index]
        {
            get
            {
                if (isIndexOutsideBounds(index))
                    NewException("Index was outside of bounds!");

                return bits[index];
            }
            set
            {
                if (isIndexOutsideBounds(index))
                    NewException("Index was outside of bounds!");

                bits[index] = value;
            }
        }

        public static bool operator >=(BitCollection a, BitCollection b) => (a == b || a > b);

        public static bool operator <=(BitCollection a, BitCollection b) => (a == b || a < b);

        public static bool operator >(BitCollection a, BitCollection b) => a.BitsToInt() > b.BitsToInt();

        public static bool operator <(BitCollection a, BitCollection b) => a.BitsToInt() < b.BitsToInt();

        public static bool operator ==(BitCollection a, BitCollection b)
        {
            int aLen = a.Length, bLen = b.Length;
            if(aLen != bLen)
            {
                if (aLen > bLen)
                {
                    int dif = aLen - bLen;
                    b.PadLeft(dif, true);
                }
                else
                {
                    int dif = bLen - aLen;
                    a.PadLeft(dif, true);
                }
            }

            for (int i = 0; i < aLen; i++)
            {
                if (!Xnor(a[i], b[i]))
                    return false;
            }

            return true;
        }

        public static bool operator !=(BitCollection a, BitCollection b) => !(a == b);

        public static BitCollection operator <<(BitCollection a, int shift)
        {
            var bits = a.bits;
            bool[] newBits = new bool[bits.Length + shift];
            Array.Copy(bits, 0, newBits, 0, bits.Length);
            return new BitCollection(newBits);
        }

        public static BitCollection operator >>(BitCollection a, int shift)
        {
            var bits = a.bits;
            int dif = bits.Length - shift;
            if (dif < 0)
            {
                return new BitCollection(new bool[bits.Length]);
            }

            bool sign = bits[0];

            bool[] newBits = new bool[bits.Length];
            Array.Copy(bits, 0, newBits, shift, dif);
            for (int i = 0; i < shift; i++)
            {
                newBits[i] = sign;
            }
            return new BitCollection(newBits);
        }

        public static BitCollection operator +(BitCollection a, BitCollection b)
        {
            bool[] newBits = new bool[a.Length + b.Length];
            Array.Copy(a.bits, 0, newBits, 0, a.Length);
            Array.Copy(b.bits, 0, newBits, a.Length, b.Length);
            return new BitCollection(newBits);
        }

        #endregion

        #region Others
        bool isIndexOutsideBounds(int index)
        {
            return index < 0 || index > bitsSize - 1;
        }

        void NewException(string message)
        {
            throw new Exception(message);
        }

        public override string ToString()
        {
            string str = "";
            foreach (var bit in bits)
            {
                str += BoolToNumeric(bit);
            }
            return str;
        }

        static bool Xnor(bool a, bool b) => !(a ^ b);
        #endregion
    };
}
