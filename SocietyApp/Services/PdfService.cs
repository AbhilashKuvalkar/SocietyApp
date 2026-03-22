using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SocietyApp.Models;

namespace SocietyApp.Services
{
    public class PdfService
    {
        private static readonly string BorderColor = "#000000";
        private static readonly string MutedColor = "#6c757d";

        public byte[] GenerateReceipt(Receipt receipt)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var society = receipt.Society!;
            var member = receipt.Member!;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(18);
                    page.DefaultTextStyle(t => t.FontFamily("Times New Roman").FontSize(10));

                    page.Content().Column(col =>
                    {
                        col.Spacing(0);

                        // ── Header ────────────────────────────────────────────────
                        col.Item().AlignCenter().Text(society.Name).Bold().FontSize(14).FontColor(Colors.Red.Darken1);
                        col.Item().AlignCenter().Text($"(Reg. No. {society.RegistrationNumber})").FontSize(9);
                        col.Item().AlignCenter().Text(society.Address).FontSize(9).Italic();
                        col.Item().PaddingVertical(5).LineHorizontal(2).LineColor(BorderColor);

                        // ── Receipt No. + Date ────────────────────────────────────
                        col.Item().PaddingBottom(5).Row(row =>
                        {
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("Receipt No.: ");
                                //txt.Span(receipt.ReceiptNumber).Bold();
                            });
                            row.RelativeItem().AlignRight().Text(txt =>
                            {
                                txt.Span("Date: ").Bold();
                                txt.Span(receipt.ReceiptDate.ToString("dd-MM-yyyy")).Bold();
                            });
                        });

                        // ── Member + Payment Reference ────────────────────────────
                        col.Item().PaddingBottom(5).Border(1).BorderColor(BorderColor).Row(row =>
                        {
                            row.RelativeItem(3).BorderRight(1).BorderColor(BorderColor).Padding(5).Column(c =>
                            {
                                c.Item().Text($"{member.Salutation}. {member.Name}").Bold();
                                c.Item().Text($"Flat No. {member.FlatNumber}").FontSize(9);
                            });
                            row.RelativeItem(2).Padding(5).Column(c =>
                            {
                                c.Item().AlignRight().Text("Cheque/NEFT/IMPS Reference").FontSize(8).FontColor(MutedColor);
                                c.Item().AlignRight().Text(receipt.PaymentReference ?? "—").Bold();
                            });
                        });

                        // ── Amount in words ───────────────────────────────────────
                        col.Item().PaddingBottom(5).Row(row =>
                        {
                            row.AutoItem().Text("Rupees in words: ").Bold();
                            row.RelativeItem().PaddingLeft(4).Text(ReceiptService.AmountToWords(receipt.TotalAmount));
                        });

                        // ── Line Items Table ──────────────────────────────────────
                        col.Item().PaddingBottom(5).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(30);
                                cols.RelativeColumn(4);
                                cols.ConstantColumn(72);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Border(1).BorderColor(BorderColor).Padding(4).AlignCenter().Text("S.No.").Bold().FontSize(9);
                                h.Cell().Border(1).BorderColor(BorderColor).Padding(4).Text("PARTICULARS").Bold().FontSize(9);
                                h.Cell().Border(1).BorderColor(BorderColor).Padding(4).AlignRight().Text("AMOUNT Rs.").Bold().FontSize(9);
                            });

                            int sno = 1;
                            foreach (var item in receipt.LineItems.OrderBy(l => l.Particular?.DisplayOrder))
                            {
                                table.Cell().BorderLeft(1).BorderRight(1).BorderBottom(0.5f).BorderColor(BorderColor)
                                    .Padding(4).AlignCenter().Text(sno++.ToString()).FontSize(9);
                                table.Cell().BorderRight(1).BorderBottom(0.5f).BorderColor(BorderColor)
                                    .Padding(4).Text(item.Particular?.Name ?? "").FontSize(9);
                                table.Cell().BorderRight(1).BorderBottom(0.5f).BorderColor(BorderColor)
                                    .Padding(4).AlignRight()
                                    .Text(item.Amount > 0 ? item.Amount.ToString("N0") : "").FontSize(9);
                            }

                            table.Cell().ColumnSpan(2).Border(1).BorderColor(BorderColor)
                                .Padding(4).AlignRight().Text("TOTAL").Bold().FontSize(9);
                            table.Cell().Border(1).BorderColor(BorderColor)
                                .Padding(4).AlignRight().Text(receipt.TotalAmount.ToString("N0")).Bold().FontSize(9);
                        });

                        // ── Period ────────────────────────────────────────────────
                        col.Item().PaddingBottom(10).Row(row =>
                        {
                            row.AutoItem().AlignMiddle().Text("Received for the month of");
                            row.ConstantItem(8);  // gap between label and bordered box
                            row.AutoItem().Border(1).BorderColor(BorderColor).Padding(4)
                                .Text($"{receipt.FinancialYear} (Q{receipt.Quarter})");
                        });

                        // ── Footer: Rs Box + Signatures ───────────────────────────
                        col.Item().Row(row =>
                        {
                            row.AutoItem().Border(2).BorderColor(BorderColor).MinWidth(90).Padding(8).Column(c =>
                            {
                                c.Item().Text("Rs.").Bold().FontSize(10);
                                c.Item().Text(receipt.TotalAmount.ToString("N0")).Bold().FontSize(18);
                            });

                            row.ConstantItem(16);

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().AlignRight().Text(txt =>
                                {
                                    txt.Span("For ").FontSize(9);
                                    txt.Span(society.Name).Bold().FontSize(9);
                                });
                                c.Item().Height(28);
                                c.Item().Row(sigRow =>
                                {
                                    sigRow.RelativeItem().AlignCenter().Text("Secretary").FontSize(9);
                                    sigRow.RelativeItem().AlignCenter().Text("Chairman").FontSize(9);
                                    sigRow.RelativeItem().AlignCenter().Text("Treasurer").FontSize(9);
                                });
                            });
                        });

                        col.Item().Height(30);

                        // ── Footer note ───────────────────────────────────────────
                        col.Item().BorderTop(1).BorderColor(BorderColor)
                            .PaddingTop(4).AlignCenter()
                            .Text("Receipt Subject to the Realisation of the cheque")
                            .Italic().FontSize(8);
                    });
                });
            }).GeneratePdf();
        }
    }
}