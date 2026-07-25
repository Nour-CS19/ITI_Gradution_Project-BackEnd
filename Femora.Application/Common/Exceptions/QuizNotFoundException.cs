using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Common.Exceptions;

public class QuizNotFoundException(Guid moduleId)
    : Exception($"No quiz found for module '{moduleId}'. Complete all lessons first.");