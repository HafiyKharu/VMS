using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Agreement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Twilio.Rest.Verify.V2.Service;
using Visitor.Authorization;
using Visitor.Departments.Dtos;
using Visitor.Departments.Exporting;
using Visitor.Dto;
using Visitor.Level.Dtos;

namespace Visitor.Departments
{
    public class DepartmentsAppService : VisitorAppServiceBase, IDepartmentsAppService
    {
        private readonly IRepository<Department, Guid> _departmentsRepository;
        private readonly IDepartmentExcelExporter _departmentsExcelExporter;

        public DepartmentsAppService(IRepository<Department, Guid> departmentsRepository , IDepartmentExcelExporter departmentExcelExporter)
        {
            _departmentsRepository = departmentsRepository;
            _departmentsExcelExporter = departmentExcelExporter;
        }

        [AbpAuthorize(AppPermissions.Pages_Departments_Create)]
        protected virtual async Task Create(CreateOrEditDepartmentDto input)
        {
            var department = ObjectMapper.Map<Department>(input);
            await _departmentsRepository.InsertAsync(department);
        }
        [AbpAuthorize(AppPermissions.Pages_Departments_Edit)]
        protected virtual async Task Update(CreateOrEditDepartmentDto input)
        {
            var department = await _departmentsRepository.FirstOrDefaultAsync((Guid)input.Id);
            ObjectMapper.Map(input, department);
        }
        [AbpAuthorize(AppPermissions.Pages_Departments_Delete)]
        public async Task Delete(EntityDto<Guid> input)
        {
            await _departmentsRepository.DeleteAsync(input.Id);
        }
        
        public async Task CreateOrEdit(CreateOrEditDepartmentDto input)
        {
            if(input.Id == null)
            {
                await Create(input);
            }
            else
            {
                await Update(input);
            }
        }
        public async Task<GetDepartmentForViewDto> GetDepartmentForView (Guid id)
        {
            var department = await _departmentsRepository.GetAsync(id);
            var output = new GetDepartmentForViewDto { Department = ObjectMapper.Map<DepartmentDto>(department) };
            return output;
        }
        public async Task<GetDepartmentForEditDto> GetDepartmentForEdit (EntityDto<Guid> input)
        {
            var department = await _departmentsRepository.FirstOrDefaultAsync(input.Id);
            var output = new GetDepartmentForEditDto { Department = ObjectMapper.Map<CreateOrEditDepartmentDto>(department) };
            return output;
        }
        public async Task<PagedResultDto<GetDepartmentForViewDto>> GetAll(GetAllDepartmentsInput input)
        {
            var filteredDepartments = _departmentsRepository.GetAll().WhereIf(!string.IsNullOrWhiteSpace(input.Filter), d => false || d.DepartmentName.Contains(input.Filter))
                .WhereIf(!string.IsNullOrWhiteSpace(input.DepartmentNameFilter), d => d.DepartmentName == input.DepartmentNameFilter);

            var pagedAndFilteredDepartments = filteredDepartments.OrderBy(input.Sorting ?? "id asc").PageBy(input);

            var departments = from d in pagedAndFilteredDepartments
                              select new
                              {
                                  Id = d.Id,
                                  d.DepartmentName
                              };
            var totalCount = await filteredDepartments.CountAsync();
            var dbList = await departments.ToListAsync();
            var results = new List<GetDepartmentForViewDto>();

            foreach (var d in dbList)
            {
                var res = new GetDepartmentForViewDto()
                {
                    Department = new DepartmentDto
                    {
                        Id = d.Id,
                        DepartmentName = d.DepartmentName,
                    }
                };
                results.Add(res);
            }
            return new PagedResultDto<GetDepartmentForViewDto>(
                totalCount, results);
        }
        public async Task<FileDto> GetAllDepartmentToExcel(GetAllDepartmentForExcelInput input)
        {
            var filteredDepartments = _departmentsRepository.GetAll().WhereIf(!string.IsNullOrWhiteSpace(input.Filter), d => false || d.DepartmentName.Contains(input.Filter))
                .WhereIf(!string.IsNullOrWhiteSpace(input.DepartmentNameFilter), d => d.DepartmentName == input.DepartmentNameFilter);

            var query = (from o in filteredDepartments
                         select new GetDepartmentForViewDto()
                         {
                             Department = new DepartmentDto
                             {
                                 Id = o.Id,
                                 DepartmentName = o.DepartmentName,
                             }
                         });
            var department = await query.ToListAsync();

            return _departmentsExcelExporter.ExportToFile(department);
        }
    }
}
