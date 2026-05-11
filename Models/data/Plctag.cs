using System;
using System.Collections.Generic;

namespace Water_Filtration.Models.data;

public partial class Plctag
{
    public int Id { get; set; }

    public int? DeviceFk { get; set; }

    public string TagName { get; set; } = null!;

    public string Offset { get; set; } = null!;

    public string DataType { get; set; } = null!;

    public ulong IsActive { get; set; }

    public int? IsImportant { get; set; }
}
