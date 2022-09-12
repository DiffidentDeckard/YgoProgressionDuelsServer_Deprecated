using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using YgoProgressionDuels.Data;

namespace YgoProgressionDuels.Pages.Shared
{
    public partial class DuelistBoosterPacksCountComponent : ComponentBase
    {
        [Parameter]
        public Duelist Duelist { get; set; }

        [Parameter]
        public DuelistBoosterPackMode Mode { get; set; }

        [BindProperty]
        private List<BoosterPack> _boosterPacks { get; set; }

        protected override void OnInitialized()
        {
            _boosterPacks = Duelist.BoosterPacks.Where(boosterPack => GetBoosterPackCount(boosterPack) > 0)
                .OrderBy(boosterPack => boosterPack.PackInfo.ReleaseDate).ToList();
        }

        private uint GetBoosterPackCount(BoosterPack boosterPack)
        {
            switch (Mode)
            {
                case DuelistBoosterPackMode.Available:
                    return boosterPack.NumAvailable;

                case DuelistBoosterPackMode.Opened:
                    return boosterPack.NumOpened;

                default:
                    throw new NotImplementedException();
            }
        }
    }

    public enum DuelistBoosterPackMode
    {
        Available,
        Opened
    }
}
