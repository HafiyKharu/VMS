using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visitor.Authorization;
using Visitor.PurposeOfVisit.Dtos;
using Visitor.PurposeOfVisit;
using Abp.Collections.Extensions;
using Abp.Linq.Extensions;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Visitor.PurposeOfVisit.Exporting;
using Visitor.Dto;
using Visitor.Title.Dtos;

namespace Visitor.PurposeOfVisit
{


    public class PurposeOfVisitsAppService: VisitorAppServiceBase
    {
        private readonly IRepository<PurposeOfVisitEnt, Guid> _purposeOfVisitRepository;
        private readonly IPurposeOfVisitExcelExporter _purposeOfVisitExcelExporter;


        public PurposeOfVisitsAppService(IRepository<PurposeOfVisitEnt, Guid> purposeOfVisitRepository , IPurposeOfVisitExcelExporter purposeOfVisitExcelExporter)
        {
            _purposeOfVisitRepository = purposeOfVisitRepository;
            _purposeOfVisitExcelExporter = purposeOfVisitExcelExporter;

        }

        public async Task<PagedResultDto<GetPurposeOfVisitForViewDto>> GetAll(GetAllPurposeOfVisitsInput input)
        {

            var filteredPurposeOfVisits = _purposeOfVisitRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.PurposeOfVisitApp.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.POFFilter), e => e.PurposeOfVisitApp == input.POFFilter);

            var pagedAndFilteredPurposeOfVisits = filteredPurposeOfVisits
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var purposeOfVisits = from o in pagedAndFilteredPurposeOfVisits
                         select new
                         {
                             Id = o.Id,
                             o.PurposeOfVisitApp
                         };

            var totalCount = await filteredPurposeOfVisits.CountAsync();

            var dbList = await purposeOfVisits.ToListAsync();
            var results = new List<GetPurposeOfVisitForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetPurposeOfVisitForViewDto()
                {
                    PurposeOfVisit = new Dtos.PurposeOfVisitDto()
                    {

                        PurposeOfVisitApp = o.PurposeOfVisitApp,
                        Id = o.Id,
                    }
                };

                results.Add(res);
            }

            return new PagedResultDto<GetPurposeOfVisitForViewDto>(
                totalCount,
                results
            );

        }

        public async Task<GetPurposeOfVisitForViewDto> GetPurposeOfVisitForView(Guid id)
        {
            var purposeOfVisit = await _purposeOfVisitRepository.GetAsync(id);

            var output = new GetPurposeOfVisitForViewDto { PurposeOfVisit = ObjectMapper.Map<Dtos.PurposeOfVisitDto>(purposeOfVisit) };

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_PurposeOfVisits_Edit)]
        public async Task<GetPurposeOfVisitForEditOutput> GetpurposeOfVisitForEdit(EntityDto<Guid> input)
        {
            var purposeOfVisit = await _purposeOfVisitRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetPurposeOfVisitForEditOutput { PurposeOfVisit = ObjectMapper.Map<CreateOrEditPurposeOfVisitDto>(purposeOfVisit) };

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditPurposeOfVisitDto input)
        {
            if (input.Id == null)
            {
                await Create(input);
            }
            else
            {
                await Update(input);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PurposeOfVisits_Create)]
        protected virtual async Task Create(CreateOrEditPurposeOfVisitDto input)
        {
            var purposeOfVisit = ObjectMapper.Map<PurposeOfVisitEnt>(input);

            await _purposeOfVisitRepository.InsertAsync(purposeOfVisit);

        }

        [AbpAuthorize(AppPermissions.Pages_PurposeOfVisits_Edit)]
        protected virtual async Task Update(CreateOrEditPurposeOfVisitDto input)
        {
            var purposeOfVisit = await _purposeOfVisitRepository.FirstOrDefaultAsync((Guid)input.Id);
            ObjectMapper.Map(input, purposeOfVisit);

        }

        [AbpAuthorize(AppPermissions.Pages_PurposeOfVisits_Delete)]
        public async Task Delete(EntityDto<Guid> input)
        {
            await _purposeOfVisitRepository.DeleteAsync(input.Id);
        }
        public async Task<FileDto> GetAllPurposeOfVisitToExcel(GetAllPurposeOfVisitForExcelInput input)
        {
            var filteredPurposeOfVisits = _purposeOfVisitRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.PurposeOfVisitApp.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.POFFilter), e => e.PurposeOfVisitApp == input.POFFilter);

            var query = (from o in filteredPurposeOfVisits
                         select new GetPurposeOfVisitForViewDto()
                         {
                             PurposeOfVisit = new PurposeOfVisitDto
                             {
                                 Id = o.Id,
                                 PurposeOfVisitApp = o.PurposeOfVisitApp,
                             }
                         });
            var POV = await query.ToListAsync();

            return _purposeOfVisitExcelExporter.ExportToFile(POV);
        }
    }
}
