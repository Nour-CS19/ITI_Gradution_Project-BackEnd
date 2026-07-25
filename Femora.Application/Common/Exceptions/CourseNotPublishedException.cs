using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Common.Exceptions;

public class CourseNotPublishedException(Guid courseId) : Exception($"Course with id: '{courseId}' is not published");