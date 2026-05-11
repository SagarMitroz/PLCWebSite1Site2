using System;
using System.Collections.Generic;

namespace Water_Filtration.Models.data;

public partial class Sitetankaccess
{
    public int SitetankaccessId { get; set; }

    public int? FkSite { get; set; }

    public int? FkTank { get; set; }

    public sbyte? HasAccess { get; set; }

    public int? IsCwtank { get; set; }

    public DateTime? CreatedOn { get; set; }
}
