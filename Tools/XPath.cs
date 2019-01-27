
using System.Text;

namespace CarSearch.Tools
{
    class XPath
    {
        private readonly StringBuilder _builder;

        public XPath()
        {
            _builder = new StringBuilder();
            _builder.Append(".");
        }

        public XPath Child(string type = "*")
        {
            _builder.Append($"//{type}");
            return this;
        }

        public XPath DirectChild(params string[] types)
        {
            foreach (var type in types)
            {
                _builder.Append($"/{type}");
            }
            return this;
        }

        public XPath WithAttribute(string attribute, string value)
        {
            _builder.Append($"[contains(@{attribute}, '{value}')]");
            return this;
        }

        public XPath WithText(string text)
        {
            _builder.Append($"[text() = '{text}']");
            return this;
        }

        public XPath Index(int index)
        {
            _builder.Append($"[{index}]");
            return this;
        }

        public XPath WithClass(string value)
        {
            return WithAttribute("class", value);
        }

        public override string ToString()
        {
            return _builder.ToString();
        }
    }
}