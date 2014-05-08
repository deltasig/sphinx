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
        IChoresRepository ChoreRepository { get; }
        IChoreGroupsRepository ChoreGroupRepository { get; }
        IChoreAssignmentsRepository ChoreAssignmentRepository { get; }
        IChoreClassesRepository ChoreClassRepository { get; }
        IChoreGroupTypesRepository ChoreGroupTypeRepository { get; }
        IStudyHoursRepository StudyHourRepository { get; }
        IIncidentReportsRepository IncidentReportRepository { get; }
        void Save();
    }
}
