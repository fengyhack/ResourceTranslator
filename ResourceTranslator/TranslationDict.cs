using System.Collections.Generic;
using System.Linq;

namespace ResourceTranslator
{
    public class TranslationDict : ObservableObject
    {
        private Dictionary<string,int> indexMap;

        private List<Translation> _Translations;
        public List<Translation> Translations
        {
            get
            {
                return _Translations;
            }
            private set
            {
                _Translations = value;
                RaisePropertyChanged();
            }
        }

        public Translation this[string key]
        {
            get
            {
                return FindByKey(key);
            }
            set
            {
                Add(value);
            }
        }

        public int Count
        {
            get
            {
                return Translations.Count;
            }
        }

        public List<string> Keys
        {
            get
            {
                return indexMap.Keys.ToList();
            }
        }

        public TranslationDict()
        {
            indexMap = new Dictionary<string, int>();
            Translations = new List<Translation>();
        }

        public bool ContainsKey(string key)
        {
            return indexMap.ContainsKey(key);
        }

        public Translation FindByKey(string key)
        {
            if (indexMap.ContainsKey(key))
            {
                var index = indexMap[key];
                return Translations[index];
            }
            else
            {
                return null;
            }
        }

        public void Add(Translation translation)
        {
            var key = translation.Key;
            if(string.IsNullOrEmpty(key))
            {
                return;
            }

            if (indexMap.ContainsKey(key))
            {
                var index = indexMap[key];
                Translations[index] = translation;
            }
            else
            {
                indexMap.Add(key, Translations.Count);
                Translations.Add(translation);
            }
            RaisePropertyChanged("Translations");
        }

        public void Combine(TranslationDict dict)
        {
            var keys = dict.Keys;
            foreach(var key in keys)
            {
                if (indexMap.ContainsKey(key))
                {
                    var index = indexMap[key];
                    Translations[index] = dict[key];
                }
                else
                {
                    indexMap.Add(key, Translations.Count);
                    Translations.Add(dict[key]);
                }
            }
            RaisePropertyChanged("Translations");
        }

        public void Append(string key, EnumLanguage lang, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            if (indexMap.ContainsKey(key))
            {
                var index = indexMap[key];
                Translations[index].Add(lang, value);
            }
            else
            {
                indexMap.Add(key, Translations.Count);
                var translation = new Translation(key);
                translation.Add(lang, value);
                Translations.Add(translation);                
            }
            RaisePropertyChanged("Translations");
        }

        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            if (indexMap.ContainsKey(key))
            {
                var index = indexMap[key];
                Translations.RemoveAt(index);
                indexMap.Remove(key);
                RaisePropertyChanged("Translations");
            }            
        }

        public void Clear()
        {
            indexMap.Clear();
            Translations.Clear();
            RaisePropertyChanged("Translations");
        }
    }
}
