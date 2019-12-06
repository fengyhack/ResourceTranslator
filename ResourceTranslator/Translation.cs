using System.Collections.Generic;

namespace ResourceTranslator
{
    public class Translation : ObservableObject
    {
        private string _Key;
        public string Key
        {
            get
            {
                return _Key;
            }
            private set
            {
                _Key = value;
                RaisePropertyChanged();
            }
        }

        private Dictionary<EnumLanguage, string> _Collection;
        public Dictionary<EnumLanguage,string> Collection
        {
            get
            {
                return _Collection;
            }
            private set
            {
                _Collection = value;
                RaisePropertyChanged();
            }
        }

        public string this[EnumLanguage lang]
        {
            get
            {
                if (Collection != null && Collection.ContainsKey(lang))
                {
                    return Collection[lang];
                }
                else
                {
                    return null;
                }
            }
        }

        public Translation(string key)
        {
            Key = key;
            Collection = new Dictionary<EnumLanguage, string>();
        }

        public void Add(EnumLanguage lang, string value)
        {
            if(Collection.ContainsKey(lang))
            {
                Collection[lang] = value;
            }
            else
            {
                Collection.Add(lang, value);
            }
            RaisePropertyChanged("Collection");
        }

        public void Remove(EnumLanguage lang)
        {
            if(Collection.ContainsKey(lang))
            {
                Collection.Remove(lang);
                RaisePropertyChanged("Collection");
            }
        }
    }
}
