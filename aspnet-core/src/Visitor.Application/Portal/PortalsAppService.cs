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
using Visitor;
using Visitor.Appointment;
using System.Threading;
using NPOI.OpenXmlFormats.Dml;
using Abp.Domain.Uow;
using Visitor.Chat;
using Visitor.Portal;
using Abp.UI;
using SixLabors.ImageSharp.Formats;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Visitor.core.Portal;
using Stripe;
using Microsoft.Extensions.Logging;
using Visitor.Appointments;
using Visitor.Appointment.ExpiredUrl;
using NPOI.SS.Formula.Functions;
using Visitor.Blacklist;

namespace Visitor.Portal
{
    public class PortalsAppService : VisitorAppServiceBase, IPortalAppServices
    {
        private readonly IRepository<AppointmentEnt, Guid> _appointmentRepository;
        private readonly IRepository<PurposeOfVisitEnt, Guid> _purposeOfVisitRepository;
        private readonly IRepository<TitleEnt, Guid> _titleRepository;
        private readonly IRepository<TowerEnt, Guid> _towerRepository;
        private readonly IRepository<LevelEnt, Guid> _levelRepository;
        private readonly IRepository<CompanyEnt, Guid> _companyRepository;
        private readonly IRepository<Department, Guid> _departmentRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ITempFileCacheManager _tempFileCacheManager;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private const int MaxPictureBytes = 5242880; //5MB

        private readonly IPortalEmailer _portalEmailer;
        private readonly IRepository<ExpiredUrlEnt, Guid> _expiredUrlRepository;
        private readonly IExpiredUrlsAppService _expiredUrl;
        private readonly IBlacklistsAppService _blacklistsAppService;

        public PortalsAppService

            (IRepository<AppointmentEnt, Guid> appointmentRepository,
            IRepository<PurposeOfVisitEnt, Guid> purposeOfVisitRepository,
            IRepository<TitleEnt, Guid> titleRepository,
            IRepository<TowerEnt, Guid> towerRepository,
            IRepository<LevelEnt, Guid> levelRepository,
            IRepository<CompanyEnt, Guid> companyRepository,
            IRepository<Department, Guid> departmentRepository,
            IUnitOfWorkManager unitOfWorkManager,
            ITempFileCacheManager tempFileCacheManager,
            IBinaryObjectManager binaryObjectManager,
            IPortalEmailer portalEmailer,
            IRepository<ExpiredUrlEnt,Guid> expiredUrlsRepository,
            IExpiredUrlsAppService expiredUrl,
            IBlacklistsAppService blacklistsAppService
            )
        {
            _appointmentRepository = appointmentRepository;
            _purposeOfVisitRepository = purposeOfVisitRepository;
            _titleRepository = titleRepository;
            _towerRepository = towerRepository;
            _levelRepository = levelRepository;
            _companyRepository = companyRepository;
            _departmentRepository = departmentRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _tempFileCacheManager = tempFileCacheManager;
            _binaryObjectManager = binaryObjectManager;
            _portalEmailer = portalEmailer;
            _expiredUrlRepository = expiredUrlsRepository;
            _expiredUrl = expiredUrl;
            _blacklistsAppService = blacklistsAppService;
        }
        public async Task<GetAppointmentForEditOutput> GetAppointmentForEdit(EntityDto<Guid> input)
        {
            var appointment = await _appointmentRepository.FirstOrDefaultAsync(input.Id);

            var output = new GetAppointmentForEditOutput { Appointment = ObjectMapper.Map<CreateOrEditAppointmentDto>(appointment) };

            return output;
        }
        public async Task<GetAppointmentForViewDto> GetAppointmentForView(Guid id)
        {
            var appointment = await _appointmentRepository.GetAsync(id);

            var output = new GetAppointmentForViewDto { Appointment = ObjectMapper.Map<AppointmentDto>(appointment) };

            return output;
        }
        public async Task<Guid> CreateOrEdit(CreateOrEditAppointmentDto input)
        {
            var output = await Create(input);
            return output;
        }

        protected virtual async Task<Guid> Create(CreateOrEditAppointmentDto input)
        {
                var rand = new Random();
                int num = rand.Next(1000);
                var date = input.AppDateTime.Date.ToString("yyMMdd");
                input.Status = 0;
                input.AppRefNo = "AR" + date + num;

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
                        //if duplicate booking ref no
                        checker = true;
                        num = rand.Next(1000);
                        input.AppRefNo = "AR" + date + num;

                    }
                    else
                    {
                        checker = false;
                        break;
                    }
                };

                var appointment = ObjectMapper.Map<AppointmentEnt>(input);

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

                var appId = await _appointmentRepository.InsertAndGetIdAsync(appointment);
                return appId;
            

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
        public List<GetLevelForViewDto> GetLevel()
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

        /*public virtual long Save(ChatMessage message)
        {
            *//*return _unitOfWorkManager.WithUnitOfWork(() =>
            {
                using (CurrentUnitOfWork.SetTenantId(message.TenantId))
                {
                    return _chatMessageRepository.InsertAndGetId(message);
                }
            });*//*

        }*/

        public async Task<String> CreateOrEditExpiredUrl(Guid? appointmentId, string Item)
        {
            //IdBooking = "c4579a27-5106-484e-9376-08d936e61aac";
            var lang = Thread.CurrentThread.CurrentCulture;
            var timeNow = DateTime.Now;
            var appointment = await _appointmentRepository.GetAsync((Guid)appointmentId);
            var input = new CreateOrEditExpiredUrlDto
            {
                UrlCreateDate = DateTime.Now,
                UrlExpiredDate = appointment.AppDateTime.AddMinutes(-30),
                AppointmentId = appointmentId,
                Item = "Cancel",
                Status = "New",
            };
            var checkExpired = await _expiredUrlRepository.FirstOrDefaultAsync(x => x.AppointmentId == appointmentId && x.Item == Item);
            if (checkExpired?.Id != null)
            {
                input.Id = checkExpired.Id;
            }
            await _expiredUrl.CreateOrEdit(input);
            return "Success";
            /*try
            {
                if (Item == "Cancel")
                {
                    await _portalEmailer.SendCancelEmailAsync(appointment);
                }
                else
                {
                    //await _portalEmailer.SendRescheduleEmailAsync(appointment);
                }

                return "Success";
            }
            catch (Exception ex)
            {
                //code for any other type of exception
                Logger.Error("ERROR CancelAppointment");
                Logger.Error(ex.Message);
                return "Error";
            }*/

        }

        public async Task<String> ConfirmCancelAppointment(string appointmentId)
        {
            try
            {
                DateTime now = DateTime.Now;
                var appointment = await _appointmentRepository.GetAsync(Guid.Parse(appointmentId));
                appointment.Status = StatusType.Cancel;
                appointment.CancelDateTime = now;
                await _portalEmailer.SendConfirmCancelEmailAsync(ObjectMapper.Map<AppointmentEnt>(appointment));
                return "Success";
            }
            catch (Exception ex)
            {
                //code for any other type of exception
                Logger.Error("ERROR CancelAppointment");
                Logger.Error(ex.Message);
                return "Error";
            }

        }/*
        public async Task<String> test(string appointmentId)
        {
            try
            {
                DateTime now = DateTime.Now;
                var appointment = await _appointmentRepository.GetAsync(Guid.Parse(appointmentId));
                await _portalEmailer.SendConfirmCancelEmailAsync(ObjectMapper.Map<AppointmentEnt>(appointment));
                return "Success";
            }
            catch (Exception ex)
            {
                //code for any other type of exception
                Logger.Error("ERROR CancelAppointment");
                Logger.Error(ex.Message);
                return "Error";
            }
        }*/

            public async Task<GetAppointmentForEditOutput> GetAppointmentById(Guid appointmentId)
        {
            var appointment = await _appointmentRepository.FirstOrDefaultAsync(appointmentId);

            var output = new GetAppointmentForEditOutput { Appointment = ObjectMapper.Map<CreateOrEditAppointmentDto>(appointment) };

            return output;
        }
    }
    
}
