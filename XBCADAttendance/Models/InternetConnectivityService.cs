namespace XBCADAttendance.Models
{
    public static class InternetConnectivityService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<bool> IsInternetAvailableAsync()
        {
            try
            {
                // Sending a simple HEAD request to check internet connectivity
                var response = await _httpClient.GetAsync("https://www.google.com");

                // If the status code is successful, the server has internet access
                return response.IsSuccessStatusCode;
            } catch
            {
                // If there is an error return false
                return false;
            }
        }
    }
}
