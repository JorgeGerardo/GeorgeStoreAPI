using GeorgeStore.Common;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Extensions;

public static class ProblemDetailFactory
{
    public static ProblemDetails FromError(Error error) =>
        new() { Title = error.Tittle, Detail = error.Message };
}

