using System;
using System.Linq;
using System.Net.Http;
using HtmlAgilityPack;
using NHunspell;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WebScraping_
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void araButton_Click(object sender, EventArgs e)
        {
            string arananKelime = araTextBox.Text;
            if (!string.IsNullOrEmpty(arananKelime))
            {
                string correctedText = arananKelime;

                string url = "https://scholar.google.com/scholar?hl=tr&as_sdt=0%2C5&q=" + Uri.EscapeUriString(correctedText) + "&btnG=";
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(url);
                var HeaderNames = doc.DocumentNode.SelectNodes("//h3[@class='gs_rt']");
                var HeaderUrls = doc.DocumentNode.SelectNodes("//h3[@class='gs_rt']//a[@href]");
                var CiteCounts = doc.DocumentNode.SelectNodes("//div[@class='gs_fl gs_flb']//a[contains(@href,'cites')]");
                var User = doc.DocumentNode.SelectNodes("//div[@class='gs_a']//a[contains(@href,'user')]");
                var PublicationDates = doc.DocumentNode.SelectNodes("//div[@class='gs_a']");

                results.InnerHtml = "";
                if (HeaderNames != null && HeaderUrls != null && CiteCounts != null && User != null && PublicationDates != null)
                {
                    for (int i = 0; i < HeaderNames.Count && i < HeaderUrls.Count && i < CiteCounts.Count && i < PublicationDates.Count && i < 10; i++) // İlk 10 sonucu al
                    {
                        string title = HeaderNames[i].InnerText;
                        string urlLink = HeaderUrls[i].GetAttributeValue("href", "");
                        string citeCountStr = CiteCounts[i].InnerText.Trim().Split(' ')[2]; // Cite sayısını al
                        string publicationDate = PublicationDates[i].InnerText; // Yayınlanma tarihini al

                        // Yayınlanma tarihini ve yayıncı adını ayır
                        Match match = Regex.Match(publicationDate, @"(?<date>\d+)");
                        if (match.Success)
                        {
                            string publicationDateStr = match.Groups["date"].Value.Trim();

                            // HTML olarak sonuçları oluştur
                            results.InnerHtml += "<div class='result'><a href='" + urlLink + "' target='_blank'>" + title + "</a></div>";

                            // MongoDB bağlantısı ve veri ekleme işlemi
                            MongoConnection mongoConnection = new MongoConnection();
                            IMongoDatabase database = mongoConnection.ConnectToMongoDB();
                            var collection = database.GetCollection<BsonDocument>("Test");
                            var document = new BsonDocument
                                 {
                                    { "arananKelime", arananKelime },
                                    { "baslik", title },
                                    { "urlLink", urlLink },
                                    { "alintiSayisi", int.Parse(citeCountStr) },
                                    { "yazarlar, yayınlanma tarihi ve yayıncı adı", publicationDate },
                                    { "yayinlanmaTarihi", publicationDateStr }
                                 };
                            collection.InsertOne(document);
                        }
                        else
                        {
                            // Eşleşme sağlanamadıysa, hata işle
                        }
                    }
                }
                else
                {
                    results.InnerHtml += "<div class='result'>Sonuç bulunamadı veya alıntı sayısı/yazar bilgisi/yayınlanma tarihi alınamadı.</div>";
                }
            }
            else
            {
                results.InnerHtml += "<div class='result'>Lütfen bir kelime girin.</div>";
            }
        }

        private void ShowAllDocuments()
        {
            MongoConnection mongoConnection = new MongoConnection();
            IMongoDatabase database = mongoConnection.ConnectToMongoDB();
            var collection = database.GetCollection<BsonDocument>("Test");
            var documents = collection.Find(new BsonDocument()).ToList();
            results.InnerHtml = "";
            foreach (var document in documents)
            {
                string arananKelime = document["arananKelime"].AsString;
                string title = document["baslik"].AsString;
                string urlLink = document["urlLink"].AsString;

                results.InnerHtml += "<div class='result'><a href='" + urlLink + "' target='_blank'>" + title + "</a></div>";
            }
        }

        protected void showAllButton_Click(object sender, EventArgs e)
        {
            ShowAllDocuments();
        }

        protected void FilterResults(string arananKelime, string baslikFilter, int? alintiSayisiFilter, string yayinlanmaTarihiFilter, bool enSonSiralama)
        {
            MongoConnection mongoConnection = new MongoConnection();
            IMongoDatabase database = mongoConnection.ConnectToMongoDB();
            var collection = database.GetCollection<BsonDocument>("Test");

            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Eq("arananKelime", arananKelime); // Aranan kelimeye göre ilk filtreleme

            // Diğer filtrelerin eklenmesi
            if (!string.IsNullOrEmpty(baslikFilter))
            {
                filter &= builder.Regex("baslik", new BsonRegularExpression(new Regex(baslikFilter, RegexOptions.IgnoreCase)));
            }

            if (alintiSayisiFilter.HasValue)
            {
                filter &= builder.Eq("alintiSayisi", alintiSayisiFilter.Value);
            }

            if (!string.IsNullOrEmpty(yayinlanmaTarihiFilter))
            {
                filter &= builder.Regex("yayinlanmaTarihi", new BsonRegularExpression(new Regex(yayinlanmaTarihiFilter, RegexOptions.IgnoreCase)));
            }

            var sortBuilder = Builders<BsonDocument>.Sort;
            var sort = enSonSiralama ? sortBuilder.Descending("yayinlanmaTarihi") : sortBuilder.Ascending("yayinlanmaTarihi");

            var documents = collection.Find(filter).Sort(sort).ToList();

            results.InnerHtml = "";
            foreach (var document in documents)
            {
                string title = document["baslik"].AsString;
                string urlLink = document["urlLink"].AsString;

                results.InnerHtml += "<div class='result'><a href='" + urlLink + "' target='_blank'>" + title + "</a></div>";
            }
        }
        protected void sortNewestButton_Click(object sender, EventArgs e)
        {
            SortDocumentsByDate("desc");
        }

        protected void sortOldestButton_Click(object sender, EventArgs e)
        {
            SortDocumentsByDate("asc");
        }

        private void SortDocumentsByDate(string sortOrder)
        {
            MongoConnection mongoConnection = new MongoConnection();
            IMongoDatabase database = mongoConnection.ConnectToMongoDB();
            var collection = database.GetCollection<BsonDocument>("Test");

            // Sıralama parametreleri
            var sortBuilder = Builders<BsonDocument>.Sort;
            var sortDefinition = sortOrder == "desc" ? sortBuilder.Descending("yayinlanmaTarihi") : sortBuilder.Ascending("yayinlanmaTarihi");

            // Sıralama işlemi ve sonuçların alınması
            var documents = collection.Find(new BsonDocument()).Sort(sortDefinition).ToList();

            results.InnerHtml = "";
            foreach (var document in documents)
            {
                string title = document["baslik"].AsString;
                string urlLink = document["urlLink"].AsString;

                results.InnerHtml += "<div class='result'><a href='" + urlLink + "' target='_blank'>" + title + "</a></div>";
            }
        }

        protected void filterButton_Click(object sender, EventArgs e)
        {
            string arananKelime = araTextBox.Text;
            string baslikFilter = baslikTextBox.Text;
            int? alintiSayisiFilter = null;
            if (!string.IsNullOrEmpty(alintiSayisiTextBox.Text))
            {
                alintiSayisiFilter = int.Parse(alintiSayisiTextBox.Text);
            }
            string yayinlanmaTarihiFilter = yayinlanmaTarihiTextBox.Text;

            FilterResults(arananKelime, baslikFilter, alintiSayisiFilter, yayinlanmaTarihiFilter,true);
        }

        protected void temizleButton_Click(object sender, EventArgs e)
        {
            MongoConnection mongoConnection = new MongoConnection();
            mongoConnection.DeleteAllDocuments("Test");
        }
    }
}
