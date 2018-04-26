namespace Dsp.Services
{
    using Data;
    using Data.Entities;
    using Dsp.Repositories;
    using Dsp.Repositories.Interfaces;
    using Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Linq;
    using System.Threading.Tasks;

    public class PositionService : BaseService, IPositionService
    {
        private readonly IRepository _repository;

        public PositionService() : this(new Repository<SphinxDbContext>(new SphinxDbContext()))
        {

        }

        public PositionService(SphinxDbContext db) : this(new Repository<SphinxDbContext>(db))
        {

        }

        public PositionService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Position>> GetAllPositionsAsync()
        {
            return await _repository.GetAllAsync<Position>();
        }

        public async Task<Position> GetPositionByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("The entity's ID is not valid.");
            }
            return await _repository.GetOneAsync<Position>(p => p.Id == id);
        }

        public async Task<Position> GetPositionByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The entity's name is not valid.");
            }
            return await _repository.GetOneAsync<Position>(m => m.Name == name);
        }

        public async Task RemovePositionAsync(Position entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Cannot update a null entity.");
            }
            if (entity.Id <= 0)
            {
                throw new ArgumentException("The entity's ID is not valid.");
            }
            _repository.Delete(entity);
            await _repository.SaveAsync();
        }

        public async Task RemovePositionByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("The entity's ID is not valid.");
            }
            var entity = await _repository.GetByIdAsync<Position>(id);
            if (entity == null)
            {
                throw new ObjectNotFoundException("A position for the given ID was not found.");
            }
            await RemovePositionAsync(entity);
        }

        public async Task CreatePositionAsync(Position entity)
        {
            var exists = await _repository.GetExistsAsync<Position>(m => m.Name == entity.Name);
            if (exists)
            {
                throw new ArgumentException("A position with that name already exists.");
            }

            _repository.Create(entity);
            await _repository.SaveAsync();
        }

        public async Task UpdatePositionAsync(Position entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Cannot update a null entity.");
            }
            if (entity.Id <= 0)
            {
                throw new ArgumentException("The entity's ID is not valid.");
            }

            var oldPosition = await GetPositionByIdAsync(entity.Id);
            if (oldPosition == null)
            {
                throw new ObjectNotFoundException("The entity being updated does not exist in the database.");
            }
            oldPosition.Name = entity.Name;
            oldPosition.Description = entity.Description;
            oldPosition.Inquiries = entity.Inquiries;
            oldPosition.Type = entity.Type;
            oldPosition.IsExecutive = entity.IsExecutive;
            oldPosition.IsElected = entity.IsElected;
            oldPosition.IsDisabled = entity.IsDisabled;
            oldPosition.IsPublic = entity.IsPublic;

            // Check if order changed.
            if (oldPosition.DisplayOrder != entity.DisplayOrder)
            {
                oldPosition.DisplayOrder = entity.DisplayOrder;
                // Auto adjusting the display order of all positions to accomodate change.
                var allPositions = await _repository
                    .GetAsync<Position>(
                        p => p.Type == entity.Type && p.Id != entity.Id,
                        o => o.OrderBy(p => p.DisplayOrder));

                var positionsList = allPositions.ToList();
                positionsList.Add(oldPosition);

                for (var i = 0; i < positionsList.Count; i++)
                {
                    positionsList[i].DisplayOrder = i;
                    _repository.Update(positionsList[i]);
                }
            }
            else
            {
                _repository.Update(oldPosition);
            }

            await _repository.SaveAsync();
        }

        public async Task<IEnumerable<Position>> GetAppointmentsAsync(int sid)
        {
            var positions = await _repository
                .GetAsync<Position>(
                    p => !p.IsDisabled && p.Name != "Administrator",
                    x => x.OrderBy(p => p.Type).ThenBy(p => p.DisplayOrder));
            return positions;
        }

        public async Task AppointMemberToPositionAsync(int mid, int pid, int sid)
        {
            var entity = new Leader
            {
                UserId = mid,
                RoleId = pid,
                SemesterId = sid,
                AppointedOn = DateTime.UtcNow
            };
            _repository.Create(entity);
            await _repository.SaveAsync();
        }

        public async Task RemoveMemberFromPositionAsync(int mid, int pid, int sid)
        {
            var appointment = await _repository
                .GetOneAsync<Leader>(l => l.UserId == mid && l.RoleId == pid && l.SemesterId == sid);
            _repository.Delete(appointment);
            await _repository.SaveAsync();
        }
    }
}
