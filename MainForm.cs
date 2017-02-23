using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Text.RegularExpressions;

namespace spellchk
{
    public partial class MainForm : Form
    {
        ClipboardMonitor clipboardMonitor = null;
        Regex htmlPattern = new Regex("\\\"html\\\":\\\"(.*?)\\\"", RegexOptions.Compiled);
        Regex resultPattern = new Regex("\\\"(.*?)\\\"", RegexOptions.Compiled);
        Regex purplePattern = new Regex("<span class='re_purple'>(.*?)</span>", RegexOptions.Compiled);
        Regex greenPattern = new Regex("<span class='re_green'>(.*?)</span>", RegexOptions.Compiled);
        Regex redPattern = new Regex("<span class='re_red'>(.*?)</span>", RegexOptions.Compiled);
        Regex brTagPattern = new Regex("<br>", RegexOptions.Compiled);
        Regex tagPattern = new Regex("<.*?>", RegexOptions.Compiled);
        Regex specialChPattern = new Regex("(&quot;)|(&#34;)|(&amp;)|(&#38;)|(&apos;)|(&#39;)|(&lt;)|(&#60;)|(&gt;)|(&#62;)", RegexOptions.Compiled);

        public MainForm()
        {
            InitializeComponent();

            clipboardMonitor = new ClipboardMonitor(OnClipboardChanged);
        }

        private void OnClipboardChanged(object sender, ClipboardChangedEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.UnicodeText))
            {
                var inputText = e.DataObject.GetData(DataFormats.UnicodeText) as string;

                if (inputText.Length > 500)
                {
                    return;
                }

                setFormatting(checkSpelling(inputText));
            }
        }

        private void replaceHtmlChar(RichTextBox rtb)
        {
            string text = string.Empty;
            while (true)
            {
                text = rtb_output.Text;
                Match match = specialChPattern.Match(text);
                if (!match.Success)
                {
                    break;
                }
                rtb_output.Select(match.Index, match.Length);
                if (match.Value.Equals("&quot;") || match.Value.Equals("&#34;"))
                {
                    rtb_output.SelectedText = "\"";
                }
                else if (match.Value.Equals("&amp;") || match.Value.Equals("&#38;"))
                {
                    rtb_output.SelectedText = "&";
                }
                else if (match.Value.Equals("&apos;") || match.Value.Equals("&#39;"))
                {
                    rtb_output.SelectedText = "'";
                }
                else if (match.Value.Equals("&lt;") || match.Value.Equals("&#60;"))
                {
                    rtb_output.SelectedText = "<";
                }
                else if (match.Value.Equals("&gt;") || match.Value.Equals("&#62;"))
                {
                    rtb_output.SelectedText = ">";
                }
            }
        }

        private void setFormatting(string str)
        {
            rtb_output.Clear();
            str = brTagPattern.Replace(str, "\n");
            rtb_output.Text = str;

            MatchCollection purpleMatches = purplePattern.Matches(str);
            if (purpleMatches.Count > 0)
            {
                foreach (Match match in purpleMatches)
                {
                    rtb_output.Select(match.Index, match.Length);
                    rtb_output.SelectionColor = Color.Purple;
                }
            }
            MatchCollection greenMatches = greenPattern.Matches(str);
            if (greenMatches.Count > 0)
            {
                foreach (Match match in greenMatches)
                {
                    rtb_output.Select(match.Index, match.Length);
                    rtb_output.SelectionColor = Color.Green;
                }
            }
            MatchCollection redMatches = redPattern.Matches(str);
            if (redMatches.Count > 0)
            {
                foreach (Match match in redMatches)
                {
                    rtb_output.Select(match.Index, match.Length);
                    rtb_output.SelectionColor = Color.Red;
                }
            }

            while(true)
            {
                string text = rtb_output.Text;
                Match tagMatch = tagPattern.Match(text);
                if(!tagMatch.Success)
                {
                    break;
                }
                rtb_output.Select(tagMatch.Index, tagMatch.Length);
                rtb_output.SelectedText = "";
            }

            replaceHtmlChar(rtb_output);
            rtb_output.SelectionStart = 0;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            clipboardMonitor.Dispose();
        }

        private string checkSpelling(string text)
        {
            string result = string.Empty;
            try
            {
                string url = "https://m.search.naver.com/p/csearch/dcontent/spellchecker.nhn?_callback=callback&q=" + HttpUtility.UrlEncode(text);

                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = @".\curl\curl.exe";
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                start.WindowStyle = ProcessWindowStyle.Hidden;
                start.CreateNoWindow = true;
                start.Arguments = url;

                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = new StreamReader(process.StandardOutput.BaseStream, Encoding.UTF8))
                    {
                        Match htmlMat = htmlPattern.Match(reader.ReadToEnd());
                        result = resultPattern.Match(htmlMat.Value.Replace("\"html\"", String.Empty)).Value.Replace("\"", String.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
        }
    }
}
