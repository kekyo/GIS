using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace importfromcsv
{
    static class CsvReader
    {
        public static CsvReader<T> Create<T>(string path)
            where T : class, new() =>
            new CsvReader<T>(path);
    }

    sealed class CsvReader<T> : IEnumerable<T>
        where T : class, new()
    {
        private static readonly char[] comma = new[] { ',' };
        private static readonly Dictionary<string, Action<object, string>> settersByName =
            typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).
            Where(field => field.CanRead && field.CanWrite).
            ToDictionary(
                field => field.Name,
                field => new Action<object, string>((instance, value) => field.SetValue(instance, Convert.ChangeType(value, field.PropertyType))));

        private readonly string path;

        public CsvReader(string path) => this.path = path;

        public IEnumerator<T> GetEnumerator()
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            {
                var tr = new StreamReader(fs, Encoding.UTF8, true);

                if (!tr.EndOfStream)
                {
                    var line = tr.ReadLine();
                    var setters =
                        line.Split(comma).
                        Select(name => settersByName.TryGetValue(name.Trim(), out var setter) ? setter : null).
                        ToArray();

                    while (!tr.EndOfStream)
                    {
                        line = tr.ReadLine();

                        var instance = new T();
                        foreach (var entry in
                            line.Split(comma).
                            Select(value => value.Trim()).
                            Zip(setters, (value, setter) => (value, setter)).
                            Where(entry => entry.setter != null))
                        {
                            entry.setter(instance, entry.value);
                        }

                        yield return instance;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
