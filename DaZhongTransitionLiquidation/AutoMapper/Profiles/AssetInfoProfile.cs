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
            CreateMap<Business_AssetMaintenanceInfo, AssetMaintenanceInfo_Swap>();
            CreateMap<Business_AssetMaintenanceInfo, AssetsLedger_SwapCompare>()
                .ForMember(dest => dest.FA_LOC_1, opt => opt.MapFrom(src => src.BELONGTO_COMPANY))
                .ForMember(dest => dest.ASSET_CREATION_DATE, opt => opt.MapFrom(src => src.LISENSING_DATE))
                .ForMember(dest => dest.FA_LOC_2, opt => opt.MapFrom(src => src.MANAGEMENT_COMPANY))
                .ForMember(dest => dest.ACCT_DEPRECIATION, opt => opt.MapFrom(src => src.ACCT_DEPRECIATION.ToString()))
                .ForMember(dest => dest.FA_LOC_3, opt => opt.MapFrom(src => src.ORGANIZATION_NUM));
        }
    }
}