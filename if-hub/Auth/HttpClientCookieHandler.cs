namespace if_hub.Auth
{
    public class HttpClientCookieHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpClientCookieHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var cookie = _httpContextAccessor.HttpContext?.Request.Headers["Cookie"];
            
            if (!string.IsNullOrEmpty(cookie))
            {
                // CORREÇÃO: Usamos .ToString() para resolver a ambiguidade
                request.Headers.Add("Cookie", cookie.ToString());
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}