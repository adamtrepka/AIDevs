using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDevs.Shared.Infrastructure.Embedding
{
    public static class Extensions
    {
        public static float CosineSimilarity(float[] vectorA, float[] vectorB)
        {
            float dotProduct = 0.0f;
            float magnitudeA = 0.0f;
            float magnitudeB = 0.0f;

            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                magnitudeA += vectorA[i] * vectorA[i];
                magnitudeB += vectorB[i] * vectorB[i];
            }

            return dotProduct / (float)(Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
        }
    }
}
