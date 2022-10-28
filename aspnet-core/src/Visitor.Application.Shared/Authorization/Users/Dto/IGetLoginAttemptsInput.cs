﻿using Abp.Application.Services.Dto;

namespace Visitor.Authorization.Users.Dto
{
    public interface IGetLoginAttemptsInput: ISortedResultRequest
    {
        string Filter { get; set; }
    }
}