using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class LaundrySignup
{
    public DateTime DateTimeShift { get; set; }

    public int UserId { get; set; }

    public DateTime DateTimeSignedUp { get; set; }

    public virtual User User { get; set; }

    public int GetSlotSizeActualSize(int slotSize)
    {
        // No change occurs during this shift so need to adjust.
        if (!DstChangeOccursDuringSlot(slotSize)) return slotSize;

        // Shift is in the fall (gain an hour)
        if (DateTimeShift.IsDaylightSavingTime())
        {
            return slotSize + 1;
        }
        // Shift is in spring (lose an hour)
        return slotSize - 1;
    }

    public bool DstChangeOccursDuringSlot(int slotSize)
    {
        if (DateTimeShift.IsDaylightSavingTime() && !DateTimeShift.AddHours(slotSize).IsDaylightSavingTime()) // Fall - DST End
        {
            return true;
        }
        else if (!DateTimeShift.IsDaylightSavingTime() && DateTimeShift.AddHours(slotSize).IsDaylightSavingTime()) // Spring - DST Start
        {
            return true;
        }

        return false;
    }
}
