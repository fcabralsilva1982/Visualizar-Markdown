using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DM8MarkdownViewer
{
    static class Program
    {
        public const string AppVersion = "1.0";

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ViewerForm(args.Length > 0 ? args[0] : null));
        }
    }

    public class ViewerForm : Form
    {
        private readonly WebBrowser browser = new WebBrowser();
        private readonly Label title = new Label();
        private readonly Button openButton = new Button();
        private readonly Button reloadButton = new Button();
        private readonly Button themeButton = new Button();
        private readonly Button codeButton = new Button();
        private readonly Button saveButton = new Button();
        private readonly Button pdfButton = new Button();
        private string currentFile;
        private string currentMarkdown = "";
        private bool darkTheme = false;
        private bool codeMode = false;

        public ViewerForm(string initialFile)
        {
            Text = "Markdown Viewer " + Program.AppVersion;
            try
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
                // The app can run without a window icon if Windows cannot extract it.
            }
            Width = 1180;
            Height = 820;
            MinimumSize = new Size(720, 520);
            StartPosition = FormStartPosition.CenterScreen;

            var toolbar = new Panel();
            toolbar.Dock = DockStyle.Top;
            toolbar.Height = 44;
            toolbar.BackColor = Color.FromArgb(243, 243, 243);

            openButton.Text = "Abrir .md";
            openButton.Width = 92;
            openButton.Height = 28;
            openButton.Left = 12;
            openButton.Top = 8;
            openButton.Click += (s, e) => OpenMarkdownFile();

            reloadButton.Text = "Recarregar";
            reloadButton.Width = 92;
            reloadButton.Height = 28;
            reloadButton.Left = 112;
            reloadButton.Top = 8;
            reloadButton.Enabled = false;
            reloadButton.Click += (s, e) => LoadFile(currentFile);

            themeButton.Text = "Tema escuro";
            themeButton.Width = 102;
            themeButton.Height = 28;
            themeButton.Left = 212;
            themeButton.Top = 8;
            themeButton.Click += (s, e) =>
            {
                darkTheme = !darkTheme;
                themeButton.Text = darkTheme ? "Tema claro" : "Tema escuro";
                if (!String.IsNullOrEmpty(currentFile)) LoadFile(currentFile);
                else browser.DocumentText = WelcomeHtml();
            };

            codeButton.Text = "Modo codigo";
            codeButton.Width = 102;
            codeButton.Height = 28;
            codeButton.Left = 322;
            codeButton.Top = 8;
            codeButton.Enabled = false;
            codeButton.Click += (s, e) =>
            {
                codeMode = !codeMode;
                codeButton.Text = codeMode ? "Modo preview" : "Modo codigo";
                saveButton.Enabled = codeMode && !String.IsNullOrEmpty(currentFile);
                RenderCurrent();
            };

            saveButton.Text = "Salvar";
            saveButton.Width = 72;
            saveButton.Height = 28;
            saveButton.Left = 432;
            saveButton.Top = 8;
            saveButton.Enabled = false;
            saveButton.Click += (s, e) => SaveCurrentMarkdown();

            pdfButton.Text = "Exportar PDF";
            pdfButton.Width = 102;
            pdfButton.Height = 28;
            pdfButton.Left = 512;
            pdfButton.Top = 8;
            pdfButton.Enabled = false;
            pdfButton.Click += (s, e) => ExportCurrentViewToPdf();

            title.AutoSize = false;
            title.Left = 630;
            title.Top = 12;
            title.Height = 20;
            title.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            title.Width = toolbar.Width - 650;
            title.Text = "Nenhum arquivo aberto";
            title.ForeColor = Color.FromArgb(80, 80, 80);

            toolbar.Resize += (s, e) => title.Width = toolbar.Width - 650;
            toolbar.Controls.Add(openButton);
            toolbar.Controls.Add(reloadButton);
            toolbar.Controls.Add(themeButton);
            toolbar.Controls.Add(codeButton);
            toolbar.Controls.Add(saveButton);
            toolbar.Controls.Add(pdfButton);
            toolbar.Controls.Add(title);

            browser.Dock = DockStyle.Fill;
            browser.AllowWebBrowserDrop = false;
            browser.IsWebBrowserContextMenuEnabled = true;
            browser.WebBrowserShortcutsEnabled = true;

            Controls.Add(browser);
            Controls.Add(toolbar);

            if (!String.IsNullOrEmpty(initialFile) && File.Exists(initialFile))
                LoadFile(initialFile);
            else
                browser.DocumentText = WelcomeHtml();
        }

        private void OpenMarkdownFile()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Abrir arquivo Markdown";
                dialog.Filter = "Markdown (*.md;*.markdown;*.mdown)|*.md;*.markdown;*.mdown|Todos os arquivos (*.*)|*.*";
                dialog.CheckFileExists = true;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    LoadFile(dialog.FileName);
            }
        }

        private void LoadFile(string path)
        {
            if (String.IsNullOrEmpty(path) || !File.Exists(path)) return;
            currentFile = path;
            reloadButton.Enabled = true;
            codeButton.Enabled = true;
            pdfButton.Enabled = true;
            title.Text = path;
            Text = Path.GetFileName(path) + " - Markdown Viewer " + Program.AppVersion;
            currentMarkdown = File.ReadAllText(path, Encoding.UTF8);
            saveButton.Enabled = codeMode;
            RenderCurrent();
        }

        private void RenderCurrent()
        {
            if (String.IsNullOrEmpty(currentFile)) return;
            if (codeMode)
                browser.DocumentText = BuildHtml("<pre id=\"markdown-editor\" class=\"source-code\" contenteditable=\"true\">" + WebUtility.HtmlEncode(currentMarkdown) + "</pre>", Path.GetDirectoryName(currentFile));
            else
                browser.DocumentText = BuildHtml(Markdown.ToHtml(currentMarkdown), Path.GetDirectoryName(currentFile));
        }

        private void SaveCurrentMarkdown()
        {
            if (String.IsNullOrEmpty(currentFile) || !codeMode || browser.Document == null) return;
            var editor = browser.Document.GetElementById("markdown-editor");
            if (editor == null) return;
            currentMarkdown = editor.InnerText ?? "";
            File.WriteAllText(currentFile, currentMarkdown, new UTF8Encoding(false));
            title.Text = currentFile + " - salvo";
        }

        private void ExportCurrentViewToPdf()
        {
            if (browser.Document == null) return;
            if (codeMode)
            {
                var editor = browser.Document.GetElementById("markdown-editor");
                if (editor != null) currentMarkdown = editor.InnerText ?? "";
            }
            browser.ShowPrintDialog();
        }

        private string WelcomeHtml()
        {
            return BuildHtml("<h1>Markdown Viewer " + Program.AppVersion + "</h1><p>Abra um arquivo <code>.md</code> para visualizar com um estilo inspirado no preview do VS Code.</p><p>Tambem e possivel arrastar o arquivo para cima do executavel ou usar <code>Abrir com...</code> no Windows.</p><p>Para exportar em PDF, use <code>Exportar PDF</code> e escolha <code>Microsoft Print to PDF</code>.</p>", null);
        }

        private string BuildHtml(string body, string basePath)
        {
            var bg = darkTheme ? "#1e1e1e" : "#f3f3f3";
            var pageBg = darkTheme ? "#252526" : "#ffffff";
            var fg = darkTheme ? "#d4d4d4" : "#24292f";
            var muted = darkTheme ? "#858585" : "#57606a";
            var border = darkTheme ? "#3c3c3c" : "#d0d7de";
            var codeBg = darkTheme ? "#252526" : "#f6f8fa";
            var link = darkTheme ? "#4ea3ff" : "#0969da";
            var baseTag = String.IsNullOrEmpty(basePath) ? "" : "<base href=\"" + WebUtility.HtmlEncode(new Uri(basePath + Path.DirectorySeparatorChar).AbsoluteUri) + "\">";

            return "<!doctype html><html><head><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"><meta charset=\"utf-8\">" + baseTag +
                "<style>" +
                "html,body{margin:0;padding:0;background:" + bg + ";color:" + fg + ";}" +
                "body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Arial,sans-serif;font-size:14px;line-height:1.6;}" +
                ".markdown-body{width:624px;min-height:850px;margin:32px auto 56px;padding:96px;background:" + pageBg + ";border:1px solid " + border + ";}" +
                "h1,h2,h3,h4,h5,h6{font-weight:600;line-height:1.25;margin:24px 0 16px;color:" + fg + ";}" +
                "h1{font-size:2em;padding-bottom:.3em;border-bottom:1px solid " + border + ";}h2{font-size:1.5em;padding-bottom:.3em;border-bottom:1px solid " + border + ";}h3{font-size:1.25em;}" +
                "p,ul,ol,pre,table,blockquote{margin-top:0;margin-bottom:16px;}a{color:" + link + ";text-decoration:none;}a:hover{text-decoration:underline;}" +
                "code{font-family:Consolas,'Courier New',monospace;background:" + codeBg + ";border-radius:4px;padding:.18em .35em;font-size:85%;}" +
                "pre{background:" + codeBg + ";border:1px solid " + border + ";border-radius:6px;overflow:auto;padding:16px;}pre code{padding:0;background:transparent;border-radius:0;font-size:13px;}" +
                ".tok-key{color:#0000ff;font-weight:600;}.tok-str{color:#a31515;}.tok-com{color:#008000;}.tok-num{color:#098658;}.tok-tag{color:#800000;}.tok-attr{color:#ff0000;}.tok-var{color:#795e26;font-weight:600;}.tok-op{color:#666666;}.tok-punc{color:#666666;}" +
                ".lang-label{float:right;color:" + muted + ";font-family:Consolas,'Courier New',monospace;font-size:11px;margin-top:-8px;margin-right:-4px;}" +
                ".source-code{white-space:pre-wrap;word-wrap:break-word;line-height:1.5;outline:none;cursor:text;}" +
                ".source-code:focus{border-color:" + link + ";}" +
                "blockquote{padding:0 1em;color:" + muted + ";border-left:.25em solid " + border + ";}" +
                "table{border-collapse:collapse;width:100%;}th,td{border:1px solid " + border + ";padding:6px 13px;}" +
                "img{max-width:100%;}hr{height:4px;padding:0;margin:24px 0;background:" + border + ";border:0;}" +
                "</style></head><body><div class=\"markdown-body\">" + body + "</div></body></html>";
        }
    }

    public static class Markdown
    {
        public static string ToHtml(string markdown)
        {
            var lines = markdown.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            var html = new StringBuilder();
            var codeBuffer = new StringBuilder();
            string codeLanguage = "";
            bool inCode = false, inUl = false, inOl = false, inBlockquote = false, inParagraph = false, inTable = false;
            var paragraph = new StringBuilder();

            Action flushParagraph = () =>
            {
                if (inParagraph)
                {
                    html.Append("<p>").Append(Inline(paragraph.ToString().Trim())).AppendLine("</p>");
                    paragraph.Clear();
                    inParagraph = false;
                }
            };
            Action closeLists = () =>
            {
                if (inUl) { html.AppendLine("</ul>"); inUl = false; }
                if (inOl) { html.AppendLine("</ol>"); inOl = false; }
            };
            Action closeBlockquote = () =>
            {
                if (inBlockquote) { html.AppendLine("</blockquote>"); inBlockquote = false; }
            };
            Action closeTable = () =>
            {
                if (inTable) { html.AppendLine("</tbody></table>"); inTable = false; }
            };

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.TrimStart().StartsWith("```"))
                {
                    flushParagraph(); closeLists(); closeBlockquote(); closeTable();
                    if (!inCode)
                    {
                        codeLanguage = line.Trim().Substring(3).Trim().ToLowerInvariant();
                        codeBuffer.Clear();
                        inCode = true;
                    }
                    else
                    {
                        html.Append(RenderCodeBlock(codeBuffer.ToString(), codeLanguage));
                        codeBuffer.Clear();
                        codeLanguage = "";
                        inCode = false;
                    }
                    continue;
                }
                if (inCode)
                {
                    codeBuffer.AppendLine(line);
                    continue;
                }

                if (String.IsNullOrWhiteSpace(line))
                {
                    flushParagraph(); closeLists(); closeBlockquote(); closeTable();
                    continue;
                }

                if (Regex.IsMatch(line, @"^\s*(-{3,}|\*{3,}|_{3,})\s*$"))
                {
                    flushParagraph(); closeLists(); closeBlockquote(); closeTable();
                    html.AppendLine("<hr>");
                    continue;
                }

                var heading = Regex.Match(line, @"^(#{1,6})\s+(.+)$");
                if (heading.Success)
                {
                    flushParagraph(); closeLists(); closeBlockquote(); closeTable();
                    int level = heading.Groups[1].Value.Length;
                    html.Append("<h").Append(level).Append(">").Append(Inline(heading.Groups[2].Value.Trim())).Append("</h").Append(level).AppendLine(">");
                    continue;
                }

                if (line.TrimStart().StartsWith(">"))
                {
                    flushParagraph(); closeLists(); closeTable();
                    if (!inBlockquote) { html.AppendLine("<blockquote>"); inBlockquote = true; }
                    html.Append("<p>").Append(Inline(line.TrimStart().TrimStart('>').Trim())).AppendLine("</p>");
                    continue;
                }
                closeBlockquote();

                var unordered = Regex.Match(line, @"^\s*[-+*]\s+(.+)$");
                if (unordered.Success)
                {
                    flushParagraph(); closeTable();
                    if (!inUl) { closeLists(); html.AppendLine("<ul>"); inUl = true; }
                    html.Append("<li>").Append(Inline(unordered.Groups[1].Value.Trim())).AppendLine("</li>");
                    continue;
                }

                var ordered = Regex.Match(line, @"^\s*\d+\.\s+(.+)$");
                if (ordered.Success)
                {
                    flushParagraph(); closeTable();
                    if (!inOl) { closeLists(); html.AppendLine("<ol>"); inOl = true; }
                    html.Append("<li>").Append(Inline(ordered.Groups[1].Value.Trim())).AppendLine("</li>");
                    continue;
                }
                closeLists();

                if (LooksLikeTableHeader(lines, i))
                {
                    flushParagraph(); closeTable();
                    html.AppendLine("<table><thead><tr>");
                    foreach (var cell in SplitTable(line)) html.Append("<th>").Append(Inline(cell.Trim())).AppendLine("</th>");
                    html.AppendLine("</tr></thead><tbody>");
                    i++;
                    inTable = true;
                    continue;
                }

                if (inTable && line.Contains("|"))
                {
                    html.AppendLine("<tr>");
                    foreach (var cell in SplitTable(line)) html.Append("<td>").Append(Inline(cell.Trim())).AppendLine("</td>");
                    html.AppendLine("</tr>");
                    continue;
                }
                closeTable();

                if (inParagraph) paragraph.Append(" ");
                paragraph.Append(line.Trim());
                inParagraph = true;
            }

            flushParagraph(); closeLists(); closeBlockquote(); closeTable();
            if (inCode) html.Append(RenderCodeBlock(codeBuffer.ToString(), codeLanguage));
            return html.ToString();
        }

        private static string RenderCodeBlock(string code, string language)
        {
            var lang = NormalizeLanguage(language);
            var label = String.IsNullOrEmpty(lang) ? "" : "<span class=\"lang-label\">" + WebUtility.HtmlEncode(lang) + "</span>";
            return "<pre>" + label + "<code class=\"language-" + WebUtility.HtmlEncode(lang) + "\">" + Highlight(code, lang) + "</code></pre>\n";
        }

        private static string NormalizeLanguage(string language)
        {
            if (String.IsNullOrWhiteSpace(language)) return "";
            language = language.Trim().ToLowerInvariant();
            if (language == "htm") return "html";
            if (language == "mysql" || language == "mariadb" || language == "pgsql" || language == "postgres") return "sql";
            if (language == "js") return "javascript";
            if (language == "cs") return "csharp";
            if (language == "ps1") return "powershell";
            if (language == ".env" || language == "dotenv" || language == "environment") return "env";
            return language;
        }

        private static string Highlight(string code, string language)
        {
            var encoded = WebUtility.HtmlEncode(code);
            if (language == "html" || language == "xml")
                return HighlightHtml(encoded);
            if (language == "sql")
                return HighlightKeywords(encoded, @"\b(SELECT|FROM|WHERE|JOIN|INNER|LEFT|RIGHT|FULL|OUTER|ON|GROUP|BY|ORDER|HAVING|INSERT|INTO|VALUES|UPDATE|SET|DELETE|CREATE|ALTER|DROP|TABLE|VIEW|INDEX|PRIMARY|KEY|FOREIGN|REFERENCES|NOT|NULL|DEFAULT|AUTO_INCREMENT|AND|OR|IN|IS|LIKE|BETWEEN|LIMIT|OFFSET|DISTINCT|AS|CASE|WHEN|THEN|ELSE|END|COUNT|SUM|AVG|MIN|MAX)\b", true);
            if (language == "php")
                return HighlightKeywords(encoded, @"\b(function|public|private|protected|static|return|if|else|elseif|foreach|for|while|switch|case|break|continue|try|catch|throw|new|use|namespace|echo|require|include|null|true|false|array)\b", false);
            if (language == "javascript" || language == "json")
                return HighlightKeywords(encoded, @"\b(function|const|let|var|return|if|else|for|while|switch|case|break|continue|new|try|catch|throw|async|await|import|export|from|true|false|null|undefined)\b", false);
            if (language == "css")
                return HighlightKeywords(encoded, @"\b(display|position|relative|absolute|fixed|flex|grid|block|inline|none|margin|padding|border|background|color|font|width|height|min|max|content|align|justify)\b", false);
            if (language == "csharp")
                return HighlightKeywords(encoded, @"\b(using|namespace|public|private|protected|static|void|string|int|bool|var|new|return|if|else|foreach|for|while|switch|case|break|continue|try|catch|throw|null|true|false)\b", false);
            if (language == "powershell")
                return HighlightKeywords(encoded, @"\b(function|param|if|else|elseif|foreach|for|while|switch|return|try|catch|throw|Write-Host|Get-ChildItem|Get-Content|Set-Content|New-Item)\b", false);
            if (language == "env" || language == "ini")
                return HighlightEnv(encoded);
            return encoded;
        }

        private static string HighlightKeywords(string encoded, string keywordPattern, bool upperOnly)
        {
            encoded = Regex.Replace(encoded, @"(&quot;.*?&quot;|'.*?')", "<span class=\"tok-str\">$1</span>");
            encoded = Regex.Replace(encoded, @"(--.*?$|//.*?$|#.*?$)", "<span class=\"tok-com\">$1</span>", RegexOptions.Multiline);
            encoded = Regex.Replace(encoded, @"\b\d+(\.\d+)?\b", "<span class=\"tok-num\">$0</span>");
            var options = upperOnly ? RegexOptions.None : RegexOptions.IgnoreCase;
            return Regex.Replace(encoded, keywordPattern, "<span class=\"tok-key\">$1</span>", options);
        }

        private static string HighlightHtml(string encoded)
        {
            return Regex.Replace(encoded, @"&lt;!--.*?--&gt;|&lt;/?[^&]*?&gt;", m =>
            {
                var tag = m.Value;
                if (tag.StartsWith("&lt;!--")) return "<span class=\"tok-com\">" + tag + "</span>";
                tag = Regex.Replace(tag, @"(&lt;/?)([a-zA-Z0-9:-]+)", "$1<span class=\"tok-tag\">$2</span>");
                tag = Regex.Replace(tag, @"\s([a-zA-Z_:][-a-zA-Z0-9_:.]*)(=)", " <span class=\"tok-attr\">$1</span>$2");
                return tag;
            }, RegexOptions.Singleline);
        }

        private static string HighlightEnv(string encoded)
        {
            var output = new StringBuilder();
            var lines = encoded.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            foreach (var line in lines)
            {
                if (Regex.IsMatch(line, @"^\s*(#|;|//)"))
                {
                    output.Append("<span class=\"tok-com\">").Append(line).Append("</span>").AppendLine();
                    continue;
                }

                var section = Regex.Match(line, @"^(\s*)\[([^\]]+)\](\s*)$");
                if (section.Success)
                {
                    output.Append(section.Groups[1].Value)
                        .Append("[<span class=\"tok-key\">")
                        .Append(section.Groups[2].Value)
                        .Append("</span>]")
                        .Append(section.Groups[3].Value)
                        .AppendLine();
                    continue;
                }

                var assignment = Regex.Match(line, @"^(\s*)([A-Za-z_][A-Za-z0-9_.-]*)(\s*=\s*)(.*)$");
                if (assignment.Success)
                {
                    output.Append(assignment.Groups[1].Value)
                        .Append("<span class=\"tok-var\">")
                        .Append(assignment.Groups[2].Value)
                        .Append("</span><span class=\"tok-op\">")
                        .Append(assignment.Groups[3].Value)
                        .Append("</span>")
                        .Append(HighlightEnvValue(assignment.Groups[4].Value))
                        .AppendLine();
                    continue;
                }

                output.Append(line).AppendLine();
            }
            return output.ToString();
        }

        private static string HighlightEnvValue(string value)
        {
            var comment = Regex.Match(value, @"^(.*?)(\s+(#|;|//).*)$");
            var body = comment.Success ? comment.Groups[1].Value : value;
            var suffix = comment.Success ? "<span class=\"tok-com\">" + comment.Groups[2].Value + "</span>" : "";

            if (Regex.IsMatch(body, "^(&quot;.*&quot;|'.*')$"))
                body = "<span class=\"tok-str\">" + body + "</span>";
            else if (Regex.IsMatch(body, @"^\d+(\.\d+)?$"))
                body = "<span class=\"tok-num\">" + body + "</span>";
            else if (Regex.IsMatch(body, @"^(true|false|null|yes|no|on|off)$", RegexOptions.IgnoreCase))
                body = "<span class=\"tok-key\">" + body + "</span>";

            return body + suffix;
        }

        private static bool LooksLikeTableHeader(string[] lines, int index)
        {
            if (index + 1 >= lines.Length || !lines[index].Contains("|")) return false;
            return Regex.IsMatch(lines[index + 1], @"^\s*\|?\s*:?-{3,}:?\s*(\|\s*:?-{3,}:?\s*)+\|?\s*$");
        }

        private static IEnumerable<string> SplitTable(string line)
        {
            line = line.Trim();
            if (line.StartsWith("|")) line = line.Substring(1);
            if (line.EndsWith("|")) line = line.Substring(0, line.Length - 1);
            return line.Split('|');
        }

        private static string Inline(string text)
        {
            text = WebUtility.HtmlEncode(text);
            text = Regex.Replace(text, @"!\[([^\]]*)\]\(([^)]+)\)", "<img alt=\"$1\" src=\"$2\">");
            text = Regex.Replace(text, @"\[([^\]]+)\]\(([^)]+)\)", "<a href=\"$2\">$1</a>");
            text = Regex.Replace(text, @"`([^`]+)`", "<code>$1</code>");
            text = Regex.Replace(text, @"\*\*([^*]+)\*\*", "<strong>$1</strong>");
            text = Regex.Replace(text, @"__([^_]+)__", "<strong>$1</strong>");
            text = Regex.Replace(text, @"\*([^*]+)\*", "<em>$1</em>");
            text = Regex.Replace(text, @"_([^_]+)_", "<em>$1</em>");
            return text;
        }
    }
}
