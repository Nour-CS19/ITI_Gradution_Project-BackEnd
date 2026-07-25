using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.Enrollments.Common.DTOs;

public class UnlockNextModuleResponse
{
    public Guid UnlockedModuleId { get; init; }
    public string UnlockedModuleTitle { get; init; } = string.Empty;
    public int ModuleOrderIndex { get; init; }
    public bool IsLastModule { get; init; }
    public bool AlreadyUnlocked { get; init; }
}
