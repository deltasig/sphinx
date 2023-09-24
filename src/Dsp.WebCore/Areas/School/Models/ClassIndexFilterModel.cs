namespace Dsp.WebCore.Areas.School.Models;

using Dsp.WebCore.Models;

public class ClassesIndexFilterModel : Pager
{
    public string select { get; set; }
    public string sort { get; set; }
    public string s { get; set; }

    public ClassesIndexFilterModel()
    {
        this.select = string.Empty;
        this.sort = "number";
        this.s = string.Empty;

        base.page = 1;
        base.pageSize = 10;
    }
    public ClassesIndexFilterModel(ClassesIndexFilterModel original)
    {
        this.select = original.select;
        this.sort = original.sort;
        this.s = original.s;

        base.page = original.page;
        base.pageSize = original.pageSize;
    }
}