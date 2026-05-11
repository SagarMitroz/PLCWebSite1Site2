using System;
using System.Collections.Generic;

namespace Water_Filtration.Models.data;

public partial class User
{
    public int Id { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public int? CompanyFk { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public int? UpdatedBy { get; set; }

    public int? IsDelete { get; set; }

    public string? Name { get; set; }

    public string? Contact { get; set; }

    public string? Email { get; set; }

    public int? RoleFk { get; set; }
}
