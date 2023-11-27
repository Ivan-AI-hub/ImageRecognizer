using System;
using System.Collections.Immutable;

namespace ImageRecognizer.DistributionServer;

public static class LogicUnitStorage
{
    private static List<string> _logicUnitsUrls = new List<string>();
    private static List<string> _freeUnitsUrls = new List<string>();
    private static List<string> _workUnitsUrls = new List<string>();

    public static ImmutableList<string> FreeUnitsUrls => _freeUnitsUrls.ToImmutableList();
    public static int FreeUnitsCount => _freeUnitsUrls.Count;

    public static void AddUnit(string url)
    {
        lock(_logicUnitsUrls)
        {
            _logicUnitsUrls.Add(url);
            _freeUnitsUrls.Add(url);
        }
    }

    public static void AddWorkerUnit(string url)
    {
        lock (_freeUnitsUrls)
        {
            Console.WriteLine($"Unit {url} start work");
            _freeUnitsUrls.Remove(url);
            _workUnitsUrls.Add(url);
        }
    }

    public static void RemoveWorkerUnit(string url)
    {
        lock (_workUnitsUrls)
        {
            Console.WriteLine($"Unit {url} stor work");
            _workUnitsUrls.Remove(url);
            _freeUnitsUrls.Add(url);
        }
    }

    public static void RemoveWorkerUnits(IEnumerable<string> urls)
    {
        lock (_workUnitsUrls)
        {
            Console.WriteLine($"Units {string.Join(",",urls)} stor work");
            _workUnitsUrls.RemoveAll(url => urls.Contains(url));
            _freeUnitsUrls.AddRange(urls);
        }
    }

    public static void RemoveUnit(string url)
    {
        lock (_logicUnitsUrls)
        {
            _logicUnitsUrls.Remove(url);
        }
    }
}
