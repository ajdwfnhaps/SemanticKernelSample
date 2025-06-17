using System.ComponentModel.DataAnnotations;

namespace GatewayOperationSystem.Core.Models;

public class KnowledgeBase
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public string? Summary { get; set; }
    
    public List<string> Tags { get; set; } = new();
    
    public string Category { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public float[]? Embedding { get; set; }
}

public class KnowledgeSearchResult
{
    public string? Id { get; set; }
    public float Score { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Category { get; set; }
    public List<string>? Tags { get; set; }
} 