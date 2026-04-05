using Google.OrTools.ConstraintSolver;
using Google.Protobuf.WellKnownTypes;
using TMS.Domain.Models;

namespace TMS.Application.Routing;

public static class OrToolsVehicleRouteOptimizer
{
    public const long UnreachablePenalty = 999_999_999L;

    /// <summary>
    /// Multi-vehicle routing: all vehicles start at <paramref name="depotIndex"/> and finish at <paramref name="endIndex"/>,
    /// visiting every waypoint node exactly once across the fleet. Costs minimize total travel time (seconds).
    /// </summary>
    public static IReadOnlyList<IReadOnlyList<int>> Solve(
        long?[,] timeSeconds,
        long?[,] distanceMeters,
        IReadOnlyList<RouteVehicleSpecification> vehicles,
        int depotIndex,
        int endIndex,
        int waypointNodeCount)
    {
        ArgumentNullException.ThrowIfNull(timeSeconds);
        ArgumentNullException.ThrowIfNull(distanceMeters);
        ArgumentNullException.ThrowIfNull(vehicles);
        if (vehicles.Count < 1)
            throw new ArgumentException("At least one vehicle is required.", nameof(vehicles));

        var n = timeSeconds.GetLength(0);
        if (distanceMeters.GetLength(0) != n || distanceMeters.GetLength(1) != n)
            throw new ArgumentException("Matrix dimensions must match.");

        var vehicleCount = vehicles.Count;
        var time = new long[n, n];
        var dist = new long[n, n];
        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < n; j++)
            {
                time[i, j] = timeSeconds[i, j] ?? UnreachablePenalty;
                dist[i, j] = distanceMeters[i, j] ?? UnreachablePenalty;
            }
        }

        var starts = new int[vehicleCount];
        var ends = new int[vehicleCount];
        for (var v = 0; v < vehicleCount; v++)
        {
            starts[v] = depotIndex;
            ends[v] = endIndex;
        }

        var manager = new RoutingIndexManager(n, vehicleCount, starts, ends);
        var routing = new RoutingModel(manager);

        var timeCallback = routing.RegisterTransitCallback((long fromIndex, long toIndex) =>
        {
            var fromNode = manager.IndexToNode(fromIndex);
            var toNode = manager.IndexToNode(toIndex);
            return time[fromNode, toNode];
        });
        routing.SetArcCostEvaluatorOfAllVehicles(timeCallback);

        var distCallback = routing.RegisterTransitCallback((long fromIndex, long toIndex) =>
        {
            var fromNode = manager.IndexToNode(fromIndex);
            var toNode = manager.IndexToNode(toIndex);
            return dist[fromNode, toNode];
        });

        var globalMaxDist = (long)Math.Min(
            vehicles.Max(v => v.MaxTravelMeters ?? 1_000_000_000d),
            long.MaxValue / 4.0);
        routing.AddDimension(distCallback, 0, globalMaxDist, true, "Distance");
        var distanceDimension = routing.GetMutableDimension("Distance");
        for (var v = 0; v < vehicleCount; v++)
        {
            var capMeters = vehicles[v].MaxTravelMeters ?? globalMaxDist;
            var cap = (long)Math.Min(capMeters, globalMaxDist);
            distanceDimension.CumulVar(routing.End(v)).SetMax(cap);
        }

        if (waypointNodeCount > 0)
        {
            var caps = new long[vehicleCount];
            for (var v = 0; v < vehicleCount; v++)
            {
                var c = vehicles[v].CapacityKg;
                caps[v] = c.HasValue
                    ? Math.Clamp((long)c.Value, 1, waypointNodeCount)
                    : waypointNodeCount;
            }

            var demandCallback = routing.RegisterUnaryTransitCallback((long fromIndex) =>
            {
                var node = manager.IndexToNode(fromIndex);
                if (node == depotIndex || node == endIndex)
                    return 0;
                return 1;
            });

            routing.AddDimensionWithVehicleCapacity(demandCallback, 0, caps, true, "Capacity");
        }

        var parameters = operations_research_constraint_solver.DefaultRoutingSearchParameters();
        parameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;
        parameters.LocalSearchMetaheuristic = LocalSearchMetaheuristic.Types.Value.GuidedLocalSearch;
        parameters.TimeLimit = new Duration { Seconds = 15 };

        var solution = routing.SolveWithParameters(parameters);
        if (solution is null)
            throw new InvalidOperationException("OR-Tools could not find a feasible route assignment.");

        var routes = new List<List<int>>(vehicleCount);
        for (var v = 0; v < vehicleCount; v++)
        {
            var path = new List<int>();
            if (!routing.IsVehicleUsed(solution, v))
            {
                routes.Add(path);
                continue;
            }

            long index;
            for (index = routing.Start(v); !routing.IsEnd(index); index = solution.Value(routing.NextVar(index)))
                path.Add(manager.IndexToNode((int)index));
            path.Add(manager.IndexToNode((int)index));
            routes.Add(path);
        }

        return routes;
    }
}
