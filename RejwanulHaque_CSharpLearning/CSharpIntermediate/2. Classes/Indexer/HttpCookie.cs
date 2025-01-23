namespace _2._Classes.Indexer
{
    public class HttpCookie
    {
        private readonly Dictionary<string, string> _dictionary;
        public HttpCookie()
        {
            _dictionary = new Dictionary<string, string>();
        }
        public string this[string key]
        {
            get 
            {
                if (_dictionary.ContainsKey(key))
                    return _dictionary[key];
                else 
                    return $"Property {key} for HttpCookie Not Found";
            }
            set { _dictionary[key] = value; }
        }
    }
}


/*
 * 
 * Making the object's properties stored in a dictionary. 
 * 
 */