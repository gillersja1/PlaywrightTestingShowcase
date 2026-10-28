using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;

namespace UI.Tests.Fixtures;

/// <summary>
/// Runs axe-core accessibility scans and formats violations into readable
/// assertion messages.
/// </summary>
public static class AxeScanner
{
    /// <summary>WCAG 2.0 and 2.1, levels A and AA: the usual legal/compliance bar.</summary>
    public static readonly IReadOnlyList<string> WcagAaTags = new[] { "wcag2a", "wcag2aa", "wcag21a", "wcag21aa" };

    /// <summary>Deque's best-practice rules: not WCAG requirements, but common usability issues.</summary>
    public static readonly IReadOnlyList<string> BestPracticeTags = new[] { "best-practice" };

    public static async Task<AxeResultItem[]> ScanAsync(IPage page, IReadOnlyList<string> tags)
    {
        var options = new AxeRunOptions
        {
            RunOnly = new RunOnlyOptions { Type = "tag", Values = tags.ToList() }
        };
        var result = await page.RunAxe(options);
        return result.Violations;
    }

    /// <summary>
    /// One line per violation, e.g. "region (moderate, 1 node): All page
    /// content should be contained by landmarks -> .login_logo".
    /// </summary>
    public static string Describe(IEnumerable<AxeResultItem> violations) =>
        string.Join(Environment.NewLine, violations.Select(v =>
            $"{v.Id} ({v.Impact}, {v.Nodes.Length} node{(v.Nodes.Length == 1 ? "" : "s")}): {v.Help} -> " +
            string.Join(" | ", v.Nodes.Take(3).Select(n => n.Target.ToString()))));
}
