using System;
using System.Collections.ObjectModel;
using System.Windows;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Win32;
using Project.Models;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
using MessageBox = System.Windows.MessageBox;

namespace Project.Tools
{
    public class CheckGenerator
    {
        public void GenerateCheck(string orderId, DateTime? orderDate, Client client, User manager,
            Payment payment, Delivery delivery, string deliveryAddress, ObservableCollection<Car> cars)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                FileName = $"Check_Order_{orderId}_{DateTime.Now:dd.MM.yyyy_HH-mm}.docx",
                DefaultExt = "docx"
            };

            if (saveFileDialog.ShowDialog() != true)
                return;

            using (var wordDocument = WordprocessingDocument.Create(saveFileDialog.FileName, WordprocessingDocumentType.Document))
            {
                var mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                // ООО "Автосалон" слева и дата справа на одной строке
                var headerPara = body.AppendChild(new Paragraph());

                var companyRun = headerPara.AppendChild(new Run());
                companyRun.AppendChild(new Text("ООО \"Автосалон\""));
                companyRun.RunProperties = new RunProperties
                {
                    Bold = new Bold(),
                    Underline = new Underline { Val = UnderlineValues.Single },
                    FontSize = new FontSize { Val = "28" }, // 14 pt
                    RunFonts = new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }
                };

                headerPara.AppendChild(new Run(new TabChar()));

                var dateRun = headerPara.AppendChild(new Run());
                dateRun.AppendChild(new Text(DateTime.Now.ToString("dd.MM.yyyy HH:mm")));
                dateRun.RunProperties = new RunProperties
                {
                    Bold = new Bold(),
                    Underline = new Underline { Val = UnderlineValues.Single },
                    FontSize = new FontSize { Val = "28" },
                    RunFonts = new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }
                };

                headerPara.ParagraphProperties = new ParagraphProperties(
                    new Tabs(
                        new TabStop
                        {
                            Val = TabStopValues.Right,
                            Position = 9000
                        }
                    )
                );

                // Заголовок "СЧЕТ НА ОПЛАТУ"
                var titlePara = body.AppendChild(new Paragraph());
                var titleRun = titlePara.AppendChild(new Run());
                titleRun.AppendChild(new Text("СЧЕТ НА ОПЛАТУ"));
                titleRun.RunProperties = new RunProperties
                {
                    Bold = new Bold(),
                    FontSize = new FontSize { Val = "28" }, // 14 pt
                    RunFonts = new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }
                };
                titlePara.ParagraphProperties = new ParagraphProperties
                {
                    Justification = new Justification { Val = JustificationValues.Center },
                    SpacingBetweenLines = new SpacingBetweenLines { After = "200" }
                };

                // Номер заказа
                var orderNumberPara = body.AppendChild(new Paragraph());
                var orderNumberRun = orderNumberPara.AppendChild(new Run());
                orderNumberRun.AppendChild(new Text($"Номер заказа: {orderId} от {(orderDate?.ToString("dd.MM.yyyy") ?? DateTime.Now.ToString("dd.MM.yyyy"))}"));
                orderNumberRun.RunProperties = new RunProperties
                {
                    FontSize = new FontSize { Val = "22" }, // 11 pt
                    RunFonts = new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }
                };
                orderNumberPara.ParagraphProperties = new ParagraphProperties
                {
                    SpacingBetweenLines = new SpacingBetweenLines { After = "100" }
                };

                // Клиент
                var clientText = $"Клиент: {client?.ClientName}".Trim();
                var clientPara = body.AppendChild(new Paragraph());
                var clientRun = clientPara.AppendChild(new Run());
                clientRun.AppendChild(new Text(clientText));
                clientRun.RunProperties = new RunProperties
                {
                    FontSize = new FontSize { Val = "22" },
                    RunFonts = new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }
                };

                // Менеджер
                var managerText = $"Менеджер: {manager?.Surname} {manager?.Firstname} {manager?.Patronymic}".Trim();
                var managerPara = body.AppendChild(new Paragraph());
                var managerRun = managerPara.AppendChild(new Run());
                managerRun.AppendChild(new Text(managerText));
                managerRun.RunProperties = new RunProperties
                {
                    FontSize = new FontSize { Val = "22" },
                    RunFonts = new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }
                };

                // Тип оплаты
                var paymentText = $"Тип оплаты: {payment?.PaymentName ?? "Не указан"}";
                var paymentPara = body.AppendChild(new Paragraph());
                var paymentRun = paymentPara.AppendChild(new Run());
                paymentRun.AppendChild(new Text(paymentText));
                paymentRun.RunProperties = new RunProperties
                {
                    FontSize = new FontSize { Val = "22" },
                    RunFonts = new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }
                };

                // Тип доставки
                var deliveryText = $"Тип доставки: {delivery?.DeliveryName ?? "Не указан"}";
                var deliveryPara = body.AppendChild(new Paragraph());
                var deliveryRun = deliveryPara.AppendChild(new Run());
                deliveryRun.AppendChild(new Text(deliveryText));
                deliveryRun.RunProperties = new RunProperties
                {
                    FontSize = new FontSize { Val = "22" },
                    RunFonts = new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }
                };

                // Адрес доставки
                var addressText = $"Адрес доставки: {deliveryAddress}";
                var addressPara = body.AppendChild(new Paragraph());
                var addressRun = addressPara.AppendChild(new Run());
                addressRun.AppendChild(new Text(addressText));
                addressRun.RunProperties = new RunProperties
                {
                    FontSize = new FontSize { Val = "22" },
                    RunFonts = new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }
                };
                addressPara.ParagraphProperties = new ParagraphProperties
                {
                    SpacingBetweenLines = new SpacingBetweenLines { After = "200" }
                };

                // Таблица автомобилей
                var carsTable = body.AppendChild(new Table());
                // Задаем фиксированную ширину таблицы (примерно 16000 twips = 28 см)
                var tableProperties = new TableProperties(
                    new TableWidth { Width = "9072", Type = TableWidthUnitValues.Dxa }, // ширина 16 см
                    new TableIndentation { Width = 57, Type = TableWidthUnitValues.Dxa }, // отступ слева 0.1 см
                    new TableJustification { Val = TableRowAlignmentValues.Left }, // выравнивание таблицы слева
                    new TableBorders(
                        new TopBorder { Val = BorderValues.Single, Size = 4 },
                        new BottomBorder { Val = BorderValues.Single, Size = 4 },
                        new LeftBorder { Val = BorderValues.Single, Size = 4 },
                        new RightBorder { Val = BorderValues.Single, Size = 4 },
                        new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                        new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }
                    ),
                    new TableLayout { Type = TableLayoutValues.Fixed }
                );

                carsTable.AppendChild(tableProperties);


                // Добавляем TableGrid с ширинами столбцов
                // Ширины примерно: № — 1 см, Марка — 4 см, Модель — 6 см, Цвет — 2,5 см, Цена — 2,5 см
                int[] widths = {
                    567,    // № - 1 см
                    2268,   // Марка - 4 см
                    3402,   // Модель - 6 см
                    1417,   // Цвет - 2.5 см
                    1417    // Цена - 2.5 см
                };

                var tableGrid = new TableGrid();
                foreach (var w in widths)
                {
                    tableGrid.AppendChild(new GridColumn { Width = w.ToString() });
                }
                carsTable.AppendChild(tableGrid);
                
                // Заголовки таблицы
                var carsHeaderRow = carsTable.AppendChild(new TableRow());
                string[] headers = { "№", "Марка", "Модель", "Цвет", "Цена, ₽" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = carsHeaderRow.AppendChild(new TableCell());
                    var para = cell.AppendChild(new Paragraph());
                    var run = para.AppendChild(new Run());
                    run.AppendChild(new Text(headers[i]));
                    run.RunProperties = new RunProperties
                    {
                        Bold = new Bold(),
                        FontSize = new FontSize { Val = "24" }, // 12 pt
                        RunFonts = new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }
                    };
                    cell.AppendChild(new TableCellProperties(new TableCellWidth { Width = widths[i].ToString(), Type = TableWidthUnitValues.Dxa }));
                }

                // Данные таблицы
                decimal totalPrice = 0;
                for (int i = 0; i < cars.Count; i++)
                {
                    var car = cars[i];
                    var row = carsTable.AppendChild(new TableRow());
                    string[] values = {
                        (i + 1).ToString(),
                        car.Mark?.MarkName ?? "-",
                        car.Model?.ModelName ?? "-",
                        car.Color?.ColorName ?? "-",
                        car.Price.ToString("N2")
                    };
                    totalPrice += car.Price;

                    for (int j = 0; j < values.Length; j++)
                    {
                        var cell = row.AppendChild(new TableCell());
                        var para = cell.AppendChild(new Paragraph());
                        var run = para.AppendChild(new Run());
                        run.AppendChild(new Text(values[j]));
                        run.RunProperties = new RunProperties
                        {
                            FontSize = new FontSize { Val = "22" }, // 11 pt
                            RunFonts = new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }
                        };
                        cell.AppendChild(new TableCellProperties(new TableCellWidth { Width = widths[j].ToString(), Type = TableWidthUnitValues.Dxa }));
                    }
                }

                // Итоговая сумма
                var totalPara = body.AppendChild(new Paragraph());
                var totalRun = totalPara.AppendChild(new Run());
                totalRun.AppendChild(new Text($"Итоговая сумма: {totalPrice:N2} ₽"));
                totalRun.RunProperties = new RunProperties
                {
                    Bold = new Bold(),
                    FontSize = new FontSize { Val = "22" },
                    RunFonts = new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }
                };
                totalPara.ParagraphProperties = new ParagraphProperties
                {
                    Justification = new Justification { Val = JustificationValues.Right },
                    SpacingBetweenLines = new SpacingBetweenLines { Before = "200" }
                };

                // Подпись
                var signaturePara = body.AppendChild(new Paragraph());
                var signatureRun = signaturePara.AppendChild(new Run());
                signatureRun.AppendChild(new Text("Подпись: _____________________"));
                signatureRun.RunProperties = new RunProperties
                {
                    FontSize = new FontSize { Val = "22" },
                    RunFonts = new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }
                };
                signaturePara.ParagraphProperties = new ParagraphProperties
                {
                    SpacingBetweenLines = new SpacingBetweenLines { Before = "400" }
                };

                mainPart.Document.Save();
            }

            MessageBox.Show("Чек успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
