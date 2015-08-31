namespace Dsp.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents a time span during which studying occurs, usually a week long.
    /// </summary>
    public class StudyPeriod
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Begins On")]
        public DateTime BeginsOn { get; set; }

        [Display(Name = "Ends On")]
        public DateTime EndsOn { get; set; }

        [Display(Name = "Fine Amount"), Range(0, 20, ErrorMessage = "Fine amounts can be from $0 up to $20 (decimal is allowed).")]
        public double FineAmount { get; set; }

        [InverseProperty("Period")]
        public virtual ICollection<StudyAssignment> Assignments { get; set; }
    }

    /// <summary>
    /// Represents a time and place where study hours can be performed.
    /// </summary>
    public class StudySession
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Location { get; set; }

        [Display(Name = "Begins On")]
        public DateTime BeginsOn { get; set; }

        [Display(Name = "Ends On")]
        public DateTime EndsOn { get; set; }

        [InverseProperty("Session")]
        public virtual ICollection<StudyProctor> Proctors { get; set; }
        [InverseProperty("Session")]
        public virtual ICollection<StudyHour> StudyHours { get; set; }
    }

    /// <summary>
    /// Defines members who oversee study sessions.
    /// </summary>
    public class StudyProctor
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SessionId { get; set; }

        public int MemberId { get; set; }

        [ForeignKey("SessionId")]
        public virtual StudySession Session { get; set; }
        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }
    }

    /// <summary>
    /// Represents the expectation for a member to complete X number of study hours within a period of time.
    /// </summary>
    public class StudyAssignment
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int PeriodId { get; set; }

        public int MemberId { get; set; }
        
        public double AmountOfHours { get; set; }

        [ForeignKey("PeriodId")]
        public virtual StudyPeriod Period { get; set; }
        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }
        [InverseProperty("Assignment")]
        public virtual ICollection<StudyHour> StudyHours { get; set; }
    }

    /// <summary>
    /// Represents a amount of time spent studying a study session to complete an assignment.
    /// </summary>
    public class StudyHour
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? AssignmentId { get; set; }

        public int? SessionId { get; set; }

        public int MemberId { get; set; }

        [Display(Name = "Sign In")]
        public DateTime SignedInOn { get; set; }

        [Display(Name = "Sign Out")]
        public DateTime SignedOutOn { get; set; }

        public double DurationMinutes { get; set; }

        [ForeignKey("AssignmentId")]
        public virtual StudyAssignment Assignment { get; set; }
        [ForeignKey("SessionId")]
        public virtual StudySession Session { get; set; }
        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }
    }
}
