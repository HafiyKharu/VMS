using Abp.Application.Services;
using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Visitor.Authorization.Users.Profile.Dto;
using Visitor.Departments.Dtos;
using Visitor.Dto;

namespace Visitor.Appointment
{
    public interface IAppointmentsAppService : IApplicationService
    {
        Task<PagedResultDto<GetAppointmentForViewDto>> GetAll(GetAllAppointmentsInput input);
        Task<PagedResultDto<GetAppointmentForViewDto>> GetAllToday(GetAllAppointmentsInput input);
        Task<PagedResultDto<GetAppointmentForViewDto>> GetAllTomorrow(GetAllAppointmentsInput input);
        Task<PagedResultDto<GetAppointmentForViewDto>> GetAllYesterday(GetAllAppointmentsInput input);

        Task<GetAppointmentForViewDto> GetAppointmentForView(Guid id);

        Task<GetAppointmentForEditOutput> GetAppointmentForEdit(EntityDto<Guid> input);

        Task CreateOrEdit(CreateOrEditAppointmentDto input);

        Task Delete (EntityDto<Guid> input);
        List<GetDepartmentForViewDto> GetDepartmentName();

        //Upload image services (referring to profile services)
        //Task<Guid> UpdatePictureForAppointment(string inputFileToken, int xInput, int yInput, int widthInput, int heightInput);
        Task<byte[]> GetPictureByIdOrNull(Guid imageId);
        Task<GetPictureOutput> GetPictureByAppointment(Guid appId);
        Task ChangeStatusToIn(CreateOrEditAppointmentDto input);
        Task ChangeStatusToOut(CreateOrEditAppointmentDto input);
        Task<FileDto> GetAllAppointmentsToExcel(GetAllAppointmentForExcelInput input);
        Task<FileDto> GetAllTodayAppointmentsToExcel(GetAllAppointmentForExcelInput input);
        Task<FileDto> GetAllTomorrowAppointmentsToExcel(GetAllAppointmentForExcelInput input);
        Task<FileDto> GetAllYesterdayAppointmentsToExcel(GetAllAppointmentForExcelInput input);
    }
}
