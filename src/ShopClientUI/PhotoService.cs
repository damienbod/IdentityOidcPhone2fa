namespace AdminClientUI;

public class PhotoService
{
    private readonly IHttpClientFactory _clientFactory;

    public PhotoService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<string> GetPhotoAsync()
    {
        try
        {
            var client = _clientFactory.CreateClient("profileClient");

            var response = await client.GetAsync("api/Profiles/photo");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();

                if (data != null)
                    return data;

                return string.Empty;
            }

            throw new ApplicationException($"Status code: {response.StatusCode}, Error: {response.ReasonPhrase}");
        }
        catch (Exception e)
        {
            throw new ApplicationException($"Exception {e}");
        }
    }
}