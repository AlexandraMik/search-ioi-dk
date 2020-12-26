
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Snowball;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Directory = Lucene.Net.Store.Directory;

namespace Search1
{
    class Program
    {
        static void BuildIndex()
        {
            // read file into a string and deserialize JSON to a type
            List<News> news = JsonConvert.DeserializeObject<List<News>>(File.ReadAllText(@"C:\Users\AlexandraMik\source\repos\Search1\data_file.json"));

            // deserialize JSON directly from a file
            using (var directory = GetDirectory())
            using (var analyzer = GetAnalyzer())
            using (var writer = new IndexWriter(directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                writer.DeleteAll();
                foreach (var product in news)
                {
                    var document = MapNews(product);
                    writer.AddDocument(document);
                }
            }
        }

        static Document MapNews(News news)
        {
            var document = new Document();
            document.Add(new Field("title", news.title, Field.Store.YES, Field.Index.ANALYZED));
            document.Add(new Field("text", news.text, Field.Store.YES, Field.Index.ANALYZED));
            document.Add(new Field("url", news.url, Field.Store.YES, Field.Index.NO));
            document.Add(new Field("date", news.date, Field.Store.YES, Field.Index.ANALYZED));
            return document;
        }

        static Directory GetDirectory()
        {
            return new SimpleFSDirectory(new DirectoryInfo(@"D:\SampleIndex"));
        }

        static Analyzer GetAnalyzer()
        {
            return new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
        }
        //static Query GetQuery(string keywords)
        //{
        //    using (var analyzer = GetAnalyzer())
        //    {
        //        var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "text", analyzer);
        //        var query = new BooleanQuery();
        //        var keywordsQuery = parser.Parse(keywords);
        //        query.Add(keywordsQuery, Occur.SHOULD);
        //        return query; 
        //    }
        //}
        static Query searchIndexWithTerm(string toSearch, string searchField, int limit)
        {
            using (var analyzer = GetAnalyzer())
            {
                Term term = new Term(searchField, toSearch);
                var query = new TermQuery(term);
                return query;
            }
        }
        static Query searchInBody(string toSearch, int limit)
        {
            using (var analyzer = GetAnalyzer())
            {
                QueryParser queryParser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30,"text", analyzer);
                Query query = queryParser.Parse(toSearch);
                return query;
            }
        }
        static void Main()
        {
            BuildIndex();
            int count;
            Console.WriteLine("Введите то, что нужно найти");
            string a = Console.ReadLine();
            var news = Search(a, 5, out count);
            foreach (var n in news)
            {
                Console.WriteLine(n.title);
                Console.WriteLine(n.date);
                Console.WriteLine(n.url);
                Console.WriteLine("---------------------------------");
            }
            Console.ReadKey();
        }
        static List<News> Search(string toSearch, int limit, out int count)
        {
            using (var directory = GetDirectory())
            using (var searcher = new IndexSearcher(directory))
            {
                var query = searchInBody(toSearch, 10);
                //var query = fuzzySearch(toSearch, "text", 10);
                var docs = searcher.Search(query, 5);
                count = docs.TotalHits;

                var products = new List<News>();
                foreach (var scoreDoc in docs.ScoreDocs)
                {
                    var doc = searcher.Doc(scoreDoc.Doc);
                    var product = new News
                    {
                        title = doc.Get("title"),
                        text = doc.Get("text"),
                        date = doc.Get("date"),
                        url = doc.Get("url")
                    };
                    products.Add(product);
                }
                return products;
            }
        }
    }
}
