namespace Dsp.Services;

using Data;
using Data.Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class RoleService : BaseService, IRoleService
{
    private readonly DspDbContext _context;
    private readonly IUserService _userService;

    public RoleService(DspDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task<IEnumerable<Position>> GetAllRolesAsync()
    {
        return await _context.Positions.ToListAsync();
    }

    public async Task<Position> GetRoleByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("The entity's ID is not valid.");
        }
        return await _context.FindAsync<Position>(id);
    }

    public async Task<Position> GetRoleByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("The entity's name is not valid.");
        }
        return await _context.Positions
            .Where(m => m.Name == name)
            .SingleAsync();
    }

    public async Task<IEnumerable<Position>> GetEboardRolesAsync()
    {
        var eBoardPositions = await _context.Positions
            .Where(m => m.IsExecutive)
            .ToListAsync();

        return eBoardPositions;
    }

    public async Task<Member> GetUserInRoleAsync(string positionName, int sid)
    {
        var position = await _context.Roles
            .Where(x => x.Name == positionName)
            .SingleOrDefaultAsync();
        if (position == null) return null;

        var appointment = await _context.MembersPositions
            .Where(x => x.RoleId == position.Id && x.SemesterId == sid)
            .SingleOrDefaultAsync();
        if (appointment == null) return null;

        return appointment.User;
    }

    public async Task<IEnumerable<Position>> GetCurrentRolesByUserIdAsync(int userId)
    {
        var currentAppointmentsForUser = new List<Position>();
        var adminAppointmentForUser = await GetAdminUserRoleForUserAsync(userId);
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

    public async Task<IEnumerable<Position>> GetCurrentRolesByUserNameAsync(string userName)
    {
        var user = await _userService.GetUserByUserNameAsync(userName);
        var currentAppointmentsForUser = await GetCurrentRolesByUserIdAsync(user.Id);
        return currentAppointmentsForUser;
    }

    public async Task<Position> GetAdminRoleAsync()
    {
        var adminPosition = await GetRoleByNameAsync("Administrator");
        return adminPosition;
    }

    public async Task<MemberPosition> GetAdminUserRoleForUserAsync(int userId)
    {
        var adminPosition = await GetAdminRoleAsync();
        var adminAppointForUser = adminPosition.Users
            .FirstOrDefault(x => x.UserId == userId);

        return adminAppointForUser;
    }

    public async Task<bool> UserIsAdminAsync(int userId)
    {
        var adminAppointForUser = await GetAdminUserRoleForUserAsync(userId);
        var userIsAdmin = adminAppointForUser == null;
        return userIsAdmin;
    }

    public async Task<bool> UserIsAdminAsync(string userName)
    {
        var user = await _userService.GetUserByUserNameAsync(userName);
        var userIsAdmin = await UserIsAdminAsync(user.Id);
        return userIsAdmin;
    }

    public async Task<bool> UserIsCurrentlyInRoleAsync(int userId, int positionId)
    {
        var currentUserPositions = await GetCurrentRolesByUserIdAsync(userId);
        var adminPosition = await GetAdminRoleAsync();
        var userHasPositionPower = currentUserPositions
            .Any(x => x.Id == positionId ||
                      x.Id == adminPosition.Id); // Admins have all power

        return userHasPositionPower;
    }

    public async Task<bool> UserIsCurrentlyInRoleAsync(int userId, string positionName)
    {
        var position = await GetRoleByNameAsync(positionName);
        var userIsInPosition = await UserIsCurrentlyInRoleAsync(userId, position.Id);
        return userIsInPosition;
    }

    public async Task<bool> UserIsCurrentlyInRoleAsync(string userName, string positionName)
    {
        var user = await _userService.GetUserByUserNameAsync(userName);
        var userIsInPosition = await UserIsCurrentlyInRoleAsync(user.Id, positionName);
        return userIsInPosition;
    }

    public async Task<bool> UserIsCurrentlyInAtLeastOneRoleAsync(int userId, string[] positionNames)
    {
        var currentUserPositions = await GetCurrentRolesByUserIdAsync(userId);
        var adminPosition = await GetAdminRoleAsync();
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

    public async Task RemoveRoleAsync(Position role)
    {
        if (role == null)
        {
            throw new ArgumentNullException("Cannot update a null entity.");
        }
        if (role.Id <= 0)
        {
            throw new ArgumentException("The entity's ID is not valid.");
        }
        _context.Remove(role);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveRoleByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("The entity's ID is not valid.");
        }
        var entity = await _context.FindAsync<Position>(id);
        if (entity == null)
        {
            throw new ArgumentException("A position for the given ID was not found.");
        }
        await RemoveRoleAsync(entity);
    }

    public async Task CreateRoleAsync(Position role)
    {
        var exists = await _context.Roles.AnyAsync(m => m.Name == role.Name);
        if (exists)
        {
            throw new ArgumentException("A position with that name already exists.");
        }

        _context.Add(role);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRoleAsync(Position role)
    {
        if (role == null)
        {
            throw new ArgumentNullException("Cannot update a null entity.");
        }
        if (role.Id <= 0)
        {
            throw new ArgumentException("The entity's ID is not valid.");
        }

        var oldPosition = await GetRoleByIdAsync(role.Id);
        if (oldPosition == null)
        {
            throw new ArgumentException("The entity being updated does not exist in the database.");
        }
        oldPosition.Name = role.Name;
        oldPosition.Description = role.Description;
        oldPosition.Inquiries = role.Inquiries;
        oldPosition.Type = role.Type;
        oldPosition.IsExecutive = role.IsExecutive;
        oldPosition.IsElected = role.IsElected;
        oldPosition.IsDisabled = role.IsDisabled;
        oldPosition.IsPublic = role.IsPublic;

        // Check if order changed.
        if (oldPosition.DisplayOrder != role.DisplayOrder)
        {
            oldPosition.DisplayOrder = role.DisplayOrder;
            // Auto adjusting the display order of all positions to accomodate change.
            var allPositions = await _context.Positions
                .Where(p => p.Type == role.Type && p.Id != role.Id)
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

    public async Task<IEnumerable<Position>> GetAppointmentsAsync(int semesterId)
    {
        var positions = await _context.Positions
            .Where(p => !p.IsDisabled && p.Name != "Administrator")
            .OrderBy(p => p.Type)
            .ThenBy(p => p.DisplayOrder)
            .ToListAsync();
        return positions;
    }

    public async Task AssignUserToSemesterRoleAsync(int userId, int roleId, int semsterId)
    {
        var entity = new MemberPosition
        {
            UserId = userId,
            RoleId = roleId,
            SemesterId = semsterId,
            AppointedOn = DateTime.UtcNow
        };
        _context.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveUserFromSemesterRoleAsync(int userId, int roleId, int semesterId)
    {
        var appointment = await _context.MembersPositions
            .Where(l => l.UserId == userId && l.RoleId == roleId && l.SemesterId == semesterId)
            .SingleAsync();
        _context.Remove(appointment);
        await _context.SaveChangesAsync();
    }
}
