using Microsoft.EntityFrameworkCore;
using SiteDb;
using static SiteDb.DbEnums;

namespace SitePrep
{
    internal class SetTimestepQuality
    {
        public static void Create()
        {
            using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);
            siteDb.QualityCode.ExecuteDelete();
            siteDb.SaveChanges();
            List<QualityCode> codes = new();
            // Each timestepQuality code carries its own severity, optional blurb
            foreach (TimestepQuality q in Enum.GetValues<TimestepQuality>())
            {
                if ((int)q < 0)
                    continue;

                Severity severity;
                string blurb;
                switch (q)
                {
                    // --- Pro timeline ---
                    case TimestepQuality.ProEarlyLow:
                        severity = Severity.Low;
                        blurb = "Low Data";
                        break;
                    case TimestepQuality.ProEarlyMed:
                        severity = Severity.Medium;
                        blurb = "";
                        break;
                    case TimestepQuality.ProEarlyHigh:
                        severity = Severity.High;
                        blurb = "";
                        break;
                    case TimestepQuality.ProPeak:
                        severity = Severity.VeryHigh;
                        blurb = "";
                        break;
                    case TimestepQuality.ProDeclineHigh:
                        severity = Severity.High;
                        blurb = "";
                        break;
                    case TimestepQuality.ProDeclineLow:
                        severity = Severity.Low;
                        blurb = "Model Degredation for long prospect timeline";
                        break;
                    case TimestepQuality.ProDeclineVeryLow:
                        severity = Severity.VeryLow;
                        blurb = "Model Degredation for long prospect timeline";
                        break;
                    // --- College/draft timeline ---
                    case TimestepQuality.CollegeLow:
                        severity = Severity.Low;
                        blurb = "Only 1 year of data";
                        break;
                    case TimestepQuality.CollegeMed:
                        severity = Severity.Medium;
                        blurb = "";
                        break;
                    case TimestepQuality.CollegeHigh:
                        severity = Severity.High;
                        blurb = "";
                        break;
                    case TimestepQuality.CollegeVeryLow:
                        severity = Severity.VeryLow;
                        blurb = "Poor Modeling of Sr. Players";
                        break;
                    case TimestepQuality.HSVeryLow:
                        severity = Severity.VeryLow;
                        blurb = "Only Draft/Signing/Age Data";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(q), q, "Unhandled TimestepQuality value");
                }
                codes.Add(new QualityCode
                {
                    Category = "timestepQuality",
                    Code = q,
                    Severity = severity,
                    Label = severity.ToString(),
                    Blurb = blurb,
                });
            }

            codes.Add(new QualityCode
            {
                Category = "trainingBias",
                Code = TimestepQuality.NotInTraining,
                Severity = Severity.VeryHigh,
                Label = "Not In Training",
                Blurb = "",
            });
            codes.Add(new QualityCode
            {
                Category = "trainingBias",
                Code = TimestepQuality.InTraining,
                Severity = Severity.VeryLow,
                Label = "In Training",
                Blurb = "Included in training data, potential for model overfit",
            });
            siteDb.QualityCode.AddRange(codes);
            siteDb.SaveChanges();
        }
    }
}
