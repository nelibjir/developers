using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MewHomeWork.Services.Externals.Clients
{
  public class HttpClient
  {
    public async Task<string> GetDataFromSourceAsync(string url, CancellationToken cancellation)
    {
      using (var client = new System.Net.Http.HttpClient())
      {
        try
        {
          HttpResponseMessage response = await client.GetAsync(url, cancellation);
          response.EnsureSuccessStatusCode();
          return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException e)
        {
          throw new HttpRequestException("Bad response code from CNB ", e);
        }
      }
    }
  }
}
