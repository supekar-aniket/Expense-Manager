namespace ExpenseManager.Helper
{
    public class UrlHelper
    {
        private readonly IConfiguration _config;

        public UrlHelper(IConfiguration config)
        {
            _config = config;
        }

        public string BuildUrl(string path)
        {
            var publicUrl = _config["AppSettings:PublicUrl"].TrimEnd('/');
            path = path.TrimStart('/');
            return $"{publicUrl}/{path}";
        }
    }

}
