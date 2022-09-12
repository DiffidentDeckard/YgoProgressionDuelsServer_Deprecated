namespace YgoProgressionDuels.Services
{
    public interface IWebScraper
    {
        /// <summary>
        /// Initializes master booster pack list, and the timer to check for new booster packs every so often.
        /// </summary>
        void Initialize();
    }
}
