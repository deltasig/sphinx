namespace Dsp.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class LaundrySignup
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public DateTime DateTimeShift { get; set; }

        public int UserId { get; set; }

        public DateTime DateTimeSignedUp { get; set; }

        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }

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
}
