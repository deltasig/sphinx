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
        ISoberSignupsRepository SoberSignupsRepository { get; }
        IMemberStatusRepository MemberStatusRepository { get; }
        IStudyHoursRepository StudyHourRepository { get; }
        IIncidentReportsRepository IncidentReportRepository { get; }
        IAddressesRepository AddressRepository { get; }
        IPhoneNumbersRepository PhoneNumberRepository { get; }
        ILeadersRepository LeaderRepository { get; }
        IPositionsRepository PositionRepository { get; }
        IPledgeClassesRepository PledgeClassRepository { get; }
        IClassesRepository ClassesRepository { get; }
        IClassesTakenRepository ClassesTakenRepository { get; }
        IDepartmentsRepository DepartmentsRepository { get; }
        void Save();
    }
}
