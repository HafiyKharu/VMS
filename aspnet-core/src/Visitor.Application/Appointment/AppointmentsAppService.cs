using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Visitor.Authorization;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Visitor.PurposeOfVisit.Dtos;
using Visitor.PurposeOfVisit;
using Visitor.Title;
using Visitor.Tower;
using Visitor.Company;
using Visitor.Title.Dtos;
using Visitor.Tower.Dtos;
using Visitor.Level;
using Visitor.Level.Dtos;
using Visitor.Company.Dtos;
using Visitor.Departments;
using Visitor.Departments.Dtos;
using Visitor.Storage;
using Visitor.Authorization.Users.Profile;
using Abp.Collections.Extensions;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using UserIdentifier = Abp.UserIdentifier;
using Visitor.Migrations;
using Microsoft.AspNetCore.Mvc;
using Abp.AspNetZeroCore.Net;
using ImageDrawing = System.Drawing.Image;
using System.Reflection.Metadata;
using Visitor.Common;
using System.Threading;
using Stripe;
using Twilio.Types;
using NPOI.SS.Formula.Functions;
using Visitor.Appointments;
using SixLabors.ImageSharp.Formats;
using Abp.UI;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Visitor.core.Portal;
using Visitor.Appointment.ExpiredUrl;
using Visitor.Dto;
using Visitor.Appointment.Exporting;
/*using StatusEnum = Visitor.Appointments.StatusEnum;*/

namespace Visitor.Appointment
{
    public class AppointmentsAppService : VisitorAppServiceBase, IAppointmentsAppService
    {
        private readonly IRepository<AppointmentEnt, Guid> _appointmentRepository;
        private readonly IRepository<PurposeOfVisitEnt, Guid> _purposeOfVisitRepository;
        private readonly IRepository<TitleEnt, Guid> _titleRepository;
        private readonly IRepository<TowerEnt, Guid> _towerRepository;
        private readonly IRepository<LevelEnt, Guid> _levelRepository;
        private readonly IRepository<CompanyEnt, Guid> _companyRepository;
        private readonly IRepository<Department, Guid> _departmentRepository;
        private readonly ITempFileCacheManager _tempFileCacheManager;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly ProfileImageServiceFactory _profileImageServiceFactory;
        private const int MaxPictureBytes = 5242880; //5MB

        private readonly IPortalEmailer _portalEmailer;
        private readonly IRepository<ExpiredUrlEnt, Guid> _expiredUrlRepository;
        private readonly IExpiredUrlsAppService _expiredUrl;
        private readonly IAppointmentExcelExporter _appointmentExcelExporter;


        public AppointmentsAppService

            (IRepository<AppointmentEnt, Guid> appointmentRepository,
            IRepository<PurposeOfVisitEnt, Guid> purposeOfVisitRepository,
            IRepository<TitleEnt, Guid> titleRepository,
            IRepository<TowerEnt, Guid> towerRepository,
            IRepository<LevelEnt, Guid> levelRepository,
            IRepository<CompanyEnt, Guid> companyRepository,
            IRepository<Department, Guid> departmentRepository,
            ITempFileCacheManager tempFileCacheManager,
            IBinaryObjectManager binaryObjectManager,
            ProfileImageServiceFactory profileImageServiceFactory,
            IPortalEmailer portalEmailer,
            IRepository<ExpiredUrlEnt, Guid> expiredUrlsRepository,
            IExpiredUrlsAppService expiredUrl,
            IAppointmentExcelExporter appointmentExcelExporter
            )
        {
            _appointmentRepository = appointmentRepository;
            _purposeOfVisitRepository = purposeOfVisitRepository;
            _titleRepository = titleRepository;
            _towerRepository = towerRepository;
            _levelRepository = levelRepository;
            _companyRepository = companyRepository;
            _departmentRepository = departmentRepository;
            _tempFileCacheManager = tempFileCacheManager;
            _binaryObjectManager = binaryObjectManager;
            _profileImageServiceFactory = profileImageServiceFactory;
            _portalEmailer = portalEmailer;
            _expiredUrlRepository = expiredUrlsRepository;
            _expiredUrl = expiredUrl;
            _appointmentExcelExporter = appointmentExcelExporter;



        }
        protected DateTime GetToday()
        {
            return DateTime.Now.Date;
            /*DateOnly testTimeOnly = DateOnly.FromDateTime(now);
            return testTimeOnly; */
        }
        protected DateTime GetTomorrow()
        {
            return DateTime.Now.AddDays(1).Date;
        }
        protected DateTime GetYesterday()
        {
            return DateTime.Now.AddDays(-1).Date;
        }


        public async Task<PagedResultDto<GetAppointmentForViewDto>> GetAll(GetAllAppointmentsInput input)
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


            var todaysDate = DateTime.Today;

            var statusEnumFilter = input.StatusFilter.HasValue
                        ? (StatusType)input.StatusFilter
                        : default;

            /*var serviceType = 0;*/

            var filteredAppointments = _appointmentRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.FullName.Contains(input.Filter) || e.IdentityCard.Contains(input.Filter) || e.PhoneNo.Contains(input.Filter) || e.Email.Contains(input.Filter) || e.PassNumber.Contains(input.Filter) || e.OfficerToMeet.Contains(input.Filter) || e.AppRefNo.Contains(input.Filter))





                        .WhereIf(!string.IsNullOrWhiteSpace(input.FullNameFilter), e => e.FullName == input.FullNameFilter)
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
            //.WhereIf(input.AppDateTimeFilter != null, e => e.AppDateTime == input.AppDateTimeFilter );



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

            var totalCount = await filteredAppointments.CountAsync();

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
                        AppDateTime = o.AppDateTime.ToString("dddd, dd MMMM yyyy hh:mm tt"),
                        RegDateTime = o.CreationTime.ToString("dddd, dd MMMM yyyy hh:mm tt"),
                        Status = o.Status,
                        Title = o.Title,
                        ImageId = o.ImageId,
                        //ImageId = image.Id,,
                        AppRefNo = o.AppRefNo,
                        PassNumber = o.PassNumber,
                        CheckInDateTime = o.CheckInDateTime.ToString(" dd/MM hh:mm tt"),
                        CheckOutDateTime = o.CheckOutDateTime.ToString("dd/MM hh:mm tt"),
                        CancelDateTime = o.CancelDateTime.ToString("dd/MM hh:mm tt"),
                        EmailOfficerToMeet = o.EmailOfficerToMeet,
                        PhoneNoOfficerToMeet = o.PhoneNoOfficerToMeet
                    }
                };

                results.Add(res);
            }

            return new PagedResultDto<GetAppointmentForViewDto>(
                totalCount,
                results
            );

        }
        public async Task<PagedResultDto<GetAppointmentForViewDto>> GetAllToday(GetAllAppointmentsInput input)
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

            var serviceType = 0;


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
                                   o.CancelDateTime,
                                   o.EmailOfficerToMeet,
                                   o.PhoneNoOfficerToMeet
                               };

            var totalCount = await filteredAppointments.CountAsync();

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
                        AppDateTime = o.AppDateTime.ToString("dddd, dd MMMM yyyy hh:mm tt"),
                        RegDateTime = o.CreationTime.ToString("dddd, dd MMMM yyyy hh:mm tt"),
                        Status = o.Status,
                        Title = o.Title,
                        ImageId = o.ImageId,
                        //ImageId = image.Id,,
                        AppRefNo = o.AppRefNo,
                        PassNumber = o.PassNumber,
                        CheckInDateTime = o.CheckInDateTime.ToString("dd/MM hh:mm tt"),
                        CheckOutDateTime = o.CheckOutDateTime.ToString("dd/MM hh:mm tt"),
                        CancelDateTime = o.CancelDateTime.ToString("dd/MM hh:mm tt"),
                        EmailOfficerToMeet = o.EmailOfficerToMeet,
                        PhoneNoOfficerToMeet = o.PhoneNoOfficerToMeet
                    }
                };

                results.Add(res);
            }

            return new PagedResultDto<GetAppointmentForViewDto>(
                totalCount,
                results
            );

        }
        public async Task<PagedResultDto<GetAppointmentForViewDto>> GetAllTomorrow(GetAllAppointmentsInput input)
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

            DateTime d2 = GetTomorrow();

            var todaysDate = DateTime.Today;

            var statusEnumFilter = input.StatusFilter.HasValue
                        ? (StatusType)input.StatusFilter
                        : default;


            var filteredAppointments = _appointmentRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.FullName.Contains(input.Filter) || e.IdentityCard.Contains(input.Filter) || e.PhoneNo.Contains(input.Filter) || e.Email.Contains(input.Filter) || e.PassNumber.Contains(input.Filter) || e.OfficerToMeet.Contains(input.Filter) || e.OfficerToMeet.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.FullNameFilter), e => e.FullName == input.FullNameFilter)
                        .Where(e => e.AppDateTime.Date == d2)

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

            var totalCount = await filteredAppointments.CountAsync();

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
                        AppDateTime = o.AppDateTime.ToString("dddd, dd MMMM yyyy hh:mm tt"),
                        RegDateTime = o.CreationTime.ToString("dddd, dd MMMM yyyy hh:mm tt"),
                        Status = o.Status,
                        Title = o.Title,
                        ImageId = o.ImageId,
                        //ImageId = image.Id,,
                        AppRefNo = o.AppRefNo,
                        PassNumber = o.PassNumber,
                        CheckInDateTime = o.CheckInDateTime.ToString("dd/MM hh:mm tt"),
                        CheckOutDateTime = o.CheckOutDateTime.ToString("dd/MM hh:mm tt"),
                        CancelDateTime = o.CancelDateTime.ToString("dd/MM hh:mm tt"),
                        EmailOfficerToMeet = o.EmailOfficerToMeet,
                        PhoneNoOfficerToMeet = o.PhoneNoOfficerToMeet
                    }
                };

                results.Add(res);
            }

            return new PagedResultDto<GetAppointmentForViewDto>(
                totalCount,
                results
            );

        }
        public async Task<PagedResultDto<GetAppointmentForViewDto>> GetAllYesterday(GetAllAppointmentsInput input)
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

            DateTime d3 = GetYesterday();

            var todaysDate = DateTime.Today;

            var statusEnumFilter = input.StatusFilter.HasValue
                        ? (StatusType)input.StatusFilter
                        : default;

            var serviceType = 0;

            var filteredAppointments = _appointmentRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.FullName.Contains(input.Filter) || e.IdentityCard.Contains(input.Filter) || e.PhoneNo.Contains(input.Filter) || e.Email.Contains(input.Filter) || e.PassNumber.Contains(input.Filter) || e.OfficerToMeet.Contains(input.Filter) || e.OfficerToMeet.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.FullNameFilter), e => e.FullName == input.FullNameFilter)
                        .Where(e => e.AppDateTime.Date == d3)

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

            var totalCount = await filteredAppointments.CountAsync();

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
                        AppDateTime = o.AppDateTime.ToString("dddd, dd MMMM yyyy hh:mm tt"),
                        RegDateTime = o.CreationTime.ToString("dddd, dd MMMM yyyy hh:mm tt"),
                        Status = o.Status,
                        Title = o.Title,
                        ImageId = o.ImageId,
                        //ImageId = image.Id,,
                        AppRefNo = o.AppRefNo,
                        PassNumber = o.PassNumber,
                        CheckInDateTime = o.CheckInDateTime.ToString("dd/MM hh:mm tt"),
                        CheckOutDateTime = o.CheckOutDateTime.ToString("dd/MM hh:mm tt"),
                        CancelDateTime = o.CancelDateTime.ToString("dd/MM hh:mm tt"),
                        EmailOfficerToMeet = o.EmailOfficerToMeet,
                        PhoneNoOfficerToMeet = o.PhoneNoOfficerToMeet
                    }
                };

                results.Add(res);
            }

            return new PagedResultDto<GetAppointmentForViewDto>(
                totalCount,
                results
            );

        }
        public async Task<GetExpiredUrlForViewDto> CheckExpiredUrl(Guid? appointmentId, string Item)
        {
            var expiredUrl = await _expiredUrlRepository.FirstOrDefaultAsync(x => x.AppointmentId == appointmentId && x.Item == Item);
            var output = new GetExpiredUrlForViewDto { ExpiredUrl = ObjectMapper.Map<ExpiredUrlDto>(expiredUrl) };

            return output;
        }

        public async Task<GetAppointmentForViewDto> GetAppointmentForView(Guid id)
        {
            var appointment = await _appointmentRepository.GetAsync(id);

            // var output = new GetAppointmentForViewDto { Appointment = ObjectMapper.Map<AppointmentDto>(appointment) };

            var output = new GetAppointmentForViewDto
            {
                Appointment = new AppointmentDto
                {
                    Id = appointment.Id,
                    FullName = appointment.FullName,
                    PhoneNo = appointment.PhoneNo,
                    Email = appointment.Email,
                    Title = appointment.Title,
                    CompanyName = appointment.CompanyName,
                    OfficerToMeet = appointment.OfficerToMeet,
                    PurposeOfVisit = appointment.PurposeOfVisit,
                    Department = appointment.Department,
                    Tower = appointment.Tower,
                    Level = appointment.Level,
                    AppDateTime = appointment.AppDateTime.ToString("dd/MM/yy, hh:mm tt"),
                    RegDateTime = appointment.CreationTime.ToString("dd/MM/yy, hh:mm tt"),
                    Status = appointment.Status,
                    ImageId = appointment.ImageId,
                    AppRefNo = appointment.AppRefNo,
                    PassNumber = appointment.PassNumber,
                    CheckInDateTime = appointment.CheckInDateTime.ToString("dd/MM hh:mm tt"),
                    CheckOutDateTime = appointment.CheckOutDateTime.ToString("dd/MM hh:mm tt"),
                    CancelDateTime = appointment.CancelDateTime.ToString("dd/MM hh:mm tt"),
                    EmailOfficerToMeet = appointment.EmailOfficerToMeet,
                    PhoneNoOfficerToMeet = appointment.PhoneNoOfficerToMeet
                }
            };

            return output;
        }

        /*[AbpAuthorize(AppPermissions.Pages_Appointments_Edit)]*/
        public async Task<GetAppointmentForEditOutput> GetAppointmentForEdit(EntityDto<Guid> input)
        {
            var appointment = await _appointmentRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetAppointmentForEditOutput { Appointment = ObjectMapper.Map<CreateOrEditAppointmentDto>(appointment) };

            return output;
        }

        public async Task CreateOrEdit(CreateOrEditAppointmentDto input)
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

        /*public async Task Validation()
        {
            var currentDate = GetToday();
            int total = _appointmentRepository.Count();
            
            for(int i = 1; i <= total ; i++)
            {

                var bookhCheck = _appointmentRepository.GetAll().Where(e => e.CheckInDateTime < currentDate && e.Status == StatusType.In);
                *//*if (bookhCheck?.Id != null)
                {
                    var appointment = await _appointmentRepository.GetAsync((Guid)bookhCheck.Id);
                    appointment.Status = StatusType.Overstayed;
                }*//*
            }
            
        }*/




        [AbpAuthorize(AppPermissions.Pages_Appointments_Create)]
        protected virtual async Task Create(CreateOrEditAppointmentDto input)
        {

            var pn = input.PhoneNo;
            var ic = input.IdentityCard;
            //input.MobileNumber = "+6" + input.MobileNumber; 
            var rand = new Random();
            int num = rand.Next(1000);
            var date = input.AppDateTime.Date.ToString("yyMMdd");
            var lang = Thread.CurrentThread.CurrentCulture;
            input.Status = 0;
            /*string lastFourDigitsPN = pn.Substring(pn.Length - 4, 4);
            string lastFourDigitsIC = ic.Substring(pn.Length - 2, 4);*/
            //input.AppointmentDate = input.AppointmentDate.ToShortDateString();
            input.AppRefNo = "AR" + date + num;
            /*var appointment = ObjectMapper.Map<AppointmentEnt>(input);*/
            /* var idImage = UpdatePictureForAppointment();
             input.ImageId = idImage.ToString();*/
            /*var res = await UpdatePictureForAppointment(input.FileToken, input.X, input.Y, input.Width, input.Height);
            input.ImageId = res.ToString();*/

            byte[] byteArray;
            var imageBytes = _tempFileCacheManager.GetFile(input.FileToken);

            if (input.Tower == "Tower 1")
            {
                input.CompanyName = "Bank Rakyat";
            }

            if (imageBytes == null)
            {
                throw new UserFriendlyException("There is no such image file with the token: " + input.FileToken);
            }

            using (var image = Image.Load(imageBytes, out IImageFormat format))
            {
                var width = (input.Width == 0 || input.Width > image.Width) ? image.Width : input.Width;
                var height = (input.Height == 0 || input.Height > image.Height) ? image.Height : input.Height;

                var bmCrop = image.Clone(i =>
                    i.Crop(new Rectangle(input.X, input.Y, width, height))
                );

                await using (var stream = new MemoryStream())
                {
                    await bmCrop.SaveAsync(stream, format);
                    byteArray = stream.ToArray();
                }
            }

            if (byteArray.Length > MaxPictureBytes)
            {
                throw new UserFriendlyException(L("ResizedProfilePicture_Warn_SizeLimit",
                    AppConsts.ResizedMaxProfilePictureBytesUserFriendlyValue));
            }
            var storedFile = new BinaryObject(AbpSession.TenantId, byteArray, $"Appointment picture at {DateTime.UtcNow}");
            await _binaryObjectManager.SaveAsync(storedFile);

            input.ImageId = storedFile.Id.ToString();


            var checker = true;
            while (checker)
            {
                var bookhCheck = _appointmentRepository.FirstOrDefault(e => e.AppRefNo == input.AppRefNo);
                if (bookhCheck?.Id != null)
                {
                    /*string PhoneNo = "";*/

                    //if duplicate booking ref no
                    checker = true;
                    num = rand.Next(1000);
                    /*string lastFourDigits = pn.Substring(pn.Length - 4, 4);*/
                    input.AppRefNo = "AR" + date + num;

                }
                else
                {
                    checker = false;
                    break;
                }
            };



            var appointment = ObjectMapper.Map<AppointmentEnt>(input);
            await _appointmentRepository.InsertAsync(appointment);
            /*await UpdateSlot(input.BranchSlotSettingId, input.AppointmentSlot);*/
            /*var bookingDetail = await _bookingRepository.InsertAsync(booking);
            var branch = await _lookup_branchRepository.GetAsync(input.BranchId.Value);*/

            /*if (input.Email != null)
            {
                try
                {
                    await _portalEmailer.SendEmailDetailBookingAsync(ObjectMapper.Map<Booking>(bookingDetail), branch, service);
                }
                catch
                {
                    //To do
                }
            }

            try
            {
                await SendSms(input);
            }
            catch
            {

            }*/

            /*if (input.Email != null)
            {
                try
                {
                    await _portalEmailer.SendEmailDetailBookingAsync(ObjectMapper.Map<Booking>(bookingDetail), branch, service);
                }
                catch
                {
                    //To do
                }
            }

            try
            {
                await SendSms(input);
            }
            catch
            {

            }*/
            var appointmentDetail = await _appointmentRepository.InsertAsync(appointment);

            if (input.Email != null)
            {
                try
                {
                    await _portalEmailer.SendEmailDetailAppointmentAsync(ObjectMapper.Map<AppointmentEnt>(appointmentDetail));
                }
                catch
                {
                    //To do 20210815
                }

            }
        }

        [AbpAuthorize(AppPermissions.Pages_Appointments_Edit)]
        protected virtual async Task Update(CreateOrEditAppointmentDto input)
        {
            var appointment = await _appointmentRepository.FirstOrDefaultAsync((Guid)input.Id);
            ObjectMapper.Map(input, appointment);

        }

        [AbpAuthorize(AppPermissions.Pages_Appointments_Delete)]
        public async Task Delete(EntityDto<Guid> input)
        {
            await _appointmentRepository.DeleteAsync(input.Id);
        }

        public List<GetPurposeOfVisitForViewDto> GetPurposeOfVisit()
        {
            var query = _purposeOfVisitRepository.GetAll();
            var queryPOV = from a in query
                           select new
                           {
                               Id = a.Id,
                               a.PurposeOfVisitApp
                           };
            var dblist = queryPOV.ToList();
            var results = new List<GetPurposeOfVisitForViewDto>();
            foreach (var db in dblist)
            {
                var res = new GetPurposeOfVisitForViewDto()
                {
                    PurposeOfVisit = new PurposeOfVisitDto
                    {
                        Id = db.Id,
                        PurposeOfVisitApp = db.PurposeOfVisitApp,
                    }
                };
                results.Add(res);
            }

            return new List<GetPurposeOfVisitForViewDto>(results);
        }

        public List<GetTitleForViewDto> GetTitle()
        {
            var query = _titleRepository.GetAll();
            var qTitle = from a in query
                         select new
                         {
                             Id = a.Id,
                             a.VisitorTitle
                         };
            var dblist = qTitle.ToList();
            var results = new List<GetTitleForViewDto>();
            foreach (var db in dblist)
            {
                var res = new GetTitleForViewDto()
                {
                    Title = new TitleDto
                    {
                        Id = db.Id,
                        VisitorTitle = db.VisitorTitle,
                    }
                };
                results.Add(res);
            }

            return new List<GetTitleForViewDto>(results);
        }
        public List<GetTowerForViewDto> GetTower()
        {
            var query = _towerRepository.GetAll();
            var qTower = from a in query
                         select new
                         {
                             Id = a.Id,
                             a.TowerBankRakyat
                         };
            var dblist = qTower.ToList();
            var results = new List<GetTowerForViewDto>();
            foreach (var db in dblist)
            {
                var res = new GetTowerForViewDto()
                {
                    Tower = new TowerDto
                    {
                        Id = db.Id,
                        TowerBankRakyat = db.TowerBankRakyat,
                    }
                };
                results.Add(res);
            }

            return new List<GetTowerForViewDto>(results);
        }
        /* public List<GetLevelForViewDto> GetLevel()
         {
             var query = _levelRepository.GetAll();
             var qLevel = from a in query
                          select new
                          {
                              Id = a.Id,
                              a.LevelBankRakyat
                          };
             var dblist = qLevel.ToList();
             var results = new List<GetLevelForViewDto>();
             foreach (var db in dblist)
             {
                 var res = new GetLevelForViewDto()
                 {
                     Level = new LevelDto
                     {
                         Id = db.Id,
                         LevelBankRakyat = db.LevelBankRakyat
                     }
                 };
                 results.Add(res);
             }
             return new List<GetLevelForViewDto>(results);
         }*/

        public List<GetLevelForViewDto> GetLevel()
        {
            var query = _levelRepository.GetAll();
            var qLevel = from a in query
                         orderby Convert.ToInt32(a.LevelBankRakyat) ascending
                         select new
                         {
                             Id = a.Id,
                             a.LevelBankRakyat
                         };
            var dblist = qLevel.ToList();
            var results = new List<GetLevelForViewDto>();
            foreach (var db in dblist)
            {
                var res = new GetLevelForViewDto()
                {
                    Level = new LevelDto
                    {
                        Id = db.Id,
                        LevelBankRakyat = db.LevelBankRakyat
                    }
                };
                results.Add(res);
            }
            return new List<GetLevelForViewDto>(results);
        }

        public List<GetCompanyForViewDto> GetCompanyName()
        {
            var query = _companyRepository.GetAll();
            var qCompany = from a in query
                           select new
                           {
                               Id = a.Id,
                               a.CompanyName,
                               a.OfficePhoneNumber,
                               a.CompanyEmail,
                               a.CompanyAddress
                           };
            var dblist = qCompany.ToList();
            var results = new List<GetCompanyForViewDto>();
            foreach (var db in dblist)
            {
                var res = new GetCompanyForViewDto()
                {
                    Company = new CompanyDto
                    {
                        Id = db.Id,
                        CompanyName = db.CompanyName,
                        OfficePhoneNumber = db.OfficePhoneNumber,
                        CompanyEmail = db.CompanyEmail,
                        CompanyAddress = db.CompanyAddress
                    }
                };
                results.Add(res);
            }

            return new List<GetCompanyForViewDto>(results);
        }
        public List<GetDepartmentForViewDto> GetDepartmentName()
        {
            var query = _departmentRepository.GetAll();
            var qDepartment = from a in query
                              select new
                              {
                                  Id = a.Id,
                                  a.DepartmentName
                              };
            var dblist = qDepartment.ToList();
            var results = new List<GetDepartmentForViewDto>();
            foreach (var db in dblist)
            {
                var res = new GetDepartmentForViewDto()
                {
                    Department = new DepartmentDto
                    {
                        Id = db.Id,
                        DepartmentName = db.DepartmentName,
                    }
                };
                results.Add(res);
            }
            return new List<GetDepartmentForViewDto>(results);
        }


        // Upload image services (referring to profile services)
        /*protected async Task<Guid> UpdatePictureForAppointment(string inputFileToken, int xInput, int yInput, int widthInput, int heightInput)
        {

            byte[] byteArray;
            var imageBytes = _tempFileCacheManager.GetFile(inputFileToken);

            if (imageBytes == null)
            {
                throw new UserFriendlyException("There is no such image file with the token: " + inputFileToken);
            }

            using(var image = Image.Load(imageBytes, out IImageFormat format))
            {
                var width = (widthInput == 0 || widthInput > image.Width) ? image.Width : widthInput;
                var height = (heightInput == 0 || heightInput > image.Height) ? image.Height : heightInput;

                var bmCrop = image.Clone(i =>
                    i.Crop(new Rectangle(xInput, yInput, width, height))
                );

                await using (var stream = new MemoryStream())
                {
                    await bmCrop.SaveAsync(stream, format);
                    byteArray = stream.ToArray();
                }
            }

            if (byteArray.Length > MaxPictureBytes)
            {
                throw new UserFriendlyException(L("ResizedProfilePicture_Warn_SizeLimit",
                    AppConsts.ResizedMaxProfilePictureBytesUserFriendlyValue));
            }
            var storedFile = new BinaryObject(AbpSession.TenantId, byteArray, $"Appointment picture at {DateTime.UtcNow}");
            await _binaryObjectManager.SaveAsync(storedFile);

            var picId = storedFile.Id;
            return picId;
        }*/

        public async Task<byte[]> GetPictureByIdOrNull(Guid imageId)
        {
            var file = await _binaryObjectManager.GetOrNullAsync(imageId);
            if (file == null)
            {
                return null;
            }
            return file.Bytes;
        }

        public async Task<string> GetFilePictureByIdOrNull(Guid imageId)
        {
            var output = await _binaryObjectManager.GetOrNullAsync(imageId);
            //var memoryStream = new MemoryStream(output.Bytes);
            //return ImageDrawing.FromStream(memoryStream);
            //byte[] bytes = output.Bytes;
            //return File(bytes);
            //Blob blob = new Blob(memoryStream.GetBuffer());

            using (MemoryStream ms = new MemoryStream(output.Bytes))
            {
                var image = ImageDrawing.FromStream(ms);
                string base64 = Convert.ToBase64String(output.Bytes);
                return base64;
            }

        }

        public async Task<GetPictureOutput> GetPictureByAppointment(Guid appId)
        {
            var output = await _appointmentRepository.GetAsync(appId);

            return new GetPictureOutput(output.ImageId);
        }

        public async Task ChangeStatusToIn(CreateOrEditAppointmentDto input)
        {
            DateTime now = DateTime.Now;
            await Update(input);

            var appointment = await _appointmentRepository.GetAsync((Guid)input.Id);
            appointment.Status = StatusType.In;
            appointment.CheckInDateTime = now;

        }
        public async Task ChangeStatusToOut(CreateOrEditAppointmentDto input)
        {
            DateTime now = DateTime.Now;
            await Update(input);

            var appointment = await _appointmentRepository.GetAsync((Guid)input.Id);
            appointment.Status = StatusType.Out;
            appointment.CheckOutDateTime = now;

        }
        public async Task CancelAppointment(CreateOrEditAppointmentDto input)
        {
            DateTime now = DateTime.Now;
            await Update(input);

            var appointment = await _appointmentRepository.GetAsync((Guid)input.Id);
            appointment.Status = StatusType.Cancel;
            appointment.CancelDateTime = now;

        }

        public async Task<FileDto> GetAllAppointmentsToExcel(GetAllAppointmentForExcelInput input)
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


            var todaysDate = DateTime.Today;

            var statusEnumFilter = input.StatusFilter.HasValue
                        ? (StatusType)input.StatusFilter
                        : default;

            var filteredAppointments = _appointmentRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.FullName.Contains(input.Filter) || e.IdentityCard.Contains(input.Filter) || e.PhoneNo.Contains(input.Filter) || e.Email.Contains(input.Filter) || e.PassNumber.Contains(input.Filter) || e.OfficerToMeet.Contains(input.Filter) || e.AppRefNo.Contains(input.Filter))

                        .WhereIf(!string.IsNullOrWhiteSpace(input.FullNameFilter), e => e.FullName == input.FullNameFilter)
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

            var query = (from o in filteredAppointments
                         select new GetAppointmentForViewDto()
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
                                 AppDateTime = o.AppDateTime.ToString("dddd, dd MMMM yyyy hh:mm tt"),
                                 RegDateTime = o.CreationTime.ToString("dddd, dd MMMM yyyy hh:mm tt"),
                                 Status = o.Status,
                                 Title = o.Title,
                                 ImageId = o.ImageId,
                                 AppRefNo = o.AppRefNo,
                                 PassNumber = o.PassNumber,
                                 CheckInDateTime = o.CheckInDateTime.ToString(" dd/MM hh:mm tt"),
                                 CheckOutDateTime = o.CheckOutDateTime.ToString("dd/MM hh:mm tt"),
                                 CancelDateTime = o.CancelDateTime.ToString("dd/MM hh:mm tt"),
                                 EmailOfficerToMeet = o.EmailOfficerToMeet,
                                 PhoneNoOfficerToMeet = o.PhoneNoOfficerToMeet
                             }
                         });

            var test = await query.ToListAsync();

            return _appointmentExcelExporter.ExportToFile(test);
        }
        public async Task<FileDto> GetAllTodayAppointmentsToExcel(GetAllAppointmentForExcelInput input)
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

            var serviceType = 0;


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

            var query = (from o in filteredAppointments
                         select new GetAppointmentForViewDto()
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
                                 AppDateTime = o.AppDateTime.ToString("dddd, dd MMMM yyyy hh:mm tt"),
                                 RegDateTime = o.CreationTime.ToString("dddd, dd MMMM yyyy hh:mm tt"),
                                 Status = o.Status,
                                 Title = o.Title,
                                 ImageId = o.ImageId,
                                 AppRefNo = o.AppRefNo,
                                 PassNumber = o.PassNumber,
                                 CheckInDateTime = o.CheckInDateTime.ToString(" dd/MM hh:mm tt"),
                                 CheckOutDateTime = o.CheckOutDateTime.ToString("dd/MM hh:mm tt"),
                                 CancelDateTime = o.CancelDateTime.ToString("dd/MM hh:mm tt"),
                                 EmailOfficerToMeet = o.EmailOfficerToMeet,
                                 PhoneNoOfficerToMeet = o.PhoneNoOfficerToMeet
                             }
                         });

            var test = await query.ToListAsync();

            return _appointmentExcelExporter.ExportToFile(test);
        }
        public async Task<FileDto> GetAllTomorrowAppointmentsToExcel(GetAllAppointmentForExcelInput input)
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

            DateTime d2 = GetTomorrow();

            var todaysDate = DateTime.Today;

            var statusEnumFilter = input.StatusFilter.HasValue
                        ? (StatusType)input.StatusFilter
                        : default;


            var filteredAppointments = _appointmentRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.FullName.Contains(input.Filter) || e.IdentityCard.Contains(input.Filter) || e.PhoneNo.Contains(input.Filter) || e.Email.Contains(input.Filter) || e.PassNumber.Contains(input.Filter) || e.OfficerToMeet.Contains(input.Filter) || e.OfficerToMeet.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.FullNameFilter), e => e.FullName == input.FullNameFilter)
                        .Where(e => e.AppDateTime.Date == d2)

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

            var query = (from o in filteredAppointments
                         select new GetAppointmentForViewDto()
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
                                 AppDateTime = o.AppDateTime.ToString("dddd, dd MMMM yyyy hh:mm tt"),
                                 RegDateTime = o.CreationTime.ToString("dddd, dd MMMM yyyy hh:mm tt"),
                                 Status = o.Status,
                                 Title = o.Title,
                                 ImageId = o.ImageId,
                                 AppRefNo = o.AppRefNo,
                                 PassNumber = o.PassNumber,
                                 CheckInDateTime = o.CheckInDateTime.ToString(" dd/MM hh:mm tt"),
                                 CheckOutDateTime = o.CheckOutDateTime.ToString("dd/MM hh:mm tt"),
                                 CancelDateTime = o.CancelDateTime.ToString("dd/MM hh:mm tt"),
                                 EmailOfficerToMeet = o.EmailOfficerToMeet,
                                 PhoneNoOfficerToMeet = o.PhoneNoOfficerToMeet
                             }
                         });

            var test = await query.ToListAsync();

            return _appointmentExcelExporter.ExportToFile(test);
        }
        public async Task<FileDto> GetAllYesterdayAppointmentsToExcel(GetAllAppointmentForExcelInput input)
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

            DateTime d3 = GetYesterday();

            var todaysDate = DateTime.Today;

            var statusEnumFilter = input.StatusFilter.HasValue
                        ? (StatusType)input.StatusFilter
                        : default;

            var serviceType = 0;

            var filteredAppointments = _appointmentRepository.GetAll()
                        .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), e => false || e.FullName.Contains(input.Filter) || e.IdentityCard.Contains(input.Filter) || e.PhoneNo.Contains(input.Filter) || e.Email.Contains(input.Filter) || e.PassNumber.Contains(input.Filter) || e.OfficerToMeet.Contains(input.Filter) || e.OfficerToMeet.Contains(input.Filter))
                        .WhereIf(!string.IsNullOrWhiteSpace(input.FullNameFilter), e => e.FullName == input.FullNameFilter)
                        .Where(e => e.AppDateTime.Date == d3)

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

            var query = (from o in filteredAppointments
                         select new GetAppointmentForViewDto()
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
                                 AppDateTime = o.AppDateTime.ToString("dddd, dd MMMM yyyy hh:mm tt"),
                                 RegDateTime = o.CreationTime.ToString("dddd, dd MMMM yyyy hh:mm tt"),
                                 Status = o.Status,
                                 Title = o.Title,
                                 ImageId = o.ImageId,
                                 AppRefNo = o.AppRefNo,
                                 PassNumber = o.PassNumber,
                                 CheckInDateTime = o.CheckInDateTime.ToString(" dd/MM hh:mm tt"),
                                 CheckOutDateTime = o.CheckOutDateTime.ToString("dd/MM hh:mm tt"),
                                 CancelDateTime = o.CancelDateTime.ToString("dd/MM hh:mm tt"),
                                 EmailOfficerToMeet = o.EmailOfficerToMeet,
                                 PhoneNoOfficerToMeet = o.PhoneNoOfficerToMeet
                             }
                         });

            var test = await query.ToListAsync();

            return _appointmentExcelExporter.ExportToFile(test);
        }
    }
}
