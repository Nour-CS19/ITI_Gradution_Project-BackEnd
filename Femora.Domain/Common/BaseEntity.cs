using Femora.Domain.Entities.LMS;
using Femora.Domain.Entities.LMS.Quizzes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Domain.Common;
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

}
