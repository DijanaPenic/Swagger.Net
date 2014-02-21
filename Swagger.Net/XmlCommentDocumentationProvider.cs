using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Xml.XPath;

using System.Collections.Generic;
using System.Web.Mvc;

using ColorCode;

namespace Swagger.Net
{
    /// <summary>
    /// Accesses the XML doc blocks written in code to further document the API.
    /// All credit goes to: <see cref="http://blogs.msdn.com/b/yaohuang1/archive/2012/05/21/asp-net-web-api-generating-a-web-api-help-page-using-apiexplorer.aspx"/>
    /// </summary>
    public class XmlCommentDocumentationProvider : IDocumentationProvider
    {
        XPathNavigator _documentNavigator;
        private const string _methodExpression = "/doc/members/member[@name='M:{0}']";
        private static Regex nullableTypeNameRegex = new Regex(@"(.*\.Nullable)" + Regex.Escape("`1[[") + "([^,]*),.*");

        public XmlCommentDocumentationProvider(string documentPath)
        {
            XPathDocument xpath = new XPathDocument(documentPath);
            _documentNavigator = xpath.CreateNavigator();
        }

        public virtual string GetDocumentation(HttpParameterDescriptor parameterDescriptor)
        {
            ReflectedHttpParameterDescriptor reflectedParameterDescriptor = parameterDescriptor as ReflectedHttpParameterDescriptor;
            if (reflectedParameterDescriptor != null)
            {
                XPathNavigator memberNode = GetMemberNode(reflectedParameterDescriptor.ActionDescriptor);
                if (memberNode != null)
                {
                    string parameterName = reflectedParameterDescriptor.ParameterInfo.Name;
                    XPathNavigator parameterNode = memberNode.SelectSingleNode(string.Format("param[@name='{0}']", parameterName));
                    if (parameterNode != null)
                    {
                        return parameterNode.Value.Trim();
                    }
                }
            }

            return "No Documentation Found.";
        }

        public virtual bool GetRequired(HttpParameterDescriptor parameterDescriptor)
        {
            ReflectedHttpParameterDescriptor reflectedParameterDescriptor = parameterDescriptor as ReflectedHttpParameterDescriptor;
            if (reflectedParameterDescriptor != null)
            {
                return !reflectedParameterDescriptor.ParameterInfo.IsOptional;
            }

            return true;
        }

        public virtual string GetDocumentation(HttpActionDescriptor actionDescriptor)
        {
            XPathNavigator memberNode = GetMemberNode(actionDescriptor);
            if (memberNode != null)
            {
                XPathNavigator summaryNode = memberNode.SelectSingleNode("summary");
                if (summaryNode != null)
                {
                    return summaryNode.Value.Trim();
                }
            }

            return "No Documentation Found.";
        }

        public virtual string GetNotes(HttpActionDescriptor actionDescriptor)
        {
            XPathNavigator memberNode = GetMemberNode(actionDescriptor);
            if (memberNode != null)
            {
                XPathNavigator summaryNode = memberNode.SelectSingleNode("remarks");
                if (summaryNode != null)
                {
                    var PrittifyString = "<pre style=\"white-space: pre-wrap;\">" + summaryNode.Value + "</pre>";
                    var HtmlString = FormatUrls(PrittifyString);
                    return HtmlString.Trim().Replace("\r\n            ", "\r\n");
                }
            }

            return "No Documentation Found.";
        }

        public virtual string GetReturn(HttpActionDescriptor actionDescriptor)
        {
            XPathNavigator memberNode = GetMemberNode(actionDescriptor);
            if (memberNode != null)
            {
                XPathNavigator summaryNode = memberNode.SelectSingleNode("returns");
                if (summaryNode != null)
                {
                    var PrittifyString = "<pre style=\" white-space: pre-wrap;\">" + summaryNode.Value + "</pre>";
                    var HtmlString = FormatUrls(PrittifyString);
                    return HtmlString.Trim().Replace("\r\n            ", "\r\n");
                }
            }

            return "No Documentation Found.";
        }

        public virtual string GetExamples(HttpActionDescriptor actionDescriptor)
        {
            XPathNavigator memberNode = GetMemberNode(actionDescriptor);
            if (memberNode != null)
            {
                XPathNavigator summaryNode = memberNode.SelectSingleNode("example");
                if (summaryNode != null)
                {
                    var result = JsonHelper.FormatJson(summaryNode.Value.Trim());
                    string colorizedSourceCode = new CodeColorizer().Colorize(result, Languages.Sql);
                    var HtmlString = FormatUrls(colorizedSourceCode.Replace(";,", " ").Replace(";</span>,", " "), true);
                    return HtmlString;
                }
            }

            return "No Documentation Found.";
        }

        public virtual string GetCode(HttpActionDescriptor actionDescriptor)
        {
            XPathNavigator memberNode = GetMemberNode(actionDescriptor);
            if (memberNode != null)
            {
                XPathNavigator CodeType = memberNode.SelectSingleNode("code");

                if (CodeType != null)
                {
                    string colorizedSourceCode = new CodeColorizer().Colorize(CodeType.Value.Trim().Replace("\r\n       ", "\r\n"), Languages.CSharp);
                    return colorizedSourceCode;
                }
            }

            return "No Documentation Found.";
        }

        public virtual string GetResponseClass(HttpActionDescriptor actionDescriptor)
        {
            ReflectedHttpActionDescriptor reflectedActionDescriptor = actionDescriptor as ReflectedHttpActionDescriptor;
            if (reflectedActionDescriptor != null)
            {
                if (reflectedActionDescriptor.MethodInfo.ReturnType.IsGenericType)
                {
                    StringBuilder sb = new StringBuilder(reflectedActionDescriptor.MethodInfo.ReturnParameter.ParameterType.Name);
                    sb.Append("<");
                    Type[] types = reflectedActionDescriptor.MethodInfo.ReturnParameter.ParameterType.GetGenericArguments();
                    for (int i = 0; i < types.Length; i++)
                    {
                        sb.Append(types[i].Name);
                        if (i != (types.Length - 1)) sb.Append(", ");
                    }
                    sb.Append(">");
                    return sb.Replace("`1", "").ToString();
                }
                else
                    return reflectedActionDescriptor.MethodInfo.ReturnType.Name;
            }

            return "void";
        }

        public virtual string GetNickname(HttpActionDescriptor actionDescriptor)
        {
            ReflectedHttpActionDescriptor reflectedActionDescriptor = actionDescriptor as ReflectedHttpActionDescriptor;
            if (reflectedActionDescriptor != null)
            {
                return reflectedActionDescriptor.MethodInfo.Name;
            }

            return "NicknameNotFound";
        }

        private XPathNavigator GetMemberNode(HttpActionDescriptor actionDescriptor)
        {
            ReflectedHttpActionDescriptor reflectedActionDescriptor = actionDescriptor as ReflectedHttpActionDescriptor;
            if (reflectedActionDescriptor != null)
            {
                string selectExpression = string.Format(_methodExpression, GetMemberName(reflectedActionDescriptor.MethodInfo));
                XPathNavigator node = _documentNavigator.SelectSingleNode(selectExpression);
                if (node != null)
                {
                    return node;
                }
            }

            return null;
        }

        private static string GetMemberName(MethodInfo method)
        {
            string name = string.Format("{0}.{1}", method.DeclaringType.FullName, method.Name);
            var parameters = method.GetParameters();
            if (parameters.Length != 0)
            {
                string[] parameterTypeNames = parameters.Select(param => ProcessTypeName(param.ParameterType.FullName)).ToArray();
                name += string.Format("({0})", string.Join(",", parameterTypeNames));
            }

            return name;
        }

        private static string ProcessTypeName(string typeName)
        {
            //handle nullable
            var result = nullableTypeNameRegex.Match(typeName);
            if (result.Success)
            {
                return string.Format("{0}{{{1}}}", result.Groups[1].Value, result.Groups[2].Value);
            }
            return typeName;
        }

        public static string FormatUrls(string input, bool indent = false)
        {
            string output = input;
            Regex regx1 = new Regex("http(s)?://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,\\{\\}]*([a-zA-Z0-9\\?\\#\\=\\/]){1})?", RegexOptions.IgnoreCase);
            Regex regx2 = new Regex("http(s)? : //([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,\\{\\}]*([a-zA-Z0-9\\?\\#\\=\\/]){1})?", RegexOptions.IgnoreCase);

            MatchCollection mactches = regx1.Matches(output);
            MatchCollection mactches2 = regx2.Matches(output);

            foreach (Match match in mactches)
            {
                output = UrlOutput(match, output, indent);
            }

            foreach (Match match in mactches2)
            {
                output = UrlOutput(match, output, indent);
            }

            return output;
        }

        public static string UrlOutput(Match match, string output, bool indent)
        {
            StringBuilder sb = new StringBuilder();
            if (indent == false) 
                sb.Append("<span style = \"word-break: break-all;\">");
            else
                sb.Append("<span style = \"word-break: break-all;  display:inline-block; width:650px;\">");


            var MatchString = sb.ToString() + "<a href='" + match.Value + "' target='blank' style = \"color:#004D51; white-space: pre-wrap;\">" + match.Value + "</a> </span>";

            Regex Match = new Regex(MatchString, RegexOptions.IgnoreCase);
            MatchCollection VerifyMatches = Match.Matches(output);

            if (VerifyMatches.Count == 0)
                output = output.Replace(match.Value, MatchString);

            return output;
        }

        public class JsonHelper
        {
            private const int INDENT_SIZE = 4;

            public static string FormatJson(string str)
            {
                str = (str ?? "").Replace("{}", @"\{\}").Replace("[]", @"\[\]");

                var inserts = new List<int[]>();
                bool quoted = false, escape = false;
                int depth = 0/*-1*/;

                for (int i = 0, N = str.Length; i < N; i++)
                {
                    var chr = str[i];

                    if (!escape && !quoted)
                        switch (chr)
                        {
                            case '{':
                            case '[':
                                inserts.Add(new[] { i, +1, 0, INDENT_SIZE * ++depth });
                                break;
                            case ',':
                                inserts.Add(new[] { i, +1, 0, INDENT_SIZE * depth });
                                break;
                            case '}':
                            case ']':
                                inserts.Add(new[] { i, -1, INDENT_SIZE * --depth, 0 });
                                break;
                            case ':':
                                inserts.Add(new[] { i, 0, 1, 1 });
                                break;
                        }

                    quoted = (chr == '"') ? !quoted : quoted;
                    escape = (chr == '\\') ? !escape : false;
                }

                if (inserts.Count > 0)
                {
                    var sb = new System.Text.StringBuilder(str.Length * 2);

                    int lastIndex = 0;
                    foreach (var insert in inserts)
                    {
                        int index = insert[0], before = insert[2], after = insert[3];
                        bool nlBefore = (insert[1] == -1), nlAfter = (insert[1] == +1);

                        sb.Append(str.Substring(lastIndex, index - lastIndex));

                        if (nlBefore) sb.AppendLine();
                        if (before > 0) sb.Append(new String(' ', before));

                        sb.Append(str[index]);

                        if (nlAfter) sb.AppendLine();
                        if (after > 0) sb.Append(new String(' ', after));

                        lastIndex = index + 1;
                    }

                    str = sb.ToString();
                }

                return str.Replace(@"\{\}", "{}").Replace(@"\[\]", "[]");
            }
        }
    }
}
