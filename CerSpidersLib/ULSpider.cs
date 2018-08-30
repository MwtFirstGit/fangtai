using ExtractLib;
using HttpToolsLib;
using ICerSpiderTaskLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CerSpidersLib
{
    public class ULSpider : CerSpiderBase
    {

        #region 公共变量声明
        /// <summary>
        /// 主页Url
        /// </summary>
        const String UL_Index_Url = "http://database.ul.com/cgi-bin/XYV/cgifind/LISEXT/1FRAME/srchres.html";
        /// <summary>
        /// 详情页Url
        /// </summary>
        const String UL_Details_Url = "http://database.ul.com{0}";
        /// <summary>
        /// 主页PostData
        /// </summary>
        const String UL_PostData = "query={0}";
        /// <summary>
        /// url参数队列
        /// </summary>
        static ConcurrentQueue<string> urldata_queue = new ConcurrentQueue<string>();

        #region 抽取或匹配相关
        /// <summary>
        /// 正则匹配详情Url参数
        /// </summary>
        const String reg_UL_Details = "BldLink\\((.*?)\\)";
        //匹配Remarks
        const String reg_remarks = "<title>(.*?)</title>";
        //匹配Brand
        const String reg_brand = "<NAMELINE>(.*?)</NAMELINE>";
        //匹配Product
        const String reg_produce = "<center>(.*?)</center>";
        //匹配type
        const String reg_type_s = "<RECCOMP>([\\s\\S]+?)</RECCOMP>";
        const String reg_type = "<p>(.*?)</p>";
        const String reg_type_details = "<TR><TH VALIGN=\"BOTTOM\"[\\s\\S]+?<TABLE WIDTH=\"";
        //匹配Fixed类型title
        const String reg_first_title = "<TR>([\\s\\S]+?)</TR>";
        const String reg_fixed_title = "<TH VALIGN.*?>(.*?)</TH>";
        const String reg_fixed_title_1 = "<TH.*?>(.*?)</TH>";
        //匹配Fixed类型详情
        const String reg_fixed_details = "<TD VALIGN.*?>(.*?)</TD>";
        const String reg_fixed_details_1 = "<TD VALIGN.*?>([\\s\\S]+?)</TD>";
        #endregion
        #endregion
        /// <summary>
        /// 构造函数
        /// </summary>
        public ULSpider() { this.CerType = 2; }

        /// <summary>
        /// 使用基类方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parms"></param>
        /// <returns></returns>
        public override void GetTask(object[] parms)
        {
            base.GetTask(parms);
        }

        /// <summary>
        /// 重写基类方法
        /// </summary>
        /// <param name="parms"></param>
        public override void RunTask(object[] parms)
        {
            #region 执行任务结构示例
            String cernum = String.Empty;
            while (CerQueue.TryDequeue(out cernum))
            {
                /*这里写执行任务相关代码
                 *
                 */
                Console.WriteLine($"UL证书号{cernum}开始");
                UL_Index(cernum,ref urldata_queue);
                string urldata = string.Empty;
                while(urldata_queue.TryDequeue(out urldata))
                {
                    Dictionary<String, String> updata = UL_Details(urldata, cernum);
                    //上传数据入队
                    UpLoadQueue.Enqueue(updata);
                    Thread.Sleep(1);
                }                
                Console.WriteLine($"UL证书号{cernum}完毕");                        
                Thread.Sleep(1);
            }
            #endregion
        }

        /// <summary>
        /// 访问详情页
        /// </summary>
        /// <param name="Certi_No"></param>
        public static Dictionary<String, String> UL_Details(String urldata, String Certi_No)
        {
            Dictionary<string, String> dirs = new Dictionary<string, string>();
            List<String> THlist = new List<string>();
            List<String> _THlist = new List<string>();
            List<String> _THlist_ = new List<string>();
            try
            {
                String html = "start";
                while (!html.Contains(Certi_No))
                {
                    html = HttpMethod.FastGetMethod(String.Format(UL_Details_Url, urldata));
                    Thread.Sleep(1);
                }

                //处理数据
                dirs.Add("CertNo", Certi_No);
                dirs.Add("Product", RegexMethod.GetSingleResult(reg_produce, html, 1));
                dirs.Add("Brand", RegexMethod.GetSingleResult(reg_brand, html, 1));
                dirs.Add("Remarks", RegexMethod.GetSingleResult(reg_remarks, html, 1));

                string types = RegexMethod.GetSingleResult(reg_type_s, html, 1);
                if (String.IsNullOrEmpty(types))
                {
                    types = RegexMethod.GetSingleResult(reg_type_details, html);
                }
                if (!String.IsNullOrEmpty(types))
                {
                    var typelist = RegexMethod.GetMutResult(reg_type, types, 1);
                    var TRs = RegexMethod.GetMutResult(reg_first_title, types, 1);
                    if (TRs.Count > 0)
                    {
                        THlist = RegexMethod.GetMutResult(reg_fixed_title, TRs[0], 1);
                        _THlist = RegexMethod.GetMutResult(reg_fixed_title_1, TRs[2], 1).Count < RegexMethod.GetMutResult(reg_fixed_title_1, TRs[3], 1).Count ? RegexMethod.GetMutResult(reg_fixed_title_1, TRs[3], 1) : RegexMethod.GetMutResult(reg_fixed_title_1, TRs[2], 1);
                        _THlist_ = RegexMethod.GetMutResult(reg_fixed_title_1, TRs[1], 1);
                        if (THlist.Count == 0)
                        {
                            THlist = RegexMethod.GetMutResult(reg_fixed_title, TRs[1], 1);
                        }
                    }
                    if (typelist.Count > 0 && THlist.Count != 7 && THlist.Count != 5 && THlist.Count != 16 && THlist.Count != 6 && _THlist.Count != 11 && _THlist.Count != 12 && _THlist_.Count != 13)
                    {
                        string newtype = string.Empty;
                        foreach (var type in typelist)
                        {
                            string type1 = type;
                            var dsg = RegexMethod.GetMutResult("<.*?>", type);
                            foreach (var itemdsg in dsg)
                            {
                                type1 = type1.Replace(itemdsg, "");
                            }
                            newtype = newtype + type1 + ";";
                        }
                        dirs.Add("Type", newtype);
                    }
                    else
                    {
                        if (_THlist_.Count == 13)
                        {
                            string catno = string.Empty;
                            string load = string.Empty;
                            string amps = string.Empty;
                            string volts = string.Empty;
                            string hz = string.Empty;
                            string temp = string.Empty;
                            string polthr = string.Empty;
                            string endur30c = string.Empty;
                            string endur50c = string.Empty;
                            string ip = string.Empty;
                            string dis_mm = string.Empty;
                            string spca = string.Empty;
                            string stded = string.Empty;
                            string endur = string.Empty;
                            string perpole = string.Empty;
                            string spcoa = string.Empty;
                            foreach (var TDitem in TRs)
                            {
                                var td_itemlist = RegexMethod.GetMutResult(reg_fixed_details, TDitem, 1);
                                var td_itemlist1 = RegexMethod.GetMutResult(reg_fixed_details_1, TDitem, 1);
                                var newtd_itemlist = td_itemlist.Count > td_itemlist1.Count ? td_itemlist : td_itemlist1;
                                if (newtd_itemlist.Count < 11)
                                {
                                    continue;
                                }
                                if (!String.IsNullOrEmpty(newtd_itemlist[0]))
                                {
                                    var dsg = RegexMethod.GetMutResult("<.*?>", newtd_itemlist[0]);
                                    if (dsg.Count > 0)
                                    {
                                        foreach (var itemdsg in dsg)
                                        {
                                            newtd_itemlist[0] = newtd_itemlist[0].Replace(itemdsg, "");
                                        }
                                    }
                                }
                                catno = String.IsNullOrEmpty(newtd_itemlist[0]) ? catno + "-;" : catno + newtd_itemlist[0].Replace("\n", "") + ";";
                                load = String.IsNullOrEmpty(newtd_itemlist[1]) ? load + "-;" : load + newtd_itemlist[1].Replace("<br>", "") + ";";
                                amps = amps + newtd_itemlist[2].Replace("<br>", "") + ";";
                                volts = volts + newtd_itemlist[3] + ";";
                                hz = hz + newtd_itemlist[4] + ";";
                                temp = String.IsNullOrEmpty(newtd_itemlist[5]) ? temp + "-;" : temp + newtd_itemlist[5] + ";";
                                polthr = String.IsNullOrEmpty(newtd_itemlist[6]) ? polthr + "-;" : polthr + newtd_itemlist[6] + ";";
                                endur30c = String.IsNullOrEmpty(newtd_itemlist[7]) ? endur30c + "-;" : endur30c + newtd_itemlist[7] + ";";
                                endur50c = String.IsNullOrEmpty(newtd_itemlist[8]) ? endur50c + "-:" : endur50c + newtd_itemlist[8] + ";";
                                ip = String.IsNullOrEmpty(newtd_itemlist[9]) ? ip + "-:" : ip + newtd_itemlist[9] + ";";
                                dis_mm = String.IsNullOrEmpty(newtd_itemlist[10]) ? dis_mm + "-;" : dis_mm + newtd_itemlist[10] + ";";
                                spca = String.IsNullOrEmpty(newtd_itemlist[11]) ? spca + "-;" : spca + newtd_itemlist[11] + ";";
                                stded = String.IsNullOrEmpty(newtd_itemlist[12]) ? stded + "-;" : stded + newtd_itemlist[12] + ";";
                                endur = endur + "/;";
                                perpole = perpole + "/;";
                                spcoa = spcoa + "/;";
                            }
                            dirs.Add("CatNo", catno);
                            dirs.Add("Load", load);
                            dirs.Add("Amps", amps);
                            dirs.Add("Volts", volts);
                            dirs.Add("Hz", hz);
                            dirs.Add("Temp", temp);
                            dirs.Add("PolThr", polthr);
                            dirs.Add("Endurance30C", endur30c);
                            dirs.Add("Endurance50C", endur50c);
                            dirs.Add("IP", ip);
                            dirs.Add("Dis_mm", dis_mm);
                            dirs.Add("SPCA", spca);
                            dirs.Add("StdEd", stded);
                            dirs.Add("Endurance", endur);
                            dirs.Add("PerPoleCircuitCode", perpole);
                            dirs.Add("SPCOA", spcoa);
                        }
                        if (_THlist.Count == 11 || _THlist.Count == 12)
                        {
                            string materialdsg = string.Empty;
                            string color = string.Empty;
                            string minThk = string.Empty;
                            string flamclass = string.Empty;
                            string hwi = string.Empty;
                            string hai = string.Empty;
                            string hva = string.Empty;
                            string hvtr = string.Empty;
                            string rtielec = string.Empty;
                            string mechimp = string.Empty;
                            string mechstr = string.Empty;
                            string d495 = string.Empty;
                            string cti = string.Empty;
                            foreach (var TDitem in TRs)
                            {
                                var td_itemlist = RegexMethod.GetMutResult(reg_fixed_details, TDitem, 1);
                                var td_itemlist1 = RegexMethod.GetMutResult(reg_fixed_details_1, TDitem, 1);
                                var newtd_itemlist = td_itemlist.Count > td_itemlist1.Count ? td_itemlist : td_itemlist1;
                                if (newtd_itemlist.Count < 11)
                                {
                                    continue;
                                }
                                if (!String.IsNullOrEmpty(newtd_itemlist[0]))
                                {
                                    var dsg = RegexMethod.GetMutResult("<.*?>", newtd_itemlist[0]);
                                    if (dsg.Count > 0)
                                    {
                                        foreach (var itemdsg in dsg)
                                        {
                                            newtd_itemlist[0] = newtd_itemlist[0].Replace(itemdsg, "");
                                        }
                                    }
                                }
                                materialdsg = String.IsNullOrEmpty(newtd_itemlist[0]) ? materialdsg + "-;" : materialdsg + newtd_itemlist[0].Replace("\n", "") + ";";
                                color = String.IsNullOrEmpty(newtd_itemlist[1]) ? color + "-;" : color + newtd_itemlist[1].Replace("<br>", "") + ";";
                                minThk = minThk + newtd_itemlist[2].Replace("<br>", "") + ";";
                                flamclass = flamclass + newtd_itemlist[3] + ";";
                                hwi = hwi + newtd_itemlist[4] + ";";
                                hai = String.IsNullOrEmpty(newtd_itemlist[5]) ? hai + "-;" : hai + newtd_itemlist[5] + ";";
                                if (String.IsNullOrEmpty(_THlist[7]))
                                {
                                    hva = String.IsNullOrEmpty(newtd_itemlist[6]) ? hva + "-;" : hva + newtd_itemlist[6] + ";";
                                    rtielec = String.IsNullOrEmpty(newtd_itemlist[7]) ? rtielec + "-;" : rtielec + newtd_itemlist[7] + ";";
                                    mechimp = String.IsNullOrEmpty(newtd_itemlist[8]) ? mechimp + "-:" : mechimp + newtd_itemlist[8] + ";";
                                    mechstr = String.IsNullOrEmpty(newtd_itemlist[9]) ? mechstr + "-:" : mechstr + newtd_itemlist[9] + ";";

                                }
                                if (String.IsNullOrEmpty(_THlist[6]))
                                {
                                    rtielec = String.IsNullOrEmpty(newtd_itemlist[6]) ? rtielec + "-;" : rtielec + newtd_itemlist[6] + ";";
                                    mechimp = String.IsNullOrEmpty(newtd_itemlist[7]) ? mechimp + "-:" : mechimp + newtd_itemlist[7] + ";";
                                    mechstr = String.IsNullOrEmpty(newtd_itemlist[8]) ? mechstr + "-:" : mechstr + newtd_itemlist[8] + ";";
                                    hvtr = String.IsNullOrEmpty(newtd_itemlist[9]) ? hvtr + "-;" : hvtr + newtd_itemlist[9] + ";";
                                }
                                if (newtd_itemlist.Count == 11)
                                {
                                    cti = String.IsNullOrEmpty(newtd_itemlist[10]) ? cti + "-;" : cti + newtd_itemlist[10] + ";";
                                }
                                if (newtd_itemlist.Count == 12)
                                {
                                    d495 = String.IsNullOrEmpty(newtd_itemlist[10]) ? d495 + "-;" : d495 + newtd_itemlist[10] + ";";
                                    cti = String.IsNullOrEmpty(newtd_itemlist[11]) ? cti + "-;" : cti + newtd_itemlist[11] + ";";
                                }
                            }
                            dirs.Add("MaterialDsg", materialdsg);
                            dirs.Add("Color", color);
                            dirs.Add("MinThkmm", minThk);
                            dirs.Add("FlameClass", flamclass);
                            dirs.Add("RTIElec", rtielec);
                            dirs.Add("MechImp", mechimp);
                            dirs.Add("MechStr", mechstr);
                            dirs.Add("HWI", hwi);
                            dirs.Add("HAI", hai);
                            dirs.Add("HVTR", hvtr);
                            dirs.Add("D495", d495);
                            dirs.Add("CTI", cti);
                            dirs.Add("HVA", hva);
                        }
                        if (THlist.Count == 7)
                        {
                            var TDlist = RegexMethod.GetMutResult(reg_fixed_details, types, 1);
                            string dsgdetails = string.Empty;
                            string capclass = string.Empty;
                            string ratv = string.Empty;
                            string capuf = string.Empty;
                            string res = string.Empty;
                            string low = string.Empty;
                            string upp = string.Empty;
                            int i = TDlist.Count / 7;
                            for (int m = 0; m < i; m++)
                            {
                                var dsg = RegexMethod.GetMutResult("<.*?>", TDlist[7 * m]);
                                foreach (var itemdsg in dsg)
                                {
                                    TDlist[7 * m] = TDlist[7 * m].Replace(itemdsg, "");
                                }
                                dsgdetails = dsgdetails + TDlist[7 * m].Replace("&nbsp;", "-") + ";";
                                capclass = capclass + TDlist[7 * m + 1] + ";";
                                ratv = ratv + TDlist[7 * m + 2].Replace("<br>", "") + ";";
                                capuf = capuf + TDlist[7 * m + 3].Replace("<br>", "") + ";";
                                res = res + TDlist[7 * m + 4].Replace("&#8212;", "-") + ";";
                                low = low + TDlist[7 * m + 5] + ";";
                                upp = upp + TDlist[7 * m + 6] + ";";
                            }
                            dirs.Add("TypeDsg", dsgdetails);
                            dirs.Add("CapClass", capclass);
                            dirs.Add("Rating_V", ratv);
                            dirs.Add("Cap_uf", capuf);
                            dirs.Add("ResforRC_ohms", res);
                            dirs.Add("LowerTemp", low);
                            dirs.Add("UpperTemp", upp);
                        }
                        if (THlist.Count == 5 && _THlist.Count != 11)
                        {
                            //var TDlist = RegexMethod.GetMutResult(reg_first_title, types, 1);
                            string catno = string.Empty;
                            string size = string.Empty;
                            string amps = string.Empty;
                            string volts = string.Empty;
                            string inter = string.Empty;
                            foreach (var TDitem in TRs)
                            {
                                var td_itemlist = RegexMethod.GetMutResult(reg_fixed_details, TDitem, 1);
                                var td_itemlist1 = RegexMethod.GetMutResult(reg_fixed_details_1, TDitem, 1);
                                var newtd_itemlist = td_itemlist.Count > td_itemlist1.Count ? td_itemlist : td_itemlist1;
                                if (newtd_itemlist.Count == 0)
                                {
                                    continue;
                                }
                                if (!String.IsNullOrEmpty(newtd_itemlist[0]))
                                {
                                    var dsg = RegexMethod.GetMutResult("<.*?>", newtd_itemlist[0]);
                                    if (dsg.Count > 0)
                                    {
                                        foreach (var itemdsg in dsg)
                                        {
                                            newtd_itemlist[0] = newtd_itemlist[0].Replace(itemdsg, "");
                                        }
                                    }
                                }
                                catno = String.IsNullOrEmpty(newtd_itemlist[0]) ? catno + "-" : catno + newtd_itemlist[0].Replace("\n", "") + ";";
                                size = String.IsNullOrEmpty(newtd_itemlist[1]) ? size + "-" : size + newtd_itemlist[1].Replace("<br>", "") + ";";
                                amps = String.IsNullOrEmpty(newtd_itemlist[2]) ? amps + "-" : amps + newtd_itemlist[2].Replace("<br>", "") + ";";
                                volts = String.IsNullOrEmpty(newtd_itemlist[3]) ? volts + "-" : volts + newtd_itemlist[3] + ";";
                                inter = String.IsNullOrEmpty(newtd_itemlist[4]) ? inter + "-" : inter + newtd_itemlist[4] + ";";
                            }
                            if (THlist[1].Contains("Resistance"))
                            {
                                dirs.Add("ModelNO", catno);
                                dirs.Add("Res_at25_kohms", size);
                                dirs.Add("Tmoa", amps);
                                dirs.Add("Class", volts);
                                dirs.Add("CA", inter);
                            }
                            else
                            {
                                dirs.Add("CatNo", catno);
                                dirs.Add("Sizemm_in", size);
                                dirs.Add("Amps_A", amps);
                                dirs.Add("Volts", volts);
                                dirs.Add("InterruptingRating_A", inter);
                            }
                        }
                        if (THlist.Count == 16)
                        {
                            if (THlist[1].ToUpper().Contains("COLOR"))
                            {
                                string newtype = string.Empty;
                                foreach (var type in typelist)
                                {
                                    string type1 = type;
                                    var dsg = RegexMethod.GetMutResult("<.*?>", type);
                                    if (!String.IsNullOrEmpty(type))
                                    {
                                        if (dsg.Count > 0)
                                        {
                                            foreach (var itemdsg in dsg)
                                            {
                                                type1 = type1.Replace(itemdsg, "");
                                            }
                                            newtype = newtype + type1 + ";";
                                        }
                                    }
                                }
                                dirs.Add("Models", newtype);
                            }
                            string materialdsg = string.Empty;
                            string color = string.Empty;
                            string minThk = string.Empty;
                            string flamclass = string.Empty;
                            string hwi = string.Empty;
                            string hai = string.Empty;
                            string rtielc = string.Empty;
                            string mechimp = string.Empty;
                            string mechstr = string.Empty;
                            string hvtr = string.Empty;
                            string d495 = string.Empty;
                            string cti = string.Empty;
                            string insclass = string.Empty;
                            string duty = string.Empty;
                            string prottype = string.Empty;
                            string ratedambient = string.Empty;
                            foreach (var TDitem in TRs)
                            {
                                var td_itemlist = RegexMethod.GetMutResult(reg_fixed_details, TDitem, 1);
                                var td_itemlist1 = RegexMethod.GetMutResult(reg_fixed_details_1, TDitem, 1);
                                var newtd_itemlist = td_itemlist.Count > td_itemlist1.Count ? td_itemlist : td_itemlist1;
                                if (newtd_itemlist.Count != 16)
                                {
                                    continue;
                                }
                                if (!String.IsNullOrEmpty(newtd_itemlist[0]))
                                {
                                    var dsg = RegexMethod.GetMutResult("<.*?>", newtd_itemlist[0]);
                                    if (dsg.Count > 0)
                                    {
                                        foreach (var itemdsg in dsg)
                                        {
                                            newtd_itemlist[0] = newtd_itemlist[0].Replace(itemdsg, "");
                                        }
                                    }
                                }
                                materialdsg = String.IsNullOrEmpty(newtd_itemlist[0]) ? materialdsg + "-;" : materialdsg + newtd_itemlist[0].Replace("\n", "") + ";";
                                color = String.IsNullOrEmpty(newtd_itemlist[1]) ? color + "-;" : color + newtd_itemlist[1].Replace("<br>", "") + ";";
                                minThk = minThk + newtd_itemlist[2].Replace("<br>", "") + ";";
                                flamclass = flamclass + newtd_itemlist[3] + ";";
                                hwi = hwi + newtd_itemlist[4] + ";";
                                hai = String.IsNullOrEmpty(newtd_itemlist[5]) ? hai + "-;" : hai + newtd_itemlist[5] + ";";
                                rtielc = String.IsNullOrEmpty(newtd_itemlist[6]) ? rtielc + "-;" : rtielc + newtd_itemlist[6] + ";";
                                mechimp = String.IsNullOrEmpty(newtd_itemlist[7]) ? mechimp + "-:" : mechimp + newtd_itemlist[7] + ";";
                                mechstr = mechstr + newtd_itemlist[8] + ";";
                                hvtr = String.IsNullOrEmpty(newtd_itemlist[9]) ? hvtr + "-;" : hvtr + newtd_itemlist[9] + ";";
                                d495 = String.IsNullOrEmpty(newtd_itemlist[10]) ? d495 + "-;" : d495 + newtd_itemlist[10] + ";";
                                cti = String.IsNullOrEmpty(newtd_itemlist[11]) ? cti + "-;" : cti + newtd_itemlist[11] + ";";
                                insclass = insclass + newtd_itemlist[12] + ";";
                                duty = duty + newtd_itemlist[13] + ";";
                                prottype = String.IsNullOrEmpty(newtd_itemlist[14]) ? prottype + "-;" : prottype + newtd_itemlist[14] + ";";
                                ratedambient = String.IsNullOrEmpty(newtd_itemlist[15]) ? ratedambient + "-;" : ratedambient + newtd_itemlist[15] + ";";
                            }
                            if (THlist[1].ToUpper().Contains("COLOR"))
                            {
                                dirs.Add("MaterialDsg", materialdsg);
                                dirs.Add("Color", color);
                                dirs.Add("MinThkmm", minThk);
                                dirs.Add("FlameClass", flamclass);
                                dirs.Add("HWI", hwi);
                                dirs.Add("HAI", hai);
                                dirs.Add("RTIElec", rtielc);
                                dirs.Add("MechImp", mechimp);
                                dirs.Add("MechStr", mechstr);
                                dirs.Add("HVTR", hvtr);
                                dirs.Add("D495", d495);
                                dirs.Add("CTI", cti);
                                dirs.Add("InsClass", insclass);
                                dirs.Add("Duty", duty);
                                dirs.Add("ProtType", prottype);
                                dirs.Add("RatedAmbient", ratedambient);
                            }
                            if (THlist[1].ToUpper().Contains("SPD"))
                            {
                                dirs.Add("CatNo", materialdsg);
                                dirs.Add("SPDType", color);
                                dirs.Add("Volts", minThk);
                                dirs.Add("AC_DC_DCPV", flamclass);
                                dirs.Add("PH", hwi);
                                dirs.Add("AMPS", hai);
                                dirs.Add("AMBMin", rtielc);
                                dirs.Add("AMBMax", mechimp);
                                dirs.Add("MODE", mechstr);
                                dirs.Add("VPR_Vpk", hvtr);
                                dirs.Add("MLV_Vpk", d495);
                                dirs.Add("MCOV_V", cti);
                                dirs.Add("VN_Vdc", insclass);
                                dirs.Add("In_kA", duty);
                                dirs.Add("SCCR_kA", prottype);
                                dirs.Add("NOTES", ratedambient);
                            }
                        }
                        if (THlist.Count == 6)
                        {
                            string catno = string.Empty;
                            string insula = string.Empty;
                            string conduct = string.Empty;
                            string tempclass = string.Empty;
                            string ratedvpeak = string.Empty;
                            string ratedvrms = string.Empty;
                            string testvolt = string.Empty;
                            foreach (var TDitem in TRs)
                            {
                                var td_itemlist = RegexMethod.GetMutResult(reg_fixed_details, TDitem, 1);
                                var td_itemlist1 = RegexMethod.GetMutResult(reg_fixed_details_1, TDitem, 1);
                                var newtd_itemlist = td_itemlist.Count > td_itemlist1.Count ? td_itemlist : td_itemlist1;
                                if (newtd_itemlist.Count == 0)
                                {
                                    continue;
                                }
                                if (!String.IsNullOrEmpty(newtd_itemlist[0]))
                                {
                                    var dsg = RegexMethod.GetMutResult("<.*?>", newtd_itemlist[0]);
                                    if (dsg.Count > 0)
                                    {
                                        foreach (var itemdsg in dsg)
                                        {
                                            newtd_itemlist[0] = newtd_itemlist[0].Replace(itemdsg, "");
                                        }
                                    }
                                }
                                catno = String.IsNullOrEmpty(newtd_itemlist[0]) ? catno + "-" : catno + newtd_itemlist[0].Replace("\n", "") + ";";
                                insula = String.IsNullOrEmpty(newtd_itemlist[1]) ? insula + "-" : insula + newtd_itemlist[1].Replace("<br>", "") + ";";
                                conduct = conduct + newtd_itemlist[2].Replace("<br>", "") + ";";
                                tempclass = tempclass + newtd_itemlist[3] + ";";
                                if (newtd_itemlist.Count == 5)
                                {
                                    ratedvpeak = ratedvpeak + "/;";
                                    ratedvrms = ratedvrms + "/;";
                                    testvolt = String.IsNullOrEmpty(newtd_itemlist[4]) ? testvolt + "-;" : testvolt + newtd_itemlist[4] + ";";
                                }
                                if (newtd_itemlist.Count == 6)
                                {
                                    ratedvpeak = String.IsNullOrEmpty(newtd_itemlist[4]) ? ratedvpeak + "-;" : ratedvpeak + newtd_itemlist[4] + ";";
                                    ratedvrms = String.IsNullOrEmpty(newtd_itemlist[5]) ? ratedvrms + "-;" : ratedvrms + newtd_itemlist[5] + ";";
                                    testvolt = testvolt + "/;";
                                }
                                if (newtd_itemlist.Count == 7)
                                {
                                    testvolt = String.IsNullOrEmpty(newtd_itemlist[4]) ? testvolt + "-;" : testvolt + newtd_itemlist[4] + ";";
                                    ratedvpeak = String.IsNullOrEmpty(newtd_itemlist[5]) ? ratedvpeak + "-;" : ratedvpeak + newtd_itemlist[5] + ";";
                                    ratedvrms = String.IsNullOrEmpty(newtd_itemlist[6]) ? ratedvrms + "-;" : ratedvrms + newtd_itemlist[6] + ";";
                                }
                            }
                            if (!THlist[1].ToUpper().Contains("COLOR") && !THlist[1].ToUpper().Contains("MARK"))
                            {
                                dirs.Add("CatNo", catno);
                                dirs.Add("InsulationType", insula);
                                dirs.Add("ConductorSizeRange", conduct);
                                dirs.Add("TempClass", tempclass);
                                dirs.Add("RatedVpeak", ratedvpeak);
                                dirs.Add("RatedVrms", ratedvrms);
                                dirs.Add("TestVolt", testvolt);
                            }
                            if (THlist[1].ToUpper().Contains("COLOR"))
                            {
                                dirs.Add("CatNo", catno);
                                dirs.Add("Color", insula);
                                dirs.Add("Temp", conduct);
                                dirs.Add("FlameRetardant", tempclass);
                                dirs.Add("SunlightResitant", ratedvpeak);
                                dirs.Add("ColdResitant", ratedvrms);
                            }
                            if (THlist[1].ToUpper().Contains("MARK"))
                            {
                                dirs.Add("MaterialDesignation", catno);
                                dirs.Add("MarkDsg", insula);
                                dirs.Add("BaseCoat", conduct);
                                dirs.Add("TopCoat", tempclass);
                                dirs.Add("ANSIType", ratedvpeak);
                                dirs.Add("TempClass", ratedvrms);
                                dirs.Add("BondCoat", testvolt);
                            }
                        }
                    }
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return dirs;
        }
        /// <summary>
        /// 使用基类方法 服务端接口未完成前先输出到本地
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parms">预留 可空</param>
        /// <returns></returns>
        public override void UploadData(object[] parms = null)
        {
            base.UploadData(parms);
        }

        #region 辅助函数
        /// <summary>
        /// 获取详情Url参数
        /// </summary>
        /// <param name="Certi_No"></param>
        public static void UL_Index(String Certi_No, ref ConcurrentQueue<string> newdata)
        {
            String html = "start";
            try
            {
                HttpInfo info = new HttpInfo();
                info.RequestUrl = UL_Index_Url;
                info.PostData = String.Format(UL_PostData, Certi_No);
                while (!html.Contains(Certi_No))
                {
                    html = HttpMethod.HttpWork(info);
                    Thread.Sleep(1);
                }
                var list = RegexMethod.GetMutResult(reg_UL_Details, html, 1);
                foreach (var item in list)
                {
                    if (item.Contains(Certi_No) && !item.Contains("Refine Your Search"))
                    {
                        var urllist = RegexMethod.GetMutResult("\"(.*?)\"", item, 1);
                        string urldata = null;
                        for (int i = 1; i < urllist.Count; i++)
                        {
                            urldata = urldata + urllist[i];
                        }
                        newdata.Enqueue(urldata);
                    }
                }
            }
            catch
            {

            }
        }
        #endregion
    }
}
