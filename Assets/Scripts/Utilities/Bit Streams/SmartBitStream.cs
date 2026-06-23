using Utilities.Bit_Streams;

namespace Utilities
{
    public class SmartBitStream
    {
        private BitStream b;
        private int[] lengths;
        private int segmentLength;
        private int currentWriteLengthIndex;
        private int currentReadLengthIndex;
        private int currentReadBitIndex;

        public SmartBitStream(int[] lengths)
        {
            b = new BitStream();
            this.lengths = lengths;
            currentWriteLengthIndex = 0;
            currentReadLengthIndex = 0;
            currentReadBitIndex = 0;

            segmentLength = 0;
            foreach (int len in lengths)
            {
                segmentLength += len;
            }
        }

        public void AddNext(int bits)
        {
            b.AddBits(lengths[currentWriteLengthIndex], bits);
            currentWriteLengthIndex = (currentWriteLengthIndex + 1) % lengths.Length;
        }

        public int ReadNext()
        {
            int numBits = lengths[currentReadLengthIndex];
            int val = b.Read(currentReadBitIndex, numBits);
            currentReadBitIndex += numBits;
            currentReadLengthIndex = (currentReadLengthIndex + 1) % lengths.Length;
            return val;
        }

        public void GoToSegment(int index)
        {
            currentReadLengthIndex = 0;
            currentReadBitIndex = index * segmentLength;
        }

        public int[] PackAndExport()
        {
            return b.PackAndExport();
        }
        
        public void PackAndExport(int[] export)
        {
            b.PackAndExport(export);
        }

        public override string ToString()
        {
            return b.ToString();
        }

        public void Clear()
        {
            b.Clear();
            currentWriteLengthIndex = 0;
            currentReadLengthIndex = 0;
            currentReadBitIndex = 0;
        }
        
        public int GetCount()
        {
            return b.GetCount();
        }

        public int GetSegmentLength()
        {
            return segmentLength;
        }
    }
}