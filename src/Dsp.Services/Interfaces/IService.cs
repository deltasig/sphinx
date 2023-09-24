using System;

namespace Dsp.Services.Interfaces;

public interface IService
{
    DateTime ConvertUtcToCst(DateTime utc);
    DateTime ConvertCstToUtc(DateTime cst);
}
