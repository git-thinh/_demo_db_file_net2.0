
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;

using mshtml;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection; 

namespace ctext
{
    public class App
    {

        static App()
        {

            //////////////////////////////////////////////////////
            // Assembly resolve get stream component from resource
            AppDomain.CurrentDomain.AssemblyResolve += (se, ev) =>
            {
                string comName = ev.Name.Split(',')[0];
                Assembly asm = null;
                try
                {
                    string resourceName = @"Component\" + comName + ".dll";
                    var assembly = Assembly.GetExecutingAssembly();
                    resourceName = assembly.GetName().Name + "." + resourceName.Replace(" ", "_").Replace("\\", ".").Replace("/", ".");
                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            byte[] buffer = new byte[stream.Length];
                            using (MemoryStream ms = new MemoryStream())
                            {
                                int read;
                                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                                    ms.Write(buffer, 0, read);
                                buffer = ms.ToArray();
                            }
                            asm = Assembly.Load(buffer);
                        }
                    }

                    ///////////////////////////////////////////
                    /////// Update message log update component
                    ////if (asm == null)
                    ////    listComLog.Add(string.Format("System load component: {0} fail", comName));
                    ////else
                    ////    listComLog.Add(string.Format("System load component: {0} successfully", comName));
                }
                catch { }
                return asm;
            };
        }

        //public System.Windows.Forms.HtmlDocument GetHtmlDocument(string html)
        //{
        //    WebBrowser browser = new WebBrowser();
        //    browser.ScriptErrorsSuppressed = true;
        //    browser.DocumentText = html;
        //    browser.Document.OpenNew(true);
        //    browser.Document.Write(html);
        //    browser.Refresh();
        //    return browser.Document;
        //}

        static string getContent(string url, string text)
        {
            string[] a = url.Split('/');
            string dom = a[2];
            a = dom.Split('.');
            if (a[a.Length - 2].Length > 3) dom = a[a.Length - 2] + "." + a[a.Length - 1];

            var adom = listTagSplit.Where(x => x.Domain == dom).ToArray();
            if (adom.Length > 0)
            {
                oTagSplit rs = adom[0];

                if (rs != null && !string.IsNullOrEmpty(rs.TrimLeft))
                {
                    a = text.Split(new string[] { br }, StringSplitOptions.None)
                        .Select(x => x.Trim()).Where(x => x != "").ToArray();
                    text = string.Join(br, a);

                    int pcut = text.IndexOf(rs.TrimLeft);
                    if (pcut > 0)
                        text = text.Substring(pcut + rs.TrimLeft.Length);

                    pcut = text.IndexOf(rs.TrimRight);
                    if (pcut > 0)
                        text = text.Substring(0, pcut);

                    var ls = text.Split(new string[] { br }, StringSplitOptions.None)
                        .Select(x => x.Trim()).Where(x => x != "").ToList();
                    if (rs.TrimRowLeft > 0)
                        for (int k = 0; k < rs.TrimRowLeft; k++)
                            ls.RemoveAt(0);
                    if (rs.TrimRowRight > 0)
                        for (int k = 0; k < rs.TrimRowRight; k++)
                            ls.RemoveAt(ls.Count - 1);
                    text = string.Join(br, ls.ToArray());
                }
            }
            return text.Trim();
        }

        static string br = Environment.NewLine;
        static List<oTagSplit> listTagSplit = new List<oTagSplit>()
        {
            new oTagSplit(){ Domain = "vnexpress.net", TrimLeft = "GMT+7" + br, TrimRight = br + "Xem thêm:" + br, TrimRowLeft = 2, TrimRowRight = 1 },
            new oTagSplit(){ Domain = "zing.vn", TrimLeft = br + "Nhịp sống" + br, TrimRight = "Bình luận" + br, TrimRowLeft = 0, TrimRowRight = 2 }, 
        };

        public static void run1()
        {
            string url = "http://vnexpress.net/";
            //url = "http://news.zing.vn/iphone-6-32-gb-ve-viet-nam-thang-sau-gia-10-trieu-dong-post725522.html";
            //url = "http://vnexpress.net/tin-tuc/khoa-hoc/nhung-hon-dao-ky-la-hinh-trai-tim-3549883.html";
            //url = "http://suckhoe.vnexpress.net/photo/thuoc-va-thuc-pham/9-loai-cay-trong-trong-nha-lam-gia-vi-va-thuoc-chua-benh-3529425.html";
            //url = "http://suckhoe.vnexpress.net/tin-tuc/cac-benh/thuoc-va-thuc-pham/tri-ho-bang-la-he-chung-duong-phen-3537245.html";

            WebClient objWebClient = new WebClient();
            byte[] buf = objWebClient.DownloadData(url);
            string htm = Encoding.UTF8.GetString(buf);
            //string htm = File.ReadAllText("demo2.html");


            htm = Regex.Replace(htm, @"<script[^>]*>[\s\S]*?</script>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            htm = Regex.Replace(htm, @"<head[^>]*>[\s\S]*?</head>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            htm = Regex.Replace(htm, @"<style[^>]*>[\s\S]*?</style>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            htm = Regex.Replace(htm, @"<iframe[^>]*>[\s\S]*?</iframe>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            htm = Regex.Replace(htm, @"<form[^>]*>[\s\S]*?</form>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            htm = Regex.Replace(htm, @"<ul[^>]*>[\s\S]*?</ul>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            var ps = Regex.Matches(htm, "<img.+?src=[\"'](.+?)[\"'].*?", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            foreach (Match mi in ps)
            {
                string img = mi.ToString(), src = mi.Groups[1].Value;
                //string id = " " + Guid.NewGuid().ToString() + " ";
                string id = " {img" + src + "img} ";

                int p = htm.IndexOf(img);
                if (p > 0)
                {
                    string s0 = htm.Substring(0, p),
                        s1 = htm.Substring(p + img.Length, htm.Length - (p + img.Length));
                    int pend = s1.IndexOf(">");
                    if (pend != -1) s1 = s1.Substring(pend + 1);
                    htm = s0 + id + s1;
                }
                //htm = htm.Replace(img, id); 
            }


            // Loading HTML Page into DOM Object
            IHTMLDocument2 doc = new HTMLDocumentClass();
            doc.clear();
            doc.write(htm);
            doc.close();


            //////var htmlDoc = new HTMLDocumentClass();
            //////var ips = (IPersistStreamInit)htmlDoc;
            //////ips.InitNew(); 
            //////var doc = htmlDoc.createDocumentFromUrl(url, "null"); 
            //' The following is a must do, to make sure that the data is fully load.
            while (doc.readyState != "complete")
            {
                //This is also a important part, without this DoEvents() appz hangs on to the “loading”
                //System.Windows.Forms.Application.DoEvents(); 
                ;
            }

            string js = "function(){ return 1; }";
            //js = "functionName(param1);";
            js = "(function() { return confirm('Continue?'); })()";

            //doc.parentWindow.execScript(js, "javascript");

            //doc.parentWindow.alert("dsdsadsad");

            //HTMLWindow2 iHtmlWindow2 = (HTMLWindow2)doc.Script;
            //var rs = iHtmlWindow2.execScript(js, "javascript");

            //mshtml.IHTMLWindow2 win = doc.parentWindow as IHTMLWindow2;
            //win.execScript(js, "javascript");

            string source = doc.body.outerHTML; ;
            string text = doc.body.innerText;

            string content = getContent(url, text);

            List<string> listImg = new List<string>();
            var ps2 = Regex.Matches(text, "{img(.+?)img}", RegexOptions.IgnoreCase);
            foreach (Match mi in ps2)
            {
                string img = mi.ToString();
                listImg.Add(img);
            }

            ////var it = ((mshtml.HTMLDocument)doc).all.Cast<mshtml.IHTMLElement>().ToList();
            //var ls = doc.Where(x => x.Name == "q").ToList();
            //foreach (var li in ls) li.SetAttribute("value", "dây hàn nhôm");
            //doc.First(p => p.TagName == "FORM").InvokeMember("Submit");


            ////List<mshtml.IHTMLInputElement> allInput = doc.all.OfType<mshtml.IHTMLInputElement>().ToList();
            //((mshtml.IHTMLElement)doc.all.item("q")).setAttribute("value", "dây hàn nhôm");
            //((mshtml.IHTMLFormElement)doc.all.item("f")).submit();

            Console.WriteLine(content);
            Console.ReadKey();
        }

        string t2 = @" 
Phiên bản: VnExpress International – Vietnam and ASEAN news 
© Copyright 1997 - VnExpress.net, All rights reserved.
® VnExpress giữ bản quyền nội dung trên website này.
Hotline:
0123.888.0123 (Hà Nội)
0129.233.3555 (TP HCM) 
 
{imghttp://st.f1.vnecdn.net/responsive/i/v41/graphics/logo.pngimg} 
{imghttp://st.f1.vnecdn.net/responsive/i/v41/icons/icon_videomenu.gifimg} 
 
 
{imghttp://st.f1.vnecdn.net/responsive/i/v41/graphics/img_logo_vne_web.gifimg} 
{imghttp://st.f1.suckhoe.vnecdn.net/i/v1/icons/icon_swich_en.gifimg} 24h qua RSS  

  
Thứ ba, 17/1/2017 | 12:51 GMT+7
| 
Thứ ba, 17/1/2017 | 12:51 GMT+7
9 loại cây trồng trong nhà làm gia vị và thuốc chữa bệnh 
Rau răm, rau mùi, húng quế, húng chanh, gừng... là những cây gia vị có khả năng chữa bệnh, theo sách Bài thuốc hay từ cây thuốc quý của tác giả Võ Văn Chi.
{imghttp://img.f42.suckhoe.vnecdn.net/2017/01/17/cay-gia-vi-chua-benh12-1484618438_660x0.pngimg}   
Húng chanh là một loại rau gia vị quen thuộc trong bữa ăn của người Việt Nam. Cây này còn có tên gọi khác là rau tần. Húng chanh vị chua the, thơm hăng, tính ấm vào phế, công dụng giải cảm, tiêm đờm, khử độc và các chứng bệnh cảm cúm, lạnh phổi...
Để chữa hen suyễn lấy 12 g lá húng chanh, 10 g lá tía tô, rửa sạch, sắc uống. Khi uống nên kiêng ăn đồ chiên xào, thức uống lạnh, hải sản.
Chữa ho cho trẻ: Húng chanh kết hợp với lá hẹ, mật ong. Cả 3 thứ đem hấp, uống rất sạch miệng lại đỡ ho.
Bị rết, bọ cạp cắn, ong đốt, dùng lá húng chanh rửa sạch, thái nhỏ hoặc nhai kỹ, cho thêm ít muối vào rồi đắp lên vết thương.
 
{imghttp://img.f42.suckhoe.vnecdn.net/2017/01/17/cay-gia-vi-chua-benh13-1484618438_660x0.jpgimg}   
Mùi tàu còn gọi là ngò gai, ngò tàu, vị the, tính ấm, mùi thơm hắc, khử thấp nhiệt, thanh uế, mạnh tì vị, kích thích tiêu hoá...
Khi bị đầy hơi, ăn không tiêu dùng 50 g rau mùi tàu cắt dài 4 cm, kết hợp với gừng tươi đập dập. Cho 2 thứ vào siêu đất, đổ chừng 400 ml nước sắc lại còn 200 ml. Chia làm 2 lần uống cách nhau 3 giờ.
Sốt nhẹ: Mùi tàu 30 g, thịt bò tươi 50 g, vài lát gừng tươi. Tất cả thái nhỏ, nấu chín với 600 ml nước. Ăn nóng, khi ăn thêm ít tiêu bột, trùm chăn kín người cho ra mồ hôi.
 
{imghttp://img.f44.suckhoe.vnecdn.net/2017/01/17/cay-gia-vi-chua-benh11-1484618437_660x0.jpgimg}   
Rau răm còn gọi là thuỷ liễu, hương lục. Cây có vị cay, tính ấm không độc, dùng để trị đau bụng lạnh, chữa rắn cắn, chàm ghẻ, mụn trĩ, kích thích tiêu hoá, kém ăn, làm dịu tình dục. Dùng tươi, không qua chế biến.
Để trị chứng tiêu hoá kém, mỗi ngày dùng từ 15 đến 20 g thân và lá rau răm tươi, rửa sạch, giã nát hoặc xay nhuyễn, vắt lấy nước cốt uống.
Trị say nắng: Rau răm kết hợp với sâm bố chính tẩm nước gừng 30 g, đinh lăng 16 g, mạch môn 10 g. Tất cả đem sao vàng, sắc với 600 ml nước cô lại 300 ml. Uống hết trong ngày, chia làm 2 lần.
 
{imghttp://img.f43.suckhoe.vnecdn.net/2017/01/17/cay-gia-vi-chua-benh16-1484618445_660x0.jpgimg}   
Thì là (hay thìa là) còn gọi là thời la, đông phong. Đây là một vị thuốc rất thông dụng trong Đông y. Hạt thì là vị cay, tính ẩm, không độc, điều hoà món ăn, bổ thận, mạnh tì, tiêu trướng, trị đau bụng và đau răng.
Người bị tiểu rắt (tiểu són): lấy một nắm thì là tẩm với nước muối, sao vàng, tán thành bột. Khi dùng, lấy bánh dày quết với bột thì là ăn. Phương thuốc này rất hiệu nghiệm đối với những người bị tiểu không có chừng mực hay bị đau buốt.
Những người đi rừng lâu ngày bị sốt rét ác tính, sẽ rất nguy hiểm đến tính mạng. Để trị chứng này, dân gian dùng hạt thì là tươi, giã, vắt lấy nước uống hay phơi khô hạt, tán thành bột, sắc lấy nước uống rất công hiệu.
 
{imghttp://img.f44.suckhoe.vnecdn.net/2017/01/17/cay-gia-vi-chua-benh17-1484618447_660x0.jpgimg}   
Tía tô còn gọi là tử tô, xích tô, bạch tô. Toàn cây dùng làm thuốc chữa bệnh. Lá tía tô vị cay, tính ấm, làm ra mồ hôi, tiêu đờm. Quả tác dụng khử đờm, hen suyễn, tê thấp. Hạt chữa táo bón, mộng tinh...
Để trị chứng cảm cúm không ra mồ hôi, ho nặng: Nấu cháo gạo, cho 10 g lá tía to thái chỉ vào, ăn nóng, đắp chăn kín cho ra mồ hôi, bệnh sẽ khỏi. Hoặc dùng từ 15 đến 20 g lá tía tô tươi, giã nát, đun sôi với nước để uống.
Những người ăn hải sản bị dị ứng, mẩn đỏ, dùng một nắm lá tía tô giã hay xay lấy nước uống, bã xát vào chỗ mẩn ngứa. Hoặc có thể kết hợp với sinh khương 8 g, gừng tươi 8 g, cam thảo 4 g đun với 600 ml, cô lại còn 200 ml. Uống lúc nóng, chia thành 3 lần trong ngày.
Chữa táo bón: Dùng 15 g hạt tía tô,15 g hạt hẹ giã nhỏ, trộn với nhau chế thêm 200ml nước, lọc lấy nước cốt để nấu cháo ăn rất tốt. Món này đặc biệt là thích hợp trị chứng táo bón lâu ngày ở các cụ già và người suy yếu.
 
{imghttp://img.f41.suckhoe.vnecdn.net/2017/01/17/cay-gia-vi-chua-benh18-1484618449_660x0.jpgimg}   
Tinh chất bạc hà được sử dụng nhiều trong các thực phẩm giải khát như trà bạc hà, siro... Dầu bạc hà còn được trộn trong kem đánh răng, xà phòng tắm, dầu gội đầu. Loại cây này có vị cay, mát, trị cảm mạo phong nhiệt, nhức đầu và các vấn đề về tiêu hóa. Uống trà bạc hà cũng giúp thư giãn tinh thần.
 
{imghttp://img.f44.suckhoe.vnecdn.net/2017/01/17/cay-gia-vi-chua-benh14_660x0.jpgimg}   
Rau mùi còn được gọi là ngò ta, hương tuy, vị cay, tính ấm, không độc. Cây thuốc này giúp tiêu thức ăn, trị phong tà, thông đại tiểu tiện, trị các chứng đậu, sởi khó mọc, phá mụn độc.
Người bị kiết lị, dùng một vốc hạt mùi sao vàng, tán nhỏ. Khi dùng, lấy ra 7 đến 8 g pha với nước, ngày uống 2 lần. Nếu lị ra máu thì uống với nước đường, lị đàm thì uống với nước gừng.
Người bị loét niêm mạc lưỡi, dùng rau mùi kết hợp với rau húng chanh. Tất cả ngâm với nước muối pha loãng rồi nhai kỹ, nuốt lấy nước từ từ.
 
{imghttp://img.f42.suckhoe.vnecdn.net/2017/01/17/hung-que-1484618937_660x0.jpgimg}   
Húng quế có họ giống cây bạc hà. Loài rau này có mùi thơm đặc trưng và giúp tăng cường sức khỏe. Húng quế còn là một loại thảo dược giúp chữa trị đầy hơi, ăn không ngon miệng, làm lành các vết đứt, trầy. Nên sử dụng lá húng quế non rất tốt cho sức khỏe.
 
{imghttp://img.f41.suckhoe.vnecdn.net/2017/01/17/gung-1484619095_660x0.jpgimg}   
Gừng chống buồn nôn, đặc biệt rất hữu hiệu cho các bà bầu trong giai đoạn ốm nghén. Vị thuốc này còn giúp giảm đau và sưng ở những người bị viêm khớp, chống chứng đau nửa đầu nhờ cơ chế chẹn chất gây viêm prostaglandin. Gừng còn hỗ trợ hoạt động của đường tiêu hóa, thúc đẩy dịch tiêu hóa và trung hòa axit cũng như làm giảm co thắt ruột. Lưu ý: Người bị bệnh về gan, dạ dày, trĩ... không nên dùng gừng.
 
Thi Trân
Xem thêm: 
Thực phẩm có lợi cho sức khỏe 
Xem thêm 
 
 
Ý kiến bạn đọc (0) 
Mới nhất | Quan tâm nhất
Mới nhấtQuan tâm nhất
 
Xem thêm
 Tags
cây
gia vị
thuốc
cây trồng trong nhà
Tin khác
Tư vấn
Xuất tinh tuổi dậy thì có phải bệnh? 
{imghttp://img.f42.suckhoe.vnecdn.net/2017/03/04/Mong-tinh-nhieu-vao-ban-dem-co-9893-3147-1488589337_180x108.jpgimg} 
Em 19 tuổi, trong lúc ngủ thường bị xuất tinh không tự chủ, trung bình mỗi tháng xảy ra từ 4 đến 5 lần.
Tư vấn đã thực hiện 
{imghttp://st.f1.suckhoe.vnecdn.net/i/v1/graphics/tuvan/chongdoc.jpgimg} 
{imghttp://st.f1.suckhoe.vnecdn.net/i/v1/graphics/tuvan/giamcan.jpgimg} 
{imghttp://st.f1.suckhoe.vnecdn.net/i/v1/graphics/tuvan/thankinh.jpgimg} 
{imghttp://st.f1.suckhoe.vnecdn.net/i/v1/graphics/tuvan/jex.jpgimg} 
{imghttp://st.f1.suckhoe.vnecdn.net/i/v1/graphics/tuvan/wit.jpgimg} 
{imghttp://st.f1.suckhoe.vnecdn.net/i/v1/graphics/tuvan/alipas.jpgimg} 
Câu lạc bộ bệnh nhân
Đo chỉ số BMI
Tìm hiểu 


 
Xem kết quả 
Đo chỉ số Calo
Tìm hiểu 



 Giới tính Nam Nữ 
 Hoạt động Thụ động Nhẹ: 1-3 lần/1 tuần Trung bình: 3-5 lần/1 tuần Năng động: 6-7 lần/1 tuần Rất tích cực: Trên 7 lần/1 tuần 
 Tập luyện Nặng Trung bình Nhẹ 
Xem kết quả 
Trang chủ 
Đặt VnExpress làm trang chủ Đặt VnExpress làm trang chủ Về đầu trang 
 
{imghttp://st.f1.vnecdn.net/responsive/i/v41/graphics/img_logo_vne.jpgimg} 
© Copyright 1997 VnExpress.net, All rights reserved.
® VnExpress giữ bản quyền nội dung trên website này.
VnExpress tuyển dụng Liên hệ quảng cáo / Liên hệ Tòa soạn
Thông tin Tòa soạn: 0123.888.0123 (HN) - 0129.233.3555 (TP HCM)
{imghttp://st.f1.vnecdn.net/responsive/i/v41/icons/hotline.gifimg} Liên hệ Tòa soạn
{imghttp://st.f1.vnecdn.net/responsive/i/v41/icons/icon_thongtintoasoan.gifimg} Thông tin Tòa soạn
0123.888.0123 (HN) 
0129.233.3555 (TP HCM)
© Copyright 1997 VnExpress.net, All rights reserved.
® VnExpress giữ bản quyền nội dung trên website này.
 
 
 ";

        string t1 = @" 
Phiên bản: VnExpress International – Vietnam and ASEAN news 
Copyright 1997 VnExpress, All rights reserved
VnExpress giữ bản quyền nội dung trên website này.
Hotline:
0123.888.0123 (Hà Nội) 
0129.233.3555 (TP HCM) 
{imghttp://st.f1.vnecdn.net/responsive/i/v41/graphics/logo.pngimg} 
{imghttp://st.f1.vnecdn.net/responsive/i/v41/icons/icon_videomenu.gifimg} 
 
 
{imghttp://st.f1.vnecdn.net/responsive/i/v41/graphics/img_logo_vne_web.gifimg} 
VnExpress International – Vietnam and ASEAN news 24h qua RSS  
Thứ sáu, 3/3/2017 | 15:00 GMT+7 
| 
Thứ sáu, 3/3/2017 | 15:00 GMT+7
Những hòn đảo kỳ lạ hình trái tim 
Đảo Tavarua, đảo Galesnjak, đảo Tupai, là ba trong số những hòn đảo kỳ lạ trên thế giới có hình dạng giống trái tim.
Đảo Tavarua, Fiji
{imghttp://img.f29.vnecdn.net/2017/03/03/1-8159-1488526055.jpgimg} 
Hòn đảo Tavarua khi nhìn từ trên cao. Ảnh: Tyler Rooke.

 
Đảo Tavarua hình trái tim 
Tavarua là một trong những hòn đảo hình trái tim nổi tiếng nhất thế giới tại Fiji. Hòn đảo có diện tích khoảng 120.000m2 và được bao quanh bởi những rạn san hô ngầm, theo Mother Nature Network.
Đảo trái tim trên hồ Mascardi
{imghttp://img.f29.vnecdn.net/2017/03/03/2-1443-1488526055.jpgimg} 
Đảo Corazon được bao phủ bởi cây cối xanh tươi và rậm rạp. Ảnh: Sunsinger.

Đảo Corazon trông giống trái tim khi nhìn từ thung lũng phía hồ Mascardi
Mascardi là một hồ nước nằm trong Công viên Quốc gia Nahuel Huapi, phía bắc Patagonia, Argentina. Thung lũng xung quanh hồ được cây cối bao phủ. Hồ Mascardi có hình dạng chữ V. Ở đáy chữ V là đảo Corazon.
Rạn san hô trái tim, Australia
{imghttp://img.f29.vnecdn.net/2017/03/03/3-9468-1488526055.jpgimg} 
Rạn san hô trái tim. Ảnh: Tanya Puntti.

Hình dạng của rạn san hô trái tim khi nhìn từ máy bay
Rạn san hô tự nhiên thuộc quần đảo Whitsunday, Australia, có hình dạng trái tim gần như hoàn hảo. Việc tiếp cận rạn san hô này khá khó khăn do nó đang được bảo tồn.
Đảo Galesnjak, Croatia
{imghttp://img.f29.vnecdn.net/2017/03/03/4-4567-1488526055.jpgimg} 
Đảo Galesnjak được biết đến từ thế kỷ 19. Ảnh: Istock.

 
Quan sát đảo Galesnjak nhờ ứng dụng Google Earth
Đảo Galesnjak, hay còn gọi là đảo Tình Nhân, nằm gần bờ biển Croatia. Hòn đảo này xuất hiện trên bản đồ địa lý từ đầu thế kỷ 19, nhưng chỉ chính thức được công nhận vào năm 2009 khi mọi người nhìn thấy nó có hình dạng độc đáo, giống trái tim trên biển Adriatic nhờ ứng dụng Google Earth.
Bãi biển xung quanh đảo Galesnjak dài khoảng 1,55 km. Đảo có hai đỉnh núi, đỉnh cao nhất nằm trên mực nước biển 36 m. Hiện nay, đảo không có người ở mà chỉ có các cây mọc tự nhiên.
Đảo Tupai, Polynesia
{imghttp://img.f29.vnecdn.net/2017/03/03/5-9077-1488526055.jpgimg} 
Đảo Tupai ở phía nam Thái Bình Dương. Ảnh: Worldtoptop.

 
Đảo Tupai khi quan sát trên cao bằng máy bay trực thăng        
Tupai là một đảo san hô hình trái tim ở Polynesia, phía nam Thái Bình Dương. Hòn đảo có diện tích khoảng 11 km2. Nó được biết đến với những bãi biển cát trắng, hàng dừa xanh, nước biển trong vắt.
Trên đảo gần như không có ai sinh sống ngoài một số công nhân đồn điền dừa. Tupai cũng có sân bay nhưng thuộc quyền sở hữu tư nhân và ít khi được dùng đến.
Đảo Makepeace, Australia
{imghttp://img.f29.vnecdn.net/2017/03/03/6-6385-1488526055.jpgimg} 
Hòn đảo Makepeace thuộc sở hữu của tỷ phú Mỹ Richard Branson. Ảnh: Makepeace Island.

 
Khung cảnh trên đảo Makepeace, Australia
Đảo Makepeace nằm trên sông Noosa ở Queensland, Australia, có hình dạng trái tim tuyệt đẹp. Nó thuộc sở hữu của tỷ phú Richard Branson. Hòn đảo được cải tạo, trở thành khu nghỉ dưỡng vào năm 2011.
Lê Hùng
Xem thêm: 
Xem nhiều nhất
 
Ý kiến bạn đọc (0) 
Mới nhất | Quan tâm nhất
Mới nhấtQuan tâm nhất
Xem thêm
 Tags
đảo
đảo kỳ lạ
đảo hình trái tim
rạn san hô
Thái Bình Dương
Google Earth
 
Tin khác
Công nghệ mới
 
Căn phòng hỗ trợ sạc không dây cho 10 thiết bị cùng lúc {imghttp://st.f1.vnecdn.net/responsive/i/v15/graphics/img_blank.gifimg}  
{imghttp://img.f32.vnecdn.net/2017/03/01/can-phong-ho-tro-sac-khong-day-cho-10-thiet-bi-cung-luc-1488361787_180x108.jpgimg} 
Phòng thí nghiệm Disney Research thành công biến toàn bộ căn phòng thành một trạm sạc không dây. 
Môi trường
 
Dự án 500 tỷ USD đóng băng Bắc Cực chống biến đổi khí hậu {imghttp://st.f1.vnecdn.net/responsive/i/v15/graphics/img_blank.gifimg}  
{imghttp://img.f31.vnecdn.net/2017/02/25/Anh1-1488009432_180x108.jpgimg} 
Các nhà khoa học Mỹ đề xuất ý tưởng sử dụng 10 triệu máy bơm khổng lồ, mang nước biển bên dưới lớp băng lên trên bề mặt nhằm làm {imghttp://st.f1.vnecdn.net/responsive/i/v41/icons/icon_more2.gifimg} 
Trang chủ 
Đặt VnExpress làm trang chủ  Đặt VnExpress làm trang chủ Về đầu trang 
 
{imghttp://st.f1.vnecdn.net/responsive/i/v41/graphics/img_logo_vne.jpgimg} 
Copyright 1997 VnExpress, All rights reserved
VnExpress giữ bản quyền nội dung trên website này.
VnExpress tuyển dụng Liên hệ:Quảng cáo / Tòa soạn
Đường dây nóng: 0123.888.0123 (HN) - 0129.233.3555 (TP HCM)
{imghttp://st.f1.vnecdn.net/responsive/i/v41/graphics/img_blank.gifimg} Liên hệ Tòa soạn
{imghttp://st.f1.vnecdn.net/responsive/i/v41/graphics/img_blank.gifimg} Thông tin Tòa soạn
0123.888.0123 (HN) 
0129.233.3555 (TP HCM)
Thuộc Bộ Khoa học Công nghệ. Copyright 1997 VnExpress, All rights reserved
VnExpress giữ bản quyền nội dung trên website này.
 
  ";
    }
         
}
