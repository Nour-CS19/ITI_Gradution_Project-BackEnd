using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Common.Exceptions;

public class NoNextModuleException(Guid courseId)
    : Exception($"No next module available. This is the last module in course '{courseId}'.");
