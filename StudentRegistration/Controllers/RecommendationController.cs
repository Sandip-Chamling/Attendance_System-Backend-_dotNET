using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Models;
using StudentRegistration.DTO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudentRegistration.Data;
using System;

namespace StudentRegistration.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationController : ControllerBase
    {
        private readonly AppDBContext context;

        public RecommendationController(AppDBContext context)
        {
            this.context = context;
        }

        [HttpGet("attendance")]
        public async Task<ActionResult> GetAttendanceRecommendations()
        {
            var attendanceDetails = await context.AttendanceDetails
                .Include(a => a.Student)
                .ThenInclude(s => s.Faculty)
                .Include(a => a.Attendance)
                .ToListAsync();

            if (!attendanceDetails.Any())
                return BadRequest("No attendance data available.");

            var totalClasses = attendanceDetails.Select(a => a.AttendanceId).Distinct().Count();

            // Calculate attendance rate
            var attendanceRates = attendanceDetails
                .GroupBy(a => a.StudentId)
                .Select(group => new
                {
                    StudentId = group.Key,
                    AttendanceRate = (double)group.Count(a => a.Status == "present") / totalClasses * 100,
                    StudentName = $"{group.FirstOrDefault()?.Student?.FirstName} {group.FirstOrDefault()?.Student?.LastName}" ?? "Unknown",
                    FacultyName = group.FirstOrDefault()?.Student?.Faculty?.FacultyName ?? "Unknown",
                    Semester = group.FirstOrDefault()?.Student?.Semester ?? "Unknown"
                })
                .ToList();

          
            var attendanceRateValues = attendanceRates.Select(r => r.AttendanceRate).ToList();

            int optimalK = FindOptimalK(attendanceRateValues, 3);

            var clusterResults = PerformKMeansClustering(attendanceRateValues, optimalK);

            var groupedRecommendations = new
            {
                FrequentAbsenteeism = new List<RecommendationDto>(),
                BelowAverageAttendance = new List<RecommendationDto>(),
                GoodAttendance = new List<RecommendationDto>()
            };

            var centroids = GetClusterCentroids(attendanceRateValues, clusterResults, optimalK)
                .OrderBy(c => c.Value)
                .Select((c, index) => new { ClusterIndex = c.Key, Level = index }) // Map cluster indices to levels
                .ToDictionary(c => c.ClusterIndex, c => c.Level);

            foreach (var rate in attendanceRates)
            {
                var cluster = clusterResults[attendanceRates.IndexOf(rate)];
                var level = centroids[cluster];
                var recommendation = new RecommendationDto
                {
                    StudentId = rate.StudentId,
                    StudentName = rate.StudentName,
                    FacultyName = rate.FacultyName,
                    Semester = rate.Semester,
                    AttendanceRate = rate.AttendanceRate,
                    RecommendationMessage = GetClusterRecommendation(level)
                };

                switch (level)
                {
                    case 0:
                        groupedRecommendations.FrequentAbsenteeism.Add(recommendation);
                        break;
                    case 1:
                        groupedRecommendations.BelowAverageAttendance.Add(recommendation);
                        break;
                    case 2:
                        groupedRecommendations.GoodAttendance.Add(recommendation);
                        break;
                }
            }

            return Ok(groupedRecommendations);
        }

        private List<int> PerformKMeansClustering(List<double> data, int k)
        {
            var centroids = InitializeCentroids(data, k);
            var clusterAssignments = new List<int>(new int[data.Count]);

            bool hasChanged;
            do
            {
                hasChanged = false;

                // Step 1: Assign each student to the closest centroid
                for (int i = 0; i < data.Count; i++)
                {
                    var closestCentroidIndex = centroids
                        .Select((c, index) => new { Index = index, Distance = Math.Abs(data[i] - c) })
                        .OrderBy(d => d.Distance)
                        .First()
                        .Index;

                    if (clusterAssignments[i] != closestCentroidIndex)
                    {
                        clusterAssignments[i] = closestCentroidIndex;
                        hasChanged = true;
                    }
                }

                // Step 2: Recompute centroids
                for (int i = 0; i < k; i++)
                {
                    var clusterPoints = data.Where((_, index) => clusterAssignments[index] == i).ToList();
                    centroids[i] = clusterPoints.Any() ? clusterPoints.Average() : centroids[i];
                }
            } while (hasChanged);

            return clusterAssignments;
        }

        private List<double> InitializeCentroids(List<double> data, int k)
        {
            var random = new Random();
            var centroids = new List<double> { data[random.Next(data.Count)] };

            for (int i = 1; i < k; i++)
            {
                var distances = data.Select(d => centroids.Min(c => Math.Pow(d - c, 2))).ToList();
                var probabilities = distances.Select(d => d / distances.Sum()).ToList();
                var cumulativeProbabilities = probabilities.Select((p, index) => probabilities.Take(index + 1).Sum()).ToList();

                var randomValue = random.NextDouble();
                centroids.Add(data[cumulativeProbabilities.FindIndex(c => c >= randomValue)]);
            }

            return centroids;
        }

        private int FindOptimalK(List<double> data, int maxK)
        {
            var sseList = new List<double>();

            for (int k = 1; k <= maxK; k++)
            {
                var clusterAssignments = PerformKMeansClustering(data, k);
                var centroids = Enumerable.Range(0, k).Select(i => data.Where((_, index) => clusterAssignments[index] == i).Average()).ToList();

                var sse = data.Select((point, index) => Math.Pow(point - centroids[clusterAssignments[index]], 2)).Sum();
                sseList.Add(sse);
            }

            for (int i = 1; i < sseList.Count - 1; i++)
            {
                if (sseList[i - 1] - sseList[i] < sseList[i] - sseList[i + 1])
                    return i + 1;
            }

            return maxK;
        }

        private Dictionary<int, double> GetClusterCentroids(List<double> data, List<int> clusterAssignments, int k)
        {
            var centroids = new Dictionary<int, double>();

            for (int i = 0; i < k; i++)
            {
                var clusterPoints = data.Where((_, index) => clusterAssignments[index] == i).ToList();
                centroids[i] = clusterPoints.Any() ? clusterPoints.Average() : 0.0;
            }

            return centroids;
        }

        private string GetClusterRecommendation(int cluster)
        {
            switch (cluster)
            {
                case 0:
                    return "Frequent absenteeism. Immediate attention required.";
                case 1:
                    return "Attendance is below average. Consider attending more classes.";
                case 2:
                    return "Good attendance. Keep it up!";
                default:
                    return "No recommendation available.";
            }
        }
    }
}
