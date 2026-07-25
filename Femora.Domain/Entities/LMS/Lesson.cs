using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Femora.Domain.Common;
using Femora.Domain.Enums;

namespace Femora.Domain.Entities.LMS
{
    public class Lesson : BaseEntity
    {
        [Required]
        public Guid ModuleId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public LessonType Type { get; set; }

        [MaxLength(500)]
        public string? ContentUrl { get; set; }

        public string? ArticleContent { get; set; }

        [Range(0, int.MaxValue)]
        public int? DurationSeconds { get; set; }

        [Required]
        [Range(1, 1000)]
        public int OrderIndex { get; set; }

        public bool IsPreview { get; set; } = false;

        // Navigation Properties
        [ForeignKey(nameof(ModuleId))]
        public Module Module { get; set; } = null!;

        public ICollection<Resource> Resources { get; set; } = new List<Resource>();
        public ICollection<LessonResource> LessonResources { get; set; } = new List<LessonResource>();
        public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
    }
}