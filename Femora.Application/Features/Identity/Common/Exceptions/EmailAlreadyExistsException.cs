using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.Identity.Common.Exceptions;
public sealed class EmailAlreadyExistsException(string email) : Exception($"Email: {email} is already exist");
