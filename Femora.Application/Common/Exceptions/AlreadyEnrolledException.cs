using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Common.Exceptions;

public class AlreadyEnrolledException(Guid courseId) : Exception($"User already enrolled in this course id: '{courseId}'");