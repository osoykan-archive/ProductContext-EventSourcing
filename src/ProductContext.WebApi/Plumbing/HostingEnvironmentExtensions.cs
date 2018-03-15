using Microsoft.AspNetCore.Hosting;

namespace ProductContext.WebApi.Plumbing
{
    public static class HostingEnvironmentExtensions
    {
        public static bool IsLocal(this IHostingEnvironment env)
        {
            if (env.EnvironmentName == "Local")
            {
                return true;
            }

            return false;
        }
    }
}
