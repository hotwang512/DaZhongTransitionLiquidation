using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;
using DaZhongTransitionLiquidation.Areas.AssetPurchase.Models;

namespace DaZhongTransitionLiquidation.AutoMapper.Profiles
{
    public class AssignProfile:Profile
    {
        protected override void Configure()
        {
            CreateMap<Business_PurchaseAssign, Business_FundClearing>();
        }
    }
}