﻿namespace Dsp.Areas.Edu.Models
{
    using System.Collections.Generic;
    using Entities;

    public class DuplicateGroup
    {
        public string Shorthand { get; set; }
        public List<DuplicateClass> Classes { get; set; }
    }

    public class DuplicateClass
    {
        public Class Class { get; set; }
        public bool IsPrimary { get; set; }
    }
}