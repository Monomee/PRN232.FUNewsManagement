using System;
using System.Collections.Generic;

namespace FUNewsManagement_AnalyticsAPI.DataAccess.Entities;

public class Category
{
    public short CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string CategoryDesciption { get; set; } = null!;
    public short? ParentCategoryId { get; set; }
    public bool? IsActive { get; set; }

    public virtual Category? ParentCategory { get; set; }
    public virtual ICollection<Category> InverseParentCategory { get; set; } = new List<Category>();
    public virtual ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();
}

public class SystemAccount
{
    public short AccountId { get; set; }
    public string? AccountName { get; set; }
    public string? AccountEmail { get; set; }
    public int? AccountRole { get; set; }
    public string? AccountPassword { get; set; }

    public virtual ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();
}

public class Tag
{
    public int TagId { get; set; }
    public string? TagName { get; set; }
    public string? Note { get; set; }

    public virtual ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();
}

public class NewsArticle
{
    public string NewsArticleId { get; set; } = null!;
    public string? NewsTitle { get; set; }
    public string Headline { get; set; } = null!;
    public DateTime? CreatedDate { get; set; }
    public string? NewsContent { get; set; }
    public string? NewsSource { get; set; }
    public short? CategoryId { get; set; }
    public bool? NewsStatus { get; set; }
    public short? CreatedById { get; set; }
    public short? UpdatedById { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public string? ImageUrl { get; set; }
    public int ViewCount { get; set; }

    public virtual Category? Category { get; set; }
    public virtual SystemAccount? CreatedBy { get; set; }
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
