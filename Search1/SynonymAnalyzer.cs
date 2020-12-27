using System;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Synonym;
using Lucene.Net.Analysis.Util;

namespace Search1
{
    class SynonymAnalyzer:Analyzer
    {

        
        SynonymMap buildSynonym()
        {
            Stream stream = new FileStream(@"C:\Users\AlexandraMik\source\repos\Search1\Search1\wn_s.txt", FileMode.Open);
            TextReader rulesReader = new StreamReader(stream);
            WordnetSynonymParser parser = new WordnetSynonymParser(true, true, new StandardAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48, CharArraySet.EMPTY_SET));

            try
            {
                parser.Parse(rulesReader);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }
            SynonymMap synonymMap = parser.Build();
            return synonymMap;
        }

        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            Tokenizer source = new ClassicTokenizer(Lucene.Net.Util.LuceneVersion.LUCENE_48, reader);
            TokenStream filter = new StandardFilter(Lucene.Net.Util.LuceneVersion.LUCENE_48, source);
            filter = new LowerCaseFilter(Lucene.Net.Util.LuceneVersion.LUCENE_48, filter);
            SynonymMap mySynonymMap = null;
            try
            {
                mySynonymMap = buildSynonym();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
            filter = new SynonymFilter(filter, mySynonymMap, false);
            return new TokenStreamComponents(source, filter);
        }
    }
}
