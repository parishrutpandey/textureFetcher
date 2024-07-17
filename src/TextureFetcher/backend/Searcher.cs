using System.Collections.Generic;
using Lucene.Net.Analysis;
using Lucene.Net.Store;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Lucene.Net.Analysis.Standard;


namespace TextureFetcher;


public class Searcher
{
    static List<TextureMetadata> luceneSearch(string query, List<TextureMetadata> data)
    {
        List<TextureMetadata> returnValue_SearchResult = new();
        Analyzer analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);
        Directory directory = new RAMDirectory();
        IndexWriterConfig config = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer);
        using (Lucene.Net.Index.IndexWriter iwriter = new(directory, config))
        {
            foreach (var metadata in data)
            {
                Document doc = new Document();
                doc.Add(new Field("id", metadata.Identifier, TextField.TYPE_STORED));
                iwriter.AddDocument(doc);
            }
        }
        using (Lucene.Net.Index.DirectoryReader ireader = DirectoryReader.Open(directory))
        {
            var isearcher = new IndexSearcher(ireader);
            var _query = new FuzzyQuery(new Term("id", query), 2);

            var referenceDict = dictifyByIdentifier(data);
            foreach (var hit in isearcher.Search(_query, null, 10000).ScoreDocs)
            {
                string searchHitId = isearcher.Doc(hit.Doc).Get("id");
                TextureMetadata metadataToAdd = referenceDict[searchHitId];
                returnValue_SearchResult.Add(metadataToAdd);
            }
        }

        return returnValue_SearchResult;


        static Dictionary<string, TextureMetadata> dictifyByIdentifier(List<TextureMetadata> list)
        {
            Dictionary<string, TextureMetadata> dict_ = new();

            foreach (var metadata in list)
            {
                dict_[metadata.Identifier] = metadata;
            }

            return dict_;
        }
    }

    public static List<TextureMetadata> Search(string query, List<TextureMetadata> data)
    {
        if (query == "")
        {
            return data;
        }
        else
        {
            return luceneSearch(query, data);
        }


    }
}

