using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Common.Exceptions;

public class QuizNotPassedException(Guid moduleId)
    : Exception($"Quiz for module '{moduleId}' has not been passed yet. Score must be ≥ 60%.");