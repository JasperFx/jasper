using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Baseline;

namespace Jasper.Conneg
{
    public class MimeTypeList : IEnumerable<string>
    {
        private readonly IList<string> _mimeTypes = new List<string>();

        // put stuff after ';' over to the side
        // look for ',' separated values
        public MimeTypeList(string mimeType)
        {
            Raw = mimeType;


            var types = mimeType?.ToDelimitedArray().Select(x => x.Split(';')[0]).Where(x => x.IsNotEmpty())
                        ?? new string[0];

            _mimeTypes.AddRange(types);

        }

        public string Raw { get; private set; }

        public MimeTypeList(params string[] mimeTypes)
        {
            Raw = mimeTypes.Select(x => x).Join(";");
            _mimeTypes.AddRange(mimeTypes);
        }

        public void AddMimeType(string mimeType)
        {
            _mimeTypes.Add(mimeType);
        }

        public bool Matches(params string[] mimeTypes)
        {
            return _mimeTypes.Intersect(mimeTypes).Any();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _mimeTypes.GetEnumerator();
        }

        public override string ToString()
        {
            return _mimeTypes.Join(", ");
        }

        public bool AcceptsAny()
        {
            return _mimeTypes.Count == 0 || _mimeTypes.Contains("*/*");
        }
    }
}