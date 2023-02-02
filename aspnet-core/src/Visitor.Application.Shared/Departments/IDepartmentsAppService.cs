using Abp.Application.Services;
using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Visitor.Departments.Dtos;
using Visitor.Dto;
using Visitor.Level.Dtos;

namespace Visitor.Departments
{
    public interface IDepartmentsAppService : IApplicationService
    {
        Task<GetDepartmentForViewDto> GetDepartmentForView(Guid id);
        Task<GetDepartmentForEditDto> GetDepartmentForEdit(EntityDto<Guid> input);
        Task<PagedResultDto<GetDepartmentForViewDto>> GetAll(GetAllDepartmentsInput input);
        Task CreateOrEdit(CreateOrEditDepartmentDto input);
        Task<FileDto> GetAllDepartmentToExcel(GetAllDepartmentForExcelInput input);

    }
}
