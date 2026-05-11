using System;
using System.Collections.Generic;

namespace Water_Filtration.Models.data;

public partial class Sitetempaccess
{
    public int SitetempId { get; set; }

    public int? FkSite { get; set; }

    public int? FkTemp { get; set; }

    public bool? HasAccess { get; set; }

    public DateTime? CreatedOn { get; set; }
}
