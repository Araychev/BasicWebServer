using System.Collections;

namespace BasicWebServer.Server.HTTP
{
    public class CookieCollection : IEnumerable<Cookie>
    {
        private readonly Dictionary<string, Cookie> _cookies;

        public CookieCollection()
        {
            _cookies = new Dictionary<string, Cookie>();
        }

        public string this[string name]
            => _cookies[name].Value;

        public void Add(string name, string value)
            => _cookies[name] = new Cookie(name, value);

        public bool Contains(string name)
            => _cookies.ContainsKey(name);


        public IEnumerator<Cookie> GetEnumerator()
            => _cookies.Values.GetEnumerator();
        

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
