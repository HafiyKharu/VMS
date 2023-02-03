using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visitor.Authorization;
using Visitor.Level.Dtos;
using Visitor.Level;
using Abp.Linq.Extensions;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Visitor.Dto;
using Visitor.PurposeOfVisit.Dtos;
using Visitor.Level.Exporting;

namespace Visitor.Level
{
    [AbpAuthorize(AppPermissions.Pages_Levels)]
    public class LevelsAppService : VisitorAppServiceBase, ILevelAppService
    {

        private readonly IRepository<LevelEnt, Guid> _levelRepository;
        private readonly ILevelExcelExporter _levelExcelExporter;


        public LevelsAppService(IRepository<LevelEnt, Guid> levelRepository , ILevelExcelExporter levelExcelExporter)
        {
            _levelRepository = levelRepository;
            _levelExcelExporter = levelExcelExporter;
        }

        public async Task<PagedResultDto<GetLevelForViewDto>> GetAll(GetAllLevelsInput input)
        {

            var filteredLevels = _levelRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.LevelBankRakyat.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.LevelFilter), e => e.LevelBankRakyat == input.LevelFilter);

            var pagedAndFilteredLevels = filteredLevels
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var levels = from o in pagedAndFilteredLevels
                         select new
                         {
                             Id = o.Id,
                             o.LevelBankRakyat
                         };

            var totalCount = await filteredLevels.CountAsync();

            var dbList = await levels.ToListAsync();
            var results = new List<GetLevelForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetLevelForViewDto()
                {
                    Level = new Dtos.LevelDto()
                    {

                        LevelBankRakyat = o.LevelBankRakyat,
                        Id = o.Id,
                    }
                };

                results.Add(res);
            }

            return new PagedResultDto<GetLevelForViewDto>(
                totalCount,
                results
            );

        }

        public async Task<GetLevelForViewDto> GetLevelForView(Guid id)
        {
            var level = await _levelRepository.GetAsync(id);

            var output = new GetLevelForViewDto { Level = ObjectMapper.Map<Dtos.LevelDto>(level) };

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_Levels_Edit)]
        public async Task<GetLevelForEditOutput> GetLevelForEdit(EntityDto<Guid> input)
        {
            var level = await _levelRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetLevelForEditOutput { Level = ObjectMapper.Map<CreateOrEditLevelDto>(level) };

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditLevelDto input)
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

        [AbpAuthorize(AppPermissions.Pages_Levels_Create)]
        protected virtual async Task Create(CreateOrEditLevelDto input)
        {
            var level = ObjectMapper.Map<LevelEnt>(input);

            await _levelRepository.InsertAsync(level);

        }

        [AbpAuthorize(AppPermissions.Pages_Levels_Edit)]
        protected virtual async Task Update(CreateOrEditLevelDto input)
        {
            var level = await _levelRepository.FirstOrDefaultAsync((Guid)input.Id);
            ObjectMapper.Map(input, level);

        }

        [AbpAuthorize(AppPermissions.Pages_Levels_Delete)]
        public async Task Delete(EntityDto<Guid> input)
        {
            await _levelRepository.DeleteAsync(input.Id);
        }
        public async Task<FileDto> GetAllLevelToExcel(GetAllLevelForExcelInput input)
        {
            var filteredLevels = _levelRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.LevelBankRakyat.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.LevelFilter), e => e.LevelBankRakyat == input.LevelFilter);

            var query = (from o in filteredLevels
                         select new GetLevelForViewDto()
                         {
                             Level = new LevelDto
                             {
                                 Id = o.Id,
                                 LevelBankRakyat = o.LevelBankRakyat,
                             }
                         });
            var level = await query.ToListAsync();

            return _levelExcelExporter.ExportToFile(level);

        }
    }
}
