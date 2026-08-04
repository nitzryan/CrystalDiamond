using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PitchTrackingDb;
using ShellProgressBar;

namespace DataAquisition.PitchTracking
{
    internal class GeneratePitchTrajectory
    {
        public static void Calculate(bool forceRefresh)
        {
            PitchTrackingDbContext pitchDb = new(PitchTrackingDb.Connection.PITCHTRACK_DB_OPTIONS);
            SqliteDbContext db = new(Db.Connection.DB_READONLY_OPTIONS);
            
            // Remove old data if refresh
            if (forceRefresh)
                pitchDb.PitchFlightpath.ExecuteDelete();

            // Get all pitch data
            var completedGameIds = pitchDb.PitchFlightpath
                .Select(f => f.GameId)
                .ToHashSet();
            var pitches = db.PitchStatcast
                .Where(f => !completedGameIds.Contains(f.GameId))
                .AsNoTracking()
                .AsEnumerable();

            // Get flightpath from statcast data for each pitch
            int pitchCount = pitches.Count();
            List<PitchFlightpath> flightpaths = new(pitchCount);
            using (ProgressBar progressBar = new(pitchCount, $"Calculating Pitch Trajectories"))
            {
                foreach (var pitch in pitches)
                {
                    progressBar.Tick();

                    PitchFlightpath? pf = CalculateFlightpath(pitch);
                    if (pf == null) // Not all data is valid
                        continue;

                    flightpaths.Add(pf);
                }
            }

            pitchDb.BulkInsert(flightpaths);
        }

        private record PitchCurrentState(double x, double y, double z,
            double vx, double vy, double vz,
            double ax, double ay, double az);

        private static PitchFlightpath? CalculateFlightpath(PitchStatcast pitch)
        {
            // Ensure all data exists
            bool pitchValid =
                pitch.VX is not null &&
                pitch.VY is not null &&
                pitch.VZ is not null &&
                pitch.VStart is not null &&
                pitch.VEnd is not null &&
                pitch.AX is not null &&
                pitch.AY is not null &&
                pitch.AZ is not null &&
                pitch.PfxX is not null &&
                pitch.PfxZ is not null &&
                pitch.BreakAngle is not null &&
                pitch.BreakVertical is not null &&
                pitch.BreakInduced is not null &&
                pitch.BreakHorizontal is not null &&
                pitch.SpinRate is not null &&
                pitch.SpinDirection is not null &&
                pitch.PX is not null &&
                pitch.PZ is not null &&
                pitch.ZoneTop is not null &&
                pitch.ZoneBot is not null &&
                pitch.Extension is not null &&
                pitch.X0 is not null &&
                pitch.Y0 is not null &&
                pitch.Z0 is not null &&
                pitch.PlateTime is not null &&
                pitch.PitchType != DbEnums.PitchType.Fastball;

            if (!pitchValid)
                return null;

            // Setup initial State
            #pragma warning disable CS8629 // pitchValid checks
            PitchCurrentState actualState = new(pitch.X0.Value, pitch.Y0.Value, pitch.Z0.Value,
                pitch.VX.Value, pitch.VY.Value, pitch.VZ.Value,
                pitch.AX.Value, pitch.AY.Value, pitch.AZ.Value);
            PitchCurrentState noAccelState = new(pitch.X0.Value, pitch.Y0.Value, pitch.Z0.Value,
                pitch.VX.Value, pitch.VY.Value, pitch.VZ.Value,
                0, 0, 0);
            #pragma warning disable CS8629

            PitchFlightpath pf = new PitchFlightpath
            {
                GameId = pitch.GameId,
                PitchId = pitch.PitchId,
                Year = pitch.Year,
                PitcherId = pitch.PitcherId,
                PitchClass = Db.DbEnums.StatcastPitchToPitchClass(pitch.PitchType),
                PitchType = pitch.PitchType,
                BreakHoriz_05 = -1000,
                BreakVer_05 = -1000,
                BreakHoriz_10 = -1000,
                BreakVer_10 = -1000,
                BreakHoriz_15 = -1000,
                BreakVer_15 = -1000,
                BreakHoriz_20 = -1000,
                BreakVer_20 = -1000,
                BreakHoriz_25 = -1000,
                BreakVer_25 = -1000,
                HAA = -1000,
                VAA = -1000,
                TrackingError = -1000,
                HB = pitch.BreakHorizontal.Value,
                IVB = pitch.BreakInduced.Value,
                VB = pitch.BreakVertical.Value,
                Vel = pitch.VStart.Value
            };
            // Track ball
            const double dt = 0.01f;
            double prevY = actualState.y;
            const double PLATE_Y = 1.417; // Doesn't correspond with plateX/Z in 2026+
            for (int i = 1; i <= 1000; i++) // Will early-terminate
            {
                PitchCurrentState newState = UpdateState(actualState, dt);
                noAccelState = UpdateState(noAccelState, dt);

                // Track movement from initial velocity path for first 0.25 seconds
                if (i == 5 || i == 10 || i == 15 || i == 20 || i == 25)
                {
                    double breakVert = newState.z - noAccelState.z;
                    double breakHoriz = newState.x - noAccelState.x;

                    switch (i)
                    {
                        case 5:
                            pf.BreakHoriz_05 = (float)breakHoriz * 12;
                            pf.BreakVer_05 = (float)breakVert * 12;
                            break;
                        case 10:
                            pf.BreakHoriz_10 = (float)breakHoriz * 12;
                            pf.BreakVer_10 = (float)breakVert * 12;
                            break;
                        case 15:
                            pf.BreakHoriz_15 = (float)breakHoriz * 12;
                            pf.BreakVer_15 = (float)breakVert * 12;
                            break;
                        case 20:
                            pf.BreakHoriz_20 = (float)breakHoriz * 12;
                            pf.BreakVer_20 = (float)breakVert * 12;
                            break;
                        case 25:
                            pf.BreakHoriz_25 = (float)breakHoriz * 12;
                            pf.BreakVer_25 = (float)breakVert * 12;
                            break;
                    }
                }

                // Check for when the ball crosses the plate to calculate the delta and VAA/HAA
                if (newState.y < PLATE_Y)
                {
                    // Get state at moment of crossing the plate
                    float timeProp = (float)((prevY - PLATE_Y) / (prevY - newState.y));
                    newState = UpdateState(actualState, dt * timeProp);

                    // Get how close the measured position is to statcast for validation
                    float deltaX = (float)newState.x - pitch.PX.Value;
                    float deltaZ = (float)newState.z - pitch.PZ.Value;
                    float deltaPos = (float)Math.Sqrt((deltaX * deltaX) + (deltaZ * deltaZ)) * 12;

                    pf.TrackingError = deltaPos;
                    // Check to ensure that data is not too far off
                    if (pf.TrackingError > 0.5)
                        return null;

                    // Calculate Attack Angles
                    float vaa = (float)(Math.Atan2(newState.vz, -newState.vy) * 180 / Math.PI);
                    float haa = (float)(Math.Atan2(newState.vx, -newState.vy) * 180 / Math.PI);

                    pf.VAA = vaa;
                    pf.HAA = haa;
                    break;
                }

                actualState = newState;
                prevY = actualState.y;
            }

            return pf;
        }

        // Does a single RK4 step using constant accel
        private static PitchCurrentState UpdateState(PitchCurrentState pcs, double dt)
        {
            double ax = pcs.ax, ay = pcs.ay, az = pcs.az;

            // k1
            double k1x = pcs.vx;
            double k1y = pcs.vy;
            double k1z = pcs.vz;
            double k1vx = ax;
            double k1vy = ay;
            double k1vz = az;

            // k2
            double k2x = pcs.vx + (0.5 * dt * k1vx);
            double k2y = pcs.vy + (0.5 * dt * k1vy);
            double k2z = pcs.vz + (0.5 * dt * k1vz);
            double k2vx = ax;
            double k2vy = ay;
            double k2vz = az;

            // k3
            double k3x = pcs.vx + (0.5 * dt * k2vx);
            double k3y = pcs.vy + (0.5 * dt * k2vy);
            double k3z = pcs.vz + (0.5 * dt * k2vz);
            double k3vx = ax;
            double k3vy = ay;
            double k3vz = az;

            // k4
            double k4x = pcs.vx + (dt * k3vx);
            double k4y = pcs.vy + (dt * k3vy);
            double k4z = pcs.vz + (dt * k3vz);
            double k4vx = ax;
            double k4vy = ay;
            double k4vz = az;

            // Weighted average
            double newX = pcs.x + ((dt / 6.0) * (k1x + (2.0 * k2x) + (2.0 * k3x) + k4x));
            double newY = pcs.y + ((dt / 6.0) * (k1y + (2.0 * k2y) + (2.0 * k3y) + k4y));
            double newZ = pcs.z + ((dt / 6.0) * (k1z + (2.0 * k2z) + (2.0 * k3z) + k4z));
            double newVx = pcs.vx + ((dt / 6.0) * (k1vx + (2.0 * k2vx) + (2.0 * k3vx) + k4vx));
            double newVy = pcs.vy + ((dt / 6.0) * (k1vy + (2.0 * k2vy) + (2.0 * k3vy) + k4vy));
            double newVz = pcs.vz + ((dt / 6.0) * (k1vz + (2.0 * k2vz) + (2.0 * k3vz) + k4vz));

            return new PitchCurrentState(newX, newY, newZ, newVx, newVy, newVz, ax, ay, az);
        }
    }
}
