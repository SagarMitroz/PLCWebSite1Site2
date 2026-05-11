using System;
using System.Collections.Generic;

namespace Water_Filtration.Models.data;

public partial class Tankmaster
{
    public int Id { get; set; }

    public string? TankName { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? IsDelete { get; set; }
}
