using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace BlogML2Wp
{
    public class Program
    {
        static string inputFile = "BlogML.xml";
        static string outputFile = "BlogML.Output.xml";
        static string domainPrefix = "http://rtur.net";
        static int _postCnt = 0;
        static int _postErr = 0;
        static int _commCnt = 0;
        static int _commErr = 0;

        public static void Main(string[] args)
        {
            if (args.Length == 1 && (
                args[0].ToLower() == "help" || 
                args[0].ToLower() == "?" ||
                args[0].ToLower() == "-h" ||
                args[0].ToLower() == "-help" || 
                args[0].ToLower() == "--help"))
            {
                DisplayHelp();
                Console.Read();
                return;
            }

            if (args != null && args.Count() > 0)
            {
                inputFile = args[0];

                if (args.Count() > 1)
                {
                    domainPrefix = args[1];
                }
            }

            if (!File.Exists(inputFile))
            {
                Console.WriteLine("File " + inputFile + " does not exist");
                Console.Read();
                return;
            }

            var doc = new XDocument();
            doc = XDocument.Load(inputFile);

            StartChannel();

            foreach (var item in doc.Root.Descendants().First(i => i.Name.LocalName == "posts").Elements().Where(i => i.Name.LocalName == "post"))
            {
                var published = item.Attribute("is-published").Value;
                if (published.ToLower() == "true")
                {
                    WritePost(item);
                }
            }

            EndChannel();

            Console.WriteLine(string.Format("Converted {0} posts with {1} comments", _postCnt, _commCnt));
            Console.WriteLine(string.Format("Errors in {0} posts and {1} comments", _postErr, _commErr));

            Console.Read();
        }

        static void WritePost(XElement item)
        {
            try
            {
                //var postid = item.Attribute("id").Value;
                var title = item.Elements().First(i => i.Name.LocalName == "title").Value;
                var link = item.Attribute("post-url").Value;
                var slug = GetSlug(link);
                var hasexcerpt = item.Attribute("hasexcerpt").Value;
                var content = item.Elements().First(i => i.Name.LocalName == "content").Value;
                if (hasexcerpt == "true")
                {
                    content = item.Elements().First(i => i.Name.LocalName == "excerpt").Value;
                }
                var created = item.Attribute("date-created").Value;

                Write("    <item>");
                Write("      <title>" + title + "</title>");
                Write("      <link>" + domainPrefix + link + "</link>");
                Write("      <content:encoded><![CDATA[" + content + "]]></content:encoded>");
                Write("      <dsq:thread_identifier>" + slug + "</dsq:thread_identifier>");
                Write("      <wp:post_date_gmt>" + created.Replace("T", " ") + "</wp:post_date_gmt>");
                Write("      <wp:comment_status>closed</wp:comment_status>");

                try
                {
                    var comments = item.Elements().First(i => i.Name.LocalName == "comments");
                    foreach (var comment in comments.Elements().Where(i => i.Name.LocalName == "comment"))
                    {
                        WriteComment(comment);
                    }
                }
                catch { }

                Write("    </item>");
                _postCnt++;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex.Message);
                _postErr++;
            }
        }

        static void WriteComment(XElement comment)
        {
            try
            {
                var id = comment.Attribute("id").Value;
                var parent = comment.Attribute("parentid").Value;
                var email = comment.Attribute("user-email").Value;
                var ip = comment.Attribute("user-ip").Value;
                var url = comment.Attribute("user-url").Value;
                var created = comment.Attribute("date-created").Value;
                var author = comment.Attribute("user-name").Value;
                var content = comment.Elements().First(i => i.Name.LocalName == "content").Value;
                var approved = comment.Attribute("approved").Value;

                if (parent.StartsWith("00000000"))
                    parent = "0";

                approved = approved == "true" ? "1" : "0";

                Write("      <wp:comment>");
                Write("        <wp:comment_id>" + id + "</wp:comment_id>");
                Write("        <wp:comment_author>" + author + "</wp:comment_author>");
                Write("        <wp:comment_author_email>" + email + "</wp:comment_author_email>");
                Write("        <wp:comment_author_url>" + url + "</wp:comment_author_url>");
                Write("        <wp:comment_author_IP>" + ip + "</wp:comment_author_IP>");
                Write("        <wp:comment_date_gmt>" + created.Replace("T", " ") + "</wp:comment_date_gmt>");
                Write("        <wp:comment_content><![CDATA[" + content + "]]></wp:comment_content>");
                Write("        <wp:comment_approved>" + approved + "</wp:comment_approved>");
                Write("        <wp:comment_parent>" + parent + "</wp:comment_parent>");
                Write("      </wp:comment>");
                _commCnt++;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex.Message);
                _commErr++;
            }
        }

        static void Write(string str)
        {
            File.AppendAllText(outputFile, str + Environment.NewLine);
        }

        static void StartChannel()
        {
            Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            Write("<rss version=\"2.0\"");
            Write("  xmlns:content=\"http://purl.org/rss/1.0/modules/content/\"");
            Write("  xmlns:dsq=\"http://www.disqus.com/\"");
            Write("  xmlns:dc=\"http://purl.org/dc/elements/1.1/\"");
            Write("  xmlns:wp=\"http://wordpress.org/export/1.0/\"");
            Write(">");
            Write("  <channel>");
        }

        static void EndChannel()
        {
            Write("  </channel>");
            Write("</rss>");
        }

        static void DisplayHelp()
        {
            Console.WriteLine("Converts BlogML into Disqus export file");
            Console.WriteLine("Usage:");
            Console.WriteLine("BlogML2Wp <inputfile> <domain>");
            Console.WriteLine("Example:");
            Console.WriteLine("BlogML2Wp BlogML.xml http://rtur.net");
        }

        static string GetSlug(string url)
        {
            var slug = url;
            try
            {
                var idx = slug.LastIndexOf("/");
                if(idx > 0)
                {
                    slug = slug.Substring(idx + 1);
                }
            }
            catch { }

            return slug;
        }
    }
}
