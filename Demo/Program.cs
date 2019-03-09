using System;

namespace Demo
{
#if WINDOWS || LINUX

    public static class Program
    {
        private static bool _windowed = true;
        private static bool _preferMultiSampling = true;
        private static bool _preferHiDef = true;

        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length % 2 == 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].Contains("windowed"))
                    {
                        if (args[i + 1] == "true")
                            _windowed = true;
                        if (args[i + 1] == "false")
                            _windowed = false;
                    }

                    if (args[i].Contains("msaa"))
                    {
                        if (args[i + 1] == "true")
                            _preferMultiSampling = true;
                        if (args[i + 1] == "false")
                            _preferMultiSampling = false;
                    }

                    if (args[i].Contains("hidef"))
                    {
                        if (args[i + 1] == "true")
                            _preferHiDef = true;
                        if (args[i + 1] == "false")
                            _preferHiDef = false;
                    }
                }
            }

            using (var game = new SimulationDemo(_windowed, _preferHiDef, _preferMultiSampling))
                game.Run();
        }
    }

#endif
}