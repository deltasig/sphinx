﻿namespace Dsp.WebCore.Areas.Service.Models;

using Dsp.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

public class ServiceNavModel
{
    public bool HasElevatedPermissions { get; }
    public Semester SelectedSemester { get; }
    public SelectList SemesterList { get; }
    public string SemesterListLabel { get; }

    public ServiceNavModel(bool hasElevatedPermissions, Semester selectedSemester, SelectList semesterList)
    {
        HasElevatedPermissions = hasElevatedPermissions;
        SelectedSemester = selectedSemester;
        SemesterList = semesterList;
        SemesterListLabel = $"Semester: {selectedSemester}";
    }
}
