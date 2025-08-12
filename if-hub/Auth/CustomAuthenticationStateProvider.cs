namespace if_hub.Auth
{
    using Microsoft.AspNetCore.Components.Authorization;
    using System.Security.Claims;
    using System.Text.Json;
    using Microsoft.JSInterop;

    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

                if (string.IsNullOrWhiteSpace(token))
                {
                    return new AuthenticationState(_anonymous);
                }

                var claims = ParseClaimsFromJwt(token);
                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch
            {
                return new AuthenticationState(_anonymous);
            }
        }

        public void NotifyUserAuthentication(string token)
        {
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            var authState = Task.FromResult(new AuthenticationState(user));
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            var authState = Task.FromResult(new AuthenticationState(_anonymous));
            NotifyAuthenticationStateChanged(authState);
        }

        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs != null)
            {
                // Pega o ID (nameid) e cria a claim com o tipo padrão NameIdentifier
                keyValuePairs.TryGetValue("nameid", out object? id);
                if (id != null)
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, id.ToString()!));
                }

                // Pega o Email
                keyValuePairs.TryGetValue("email", out object? email);
                if (email != null)
                {
                    claims.Add(new Claim(ClaimTypes.Email, email.ToString()!));
                }

                // Pega o Papel (Role)
                keyValuePairs.TryGetValue("role", out object? role);
                if (role != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.ToString()!));
                }

                // Pega o Nome (unique_name) e cria a claim com o tipo padrão Name
                keyValuePairs.TryGetValue("unique_name", out object? name);
                if (name != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, name.ToString()!));
                }
            }
            return claims;
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
