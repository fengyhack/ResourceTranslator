using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;
using ClosedXML.Excel;

namespace ResourceTranslator
{
    public static class StringResourceHelper
    {
        /// <param name="ki">keyIndex = 2</param>
        public static TranslationDict LoadFromXlsx(string xlsxFile, int ki, Dictionary<EnumLanguage,int> tMap)
        {
            var dict = new TranslationDict();
            var wb = new XLWorkbook(xlsxFile);
            IXLWorksheet sheet = wb.Worksheet(1);
            var rows = sheet.Rows();
            var nr = 0;
            foreach (var row in rows)
            {
                if (++nr > 1)
                {
                    var key = row.Cell(ki).Value.ToString().Trim();
                    if (!dict.ContainsKey(key))
                    {
                        dict.Add(new Translation(key));                        
                    }
                    foreach (var t in tMap)
                    {
                        var s = row.Cell(t.Value).Value.ToString().Trim();
                        dict[key].Add(t.Key, s);
                    }
                }
            }

            return dict;
        }

        public static TranslationDict LoadFromResx(string resxFile, EnumLanguage lang)
        {
            var dict = new TranslationDict();
            var xDoc = new XmlDocument();
            xDoc.Load(resxFile);
            var root = xDoc.DocumentElement;
            var nodeList = root.GetElementsByTagName("data");
            foreach (var node in nodeList)
            {
                var e = (XmlElement)node;
                var key = e.GetAttribute("name");
                var children = e.GetElementsByTagName("value");
                foreach (var c in children)
                {
                    var value = ((XmlElement)c).InnerText;
                    dict.Append(key, lang, value);
                    break;
                }
            }
            return dict;
        }

        public static TranslationDict LoadFromXaml(string xamlFile, EnumLanguage lang)
        {
            var dict = new TranslationDict();
            var p = "x:Key=\"";
            var s1 = " xml:space=\"preserve\">";
            var lines = new List<string>();
            using (var sr = new StreamReader(xamlFile))
            {
                while (!sr.EndOfStream)
                {
                    var s = sr.ReadLine();
                    if (s.Length > p.Length)
                    {
                        var i = s.IndexOf(p);
                        if (i > 0)
                        {
                            i = i + p.Length;
                            var j = s.IndexOf('\"', i + 1);
                            if (j > 0)
                            {
                                var key = s.Substring(i, j - i);
                                i = j + s1.Length;
                                j = s.IndexOf('<');
                                var value = s.Substring(i, j - i);
                                dict.Append(key, lang, value);
                            }
                        }
                    }
                }
            }
            return dict;
        }

        public static void SaveAsXlsx(TranslationDict dict, string xlsxFile)
        {
            var map = new Dictionary<string, EnumLanguage>();
            var enumType = typeof(EnumLanguage);
            var descType = typeof(DescriptionAttribute);
            foreach (EnumLanguage lang in Enum.GetValues(enumType))
            {
                var fields = enumType.GetField(lang.ToString());
                var attr = (fields.GetCustomAttributes(descType, false))[0] as DescriptionAttribute;
                map.Add(attr.Description, lang);
            }

            var wb = new XLWorkbook();
            var name = DateTime.Now.ToString("yyyyMMdd");
            var sheet = wb.AddWorksheet(name, 1);
            var row = 1;
            var col = 1;
            sheet.Cell(row, col).Value = "-";
            ++col;
            sheet.Cell(row, col).Value = "key";
            foreach (var v in map)
            {
                ++col;
                sheet.Cell(row, col).Value = v.Key;
            }
            foreach (var key in dict.Keys)
            {
                ++row;
                col = 2;
                sheet.Cell(row, col).Value = key;
                foreach (var v in map)
                {
                    ++col;
                    sheet.Cell(row, col).Value = dict[key][v.Value];
                }
            }
            wb.SaveAs(xlsxFile);            
        }

        public static void ReplaceResx(TranslationDict dict, EnumLanguage lang, string resxFile, string destFile)
        {
            var xDoc = new XmlDocument();
            xDoc.Load(resxFile);
            var root = xDoc.DocumentElement;
            var nodeList = root.GetElementsByTagName("data");
            foreach (var node in nodeList)
            {
                var e = (XmlElement)node;
                var key = e.GetAttribute("name");
                if (!dict.ContainsKey(key))
                {
                    Console.WriteLine(key);
                    continue;
                }
                var children = e.GetElementsByTagName("value");
                foreach (var c in children)
                {
                    var value = ((XmlElement)c).InnerText;
                    ((XmlElement)c).InnerText = dict[key][lang];
                    break;
                }
            }
            xDoc.Save(destFile);
        }

        public static void ReplaceXaml(TranslationDict dict, EnumLanguage lang, string xamlFile, string destFile)
        {
            var p = "x:Key=\"";
            var s1 = " xml:space=\"preserve\">";
            var s2 = "</sys:String>";
            var lines = new List<string>();
            using (var sr = new StreamReader(xamlFile))
            {
                while (!sr.EndOfStream)
                {
                    var s = sr.ReadLine();
                    if (p.Length > s.Length)
                    {
                        lines.Add(s);
                    }
                    else
                    {
                        var i = s.IndexOf(p);
                        if (i < 0)
                        {
                            lines.Add(s);
                        }
                        else
                        {
                            i = i + p.Length;
                            var j = s.IndexOf('\"', i + 1);
                            if (j > 0)
                            {
                                var key = s.Substring(i, j - i);
                                if (dict.ContainsKey(key))
                                {
                                    var c = s.Substring(0, j + 1) + s1 + dict[key][lang] + s2;
                                    lines.Add(c);
                                }
                                else
                                {
                                    lines.Add(s);
                                }
                            }
                            else
                            {
                                lines.Add(s);
                            }
                        }
                    }
                }
            }
            File.WriteAllLines(destFile, lines);
        }
    }
}
