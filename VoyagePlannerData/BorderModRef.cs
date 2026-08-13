using System;

namespace DeepwaterEngagementSuite.VoyagePlannerData;

public sealed class BorderModRef
{
    public string Id { get; init; } = "";
    public string DisplayText { get; init; } = "";
    public string Source { get; init; } = "";
    public int[] Values { get; init; } = Array.Empty<int>();

    public string Label =>
        !string.IsNullOrWhiteSpace(DisplayText) ? DisplayText :
        !string.IsNullOrWhiteSpace(Id) ? Id :
        "";
}
