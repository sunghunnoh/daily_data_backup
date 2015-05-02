using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using MySql.Data.MySqlClient;
using MySql.Data.Types;



using XA_DATASETLib;
using XA_SESSIONLib;



namespace stock_simulator
{
    

    public partial class Form1 : Form
    {
        private XASession xingSession = null;
        private XAQuery xingQuery = null;

        MySqlConnection conn = null;

        string SC = "";

        Int16 update_flag = 0;

        string[] shcode_50 = 

        int date_flag = 0;

        int save_data_flag = 0;
        int save_data_count = 0;
        int save_data_order = 0;
        string[] stock_code_data = new string[2000];
        string[] stock_name_data = new string[2000];


        // 데이터 숫자 정보
        //int day_num = 5000, stock_num = 1082;
        int day_num = 5000, stock_num = 1082;

        // 자료 카운트를 위한 변수
        int count_data = 0;

        // 자료 취득을 위한 날짜, 주식 코드 정보
        string[] date = new string[5000];
        string[] shcode = new string[1082];

        string[] name = new string[1082];

        UInt64[] BS_start_price = new UInt64[1082];
        UInt64[] BS_buy_price = new UInt64[1082];
        UInt64[] BS_last_price = new UInt64[1082];

        string[] BS_start_date = new string[1082];
        string[] BS_buy_date = new string[1082];
        string[] BS_last_date = new string[1082];

        // 데이터 베이스 자료 저장용 버퍼
        string[,] s_date = new string[5000, 1082];
        UInt64[,] close = new UInt64[5000, 1082];
        UInt64[,] volume = new UInt64[5000, 1082];
        UInt64[,] marketcap = new UInt64[5000, 1082];
        UInt64[,] amount = new UInt64[5000, 1082];
        UInt64[,] buy_price = new UInt64[5000, 1082];

        UInt64[,] stable_flag = new UInt64[5000, 1082];

        UInt64 print_index = 100000;

        string[] buy_sell_start_date = new string[100000];
        UInt64[] buy_sell_start_price = new UInt64[100000];

        string[] buy_sell_close_date = new string[100000];
        UInt64[] buy_sell_close = new UInt64[100000];

        string[] buy_sell_last_date = new string[100000];
        UInt64[] buy_sell_last_price = new UInt64[100000];

        string[] buy_sell_stock_code = new string[100000];

        float[] buy_sell_margin = new float[100000];

        int buy_sell_index = 0;


        int[] state = new int[1082];
        int buy_flag = 0;

        // 매매로 인한 전체 마진 계산용
        float total_margin = 1;

        public Form1()
        {
            InitializeComponent();

            xingSession = new XASession();
            //((XA_SESSIONLib._IXASessionEvents_Event)xingSession).Login += xingSession_Login;

            xingQuery = new XAQuery();
            //xingQuery.ReceiveData += xingQuery_ReceiveData;

            ((XA_SESSIONLib._IXASessionEvents_Event)xingSession).Login += xingSession_Login;
            xingSession.ConnectServer("demo.etrade.co.kr", 20001);
            ((XA_SESSIONLib.IXASession)xingSession).Login("nshhsn", "shtjdgns", "", 0, false);

            string strConn = "Server=localhost;Database=stock;Uid=nshhsn;Pwd=shtjdgns";
            conn = new MySqlConnection(strConn);
            conn.Open();

            /*
            string a = "19950612";
            int y = Convert.ToInt32(a.Substring(0,4));
            int m = Convert.ToInt32(a.Substring(4).Substring(0,2));
            int d = Convert.ToInt32(a.Substring(6));

            resultList.Items.Add(y);
            resultList.Items.Add(m);
            resultList.Items.Add(d);
            */


        }


        /* Login Xing server & Database server*/
        private void Login_xing_Click(object sender, EventArgs e)
        {
            ((XA_SESSIONLib._IXASessionEvents_Event)xingSession).Login += xingSession_Login;
            xingSession.ConnectServer("hts.ebestsec.co.kr", 20001);
            ((XA_SESSIONLib.IXASession)xingSession).Login("nshhsn", "shtjdgns", "1q2w3e4r%t", 0, false);

            string strConn = "Server=localhost;Database=stock;Uid=nshhsn;Pwd=shtjdgns";
            conn = new MySqlConnection(strConn);
            conn.Open();
        }

        // 로그인 시 세션 연결
        public void xingSession_Login(string code, string msg)
        {
            if (code.Equals("0000"))
            {
                //MessageBox.Show("Sucessed to login ( " + msg + " )", "CONNECTED");
            }
            else
            {
                ((XA_SESSIONLib._IXASessionEvents_Event)xingSession).Login -= xingSession_Login;
                MessageBox.Show("Failed to login ( " + msg + " )", "ERROR");
            }
        }


        // 날짜 데이터를 저장
        private void date_table_Click(object sender, EventArgs e)
        {
            date_flag = 1;
            string sql = String.Format("CREATE TABLE `stock`.`date` (`num` INT NOT NULL,`day` VARCHAR(8) NULL,PRIMARY KEY (`num`) )");
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();

            request_t1305("000020");
        }

        /* 주식 코드 데이터 stock_code table에 저장 */
        private void stock_code_Click(object sender, EventArgs e)
        {            
            try
            {
                string i = "stock_code";

                string sql = String.Format("CREATE TABLE `stock`.`{0}` (`num` INT NOT NULL,`name` VARCHAR(40) NULL,`code` char(6) NULL,PRIMARY KEY (`num`) )", i);

                MySqlCommand cmd = new MySqlCommand(sql, conn);

                cmd.ExecuteNonQuery();
            }
            catch (MySqlException a)
            {
                // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
            }

            xingQuery.ReceiveData += xingQuery_ReceiveData;

            xingQuery.LoadFromResFile("c:/etrade/xingapi/res/t9945.res");
            xingQuery.SetFieldData("t9945InBlock", "gubun", 0, "1");
            xingQuery.Request(false);
        }

        // 전체 주가지수 데이터 저장 함수
        private void save_data_Click(object sender, EventArgs e)
        {
            resultList.Items.Clear();
            resultList.Items.Add("Retriving stock_code table...");
            resultList.Items.Add("-----");

            string sql = "select * from stock_code";

            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();

            int count = 0;

            // database로 부터 데이터 읽어 변수에 저장
            while (reader.Read())
            {
                stock_code_data[count] = Convert.ToString(reader["code"]);
                stock_name_data[count] = Convert.ToString(reader["name"]);
                count++;
            }
            
            reader.Close();

            resultList.Items.Add("-----");
            resultList.Items.Add(String.Format("{0} records has been retrieved.", count));
            
            save_data_count = count;
            save_data_flag = 1;

            request_t1305(stock_code_data[save_data_order]);

        }

        // 기간별 주가 요청 함수
        void request_t1305(string shcode)
        {
            string i = shcode;
            SC = shcode;

            if (update_flag == 1)
            {
                //update_flag = 0;

                xingQuery.ReceiveData += xingQuery_ReceiveData;

                xingQuery.LoadFromResFile("c:/etrade/xingapi/res/t1305.res");
                xingQuery.SetFieldData("t1305InBlock", "shcode", 0, "000020");
                xingQuery.SetFieldData("t1305InBlock", "dwmcode", 0, "1");
                xingQuery.SetFieldData("t1305InBlock", "cnt", 0, "9999");
                xingQuery.Request(false);
            }


            if (date_flag == 0)
            {
                resultList.Items.Add("shcode == " + shcode);
                Delay(1000);
                try
                {
                    string sql = String.Format("CREATE TABLE `stock`.`{0}` (`date` VARCHAR(8) NOT NULL,`close` VARCHAR(8) NULL,`volume` VARCHAR(12) NULL,`marketcap` VARCHAR(12) NULL,`amount` VARCHAR(18) NULL,PRIMARY KEY (`date`) )", i);

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException a)
                {
                    // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
                }

                xingQuery.ReceiveData += xingQuery_ReceiveData;

                xingQuery.LoadFromResFile("c:/etrade/xingapi/res/t1305.res");
                xingQuery.SetFieldData("t1305InBlock", "shcode", 0, SC);
                xingQuery.SetFieldData("t1305InBlock", "dwmcode", 0, "1");
                xingQuery.SetFieldData("t1305InBlock", "cnt", 0, "5000");
                xingQuery.Request(false);
            }
            else
            {
                resultList.Items.Add("date table save");
                xingQuery.ReceiveData += xingQuery_ReceiveData;

                xingQuery.LoadFromResFile("c:/etrade/xingapi/res/t1305.res");
                xingQuery.SetFieldData("t1305InBlock", "shcode", 0, SC);
                xingQuery.SetFieldData("t1305InBlock", "dwmcode", 0, "1");
                xingQuery.SetFieldData("t1305InBlock", "cnt", 0, "5000");
                xingQuery.Request(false);
            }
            
        }

                /* 데이터 수신 */
        void xingQuery_ReceiveData(string szTrCode)
        {
            //기간별 주가 
            if (szTrCode.Equals("t1305"))
            {
                if (update_flag == 2)
                {
                    int count = xingQuery.GetBlockCount("t1305OutBlock1");
                    for (int idx = 0; idx < count; idx++)
                    {



                }
                if(update_flag == 1)
                {
                    string date;

                    int count = xingQuery.GetBlockCount("t1305OutBlock1");

                    for (int idx = 0; idx < count; idx++)
                    {

                        date = xingQuery.GetFieldData("t1305OutBlock1", "date", count - 1 - idx);

                        try
                        {
                            string sql = String.Format("insert into `stock`.`new_date` values ('{0}', '{1}')", idx, date);
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            cmd.ExecuteNonQuery();
                        }
                        catch (MySqlException c)
                        {
                            // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
                        }
                    }
                    update_flag = 0;


                }
                if (date_flag == 1)
                {
                    string date;

                    int count = xingQuery.GetBlockCount("t1305OutBlock1");

                    for (int idx = 0; idx < count; idx++)
                    {

                        date = xingQuery.GetFieldData("t1305OutBlock1", "date", count - 1 - idx);

                        try
                        {
                            string sql = String.Format("insert into `stock`.`date` values ('{0}', '{1}')", idx, date);
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            cmd.ExecuteNonQuery();
                        }
                        catch (MySqlException c)
                        {
                            // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
                        }
                    }
                    date_flag = 0;
                }
                else
                {
                    string date;
                    string close;
                    string volume;
                    string marketcap;
                    string amount;
                    //string buy_price;

                    string pre_date;
                    string pre_close = "";
                    string pre_volume;
                    string pre_marketcap;
                    string pre_amount = "";
                    //string pre_buy_price = "";

                    xingQuery.ReceiveData -= xingQuery_ReceiveData;

                    //resultList.Items.Clear();

                    int count = xingQuery.GetBlockCount("t1305OutBlock1");

                    /*
                    string sql = String.Format("LOCK TABLES `stock`.`{0}` WRITE", SC);
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                    */

                    string sql = String.Format("insert into `stock`.`{0}` values", SC);
                    MySqlCommand cmd;

                    /*
                    int stable_flag = 0;
                    UInt64 count_stable = 0;
                    String buy_price_p = "0";
                     * */

                    // 요청한 기간별 주가 정보를 수신
                    for (int idx = 0; idx < count; idx++)
                    {
                        date = xingQuery.GetFieldData("t1305OutBlock1", "date", count - 1 - idx);
                        close = xingQuery.GetFieldData("t1305OutBlock1", "close", count - 1 - idx);
                        volume = xingQuery.GetFieldData("t1305OutBlock1", "volume", count - 1 - idx);
                        marketcap = xingQuery.GetFieldData("t1305OutBlock1", "marketcap", count - 1 - idx);
                        amount = Convert.ToString((Convert.ToUInt64(marketcap) * 1000000) / Convert.ToUInt64(close));

                        /*
                        if (stable_flag == 1)
                        {
                            if (count_stable >= (Convert.ToUInt64(amount) * 3))
                            {
                                stable_flag = 0;
                                count_stable = 0;
                            }
                            else
                            {
                                count_stable = count_stable + Convert.ToUInt64(volume);
                            }
                        }

                        if ((idx == 0) || (Convert.ToUInt64(pre_amount) >= (Convert.ToUInt64(amount) + 500)) || (Convert.ToUInt64(amount) >= (Convert.ToUInt64(pre_amount) + 500)))
                        {
                            buy_price = close;
                            stable_flag = 1;
                        }
                        else
                        {
                            // 수량 * 전날 구매 가격 
                            buy_price = Convert.ToString(((Convert.ToUInt64(pre_amount) * Convert.ToUInt64(pre_buy_price))
                                                             - (Convert.ToUInt64(volume) * Convert.ToUInt64(pre_buy_price))
                                                             + (Convert.ToUInt64(volume) - (Convert.ToUInt64(amount) - Convert.ToUInt64(pre_amount))) * Convert.ToUInt64(close))
                                                          / Convert.ToUInt64(amount));
                        }
                         * */

                        pre_date = date;
                        pre_close = close;
                        pre_volume = volume;
                        pre_marketcap = marketcap;
                        pre_amount = amount;
                        //pre_buy_price = buy_price;

                        // List box에 기간별 주가를 출력
                        //string result = idx + "\t" + date + "\t" + close + "\t" + volume + "\t" + marketcap + "\t\t" + amount + "\t" + buy_price;
                        //resultList.Items.Add(result);
                        /*
                        if (stable_flag == 1)
                            buy_price_p = "0";
                        else if (stable_flag == 0)
                            buy_price_p = buy_price;
                         * */

                        if (idx == count - 1)
                            sql = sql + String.Format(" ('{0}', '{1}', '{2}', '{3}', '{4}') ", date, close, volume, marketcap, amount);
                        else
                            sql = sql + String.Format(" ('{0}', '{1}', '{2}', '{3}', '{4}'), ", date, close, volume, marketcap, amount);
                        // database에 기간별 주가를 저장

                    }

                    try
                    {
                        cmd = new MySqlCommand(sql, conn);
                        cmd.ExecuteNonQuery();
                    }
                    catch (MySqlException e)
                    {
                        // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
                    }


                    /*
                    sql = String.Format("UNLOCK TABLES");
                    cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                    */

                    resultList.Items.Add("-----");

                    // 전체 데이터 저장시
                    if (save_data_flag == 1)
                    {
                        // 다음 데이터를 요청
                        save_data_order++;
                        if (save_data_order < 1082)
                        {
                            resultList.Items.Add("re_request");
                            request_t1305(stock_code_data[save_data_order]);
                        }
                        else
                            save_data_flag = 0;
                    }
                }

            }
            // 주식 코드 데이터 
            else if (szTrCode.Equals("t9945"))
            {
                string name = "";
                string code = "";

                int count = xingQuery.GetBlockCount("t9945OutBlock");

                resultList.Items.Clear();
                resultList.Items.Add("t9945OutBlockTEST");

                // 수신한 코드 정보를 처리
                for (int idx = 0; idx < count; idx++)
                {
                    name = xingQuery.GetFieldData("t9945OutBlock", "hname",idx);
                    code = xingQuery.GetFieldData("t9945OutBlock", "shcode", idx);

                    // List box에 출력
                    string result = idx + "\t" + name + "\t\t\t" + code;
                    resultList.Items.Add(result);

                    // database에 저장
                    try
                    {
                        string sql = String.Format("insert into `stock`.`stock_code` values ('{0}', '{1}', '{2}')", idx, name, code);
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        cmd.ExecuteNonQuery();
                    }
                    catch (MySqlException e)
                    {
                        // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
                    }
                }
            }


        }


        private void simulator2_Click(object sender, EventArgs e)
        {
            // 데이터 숫자 정보
            //int day_num = 5000, stock_num = 1082;
            int day_num = 5000, stock_num;

            // 구매 여부를 결정하기 위한 파라매터   
            UInt64 buy_margin = Convert.ToUInt64(textBox2.Text);
            UInt64 sell_margin = Convert.ToUInt64(textBox3.Text);
            UInt64 low_count = Convert.ToUInt64(textBox4.Text);
            stock_num = Convert.ToInt32(textBox5.Text);

            /*
            UInt64 buy_margin = 120;
            UInt64 sell_margin = 80;
            UInt64 low_count = 80;
            UInt64 low_percent = 100;
            */

            // 자료 카운트를 위한 변수
            int count_data = 0;

            // 자료 취득을 위한 날짜, 주식 코드 정보
            string[] date = new string[day_num];
            string[] shcode = new string[5000];

            // 데이터 베이스 자료 저장용 버퍼
            string[,] s_date = new string[day_num, stock_num];
            UInt64[,] close = new UInt64[day_num, stock_num];
            UInt64[,] volume = new UInt64[day_num, stock_num];
            UInt64[,] amount = new UInt64[day_num, stock_num];
            UInt64[,] buy_price = new UInt64[day_num, stock_num];

            // 구매 판매 여부를 결정 하기 위한 버퍼
            UInt64[] delay_count = new UInt64[stock_num];
            UInt64[] buy_flag = new UInt64[stock_num];
            UInt64[] count = new UInt64[stock_num];
            UInt64[] lowest_value = new UInt64[stock_num];
            UInt64[] start_value = new UInt64[stock_num];
            float[] gap_value = new float[stock_num];

            // 구매 판매 결과를 저장 하기 위한 버퍼
            string[] buy_sell_date = new string[day_num];
            string[] buy_sell_stock = new string[day_num];
            UInt64[] buy_sell_start = new UInt64[day_num];
            UInt64[] buy_sell_lowest = new UInt64[day_num];
            UInt64[] buy_sell_close = new UInt64[day_num];
            float[] buy_sell_gap = new float[day_num];
            string[] buy_sell_direction = new string[day_num];

            // 현재 구매 상태인지 확인하기 위한 변수
            int global_buy_flag = 0;

            // 매매로 인한 전체 마진 계산용
            float total_margin = 1;

            //List_box clear
            resultList.Items.Clear();


            //날짜를 불러 date에 저장
            MySqlCommand cmd;
            MySqlDataReader reader;

            count_data = 0;
            string sql = String.Format("select day from stock.date");
            cmd = new MySqlCommand(sql, conn);
            reader = cmd.ExecuteReader();

            while(reader.Read())
            {
                date[count_data] = Convert.ToString(reader["day"]);
                count_data++;
            }
                
            reader.Close();
            reader.Dispose();


            // 주가 코드를 불러 shcode에 저장
            count_data = 0;
            sql = String.Format("select code from stock.stock_code");
            cmd = new MySqlCommand(sql, conn);
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                shcode[count_data] = Convert.ToString(reader["code"]);
                count_data++;
            }

            reader.Close();
            reader.Dispose();


            int buy_sell_index = 0;

            for (int j= 0; j<stock_num; j++)
            {
                sql = String.Format("select * from stock.{0};", shcode[j]);
                cmd = new MySqlCommand(sql, conn);
                reader = cmd.ExecuteReader();

                count_data = 0;
                string a;

                while (reader.Read())
                {
                    a = Convert.ToString(reader["date"]);           

                    while (date[count_data].Equals(a) == false)
                    {
                        if (Convert.ToInt32(date[count_data]) > Convert.ToInt32(a))
                        {
                            reader.Read();
                            a = Convert.ToString(reader["date"]); 
                        }
                        else
                        {
                            s_date[count_data, j] = date[count_data];
                            close[count_data, j] = 0;
                            volume[count_data, j] = 0;
                            amount[count_data, j] = 0;
                            buy_price[count_data, j] = 0;
                            count_data++;
                        }
                    }

                    s_date[count_data,j] = Convert.ToString(reader["date"]);
                    close[count_data,j] = Convert.ToUInt64(reader["close"]);
                    volume[count_data,j] = Convert.ToUInt64(reader["volume"]);
                    amount[count_data,j] = Convert.ToUInt64(reader["amount"]);
                    buy_price[count_data,j] = Convert.ToUInt64(reader["buy_price"]);
                    count_data++;
                }

                reader.Close();
                reader.Dispose();
            }

            for(int m = 1;m<day_num;m++)
            {
                for (int n = 0; n < stock_num; n++)
                {   
                    // 종가 또는 구매가가 20%이상 변화하면 구매를 위한 카운터를 초기화
                    if ((close[m - 1, n] >= close[m, n] * 1.2) || (close[m - 1, n] * 1.2 <= close[m, n])||
                        (buy_price[m - 1, n] >= buy_price[m, n] * 1.2) || (buy_price[m - 1, n] * 1.2 <= buy_price[m, n]) ||
                        (amount[m - 1, n] >= amount[m, n] * 1.01) || (amount[m - 1, n] * 1.01 <= amount[m, n]))
                    {
                        delay_count[n] = 0;
                        if (buy_flag[n] == 1)
                        {
                            // 구매 flag 해제    
                            global_buy_flag = 0;
                            buy_flag[n] = 0;

                            // 판매 날짜, 구매 주식, 구매 가격, 구매 상태를 업데이트
                            buy_sell_date[buy_sell_index] = date[m-1];
                            buy_sell_stock[buy_sell_index] = shcode[n];
                            buy_sell_close[buy_sell_index] = close[m-1, n];
                            buy_sell_direction[buy_sell_index] = "sell";
                            buy_sell_index++;
                        }

                    }
                    else // 아닐경우 당일의 매매량을 카운터에 합
                    { 
                        if (delay_count[n] < amount[m, n] * 8)
                            delay_count[n] = delay_count[n] + volume[m, n];
                    }

                    // 카운터가 전체 주식량의 3배가 되었을 때 부터 유효한 데이터로 판단함
                    if (delay_count[n] > amount[m,n] * 8)
                    {
                        // 주식을 구매하지 않았을 때 Buy_flag = 0
                        if (buy_flag[n] == 0)
                        {
                            // 종가가 buy_price 보다 싸면
                            if (close[m,n] < buy_price[m,n])
                            {                                
                                count[n]++; //카운터를 증가
                                gap_value[n] = gap_value[n] + ((float)buy_price[m, n] / (float)close[m, n]);

                                // 처음 카운터가 증가하면
                                if (count[n] == 1)
                                {
                                    start_value[n] = close[m, n];
                                    lowest_value[n] = close[m, n];   // lowest_vlaue 값을 등록
                                }
                                else // 이후에는
                                {
                                    if (lowest_value[n] > close[m, n]) // lowest_value 보다 작은 값일 때 업데이트
                                        lowest_value[n] = close[m, n];
                                }
                            }
                            else // 종가가 buy_price 보다 비싸면 구매 신호임
                            {
                                if ((count[n] > low_count) && (global_buy_flag == 0) && ((lowest_value[n] * 100 / close[m, n]) < 70) /*&& ((lowest_value[n] * 100 / close[m, n]) > 40)*/) // 카운터 수가 지정된 양 이상이고, 다른 주식을 구매하지 않음
                                {
                                    // 구매 flag를 셋팅
                                    buy_flag[n] = 1;    
                                    global_buy_flag = 1;

                                    // 구매 날짜, 구매 주식, 구매 가격, 구매 상태를 업데이트
                                    buy_sell_date[buy_sell_index] = date[m];
                                    buy_sell_stock[buy_sell_index] = shcode[n];
                                    buy_sell_start[buy_sell_index] = start_value[n];
                                    buy_sell_lowest[buy_sell_index] = lowest_value[n];
                                    buy_sell_close[buy_sell_index] = close[m,n];
                                    buy_sell_gap[buy_sell_index] = gap_value[n] / (float)count[n];
                                    buy_sell_direction[buy_sell_index] = "buy";
                                    buy_sell_index++;

                                    resultList.Items.Add(delay_count[n]);
                                }
                                count[n] = 0;   // 카운터는 초기화, 다른 주식을 보유중에는 카운터가 초기화 되서 구매 하지 않게 됨
                                gap_value[n] = 0;
                                start_value[n] = 0;
                                lowest_value[n] = 0;
                            }                            
                        }
                        else if(buy_flag[n] == 1)   //주식을 보유 하고 있다면
                        {
                            if (close[m,n] >= ((buy_sell_close[buy_sell_index - 1] * buy_margin) / 100))    // 이익 실현 조건
                            {
                                // 구매 flag 해제    
                                global_buy_flag = 0;                                    
                                buy_flag[n] = 0;

                                // 판매 날짜, 구매 주식, 구매 가격, 구매 상태를 업데이트
                                buy_sell_date[buy_sell_index] = date[m];
                                buy_sell_stock[buy_sell_index] = shcode[n];
                                buy_sell_close[buy_sell_index] = close[m,n];
                                buy_sell_direction[buy_sell_index] = "sell";
                                buy_sell_index++;
                            }
                            else if (close[m,n] <= ((buy_sell_close[buy_sell_index - 1] * sell_margin) / 100))  // 손절매 조건
                            {
                                // 구매 flag 해제    
                                global_buy_flag = 0;                                    
                                buy_flag[n] = 0;

                                // 판매 날짜, 구매 주식, 구매 가격, 구매 상태를 업데이트
                                buy_sell_date[buy_sell_index] = date[m];
                                buy_sell_stock[buy_sell_index] = shcode[n];
                                buy_sell_close[buy_sell_index] = close[m,n];
                                buy_sell_direction[buy_sell_index] = "sell";
                                buy_sell_index++;
                            }

                            else if ((convert_date(date[m]) - convert_date(buy_sell_date[buy_sell_index-1])).Days > 180)
                            {
                                    // 구매 flag 해제    
                                    global_buy_flag = 0;
                                    buy_flag[n] = 0;

                                    // 판매 날짜, 구매 주식, 구매 가격, 구매 상태를 업데이트
                                    buy_sell_date[buy_sell_index] = date[m];
                                    buy_sell_stock[buy_sell_index] = shcode[n];
                                    buy_sell_close[buy_sell_index] = close[m, n];
                                    buy_sell_direction[buy_sell_index] = "sell";
                                    buy_sell_index++;
                             }
 
                        }
                    }
                }                    
            }

            // 시뮬레이션 결과 출력
            for (int d = 0; d < buy_sell_index; d++ )
            {
                if (buy_sell_direction[d].Equals("buy"))
                    resultList.Items.Add(buy_sell_date[d] + "\t" + buy_sell_stock[d] + "\t" + buy_sell_start[d] + "\t" + buy_sell_lowest[d] + "\t" + buy_sell_close[d] + "\t" + buy_sell_direction[d] + "\t" + buy_sell_gap[d]);
                else
                {
                    total_margin = total_margin * buy_sell_close[d] / buy_sell_close[d - 1];
                    resultList.Items.Add(buy_sell_date[d] + "\t" + buy_sell_stock[d] + "\t\t\t" + buy_sell_close[d] + "\t" + buy_sell_direction[d] + "\t\t" + total_margin);
                }
            }

            resultList.Items.Add("finish");
        }

        


        private void statistics_test_Click(object sender, EventArgs e)
        {
            // 데이터 숫자 정보
            //int day_num = 5000, stock_num = 1082;
            int day_num = 5000, stock_num = 1082;

            // 구매 여부를 결정하기 위한 파라매터   
            /*
            UInt64 buy_margin = Convert.ToUInt64(textBox2.Text);
            UInt64 sell_margin = Convert.ToUInt64(textBox3.Text);
            UInt64 low_count = Convert.ToUInt64(textBox4.Text);
            stock_num = Convert.ToInt32(textBox5.Text);
             * */

            /*
            UInt64 buy_margin = 120;
            UInt64 sell_margin = 80;
            UInt64 low_count = 80;
            UInt64 low_percent = 100;
            */

            // 자료 카운트를 위한 변수
            int count_data = 0;

            // 자료 취득을 위한 날짜, 주식 코드 정보

            /*
            string[] date = new string[day_num];
            string[] shcode = new string[1082];


            int BS_flag = 0;
            int SS_flag = 0;

            UInt64[] count = new UInt64[stock_num];
            UInt64[] sum_amount = new UInt64[stock_num];

            UInt64[] BS_start_price = new UInt64[stock_num];
            string[] BS_start_date = new string[stock_num];

            UInt64[] BS_low_price = new UInt64[stock_num];
            string[] BS_low_date = new string[stock_num];
            UInt64[] BS_low_amount = new UInt64[stock_num];
            UInt64[] BS_low_count = new UInt64[stock_num];

            UInt64[] BS_close = new UInt64[stock_num];
            string[] BS_close_date = new string[stock_num];

            float[] BS_gap_amount = new float[stock_num];

            UInt64[] SS_high_price = new UInt64[stock_num];
            string[] SS_high_date = new string[stock_num];
            UInt64[] SS_high_amount = new UInt64[stock_num];
            UInt64[] SS_high_count = new UInt64[stock_num];


            float[] SS_gap_amount = new float[stock_num];

            // 데이터 베이스 자료 저장용 버퍼
            string[,] s_date = new string[day_num, stock_num];
            UInt64[,] close = new UInt64[day_num, stock_num];
            UInt64[,] volume = new UInt64[day_num, stock_num];
            UInt64[,] amount = new UInt64[day_num, stock_num];
            UInt64[,] buy_price = new UInt64[day_num, stock_num];

            UInt64[,] stable_flag = new UInt64[day_num, stock_num];


            UInt64 print_index = 100000;

            string[] buy_sell_start_date = new string[print_index];
            UInt64[] buy_sell_start_price = new UInt64[print_index];

            string[] buy_sell_low_date = new string[print_index];
            UInt64[] buy_sell_low_price = new UInt64[print_index];
            UInt64[] buy_sell_low_days = new UInt64[print_index];
            UInt64[] buy_sell_low_amount = new UInt64[print_index];

            string[] buy_sell_close_date = new string[print_index];
            UInt64[] buy_sell_close = new UInt64[print_index];
            UInt64[] buy_sell_close_days = new UInt64[print_index];
            UInt64[] buy_sell_close_amount = new UInt64[print_index];

            float[] buy_sell_close_gap = new float[print_index];

            string[] buy_sell_high_date = new string[print_index];
            UInt64[] buy_sell_high_price = new UInt64[print_index];
            UInt64[] buy_sell_high_days = new UInt64[print_index];
            UInt64[] buy_sell_high_amount = new UInt64[print_index];

            string[] buy_sell_last_date = new string[print_index];
            UInt64[] buy_sell_last_price = new UInt64[print_index];
            UInt64[] buy_sell_last_days = new UInt64[print_index];
            UInt64[] buy_sell_last_amount = new UInt64[print_index];

            float[] buy_sell_last_gap = new float[print_index];

            string[] buy_sell_stock_code = new string[print_index];


            // 현재 구매 상태인지 확인하기 위한 변수
            int global_buy_flag = 0;

            // 매매로 인한 전체 마진 계산용
            float total_margin = 1;

            //List_box clear
            resultList.Items.Clear();


            //날짜를 불러 date에 저장
            MySqlCommand cmd;
            MySqlDataReader reader;

            count_data = 0;
            string sql = String.Format("select day from stock.date");
            cmd = new MySqlCommand(sql, conn);
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                date[count_data] = Convert.ToString(reader["day"]);
                count_data++;
            }

            reader.Close();
            reader.Dispose();


            // 주가 코드를 불러 shcode에 저장
            count_data = 0;
            sql = String.Format("select code from stock.stock_code");
            cmd = new MySqlCommand(sql, conn);
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                shcode[count_data] = Convert.ToString(reader["code"]);
                count_data++;
            }

            reader.Close();
            reader.Dispose();

            int buy_sell_index = 0;



            for (int j = 0; j < stock_num; j++)
            {
                UInt64 count_stable = 0;

                sql = String.Format("select * from stock.{0};", shcode[j]);
                cmd = new MySqlCommand(sql, conn);
                reader = cmd.ExecuteReader();

                count_data = 0;
                string a;

                while (reader.Read())
                {
                    a = Convert.ToString(reader["date"]);

                    while (date[count_data].Equals(a) == false)
                    {
                        if (Convert.ToInt32(date[count_data]) > Convert.ToInt32(a))
                        {
                            reader.Read();
                            a = Convert.ToString(reader["date"]);
                        }
                        else
                        {
                            s_date[count_data, j] = date[count_data];
                            close[count_data, j] = 0;
                            volume[count_data, j] = 0;
                            amount[count_data, j] = 0;
                            buy_price[count_data, j] = 0;
                            count_data++;
                        }
                    }

                    s_date[count_data, j] = Convert.ToString(reader["date"]);
                    close[count_data, j] = Convert.ToUInt64(reader["close"]);
                    volume[count_data, j] = Convert.ToUInt64(reader["volume"]);
                    amount[count_data, j] = Convert.ToUInt64(reader["amount"]);



                    if (count_data >= 1)
                    {
                        if (stable_flag[count_data - 1, j] == 1)
                        {
                            if (count_stable >= (amount[count_data, j] * 3))
                            {
                                stable_flag[count_data, j] = 0;
                                count_stable = 0;
                            }
                            else
                            {
                                stable_flag[count_data, j] = 1;
                                count_stable = count_stable + volume[count_data, j];
                            }
                        }


                        if ((amount[count_data - 1, j] >= (amount[count_data, j] + 500)) || (amount[count_data, j] >= (amount[count_data - 1, j] + 500)))
                        {
                            buy_price[count_data, j] = close[count_data, j];
                            stable_flag[count_data, j] = 1;
                        }
                        else
                        {
                            // 수량 * 전날 구매 가격 
                            buy_price[count_data, j] = ((amount[count_data - 1, j] * buy_price[count_data - 1, j])
                                                         - (volume[count_data, j] * buy_price[count_data - 1, j])
                                                         + (volume[count_data, j] - (amount[count_data, j] - amount[count_data - 1, j])) * close[count_data, j])
                                                          / amount[count_data, j];
                        }
                    }
                    else if (count_data == 0)
                    {
                        buy_price[count_data, j] = close[count_data, j];
                        stable_flag[count_data, j] = 1;
                    }

                    count_data++;

                }

                reader.Close();
                reader.Dispose();
            }

            stock_num = 1082;

            resultList.Items.Add("SD" + "\t" + "SP" + "\t" + "LD" + "\t" + "LP" + "\t" +
                                 "LDAYs" + "\t" + "LA" + "\t  " + "CD" + "\t" + "CP" + "\t" +
                                 "CDAYs" + "\t" + "CA" + "\t  " + "CG" + "\t" + "HD" + "\t" +
                                 "HP" + "\t" + "HDAYs" + "\t" + "HA" + "\t" + "LD" + "\t" +
                                 "LP" + "\t" + "LDAYs" + "\t" + "LA" + "\t  " + "LG" + "\t" + "L/C" + "\t" + "H/C");


            for (int n = 0; n < stock_num; n++)
            {
                count[n] = 0;

                for (int m = 1; m < day_num; m++)
                {
                    if (stable_flag[m, n] == 0)
                    {
                        // 종가가 buy_price 보다 싸면
                        if (close[m, n] < buy_price[m, n])      // 종가가 구매가보다 쌀 때 -> 구매 루틴 진행
                        {
                            if (SS_flag == 1)
                            {
                                buy_sell_high_date[buy_sell_index] = SS_high_date[n];
                                buy_sell_high_price[buy_sell_index] = SS_high_price[n];
                                buy_sell_high_days[buy_sell_index] = SS_high_count[n];
                                buy_sell_high_amount[buy_sell_index] = SS_high_amount[n];

                                buy_sell_last_date[buy_sell_index] = date[m];
                                buy_sell_last_price[buy_sell_index] = close[m, n];
                                buy_sell_last_days[buy_sell_index] = count[n];            //구매 루틴 전체 일자 저장
                                buy_sell_last_amount[buy_sell_index] = sum_amount[n];     //구매 루틴 전체 거래량 저장

                                buy_sell_last_gap[buy_sell_index] = (float)(SS_gap_amount[n]) / (float)(sum_amount[n]);

                                buy_sell_stock_code[buy_sell_index] = shcode[n];

                                buy_sell_index++;

                                BS_flag = 0;
                                SS_flag = 0;
                            }


                            {
                                if (BS_flag == 0)
                                {
                                    // 초기화
                                    BS_flag = 0;
                                    SS_flag = 0;

                                    count[n] = 0;
                                    sum_amount[n] = 0;

                                    BS_start_price[n] = 0;
                                    BS_start_date[n] = "0";

                                    BS_low_price[n] = 0;
                                    BS_low_date[n] = "0";
                                    BS_low_amount[n] = 0;
                                    BS_low_count[n] = 0;

                                    BS_gap_amount[n] = 0;

                                    SS_high_price[n] = 0;
                                    SS_high_date[n] = "0";
                                    SS_high_amount[n] = 0;
                                    SS_high_count[n] = 0;

                                    buy_sell_last_price[buy_sell_index] = 0;
                                    buy_sell_last_date[buy_sell_index] = "0";
                                    buy_sell_last_days[buy_sell_index] = 0;

                                    SS_gap_amount[n] = 0;

                                    //초기화 종료

                                    BS_flag = 1;

                                    count[n] = 1;                   // 구매 루틴 시작

                                    BS_start_price[n] = close[m, n];  //구매 루틴 시작 가격
                                    BS_start_date[n] = date[m];     //구매 루틴 시작 일자

                                    BS_low_price[n] = close[m, n];   //구매 루틴 최저가
                                    BS_low_date[n] = date[m];       //구매 루틴 최저가 기록 날짜

                                    sum_amount[n] = sum_amount[n] + amount[m, n];    //구매루틴 시작시 거래 수량
                                    BS_low_amount[n] = sum_amount[n];               //구래 루틴을 시작한 때 부터 최저가를 기록한 날짜까지의 누적 거래 수량

                                    BS_gap_amount[n] = (float)amount[m, n] * ((float)(buy_price[m, n] - close[m, n]) / (float)buy_price[m, n]);   //구매 루틴 시작 후 거래 금액을 합산
                                    BS_low_count[n] = count[n];                     //구매 루틴 시작 후 일자 숫자
                                }
                                else
                                {
                                    count[n]++;
                                    sum_amount[n] = sum_amount[n] + amount[m, n];    //구매 루틴의 거래량 합산
                                    BS_gap_amount[n] = (float)amount[m, n] * ((float)(buy_price[m, n] - close[m, n]) / (float)buy_price[m, n]);    //구매 루틴 시작 후 거래 금액을 합산

                                    if (BS_low_price[n] > close[m, n])                //최저가 갱신시
                                    {
                                        BS_low_price[n] = close[m, n];               //최저가 업데이트 
                                        BS_low_date[n] = date[m];                   //최저가 갱신일 업데이트
                                        BS_low_amount[n] = sum_amount[n];           //최저가 갱신시까지 거래 수량 업데이트
                                        BS_low_count[n] = count[n];                 //구매 루틴 시작 후 일자 숫자
                                    }

                                }
                            }
                        }
                        else if (close[m, n] >= buy_price[m, n])
                        {
                            if ((BS_flag == 1) && (SS_flag == 0))
                            {
                                SS_flag = 1;
                                buy_sell_start_date[buy_sell_index] = BS_start_date[n];     //구매 루틴 시작 일자 저장
                                buy_sell_start_price[buy_sell_index] = BS_start_price[n];    //구매 루틴 시작 가격 저장

                                buy_sell_low_date[buy_sell_index] = BS_low_date[n];         //구매 루틴 중 최저가 기록 일자 저장
                                buy_sell_low_price[buy_sell_index] = BS_low_price[n];       //구매 루틴 중 최저가 저장
                                buy_sell_low_days[buy_sell_index] = BS_low_count[n];        //구매 루틴 중 최저가를 기록하기 까지 일자 저장
                                buy_sell_low_amount[buy_sell_index] = BS_low_amount[n];        //구매 루틴 중 최저가 까지 거래량 저장



                                BS_close_date[n] = date[m];
                                BS_close[n] = close[m, n];

                                buy_sell_close_date[buy_sell_index] = BS_close_date[n];    //구매 루틴 종료 일자(구매 시점) 저장
                                buy_sell_close[buy_sell_index] = BS_close[n];              //구매 루틴 종료 시 가격 저장

                                buy_sell_close_days[buy_sell_index] = count[n];            //구매 루틴 전체 일자 저장
                                buy_sell_close_amount[buy_sell_index] = sum_amount[n];     //구매 루틴 전체 거래량 저장

                                buy_sell_close_gap[buy_sell_index] = (float)BS_gap_amount[n] / (float)(sum_amount[n]);  //구매 루틴 의 close-buy_price의 결합도,  0에 가까울 수록 완전 결합  

                                sum_amount[n] = 0;
                                count[n] = 1;                   // 판매 루틴 시작

                                SS_high_price[n] = close[m, n];   //판매 루틴 최저가
                                SS_high_date[n] = date[m];       //판매 루틴 최저가 기록 날짜

                                sum_amount[n] = 0;
                                sum_amount[n] = sum_amount[n] + amount[m, n];    //판매루틴 시작시 거래 수량
                                SS_high_amount[n] = sum_amount[n];               //판매 루틴을 시작한 때 부터 최저가를 기록한 날짜까지의 누적 거래 수량

                                SS_gap_amount[n] = (float)amount[m, n] * (float)(close[m, n] - buy_price[m, n]) / (float)close[m, n];   //판매 루틴 시작 후 거래 금액을 합산

                                SS_high_count[n] = count[n];                     //판매 루틴 시작 후 일자 숫자

                            }
                            else if ((BS_flag == 1) && (SS_flag == 1))
                            {
                                count[n]++;
                                sum_amount[n] = sum_amount[n] + amount[m, n];    //판매 루틴의 거래량 합산
                                SS_gap_amount[n] = SS_gap_amount[n] + (float)amount[m, n] * (float)(close[m, n] - buy_price[m, n]) / (float)close[m, n];    //판매 루틴 시작 후 거래 금액을 합산

                                if (SS_high_price[n] < close[m, n])
                                {
                                    SS_high_price[n] = close[m, n];   //판매 루틴 최고가
                                    SS_high_date[n] = date[m];       //판매 루틴 최고가 기록 날짜

                                    sum_amount[n] = sum_amount[n] + amount[m, n];    //판매루틴 시작시 거래 수량
                                    SS_high_amount[n] = sum_amount[n];              //판매 루틴을 시작한 때 부터 최저가를 기록한 날짜까지의 누적 거래 수량

                                    SS_high_count[n] = count[n];                 //구매 루틴 시작 후 일자 숫자

                                }

                                if (count[n] >= 60)
                                {
                                    buy_sell_high_date[buy_sell_index] = SS_high_date[n];
                                    buy_sell_high_price[buy_sell_index] = SS_high_price[n];
                                    buy_sell_high_days[buy_sell_index] = SS_high_count[n];
                                    buy_sell_high_amount[buy_sell_index] = SS_high_amount[n];

                                    buy_sell_last_date[buy_sell_index] = date[m];
                                    buy_sell_last_price[buy_sell_index] = close[m, n];
                                    buy_sell_last_days[buy_sell_index] = count[n];            //구매 루틴 전체 일자 저장
                                    buy_sell_last_amount[buy_sell_index] = sum_amount[n];     //구매 루틴 전체 거래량 저장

                                    buy_sell_last_gap[buy_sell_index] = (float)(SS_gap_amount[n]) / (float)(sum_amount[n]);

                                    buy_sell_stock_code[buy_sell_index] = shcode[n];

                                    buy_sell_index++;

                                    BS_flag = 0;
                                    SS_flag = 0;
                                }
                            }
                        }
                    }
                    else
                    {
                        BS_flag = 0;
                        SS_flag = 0;

                        count[n] = 0;
                        sum_amount[n] = 0;

                        BS_start_price[n] = 0;
                        BS_start_date[n] = "0";

                        BS_low_price[n] = 0;
                        BS_low_date[n] = "0";
                        BS_low_amount[n] = 0;
                        BS_low_count[n] = 0;

                        BS_gap_amount[n] = 0;

                        SS_high_price[n] = 0;
                        SS_high_date[n] = "0";
                        SS_high_amount[n] = 0;
                        SS_high_count[n] = 0;

                        buy_sell_last_price[buy_sell_index] = 0;
                        buy_sell_last_date[buy_sell_index] = "0";
                        buy_sell_last_days[buy_sell_index] = 0;

                        SS_gap_amount[n] = 0;
                    }
                }
            }

            // 시뮬레이션 결과 출력
            for (int d = 0; d < buy_sell_index; d++)
            {

                string t = "\t";
                string aaa = String.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10:D10}{11}{12}{13}{14}{15}{16}{17}{18:D10}{19}{20:F1}{21}{22}{23}{24}{25}{26}{27}{28}{29}{30}{31}{32}{33}{34}{35}{36:D10}{37:F1}{38:E1}{39}{40}{41}{42}{43}{44}", buy_sell_start_date[d], t, buy_sell_start_price[d], t, buy_sell_low_date[d], t, buy_sell_low_price[d], t, buy_sell_low_days[d], t,
                                                                                                                                                                                                                               buy_sell_low_amount[d], "  ", buy_sell_close_date[d], " ", buy_sell_close[d], t, buy_sell_close_days[d], t, buy_sell_close_amount[d], "  ",
                                                                                                                                                                                                                               buy_sell_close_gap[d], t, buy_sell_high_date[d], t, buy_sell_high_price[d], t, buy_sell_high_days[d], t, buy_sell_high_amount[d], t,
                                                                                                                                                                                                                               buy_sell_last_date[d], t, buy_sell_last_price[d], t, buy_sell_last_days[d], t, buy_sell_last_amount[d], "  ", buy_sell_last_gap[d], t,
                                                                                                                                                                                                                               (float)(buy_sell_last_price[d]) / (float)(buy_sell_close[d]), t, (float)(buy_sell_high_price[d]) / (float)(buy_sell_close[d]), t, d);
                resultList.Items.Add(aaa);
            }

            resultList.Items.Add("finish");



            try
            {

                sql = String.Format("CREATE TABLE `stock`.`analysis` (`index` INT NOT NULL, `BS_start_date` VARCHAR(8) NULL, `BS_start_price` INT NULL, `BS_lowest_date` VARCHAR(8) NULL, `BS_lowest_price` INT NULL, `BS_lowest_days` INT NULL, `BS_lowest_amount` INT(64) NULL, `BS_close_date`  VARCHAR(8) NULL, `BS_close_price`  INT NULL, `BS_close_days`  INT NULL, `BS_close_amount`  INT(64) NULL, `BS_close_gap`  FLOAT NULL,`SS_high_date`  VARCHAR(8) NULL, `SS_high_price`  INT NULL, `SS_high_days`  INT NULL, `SS_high_amount`  INT(64) NULL, `SS_last_date` VARCHAR(8) NULL, `SS_last_price` INT NULL, `SS_last_days` INT NULL, `SS_last_amount` INT(64) NULL, `SS_last_gap` FLOAT NULL, `Last_margin` FLOAT NULL, `High_margin` FLOAT NULL, `Stock code` VARCHAR(8) NULL,PRIMARY KEY (`index`) );");

                cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException a)
            {
                // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
            }

            // 수신한 코드 정보를 처리
            for (int idx = 0; idx < buy_sell_index; idx++)
            {
                // List box에 출력
                resultList.Items.Add("analysis save");

                // database에 저장
                try
                {
                    sql = String.Format("insert into `stock`.`analysis` values ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}','{19}','{20}','{21}','{22}',{23})",
                                                                                                                                                                                      idx, buy_sell_start_date[idx], buy_sell_start_price[idx], buy_sell_low_date[idx], buy_sell_low_price[idx], buy_sell_low_days[idx],
                                                                                                                                                                                      buy_sell_low_amount[idx], buy_sell_close_date[idx], buy_sell_close[idx], buy_sell_close_days[idx], buy_sell_close_amount[idx],
                                                                                                                                                                                      buy_sell_close_gap[idx], buy_sell_high_date[idx], buy_sell_high_price[idx], buy_sell_high_days[idx], buy_sell_high_amount[idx],
                                                                                                                                                                                      buy_sell_last_date[idx], buy_sell_last_price[idx], buy_sell_last_days[idx], buy_sell_last_amount[idx], buy_sell_last_gap[idx],
                                                                                                                                                                                      (float)(buy_sell_last_price[idx]) / (float)(buy_sell_close[idx]), (float)(buy_sell_high_price[idx]) / (float)(buy_sell_close[idx]), buy_sell_stock_code[idx]);
                    cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException d)
                {
                    // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
                }
            }
             */



        }

        // time delay ms
        private static DateTime Delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime AfterWards = ThisMoment.Add(duration);

            while (AfterWards >= ThisMoment)
            {
                System.Windows.Forms.Application.DoEvents();
                ThisMoment = DateTime.Now;
            }

            return DateTime.Now;
        }
        private static DateTime convert_date(string date)
        {
            if (date == null)
            {
                date = "19991212";
            }


            int y = Convert.ToInt32(date.Substring(0, 4));
            int m = Convert.ToInt32(date.Substring(4).Substring(0, 2));
            int d = Convert.ToInt32(date.Substring(6));

            DateTime a = new DateTime(y, m, d);
            return a;
        }






        


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click_2(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }


        private void button2_Click_1(object sender, EventArgs e)
        {



            //List_box clear
            resultList.Items.Clear();


            //날짜를 불러 date에 저장
            MySqlCommand cmd;
            MySqlDataReader reader;

            count_data = 0;
            string sql = String.Format("select day from stock.date");
            cmd = new MySqlCommand(sql, conn);
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                date[count_data] = Convert.ToString(reader["day"]);
                count_data++;
            }

            reader.Close();
            reader.Dispose();


            // 주가 코드를 불러 shcode에 저장
            count_data = 0;
            sql = String.Format("select code from stock.marketcap_desc_code");
            cmd = new MySqlCommand(sql, conn);
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                shcode[count_data] = Convert.ToString(reader["code"]);
                count_data++;
            }

            reader.Close();
            reader.Dispose();


            for (int j = 0; j < stock_num; j++)
            {
                UInt64 count_stable = 0;

                sql = String.Format("select * from stock.{0};", shcode[j]);
                cmd = new MySqlCommand(sql, conn);
                reader = cmd.ExecuteReader();

                count_data = 0;
                string a;

                while (reader.Read())
                {
                    a = Convert.ToString(reader["date"]);

                    while (date[count_data].Equals(a) == false)
                    {
                        if (Convert.ToInt32(date[count_data]) > Convert.ToInt32(a))
                        {
                            reader.Read();
                            a = Convert.ToString(reader["date"]);
                        }
                        else
                        {
                            s_date[count_data, j] = date[count_data];
                            close[count_data, j] = 0;
                            volume[count_data, j] = 0;
                            amount[count_data, j] = 0;
                            buy_price[count_data, j] = 0;
                            count_data++;
                        }
                    }

                    s_date[count_data, j] = Convert.ToString(reader["date"]);
                    close[count_data, j] = Convert.ToUInt64(reader["close"]);
                    volume[count_data, j] = Convert.ToUInt64(reader["volume"]);
                    amount[count_data, j] = Convert.ToUInt64(reader["amount"]);



                    if (count_data >= 1)
                    {
                        if (stable_flag[count_data - 1, j] == 1)
                        {
                            if (count_stable >= (amount[count_data, j] * 3))
                            {
                                stable_flag[count_data, j] = 0;
                                count_stable = 0;
                            }
                            else
                            {
                                stable_flag[count_data, j] = 1;
                                count_stable = count_stable + volume[count_data, j];
                            }
                        }


                        if ( (amount[count_data - 1, j] >= (amount[count_data, j] + 500)) || (amount[count_data, j] >= (amount[count_data - 1, j] + 500)))
                        {
                            buy_price[count_data, j] = close[count_data, j];
                            stable_flag[count_data, j] = 1;
                        }
                        else
                        {
                            // 수량 * 전날 구매 가격 
                            buy_price[count_data, j] = ((amount[count_data - 1, j] * buy_price[count_data - 1, j])
                                                         - (volume[count_data, j] * buy_price[count_data - 1, j])
                                                         + (volume[count_data, j] - (amount[count_data, j] - amount[count_data - 1, j])) * close[count_data, j])
                                                          / amount[count_data, j];
                        }
                    }
                    else if (count_data == 0)
                    {
                        buy_price[count_data, j] = close[count_data, j];
                        stable_flag[count_data, j] = 1;
                    }

                    count_data++;     
  
                }

                reader.Close();
                reader.Dispose();
            }

            for (int j = 0; j < 100; j++)
            {
                buy_sell_index = 0;
                buy_flag = 0;
                total_margin = 1;

                for (int n = 0; n < stock_num; n++)
                {
                    state[n] = 0;
                    BS_start_date[n] = "0";
                    BS_start_price[n] = 0;
                    BS_buy_date[n] = "0";
                    BS_buy_price[n] = 0;
                    BS_last_date[n] = "0";
                    BS_last_price[n] = 0;
                }

                for (int m = 1; m < day_num-1; m++)
                {
                    for (int n = 0; n < stock_num; n++)
                    {
                        if (stable_flag[m, n] == 0)     // 데이터가 안정된 상태                    
                        {
                            if (state[n] == 0) // 아무 상태가 아님
                            {
                                // 종가가 buy_price 보다 싸면
                                if (close[m, n] < buy_price[m, n])      // 종가가 구매가보다 쌀 때 -> 구매 루틴 진행
                                {
                                    state[n] = 1;    //구매 준비 상태
                                    BS_start_price[n] = close[m, n];    //구매루틴 시작 가격을 저장
                                    BS_start_date[n] = s_date[m, n];    //구매루틴 시작 날짜를 저장
                                }
                            }
                            else if (state[n] == 1)  // 구매 루틴 진행 상태
                            {
                                if (close[m, n] >= buy_price[m, n]) // 현재 가격이 기준 가격 보다 크고
                                {
                                    //if ((float)close[m, n] >= ((float)BS_start_price[n] * (1.32)))    // 구매 루틴 시작 값 보다 20% 클 때
                                    if ((float)close[m, n] >= ((float)BS_start_price[n] * (0.8+j*0.01)))    // 구매 루틴 시작 값 보다 20% 클 때
                                    {
                                        if (buy_flag == 0)   //현재 산 주식이 없을 때
                                        {
                                            //구매
                                            buy_flag = 1;
                                            state[n] = 2;
                                            BS_buy_price[n] = close[m, n];
                                            BS_buy_date[n] = s_date[m, n];

                                            buy_sell_stock_code[buy_sell_index] = shcode[n];   //코드
                                            buy_sell_start_date[buy_sell_index] = BS_start_date[n];
                                            buy_sell_start_price[buy_sell_index] = BS_start_price[n];
                                            buy_sell_close_date[buy_sell_index] = BS_buy_date[n];
                                            buy_sell_close[buy_sell_index] = BS_buy_price[n];

                                            buy_sell_last_date[buy_sell_index] = "0";
                                            buy_sell_last_price[buy_sell_index] = 0;
                                            //total_margin = total_margin + ((float)buy_sell_last_price[buy_sell_index] / (float)buy_sell_close[buy_sell_index] * 9967 / 10000) - 1;
                                            buy_sell_margin[buy_sell_index] = total_margin;
                                        }
                                        else
                                        {
                                            state[n] = 0;   //초기화
                                            BS_start_price[n] = 0;
                                            BS_start_date[n] = "0";
                                        }
                                    }
                                }
                            }
                            else if (state[n] == 2)  // 판매 루틴 진행 상태
                            {
                                if ((float)close[m, n] >= ((float)BS_buy_price[n] * 1.01))    // 구매 가격보다 1% 올랐을 때 판매
                                {
                                    buy_flag = 0;
                                    state[n] = 0;

                                    BS_last_price[n] = close[m, n];
                                    BS_last_date[n] = s_date[m, n];

                                    //구매해서 판매가 완료 되었을 때 기록 저장
                                    buy_sell_stock_code[buy_sell_index] = shcode[n];   //코드
                                    buy_sell_start_date[buy_sell_index] = BS_start_date[n];
                                    buy_sell_start_price[buy_sell_index] = BS_start_price[n];
                                    buy_sell_close_date[buy_sell_index] = BS_buy_date[n];
                                    buy_sell_close[buy_sell_index] = BS_buy_price[n];
                                    buy_sell_last_date[buy_sell_index] = BS_last_date[n];
                                    buy_sell_last_price[buy_sell_index] = BS_last_price[n];
                                    total_margin = total_margin * ((float)buy_sell_last_price[buy_sell_index] / (float)buy_sell_close[buy_sell_index] * 9967 / 10000);
                                    buy_sell_margin[buy_sell_index] = total_margin;
                                    buy_sell_index++;
                                }
                                else if ((float)close[m, n] <= ((float)BS_buy_price[n] * 1004 / 1000))    // 구매 가격과 같거나 작아졌을 때도 판매
                                {
                                    buy_flag = 0;
                                    state[n] = 0;

                                    BS_last_price[n] = close[m, n];
                                    BS_last_date[n] = s_date[m, n];

                                    //구매해서 판매가 완료 되었을 때 기록 저장
                                    buy_sell_stock_code[buy_sell_index] = shcode[n];   //코드
                                    buy_sell_start_date[buy_sell_index] = BS_start_date[n];
                                    buy_sell_start_price[buy_sell_index] = BS_start_price[n];
                                    buy_sell_close_date[buy_sell_index] = BS_buy_date[n];
                                    buy_sell_close[buy_sell_index] = BS_buy_price[n];
                                    buy_sell_last_date[buy_sell_index] = BS_last_date[n];
                                    buy_sell_last_price[buy_sell_index] = BS_last_price[n];
                                    total_margin = total_margin * ((float)buy_sell_last_price[buy_sell_index] / (float)buy_sell_close[buy_sell_index] * 9967 / 10000);
                                    buy_sell_margin[buy_sell_index] = total_margin;
                                    buy_sell_index++;
                                }
                            }
                        }
                        else if ((stable_flag[m, n] == 1) && (stable_flag[m - 1, n] == 0))
                        {
                            /*
                            if (state[n] == 2)
                            {
                                BS_last_price[n] = close[m, n];
                                BS_last_date[n] = s_date[m, n];

                                //구매해서 판매가 완료 되었을 때 기록 저장
                                buy_sell_stock_code[buy_sell_index] = shcode[n];   //코드
                                buy_sell_start_date[buy_sell_index] = BS_start_date[n];
                                buy_sell_start_price[buy_sell_index] = BS_start_price[n];
                                buy_sell_close_date[buy_sell_index] = BS_buy_date[n];
                                buy_sell_close[buy_sell_index] = BS_buy_price[n];
                                buy_sell_last_date[buy_sell_index] = BS_last_date[n];
                                buy_sell_last_price[buy_sell_index] = BS_last_price[n];
                                total_margin = total_margin * (float)buy_sell_last_price[buy_sell_index] / (float)buy_sell_close[buy_sell_index] * 9967 / 10000;
                                buy_sell_margin[buy_sell_index] = total_margin;
                                buy_sell_index++;
                            }
                            */

                            state[n] = 0;

                            if (BS_buy_price[n] != 0)
                                buy_flag = 0;

                            BS_start_date[n] = "0";
                            BS_start_price[n] = 0;
                            BS_buy_date[n] = "0";
                            BS_buy_price[n] = 0;
                            BS_last_date[n] = "0";
                            BS_last_price[n] = 0;
                        }
                    }
                }

                if (buy_flag == 1)
                    buy_sell_index++;

                resultList.Items.Add("buy threshold : " + (0.8+(j*0.01)));
                //resultList.Items.Add("buy threshold : " + (1.34));
                resultList.Items.Add("****************************************************************************************");
                resultList.Items.Add("code" + "\t" + "SP" + "\t" + "BP" + "\t" + "LP" + "\t" + "SD" + "\t" + "BD" + "\t  " + "LD" + "\t  " + "margine");

                // 시뮬레이션 결과 출력
                for (int d = 0; d < (buy_sell_index); d++)
                {
                    string t = "\t";
                    
                    string aaa = String.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}", buy_sell_stock_code[d], t,buy_sell_start_price[d], t, buy_sell_close[d], t, buy_sell_last_price[d], t,
                                                                                                 buy_sell_start_date[d], t, buy_sell_close_date[d], t, buy_sell_last_date[d], t, buy_sell_margin[d]); 

                    resultList.Items.Add(aaa);
                }

                string bbb = "finish : " + buy_sell_index;
                resultList.Items.Add(bbb);

            
            
                try
                {

                    sql = String.Format("CREATE TABLE `stock`.`analysis{0}` (`index` INT NOT NULL, `CODE` VARCHAR(8) NULL, `SP` INT NULL, `BP` VARCHAR(8) NULL, `LP` INT NULL, `SD` VARCHAR(8) NULL, `BD` VARCHAR(8) NULL, `LD`  VARCHAR(8) NULL , `Margin` float NULL, PRIMARY KEY (`index`) );", (80+j));
               
                    cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException a)
                {
                    // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
                }
            
                // 수신한 코드 정보를 처리
                for (int idx = 0; idx < buy_sell_index; idx++)
                {
                    // database에 저장
                    try
                    {
                        sql = String.Format("insert into `stock`.`analysis{0}` values ('{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}')",  (80+j),idx, buy_sell_stock_code[idx], buy_sell_start_price[idx], buy_sell_close[idx], buy_sell_last_price[idx], 
                                                                                                                                                   buy_sell_start_date[idx], buy_sell_close_date[idx], buy_sell_last_date[idx], buy_sell_margin[idx]);
                        cmd = new MySqlCommand(sql, conn);
                        cmd.ExecuteNonQuery();
                    }
                    catch (MySqlException d)
                    {
                        // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
                    }
                }
             
             

            }

        }

        private void button3_Click(object sender, EventArgs e)
        {

            int win_count = 0;
            int lose_count = 0;

            float all_margin = 1;

            float ave_margin = 0;

            //List_box clear
            resultList.Items.Clear();


            //날짜를 불러 date에 저장
            MySqlCommand cmd;
            MySqlDataReader reader;

            count_data = 0;
            string sql = String.Format("select day from stock.date");
            cmd = new MySqlCommand(sql, conn);
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                date[count_data] = Convert.ToString(reader["day"]);
                count_data++;
            }

            reader.Close();
            reader.Dispose();


            // 주가 코드를 불러 shcode에 저장
            count_data = 0;
            sql = String.Format("select code from stock.stock_code");
            cmd = new MySqlCommand(sql, conn);
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                shcode[count_data] = Convert.ToString(reader["code"]);
                count_data++;
            }

            reader.Close();
            reader.Dispose();


            for (int j = 0; j < stock_num; j++)
            {
                UInt64 count_stable = 0;

                sql = String.Format("select * from stock.{0};", shcode[j]);
                cmd = new MySqlCommand(sql, conn);
                reader = cmd.ExecuteReader();

                count_data = 0;
                string a;

                while (reader.Read())
                {
                    a = Convert.ToString(reader["date"]);

                    while (date[count_data].Equals(a) == false)
                    {
                        if (Convert.ToInt32(date[count_data]) > Convert.ToInt32(a))
                        {
                            reader.Read();
                            a = Convert.ToString(reader["date"]);
                        }
                        else
                        {
                            s_date[count_data, j] = date[count_data];
                            close[count_data, j] = 0;
                            volume[count_data, j] = 0;
                            amount[count_data, j] = 0;
                            buy_price[count_data, j] = 0;
                            count_data++;
                        }
                    }

                    s_date[count_data, j] = Convert.ToString(reader["date"]);
                    close[count_data, j] = Convert.ToUInt64(reader["close"]);
                    volume[count_data, j] = Convert.ToUInt64(reader["volume"]);
                    amount[count_data, j] = Convert.ToUInt64(reader["amount"]);



                    if (count_data >= 1)
                    {
                        if (stable_flag[count_data - 1, j] == 1)
                        {
                            if (count_stable >= (amount[count_data, j] * 3))
                            {
                                stable_flag[count_data, j] = 0;
                                count_stable = 0;
                            }
                            else
                            {
                                stable_flag[count_data, j] = 1;
                                count_stable = count_stable + volume[count_data, j];
                            }
                        }


                        if ((amount[count_data - 1, j] >= (amount[count_data, j] + 500)) || (amount[count_data, j] >= (amount[count_data - 1, j] + 500)))
                        {
                            buy_price[count_data, j] = close[count_data, j];
                            stable_flag[count_data, j] = 1;
                        }
                        else
                        {
                            // 수량 * 전날 구매 가격 
                            buy_price[count_data, j] = ((amount[count_data - 1, j] * buy_price[count_data - 1, j])
                                                         - (volume[count_data, j] * buy_price[count_data - 1, j])
                                                         + (volume[count_data, j] - (amount[count_data, j] - amount[count_data - 1, j])) * close[count_data, j])
                                                          / amount[count_data, j];
                        }
                    }
                    else if (count_data == 0)
                    {
                        buy_price[count_data, j] = close[count_data, j];
                        stable_flag[count_data, j] = 1;
                    }

                    count_data++;

                }

                reader.Close();
                reader.Dispose();
            }

            //for (int j = 0; j < 100; j++)
            {
                buy_sell_index = 0;
                buy_flag = 0;
                total_margin = 1;

                for (int n = 0; n < stock_num; n++)
                {
                    state[n] = 0;
                    BS_start_date[n] = "0";
                    BS_start_price[n] = 0;
                    BS_buy_date[n] = "0";
                    BS_buy_price[n] = 0;
                    BS_last_date[n] = "0";
                    BS_last_price[n] = 0;
                }

                for (int n = 0; n < stock_num; n++) 
                {

                    buy_sell_index = 0;
                    buy_flag = 0;
                    total_margin = 1;

                    state[n] = 0;
                    BS_start_date[n] = "0";
                    BS_start_price[n] = 0;
                    BS_buy_date[n] = "0";
                    BS_buy_price[n] = 0;
                    BS_last_date[n] = "0";
                    BS_last_price[n] = 0;

                    for (int m = 1; m < day_num; m++)
                    {
                        if (stable_flag[m, n] == 0)     // 데이터가 안정된 상태                    
                        {
                            if (state[n] == 0) // 아무 상태가 아님
                            {
                                // 종가가 buy_price 보다 싸면
                                if (close[m, n] < buy_price[m, n])      // 종가가 구매가보다 쌀 때 -> 구매 루틴 진행
                                {
                                    state[n] = 1;    //구매 준비 상태
                                    BS_start_price[n] = close[m, n];    //구매루틴 시작 가격을 저장
                                    BS_start_date[n] = s_date[m, n];    //구매루틴 시작 날짜를 저장
                                }
                            }
                            else if (state[n] == 1)  // 구매 루틴 진행 상태
                            {
                                if (close[m, n] >= buy_price[m, n]) // 현재 가격이 기준 가격 보다 크고
                                {
                                    if ((float)close[m, n] >= ((float)BS_start_price[n] * (1.34)) )    // 구매 루틴 시작 값 보다 20% 클 때
                                    {
                                        if (buy_flag == 0)   //현재 산 주식이 없을 때
                                        {
                                            //구매
                                            buy_flag = 1;
                                            state[n] = 2;
                                            BS_buy_price[n] = close[m, n];
                                            BS_buy_date[n] = s_date[m, n];
                                        }
                                        else
                                        {
                                            state[n] = 0;   //초기화
                                            BS_start_price[n] = 0;
                                            BS_start_date[n] = "0";
                                        }
                                    }
                                }
                            }
                            else if (state[n] == 2)  // 판매 루틴 진행 상태
                            {
                                if ((float)close[m, n] >= ((float)BS_buy_price[n] * 1.01))    // 구매 가격보다 1% 올랐을 때 판매
                                {
                                    buy_flag = 0;
                                    state[n] = 0;

                                    BS_last_price[n] = close[m, n];
                                    BS_last_date[n] = s_date[m, n];

                                    //구매해서 판매가 완료 되었을 때 기록 저장
                                    buy_sell_stock_code[buy_sell_index] = shcode[n];   //코드
                                    buy_sell_start_date[buy_sell_index] = BS_start_date[n];
                                    buy_sell_start_price[buy_sell_index] = BS_start_price[n];
                                    buy_sell_close_date[buy_sell_index] = BS_buy_date[n];
                                    buy_sell_close[buy_sell_index] = BS_buy_price[n];
                                    buy_sell_last_date[buy_sell_index] = BS_last_date[n];
                                    buy_sell_last_price[buy_sell_index] = BS_last_price[n];
                                    total_margin = total_margin * (float)buy_sell_last_price[buy_sell_index] / (float)buy_sell_close[buy_sell_index] * 9967 / 10000;
                                    buy_sell_margin[buy_sell_index] = total_margin;
                                    buy_sell_index++;
                                    win_count++;
                                }
                                else if ((float)close[m, n] <= ((float)BS_buy_price[n] * 1004 / 1000))    // 구매 가격과 같거나 작아졌을 때도 판매
                                {
                                    buy_flag = 0;
                                    state[n] = 0;

                                    BS_last_price[n] = close[m, n];
                                    BS_last_date[n] = s_date[m, n];

                                    //구매해서 판매가 완료 되었을 때 기록 저장
                                    buy_sell_stock_code[buy_sell_index] = shcode[n];   //코드
                                    buy_sell_start_date[buy_sell_index] = BS_start_date[n];
                                    buy_sell_start_price[buy_sell_index] = BS_start_price[n];
                                    buy_sell_close_date[buy_sell_index] = BS_buy_date[n];
                                    buy_sell_close[buy_sell_index] = BS_buy_price[n];
                                    buy_sell_last_date[buy_sell_index] = BS_last_date[n];
                                    buy_sell_last_price[buy_sell_index] = BS_last_price[n];
                                    total_margin = total_margin * (float)buy_sell_last_price[buy_sell_index] / (float)buy_sell_close[buy_sell_index] * 9967 / 10000;
                                    buy_sell_margin[buy_sell_index] = total_margin;
                                    buy_sell_index++;
                                    lose_count++;
                                }
                            }
                        }
                        else if ((stable_flag[m, n] == 1) && (stable_flag[m - 1, n] == 0))
                        {
                            /*
                            if (state[n] == 2)
                            {
                                BS_last_price[n] = close[m, n];
                                BS_last_date[n] = s_date[m, n];

                                //구매해서 판매가 완료 되었을 때 기록 저장
                                buy_sell_stock_code[buy_sell_index] = shcode[n];   //코드
                                buy_sell_start_date[buy_sell_index] = BS_start_date[n];
                                buy_sell_start_price[buy_sell_index] = BS_start_price[n];
                                buy_sell_close_date[buy_sell_index] = BS_buy_date[n];
                                buy_sell_close[buy_sell_index] = BS_buy_price[n];
                                buy_sell_last_date[buy_sell_index] = BS_last_date[n];
                                buy_sell_last_price[buy_sell_index] = BS_last_price[n];
                                total_margin = total_margin * (float)buy_sell_last_price[buy_sell_index] / (float)buy_sell_close[buy_sell_index] * 9967 / 10000;
                                buy_sell_margin[buy_sell_index] = total_margin;
                                buy_sell_index++;
                            }
                            */

                            state[n] = 0;

                            if (BS_buy_price[n] != 0)
                                buy_flag = 0;

                            BS_start_date[n] = "0";
                            BS_start_price[n] = 0;
                            BS_buy_date[n] = "0";
                            BS_buy_price[n] = 0;
                            BS_last_date[n] = "0";
                            BS_last_price[n] = 0;
                        }

                    }

                    resultList.Items.Add("SHCODE :" + shcode[n]);
                    resultList.Items.Add("****************************************************************************************");
                    resultList.Items.Add("code" + "\t" + "SP" + "\t" + "BP" + "\t" + "LP" + "\t" + "SD" + "\t" + "BD" + "\t  " + "LD" + "\t  " + "margine");

                    // 시뮬레이션 결과 출력
                    for (int d = 0; d < (buy_sell_index); d++)
                    {
                        string t = "\t";

                        string aaa = String.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}", buy_sell_stock_code[d], t, buy_sell_start_price[d], t, buy_sell_close[d], t, buy_sell_last_price[d], t,
                                                                                                     buy_sell_start_date[d], t, buy_sell_close_date[d], t, buy_sell_last_date[d], t, buy_sell_margin[d]);

                        resultList.Items.Add(aaa);
                    }

                    string bbb = "finish : " + buy_sell_index;
                    resultList.Items.Add(bbb);

                    if (buy_sell_index >= 1)
                        all_margin = all_margin * total_margin;

                    ave_margin = ave_margin + total_margin;
                    /*
                    try
                    {

                        sql = String.Format("CREATE TABLE `stock`.`analysis{0}` (`index` INT NOT NULL, `CODE` VARCHAR(8) NULL, `SP` INT NULL, `BP` VARCHAR(8) NULL, `LP` INT NULL, `SD` VARCHAR(8) NULL, `BD` VARCHAR(8) NULL, `LD`  VARCHAR(8) NULL , `Margin` float NULL, PRIMARY KEY (`index`) );", (80 + j));

                        cmd = new MySqlCommand(sql, conn);
                        cmd.ExecuteNonQuery();
                    }
                    catch (MySqlException a)
                    {
                        // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
                    }

                    // 수신한 코드 정보를 처리
                    for (int idx = 0; idx < buy_sell_index; idx++)
                    {
                        // database에 저장
                        try
                        {
                            sql = String.Format("insert into `stock`.`analysis{0}` values ('{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}')", (80 + j), idx, buy_sell_stock_code[idx], buy_sell_start_price[idx], buy_sell_close[idx], buy_sell_last_price[idx],
                                                                                                                                                       buy_sell_start_date[idx], buy_sell_close_date[idx], buy_sell_last_date[idx], buy_sell_margin[idx]);
                            cmd = new MySqlCommand(sql, conn);
                            cmd.ExecuteNonQuery();
                        }
                        catch (MySqlException d)
                        {
                            // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
                        }
                    }
                    */

                }
                string ccc = "win count : " + win_count + " lose count : " + lose_count + " all margin : " + all_margin + " ave margin : " + ave_margin/(float)stock_num;
                resultList.Items.Add(ccc);

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            UInt64[] price_ = new UInt64[1082];
            UInt64[] marketcap_ = new UInt64[1082];
            UInt64[] amount_ = new UInt64[1082];

            MySqlCommand cmd;
            MySqlDataReader reader;

            count_data = 0;
            string sql = String.Format("select day from stock.date");
            cmd = new MySqlCommand(sql, conn);
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                date[count_data] = Convert.ToString(reader["day"]);
                count_data++;
            }

            reader.Close();
            reader.Dispose();


            count_data = 0;
            sql = String.Format("select * from stock.stock_code");
            cmd = new MySqlCommand(sql, conn);
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                name[count_data] = Convert.ToString(reader["name"]);
                shcode[count_data] = Convert.ToString(reader["code"]);
                count_data++;
            }

            reader.Close();
            reader.Dispose();

            // table 생성
            try
            {

                sql = String.Format("CREATE TABLE `stock`.`sort_code` (`index` INT NOT NULL, `NAME` VARCHAR(40) NULL, `CODE` VARCHAR(8) NULL, `PRICE` INT NULL, `MARKETCAP` INT NULL, `AMOUNT` INT NULL, PRIMARY KEY (`index`) )");

                cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException a)
            {
                // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
            }

            for (int j = 0; j < stock_num; j++)
            {

                sql = String.Format("select * from stock.{0};", shcode[j]);
                cmd = new MySqlCommand(sql, conn);
                reader = cmd.ExecuteReader();

                count_data = 0;
                string a;

                while (reader.Read())
                {
                    a = Convert.ToString(reader["date"]);

                    while (date[count_data].Equals(a) == false)
                    {
                        if (Convert.ToInt32(date[count_data]) > Convert.ToInt32(a))
                        {
                            reader.Read();
                            a = Convert.ToString(reader["date"]);
                        }
                        else
                        {
                            s_date[count_data, j] = date[count_data];
                            close[count_data, j] = 0;
                            volume[count_data, j] = 0;
                            marketcap[count_data, j] = 0;
                            amount[count_data, j] = 0;
                            buy_price[count_data, j] = 0;
                            count_data++;
                        }
                    }

                    s_date[count_data, j] = Convert.ToString(reader["date"]);
                    close[count_data, j] = Convert.ToUInt64(reader["close"]);
                    volume[count_data, j] = Convert.ToUInt64(reader["volume"]);
                    marketcap[count_data, j] = Convert.ToUInt64(reader["marketcap"]);
                    amount[count_data, j] = Convert.ToUInt64(reader["amount"]);
                    count_data++;
                }

                reader.Close();
                reader.Dispose();
            }

            for (int j = 0; j < stock_num; j++)
            {
                try
                {                                                                                                // `index`, `NAME`, `CODE`,  `PRICE`,        `MARKEYCAP`,         `AMOUNT`
                    sql = String.Format("insert into `stock`.`sort_code` values ('{0}','{1}','{2}','{3}','{4}','{5}')", j, name[j], shcode[j], close[4999, j], marketcap[4999, j], amount[4999, j]);
                    cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException d)
                {
                    // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
                }
            }

            // table 생성
            try
            {

                sql = String.Format("CREATE TABLE `stock`.`price_asc_code` (`index` INT NOT NULL, `NAME` VARCHAR(40) NULL, `CODE` VARCHAR(8) NULL, `PRICE` INT NULL, `MARKETCAP` INT NULL, `AMOUNT` INT NULL, PRIMARY KEY (`index`) )");

                cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException a)
            {
                // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
            }

            sql = String.Format("select * from stock.sort_code order by price asc;");
            cmd = new MySqlCommand(sql, conn);
            reader = cmd.ExecuteReader();

            count_data = 0;

            while (reader.Read())
            {
                name[count_data] = Convert.ToString(reader["NAME"]);
                shcode[count_data] = Convert.ToString(reader["CODE"]);
                price_[count_data] = Convert.ToUInt64(reader["PRICE"]);
                marketcap_[count_data] = Convert.ToUInt64(reader["MARKETCAP"]);
                amount_[count_data] = Convert.ToUInt64(reader["AMOUNT"]); 
                count_data++;
            }

            reader.Close();
            reader.Dispose();

            for (int j = 0; j < stock_num; j++)
            {
                try
                {                                                                                                // `index`, `NAME`, `CODE`,  `PRICE`,        `MARKEYCAP`,  `AMOUNT`
                    sql = String.Format("insert into `stock`.`price_asc_code` values ('{0}','{1}','{2}','{3}','{4}','{5}')", j, name[j], shcode[j], price_[j], marketcap_[j], amount_[j]);
                    cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException d)
                {
                    // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
                }
            }

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            try
            {

                sql = String.Format("CREATE TABLE `stock`.`price_desc_code` (`index` INT NOT NULL, `NAME` VARCHAR(40) NULL, `CODE` VARCHAR(8) NULL, `PRICE` INT NULL, `MARKETCAP` INT NULL, `AMOUNT` INT NULL, PRIMARY KEY (`index`) )");

                cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException a)
            {
                // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
            }

            sql = String.Format("select * from stock.sort_code order by price desc;");
            cmd = new MySqlCommand(sql, conn);
            reader = cmd.ExecuteReader();

            count_data = 0;

            while (reader.Read())
            {
                name[count_data] = Convert.ToString(reader["NAME"]);
                shcode[count_data] = Convert.ToString(reader["CODE"]);
                price_[count_data] = Convert.ToUInt64(reader["PRICE"]);
                marketcap_[count_data] = Convert.ToUInt64(reader["MARKETCAP"]);
                amount_[count_data] = Convert.ToUInt64(reader["AMOUNT"]);
                count_data++;
            }

            reader.Close();
            reader.Dispose();

            for (int j = 0; j < stock_num; j++)
            {
                try
                {                                                                                                // `index`, `NAME`, `CODE`,  `PRICE`,        `MARKEYCAP`,  `AMOUNT`
                    sql = String.Format("insert into `stock`.`price_desc_code` values ('{0}','{1}','{2}','{3}','{4}','{5}')", j, name[j], shcode[j], price_[j], marketcap_[j], amount_[j]);
                    cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException d)
                {
                    // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
                }
            }

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            try
            {

                sql = String.Format("CREATE TABLE `stock`.`marketcap_asc_code` (`index` INT NOT NULL, `NAME` VARCHAR(40) NULL, `CODE` VARCHAR(8) NULL, `PRICE` INT NULL, `MARKETCAP` INT NULL, `AMOUNT` INT NULL, PRIMARY KEY (`index`) )");

                cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException a)
            {
                // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
            }

            sql = String.Format("select * from stock.sort_code order by marketcap asc;");
            cmd = new MySqlCommand(sql, conn);
            reader = cmd.ExecuteReader();

            count_data = 0;

            while (reader.Read())
            {
                name[count_data] = Convert.ToString(reader["NAME"]);
                shcode[count_data] = Convert.ToString(reader["CODE"]);
                price_[count_data] = Convert.ToUInt64(reader["PRICE"]);
                marketcap_[count_data] = Convert.ToUInt64(reader["MARKETCAP"]);
                amount_[count_data] = Convert.ToUInt64(reader["AMOUNT"]);
                count_data++;
            }

            reader.Close();
            reader.Dispose();

            for (int j = 0; j < stock_num; j++)
            {
                try
                {                                                                                                // `index`, `NAME`, `CODE`,  `PRICE`,        `MARKEYCAP`,  `AMOUNT`
                    sql = String.Format("insert into `stock`.`marketcap_asc_code` values ('{0}','{1}','{2}','{3}','{4}','{5}')", j, name[j], shcode[j], price_[j], marketcap_[j], amount_[j]);
                    cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException d)
                {
                    // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
                }
            }

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            try
            {

                sql = String.Format("CREATE TABLE `stock`.`marketcap_desc_code` (`index` INT NOT NULL, `NAME` VARCHAR(40) NULL, `CODE` VARCHAR(8) NULL, `PRICE` INT NULL, `MARKETCAP` INT NULL, `AMOUNT` INT NULL, PRIMARY KEY (`index`) )");

                cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException a)
            {
                // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
            }

            sql = String.Format("select * from stock.sort_code order by marketcap desc;");
            cmd = new MySqlCommand(sql, conn);
            reader = cmd.ExecuteReader();

            count_data = 0;

            while (reader.Read())
            {
                name[count_data] = Convert.ToString(reader["NAME"]);
                shcode[count_data] = Convert.ToString(reader["CODE"]);
                price_[count_data] = Convert.ToUInt64(reader["PRICE"]);
                marketcap_[count_data] = Convert.ToUInt64(reader["MARKETCAP"]);
                amount_[count_data] = Convert.ToUInt64(reader["AMOUNT"]);
                count_data++;
            }

            reader.Close();
            reader.Dispose();

            for (int j = 0; j < stock_num; j++)
            {
                try
                {                                                                                                // `index`, `NAME`, `CODE`,  `PRICE`,        `MARKEYCAP`,  `AMOUNT`
                    sql = String.Format("insert into `stock`.`marketcap_desc_code` values ('{0}','{1}','{2}','{3}','{4}','{5}')", j, name[j], shcode[j], price_[j], marketcap_[j], amount_[j]);
                    cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException d)
                {
                    // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
                }
            }

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
             //select * from sort_code order by price

        }

        private void resultList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            resultList.Items.Add("update_raw_data");

            string sql = String.Format("CREATE TABLE `stock`.`new_date` (`num` INT NOT NULL,`day` VARCHAR(8) NULL,PRIMARY KEY (`num`) )");
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();

            update_flag = 1;

            request_t1305("000020");
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }



#if false
        // 1개 주가를 이용 시뮬레이션
        private void simulator_Click(object sender, EventArgs e)
        {
            string date = "";
            string pre_date = "";
            UInt64 close = 0, volume = 0, amount = 0, buy_price = 0;
            UInt64 pre_close = 0, pre_volume = 0, pre_amount = 0, pre_buy_price = 0;

            UInt64 buy_close = 0;

            UInt16 buy_flag = 0;
            UInt16 count = 0;

            UInt64 delay_count = 0;

            UInt64 lowest_value = 0;
            
            // simulation variables
            UInt64 buy_margin = Convert.ToUInt64(textBox2.Text);
            UInt64 sell_margin = Convert.ToUInt64(textBox3.Text);
            UInt64 low_count = Convert.ToUInt64(textBox4.Text);
            UInt64 low_percent = 100;

            float total_margin = 1;

            resultList.Items.Clear();

            string sql = "select * from stock.000020";

            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {

                date = Convert.ToString(reader["date"]);
                close = Convert.ToUInt64(reader["close"]);
                volume = Convert.ToUInt64(reader["volume"]);
                amount = Convert.ToUInt64(reader["amount"]);
                buy_price = Convert.ToUInt64(reader["buy_price"]);

                //resultList.Items.Add(Convert.ToString(date) + "\t" + Convert.ToString(close) + "\t" + Convert.ToString(buy_price) );

                if( (pre_close >= close*1.2) || (pre_close*1.2 <= close))
                    delay_count = 0;
                else
                    delay_count = delay_count + volume;

                if (delay_count > amount*3)
                {
                    
                    if(buy_flag == 0)
                    {
                        if (close < buy_price)
                        {
                            count++;

                            //resultList.Items.Add("count:" + count);

                            if (count == 1)
                                lowest_value = close;
                            else
                            {
                                if (lowest_value > close)
                                    lowest_value = close;
                            }
                        }
                        else
                        {
                            if ( (count > low_count) /*&& (lowest_value <= count * low_percent /100 ) */)
                                buy_flag = 1;

                            count = 0;
                        }
                    }
                    
                    if(buy_flag == 1)
                    {
                        resultList.Items.Add("buy"+ date +"\t" + Convert.ToString(close) );
                        buy_close = close;
                        buy_flag = 2;
                    }

                    if(buy_flag == 2)
                    {
                        if(close >= buy_close * buy_margin / 100)
                        {
                            total_margin = total_margin * close / buy_close;
                            resultList.Items.Add("sell" + date + "\t" + Convert.ToString(close) + "\t" + total_margin);

                            buy_close = 0;
                            buy_flag = 0;
                        }
                        else if(close <= buy_close * sell_margin / 100)
                        {
                            total_margin = total_margin * close / buy_close;
                            resultList.Items.Add("sell" + date + "\t" + Convert.ToString(close) + "\t" + total_margin);

                            buy_close = 0;
                            buy_flag = 0;
                        }
                    }                   

                }


                pre_date = date;
                pre_close = close;
                pre_volume = volume;
                pre_amount = amount;
                pre_buy_price = buy_price;

            }

            reader.Close();
        }
#endif


#if false
        /* click request 기간별 데이터 */
        private void request_data_Click(object sender, EventArgs e)
        {
            SC = textBox1.Text;
            try
            {
                string i = SC;

                string sql = String.Format("CREATE TABLE `stock`.`{0}` (`date` VARCHAR(64) NOT NULL,`close` VARCHAR(64) NULL,`volume` VARCHAR(64) NULL,`marketcap` VARCHAR(64) NULL,`amount` VARCHAR(64) NULL,`buy_price` VARCHAR(64) NULL,PRIMARY KEY (`date`) )", i);

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException a)
            {
                // in case of duplicate primay key exception e.Number will be 1062. Just ignore any exception...
            }

            xingQuery.ReceiveData += xingQuery_ReceiveData;

            xingQuery.LoadFromResFile("c:/etrade/xingapi/res/t1305.res");
            xingQuery.SetFieldData("t1305InBlock", "shcode", 0, SC);
            xingQuery.SetFieldData("t1305InBlock", "dwmcode", 0, "1");
            xingQuery.SetFieldData("t1305InBlock", "cnt", 0, "10");
            xingQuery.Request(false);
        }
#endif
    }
}
