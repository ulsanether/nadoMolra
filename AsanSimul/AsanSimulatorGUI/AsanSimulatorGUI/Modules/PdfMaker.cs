using iTextSharp.text.pdf;
using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing.Imaging;
using iTextSharp.text.pdf.codec;
using AsanSimulatorGUI.Modules;
using AsanSimulatorGUI.FCU_ORM;
using DevExpress.XtraEditors;


namespace PDFmaker
{
    class CustomPageEventHelper : PdfPageEventHelper
    {
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            PdfContentByte cb = writer.DirectContent;
            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            cb.SetFontAndSize(bf, 12);

            // 현재 페이지 번호 가져오기
            int pageNumber = writer.PageNumber;

            // 페이지 중앙 하단에 페이지 번호 추가
            float x = (document.PageSize.Width / 2);
            float y = document.BottomMargin / 2;
            cb.BeginText();
            cb.SetTextMatrix(x, y);
            cb.ShowText($"Page {pageNumber}");
            cb.EndText();
        }
    }

    public class PdfMaker
    {
        List<FCUTest> test_datas;
        DateTime now;

        public PdfMaker() { }

        public void set_fcudata(List<FCUTest> test_datas)
        {
            this.test_datas = test_datas;
        }

        public void CreatePDF()
        {
            Document doc = new Document(PageSize.A4, 50f, 50f, 40f, 40f); //종이문서 크기와 Margin

            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream("debug_fcutest.pdf", FileMode.Create));
            writer.PageEvent = new CustomPageEventHelper();
            doc.Open();
            //string image_path = "C:\\Users\\THESYSTEM\\Documents\\project\\ASAN SnTech\\FCUsimulator\\PC_app\\test.jpeg";
            //Image image = Image.GetInstance(image_path);
            //PdfPTable table = new PdfPTable(1);
            //PdfPCell cell = new PdfPCell(image);
            //table.AddCell(cell);

            //헤더
            HeaderFooter header = new HeaderFooter(new Phrase("ASAN SNT  FCU  REPORT"), false)
            { Border = Rectangle.NO_BORDER, Alignment = Element.ALIGN_CENTER }; //테두리 없이 가운데 정렬
            doc.Header = header; //생성된 header를 문서에 기록

            //푸터(페이지 번호 기록 1부터 기록이 안됨)
            //string pcnt = (writer.CurrentPageNumber - 1).ToString().Substring(1); //'01'이런식으로 앞에 0이 붙으므로 0뒤인 1에서부터 끝까지 문자를 선택
            //HeaderFooter footer = new HeaderFooter(new Phrase("Page "), true) { Border = Rectangle.NO_BORDER, Alignment = Element.ALIGN_CENTER };
            //footer.PageNumber = -1;// 효과 없음
            //doc.Footer = footer; //생성된 footer를 문서에 기록

            doc.Open(); //문서 열기
            //본문내용

            //한글 폰트를 읽어온다.
            BaseFont bf = BaseFont.CreateFont(@"C:\Windows\Fonts\malgun.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            Font font = new Font(bf, 12, Font.BOLD | Font.UNDERLINE, iTextSharp.text.pdf.CMYKColor.BLACK);

            //Paragraph contents = new Paragraph("This is My First PDF Document\n\nHello World!!");

            StringBuilder sb = new StringBuilder("This is My First PDF Document");
            sb.AppendLine();
            sb.AppendLine();
            sb.Append("Hello World!!");
            sb.AppendLine();
            sb.AppendFormat("오늘의 날짜는 {0} 입니다.", System.DateTime.Now.ToLongDateString());
            sb.AppendLine();
            sb.Append("");
            sb.Append("");
            sb.Append("");
            Paragraph contents = new Paragraph(sb.ToString(), font);
            //contents.Add(image);
            doc.Add(contents);

            for(int i = 1; i <= 5; i++)
            {
                doc.Add(new Paragraph($"This is page {i}"));
                doc.NewPage();
            }

            //// ✅ 첫 번째 테이블 (3열, 너비 설정)
            //Table table1 = new Table(3);
            //table1.Width = 100f; // 전체 너비를 100%로 설정
            //table1.SetWidths(new int[] { 50, 30, 20 }); // 개별 열 너비 설정

            //table1.AddCell(new Cell("Row 1, Col 1"));
            //table1.AddCell(new Cell("Row 1, Col 2"));
            //table1.AddCell(new Cell("Row 1, Col 3"));

            //doc.Add(table1);

            //// ✅ 두 번째 테이블 (2열, 다른 너비 설정)
            //Table table2 = new Table(2);
            //table2.Width = 100f;
            //table2.SetWidths(new int[] { 60, 40 });

            //table2.AddCell(new Cell("Row 2, Col 1"));
            //table2.AddCell(new Cell("Row 2, Col 2"));

            //doc.Add(table2);

            //// ✅ 세 번째 테이블 (4열, 또 다른 너비 설정)
            //Table table3 = new Table(4);
            //table3.Width = 100f;
            //table3.SetWidths(new int[] { 25, 25, 25, 25 });

            //table3.AddCell(new Cell("Row 3, Col 1"));
            //table3.AddCell(new Cell("Row 3, Col 2"));
            //table3.AddCell(new Cell("Row 3, Col 3"));
            //table3.AddCell(new Cell("Row 3, Col 4"));

            //doc.Add(table3);


            //// ✅ 테이블 생성 (3열)
            //Table table = new Table(3);
            //table.Width = 100f;
            //table.SetWidths(new int[] { 50, 30, 20 });

            //// ✅ 행 높이 조절 적용 (Padding 없이)
            //for (int i = 0; i < 5; i++)
            //{
            //    Cell cell1 = new Cell("Row " + (i + 1) + ", Col 1");
            //    cell1.Leading = 50f; // 줄 간격 조절

            //    Cell cell2 = new Cell("Row " + (i + 1) + ", Col 2");
            //    cell2.Leading = 25f;

            //    Cell cell3 = new Cell("Row " + (i + 1) + ", Col 3");
            //    cell3.Leading = 25f;

            //    table.AddCell(cell1);
            //    table.AddCell(cell2);
            //    table.AddCell(cell3);
            //}
            //table.SetWidths(new int[] { 10, 30, 80 });
            //for (int i = 0; i < 5; i++)
            //{
            //    Cell cell1 = new Cell("Row " + (i + 1) + ", Col 1");
            //    cell1.Leading = 50f; // 줄 간격 조절

            //    Cell cell2 = new Cell("Row " + (i + 1) + ", Col 2");
            //    cell2.Leading = 25f;

            //    Cell cell3 = new Cell("Row " + (i + 1) + ", Col 3");
            //    cell3.Leading = 25f;

            //    table.AddCell(cell1);
            //    table.AddCell(cell2);
            //    table.AddCell(cell3);
            //}

            //doc.Add(table);

            // ✅ 부모 테이블 (2열)
            Table parentTable = new Table(2);
            parentTable.Width = 100f;
            parentTable.SetWidths(new int[] { 50, 50 }); // 열 비율 설정

            // ✅ 첫 번째 셀 (일반 텍스트)
            Cell parentCell1 = new Cell("부모 테이블 셀 1");
            parentTable.AddCell(parentCell1);

            // ✅ 두 번째 셀 (자식 테이블을 넣을 셀)
            Cell parentCell2 = new Cell();

            // ✅ 자식 테이블 (2열)
            Table nestedTable = new Table(2);
            nestedTable.Width = 100f;
            nestedTable.SetWidths(new int[] { 70, 30 }); // 자식 테이블의 열 비율

            // ✅ 자식 테이블에 데이터 추가
            nestedTable.AddCell(new Cell("자식 테이블 셀 1"));
            nestedTable.AddCell(new Cell("자식 테이블 셀 2"));
            nestedTable.AddCell(new Cell("자식 테이블 셀 3"));
            nestedTable.AddCell(new Cell("자식 테이블 셀 4"));

            // ✅ 자식 테이블을 부모 셀에 추가
            parentCell2.AddElement(nestedTable);
            parentTable.AddCell(parentCell2);

            // ✅ 문서에 부모 테이블 추가
            doc.Add(parentTable);

            //writer.NewPage();
            //doc.Add(table);

            doc.Close(); //문서 닫기

        }

        public void make_report()
        {
            now = DateTime.Now;
            Document doc = new Document(PageSize.A4, 50f, 50f, 40f, 40f); //종이문서 크기와 Margin
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream("debug_fcutest.pdf", FileMode.Create));
            writer.PageEvent = new CustomPageEventHelper();

            int fontsize = 12;
            BaseFont bf = BaseFont.CreateFont(@"C:\Windows\Fonts\malgun.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            Font font20 = new Font(bf, 20, Font.BOLD, iTextSharp.text.pdf.CMYKColor.BLACK);
            Font font16 = new Font(bf, 16, Font.HELVETICA, iTextSharp.text.pdf.CMYKColor.BLACK);
            Font font12 = new Font(bf, 12, Font.BOLD, iTextSharp.text.pdf.CMYKColor.BLACK);

            doc.Open();

            make_cover(doc, writer, font20, font16, font12);
            for (int i = 0; i < 8; i++)
            {
                make_result(test_datas[i], doc, writer, font20, font16, font12);
            }

            doc.Close();

            System.Diagnostics.Process.Start(@"D:\project\project\ASAN SnTech\FCUsimulator\PC_app\AsanSimulatorGUI\AsanSimulatorGUI\bin\Debug\debug_fcutest.pdf");
        }

        void make_cover(Document doc, PdfWriter writer, Font font20, Font font16, Font font12)
        {
            Paragraph header = new Paragraph("FCU 시험 결과 성적서", font20);
            header.Alignment = Element.ALIGN_CENTER;

            doc.Add(header);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.Append($"1. 성적서 작성 일자 : {now.ToString("yyyy-MM-dd HH:mm:ss"), 5}");
            sb.AppendLine();
            sb.AppendLine();
            sb.Append($"2. 제품 번호           : FCU_{now.ToString("yyyyMMdd_HHmmss"), 5}");
            sb.AppendLine();
            sb.AppendLine();
            sb.Append($"3. 시험 항목 요약");
            sb.AppendLine();
            sb.AppendLine();
            for (int i = 0; i < 8; i++)
            {
                string pass = "Pass";
                if (test_datas[i].test_result1.Contains("Fail")
                    || test_datas[i].test_result2.Contains("Fail")
                    || test_datas[i].test_result3.Contains("Fail")
                    || test_datas[i].test_result4.Contains("Fail"))
                {
                    pass = "Fail";
                }
                if (test_datas[i].small_categ.Contains("반응시간"))
                {
                    sb.Append($"    - {test_datas[i].small_categ}              : {pass,8:C}");
                }
                else
                {
                    sb.Append($"    - {test_datas[i].small_categ}       : {pass,8:C}");
                }
                
                sb.AppendLine();
            }
            sb.AppendLine();
            sb.AppendLine();
            Paragraph contents = new Paragraph(sb.ToString(), font16);
            contents.Alignment = Element.ALIGN_JUSTIFIED;
            doc.Add(contents);

            Table pdftable = new Table(2);
            pdftable.Width = 100;
            pdftable.SetWidths(new int[] { 50, 50 });

            Cell cell1 = new Cell(new Phrase($"시험 진행자        홍 길 동          (인){Environment.NewLine}{" "}", font12));
            cell1.SetHorizontalAlignment("Center");
            //cell1.SetVerticalAlignment("Middle");
            cell1.Leading = 30f;
            pdftable.AddCell(cell1);
            Cell cell2 = new Cell(new Phrase($"시험 책임자                            (인){Environment.NewLine}{" "}", font12));
            cell2.SetHorizontalAlignment("Center");
            //cell2.SetVerticalAlignment("Middle");
            cell2.Leading = 30f;
            pdftable.AddCell(cell2);
            doc.Add(pdftable);
        }

        void make_result(FCUTest fcutest, Document doc, PdfWriter writer, Font font20, Font font16, Font font12)
        {
            doc.NewPage();
            Paragraph header1 = new Paragraph("4. 시험 결과", font16);
            doc.Add(header1);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.Append($"     - {fcutest.small_categ}");
            sb.AppendLine();
            sb.AppendLine();
            Paragraph header2 = new Paragraph(sb.ToString(), font12);
            doc.Add(header2);

            Table table = new Table(2);
            table.AddCell(new Cell(new Phrase($"시험항목 {Environment.NewLine}{" "}", font12)) { HorizontalAlignment = 1, VerticalAlignment = 5 });
            table.AddCell(new Cell(new Phrase($"{fcutest.large_categ}{Environment.NewLine}{" "}", font12)) { HorizontalAlignment = 1, VerticalAlignment = 5 });

            table.AddCell(new Cell(new Phrase($"내용 {Environment.NewLine}{" "}", font12)) { HorizontalAlignment = 1, VerticalAlignment = 5 });
            table.AddCell(new Cell(new Phrase($"{fcutest.small_categ}{Environment.NewLine}{" "}", font12)) { HorizontalAlignment = 1, VerticalAlignment = 5 });

            string resister = "";
            switch (fcutest.resistor_set)
            {
                case 0://just power on 다른 것들도 해당사항이므로 switch이전에 power on해놓음
                    resister = "정상";
                    break;
                case 1000: //1kΩ
                    resister = "1kΩ";
                    break;
                case 20000:
                case 2000: //2kΩ
                    resister = "2kΩ";
                    break;
                case 2200: //2.2kΩ
                    resister = "2.2kΩ";
                    break;
                case 2400: //2.4kΩ
                    resister = "2.4kΩ";
                    break;
                case 10000: //단선 fault relay
                    resister = "1kΩ";
                    break;
            }
            string setting = $"H(+)<->F(-) : DC28V{Environment.NewLine}" +
                            $"G(+)<->D(-) : DC28V{Environment.NewLine}" +
                            $"A(S)<->E(S) : {resister}{Environment.NewLine}{" "}";
            table.AddCell(new Cell(new Phrase($"시험설정 {Environment.NewLine}{" "}", font12)) { HorizontalAlignment = 1, VerticalAlignment = 5 });
            table.AddCell(new Cell(new Phrase(setting, font12)) { HorizontalAlignment = 1, VerticalAlignment = 5 });

            //TODO 기준과 결과값도 넣어야하나.......    

            string measure_rule = "";
            switch (fcutest.test_index)
            {
                case 0:
                case 1:
                case 2:
                    measure_rule = $"Fault 전압 측정{Environment.NewLine}" +
                                    $"Fire 전압 측정{Environment.NewLine}" +
                                    $"Lamp 육안 검사{Environment.NewLine}{" "}";
                    break;
                case 3:
                    measure_rule = $"화재 반응시간 측정{Environment.NewLine}{" "}";
                    break;
                case 4:
                    measure_rule = $"전류 & 전력 측정{Environment.NewLine}{" "}";
                    break;
                case 5:
                case 6:
                case 7:
                    measure_rule = $"Fire 전압 측정{Environment.NewLine}" +
                                    $"Lamp 육안 검사{Environment.NewLine}{" "}";
                    break;
            }
            
            table.AddCell(new Cell(new Phrase($"측정방법 {Environment.NewLine}{" "}", font12)) { HorizontalAlignment = 1 , VerticalAlignment = 5 });
            table.AddCell(new Cell(new Phrase(measure_rule, font12)) { HorizontalAlignment = 1 , VerticalAlignment = 5 });

            table.AddCell(new Cell(new Phrase($"시험결과 {Environment.NewLine}{" "}", font12)) { HorizontalAlignment = 1 , VerticalAlignment = 5 });
            Cell parentCell = new Cell();

            Table nestedTable = new Table(2);
            nestedTable.Width = 100f;
            nestedTable.SetWidths(new int[] { 30, 70 }); // 자식 테이블의 열 비율

            nestedTable.AddCell(new Cell(new Phrase($"{fcutest.SMALL_TESTs[0].small_tag} {Environment.NewLine}{" "}", font12)) { HorizontalAlignment = 1, VerticalAlignment = 5 });
            nestedTable.AddCell(new Cell(new Phrase($"{fcutest.test_result1} {Environment.NewLine}{" "}", font12)) { HorizontalAlignment = 1, VerticalAlignment = 5 });

            if (!fcutest.SMALL_TESTs[1].small_tag.Contains("미실시"))
            {
                nestedTable.AddCell(new Cell(new Phrase($"{fcutest.SMALL_TESTs[1].small_tag} {Environment.NewLine}{" "}", font12)) { HorizontalAlignment = 1, VerticalAlignment = 5 });
                nestedTable.AddCell(new Cell(new Phrase($"{fcutest.test_result2} {Environment.NewLine}{" "}", font12)) { HorizontalAlignment = 1, VerticalAlignment = 5 });
            }
            
            if (!fcutest.SMALL_TESTs[2].small_tag.Contains("미실시"))
            {
                nestedTable.AddCell(new Cell(new Phrase($"{fcutest.SMALL_TESTs[2].small_tag} {Environment.NewLine}{" "}", font12)) { HorizontalAlignment = 1, VerticalAlignment = 5 });
                nestedTable.AddCell(new Cell(new Phrase($"{fcutest.test_result3} {Environment.NewLine}{" "}", font12)) { HorizontalAlignment = 1, VerticalAlignment = 5 });
            }
            
            if (!fcutest.SMALL_TESTs[3].small_tag.Contains("미실시"))
            {
                nestedTable.AddCell(new Cell(new Phrase($"{fcutest.SMALL_TESTs[3].small_tag} {Environment.NewLine}{" "}", font12)) { HorizontalAlignment = 1, VerticalAlignment = 5 });
                nestedTable.AddCell(new Cell(new Phrase($"{fcutest.test_result4} {Environment.NewLine}{" "}", font12)) { HorizontalAlignment = 1, VerticalAlignment = 5 });
            }
            parentCell.AddElement(nestedTable);

            table.AddCell(parentCell);

            doc.Add(table);
        }
    }
}
