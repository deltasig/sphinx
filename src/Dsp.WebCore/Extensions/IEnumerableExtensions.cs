namespace Dsp.WebCore.Extensions;

using Dsp.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

public static class IEnumerableExtensions
{
    public static SelectList ToSelectList(this IEnumerable<Semester> semesters)
    {
        if (semesters == null)
            throw new ArgumentException(null, nameof(semesters));

        var semesterSelectList = new SelectList(semesters, "Id", "Name");
        return semesterSelectList;
    }

    public static SelectList ToSelectList(this IEnumerable<PledgeClass> pledgeClasses)
    {
        if (pledgeClasses == null)
            throw new ArgumentException(null, nameof(pledgeClasses));

        var semesterSelectList = new SelectList(pledgeClasses, "PledgeClassId", "PledgeClassName");
        return semesterSelectList;
    }

    public static SelectList ToSelectListWithNone(this IEnumerable<PledgeClass> pledgeClasses)
    {
        if (pledgeClasses == null)
            throw new ArgumentException(null, nameof(pledgeClasses));

        var newList = pledgeClasses.ToList();
        newList.Insert(0, new PledgeClass { PledgeClassId = 0, PledgeClassName = "None" });
        var semesterSelectList = newList.ToSelectList();
        return semesterSelectList;
    }

    public static SelectList ToSelectList(this IEnumerable<Member> members)
    {
        var newList = new List<object>();
        foreach (var u in members)
        {
            newList.Add(new
            {
                UserId = u.Id,
                Name = $"{u.FirstName} {u.LastName}"
            });
        }
        return new SelectList(newList, "UserId", "Name");
    }

    public static SelectList ToSelectListWithNone(this IEnumerable<Member> members)
    {
        var newList = new List<object> { new { UserId = 0, Name = "None" } };
        foreach (var member in members)
        {
            newList.Add(new
            {
                UserId = member.Id,
                Name = member.FirstName + " " + member.LastName
            });
        }
        return new SelectList(newList, "UserId", "Name");
    }

    public static SelectList ToSelectList(this IEnumerable<ScholarshipType> scholarshipTypes)
    {
        var newList = new List<object>();
        foreach (var t in scholarshipTypes)
        {
            newList.Add(new
            {
                t.ScholarshipTypeId,
                t.Name
            });
        }
        return new SelectList(newList, "ScholarshipTypeId", "Name");
    }
}
