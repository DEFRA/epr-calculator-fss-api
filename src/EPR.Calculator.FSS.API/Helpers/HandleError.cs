﻿using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EPR.Calculator.FSS.API.Helpers;

public static class HandleError
{
    public static ActionResult Handle(Exception e)
    {
        if (e is HttpRequestException exception)
        {
            return HandleErrorWithStatusCode(exception.StatusCode);
        }

        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
    }

    public static ActionResult HandleErrorWithStatusCode(HttpStatusCode? statusCode)
    {
        switch (statusCode)
        {
            case HttpStatusCode.BadRequest:
                return new BadRequestResult();
            case HttpStatusCode.NotFound:
                return new NotFoundResult();
            case HttpStatusCode.Forbidden:
                return new StatusCodeResult((int)HttpStatusCode.Forbidden);
            default:
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
}