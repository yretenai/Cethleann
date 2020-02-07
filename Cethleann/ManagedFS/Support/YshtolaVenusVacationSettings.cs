using JetBrains.Annotations;

namespace Cethleann.ManagedFS.Support
{
    [PublicAPI]
    public class YshtolaVenusVacationSettings : YshtolaSettings
    {
        public YshtolaVenusVacationSettings()
        {
            TableNames = new[] { "COMMON/4be7efcc93f64e14d7bd7984ef6eeb2ef5ad802ea74cf12ad50a4d39bab71798", "HIGH/9b2d39b6d57f8613d9a316d8c3e6120886640a5762ea23c8fcc88d8acc7563b4", "LOW/4e172f40859f78ae3b61db95d114f046e39211a356989a418f0069f66a24bb0d" };
            Multiplier = 0x69UL;
            Divisor = 0xBUL;
            XorTruth = new byte[]
            {
                // TODO: Get XOR Truths from DOAXVV.exe and DOAXVV_Launcher.exe
            };
        }
    }
}
