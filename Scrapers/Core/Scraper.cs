
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CarSearch.Tools;
using HtmlAgilityPack;

namespace CarSearch.Scrapers
{
    struct State<T>
    {
        public string Name { get; }
        public HtmlNode Node { get; }
        public T Value { get; }

        public State(string name, HtmlNode node)
        {
            Name = name;
            Node = node;
            Value = default(T);
        }

        public State(string name, HtmlNode node, T value)
        {
            Name = name;
            Node = node;
            Value = value;
        }
    }

    struct Transform<T>
    {
        public string From { get; }
        public string To { get; }
        public Func<XPath, XPath> Perform { get; }
        public Func<T> Creates { get; set; }
        public Action<T, HtmlNode> Modifies { get; set; }
        public Func<HtmlNode, string> Follows { get; set; }

        public Transform(string from, string to, Func<XPath, XPath> perform)
        {
            From = from;
            To = to;
            Perform = perform;
            Creates = null;
            Modifies = null;
            Follows = null;
        }
    }

    class TransformBuilder<T>
    {
        private Transform<T> _transform;
        private readonly List<Transform<T>> _list;

        public TransformBuilder(Transform<T> transform, List<Transform<T>> list)
        {
            _transform = transform;
            _list = list;
        }

        public TransformBuilder<T> Constructs(Func<T> creates)
        {
            _transform.Creates = creates;
            return this;
        }

        public TransformBuilder<T> SetsValue(Action<T, HtmlNode> updates)
        {
            _transform.Modifies = updates;
            return this;
        }

        public TransformBuilder<T> Follows(Func<HtmlNode, string> getUrl)
        {
            _transform.Follows = getUrl;
            return this;
        }

        public void Build()
        {
            _list.Add(_transform);
        }
    }

    class Scraper<T>
    {
        public string Url { get; set; }
        public List<T> Results = new List<T>();

        private List<Transform<T>> Transforms = new List<Transform<T>>();

        protected TransformBuilder<T> Transform(string to, Func<XPath, XPath> parsing)
        {
            var transform = new Transform<T>(null, to, parsing);
            return new TransformBuilder<T>(transform, Transforms);
        }

        protected TransformBuilder<T> Transform(string from, string to, Func<XPath, XPath> parsing)
        {
            var transform = new Transform<T>(from, to, parsing);
            return new TransformBuilder<T>(transform, Transforms);
        }

        protected TransformBuilder<T> Transform(string from)
        {
            var transform = new Transform<T>(from, null, null);
            return new TransformBuilder<T>(transform, Transforms);
        }

        public void Execute()
        {
            var states = new Queue<State<T>>();
            states.Enqueue(LoadUrl(Url));

            while (states.Any())
            {
                var state = states.Dequeue();
                var transforms = Transforms.Where(t => t.From == state.Name);
                var newStates = transforms.SelectMany(t => TransformState(t, state));

                foreach (var newSate in newStates)
                {
                    states.Enqueue(newSate);
                }
            }

            Results = Filter(Results).ToList();
        }

        private IEnumerable<State<T>> TransformState(Transform<T> transform, State<T> state)
        {
            var to = transform.To;
            var xpath = transform.Perform(new XPath());
            var nodes = state.Node.SelectNodes(xpath.ToString());

            if (nodes == null)
            {
                yield break;
            }

            foreach (var node in nodes)
            {
                if (transform.Follows != null)
                {
                    var url = transform.Follows(node);
                    if (!string.IsNullOrEmpty(url))
                    {
                        yield return LoadUrl(url);
                    }
                }

                var value = state.Value;
                if (value == null && transform.Creates != null)
                {
                    value = transform.Creates();
                    Results.Add(value);
                }
                else if (value != null && transform.Modifies != null)
                {
                    transform.Modifies(value, node);
                }

                yield return new State<T>(to, node, value);
            }
        }

        protected virtual IEnumerable<T> Filter(IEnumerable<T> results)
        {
            return results;
        }

        protected decimal InnerNumber(string text)
        {
            var textNumbers = Regex.Replace(text, @"[^\d]", ""); ;
            if (int.TryParse(textNumbers, out int value))
            {
                return value;
            }
            return 0;
        }

        protected decimal InnerNumber(HtmlNode node)
        {
            return InnerNumber(node.InnerText);
        }

        private State<T> LoadUrl(string url)
        {
            var web = new HtmlWeb();
            var doc = web.Load(url);

            return new State<T>(null, doc.DocumentNode);
        }
    }
}