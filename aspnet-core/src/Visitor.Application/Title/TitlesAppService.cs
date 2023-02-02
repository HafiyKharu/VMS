using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visitor.Authorization;
using Visitor.Title.Dtos;
using Abp.Linq.Extensions;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Visitor.Dto;
using Visitor.Appointment;
using Visitor.Title.Exporting;

namespace Visitor.Title
{
    [AbpAuthorize(AppPermissions.Pages_Titles)]
    public class TitlesAppService : VisitorAppServiceBase, ITitleAppService
    {

        private readonly IRepository<TitleEnt, Guid> _titleRepository;
        private readonly ITitleExcelExporter _titleExcelExporter;


        public TitlesAppService(IRepository<TitleEnt, Guid> titleRepository , ITitleExcelExporter titleExcelExporter)
        {
            _titleRepository = titleRepository;
            _titleExcelExporter = titleExcelExporter;

        }

        public async Task<PagedResultDto<GetTitleForViewDto>> GetAll(GetAllTitlesInput input)
        {

            var filteredTitles = _titleRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.VisitorTitle.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.TitleFilter), e => e.VisitorTitle == input.TitleFilter);

            var pagedAndFilteredTitles = filteredTitles
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var titles = from o in pagedAndFilteredTitles
                         select new
                         {
                             Id = o.Id,
                             o.VisitorTitle
                         };

            var totalCount = await filteredTitles.CountAsync();

            var dbList = await titles.ToListAsync();
            var results = new List<GetTitleForViewDto>();

            foreach (var o in dbList)
            {
                var res = new GetTitleForViewDto()
                {
                    Title = new Dtos.TitleDto()
                    {

                        VisitorTitle = o.VisitorTitle,
                        Id = o.Id,
                    }
                };

                results.Add(res);
            }

            return new PagedResultDto<GetTitleForViewDto>(
                totalCount,
                results
            );

        }

        public async Task<GetTitleForViewDto> GetTitleForView(Guid id)
        {
            var title = await _titleRepository.GetAsync(id);

            var output = new GetTitleForViewDto { Title = ObjectMapper.Map<Dtos.TitleDto>(title) };

            return output;
        }

        [AbpAuthorize(AppPermissions.Pages_Titles_Edit)]
        public async Task<GetTitleForEditOutput> GetTitleForEdit(EntityDto<Guid> input)
        {
            var title = await _titleRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetTitleForEditOutput { Title = ObjectMapper.Map<CreateOrEditTitleDto>(title) };

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditTitleDto input)
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

        [AbpAuthorize(AppPermissions.Pages_Titles_Create)]
        protected virtual async Task Create(CreateOrEditTitleDto input)
        {
            var title = ObjectMapper.Map<TitleEnt>(input);

            await _titleRepository.InsertAsync(title);

        }

        [AbpAuthorize(AppPermissions.Pages_Titles_Edit)]
        protected virtual async Task Update(CreateOrEditTitleDto input)
        {
            var title = await _titleRepository.FirstOrDefaultAsync((Guid)input.Id);
            ObjectMapper.Map(input, title);

        }

        [AbpAuthorize(AppPermissions.Pages_Titles_Delete)]
        public async Task Delete(EntityDto<Guid> input)
        {
            await _titleRepository.DeleteAsync(input.Id);
        }

        public async Task<FileDto> GetAllTitleToExcel(GetAllTitleForExcelInput input)
        {
            var filteredTitles = _titleRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.VisitorTitle.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.TitleFilter), e => e.VisitorTitle == input.TitleFilter);

            var query = (from o in filteredTitles
                         select new GetTitleForViewDto()
                         {
                             Title = new TitleDto
                             {
                                 Id = o.Id,
                                 VisitorTitle = o.VisitorTitle,                                
                             }
                         });
            var title = await query.ToListAsync();

            return _titleExcelExporter.ExportToFile(title);
        }
    }
}
