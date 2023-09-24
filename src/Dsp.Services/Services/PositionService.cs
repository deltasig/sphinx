namespace Dsp.Services;

using Data;
using Data.Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class PositionService : BaseService, IPositionService
{
    private readonly DspDbContext _context;
    private readonly IMemberService _memberService;

    public PositionService(DspDbContext context)
    {
        _context = context;
        _memberService = new MemberService(context);
    }

    public async Task<IEnumerable<Role>> GetAllPositionsAsync()
    {
        return await _context.Roles.ToListAsync();
    }

    public async Task<Role> GetPositionByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("The entity's ID is not valid.");
        }
        return await _context.FindAsync<Role>(id);
    }

    public async Task<Role> GetPositionByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("The entity's name is not valid.");
        }
        return await _context.Roles
            .Where(m => m.Name == name)
            .SingleAsync();
    }

    public async Task<IEnumerable<Role>> GetEboardPositionsAsync()
    {
        var eBoardPositions = await _context.Roles
            .Where(m => m.IsExecutive)
            .ToListAsync();

        return eBoardPositions;
    }

    public async Task<User> GetUserInPositionAsync(string positionName, int sid)
    {
        var position = await _context.Roles
            .Where(x => x.Name == positionName)
            .SingleOrDefaultAsync();
        if (position == null) return null;

        var appointment = await _context.UserRoles
            .Where(x => x.RoleId == position.Id && x.SemesterId == sid)
            .SingleOrDefaultAsync();
        if (appointment == null) return null;

        return appointment.User;
    }

    public async Task<IEnumerable<Role>> GetCurrentPositionsByUserAsync(int userId)
    {
        var currentAppointmentsForUser = new List<Role>();
        var adminAppointmentForUser = await GetAdminAppoinmentForUserAsync(userId);
        if (adminAppointmentForUser != null)
        {
            currentAppointmentsForUser.Add(adminAppointmentForUser.Role);
        }

        var now = DateTime.UtcNow;
        var applicableSemesters = await _context.Semesters
            .Where(x => now < x.TransitionDate)
            .OrderBy(x => x.DateStart)
            .ToListAsync();
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
                        .Select(x => x.Role)
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
                        .Select(x => x.Role)
                        .Distinct()
                );
            }
        }

        return currentAppointmentsForUser;
    }

    public async Task<IEnumerable<Role>> GetCurrentPositionsByUserAsync(string userName)
    {
        var user = await _memberService.GetMemberByUserNameAsync(userName);
        var currentAppointmentsForUser = await GetCurrentPositionsByUserAsync(user.Id);
        return currentAppointmentsForUser;
    }

    public async Task<Role> GetAdminPositionAsync()
    {
        var adminPosition = await GetPositionByNameAsync("Administrator");
        return adminPosition;
    }

    public async Task<UserRole> GetAdminAppoinmentForUserAsync(int userId)
    {
        var adminPosition = await GetAdminPositionAsync();
        var adminAppointForUser = adminPosition.Users
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

    public async Task RemovePositionAsync(Role entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("Cannot update a null entity.");
        }
        if (entity.Id <= 0)
        {
            throw new ArgumentException("The entity's ID is not valid.");
        }
        _context.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task RemovePositionByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("The entity's ID is not valid.");
        }
        var entity = await _context.FindAsync<Role>(id);
        if (entity == null)
        {
            throw new ArgumentException("A position for the given ID was not found.");
        }
        await RemovePositionAsync(entity);
    }

    public async Task CreatePositionAsync(Role entity)
    {
        var exists = await _context.Roles.AnyAsync(m => m.Name == entity.Name);
        if (exists)
        {
            throw new ArgumentException("A position with that name already exists.");
        }

        _context.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePositionAsync(Role entity)
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
            throw new ArgumentException("The entity being updated does not exist in the database.");
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
            var allPositions = await _context.Roles
                .Where(p => p.Type == entity.Type && p.Id != entity.Id)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();

            var positionsList = allPositions.ToList();
            positionsList.Add(oldPosition);

            for (var i = 0; i < positionsList.Count; i++)
            {
                positionsList[i].DisplayOrder = i;
                _context.Update(positionsList[i]);
            }
        }
        else
        {
            _context.Update(oldPosition);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Role>> GetAppointmentsAsync(int sid)
    {
        var positions = await _context.Roles
            .Where(p => !p.IsDisabled && p.Name != "Administrator")
            .OrderBy(p => p.Type)
            .ThenBy(p => p.DisplayOrder)
            .ToListAsync();
        return positions;
    }

    public async Task AppointMemberToPositionAsync(int mid, int pid, int sid)
    {
        var entity = new UserRole
        {
            UserId = mid,
            RoleId = pid,
            SemesterId = sid,
            AppointedOn = DateTime.UtcNow
        };
        _context.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveMemberFromPositionAsync(int mid, int pid, int sid)
    {
        var appointment = await _context.UserRoles
            .Where(l => l.UserId == mid && l.RoleId == pid && l.SemesterId == sid)
            .SingleAsync();
        _context.Remove(appointment);
        await _context.SaveChangesAsync();
    }
}
