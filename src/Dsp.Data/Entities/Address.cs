using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class Address
{
    public int AddressId { get; set; }

    public int UserId { get; set; }

    public string Type { get; set; }

    [ProtectedPersonalData]
    public string Address1 { get; set; }

    [ProtectedPersonalData]
    public string Address2 { get; set; }

    [ProtectedPersonalData]
    public string City { get; set; }

    [ProtectedPersonalData]
    public string State { get; set; }

    [ProtectedPersonalData]
    public int PostalCode { get; set; }

    [ProtectedPersonalData]
    public string Country { get; set; }

    public virtual User User { get; set; }

    public bool IsFilledOut()
    {
        return !string.IsNullOrEmpty(Address1) && !string.IsNullOrEmpty(City) && !string.IsNullOrEmpty(State);
    }

    public override string ToString()
    {
        var address = string.Empty;
        if (!string.IsNullOrEmpty(Address1))
            address += Address1;
        if (!string.IsNullOrEmpty(Address2))
            address += ", " + Address2;
        if (!string.IsNullOrEmpty(City))
            address += ", " + City;
        if (!string.IsNullOrEmpty(State))
            address += ", " + State;
        if (PostalCode > 0)
            address += ", " + PostalCode;
        if (!string.IsNullOrEmpty(Country))
            address += ", " + Country;
        return address;
    }
}
