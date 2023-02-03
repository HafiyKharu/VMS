
using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Visitor.Title.Dtos;
using Abp.Application.Services;
using Visitor.Dto;

namespace Visitor.Title
{
    public interface ITitleAppService :IApplicationService
    {
        Task<PagedResultDto<GetTitleForViewDto>> GetAll(GetAllTitlesInput Input);
        Task<GetTitleForViewDto> GetTitleForView(Guid id);
        Task<GetTitleForEditOutput> GetTitleForEdit(EntityDto<Guid> id);
        Task CreateOrEdit(CreateOrEditTitleDto input);
        Task Delete(EntityDto<Guid> id);
        Task<FileDto> GetAllTitleToExcel(GetAllTitleForExcelInput input);
    }
}
