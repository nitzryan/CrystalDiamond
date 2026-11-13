namespace PitchAnalysis.PitchZoneBinNS
{
    // Square, odd-sized weight grid centered on a cell. Offsets are in cells, not feet,
    // so borrowing vertically through the (taller) middle row reaches physically further.
    public sealed class PitchZoneKernel
    {
        private readonly double[,] weights;   // normalised so the center is 1

        // Cells reached in each direction from the center: (size - 1) / 2.
        internal int Radius { get; }

        public PitchZoneKernel(double[,] weights)
        {
            ArgumentNullException.ThrowIfNull(weights);
            int size = weights.GetLength(0);
            if (size != weights.GetLength(1) || size % 2 == 0)
            {
                throw new ArgumentException(
                    $"Kernel must be square with an odd size, was {weights.GetLength(0)}x{weights.GetLength(1)}.", nameof(weights));
            }

            foreach (double weight in weights)
            {
                if (double.IsNaN(weight) || weight < 0)
                {
                    throw new ArgumentException($"Kernel weights must be non-negative, found {weight}.", nameof(weights));
                }
            }

            Radius = size / 2;
            double center = weights[Radius, Radius];
            if (center <= 0)
            {
                throw new ArgumentException("The center weight must be positive so a cell always counts its own pitches.", nameof(weights));
            }

            // Normalise so the center is 1; the smoothed mean is unaffected, but the effective count stays comparable to MinPitches.
            this.weights = new double[size, size];
            for (int row = 0; row < size; row++)
            {
                for (int column = 0; column < size; column++)
                {
                    this.weights[row, column] = weights[row, column] / center;
                }
            }
        }

        // Weight for the cell rowOffset/columnOffset away from the center ; both must be within [-Radius, Radius].
        internal double Weight(int rowOffset, int columnOffset)
        {
            return weights[rowOffset + Radius, columnOffset + Radius];
        }

        // Constructor helpers
        // 3x3 plus shape: the cell at 1, the four edge-adjacent cells at neighbourWeight, corners at 0.
        public static PitchZoneKernel Adjacent(double neighbourWeight)
        {
            return new PitchZoneKernel(new double[,]
            {
                { 0, neighbourWeight, 0 },
                { neighbourWeight, 1, neighbourWeight },
                { 0, neighbourWeight, 0 },
            });
        }

        // (2 * radius + 1) square kernel with weight exp(-d^2 / (2 * sigma^2)), d measured in cells; the center is 1.
        public static PitchZoneKernel Gaussian(int radius, double sigma)
        {
            int size = (2 * radius) + 1;
            double[,] weights = new double[size, size];
            for (int row = 0; row < size; row++)
            {
                for (int column = 0; column < size; column++)
                {
                    int rowOffset = row - radius;
                    int columnOffset = column - radius;
                    double distanceSquared = (rowOffset * rowOffset) + (columnOffset * columnOffset);
                    weights[row, column] = Math.Exp(-distanceSquared / (2 * sigma * sigma));
                }
            }

            return new PitchZoneKernel(weights);
        }
    }
}
