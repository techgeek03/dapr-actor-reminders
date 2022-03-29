using Refit;

namespace Dapr.Testing.Actors.Runtime;

public interface IDelayMeHttpClient
{
    [Get("/{delay}/https://google.com/")]
    Task<HttpResponseMessage> Delay(int delay);
}
