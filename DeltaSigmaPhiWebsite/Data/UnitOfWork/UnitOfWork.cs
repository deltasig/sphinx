﻿namespace DeltaSigmaPhiWebsite.Data.UnitOfWork
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
        private ISoberDriversRepository _soberDriverRepository;
        public ISoberDriversRepository SoberDriverRepository
        {
            get { return _soberDriverRepository ?? (_soberDriverRepository = new SoberDriversRepository(_context)); }
        }
        private ISoberOfficersRepository _soberOfficerRepository;
        public ISoberOfficersRepository SoberOfficerRepository
        {
            get { return _soberOfficerRepository ?? (_soberOfficerRepository = new SoberOfficersRepository(_context)); }
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
        private IAddressesRepository _addressesRepository;
        public IAddressesRepository AddressesRepository
        {
            get { return _addressesRepository ?? (_addressesRepository = new AddressesRepository(_context)); }
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