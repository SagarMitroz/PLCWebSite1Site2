using System;
using System.Collections.Generic;

namespace Water_Filtration.Models.data;

public partial class Pressuremaster
{
    public int Pid { get; set; }

    public string? PressureName { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? IsDelete { get; set; }
}
