﻿namespace Dsp.Web.Areas.Edu.Models
{
    using Dsp.Data.Entities;
    using System.Collections.Generic;

    public class EditEnrollmentModel
    {
        public ClassTaken Enrollment { get; set; }
        public IEnumerable<object> Grades { get; set; }
    }
}