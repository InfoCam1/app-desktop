using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using InfoCam.Models;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace InfoCam.Services
{
    public class ReportService
    {
        private readonly XFont _fontTitle = new XFont("Arial", 20, XFontStyle.Bold);
        private readonly XFont _fontHeader = new XFont("Arial", 12, XFontStyle.Bold);
        private readonly XFont _fontBody = new XFont("Arial", 10, XFontStyle.Regular);
        private readonly XFont _fontSmall = new XFont("Arial", 8, XFontStyle.Regular);

        public void GenerateIncidenciasReport(List<Incidencia> incidencias, string filePath)
        {
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Informe de Incidencias";

            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // 1. Title and Header Info
            gfx.DrawString("Informe de Incidencias InfoCam", _fontTitle, XBrushes.DarkBlue,
                new XRect(0, 20, page.Width, 50), XStringFormats.TopCenter);

            int yPoint = 80;
            gfx.DrawString($"Fecha: {DateTime.Now:g}", _fontBody, XBrushes.Black, 40, yPoint);
            yPoint += 20;
            gfx.DrawString($"Total Incidencias: {incidencias.Count}", _fontBody, XBrushes.Black, 40, yPoint);
            yPoint += 40;

            // 2. Data Grouping
            var typeStats = incidencias.GroupBy(i => i.TipoIncidencia ?? "Otros")
                                       .Select(g => new { Label = g.Key, Count = g.Count() })
                                       .OrderByDescending(x => x.Count)
                                       .ToList();

            // 3. NEW: Pie Chart for Incident Types
            // This replaces or complements the Bar Chart
            DrawIncidenciasPieChart(gfx, "Distribución Porcentual por Tipo", typeStats, ref yPoint);

            yPoint += 40;

            // 4. Detailed Charts and Tables
            DrawBarChart(gfx, "Principales Causas", typeStats.Select(s => s.Label).ToList(), typeStats.Select(s => (double)s.Count).ToList(), ref yPoint, XColors.OrangeRed);

            // ... (rest of your table logic follows)
            document.Save(filePath);
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }

        private void DrawIncidenciasPieChart(XGraphics gfx, string title, dynamic stats, ref int yPoint)
        {
            gfx.DrawString(title, _fontHeader, XBrushes.Black, 40, yPoint);
            yPoint += 25;

            double total = 0;
            foreach (var s in stats) total += s.Count;
            if (total == 0) return;

            // Professional color palette for multiple categories
            XBrush[] colors = { XBrushes.RoyalBlue, XBrushes.OrangeRed, XBrushes.ForestGreen,
                        XBrushes.Goldenrod, XBrushes.Purple, XBrushes.Chocolate };

            double startAngle = 0;
            int colorIndex = 0;
            int legendY = yPoint + 10;
            int pieSize = 120;
            int pieX = 350;

            foreach (var item in stats)
            {
                double sweepAngle = (item.Count / total) * 360;
                XBrush currentBrush = colors[colorIndex % colors.Length];

                // Draw the slice
                gfx.DrawPie(currentBrush, pieX, yPoint, pieSize, pieSize, startAngle, sweepAngle);

                // Draw Legend (up to 6 main categories)
                if (colorIndex < 6)
                {
                    gfx.DrawRectangle(currentBrush, 40, legendY, 10, 10);
                    gfx.DrawString($"{item.Label}: {item.Count} ({(item.Count / total):P1})",
                                   _fontSmall, XBrushes.Black, 60, legendY + 9);
                    legendY += 18;
                }

                startAngle += sweepAngle;
                colorIndex++;
            }

            yPoint += pieSize + 20; // Update yPoint to continue below the chart
        }

        public void GenerateCamerasReport(List<Camera> cameras, string filePath)
        {
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Informe de Cámaras";

            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            gfx.DrawString("Informe de Cámaras InfoCam", _fontTitle, XBrushes.DarkGreen,
                new XRect(0, 20, page.Width, 50), XStringFormats.TopCenter);

            int yPoint = 80;
            gfx.DrawString($"Fecha: {DateTime.Now:g}", _fontBody, XBrushes.Black, 40, yPoint);
            yPoint += 20;
            gfx.DrawString($"Total Cámaras: {cameras.Count}", _fontBody, XBrushes.Black, 40, yPoint);
            yPoint += 40;

            // 1. Pie Chart for Status
            int active = cameras.Count(c => c.Activa);
            int inactive = cameras.Count - active;
            DrawPieChart(gfx, "Estado de Cámaras", active, inactive, ref yPoint);

            yPoint += 40;

            // 2. Summary Table
            if (yPoint > page.Height - 150) { page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); yPoint = 40; }
            
            gfx.DrawString("Resumen Estadístico:", _fontHeader, XBrushes.Black, 40, yPoint);
            yPoint += 20;

            string[] headers = { "Estado", "Cantidad", "%" };
            double[] widths = { 250, 100, 100 };
            DrawTableHeader(gfx, headers, widths, 40, ref yPoint);

            string[] row1 = { "Activas", active.ToString(), $"{(active * 100.0 / cameras.Count):F1}%" };
            DrawTableRow(gfx, row1, widths, 40, ref yPoint, XBrushes.Green);

            string[] row2 = { "Inactivas", inactive.ToString(), $"{(inactive * 100.0 / cameras.Count):F1}%" };
            DrawTableRow(gfx, row2, widths, 40, ref yPoint, XBrushes.Red);

            yPoint += 10;
            gfx.DrawLine(XPens.Black, 40, yPoint, 40 + widths.Sum(), yPoint);

            string[] rowTotal = { "TOTAL", cameras.Count.ToString(), "100%" };
            DrawTableRow(gfx, rowTotal, widths, 40, ref yPoint);

            document.Save(filePath);
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }

        private void DrawBarChart(XGraphics gfx, string title, List<string> labels, List<double> values, ref int yPoint, XColor color)
        {
            gfx.DrawString(title, _fontHeader, XBrushes.Black, 40, yPoint);
            yPoint += 25;

            if (!values.Any()) return;
            double max = values.Max();
            double chartWidth = 350;
            double barHeight = 15;

            for (int i = 0; i < labels.Count && i < 6; i++)
            {
                double width = (values[i] / max) * chartWidth;
                gfx.DrawString(labels[i], _fontSmall, XBrushes.Black, 40, yPoint + 11);
                gfx.DrawRectangle(new XSolidBrush(color), 150, yPoint, width, barHeight);
                gfx.DrawString(values[i].ToString(), _fontSmall, XBrushes.Black, 150 + width + 5, yPoint + 11);
                yPoint += 20;
            }
        }

        private void DrawPieChart(XGraphics gfx, string title, int active, int inactive, ref int yPoint)
        {
            gfx.DrawString(title, _fontHeader, XBrushes.Black, 40, yPoint);
            yPoint += 20;

            double total = active + inactive;
            if (total == 0) return;

            double activeAngle = (active / total) * 360;
            
            // Draw Pie
            gfx.DrawPie(XBrushes.Green, 300, yPoint, 100, 100, 0, activeAngle);
            gfx.DrawPie(XBrushes.Red, 300, yPoint, 100, 100, activeAngle, 360 - activeAngle);

            // Legend
            gfx.DrawRectangle(XBrushes.Green, 40, yPoint + 20, 10, 10);
            gfx.DrawString($"Activas: {active} ({active/total:P0})", _fontBody, XBrushes.Black, 60, yPoint + 29);
            
            gfx.DrawRectangle(XBrushes.Red, 40, yPoint + 40, 10, 10);
            gfx.DrawString($"Inactivas: {inactive} ({inactive/total:P0})", _fontBody, XBrushes.Black, 60, yPoint + 49);

            yPoint += 110;
        }

        private void DrawTableHeader(XGraphics gfx, string[] items, double[] widths, double xStart, ref int yPoint)
        {
            gfx.DrawRectangle(XBrushes.LightGray, xStart, yPoint, widths.Sum(), 20);
            double currentX = xStart;
            for (int i = 0; i < items.Length; i++)
            {
                gfx.DrawString(items[i], _fontHeader, XBrushes.Black, currentX + 5, yPoint + 15);
                currentX += widths[i];
            }
            yPoint += 20;
        }

        private void DrawTableRow(XGraphics gfx, string[] items, double[] widths, double xStart, ref int yPoint, XBrush lastColBrush = null)
        {
            double currentX = xStart;
            gfx.DrawLine(XPens.LightGray, xStart, yPoint + 18, xStart + widths.Sum(), yPoint + 18);
            
            for (int i = 0; i < items.Length; i++)
            {
                XBrush brush = (i == items.Length - 1 && lastColBrush != null) ? lastColBrush : XBrushes.Black;
                gfx.DrawString(items[i], _fontBody, brush, currentX + 5, yPoint + 15);
                currentX += widths[i];
            }
            yPoint += 20;
        }
    }
}
