using System.Threading.Tasks;

namespace YgoProgressionDuels.Services
{
    public interface IDbInitializer
    {
        /// <summary>
        /// Initializes the database.
        /// </summary>
        Task InitializeAsync();
    }
}
