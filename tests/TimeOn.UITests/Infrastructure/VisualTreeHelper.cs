namespace TimeOn.UITests.Infrastructure;

internal static class VisualTreeHelper
{
    public static IEnumerable<Element> Descendants(Element root)
    {
        foreach (var child in GetChildren(root))
        {
            yield return child;

            foreach (var descendant in Descendants(child))
            {
                yield return descendant;
            }
        }
    }

    public static Label? FindLabelByText(Element root, string text) =>
        Descendants(root).OfType<Label>().FirstOrDefault(label => label.Text == text);

    public static bool IsVisible(Element? element) =>
        element is VisualElement visual && visual.IsVisible;

    public static bool IsVisibleInTree(Element? element)
    {
        for (var current = element; current is not null; current = current.Parent)
        {
            if (current is VisualElement visual && !visual.IsVisible)
            {
                return false;
            }
        }

        return element is not null;
    }

    public static IEnumerable<Label> VisibleLabels(Element root) =>
        Descendants(root).OfType<Label>().Where(label => IsVisibleInTree(label));

    public static T? FindFirst<T>(Element root, Func<T, bool>? predicate = null)
        where T : Element
    {
        return Descendants(root)
            .OfType<T>()
            .FirstOrDefault(element => predicate is null || predicate(element));
    }

    private static IEnumerable<Element> GetChildren(Element element)
    {
        if (element is ContentPage contentPage && contentPage.Content is Element pageContent)
        {
            yield return pageContent;
            yield break;
        }

        switch (element)
        {
            case Layout layout:
                foreach (var child in layout.Children.OfType<Element>())
                {
                    yield return child;
                }

                break;

            case ContentView contentView when contentView.Content is Element content:
                yield return content;
                break;

            case ScrollView scrollView when scrollView.Content is Element scrollContent:
                yield return scrollContent;
                break;

            case RefreshView refreshView when refreshView.Content is Element refreshContent:
                yield return refreshContent;
                break;

            case Border border when border.Content is Element borderContent:
                yield return borderContent;
                break;
        }
    }
}
