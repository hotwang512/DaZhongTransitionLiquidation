using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;

namespace DaZhongTransitionLiquidation.AutoMapper.Profiles
{
    public class AssetLedgerProfile: Profile
    {
        protected override void Configure()
        {
            CreateMap<AssetsLedger_Swap, AssetsLedger_SwapCompare>();
        }
    }
}