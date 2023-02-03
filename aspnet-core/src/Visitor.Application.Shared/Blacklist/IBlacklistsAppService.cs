using Abp.Application.Services;
using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Visitor.Blacklist.Dtos;
using Visitor.Dto;

namespace Visitor.Blacklist
{
    public interface IBlacklistsAppService : IApplicationService
    {
        Task<PagedResultDto<GetBlacklistForViewDto>> GetAll(GetAllBlacklistsInput Input);
        Task<GetBlacklistForViewDto> GetBlacklistForView(Guid id);
        Task<GetBlacklistForEditOutput> GetblacklistForEdit(EntityDto<Guid> id);
        Task CreateOrEdit(CreateOrEditBlacklistDto input);
        Task Delete(EntityDto<Guid> id);
        bool IsExisted(string input);
    }
}
