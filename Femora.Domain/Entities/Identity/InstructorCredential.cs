using Femora.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Domain.Entities.Identity;
public class InstructorCredential : BaseEntity
{
    public Guid InstructorProfileId { get; set; }
    public string ImageUrl { get; set; }
    public string? Title { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public InstructorProfile InstructorProfile { get; set; } = null;
}
