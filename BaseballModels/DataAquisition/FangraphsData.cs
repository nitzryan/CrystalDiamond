using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAquisition
{
    internal class FangraphsData
    {
        public static bool Main(List<int> years)
        {
            try
            {
                // Apply Chadwick Register to correlate ids
                ProcessStartInfo info = new()
                {
                    FileName = "python",
                    Arguments = $"{Constants.SCRIPT_FOLDER}/GetChadwick.py",
                    UseShellExecute = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = false,
                };
                using (Process process = Process.Start(info) ?? throw new Exception("Failed to create process"))
                {
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                        throw new Exception("GetChadwick.py Python script failed");
                }

                // Update Fangraphs WAR
                foreach (int year in years)
                {
                    info.Arguments = $"{Constants.SCRIPT_FOLDER}/GetFangraphsWar.py {year}";
                    using (Process process = Process.Start(info) ?? throw new Exception("Failed to create process"))
                    {
                        process.WaitForExit();
                        if (process.ExitCode != 0)
                            throw new Exception("GetFangraphsWar.py Python script failed");
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in FangraphsData");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
