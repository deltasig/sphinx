namespace DeltaSigmaPhiWebsite.Data.UnitOfWork
{
    using System;
    using Interfaces;

    public interface IUnitOfWork : IDisposable
    {
        IMembersRepository MemberRepository { get; }
        IServiceHoursRepository ServiceHourRepository { get; }
        ISemestersRepository SemesterRepository { get; }
        IEventsRepository EventRepository { get; }
        ILaundrySignupRepository LaundrySignupRepository { get; }
        ISoberDriversRepository SoberDriverRepository { get; }
        ISoberOfficersRepository SoberOfficerRepository { get; }
        IMemberStatusRepository MemberStatusRepository { get; }
        IStudyHoursRepository StudyHourRepository { get; }
        IIncidentReportsRepository IncidentReportRepository { get; }
        IAddressesRepository AddressRepository { get; }
        IPhoneNumbersRepository PhoneNumberRepository { get; }
        ILeadersRepository LeaderRepository { get; }
        IPositionsRepository PositionRepository { get; }
        IPledgeClassesRepository PledgeClassRepository { get; }
        IClassesRepository ClassesRepository { get; }
        IDepartmentsRepository DepartmentsRepository { get; }
        void Save();
    }
}
