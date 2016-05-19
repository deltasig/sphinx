namespace Dsp.Web.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Email
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EmailId { get; set; }

        public int EmailTypeId { get; set; }

        public DateTime SentOn { get; set; }

        public string Destination { get; set; }

        public string Body { get; set; }

        [ForeignKey("EmailTypeId")]
        public virtual EmailType EmailType { get; set; }
    }
}