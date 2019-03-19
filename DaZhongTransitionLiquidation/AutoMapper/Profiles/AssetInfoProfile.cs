using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DaZhongTransitionLiquidation.Areas.AssetManagement.Models;

namespace DaZhongTransitionLiquidation.AutoMapper.Profiles
{
    public class AssetInfoProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<Business_AssetMaintenanceInfo, Business_AssetMaintenanceInfo_Swap>();
        }
    }
}