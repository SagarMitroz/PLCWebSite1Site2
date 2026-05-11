using System;
using System.Collections.Generic;

namespace Water_Filtration.Models.data;

public partial class Flowmaster
{
    public int Fid { get; set; }

    public string? FlowName { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? IsDelete { get; set; }
}
