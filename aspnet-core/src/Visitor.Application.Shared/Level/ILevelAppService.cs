using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Visitor.Level.Dtos;
using Visitor.Tower.Dtos;
using Abp.Application.Services;
using Visitor.Dto;
using Visitor.Title.Dtos;

namespace Visitor.Level
{
    public interface ILevelAppService : IApplicationService
    {
        Task<PagedResultDto<GetLevelForViewDto>> GetAll(GetAllLevelsInput Input);
        Task<GetLevelForViewDto> GetLevelForView(Guid id);
        Task<GetLevelForEditOutput> GetLevelForEdit(EntityDto<Guid> id);
        Task CreateOrEdit(CreateOrEditLevelDto input);
        Task Delete(EntityDto<Guid> id);
        Task<FileDto> GetAllLevelToExcel(GetAllLevelForExcelInput input);
    }
}
