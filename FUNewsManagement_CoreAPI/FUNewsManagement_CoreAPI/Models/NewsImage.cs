using System;
using System.Collections.Generic;

namespace FUNewsManagement_CoreAPI.Models;

public partial class NewsImage
{
    public int ImageId { get; set; }

    public string NewsArticleId { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public virtual NewsArticle NewsArticle { get; set; } = null!;
}
