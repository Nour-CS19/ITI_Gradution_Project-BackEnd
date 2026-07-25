using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.Identity.Common.Requests;

public record SigninRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
