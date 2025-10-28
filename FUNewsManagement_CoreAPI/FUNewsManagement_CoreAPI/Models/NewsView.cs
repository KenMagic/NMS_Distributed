using System;
using System.Collections.Generic;

namespace FUNewsManagement_CoreAPI.Models;

public partial class NewsView
{
    public int ViewId { get; set; }

    public string NewsArticleId { get; set; } = null!;

    public short? ViewedById { get; set; }

    public DateTime ViewedAt { get; set; }

    public virtual NewsArticle NewsArticle { get; set; } = null!;

    public virtual SystemAccount? ViewedBy { get; set; }
}
