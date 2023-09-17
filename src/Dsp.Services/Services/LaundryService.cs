namespace Dsp.Services
{
    using Data;
    using Dsp.Data.Entities;
    using Dsp.Services.Exceptions;
    using Interfaces;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class LaundryService : BaseService, ILaundryService
    {
        private readonly DspDbContext _context;

        public LaundryService(DspDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LaundrySignup>> GetSignups(DateTime beginningOn)
        {
            var existingSignups = await _context.LaundrySignups
                .Where(l => l.DateTimeShift >= beginningOn.Date)
                .ToListAsync();
            return existingSignups;
        }

        public async Task Reserve(LaundrySignup entity, int maxSignupsAllowed = 2)
        {
            var nowCst = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Central Standard Time");

            // Check if they've already signed up too many times within the current window.
            var existingSignups = await _context.LaundrySignups
                .Where(l => l.DateTimeShift >= nowCst.Date && l.UserId == entity.UserId)
                .ToListAsync();
            if (existingSignups.Count >= maxSignupsAllowed)
            {
                var message = "You have signed up too many times within the current window.  " +
                              "Your maximum allowed is: " + maxSignupsAllowed;
                throw new LaundrySignupsExceededException(message);
            }

            // Check if a signup already exists
            var shift = await _context.FindAsync<LaundrySignup>(entity.DateTimeShift);
            if (shift != null)
            {
                var message = "Sorry, " + shift.User + " signed up for that slot after you loaded page " +
                              "but before you tried to sign up.  You will have to pick a new slot.";
                throw new LaundrySignupAlreadyExistsException(message);
            }

            // All good, add their reservation.
            entity.DateTimeSignedUp = nowCst;

            _context.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Cancel(LaundrySignup entity)
        {
            var shift = await _context.FindAsync<LaundrySignup>(entity.DateTimeShift);
            if (shift == null)
            {
                throw new LaundrySignupNotFoundException("Could not cancel reservation because no existing reservation was found.");
            }
            if (shift.UserId != entity.UserId)
            {
                throw new LaundrySignupPermissionException("You cannot cancel someone else's shift!");
            }
            _context.Remove(shift);
            await _context.SaveChangesAsync();
        }
    }
}
