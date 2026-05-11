using System;
using System.Collections.Generic;

namespace Water_Filtration.Models.data;

public partial class Temperaturemaster
{
    public int Tid { get; set; }

    public string? Temperature { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? IsDelete { get; set; }
}
