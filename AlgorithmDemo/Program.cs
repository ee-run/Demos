using AlgorithmDemo.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using static AlgorithmDemo.PackHelper;

namespace AlgorithmDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            int GoodsCount = 8; // 商品数量
            List<Box> boxList = new List<Box>(); // 准备箱子集合
            List<Box> tempBoxList = null;

            List<Goods> goodsList;
            goodsList = GetGoodsList(GoodsCount); // 准备商品集合

            int totalCount = goodsList.Count;

            // 过滤不能装入大箱子的商品
            FilterGoodsList(ref goodsList, CreateBox(2, 2, 2).BoxPackPanel);

            var NoSuitGoodsList = goodsList.Where(x => !x.isCanPack).ToList();

            if (NoSuitGoodsList.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("不能装入箱子的商品：");
                NoSuitGoodsList.ForEach(x =>
                {
                    Console.WriteLine(string.Format("商品长：{0}，宽：{1}，高：{2}，体积：{3}", x.Length, x.Width, x.Hight, x.Volume));
                });
            }

            goodsList = goodsList.Where(x => x.isCanPack).ToList();

            if (goodsList == null || goodsList.Count <= 0)
            {
                Console.WriteLine("过滤可放商品数量为空。");
                Console.ReadLine();
                return;
            }

            // 排序
            SortVolume(ref goodsList);

            Console.WriteLine();
            foreach (var item in goodsList)
            {
                Console.WriteLine(string.Format("类型为{0}，商品单号：{1},长度：{2} ,宽度：{3}，高度：{4} ,体积：{5}  ",
                    "SortVolume", item.HawbCode, item.Length, item.Width, item.Hight, item.Volume));
            }

            Console.WriteLine();
            Console.WriteLine(string.Format("总生产{0}个商品，过滤后商品总数：{1}", totalCount, goodsList.Count));

            boxList = new List<Box>();

            SetGoodsUnPacked(goodsList);

            tempBoxList = Pack2BoxesByType(PutType.Normal, goodsList, boxList);

            if (tempBoxList == null || tempBoxList.Count <= 0)
            {
                Console.WriteLine("Error:未计算出结果！");
                Console.ReadLine();
                return;
            }

            List<Box> resultBoxList = tempBoxList;

            if (resultBoxList.Count <= 0)
            {
                Console.WriteLine("所有商品均不可放入箱子！");
                Console.ReadLine();
                return;
            }

            Console.WriteLine();

            // 打印信息
            PrintInfo(resultBoxList);

            var xmlStrList = PrepareXmlStringList(resultBoxList);

            foreach (var xmlStr in xmlStrList)
            {
                Console.WriteLine();
                Console.WriteLine("-------------------------------------");
                Console.WriteLine(xmlStr);
                Send2ServerByAPI(xmlStr, ApiUrl);
                Console.WriteLine("-------------------------------------");
            }

            Console.ReadLine();
        }

        /// <summary>
        /// 服务端开启的URL
        /// </summary>
        public static readonly string ApiUrl = "http://192.168.200.106:13000/Api/ ";

        /// <summary>
        /// 通过http异步将xml字符发送到服务端
        /// </summary>
        /// <param name="xmlStr">xml字符</param>
        /// <param name="sApiUrl">服务端开启的URL</param>
        /// <returns></returns>
        public static bool Send2ServerByAPI(string xmlStr, string sApiUrl)
        {
            if (string.IsNullOrWhiteSpace(xmlStr) || string.IsNullOrWhiteSpace(sApiUrl)) return false;

            try
            {
                //实例化
                WebClient client = new WebClient();

                //参数转流
                byte[] bytearray = Encoding.UTF8.GetBytes(xmlStr);
                //采取POST方式必须加的header，如果改为GET方式的话就去掉这句话即可
                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");//长度
                client.Headers.Add("ContentLength", bytearray.Length.ToString());

                ////上传，post方式，并接收返回数据（这是同步，需要等待接收返回值）
                //byte[] responseData = client.UploadData(sApiUrl, "POST", bytearray);
                ////释放
                //client.Dispose();
                ////处理返回数据（一般用json）
                //string srcString = Encoding.UTF8.GetString(responseData);

                //绑定事件，为了获取返回值
                client.UploadDataCompleted += new UploadDataCompletedEventHandler(UploadDataCallback2);
                //这里是url地址
                Uri uri = new Uri(sApiUrl);
                //异步post提交，不用等待。
                client.UploadDataAsync(uri, "POST", bytearray);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 接收返回值的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void UploadDataCallback2(Object sender, UploadDataCompletedEventArgs e)
        {
            //接收返回值
            byte[] data = (byte[])e.Result;
            //转码
            string reply = Encoding.UTF8.GetString(data);
            //打印日志
            Console.WriteLine("返回数据：" + reply + "\n");
        }

        /// <summary>
        /// 生成xml字符
        /// </summary>
        /// <param name="resultBoxList"></param>
        /// <returns></returns>
        public static List<string> PrepareXmlStringList(List<Box> resultBoxList)
        {
            if (resultBoxList == null || resultBoxList.Count <= 0) return null;

            List<string> xmlList = new List<string>();
            List<CreateBoxEntity> entityList = new List<CreateBoxEntity>();
            string sTempStr = string.Empty;

            foreach (var box in resultBoxList)
            {
                GetEntityByPackPanel(box.BoxPackPanel, ref entityList);
            }

            if (entityList == null || entityList.Count <= 0) return null;

            foreach (var entity in entityList)
            {
                sTempStr = GetXmlStringByEntity(entity);

                if (string.IsNullOrWhiteSpace(sTempStr)) continue;

                xmlList.Add(sTempStr);
            }

            return xmlList;
        }

        /// <summary>
        /// 获取单个xml字符
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string GetXmlStringByEntity(CreateBoxEntity entity)
        {
            string context = string.Empty;
            try
            {
                XmlSerializer xmlnet = new XmlSerializer(typeof(NetWorkCommandEntity));
                XmlSerializer xmlcontext = new XmlSerializer(typeof(CreateBoxEntity));

                MemoryStream ms = new MemoryStream();
                XmlWriter xw = XmlWriter.Create(ms, new XmlWriterSettings() { Encoding = Encoding.UTF8 });
                xmlcontext.Serialize(xw, entity);
                context = Encoding.UTF8.GetString(ms.ToArray());
                NetWorkCommandEntity net = new NetWorkCommandEntity();
                net.CommandName = typeof(CreateBoxEntity).Name;
                net.Context = context;
                ms.Close();
                ms = new MemoryStream();
                xw = XmlWriter.Create(ms, new XmlWriterSettings() { Encoding = Encoding.UTF8 });
                xmlnet.Serialize(xw, net);
                context = Encoding.UTF8.GetString(ms.ToArray());

                return context;
            }
            catch (Exception ex)
            {
                return context;
            }
        }

        /// <summary>
        /// 根据类型计算箱子
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static List<Box> Pack2BoxesByType(PutType type, List<Goods> goodsList, List<Box> boxList)
        {
            // 装箱，返回箱子集合
            bool bPackSuccess = Pack2Boxes(goodsList, ref boxList, type);

            if (boxList == null) boxList = new List<Box>();
            boxList = boxList.Where(x =>
                                                    x.BoxPackPanel != null
                                                    && x.BoxPackPanel.GoodsList != null
                                                    && x.BoxPackPanel.GoodsList.Count > 0).ToList();

            return boxList;
        }

        static void PrintInfo(List<Box> boxList)
        {
            int a = 0;
            foreach (var box in boxList)
            {
                a++;
                Console.WriteLine("------------------------------------------------------");
                Console.WriteLine();
                Console.WriteLine(string.Format("第{0}个箱子", a));
                Console.WriteLine(string.Format("箱子长：{0},宽：{1}，高：{2}，体积：{3}", box.Length, box.Width, box.High, box.Volume));

                PrintInfoByPackPanel(box.BoxPackPanel);
            }
        }

        static void PrintInfoByPackPanel(PackPanel panel)
        {
            if (panel == null || panel.RemainPanelList == null || panel.RemainPanelList.Count <= 0) return;
            Console.WriteLine();
            Console.WriteLine(string.Format("容器长：{0}，宽：{1}，高：{2}，体积：{3}，商品数量：{4},  坐标：X:{5},Y:{6},Z:{7}",
                panel.Length, panel.Width, panel.High,
                panel.Volume, panel.GoodsList.Count,
                panel.PanelZeroPosition.X, panel.PanelZeroPosition.Y, panel.PanelZeroPosition.Z));

            if (panel.GoodsList != null)
            {
                foreach (var good in panel.GoodsList)
                {
                    Console.WriteLine(string.Format(@"商品：单号：{0},长：{1}，宽：{2}，高：{3}，体积：{4} ,  坐标：X:{5},Y:{6},Z:{7} ，正对面长宽高为：对视长：{8}，对视宽：{9}，对视高：{10}",
                                        good.HawbCode, good.Length, good.Width, good.Hight, good.Volume,
                                       panel.GoodsZeroPosition.X, panel.GoodsZeroPosition.Y, panel.GoodsZeroPosition.Z,
                                        panel.GoodsZeroPosition.Length, panel.GoodsZeroPosition.Width, panel.GoodsZeroPosition.Hight));
                }
            }

            foreach (var item in panel.RemainPanelList)
            {
                PrintInfoByPackPanel(item);
            }
        }

        static void GetEntityByPackPanel(PackPanel panel, ref List<CreateBoxEntity> entityList)
        {
            if (panel == null || panel.RemainPanelList == null || panel.RemainPanelList.Count <= 0) return;
            if (panel.GoodsList != null)
            {
                foreach (var good in panel.GoodsList)
                {
                    CreateBoxEntity entity = new CreateBoxEntity();
                    entity.x = good.GoodZeroPosition.X;
                    entity.y = good.GoodZeroPosition.Y;
                    entity.z = good.GoodZeroPosition.Z;

                    entity.lenght = good.GoodZeroPosition.Length;
                    entity.width = good.GoodZeroPosition.Width;
                    entity.height = good.GoodZeroPosition.Hight;

                    entityList.Add(entity);
                }
            }

            foreach (var item in panel.RemainPanelList)
            {
                GetEntityByPackPanel(item, ref entityList);
            }
        }


    }
}
