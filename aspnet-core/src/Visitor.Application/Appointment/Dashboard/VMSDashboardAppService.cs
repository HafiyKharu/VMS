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
    [AbpAuthorize(AppPermissions.Pages_VMSDashboards)]
    public class VMSDashboardAppService : VisitorAppServiceBase 
    {
        private readonly IRepository<AppointmentEnt, Guid> _appointmentRepository;

        public VMSDashboardAppService(IRepository<AppointmentEnt, Guid> appoitnmentRepository)
        {
            _appointmentRepository = appoitnmentRepository;
        }
        protected DateTime GetToday()
        {
            return DateTime.Now.Date;
        }
        [AbpAuthorize(AppPermissions.Pages_VMSDashboards)]
        public async Task<PagedResultDto <GetAppointmentForViewDto> > GetAllTodayVMSDashboard(GetAllAppointmentsInput input)
        {
            DateTime minA = DateTime.Now;
            DateTime maxA = DateTime.Now;
            DateTime minR = DateTime.Now;
            DateTime maxR = DateTime.Now;

            if (input.MinAppDateTimeFilter != null)
            {
                minA = (DateTime)input.MinAppDateTimeFilter;
            }
            if (input.MaxAppDateTimeFilter != null)
            {
                maxA = (DateTime)input.MaxAppDateTimeFilter;
            }
            if (input.MinRegDateTimeFilter != null)
            {
                minR = (DateTime)input.MinRegDateTimeFilter;
            }
            if (input.MaxRegDateTimeFilter != null)
            {
                maxR = (DateTime)input.MaxRegDateTimeFilter;
            }

            DateTime d1 = GetToday();
            var todaysDate = DateTime.Today;


            var statusEnumFilter = input.StatusFilter.HasValue
                        ? (StatusType)input.StatusFilter
                        : default;


            var filteredAppointments = _appointmentRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.FullName.Contains(input.Filter) || e.IdentityCard.Contains(input.Filter) || e.PhoneNo.Contains(input.Filter) || e.Email.Contains(input.Filter) || e.PassNumber.Contains(input.Filter) || e.OfficerToMeet.Contains(input.Filter) || e.OfficerToMeet.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.FullNameFilter), e => e.FullName == input.FullNameFilter)
                        .Where(e => e.AppDateTime.Date == d1)

                        .WhereIf(!string.IsNullOrWhiteSpace(input.IdentityCardFilter), e => e.IdentityCard == input.IdentityCardFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PhoneNoFilter), e => e.PhoneNo == input.PhoneNoFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.EmailFilter), e => e.Email == input.EmailFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.TitleFilter), e => e.Title == input.TitleFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.CompanyNameFilter), e => e.CompanyName == input.CompanyNameFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.OfficerToMeetFilter), e => e.OfficerToMeet == input.OfficerToMeetFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.EmailOfficerToMeet), e => e.EmailOfficerToMeet == input.EmailOfficerToMeet)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PhoneNoFilter), e => e.PhoneNoOfficerToMeet == input.PhoneNoOfficerToMeet)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.PurposeOfVisitFilter), e => e.PurposeOfVisit == input.PurposeOfVisitFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.DepartmentFilter), e => e.Department == input.DepartmentFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.TowerFilter), e => e.Tower == input.TowerFilter)

                        .WhereIf(input.MinAppDateTimeFilter != null, e => e.AppDateTime.Date >= minA.Date)
                        .WhereIf(input.MaxAppDateTimeFilter != null, e => e.AppDateTime.Date <= maxA.Date)
                        .WhereIf(input.MinRegDateTimeFilter != null, e => e.CreationTime.Date >= minR.Date)
                        .WhereIf(input.MaxRegDateTimeFilter != null, e => e.CreationTime.Date <= maxR.Date)


                        .WhereIf(input.StatusFilter.HasValue, e => e.Status == statusEnumFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.LevelFilter), e => e.Level == input.LevelFilter)
                        .WhereIf(!string.IsNullOrWhiteSpace(input.AppRefNoFilter), e => e.AppRefNo == input.AppRefNoFilter);

            var pagedAndFilteredAppointments = filteredAppointments
                .OrderBy(input.Sorting ?? "id asc")
                .PageBy(input);

            var appointments = from o in pagedAndFilteredAppointments
                               select new
                               {
                                   o.Id,
                                   o.FullName,
                                   o.Email,
                                   o.PhoneNo,
                                   o.IdentityCard,
                                   o.PurposeOfVisit,
                                   o.CompanyName,
                                   o.OfficerToMeet,
                                   o.Department,
                                   o.Tower,
                                   o.Level,
                                   o.AppDateTime,
                                   o.CreationTime,
                                   o.Status,
                                   o.Title,
                                   o.ImageId,
                                   o.AppRefNo,
                                   o.PassNumber,
                                   o.CheckInDateTime,
                                   o.CheckOutDateTime,
                                   o.EmailOfficerToMeet,
                                   o.PhoneNoOfficerToMeet,
                                   o.CancelDateTime,

                               };
            var dbList = await appointments.ToListAsync();
            var results = new List<GetAppointmentForViewDto>();
            var image = new UploadPictureOutput();

            foreach (var o in dbList)
            {
                var res = new GetAppointmentForViewDto()
                {
                    Appointment = new AppointmentDto
                    {
                        Id = o.Id,
                        FullName = o.FullName,
                        Email = o.Email,
                        PhoneNo = o.PhoneNo,
                        IdentityCard = o.IdentityCard,
                        PurposeOfVisit = o.PurposeOfVisit,
                        CompanyName = o.CompanyName,
                        OfficerToMeet = o.OfficerToMeet,
                        Department = o.Department,
                        Tower = o.Tower,
                        Level = o.Level,
                        AppDateTime = o.AppDateTime,
                        RegDateTime = o.CreationTime,
                        Status = o.Status,
                        Title = o.Title,
                        ImageId = o.ImageId,
                        AppRefNo = o.AppRefNo,
                        PassNumber = o.PassNumber,
                        CheckInDateTime = o.CheckInDateTime,
                        CheckOutDateTime = o.CheckOutDateTime,
                        CancelDateTime = o.CancelDateTime,
                        EmailOfficerToMeet = o.EmailOfficerToMeet,
                        PhoneNoOfficerToMeet = o.PhoneNoOfficerToMeet
                    }
                };

                results.Add(res);
            }
            var totalCount = await filteredAppointments.CountAsync();

            return new PagedResultDto<GetAppointmentForViewDto>(
                totalCount,
                results
            );
        }
        
        public async Task<VMSDashboardAppoitnmentLookupTableDto> GetTotalAppointment( DateTime appointDate)
        {
            int totalToday = 0;
            int registered = 0;
            int checkIn = 0;
            int checkOut = 0;
            int cancel = 0;

            var todaysDate = DateTime.Today;
            if (appointDate.Year == 0001)
            {
                appointDate = todaysDate.Date;
            }

            List<AppointmentEnt> appointment = await _appointmentRepository.GetAll().ToListAsync();

            registered = appointment.Count(e => e.Status.Equals(StatusType.Registered) && e.AppDateTime.Date == appointDate.Date);
            checkIn = appointment.Count(e => e.Status.Equals(StatusType.In) && e.AppDateTime.Date == appointDate.Date);
            checkOut = appointment.Count(e => e.Status.Equals(StatusType.Out) && e.AppDateTime.Date == appointDate.Date);
            cancel = appointment.Count(e => e.Status.Equals(StatusType.Cancel) && e.AppDateTime.Date == appointDate.Date);
            totalToday = appointment.Count(e => e.AppDateTime.Date == appointDate.Date);


            var output = new VMSDashboardAppoitnmentLookupTableDto
            {
                TotalToday = totalToday,
                Register = registered,
                CheckIn = checkIn,
                CheckOut = checkOut,
                Cancel = cancel,
            };

            return output;
        }

    }
}
