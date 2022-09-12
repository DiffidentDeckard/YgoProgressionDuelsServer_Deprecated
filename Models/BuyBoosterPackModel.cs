using YgoProgressionDuels.Data;

namespace YgoProgressionDuels.Models
{
    public class BuyBoosterPackModel
    {
        public BoosterPackInfo PackInfo { get; set; }
        public BoosterPack Pack { get; set; }

        public uint NumAvailable { get { return Pack?.NumAvailable ?? 0; } }
        public uint NumOpened { get { return Pack?.NumOpened ?? 0; } }
    }
}
