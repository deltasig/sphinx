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
        private readonly IMemberService _memberService;
        private readonly ISemesterService _semesterService;

        public PositionService() : this(new Repository<SphinxDbContext>(new SphinxDbContext()))
        {

        }

        public PositionService(SphinxDbContext db) : this(new Repository<SphinxDbContext>(db))
        {

        }

        public PositionService(IRepository repository)
        {
            _repository = repository;
            _memberService = new MemberService(repository);
            _semesterService = new SemesterService(repository);
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

        public async Task<IEnumerable<Position>> GetEboardPositionsAsync()
        {
            var eBoardPositions = await _repository.GetAsync<Position>(m => m.IsExecutive);

            return eBoardPositions;
        }

        public async Task<Member> GetUserInPositionAsync(string positionName, int sid)
        {
            var position = await _repository.GetOneAsync<Position>(x => x.Name == positionName);
            if (position == null) return null;

            var appointment = await _repository.GetOneAsync<Leader>(x => x.RoleId == position.Id && x.SemesterId == sid);
            if (appointment == null) return null;

            return appointment.Member;
        }

        public async Task<IEnumerable<Position>> GetCurrentPositionsByUserAsync(int userId)
        {
            var currentAppointmentsForUser = new List<Position>();
            var adminAppointmentForUser = await GetAdminAppoinmentForUserAsync(userId);
            if (adminAppointmentForUser != null)
            {
                currentAppointmentsForUser.Add(adminAppointmentForUser.Position);
            }

            var now = DateTime.UtcNow;
            var applicableSemesters = await _repository.GetAsync<Semester>(
                filter: x => now < x.TransitionDate,
                orderBy: x => x.OrderBy(o => o.DateStart)
            );
            if (applicableSemesters.Any())
            {
                var numberOfApplicableSemesters = applicableSemesters.Count();
                var firstApplicableSemester = applicableSemesters.First();
                if (now < firstApplicableSemester.DateEnd || numberOfApplicableSemesters == 1)
                {
                    /* Two scenarios are represented here, both of which are handled in the same way:
                     *   1) the first condition represents that the first applicable semester happens 
                     *      to be the current semester but we are NOT in the transition period.  
                     *      Therefore, only the current semester's appointments are in effect.
                     *   2) the second condition represents that this is the final semester in the 
                     *      database so even if we are in a transition period, there can't be any 
                     *      future appointments anyway, so only the previous semester's appointments 
                     *      are in effect (until the transition date passes).
                     */
                    currentAppointmentsForUser.AddRange(
                        firstApplicableSemester.Leaders
                            .Where(x => x.UserId == userId)
                            .Select(x => x.Position)
                    );
                }
                else
                {
                    /* Otherwise, we are in a transition period, and the second semester 
                     * happens to be the current semester.  Therefore, both the previous semester and 
                     * current semester appointments are in effect.
                     */
                    currentAppointmentsForUser.AddRange(
                        applicableSemesters
                            .Take(2)
                            .SelectMany(x => x.Leaders)
                            .Where(x => x.UserId == userId)
                            .Select(x => x.Position)
                            .Distinct()
                    );
                }
            }

            return currentAppointmentsForUser;
        }

        public async Task<IEnumerable<Position>> GetCurrentPositionsByUserAsync(string userName)
        {
            var user = await _memberService.GetMemberByUserNameAsync(userName);
            var currentAppointmentsForUser = await GetCurrentPositionsByUserAsync(user.Id);
            return currentAppointmentsForUser;
        }

        public async Task<Position> GetAdminPositionAsync()
        {
            var adminPosition = await GetPositionByNameAsync("Administrator");
            return adminPosition;
        }

        public async Task<Leader> GetAdminAppoinmentForUserAsync(int userId)
        {
            var adminPosition = await GetAdminPositionAsync();
            var adminAppointForUser = adminPosition.Leaders
                .FirstOrDefault(x => x.UserId == userId);

            return adminAppointForUser;
        }

        public async Task<bool> UserIsAdminAsync(int userId)
        {
            var adminAppointForUser = await GetAdminAppoinmentForUserAsync(userId);
            var userIsAdmin = adminAppointForUser == null;
            return userIsAdmin;
        }

        public async Task<bool> UserIsAdminAsync(string userName)
        {
            var user = await _memberService.GetMemberByUserNameAsync(userName);
            var userIsAdmin = await UserIsAdminAsync(user.Id);
            return userIsAdmin;
        }

        public async Task<bool> UserHasPositionPowerAsync(int userId, int positionId)
        {
            var currentUserPositions = await GetCurrentPositionsByUserAsync(userId);
            var adminPosition = await GetAdminPositionAsync();
            var userHasPositionPower = currentUserPositions
                .Any(x => x.Id == positionId ||
                          x.Id == adminPosition.Id); // Admins have all power

            return userHasPositionPower;
        }

        public async Task<bool> UserHasPositionPowerAsync(int userId, string positionName)
        {
            var position = await GetPositionByNameAsync(positionName);
            var userIsInPosition = await UserHasPositionPowerAsync(userId, position.Id);
            return userIsInPosition;
        }

        public async Task<bool> UserHasPositionPowerAsync(string userName, string positionName)
        {
            var user = await _memberService.GetMemberByUserNameAsync(userName);
            var userIsInPosition = await UserHasPositionPowerAsync(user.Id, positionName);
            return userIsInPosition;
        }

        public async Task<bool> UserHasAtLeastOnePositionPowerAsync(int userId, string[] positionNames)
        {
            var currentUserPositions = await GetCurrentPositionsByUserAsync(userId);
            var adminPosition = await GetAdminPositionAsync();
            bool userHasPositionPower = false;
            foreach (var p in positionNames)
            {
                userHasPositionPower = currentUserPositions
                    .Any(x => x.Name == p ||
                              x.Name == adminPosition.Name); // Admins have all power

                if (userHasPositionPower) break;
            }

            return userHasPositionPower;
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
