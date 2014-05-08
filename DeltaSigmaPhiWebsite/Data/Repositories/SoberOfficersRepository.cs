﻿namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;

    public class SoberOfficersRepository : GenericRepository<SoberOfficer>, ISoberOfficersRepository
    {
        public SoberOfficersRepository(DspContext context) : base(context)
        {
        }
    }
}