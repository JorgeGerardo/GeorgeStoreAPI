namespace GeorgeStore.Common;

public static class EmailTemplateRenderer
{
    public static string Render(string path, Dictionary<string, string> values)
    {
        var html = File.ReadAllText(path);

        foreach (var item in values)
        {
            html = html.Replace($"{{{{{item.Key}}}}}", item.Value);
        }

        return html;
    }
}