namespace Dsp.Data.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ChoreAssignment
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ChoreId { get; set; }
        public int PeriodId { get; set; }
        public int GroupId { get; set; }

        public DateTime OpensOnCst { get; set; }
        public DateTime DueOnCst { get; set; }

        public bool GroupCompleted { get; set; }
        public DateTime GroupCompletionTimeCst { get; set; }
        public int? GroupSignerId { get; set; }

        public bool? EnforcerVerified { get; set; }
        public DateTime EnforcerVerificationTimeCst { get; set; }
        public int? EnforcerId { get; set; }
        public string EnforcerFeedback { get; set; }

        public bool IsCancelled { get; set; }
        public int? EnforcementChoreId { get; set; }

        public DateTime CreatedOnCst { get; set; }
        public DateTime CancelledOnCst { get; set; }

        [ForeignKey("ChoreId")]
        public virtual Chore Chore { get; set; }
        [ForeignKey("PeriodId")]
        public virtual ChorePeriod Period { get; set; }
        [ForeignKey("GroupId")]
        public virtual ChoreGroup Group { get; set; }
        [ForeignKey("GroupSignerId")]
        public virtual Member Member { get; set; }
        [ForeignKey("EnforcerId")]
        public virtual Member Enforcer { get; set; }
        [ForeignKey("EnforcementChoreId")]
        public virtual ChoreAssignment EnforcementChore { get; set; }
    }
}
