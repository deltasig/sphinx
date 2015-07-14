namespace Dsp.Areas.Edu.Models
{
    using global::Dsp.Models;

    public class ClassesIndexFilterModel : Pager
    {
        public string select { get; set; }
        public string sort { get; set; }
        public string s { get; set; }

        public ClassesIndexFilterModel()
        {
            page = 1;
            select = string.Empty;
            sort = "number";
            s = string.Empty;
        }
        public ClassesIndexFilterModel(ClassesIndexFilterModel original)
        {
            page = original.page;
            select = original.select;
            sort = original.sort;
            s = original.s;
        }
    }
}