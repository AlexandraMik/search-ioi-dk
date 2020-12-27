using System;
using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Newtonsoft.Json;
using Search1.src;

namespace Search1
{
    class Program
    {
        public static Document MapNews(News news)
        {
            var sourse = new
            {
                title = news.title,
                text = news.text,
                url = news.url,
                date = news.date
            };
            var document = new Document()
            {
                new StringField("title", sourse.title, Field.Store.YES),
                new StringField("text", sourse.text, Field.Store.YES),
                new StringField("url", sourse.url, Field.Store.YES),
                new StringField("date", sourse.date, Field.Store.YES)
            };
            return document;

        }

        static void Main()
        {
            List<News> news = JsonConvert.DeserializeObject<List<News>>(File.ReadAllText(@"C:\Users\AlexandraMik\source\repos\Search1\Search1\data_file.json"));
            // Ensures index backward compatibility
            const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;

            // Construct a machine-independent path for the index
            var basePath = Environment.GetFolderPath(
                Environment.SpecialFolder.CommonApplicationData);
            var indexPath = Path.Combine(basePath, "index");

            using var dir = FSDirectory.Open(indexPath);

            // Create an analyzer to process the text
            var analyzer = new SynonymAnalyzer();

            // Create an index writer
            var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyzer);
            using var writer = new IndexWriter(dir, indexConfig);
            writer.DeleteAll();
            foreach (var n in news)
            {
                var doc = new Document
                {
                    // StringField indexes but doesn't tokenize
                    new StringField("title",
                        n.title,
                        Field.Store.YES),
                    new TextField("text",
                        n.text,
                        Field.Store.YES),
                    new TextField("url",
                        n.url,
                        Field.Store.YES),
                    new TextField("date",
                        n.date,
                        Field.Store.YES)
                };
                writer.AddDocument(doc);
                writer.Flush(triggerMerge: false, applyAllDeletes: false);
            }
            List<MultiPhraseQuery> list = new List<MultiPhraseQuery>();

            // Search with a phrase
            list.Add(new MultiPhraseQuery
            {
                new Term("text", "4k"),
                new Term("text", "resolution")
            });

            list.Add(new MultiPhraseQuery
            {
                new Term("text", "hitman"),
                new Term("text", "3")
            });
            list.Add(new MultiPhraseQuery
            {
                new Term("text", "talented"),
                new Term("text", "developers")
            });
            list.Add(new MultiPhraseQuery
            {
                new Term("text", "world"),
                new Term("text", "of"),
                new Term("text", "assassination")
            });
            list.Add(new MultiPhraseQuery
            {
                new Term("text", "roadmap")
            });
            list.Add(new MultiPhraseQuery
            {
                new Term("date", "may"),
                new Term("date", "29")
            });
            list.Add(new MultiPhraseQuery
            {
                new Term("date", "june"),
            });
            list.Add(new MultiPhraseQuery
            {
                new Term("text", "the"),
                new Term("text", "undying")
            });
            list.Add(new MultiPhraseQuery
            {
                new Term("text", "elusive"),
                new Term("text", "targets")
            });
            list.Add(new MultiPhraseQuery
            {
                new Term("text", "overachievers")
            });
            // Re-use the writer to get real-time updates
            foreach (var l in list)
            {
                Console.WriteLine("query result");
                using var reader = writer.GetReader(applyAllDeletes: true);
                var searcher = new IndexSearcher(reader);
                var hits = searcher.Search(l, 10 /* top 20 */).ScoreDocs;


                foreach (var hit in hits)
                {
                    var foundDoc = searcher.Doc(hit.Doc);
                    Console.WriteLine($"{hit.Score:f8}" +
                                      $" {foundDoc.Get("title"),-15}" +
                                      $" {foundDoc.Get("url"),-40}");
                }
                Console.WriteLine("------------------------");
            }

            Console.ReadKey();
        }
    }
}
