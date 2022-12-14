// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using YgoProgressionDuels.Data;

namespace YgoProgressionDuels.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20210709162927_BanListAndEntriesRework")]
    partial class BanListAndEntriesRework
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.7")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("YgoProgressionDuels.Data.ApplicationRole", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("YgoProgressionDuels.Data.ApplicationUser", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("AvatarUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("YgoProgressionDuels.Data.BanList", b =>
                {
                    b.Property<Guid>("BanListId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("ReleaseDate")
                        .HasColumnType("datetime2");

                    b.HasKey("BanListId");

                    b.HasIndex("ReleaseDate")
                        .IsUnique();

                    b.ToTable("BanLists");
                });

            modelBuilder.Entity("YgoProgressionDuels.Data.BanListEntry", b =>
                {
                    b.Property<Guid>("BanListEntryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("BanListStatus")
                        .HasColumnType("int");

                    b.Property<decimal>("CardInfoId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<Guid>("OwnerBanListId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("BanListEntryId");

                    b.HasIndex("CardInfoId");

                    b.HasIndex("OwnerBanListId", "CardInfoId")
                        .IsUnique();

                    b.ToTable("BanListEntries");
                });

            modelBuilder.Entity("YgoProgressionDuels.Data.BoosterPack", b =>
                {
                    b.Property<Guid>("BoosterPackId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("InfoId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("NumAvailable")
                        .HasColumnType("bigint");

                    b.Property<long>("NumOpened")
                        .HasColumnType("bigint");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("BoosterPackId");

                    b.HasIndex("InfoId");

                    b.HasIndex("OwnerId", "InfoId")
                        .IsUnique();

                    b.ToTable("BoosterPacks");
                });

            modelBuilder.Entity("YgoProgressionDuels.Data.BoosterPackInfo", b =>
                {
                    b.Property<Guid>("BoosterPackInfoId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ImageUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("ReleaseDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("SetCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SetInfoUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SetName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("SetType")
                        .HasColumnType("int");

                    b.HasKey("BoosterPackInfoId");

                    b.HasIndex("SetName")
                        .IsUnique()
                        .HasFilter("[SetName] IS NOT NULL");

                    b.ToTable("BoosterPackInfos");
                });

            modelBuilder.Entity("YgoProgressionDuels.Data.Card", b =>
                {
                    b.Property<Guid>("CardId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("DateObtained")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("InfoId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<long>("NumCollection")
                        .HasColumnType("bigint");

                    b.Property<long>("NumExtraDeck")
                        .HasColumnType("bigint");

                    b.Property<long>("NumMainDeck")
                        .HasColumnType("bigint");

                    b.Property<long>("NumSideDeck")
                        .HasColumnType("bigint");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("CardId");

                    b.HasIndex("InfoId");

                    b.HasIndex("OwnerId", "InfoId")
                        .IsUnique();

                    b.ToTable("Cards");
                });

            modelBuilder.Entity("YgoProgressionDuels.Data.CardInfo", b =>
                {
                    b.Property<decimal>("CardInfoId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("decimal(20,0)")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

                    b.Property<long?>("ATK")
                        .HasColumnType("bigint");

                    b.Property<string>("Attribute")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CardInfoUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("DEF")
                        .HasColumnType("bigint");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImageUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("Level")
                        .HasColumnType("bigint");

                    b.Property<long?>("Link")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Race")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("Scale")
                        .HasColumnType("bigint");

                    b.Property<string>("TreatedAs")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Type")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("CardInfoId");

                    b.ToTable("CardInfos");
                });

            modelBuilder.Entity("YgoProgressionDuels.Data.Duelist", b =>
                {
                    b.Property<Guid>("DuelistId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("CardCollectionIsPublic")
                        .HasColumnType("bit");

                    b.Property<bool>("DeckIsPublic")
                        .HasColumnType("bit");

                    b.Property<long>("NumPacksOpened")
                        .HasColumnType("bigint");

                    b.Property<long>("NumStarChips")
                        .HasColumnType("bigint");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("SeriesId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("DuelistId");

                    b.HasIndex("SeriesId");

                    b.HasIndex("OwnerId", "SeriesId")
                        .IsUnique();

                    b.ToTable("Duelists");
                });

            modelBuilder.Entity("YgoProgressionDuels.Data.ProgressionSeries", b =>
                {
                    b.Property<Guid>("ProgressionSeriesId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("AllowPurchaseBoosterPacks")
                        .HasColumnType("bit");

                    b.Property<int>("BanListFormat")
                        .HasColumnType("int");

                    b.Property<long>("BoosterPackPrice")
                        .HasColumnType("bigint");

                    b.Property<Guid?>("CurrentBanListId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("DateStarted")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("HostId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("ProgressionSeriesId");

                    b.HasIndex("CurrentBanListId");

                    b.HasIndex("HostId", "Name")
                        .IsUnique()
                        .HasFilter("[Name] IS NOT NULL");

                    b.ToTable("ProgressionSeries");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>
                {
                    b.HasOne("YgoProgressionDuels.Data.ApplicationRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>
                {
                    b.HasOne("YgoProgressionDuels.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>
                {
                    b.HasOne("YgoProgressionDuels.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>
                {
                    b.HasOne("YgoProgressionDuels.Data.ApplicationRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("YgoProgressionDuels.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>
                {
                    b.HasOne("YgoProgressionDuels.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("YgoProgressionDuels.Data.BanListEntry", b =>
                {
                    b.HasOne("YgoProgressionDuels.Data.CardInfo", "CardInfo")
                        .WithMany()
                        .HasForeignKey("CardInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("YgoProgressionDuels.Data.BanList", "OwnerBanList")
                        .WithMany("Entries")
                        .HasForeignKey("OwnerBanListId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CardInfo");

                    b.Navigation("OwnerBanList");
                });

            modelBuilder.Entity("YgoProgressionDuels.Data.BoosterPack", b =>
                {
                    b.HasOne("YgoProgressionDuels.Data.BoosterPackInfo", "PackInfo")
                        .WithMany()
                        .HasForeignKey("InfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("YgoProgressionDuels.Data.Duelist", "Owner")
                        .WithMany("BoosterPacks")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");

                    b.Navigation("PackInfo");
                });

            modelBuilder.Entity("YgoProgressionDuels.Data.Card", b =>
                {
                    b.HasOne("YgoProgressionDuels.Data.CardInfo", "Info")
                        .WithMany()
                        .HasForeignKey("InfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("YgoProgressionDuels.Data.Duelist", "Owner")
                        .WithMany("CardCollection")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Info");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("YgoProgressionDuels.Data.Duelist", b =>
                {
                    b.HasOne("YgoProgressionDuels.Data.ApplicationUser", "Owner")
                        .WithMany("Duelists")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("YgoProgressionDuels.Data.ProgressionSeries", "Series")
                        .WithMany("Duelists")
                        .HasForeignKey("SeriesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");

                    b.Navigation("Series");
                });

            modelBuilder.Entity("YgoProgressionDuels.Data.ProgressionSeries", b =>
                {
                    b.HasOne("YgoProgressionDuels.Data.BanList", "CurrentBanList")
                        .WithMany()
                        .HasForeignKey("CurrentBanListId");

                    b.HasOne("YgoProgressionDuels.Data.ApplicationUser", "Host")
                        .WithMany("HostedSeries")
                        .HasForeignKey("HostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CurrentBanList");

                    b.Navigation("Host");
                });

            modelBuilder.Entity("YgoProgressionDuels.Data.ApplicationUser", b =>
                {
                    b.Navigation("Duelists");

                    b.Navigation("HostedSeries");
                });

            modelBuilder.Entity("YgoProgressionDuels.Data.BanList", b =>
                {
                    b.Navigation("Entries");
                });

            modelBuilder.Entity("YgoProgressionDuels.Data.Duelist", b =>
                {
                    b.Navigation("BoosterPacks");

                    b.Navigation("CardCollection");
                });

            modelBuilder.Entity("YgoProgressionDuels.Data.ProgressionSeries", b =>
                {
                    b.Navigation("Duelists");
                });
#pragma warning restore 612, 618
        }
    }
}
