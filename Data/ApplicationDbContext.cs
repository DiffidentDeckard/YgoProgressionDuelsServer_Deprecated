using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace YgoProgressionDuels.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public DbSet<ProgressionSeries> ProgressionSeries { get; set; }

        public DbSet<Duelist> Duelists { get; set; }

        public DbSet<BoosterPackInfo> BoosterPackInfos { get; set; }

        public DbSet<BoosterPack> BoosterPacks { get; set; }

        public DbSet<CardInfo> CardInfos { get; set; }

        public DbSet<Card> Cards { get; set; }

        public DbSet<BanList> BanLists { get; set; }

        public DbSet<BanListEntry> BanListEntries { get; set; }

        public DbSet<Tournament> Tournaments { get; set; }

        public DbSet<TournamentDuelist> TournamentDuelists { get; set; }

        public DbSet<TournamentPairing> TournamentPairings { get; set; }

        public DbSet<PairingDuelist> PairingDuelists { get; set; }

        public DbSet<StructureDeckCard> StructureDeckCards { get; set; }

        public DbSet<SpecialProduct> SpecialProducts { get; set; }

        public DbSet<SpecialProductCard> SpecialProductCards { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
