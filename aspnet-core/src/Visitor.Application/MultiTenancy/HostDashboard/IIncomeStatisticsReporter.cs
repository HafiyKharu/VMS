﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Visitor.MultiTenancy.HostDashboard.Dto;

namespace Visitor.MultiTenancy.HostDashboard
{
    public interface IIncomeStatisticsService
    {
        Task<List<IncomeStastistic>> GetIncomeStatisticsData(DateTime startDate, DateTime endDate,
            ChartDateInterval dateInterval);
    }
}