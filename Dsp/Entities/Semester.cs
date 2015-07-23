namespace Dsp.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Semester
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SemesterId { get; set; }

        [Required, DataType(DataType.Date), Display(Name = "Start Date")]
        public DateTime DateStart { get; set; }

        [Required, DataType(DataType.Date), Display(Name = "End Date")]
        public DateTime DateEnd { get; set; }
        
        [Required, DataType(DataType.Date), Display(Name = "Transition Date")]
        public DateTime TransitionDate { get; set; }

        [Required, DefaultValue(10), Display(Name = "Minimum Service Hours")]
        public int MinimumServiceHours { get; set; }

        [InverseProperty("Semester")]
        public virtual ICollection<ClassTaken> ClassesTaken { get; set; }
        [InverseProperty("Semester")]
        public virtual ICollection<Leader> Leaders { get; set; }
        [InverseProperty("GraduationSemester")]
        public virtual ICollection<Member> GraduatingMembers { get; set; }
        [InverseProperty("Semester")]
        public virtual ICollection<PledgeClass> PledgeClasses { get; set; }
        [InverseProperty("Semester")]
        public virtual ICollection<Room> Rooms { get; set; }

        public override string ToString()
        {
            return (DateStart.Month < 6 ? "Spring " : "Fall ") + DateStart.Year;
        }
    }
}
