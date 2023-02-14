using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Visitor.Authorization.Roles;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using Abp.Authorization;
using Visitor.Authorization;
using System.Collections.Generic;
using Visitor.Appointments;

namespace Visitor.Appointment.Dashboard
{
    public class VMSDashboardAppService : VisitorAppServiceBase 
    {
        private readonly IRepository<VMSDashboardEnt> _vmsDashboardRepository;
        private readonly IRepository<AppointmentEnt, Guid> _appoitnmentRepository;
        private readonly RoleManager _roleManager;
        private readonly IRepository<UserRole, long> _userRoleRepository;

        public VMSDashboardAppService(
            IRepository<VMSDashboardEnt> vMSDasboardRepository, 
            IRepository<AppointmentEnt, Guid> appoitnmentRepository, 
            RoleManager roleManager, 
            IRepository<UserRole, long> userRoleRepository)
        {
            _vmsDashboardRepository = vMSDasboardRepository;
            _appoitnmentRepository = appoitnmentRepository;
            _roleManager = roleManager;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<PagedResultDto <GetVMSDashboardForViewDto> > GetAll(GetAllVMSDashboardInput input)
        {
            var filterVMSDashboard = _vmsDashboardRepository.GetAll()
                        .Include(e => e.AppointmentFk)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.FullName), e => e.AppointmentFk != null && e.AppointmentFk.FullName == input.FullName);

            var pagedAndFilteredVMSDashboard = filterVMSDashboard
                        .OrderBy(input.Sorting ?? "id asc")
                        .PageBy(input);

            var vmsDashboards = from o in pagedAndFilteredVMSDashboard
                                join o1 in _appoitnmentRepository.GetAll() on o.AppoitnmentId equals o1.Id into j1
                                from s1 in j1.DefaultIfEmpty()

                                select new GetVMSDashboardForViewDto()
                                {
                                    VMSDashboard = new VMSDashboardDto()
                                    {
                                        Id = o.Id
                                    },
                                    FullName = s1 == null || s1.FullName == null ? "" : s1.FullName.ToString()
                                };
            var totalCount = await filterVMSDashboard.CountAsync();
            return new PagedResultDto<GetVMSDashboardForViewDto>
                (
                    totalCount,
                    await vmsDashboards.ToListAsync()
                );
        }
        public async Task<GetVMSDashboardForViewDto> GetVMSDashboardForView (int id)
        {
            var vmsDashboard = await _vmsDashboardRepository.GetAsync (id);
            var output = new GetVMSDashboardForViewDto { VMSDashboard = ObjectMapper.Map<VMSDashboardDto>(vmsDashboard) };
            if(output.VMSDashboard.AppointmentId != null)
            {
                var _lookupAppointment = await _appoitnmentRepository.FirstOrDefaultAsync((Guid)output.VMSDashboard.AppointmentId);
                output.FullName = _lookupAppointment?.FullName?.ToString();
            }
            return output;
        }
        //[AbpAuthorize(AppPermissions.Pages_VMSDashboards_Edit)]
        public async Task<GetVMSDashboardForEditOutput> GetVMSDashboardForEdit(EntityDto input)
        {
            var vmsDashboard = await _vmsDashboardRepository.FirstOrDefaultAsync (input.Id);
            var output = new GetVMSDashboardForEditOutput { VMSDashboard = ObjectMapper.Map<CreateOrEditVMSDashboardDto>(vmsDashboard) };
            if (output.VMSDashboard.AppointmentId != null)
            {
                var _lookupAppointment = await _appoitnmentRepository.FirstOrDefaultAsync((Guid)output.VMSDashboard.AppointmentId);
                output.FulName = _lookupAppointment?.FullName?.ToString();
            }
            return output;

        }
        public async Task CreateOrEdit(CreateOrEditVMSDashboardDto input)
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
        //[AbpAuthorize(AppPermissions.Pages_VMSDashboards_Create)]
        protected virtual async Task Create(CreateOrEditVMSDashboardDto input)
        {
            var vmsDashboard = ObjectMapper.Map<VMSDashboardEnt>(input);

            if (AbpSession.TenantId != null)
            {
                vmsDashboard.TenantId = (int?)AbpSession.TenantId;
            }

            await _vmsDashboardRepository.InsertAsync(vmsDashboard);
        }
        //[AbpAuthorize(AppPermissions.Pages_VMSDashboards_Edit)]
        protected virtual async Task Update(CreateOrEditVMSDashboardDto input)
        {
            var vmsDashboard = await _vmsDashboardRepository.FirstOrDefaultAsync((int)input.Id);
            ObjectMapper.Map(input, vmsDashboard);
        }
        //[AbpAuthorize(AppPermissions.Pages_VMSDashboards_Delete)]
        public async Task Delete(EntityDto input)
        {
            await _vmsDashboardRepository.DeleteAsync(input.Id);
        }
        //[AbpAuthorize(AppPermissions.Pages_VMSDashboards)]
        public async Task<List<VMSDashboardAppoitnmentLookupTableDto>> GetAllAppointmentForTableDropdown()
        {
            return await _appoitnmentRepository.GetAll()
                .Select(appointment => new VMSDashboardAppoitnmentLookupTableDto
                {
                    Id = appointment.Id.ToString(),
                    FullName = appointment == null || appointment.FullName == null ? "" : appointment.FullName.ToString()
                }).ToListAsync();
        }
        public async Task<VMSDashboardAppoitnmentLookupTableDto> GetTotalAppointment( DateTime appointDate)
        {

            var userId = AbpSession.UserId;
            int RegisterAppointment = 0;
            int CheckInAppointment = 0;
            int CheckOutAppointment = 0;
            int CancelAppointment = 0;
            int MissAppointment = 0;
            int TodayAppointment = 0;
            int newBranchId = 0;
            var serviceType = 0;


            //Check if user registered or not, if not registered return 0 result
            //if (objBranchUser == null)
            //{
            //    return new CMSDashboardBookingLookupTableDto();
            //}

            //set date to current date
            var todaysDate = DateTime.Today;
            if (appointDate.Year == 0001)
            {
                appointDate = todaysDate.Date;
                //input.MaxAppointmentDateFilter = todaysDate.Date;
            }

            List<AppointmentEnt> appointment = await _appoitnmentRepository.GetAll().ToListAsync();

            RegisterAppointment = appointment.Count(e => e.Status.Equals(StatusType.Registered) && e.AppDateTime.Date == appointDate.Date);
            CheckInAppointment = appointment.Count(e => e.Status.Equals(StatusType.In) && e.AppDateTime.Date == appointDate.Date);
            CheckOutAppointment = appointment.Count(e => e.Status.Equals(StatusType.Out) && e.AppDateTime.Date == appointDate.Date);
            CancelAppointment = appointment.Count(e => e.Status.Equals(StatusType.Cancel) && e.AppDateTime.Date == appointDate.Date);
            MissAppointment = appointment.Count(e => e.Status.Equals(StatusType.Overstayed) && e.AppDateTime.Date == appointDate.Date);
            TodayAppointment = appointment.Count(e => e.AppDateTime.Date == appointDate.Date);


            var output = new VMSDashboardAppoitnmentLookupTableDto
            {
                RegisterAppointment = RegisterAppointment,
                CancelAppointment = CancelAppointment,
                TodayAppointment = TodayAppointment,
                CheckOutAppointment = CheckOutAppointment,
                CheckInAppointment = CheckInAppointment,
                MissAppointment = MissAppointment
            };

            return output;
        }

    }
}
