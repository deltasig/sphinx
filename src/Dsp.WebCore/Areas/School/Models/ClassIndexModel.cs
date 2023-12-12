namespace Dsp.WebCore.Areas.School.Models;

using Dsp.Data.Entities;
using System.Collections.Generic;

public class ClassIndexModel
{
    public IEnumerable<Class> Classes { get; set; }
    public Semester CurrentSemester { get; set; }
    public ClassesIndexFilterModel Filter { get; set; }
    public int TotalPages { get; set; }
    public int ResultCount { get; set; }

    public ClassIndexModel(
        IEnumerable<Class> classes,
        Semester currentSemester,
        ClassesIndexFilterModel filter,
        int totalPages,
        int resultCount)
    {
        Classes = classes;
        CurrentSemester = currentSemester;
        Filter = filter;
        TotalPages = totalPages;
        ResultCount = resultCount;
    }
}