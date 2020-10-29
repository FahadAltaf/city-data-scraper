using CsvHelper;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp45
{
    public class DataModel
    {
        public string State { get; set; }
        public string City { get; set; }
        public string Url { get; set; }
        public int Population { get; set; }
        public string BlackAlone { get; set; }
        public string WhiteAlone { get; set; }
        public string Hispanic { get; set; }
        public string AsianAlone { get; set; }
        public string Prop1 { get; set; }
        public string Prop2 { get; set; }
        public string Prop3 { get; set; }
        public string Prop4 { get; set; }
        public string Prop5 { get; set; }
        public string Prop6 { get; set; }
        public string Prop7 { get; set; }
        public string Prop8 { get; set; }
        public string Prop9 { get; set; }
        public string Prop10 { get; set; }
        public string Prop11 { get; set; }
        public string NeverMarried { get; set; }
        public string NowMarried { get; set; }
        public string Separated { get; set; }
        public string Widowed { get; set; }
        public string Divorced { get; set; }
        public int Colleges_Universities { get; set; }
        public int Businesses { get; set; }
        public string Was1 { get; set; }
        public string Was2 { get; set; }
        public string Was3 { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            List<DataModel> entries = new List<DataModel>();
            var lines = File.ReadAllLines("file.csv");
            HtmlWeb web = new HtmlWeb();
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length == 2)
                {
                    Console.WriteLine(parts[1]);
                    var doc = web.Load(parts[1]);
                    var table = doc.DocumentNode.SelectSingleNode("//*[@id=\"cityTAB\"]/tbody");

                    if (table != null)
                        foreach (var row in table.ChildNodes.Where(x => x.Name == "tr"))
                        {
                            var sub = new HtmlDocument();
                            sub.LoadHtml(row.InnerHtml);
                            var entry = new DataModel { State = parts[0] };
                            entry.City = sub.DocumentNode.SelectSingleNode("/td[2]").InnerText;
                            entry.Url = "https://www.city-data.com/city/" + sub.DocumentNode.SelectNodes("//a").FirstOrDefault().Attributes.FirstOrDefault(x => x.Name == "href").Value;
                            entry.Population = Convert.ToInt32(sub.DocumentNode.SelectSingleNode("/td[3]").InnerText.Replace(",", ""));
                            if (!entry.Url.Contains("javascript") && entry.Population > 100000)
                            {
                                GetDetails(ref entry);
                                entries.Add(entry);

                            }
                        }
                }
            }


            using (var writer = new StreamWriter("result.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(entries);
            }

            Console.WriteLine();
        }

        private static void GetDetails(ref DataModel entry)
        {

            var web = new HtmlWeb();
            var doc = web.Load(entry.Url);

            var section = doc.DocumentNode.SelectSingleNode("//*[@id=\"races-graph\"]");
            if (section != null)
            {
                var list = section.ChildNodes[1].ChildNodes[0].ChildNodes[1].ChildNodes[0];
                if (list != null)
                {
                    foreach (var li in list.ChildNodes.Where(x => x.Name == "li"))
                    {
                        var sub = new HtmlDocument();
                        sub.LoadHtml(li.InnerHtml);
                        var name = sub.DocumentNode.SelectSingleNode("/b[1]").InnerText;
                        var per = sub.DocumentNode.SelectSingleNode("/span[2]").InnerText;

                        switch (name)
                        {
                            case "White alone":
                                entry.WhiteAlone = per;
                                break;
                            case "Black alone":
                                entry.BlackAlone = per;
                                break;
                            case "Hispanic":
                                entry.Hispanic = per;
                                break;
                            case "Asian alone":
                                entry.AsianAlone = per;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            var cityPopulation = doc.DocumentNode.SelectSingleNode("//section[@id='city-population']");
            if (cityPopulation != null)
            {
                HtmlDocument sub = new HtmlDocument();
                sub.LoadHtml(cityPopulation.InnerHtml);

                var pop2017 = sub.DocumentNode.SelectSingleNode("/b[1]");
                if (pop2017.InnerText.Contains("Population in 2017"))
                {
                    var text = sub.DocumentNode.ChildNodes[1].InnerText;
                    entry.Prop1 = text.Substring(0, text.IndexOf("(")).Replace(",", "");
                }
                else
                    Console.WriteLine("Problem");

                var pop2000 = sub.DocumentNode.SelectSingleNode("/b[2]");
                if (pop2000 != null)
                    if (pop2000.InnerText.Contains("Population change since 2000"))
                    {
                        entry.Prop2 = sub.DocumentNode.ChildNodes[4].InnerText;
                    }
                    else
                        Console.WriteLine("Problem");
            }

            var medianResidentAge = doc.DocumentNode.SelectSingleNode("//section[@id='median-age']");
            if (medianResidentAge != null)
            {
                HtmlDocument sub = new HtmlDocument();
                sub.LoadHtml(medianResidentAge.InnerHtml);

                var pop2017 = sub.DocumentNode.SelectSingleNode("/div[1]/table[1]/tr[1]/td[1]");
                if (pop2017.InnerText.Contains("Median resident age"))
                {
                    entry.Prop3 = sub.DocumentNode.SelectSingleNode("/div[1]/table[1]/tr[1]/td[2]").InnerText;
                }
                else
                    Console.WriteLine("Problem");


            }
            var medianIncome = doc.DocumentNode.SelectSingleNode("//section[@id='median-income']");
            if (medianIncome != null)
            {
                HtmlDocument sub = new HtmlDocument();
                sub.LoadHtml(medianIncome.InnerHtml);

                var table = sub.DocumentNode.InnerHtml.Split(new string[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries).Where(x => x.Contains("Estimated"));
                foreach (var item in table)
                {
                    var text = item.Split(')')[0];
                    var htmlDocxument = new HtmlDocument();
                    htmlDocxument.LoadHtml(text);

                    string str = "";
                    if (htmlDocxument.DocumentNode.ChildNodes[0].InnerText == "Estimated median household income in 2017:")
                    {
                        entry.Prop4 = htmlDocxument.DocumentNode.ChildNodes[1].InnerText.Replace("(", "");
                        if (htmlDocxument.DocumentNode.ChildNodes.Count>2)
                            entry.Was1 = htmlDocxument.DocumentNode.ChildNodes[3].InnerText;
                    }
                    else if (htmlDocxument.DocumentNode.ChildNodes[0].InnerText == "Estimated per capita income in 2017:")
                    {
                        entry.Prop5 = htmlDocxument.DocumentNode.ChildNodes[1].InnerText.Replace("(", "");
                        if (htmlDocxument.DocumentNode.ChildNodes.Count > 2)
                            entry.Was2 = htmlDocxument.DocumentNode.ChildNodes[3].InnerText;
                    }
                    else if (htmlDocxument.DocumentNode.ChildNodes[0].InnerText == "Estimated median house or condo value in 2017:")
                    {
                        entry.Prop6 = htmlDocxument.DocumentNode.ChildNodes[1].InnerText.Replace("(", "");
                        if (htmlDocxument.DocumentNode.ChildNodes.Count > 2)
                            entry.Was3 = htmlDocxument.DocumentNode.ChildNodes[3].InnerText;
                    }

                }
            }

            var povertyLevel = doc.DocumentNode.SelectSingleNode("//section[@id='poverty-level']");
            if (povertyLevel != null)
            {
                HtmlDocument sub = new HtmlDocument();
                sub.LoadHtml(povertyLevel.InnerHtml);

                var text = sub.DocumentNode.ChildNodes[0].InnerText;
                if (text == "Percentage of residents living in poverty in 2017:")
                {
                    entry.Prop7 = sub.DocumentNode.ChildNodes[1].InnerText;
                }
            }

            var crime = doc.DocumentNode.SelectSingleNode("//section[@id='crime']");
            if (crime != null)
            {
                HtmlDocument sub = new HtmlDocument();
                sub.LoadHtml(crime.InnerHtml);
                var count = sub.DocumentNode.SelectSingleNode("/div[1]/table[1]/tbody[1]/tfoot[1]").ChildNodes[0].ChildNodes.Where(x => x.Name == "td").Count();
                entry.Prop8 = sub.DocumentNode.SelectSingleNode($"/div[1]/table[1]/tbody[1]/tfoot[1]/tr[1]/td[{count - 3}]").InnerText;
                entry.Prop9 = sub.DocumentNode.SelectSingleNode($"/div[1]/table[1]/tbody[1]/tfoot[1]/tr[1]/td[{count - 2}]").InnerText;
                entry.Prop10 = sub.DocumentNode.SelectSingleNode($"/div[1]/table[1]/tbody[1]/tfoot[1]/tr[1]/td[{count - 1}]").InnerText;
                entry.Prop11 = sub.DocumentNode.SelectSingleNode($"/div[1]/table[1]/tbody[1]/tfoot[1]/tr[1]/td[{count}]").InnerText;
            }



            var marital = doc.DocumentNode.SelectSingleNode("//section[@id='marital-info']");
            if (marital != null)
            {
                HtmlDocument sub = new HtmlDocument();
                sub.LoadHtml(marital.InnerHtml);
                var list = sub.DocumentNode.SelectSingleNode("/ul[1]");
                if (list != null)
                    foreach (var item in list.ChildNodes.Where(x => x.Name == "li"))
                    {
                        var text = item.ChildNodes[0].InnerText;
                        switch (text)
                        {
                            case "Never married:":
                                entry.NeverMarried = item.ChildNodes[1].InnerText;
                                break;
                            case "Now married:":
                                entry.NowMarried = item.ChildNodes[1].InnerText;
                                break;
                            case "Separated:":
                                entry.Separated = item.ChildNodes[1].InnerText;
                                break;
                            case "Widowed:":
                                entry.Widowed = item.ChildNodes[1].InnerText;
                                break;
                            case "Divorced:":
                                entry.Divorced = item.ChildNodes[1].InnerText;
                                break;
                            default:
                                break;
                        }
                    }
            }

            var schools = doc.DocumentNode.SelectSingleNode("//section[@id='schools']");
            if (schools != null)
            {
                HtmlDocument sub = new HtmlDocument();
                sub.LoadHtml(schools.InnerHtml);
                var list = sub.DocumentNode.SelectSingleNode("/div[1]/ul[1]");
                if (list != null)
                {
                    entry.Colleges_Universities = list.ChildNodes.Where(x => x.Name == "li").Count();
                }
            }

            var business = doc.DocumentNode.SelectSingleNode("//section[@id='businesses-count-table']");
            if (business != null)
            {
                HtmlDocument sub = new HtmlDocument();
                sub.LoadHtml(business.InnerHtml);
                var list = sub.DocumentNode.SelectSingleNode("/table[1]/tbody[1]");
                if (list != null)
                    foreach (var row in list.ChildNodes.Where(x => x.Name == "tr"))
                    {
                        entry.Businesses += string.IsNullOrEmpty(row.ChildNodes[1].InnerText) ? 0 : Convert.ToInt32(row.ChildNodes[1].InnerText);
                        entry.Businesses += string.IsNullOrEmpty(row.ChildNodes[4].InnerText) ? 0 : Convert.ToInt32(row.ChildNodes[4].InnerText);
                    }
            }


        }
    }
}
