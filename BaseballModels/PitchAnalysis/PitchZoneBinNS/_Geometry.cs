namespace PitchAnalysis.PitchZoneBinNS
{
    public partial class PitchZoneBin
    {
        private double[] BuildColumnEdges()
        {
            double[] edges = new double[gridColumns + 1];
            double zoneLeft = -BaseballConstants.ZoneHalfWidth;
            double zoneRight = BaseballConstants.ZoneHalfWidth;
            int zoneLeftIndex = args.OutsideColumns;
            int zoneRightIndex = args.OutsideColumns + args.ZoneColumns;

            // Outside-left edges, ending exactly on the zone's left edge.
            for (int i = 0; i <= args.OutsideColumns; i++)
            {
                edges[i] = zoneLeft - ((args.OutsideColumns - i) * cellSize);
            }

            // Equal-width columns across the zone.
            for (int i = 1; i < args.ZoneColumns; i++)
            {
                edges[zoneLeftIndex + i] = zoneLeft + (i * cellSize);
            }

            // Zone's right edge, then outside-right edges.
            for (int i = 0; i <= args.OutsideColumns; i++)
            {
                edges[zoneRightIndex + i] = zoneRight + (i * cellSize);
            }

            return edges;
        }

        private double[] BuildRowEdges(float zoneTop, float zoneBot)
        {
            double zoneBottomEdge = zoneBot - BaseballConstants.BallRadius;
            double zoneTopEdge = zoneTop + BaseballConstants.BallRadius;
            double middleHeight = zoneTopEdge - zoneBottomEdge - (2 * args.SquareRowsPerSide * cellSize);

            double[] edges = new double[gridRows + 1];

            // Outside-below edges, ending exactly on the zone's bottom edge.
            for (int i = 0; i <= args.OutsideRows; i++)
            {
                edges[i] = zoneBottomEdge - ((args.OutsideRows - i) * cellSize);
            }

            // Square rows up from the bottom of the zone.
            for (int i = 1; i <= args.SquareRowsPerSide; i++)
            {
                edges[args.OutsideRows + i] = zoneBottomEdge + (i * cellSize);
            }

            // Split the middle into two equal-height rows; the only edge not set by the square-row loops.
            if (args.SplitMiddleRow && middleHeight >= 0)
            {
                edges[args.OutsideRows + args.SquareRowsPerSide + 1] =
                    zoneBottomEdge + (args.SquareRowsPerSide * cellSize) + (middleHeight / 2);
            }

            // Square rows down from the top of the zone (the gap between these and the ones above is the middle row).
            int topZoneEdgeIndex = args.OutsideRows + zoneRows;
            for (int i = 0; i <= args.SquareRowsPerSide; i++)
            {
                edges[topZoneEdgeIndex - i] = zoneTopEdge - (i * cellSize);
            }

            // Outside-above edges.
            for (int i = 1; i <= args.OutsideRows; i++)
            {
                edges[topZoneEdgeIndex + i] = zoneTopEdge + (i * cellSize);
            }

            // Need to adjust the some edges if there is not a middle bin
            if (middleHeight < 0)
            {
                double midpoint = ((zoneTopEdge - zoneBottomEdge) / 2) + zoneBottomEdge;
                edges[topZoneEdgeIndex - args.SquareRowsPerSide] = midpoint;
                edges[topZoneEdgeIndex - args.SquareRowsPerSide - 1] = midpoint;

                if (args.SplitMiddleRow)
                {
                    edges[topZoneEdgeIndex - args.SquareRowsPerSide - 2] = midpoint;
                }
            }

            return edges;
        }

        private ZoneRegion[,] BuildRegions()
        {
            ZoneRegion[,] result = new ZoneRegion[gridRows, gridColumns];
            int zoneBottomRow = args.OutsideRows;
            int zoneTopRow = args.OutsideRows + zoneRows;
            int zoneLeftColumn = args.OutsideColumns;
            int zoneRightColumn = args.OutsideColumns + args.ZoneColumns;

            // Middle of the zone: half the columns (floored) centred horizontally, and the 1 (or 2 if split) rows at the vertical centre
            int middleColumns = args.ZoneColumns / 2;
            int middleLeftColumn = zoneLeftColumn + ((args.ZoneColumns - middleColumns) / 2);
            int middleBottomRow = args.OutsideRows + args.SquareRowsPerSide;
            int middleTopRow = middleBottomRow + (args.SplitMiddleRow ? 1 : 0);

            for (int row = 0; row < gridRows; row++)
            {
                for (int column = 0; column < gridColumns; column++)
                {
                    // Cells away from the zone on each axis; 0 when the cell is level with the zone.
                    int rowGap = row < zoneBottomRow ? zoneBottomRow - row
                        : row >= zoneTopRow ? row - zoneTopRow + 1
                        : 0;
                    int columnGap = column < zoneLeftColumn ? zoneLeftColumn - column
                        : column >= zoneRightColumn ? column - zoneRightColumn + 1
                        : 0;
                    if (rowGap == 0 && columnGap == 0)
                    {
                        bool onZoneBoundary = row == zoneBottomRow || row == zoneTopRow - 1
                                                    || column == zoneLeftColumn || column == zoneRightColumn - 1;

                        bool inMiddle = args.SeparateMiddle
                            && row >= middleBottomRow && row <= middleTopRow
                            && column >= middleLeftColumn && column < middleLeftColumn + middleColumns;
                        result[row, column] = onZoneBoundary ? ZoneRegion.InsideBorder
                            : inMiddle ? ZoneRegion.InsideMiddle
                            : ZoneRegion.InsideCenter;
                    }
                    else if (rowGap <= 1 && columnGap <= 1)
                    {
                        result[row, column] = rowGap == 1 && columnGap == 1 ? ZoneRegion.Corner : ZoneRegion.Edge;
                    }
                    else
                    {
                        result[row, column] = ZoneRegion.Other;
                    }
                }
            }
            return result;
        }
    }
}
