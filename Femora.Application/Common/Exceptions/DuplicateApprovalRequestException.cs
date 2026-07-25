using System;

namespace Femora.Application.Common.Exceptions;

public class DuplicateApprovalRequestException : Exception
{
    public DuplicateApprovalRequestException()
    {
    }

    public DuplicateApprovalRequestException(string message) : base(message)
    {
    }

    public DuplicateApprovalRequestException(string message, Exception inner) : base(message, inner)
    {
    }
}
