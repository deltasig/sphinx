namespace DeltaSigmaPhiWebsite.Data.UnitOfWork
{
    using Interfaces;
    using Models;
    using Repositories;
    using System;

    public class UnitOfWork : IUnitOfWork
    {
        private readonly DspContext _context = new DspContext();
        
        private IMembersRepository _memberRepository;
        public IMembersRepository MemberRepository
        {
            get { return _memberRepository ?? (_memberRepository = new MembersRepository(_context)); }
        }
        private IServiceHoursRepository _serviceHourRepository;
        public IServiceHoursRepository ServiceHourRepository
        {
            get { return _serviceHourRepository ?? (_serviceHourRepository = new ServiceHoursRepository(_context)); }
        }
        private ISemestersRepository _semesterRepository;
        public ISemestersRepository SemesterRepository
        {
            get { return _semesterRepository ?? (_semesterRepository = new SemestersRepository(_context)); }
        }
        private IEventsRepository _eventRepository;
        public IEventsRepository EventRepository
        {
            get { return _eventRepository ?? (_eventRepository = new EventsRepository(_context)); }
        }
        private ILaundrySignupRepository _laundrySignupRepository;
        public ILaundrySignupRepository LaundrySignupRepository
        {
            get { return _laundrySignupRepository ?? (_laundrySignupRepository = new LaundrySignupRepository(_context)); }
        }
        private ISoberSignupsRepository _soberSignupsRepository;
        public ISoberSignupsRepository SoberSignupsRepository
        {
            get { return _soberSignupsRepository ?? (_soberSignupsRepository = new SoberSignupsRepository(_context)); }
        }
        private IMemberStatusRepository _memberStatusRepository;
        public IMemberStatusRepository MemberStatusRepository
        {
            get { return _memberStatusRepository ?? (_memberStatusRepository = new MemberStatusRepository(_context)); }
        }
        private IStudyHoursRepository _studyHourRepository;
        public IStudyHoursRepository StudyHourRepository
        {
            get { return _studyHourRepository ?? (_studyHourRepository = new StudyHoursRepository(_context)); }
        }
        private IIncidentReportsRepository _incidentReportRepository;
        public IIncidentReportsRepository IncidentReportRepository
        {
            get { return _incidentReportRepository ?? (_incidentReportRepository = new IncidentReportsRepository(_context)); }
        }
        private IAddressesRepository _addressRepository;
        public IAddressesRepository AddressRepository
        {
            get { return _addressRepository ?? (_addressRepository = new AddressesRepository(_context)); }
        }
        private IPhoneNumbersRepository _phoneNumberRepository;
        public IPhoneNumbersRepository PhoneNumberRepository
        {
            get { return _phoneNumberRepository ?? (_phoneNumberRepository = new PhoneNumbersRepository(_context)); }
        }
        private ILeadersRepository _leaderRepository;
        public ILeadersRepository LeaderRepository
        {
            get { return _leaderRepository ?? (_leaderRepository = new LeadersRepository(_context)); }
        }
        private IPositionsRepository _positionRepository;
        public IPositionsRepository PositionRepository
        {
            get { return _positionRepository ?? (_positionRepository = new PositionsRepository(_context)); }
        }
        private IPledgeClassesRepository _pledgeClassRepository;
        public IPledgeClassesRepository PledgeClassRepository
        {
            get { return _pledgeClassRepository ?? (_pledgeClassRepository = new PledgeClassesRepository(_context)); }
        }
        private IClassesRepository _classesRepository;
        public IClassesRepository ClassesRepository
        {
            get { return _classesRepository ?? (_classesRepository = new ClassesRepository(_context)); }
        }
        private IClassesTakenRepository _classesTakenRepository;
        public IClassesTakenRepository ClassesTakenRepository
        {
            get { return _classesTakenRepository ?? (_classesTakenRepository = new ClassesTakenRepository(_context)); }
        }
        private IDepartmentsRepository _departmentsRepository;
        public IDepartmentsRepository DepartmentsRepository
        {
            get { return _departmentsRepository ?? (_departmentsRepository = new DepartmentsRepository(_context)); }
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}