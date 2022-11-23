using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.Collections.Generic;
using DSharpPlus.Entities;

namespace JPDB_Bot.Commands
{
    public class amazonJapan : BaseCommandModule
    {
        [Command("ajp")]
        [Aliases("amazonjapan", "amazonjp")]
        [Description("Get information about a book on amazon")]
        [Cooldown(2, 10, CooldownBucketType.User)]
        [Hidden]
        public async Task ajp(CommandContext ctx, [RemainingText] string userInput = "")
        {
            try
            {
                await ctx.Message.ModifyEmbedSuppressionAsync(true);
            } catch
            {

            }
            string bookLink = "";
            bool isLink = false;
            if (userInput.Length > 30)
            {
                if (userInput.Substring(0, 25) == "https://www.amazon.co.jp/" & !userInput.Contains(" "))
                {
                    bookLink = userInput;
                    isLink = true;
                }
            }

            if (isLink == false)
            {
                await ctx.RespondAsync("Currently, only links are supported.").ConfigureAwait(false);
                return;
            }

            List<string> reviews = scanReviews(bookLink.Replace("/dp/", "/product-reviews/"));

            string bookHTML = loadBookPage(bookLink);

            
            try
            {
                string title = getTitle(bookHTML);
                string imageURL = getBookImage(bookHTML);
                List<string> keywords = getKeywords(reviews);
                sendEmbed(ctx, title, imageURL, keywords, bookLink);
            } catch (Exception ex)
            {
                Program.printError(ex.Message);
                await ctx.RespondAsync("Something went wrong").ConfigureAwait(false);
                return;
            }
        }

        static async void sendEmbed(CommandContext ctx, string contentName, string imageURL, List<string> keywords, string url = "")
        {
            string kwString = String.Join("`, ", keywords) + "`";
            if (contentName.Length > 256) { contentName = "Novel infomation"; }

            var infoEmbed = new DiscordEmbedBuilder()
            {
                Title = contentName,
                Description = "Keywords: " + kwString,
                Color = DiscordColor.Blurple
                //Thumbnail = embedThumbnail,
            };
            if (url != "") { infoEmbed.Url = url; }
            if (imageURL != null) { infoEmbed.ImageUrl = imageURL; }

            try
            {
                var contentEmbedMessage = await ctx.Channel.SendMessageAsync(embed: infoEmbed).ConfigureAwait(false);
            }
            catch
            {
                Program.printError("Failed to send ajp info embed.");
                return;
            }

        }

        #region load book page
        static string loadBookPage(string bookLink)
        {
            WebClient client = new WebClient();
            string html = client.DownloadString(bookLink);

            return html;
        }

        static string getTitle(string html)
        {
            html = html.Substring(html.IndexOf("id=\"productTitle\""));
            html = html.Substring(html.IndexOf(">") + 2);
            html = html.Substring(0, html.IndexOf("<")).Trim();

            return html;
        }

        static string getBookImage(string html)
        {
            try
            {
                html = html.Substring(html.IndexOf("id=\"imageBlockCommon\""));
                //html = html.Substring(html.IndexOf("data-a-hires=\"https://images-fe.ssl"));
            }
            catch { return null; }

            string imageURL = "";
            try
            {
                imageURL = "https://m.media-" + html.Substring(html.IndexOf("amazon.com/images/I/"), 31) + ".jpg";
            }
            catch { return null; }

            return imageURL;
        }
        #endregion

        static List<string> getKeywords(List<string> reviews)
        {
            string[] keywords = new string[] { "VR", "強敵", "貴族", "ロリ", "告白", "甘々", "元カノ", "甘い話", "百合", "恋敵", "ドキドキ", "同居", "バカップル", "青春", "黒歴史", "捻くれ者", "面白い", "ラブラブ", "イチャイチャ", "同棲" , "社交が苦手", "ホッコリ", "ラブコメ", "日常生活", "平坦", "婚約", "結婚", "新婚", "一人暮らし", "惚気", "幼馴染", "可愛いイラスト", "世話", "学校生活", "面倒を見ている", "大学", "コミュ障", "デレ", "鈍感系", "勘違い系", "ぼっち系", "若干キャラ", "中二病", "ハーレム", "高二病", "魅力がうまく", "コミュ症", "夫婦", "ハピエン", "癒し", "糖度", "恋人契約", "体育祭", "年齢差", "イチャラブ", "おもろすぎ", "面白すぎ", "残念な性格", "恋愛" };
            List<string> appearances = new List<string>();
            foreach (string review in reviews)
            {
                int i = -1;
                foreach (string keyword in keywords)
                {
                    i++;
                    if (keyword == "") { continue; }
                    if (review.Contains(keyword))
                    {
                        appearances.Add("`" + keyword);
                        keywords[i] = "";
                    }
                }
            }

            return appearances;
        }

        static List<string> scanReviews(string bookLink)
        {
            WebClient client = new WebClient();
            string html = client.DownloadString(bookLink);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var reviews = doc.DocumentNode.SelectNodes("//*[@id=\"cm_cr-review_list\"]");

            List<string> reviewList = new List<string>();
            foreach(var review in reviews[0].ChildNodes)
            {
                if (review.Attributes[0].Value.Contains("header") || review.Attributes[0].Value.Contains("form")) { continue; }

                string reviewText = "";
                var fields = review.ChildNodes[0].ChildNodes[0];
                foreach(var field in fields.ChildNodes)
                {
                    var fieldSelect = field.Attributes.Select(a => a.Value.Contains("review-data"));
                    if (fieldSelect.Count() == 1)
                    {
                        if (fieldSelect.ToArray()[0] == true)
                        {
                            if (field == null)
                            { continue; }

                            List<bool> field0Select = new List<bool>();
                            try
                            {
                                field0Select = field.ChildNodes[0].Attributes.Select(a => a.Value == "review-body").ToList();
                            }
                            catch { }

                            if (field0Select.Count > 0)
                            {
                                if (field0Select[0] == true)
                                {
                                    reviewList.Add(field.InnerText.Trim());
                                }
                            }
                        }
                    } 
                }

                reviewText = fields.ChildNodes[5].InnerText.Trim();
            }

            return reviewList;
        }



    }
}
