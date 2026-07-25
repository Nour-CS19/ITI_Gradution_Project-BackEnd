using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Common.Interfaces.Repositories.LMS;

public interface IQuizRepository
{
    Task<bool> HasPassedAsync(Guid enrollmentId, Guid moduleId, CancellationToken ct = default);
}
