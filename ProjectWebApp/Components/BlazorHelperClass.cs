using Humanizer.Localisation;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ProjectWebApp.Components
{
    public static class BlazorHelperClass
    {
       public static Exception GetInnerException(System.Exception ex)
       {
            while (ex.InnerException != null)
                ex = ex.InnerException;
            return ex;
       }
    }
}