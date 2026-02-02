using System;
using App;
using Utilities.NoiseDotNet;
using World.Chunk;

namespace Utilities
{
    public class Random
    {
        private static int maxOctaves = 10;
        
        private static float[] _output = Array.Empty<float>();
        private static float[] _result = Array.Empty<float>();

        private static float[] _xCoords = Array.Empty<float>();
        private static float[] _yCoords = Array.Empty<float>();
        
        public static float[] GetChunk(Coord chunkPos, float smallestFreq, float largestAmplitude, int octaves, float ampMult, float freqMult, int seed, int seedOffset)
        {
            //***** Prepping Arrays *****
            //Setting array size
            int sampleCount = Chunk.ChunkSize * Chunk.ChunkSize;
            if (_output.Length != sampleCount)
            {
                _output = new float[sampleCount];
                _result = new float[sampleCount];
                _xCoords = new float[sampleCount];
                _yCoords = new float[sampleCount];
            }
            
            //Figuring out the x and y positions that are going to be generated
            Coord worldPos = Chunk.ChunkToWorld(chunkPos);
            int index = 0;
            for (int x = 0; x < Chunk.ChunkSize; x++)
            {
                for (int y = 0; y < Chunk.ChunkSize; y++)
                {
                    _xCoords[index] = x + worldPos.x;
                    _yCoords[index] = y + worldPos.y;
                    index++;
                }
            }

            for (int i = 0; i < sampleCount; i++)
            {
                _result[i] = 0;
            }
            
            //***** Octave Generation *****
            float freq = smallestFreq;
            float amp = largestAmplitude;
            for (int oct = 0; oct < octaves; oct++)
            {
                Noise.GradientNoise2D(_xCoords, _yCoords, _output, freq, freq, amp, seed + maxOctaves * (int) seedOffset + oct);
                for (int i = 0; i < sampleCount; i++)
                {
                    _result[i] += _output[i];
                }
                
                //prepping next octave
                amp /= ampMult;
                freq *= freqMult;
            }

            return _result;
        }
        
    }
}