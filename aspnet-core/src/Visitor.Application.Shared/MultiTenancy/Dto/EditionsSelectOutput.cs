﻿using System.Collections.Generic;
using Visitor.Editions.Dto;

namespace Visitor.MultiTenancy.Dto
{
    public class EditionsSelectOutput
    {
        public EditionsSelectOutput()
        {
            AllFeatures = new List<FlatFeatureSelectDto>();
            EditionsWithFeatures = new List<EditionWithFeaturesDto>();
        }

        public List<FlatFeatureSelectDto> AllFeatures { get; set; }

        public List<EditionWithFeaturesDto> EditionsWithFeatures { get; set; }
    }
}
