﻿using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Visitor.Tower.Dtos
{
    public class CreateOrEditTowerDto :EntityDto<Guid?>
    {
        public string TowerBankRakyat { get; set; } 
    }
}
