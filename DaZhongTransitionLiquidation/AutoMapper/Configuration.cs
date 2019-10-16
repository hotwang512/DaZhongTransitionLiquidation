using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.AutoMapper
{
    public class Configuration
    {
        public static void Configure()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<Profiles.AssetInfoProfile>();
                cfg.AddProfile<Profiles.AssignProfile>();
                cfg.AddProfile<Profiles.AssetLedgerProfile>();
            });
        }
    }
}