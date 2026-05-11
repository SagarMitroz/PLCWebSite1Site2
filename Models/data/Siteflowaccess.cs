using System;
using System.Collections.Generic;

namespace Water_Filtration.Models.data;

public partial class Siteflowaccess
{
    public int SiteFlowAccessId { get; set; }

    public int? FkSite { get; set; }

    public int? FkFlow { get; set; }

    public bool? HasAccess { get; set; }

    public DateTime? CreatedOn { get; set; }
}
