using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using DaZhongTransitionLiquidation.Areas.SystemManagement.Models;

namespace DaZhongTransitionLiquidation.AutoMapper.Profiles
{
    public class VehicleExtrasFeeSettingProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<Business_VehicleExtrasFeeSetting, Business_VehicleExtrasFeeSettingHistory>()
                .ForMember(dest => dest.LGUID, opt => opt.MapFrom(src => src.VGUID))
                .ForMember(dest => dest.VGUID, opt => Guid.NewGuid());
        }
    }
}