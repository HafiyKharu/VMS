﻿using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Visitor.PurposeOfVisit.Dtos
{
    public class CreateOrEditPurposeOfVisitDto:EntityDto<Guid?>
    {
        public string PurposeOfVisitApp { get; set; }  
    }
}
