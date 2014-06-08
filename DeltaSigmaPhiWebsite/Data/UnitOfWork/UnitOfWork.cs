namespace DeltaSigmaPhiWebsite.Data.UnitOfWork
{
    using System;
    using Interfaces;
    using Models;
    using Repositories;

    public class UnitOfWork : IUnitOfWork
    {
        private readonly DspContext context = new DspContext();
        
        private IMembersRepository memberRepository;
        public IMembersRepository MemberRepository
        {
            get { return memberRepository ?? (memberRepository = new MembersRepository(context)); }
        }
        private IServiceHoursRepository serviceHourRepository;
        public IServiceHoursRepository ServiceHourRepository
        {
            get { return serviceHourRepository ?? (serviceHourRepository = new ServiceHoursRepository(context)); }
        }
        private ISemestersRepository semesterRepository;
        public ISemestersRepository SemesterRepository
        {
            get { return semesterRepository ?? (semesterRepository = new SemestersRepository(context)); }
        }
        private IEventsRepository eventRepository;
        public IEventsRepository EventRepository
        {
            get { return eventRepository ?? (eventRepository = new EventsRepository(context)); }
        }
        private ILaundrySignupRepository laundrySignupRepository;
        public ILaundrySignupRepository LaundrySignupRepository
        {
            get { return laundrySignupRepository ?? (laundrySignupRepository = new LaundrySignupRepository(context)); }
        }
        private ISoberDriversRepository soberDriverRepository;
        public ISoberDriversRepository SoberDriverRepository
        {
            get { return soberDriverRepository ?? (soberDriverRepository = new SoberDriversRepository(context)); }
        }
        private ISoberOfficersRepository soberOfficerRepository;
        public ISoberOfficersRepository SoberOfficerRepository
        {
            get { return soberOfficerRepository ?? (soberOfficerRepository = new SoberOfficersRepository(context)); }
        }
        private IMemberStatusRepository memberStatusRepository;
        public IMemberStatusRepository MemberStatusRepository
        {
            get { return memberStatusRepository ?? (memberStatusRepository = new MemberStatusRepository(context)); }
        }
        private IStudyHoursRepository studyHourRepository;
        public IStudyHoursRepository StudyHourRepository
        {
            get { return studyHourRepository ?? (studyHourRepository = new StudyHoursRepository(context)); }
        }
        private IIncidentReportsRepository incidentReportRepository;
        public IIncidentReportsRepository IncidentReportRepository
        {
            get { return incidentReportRepository ?? (incidentReportRepository = new IncidentReportsRepository(context)); }
        }

        public void Save()
        {
            context.SaveChanges();
        }

        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}