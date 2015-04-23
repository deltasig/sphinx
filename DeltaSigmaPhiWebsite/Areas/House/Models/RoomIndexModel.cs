namespace DeltaSigmaPhiWebsite.Areas.House.Models
{
    using System.Collections.Generic;
    using Entities;
    using System.Web.Mvc;

    public class RoomIndexModel
    {
        public IEnumerable<Room> Rooms { get; set; }
        public IEnumerable<Member> Members { get; set; }
        public Semester Semester { get; set; }
        public int SelectedSemester { get; set; }
        public IEnumerable<SelectListItem> SemesterList { get; set; }
    }
}