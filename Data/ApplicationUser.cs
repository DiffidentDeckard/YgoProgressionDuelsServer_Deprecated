using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace YgoProgressionDuels.Data
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        // The path to this user's avatar image
        public string AvatarUrl { get; set; }

        // The list of progression series this user is hosting
        [InverseProperty(nameof(ProgressionSeries.Host))]
        public ICollection<ProgressionSeries> HostedSeries { get; set; } = new List<ProgressionSeries>();

        // The list duelist objects for progression series this user is participating in
        [InverseProperty(nameof(Duelist.Owner))]
        public ICollection<Duelist> Duelists { get; set; } = new List<Duelist>();
    }

    /// <summary>
    /// We have to create our own IdentityRole in order to use Guid instead of string for ApplicationUser's primary key
    /// </summary>
    public class ApplicationRole : IdentityRole<Guid>
    {
        /// <summary>
        /// Need default ctor for Add-Migration to work
        /// </summary>
        public ApplicationRole() : base()
        {
        }

        /// <summary>
        /// Need this ctor in order to create named Roles using this ApplicationRole class
        /// </summary>
        /// <param name="roleName"></param>
        public ApplicationRole(string roleName) : base(roleName)
        {
        }
    }
}
