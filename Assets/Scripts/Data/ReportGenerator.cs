using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AREducation.Data
{
    public static class ReportGenerator
    {
        public static ReportExport ExportPdf(StudentProfile profile, IEnumerable<QuizResult> results)
        {
            profile ??= new StudentProfile { studentId = "unknown", studentName = "Student" };
            List<QuizResult> safeResults = results?.ToList() ?? new List<QuizResult>();
            string reportId = Guid.NewGuid().ToString("N");
            string fileName = $"AR-Education-Report-{SanitizeFilePart(profile.studentName)}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf";
            string reportsDir = Path.Combine(Application.persistentDataPath, "Reports");
            Directory.CreateDirectory(reportsDir);
            string path = Path.Combine(reportsDir, fileName);

            File.WriteAllBytes(path, CreatePdf(profile, safeResults));

            return new ReportExport
            {
                reportId = reportId,
                studentId = profile.studentId,
                createdAt = DateTime.UtcNow.ToString("o"),
                filePath = path,
                includedLessonIds = safeResults.Select(r => r.lessonId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray()
            };
        }

        public static byte[] CreatePdf(StudentProfile profile, IReadOnlyList<QuizResult> results)
        {
            var lines = new List<string>
            {
                "AR Education Progress Report",
                $"Student: {Safe(profile.studentName)}",
                $"Grade: {Safe(profile.gradeLevel)}",
                $"Class: {Safe(profile.className)}",
                $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC",
                ""
            };

            if (results == null || results.Count == 0)
            {
                lines.Add("No quiz results recorded yet.");
            }
            else
            {
                float average = results.Average(r => r.percentage);
                lines.Add($"Attempts: {results.Count}");
                lines.Add($"Average Score: {average:F1}%");
                lines.Add("");
                lines.Add("Lesson | Score | Date");
                foreach (QuizResult result in results.OrderByDescending(r => r.timestamp))
                {
                    string date = FormatDate(result.timestamp);
                    lines.Add($"{Safe(result.lessonTitle)} | {result.score}/{result.totalQuestions} ({result.percentage:F0}%) | {date}");
                }
            }

            return BuildSimplePdf(lines);
        }

        private static byte[] BuildSimplePdf(IReadOnlyList<string> lines)
        {
            var objects = new List<string>();
            string escapedText = BuildTextStream(lines);
            objects.Add("<< /Type /Catalog /Pages 2 0 R >>");
            objects.Add("<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
            objects.Add("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>");
            objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
            objects.Add($"<< /Length {Encoding.ASCII.GetByteCount(escapedText)} >>\nstream\n{escapedText}\nendstream");

            var sb = new StringBuilder();
            sb.Append("%PDF-1.4\n");
            var offsets = new List<int> { 0 };
            for (int i = 0; i < objects.Count; i++)
            {
                offsets.Add(Encoding.ASCII.GetByteCount(sb.ToString()));
                sb.Append(i + 1).Append(" 0 obj\n");
                sb.Append(objects[i]).Append("\nendobj\n");
            }

            int xrefOffset = Encoding.ASCII.GetByteCount(sb.ToString());
            sb.Append("xref\n0 ").Append(objects.Count + 1).Append('\n');
            sb.Append("0000000000 65535 f \n");
            for (int i = 1; i < offsets.Count; i++)
                sb.Append(offsets[i].ToString("D10", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
            sb.Append("trailer\n<< /Size ").Append(objects.Count + 1).Append(" /Root 1 0 R >>\n");
            sb.Append("startxref\n").Append(xrefOffset).Append("\n%%EOF\n");
            return Encoding.ASCII.GetBytes(sb.ToString());
        }

        private static string BuildTextStream(IReadOnlyList<string> lines)
        {
            var sb = new StringBuilder();
            sb.Append("BT\n/F1 14 Tf\n50 740 Td\n18 TL\n");
            int count = Mathf.Min(lines.Count, 34);
            for (int i = 0; i < count; i++)
            {
                sb.Append('(').Append(EscapePdf(lines[i])).Append(") Tj\n");
                if (i < count - 1)
                    sb.Append("T*\n");
            }
            sb.Append("ET");
            return sb.ToString();
        }

        private static string EscapePdf(string value)
        {
            return Safe(value).Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        }

        private static string Safe(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "-";
            var sb = new StringBuilder(value.Length);
            foreach (char c in value)
                sb.Append(c <= 127 ? c : '?');
            return sb.ToString();
        }

        private static string FormatDate(string timestamp)
        {
            if (DateTime.TryParse(timestamp, null, DateTimeStyles.RoundtripKind, out DateTime parsed))
                return parsed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return string.IsNullOrWhiteSpace(timestamp) ? "-" : timestamp;
        }

        private static string SanitizeFilePart(string value)
        {
            string safe = Safe(value).Replace(" ", "-");
            foreach (char invalid in Path.GetInvalidFileNameChars())
                safe = safe.Replace(invalid.ToString(), "");
            return string.IsNullOrWhiteSpace(safe) || safe == "-" ? "Student" : safe;
        }
    }
}
