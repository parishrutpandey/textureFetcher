using System;
using Avalonia.Threading;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TextureFetcher;
using System.Collections.Generic;
using Fastenshtein;
using Lucene.Net.Analysis;
using Lucene.Net.Store;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Util;
using Lucene.Net.Analysis.Standard;


namespace TextureFetcher;


public class Model
{
    TextureFetcher.Index defaultIndex;
    public List<TextureMetadata>? inMemoryCache;


    public Model()
    {
        defaultIndex = new("~/AppData/Local/TextureFetcher/", "index.idx");
    }


    public async Task SyncAmbientCG()
    {
        AmbientCGProvider prov = new();
        var metadataList = prov.GetTextureMetadata(new Progress<float>()).Result;
        defaultIndex.WriteToIndex(new Progress<float>(), metadataList).Wait();
    }


    public async Task LoadFromDisk()
    {
        var data = await this.defaultIndex.ReadFromIndex(new Progress<float>());
        if (data == null)
            throw new Exception("Failed to Read Index File");
        this.inMemoryCache = data.Data;
    }
}


public class Searcher
{
    public static List<TextureMetadata> Search(string query, List<TextureMetadata> data)
    {
        if (query == "")
        {
            return data;
        }
        else
        {
            return _LuceneSearch(query, data);
        }


        List<TextureMetadata> _FuzzySearch()
        {
            var R_SearchResults = new List<TextureMetadata>();

            List<Tuple<TextureMetadata, int>> metadataAndScoreList = new();
            foreach (var m in data)
            {
                metadataAndScoreList.Add(new(m, Levenshtein.Distance(m.identifier, query)));
            }
            metadataAndScoreList.Sort((x, y) => x.Item2 - y.Item2);
            R_SearchResults.Clear();
            foreach (var e in metadataAndScoreList)
            {
                R_SearchResults.Add(e.Item1);
            }

            return R_SearchResults;
        }


        static List<TextureMetadata> _LuceneSearch(string query, List<TextureMetadata> data)
        {
            List<TextureMetadata> SearchResult_RetVal = new();

            var referenceDict = dictifyByIdentifier(data);
            Analyzer analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);
            Directory directory = new RAMDirectory();
            IndexWriterConfig config = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer);
            using (Lucene.Net.Index.IndexWriter iwriter = new(directory, config))
            {
                Document doc = new Document();
                doc.Add(new Field("id", query, TextField.TYPE_STORED));
                iwriter.AddDocument(doc);
            }
            using (Lucene.Net.Index.DirectoryReader ireader = DirectoryReader.Open(directory))
            {
                var isearcher = new IndexSearcher(ireader);
                var _query = new FuzzyQuery(new Term("id", query), 2);
                foreach (var hit in isearcher.Search(_query, null, 10000).ScoreDocs)
                {
                    string searchHitId = isearcher.Doc(hit.Doc).Get("id");
                    TextureMetadata metadataToAdd = referenceDict[searchHitId];
                    SearchResult_RetVal.Add(metadataToAdd);
                }
            }

            return SearchResult_RetVal;


            static Dictionary<string, TextureMetadata> dictifyByIdentifier(List<TextureMetadata> list)
            {
                Dictionary<string, TextureMetadata> dict_ = new();

                foreach (var metadata in list)
                {
                    dict_[metadata.identifier] = metadata;
                }

                return dict_;
            }
        }
    }


}
