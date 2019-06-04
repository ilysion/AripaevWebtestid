using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.WebTesting;
using Newtonsoft.Json;

namespace ApApiTestPlugin
{
    public class ApApiTestPlugin : WebTestPlugin
    {
        public override void PreWebTest(object sender,  PreWebTestEventArgs e)
        {
            e.WebTest.AddCommentToResult("Äripäeva API webtest plugin kontrollib apide artiklite avaldamis aegu.");

            if (!CheckAripaevMostViewed())
            {
                e.WebTest.AddCommentToResult("FAILED: Äripäeva enimvaadatud artiklites ei ole ühtegi artiklit, mis oleks uuem kui 2 päeva (Või ei saadud API-ga ühendust).");
                e.WebTest.InternalSetOutcome(Outcome.Fail);
            }
            else
            {
                e.WebTest.AddCommentToResult("SUCCESS: Äripäevas leidub uuemaid artikleid, kui 2 päeva.");
            }


            if (!CheckDvMostViewed())
            {
                e.WebTest.AddCommentToResult("FAILED:DV enimvaadatud artiklites ei ole ühtegi artiklit, mis oleks uuem kui 3 päeva (Või ei saadud API-ga ühendust).");
                e.WebTest.InternalSetOutcome(Outcome.Fail);
            }
            else
            {
                e.WebTest.AddCommentToResult("SUCCESS: DV-s leidub uuemaid artikleid, kui 3 päeva");
            }
            

            if (!CheckRaamatupidaja35Fresh())
            {
                e.WebTest.AddCommentToResult("FAILED:Raamatupidaja artiklites ei ole ühtegi artiklit, mis oleks uuem kui 7 päeva (Või ei saadud API-ga ühendust).");
                e.WebTest.InternalSetOutcome(Outcome.Fail);
            }
            else
            {
                e.WebTest.AddCommentToResult("SUCCESS: Raamatupidaja artiklite seas leidub uuemaid kui 7 päeva.");
            }

           
            if (!CheckEditorsChoice())
            {
                e.WebTest.AddCommentToResult("FAILED:Raamatupidaja editors choice artiklites ei ole ühtegi artiklit, mis oleks uuem kui 7 päeva (Või ei saadud API-ga ühendust).");
                e.WebTest.InternalSetOutcome(Outcome.Fail);
            }
            else
            {
                e.WebTest.AddCommentToResult("SUCCESS: Editors choice artiklites leidub artikleid, mis oleks uuemad kui 7 päeva.");
            }
            
            base.PreWebTest(sender, e);
        }

        public bool CheckAripaevMostViewed()
        {
            // Custom checks
            //Check aripaev most viewed
            var mostViewedUrl = new Uri("https://listapi.aripaev.ee/v1/most-viewed?period=1_day");
            HttpWebRequest getRequest = (HttpWebRequest)WebRequest.Create(mostViewedUrl);
            getRequest.Method = WebRequestMethods.Http.Get;
            getRequest.ContentType = "application/json";
            getRequest.Headers.Add("X-Channel-Id", "aripaev");
            using (HttpWebResponse getResponse = (HttpWebResponse)getRequest.GetResponse())
            using (StreamReader streamReader = new StreamReader(getResponse.GetResponseStream(), Encoding.UTF8))
            {
                string responseString = streamReader.ReadToEnd();
                List<Article> articles = JsonConvert.DeserializeObject<List<Article>>(responseString);
                bool containsNewArticle = false;

                for (int i = 0; i < 8; i++)
                {
                    Article article = articles[i];
                    if (DateTime.Parse(article.Date).AddDays(2) > DateTime.Now)
                    {
                        containsNewArticle = true;
                        break;
                    }
                }

                if (!containsNewArticle)
                {
                    //Theres no articles newer than 2 days
                    return false;
                }

            }
            return true;
        }


        public bool CheckDvMostViewed()
        {
            //Check DV most viewed
            var mostViewedDVUrl = new Uri("https://listapi.aripaev.ee/v1/most-viewed?period=1_week");
            HttpWebRequest getDVRequest = (HttpWebRequest)WebRequest.Create(mostViewedDVUrl);
            getDVRequest.Method = WebRequestMethods.Http.Get;
            getDVRequest.ContentType = "application/json";
            getDVRequest.Headers.Add("X-Channel-Id", "dv");
            using (HttpWebResponse getDVResponse = (HttpWebResponse)getDVRequest.GetResponse())
            using (StreamReader streamReader = new StreamReader(getDVResponse.GetResponseStream(), Encoding.UTF8))
            {
                string responseString = streamReader.ReadToEnd();
                List<Article> articlesDV = JsonConvert.DeserializeObject<List<Article>>(responseString);
                bool containsNewArticle = false;

                for (int i = 0; i < 8; i++)
                {
                    Article article = articlesDV[i];
                    if (DateTime.Parse(article.Date).AddDays(3) > DateTime.Now)
                    {
                        containsNewArticle = true;
                        break;
                    }
                }

                if (!containsNewArticle)
                {
                    //Theres no articles newer than 3 days
                    return false;
                }
            }
            return true;
        }

        public bool CheckRaamatupidaja35Fresh()
        {
            //Check raamatupidaja 35 fresh articles
            var raamatupidajaUrl = new Uri("https://listapi.aripaev.ee/v1/category?limit=35");
            HttpWebRequest raamatupidajaRequest = (HttpWebRequest)WebRequest.Create(raamatupidajaUrl);
            raamatupidajaRequest.Method = WebRequestMethods.Http.Get;
            raamatupidajaRequest.ContentType = "application/json";
            raamatupidajaRequest.Headers.Add("X-Channel-Id", "raamatupidaja");
            using (HttpWebResponse raamatupidajaResponse = (HttpWebResponse)raamatupidajaRequest.GetResponse())
            using (StreamReader streamReader = new StreamReader(raamatupidajaResponse.GetResponseStream(), Encoding.UTF8))
            {
                string responseString = streamReader.ReadToEnd();
                ListArticles articlesRaamatupidaja = JsonConvert.DeserializeObject<ListArticles>(responseString);

                bool containsNewArticle = false;
                foreach (Article article in articlesRaamatupidaja.articles)
                {
                    if (DateTime.Parse(article.Date).AddDays(7) > DateTime.Now)
                    {
                        containsNewArticle = true;
                        break;
                    }
                }

                if (!containsNewArticle)
                {
                    //Theres no articles newer than 7 days
                    return false;
                }
            }
            return true;
        }

        public bool CheckEditorsChoice()
        {
            //Check Editors choice 4 fresh articles
            var editorsChoiceUrl = new Uri("https://listapi.aripaev.ee/v1/lists/editorschoice");
            HttpWebRequest editorsChoiceRequest = (HttpWebRequest)WebRequest.Create(editorsChoiceUrl);
            editorsChoiceRequest.Method = WebRequestMethods.Http.Get;
            editorsChoiceRequest.ContentType = "application/json";
            editorsChoiceRequest.Headers.Add("X-Channel-Id", "raamatupidaja");
            using (HttpWebResponse editorsChoiceResponse = (HttpWebResponse)editorsChoiceRequest.GetResponse())
            using (StreamReader streamReader = new StreamReader(editorsChoiceResponse.GetResponseStream(), Encoding.UTF8))
            {
                string responseString = streamReader.ReadToEnd();
                EditorsChoiseHeadlist editorsChoiceArticles = JsonConvert.DeserializeObject<EditorsChoiseHeadlist>(responseString);

                bool containsNewArticle = false;
                foreach (Article article in editorsChoiceArticles.lists[0].articles)
                {
                    if (DateTime.Parse(article.Date).AddDays(7) > DateTime.Now)
                    {
                        containsNewArticle = true;
                        break;
                    }
                }

                if (!containsNewArticle)
                {
                    //Theres no articles newer than 7 days
                    return false;
                }
            }
            return true;
        }
    }

    public class Article
    {
        public long Id { get; set; }
        public string Uuid { get; set; }
        public string Link { get; set; }
        public string Date { get; set; }
        public string Headline { get; set; }
    }

    public class ListArticles
    {
        public List<Article> articles { get; set; }

    }

    public class EditorsChoiceArticles
    {
        public List<Article> articles { get; set; }

    }

    public class EditorsChoiseHeadlist
    {
        public List<EditorsChoiceArticles> lists { get; set; }
    }

}