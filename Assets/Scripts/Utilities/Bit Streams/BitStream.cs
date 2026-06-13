using System;
using System.Collections.Generic;

namespace Utilities.Bit_Streams
{
    public class BitStream
    {
        private const int INT_SIZE = 32; // Only works for 32 bit unsigned integers
        private List<int> stream = new List<int>(new int[]{0});
        private int currentBitIndex = 0;
        private int currentArrayIndex = 0;

        ///**
        public void AddBits(int numBits, int bits)
        {
            //& 31 does the same as % 32
            //>>5 does the division by 32

            uint mask = numBits == 32 ? 0xFFFFFFFFu : (1u << numBits) - 1;
            uint maskedBits = (uint)bits & mask;//masking other bits to ensure it doesnt break
            uint shiftedBits = maskedBits << (currentBitIndex & 31);
            stream[currentArrayIndex] |= (int)shiftedBits;
            currentBitIndex += numBits;
            
            if (currentBitIndex >> 5 != currentArrayIndex)
            {
                currentArrayIndex++;
                int alreadyWrittenBits = numBits - (currentBitIndex & 31);
                uint overflowBits = maskedBits >> alreadyWrittenBits;
                uint overflowFinal = (currentBitIndex & 31) == 0 ? 0u : overflowBits; // adjusts it to 0 if there is nothing new to write
                stream.Add((int) overflowFinal);
            }
        }

        public int Read(int bitIndex, int numBits)
        {
            //& 31 does the same as % 32
            //>>5 does the division by 32
            
            int arrayIndex = bitIndex >> 5;
            int shiftAmount = bitIndex & 31;
            uint mem = (uint) stream[arrayIndex];
            uint mask = numBits == 32 ? 0xFFFFFFFFu : (1u << numBits) - 1;
            uint shiftedMask = mask << shiftAmount;
            uint shiftedVal = mem & shiftedMask;
            uint val = shiftedVal >> shiftAmount;
            
            //Overflows into 2 bits (used to be an if but that is slower then just calculating it and adjusting it afterwards)
            int actuallyOverflows = (((bitIndex + numBits) >> 5) - arrayIndex); // 1 if true, otherwise 0
            int numBits2 = (bitIndex + numBits) & 31;
            // the -1 from the mask is automatically correct when numBits2 = 0
            int overflowArrayIndex = arrayIndex + 1 * actuallyOverflows; // gets the same if its not actually extended
            uint overflowMem = (uint) stream[overflowArrayIndex];
            uint overflowMask = (1u << numBits2) - 1;
            uint trueMask = (uint)actuallyOverflows * overflowMask;
            uint maskedOverflow = overflowMem & trueMask;
            uint shiftedOverflow = maskedOverflow << (numBits - numBits2);
            val |= shiftedOverflow;
            
            return (int)val;
        }
        //**/
        
        
        /**
        public void AddBits(int numBits, int bits)
        {
            StringBuilder s = new StringBuilder();

            s.Append("NB: ").Append(numBits).Append(" Bits: ").Append(bits).Append("\n");
            s.Append("CAI: " + currentArrayIndex + " CI: " + currentBitIndex + " %: " + currentBitIndex%32).Append("\n");
            s.Append("Current: ").Append(ToBin(stream[currentArrayIndex])).Append("\n");
            uint start = (uint)bits << (currentBitIndex % 32);
            s.Append("  Start: ").Append(ToBin(start)).Append("\n");
            stream[currentArrayIndex] |= start;
            s.Append(" Placed: ").Append(ToBin(stream[currentArrayIndex])).Append("\n");
            currentBitIndex += numBits;
            s.Append("Info: " + 
                     currentBitIndex + " " + 
                     INT_SIZE +  " " +
                     (currentBitIndex / INT_SIZE) + " " + 
                     currentArrayIndex + " " +
                     (currentBitIndex / INT_SIZE != currentArrayIndex) +  "\n");
            if (currentBitIndex / INT_SIZE != currentArrayIndex)
            {
                currentArrayIndex++;
                uint end = (currentBitIndex % 32) == 0 ? 0u : (uint)bits >> (numBits - (currentBitIndex % 32));
                s.Append("End: ").Append(ToBin(end)).Append("\n");
                stream.Add(end);
                s.Append("New Placed: ").Append(ToBin(stream[currentArrayIndex])).Append("\n");
            }

            s.Append(this);
            Debug.Log(s);
        }

        public int Read(int bitIndex, int numBits)
        {
            StringBuilder s = new StringBuilder();
            int arrayIndex = bitIndex / INT_SIZE;
            uint ar = stream[arrayIndex];
            s.Append("ArrayIndex: ").Append(arrayIndex).Append(" -> ").Append(ToBin(ar)).Append("\n");
            uint mask = (numBits == 32) ? 0xFFFFFFFFu : (1u << numBits) - 1;;
            int shiftAmount = bitIndex % INT_SIZE;
            uint shiftedMask = mask << shiftAmount;
            s.Append("Mask: ").Append(ToBin(mask)).Append("\n");
            s.Append("Shifted: ").Append(ToBin(shiftedMask)).Append("\n");
            uint shiftedVal = (ar & shiftedMask);
            s.Append("Shifted Val: ").Append(ToBin(shiftedVal)).Append("\n");
            uint val = (shiftedVal >> shiftAmount);
            s.Append("Val: ").Append(ToBin(val)).Append(" = ").Append(val).Append("\n");
            
            //Extends over 2 ints
            if ((bitIndex + numBits) / INT_SIZE != arrayIndex)
            {
                int arrayIndex2 = arrayIndex + 1;
                uint ar2 = stream[arrayIndex2];
                s.Append("\tArrayIndex: ").Append(arrayIndex2).Append(" -> ").Append(ToBin(ar2)).Append("\n");
                int numBits2 = (bitIndex + numBits) % INT_SIZE;
                uint mask2 = (numBits2 == 0) ? 0 : (1u << numBits2) - 1;
                s.Append("\tMask: ").Append(ToBin(mask2)).Append("\n");
                uint val2 = ar2 & mask2;
                s.Append("\tVal2: ").Append(ToBin(val2)).Append("\n");
                uint shiftedVal2 = val2 << (numBits - numBits2);
                s.Append("\tShifted Val2: ").Append(ToBin(shiftedVal2)).Append("\n");
                val |= shiftedVal2;
                s.Append("\tUpdated Val: ").Append(ToBin(val)).Append(" = ").Append(val).Append("\n");
            }
            
            Debug.Log(s);
            return (int)val;
        }
        //**/
        
        public override string ToString()
        {
            String s = "";
            foreach (uint i in stream)
            {
                s = ToBin(i) + " " + s;
            }

            return s.ToString();
        }

        private static String ToBin(uint i)
        {
            return Convert.ToString(i, 2).PadLeft(INT_SIZE, '0');
        }

        public int[] PackAndExport()
        {
            int[] export = new int[stream.Count + 1]; //adds extra padding 0 to end
            stream.CopyTo(export);
            return export;
        }
    }
}