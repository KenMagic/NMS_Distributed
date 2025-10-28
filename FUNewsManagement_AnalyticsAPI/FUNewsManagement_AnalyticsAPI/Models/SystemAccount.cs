using System;
using System.Collections.Generic;

namespace FUNewsManagement_AnalyticsAPI.Models;

public partial class SystemAccount
{
    public short AccountId { get; set; }

    public string? AccountName { get; set; }

    public string? AccountEmail { get; set; }

    public int? AccountRole { get; set; }

    public string? AccountPassword { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();

    public virtual ICollection<NewsView> NewsViews { get; set; } = new List<NewsView>();
}
