using System;
using System.Collections.Generic;

namespace FUNewsManagement_CoreAPI.Models;

public partial class AuditLog
{
    public int Id { get; set; }

    public short? AccountId { get; set; }

    public string EntityName { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string? BeforeData { get; set; }

    public string? AfterData { get; set; }

    public DateTime? Timestamp { get; set; }

    public virtual SystemAccount? Account { get; set; }
}
