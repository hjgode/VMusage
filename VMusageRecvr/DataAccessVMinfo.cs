using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading;

using System.Windows.Forms;

using System.Data;
using System.Data.SQLite;

namespace DataAccessVM
{
    class DataAccess:IDisposable
    {
        class _fieldsDefine {
            public string FieldName;
            public string FieldType;
            public _fieldsDefine(string s, string t)
            {
                FieldName = s;
                FieldType = t;
            }
        }
        _fieldsDefine[] _fieldsProcess = new _fieldsDefine[]{
            new _fieldsDefine("RemoteIP", "System.String"),
            new _fieldsDefine("name", "System.String"),
            new _fieldsDefine("memusage", "System.Int64"),
            new _fieldsDefine("slot", "System.Byte"),
            new _fieldsDefine("procID", "System.Int64"),
            new _fieldsDefine("Time", "System.DateTime"),
            new _fieldsDefine("idx", "System.UInt64"),
        };


        static string sDataFile = "VMUsage.sqlite";
        public static string sDataFileFull{
            get{            
                string sAppPath = System.Environment.CurrentDirectory;
                if(!sAppPath.EndsWith(@"\"))
                    sAppPath+=@"\";
                return sAppPath+sDataFile;
            }
        }

        private SQLiteConnection sql_con;
        private SQLiteCommand sql_cmd;
        private SQLiteDataAdapter sql_dap;

        BindingSource bsVMUsage;
        DataSet dsVMUsage;
        DataTable dtVMUsage;

        DataGridView _dataGrid;

        Queue<VMusage.procVMinfo> dataQueue;
        Thread myDataThread;
        bool bRunDataThread = true;

        public uint lastTotaMemUse = 0;
        public DateTime lastMemMeasure;

        public EventWaitHandle waitHandle;

        public DataAccess()
        {

        }

        public DataAccess(DataGridView dg, ref Queue<VMusage.procVMinfo> dQueue)
        {
            _dataGrid = dg;
            dataQueue = dQueue;

            sql_cmd = new SQLiteCommand();

            createTables();

            connectDB();
            createTablesSQL();

            waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

            if (System.IO.File.Exists(sDataFileFull))
                ;//open db
            else
                ;//create new db
            myDataThread = new Thread(dataAddThread);
            myDataThread.Start();
        }
        public void Dispose()
        {
            bRunDataThread = false;
            waitHandle.Set();
            myDataThread.Abort();
        }

        void dataAddThread()
        {
            try
            {
                while (bRunDataThread)
                {
                    waitHandle.WaitOne();
                    if (dataQueue.Count > 10)
                    {
                        while (dataQueue.Count > 0)
                        {
                            VMusage.procVMinfo vmInfo = dataQueue.Dequeue();
                            lastTotaMemUse += vmInfo.memusage;
                            lastMemMeasure = new DateTime(vmInfo.Time);
                            addSqlData(vmInfo);
                        }
                    }
                    Thread.Sleep(10);
                }
            }
            catch (ThreadAbortException ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + ex.Message);
            }
        }

        private void connectDB()
        {
            sql_con = new SQLiteConnection("Data Source=" + sDataFileFull + ";Version=3;New=False;Compress=True;Synchronous=Off");
            sql_con.Open();

            try
            {
                DataTable dtTest = sql_con.GetSchema("Tables", new string[] { null, null, "VMUsage", null });
            }
            catch (SQLiteException ex)
            {
                System.Diagnostics.Debug.WriteLine("SQLiteException in connectDB(): " + ex.Message);
            }
            catch (DataException ex)
            {
                System.Diagnostics.Debug.WriteLine("DataException in connectDB(): " + ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception in connectDB(): " + ex.Message);
            }
            sql_cmd.Connection = sql_con;
        }

        public bool addData(VMusage.procVMinfo vmInfo)
        {
            bool bRet = false;
            try
            {
                //string txtSQLQuery = "insert into  processes (desc) values ('" + txtDesc.Text + "')";
                //ExecuteQuery(txtSQLQuery);
                object[] o = new object[]{
                    vmInfo.remoteIP,
                    vmInfo.name,
                    vmInfo.memusage,
                    vmInfo.slot,
                    vmInfo.procID,
                    new DateTime(vmInfo.Time),
                    0,
                };

                DataRow dr;
                //check if data already exists
                dr = dsVMUsage.Tables[0].Rows.Find(vmInfo.name);
                if (dr == null)
                {   //add a new row
                    dr = dtVMUsage.NewRow();
                    dr.ItemArray = o;
                    dtVMUsage.Rows.Add(dr);
                }
                else
                    dr.ItemArray = o;

                dr.AcceptChanges();
                dtVMUsage.AcceptChanges();
                dsVMUsage.AcceptChanges();
                bRet = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("addData Exception: " + ex.Message);
            }
            return bRet;
        }
        
        public int ExportMemUsage2CSV(string sFileCSV)
        {
            //pause data read thread (socksrv)?
            sql_cmd = new SQLiteCommand();
            sql_con = new SQLiteConnection();
            connectDB();
            if (sql_con.State != ConnectionState.Open)
            {
                sql_con.Close();
                sql_con.Open();
            }
            sql_cmd = sql_con.CreateCommand();
            int iCnt = 0;
            sql_cmd.CommandText="select * from vmUsage";
            SQLiteDataReader rdr = null;

            try
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter(sFileCSV);
                rdr = sql_cmd.ExecuteReader(CommandBehavior.CloseConnection);
                sw.Write(
                        "RemoteIP" + ";" +
                        "Name" + ";" +
                        "Memusage" + ";" +
                        "Slot" + ";" +
                        "ProcID" + ";" +
                        "Time" + ";" +
                        "Idx" +
                        "\r\n"
                        );

                while (rdr.Read())
                {
                    iCnt++;
                    //Console.WriteLine(rdr["ProcID"] + " " + rdr["User"]);
                    sw.Write(
                        "\"" + rdr["RemoteIP"] + "\";" +
                        "\"" + rdr["Name"] + "\";" +
                        rdr["Memusage"] + ";" +
                        rdr["Slot"] + ";" +
                        rdr["ProcID"] + ";" +
                        DateTime.FromBinary((long)rdr["Time"]).ToString("hh:mm:ss.fff") + ";" +
                        rdr["Idx"] +
                        "\r\n"
                        );
                }
            }
            catch (SQLiteException ex)
            {
                System.Diagnostics.Debug.WriteLine("ExportMemUsage2CSV: " + sql_cmd.CommandText + " " + ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ExportMemUsage2CSV: " + sql_cmd.CommandText + " " + ex.Message);
            }
            finally
            {
                rdr.Close();
            }
            
            sql_con.Close();
            return iCnt;
        }

        public int ExportMemUsage2CSV2(string sFileCSV, string strIP)
        {
            //pause data read thread (socksrv)?
            sql_cmd = new SQLiteCommand();
            sql_con = new SQLiteConnection();
            connectDB();
            if (sql_con.State != ConnectionState.Open)
            {
                sql_con.Close();
                sql_con.Open();
            }
            sql_cmd = sql_con.CreateCommand();
            int iCnt = 0;
            sql_cmd.CommandText = "select * from vmUsage";
            SQLiteDataReader rdr = null;

            //although exporting data in normal format is not to bad
            //better export by using a different layout
            // time \ nameX     nameY
            // 00:00  memuseX   memuseY
            // 00:01  memuseX   memuseY
            // ...
            // so we need a list of unique names excluding 'Slot x: empty' values
            List<string> lNames=new List<string>();
            sql_cmd.CommandText = "select distinct [Name] from [VMUsage] where [Name] not like 'Slot %' ORDER BY [Name];";
            try
            {
                rdr = sql_cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (rdr.Read())
                    lNames.Add(rdr["Name"].ToString());
            }
            catch (Exception)
            {

            }
            finally
            {
                rdr.Close();
            }
            if (lNames.Count == 0)
            {
                goto exit_cvs2;
            }


            //create a table with the names as fields plus a field for the time
            executeNonQuery("DROP TABLE IF EXISTS [VMusageTEMP];");
            // Define the SQL Create table statement, IF NOT EXISTS 
            string createAppUserTableSQL = "CREATE TABLE IF NOT EXISTS [VMusageTEMP] (" +
                "[Time] INTEGER NOT NULL, ";
            //add the fields with names
            foreach (string sFieldName in lNames)
                createAppUserTableSQL += "[" + sFieldName + "] INTEGER DEFAULT 0, ";
            //add RemoteIP
            createAppUserTableSQL += "[RemoteIP] TEXT DEFAULT '', ";
            //add idx field
            createAppUserTableSQL += "[idx] INTEGER PRIMARY KEY AUTOINCREMENT )";
            //create Table
            executeNonQuery(createAppUserTableSQL);
            //add index
            string SqlIndex = "CREATE INDEX [Time] on VMUsage (Name ASC);";
            executeNonQuery(SqlIndex);

            //### get all distinct times
            List<ulong> lTimes = new List<ulong>();
            sql_cmd.CommandText = "Select DISTINCT Time from [VMusage] order by Time;";
            rdr = sql_cmd.ExecuteReader();
            while (rdr.Read())
            {
                lTimes.Add(Convert.ToUInt64(rdr["Time"]));
                Application.DoEvents();
            }
            rdr.Close();

            //### get all process, memusage data
            List<MEMORY_USAGE_IP> lMemoryUsages = new List<MEMORY_USAGE_IP>();
            if (strIP != "")
                sql_cmd.CommandText = "Select RemoteIP, Name, MemUsage, Time from VMusage WHERE [RemoteIP]='" + strIP + "' order by Time";
            else
                sql_cmd.CommandText = "Select RemoteIP, Name, MemUsage, Time from VMusage order by Time";

            rdr = sql_cmd.ExecuteReader();
            while (rdr.Read())
            {
                string sIP = (string)rdr["RemoteIP"];
                string sName = (string)rdr["Name"];
                int iMemUsage = Convert.ToInt32(rdr["MemUsage"]);
                ulong uTI = Convert.ToUInt64(rdr["Time"]);
                lMemoryUsages.Add(new MEMORY_USAGE_IP(sIP, sName, iMemUsage, uTI));
                Application.DoEvents();
            }
            rdr.Close();

            //now iterate thru all times and get the names and memuse values
            string sUpdateCommand = "";
            SQLiteTransaction tr = sql_con.BeginTransaction();
            //sql_cmd.CommandText = "insert into [ProcUsage]  (Time, [device.exe]) SELECT Time, User from [Processes] WHERE Time=631771077815940000 AND Process='device.exe';";
            int lCnt = 0;
            foreach (ulong uTime in lTimes)
            {
                System.Diagnostics.Debug.WriteLine("Updating for Time=" + uTime.ToString());
                //insert an empty row
                sql_cmd.CommandText = "Insert Into VMUsageTemp (RemoteIP, Time) VALUES('0.0.0.0', " + uTime.ToString() + ");";
                lCnt = sql_cmd.ExecuteNonQuery();
                foreach (string sPName in lNames)
                {
                    Application.DoEvents();

                    //is there already a line?
                    //lCnt = executeNonQuery("Select Time " + "From ProcUsage Where Time="+uTime.ToString());

                    // http://stackoverflow.com/questions/4495698/c-sharp-using-listt-find-with-custom-objects
                    MEMORY_USAGE_IP pm = lMemoryUsages.Find(x => x.procname == sPName && x.timestamp == uTime);
                    if (pm != null)
                    {
                        System.Diagnostics.Debug.WriteLine("\tUpdating Memory=" + pm.memusage + " for Process=" + sPName);
                        //update values
                        sUpdateCommand = "Update [VMUsageTemp] SET " +
                            "[" + sPName + "]=" + pm.memusage +
                            ", [RemoteIP]='" + pm.sRemoteIP + "'" +
                            //"(SELECT User from [Processes]
                            " WHERE Time=" + uTime.ToString() + //" AND Process=" + "'" + sPro + "'"+
                            ";";
                        sql_cmd.CommandText = sUpdateCommand;
                        //System.Diagnostics.Debug.WriteLine(sUpdateCommand);
                        try
                        {
                            lCnt = sql_cmd.ExecuteNonQuery();
                        }
                        catch (SQLiteException ex)
                        {
                            System.Diagnostics.Debug.WriteLine("export2CSV2()-SQLiteException: " + ex.Message + " for " + sUpdateCommand);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("export2CSV2()-Exception: " + ex.Message + " for " + sUpdateCommand);
                        }
                        //lCnt = executeNonQuery(sInsertCommand);
                        //"insert into [ProcUsage]  (Time, [device.exe]) SELECT Time, User from [Processes] WHERE Time=631771077815940000 AND Process='device.exe';"
                    }
                }
            }
            tr.Commit();

            //export the table to CSV
            lCnt = 0;
            rdr = null;
            System.IO.StreamWriter sw = null;
            try
            {
                sw = new System.IO.StreamWriter(sFileCSV);
                //sw.WriteLine("RemoteIP;" + strIP);
                string sFields = "";
                List<string> lFields = new List<string>();
                lFields.Add("RemoteIP");
                lFields.Add("Time");
                lFields.AddRange(lNames);
                foreach (string ft in lFields)
                {
                    sFields += ("'" + ft + "'" + ";");
                }
                sFields.TrimEnd(new char[] { ';' });
                sw.Write(sFields + "\r\n");

                sql_cmd.CommandText = "Select * from VMUsageTemp;";
                rdr = sql_cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (rdr.Read())
                {
                    Application.DoEvents();
                    lCnt++;
                    sFields = "";
                    //Console.WriteLine(rdr["ProcID"] + " " + rdr["User"]);
                    foreach (string ft in lFields)
                    {
                        sFields += rdr[ft] + ";";
                    }
                    sFields.TrimEnd(new char[] { ';' });
                    sw.Write(sFields + "\r\n");
                    sw.Flush();
                }
            }
            catch (Exception) { }
            finally
            {
                sw.Close();
                rdr.Close();
            }
exit_cvs2:
            sql_con.Close();
            return iCnt;
        }

        public int ExportThreads2CSV(string sFileCSV)
        {
            //pause data read thread (socksrv)?
            sql_cmd = new SQLiteCommand();
            sql_con = new SQLiteConnection();
            connectDB();
            if (sql_con.State != ConnectionState.Open)
            {
                sql_con.Close();
                sql_con.Open();
            }
            sql_cmd = sql_con.CreateCommand();
            int iCnt = 0;
            sql_cmd.CommandText = "select * from threads";
            SQLiteDataReader rdr = null;
            try
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter(sFileCSV);
                rdr = sql_cmd.ExecuteReader(CommandBehavior.CloseConnection);
                string sFields = "";
                //foreach (_fieldsDefine ft in _fieldsThread)
                //{
                //    sFields+=(ft.FieldName+ ";");
                //}
                sFields.TrimEnd(new char[] { ';' });                
                sw.Write(sFields +"\r\n");

                while (rdr.Read())
                {
                    iCnt++;
                    sFields = "";
                    //Console.WriteLine(rdr["ProcID"] + " " + rdr["User"]);
                    //foreach (_fieldsDefine fd in _fieldsThread)
                    //{
                    //    if (fd.FieldType == "System.String")
                    //        sFields += "\"" + rdr[fd.FieldName] + "\";";
                    //    else if (fd.FieldType == "System.DateTime")
                    //        sFields += DateTime.FromBinary((long)rdr[fd.FieldName]).ToString("hh:mm:ss.fff") + ";";
                    //    else
                    //        sFields += rdr[fd.FieldName] + ";";
                    //}
                    sFields.TrimEnd(new char[] { ';' });
                    sw.Write(sFields);
                    sw.Write(sFields + "\r\n");
                }
            }
            finally
            {
                rdr.Close();
            }

            sql_con.Close();
            return iCnt;
        }

        public DataRow[] executeQuery(string sSQL)
        {
            //setup
            List<DataRow> dataRows = new List<DataRow>();
            sql_cmd = new SQLiteCommand();
            sql_con = new SQLiteConnection();
            SQLiteDataReader sql_rdr= null;
            connectDB();
            if (sql_con.State != ConnectionState.Open)
            {
                sql_con.Close();
                sql_con.Open();
            }
            sql_cmd = sql_con.CreateCommand();
            sql_cmd.CommandText = sSQL;
            sql_rdr = sql_cmd.ExecuteReader(CommandBehavior.CloseConnection);
            List<string> sColList = new List<string>();
            DataColumn dc;
            while (sql_rdr.Read())
            {
                for (int i = 0; i < sql_rdr.FieldCount; i++)
                {
                    sColList.Add(sql_rdr.GetName(i));
                    dc = new DataColumn(sql_rdr.GetName(i));
                    //dc = sql_rdr.GetValue(i);
                }

            }
            return dataRows.ToArray();
        }

        #region Transform
        class MEMORY_USAGE_IP
        {
            public string sRemoteIP;
            public string procname;
            public int memusage;
            public UInt64 timestamp;
            public MEMORY_USAGE_IP(string sIP, string sProcname, int iMemusage, UInt64 iTimeStamp)
            {
                sRemoteIP = sIP;
                procname = sProcname;
                memusage = iMemusage;
                timestamp = iTimeStamp;
            }
        }
        class PROCESS_USAGE
        {
            public string procname;
            public int user;
            public UInt64 timestamp;
            public PROCESS_USAGE(string sProcessName, int iUserTime, UInt64 iTimeStamp)
            {
                procname = sProcessName;
                user = iUserTime;
                timestamp = iTimeStamp;
            }
        }
        class PROCESS_USAGE_IP
        {
            public string sRemoteIP;
            public string procname;
            public int user;
            public UInt64 timestamp;
            public PROCESS_USAGE_IP(string sIP, string sProcessName, int iUserTime, UInt64 iTimeStamp)
            {
                sRemoteIP = sIP;
                procname = sProcessName;
                user = iUserTime;
                timestamp = iTimeStamp;
            }
        }
        public string[] getKnownIPs()
        {
            //### setup
            sql_cmd = new SQLiteCommand();
            sql_con = new SQLiteConnection();
            SQLiteDataReader sql_rdr; 
            List<string> ipList = new List<string>();
            connectDB();
            if (sql_con.State != ConnectionState.Open)
            {
                sql_con.Close();
                sql_con.Open();
            }
            sql_cmd = sql_con.CreateCommand();
            //### Build a List of known IP adress's
            sql_cmd.CommandText = "Select DISTINCT RemoteIP from processes order by RemoteIP";
            List<string> lProcesses = new List<string>();
            sql_rdr = sql_cmd.ExecuteReader();
            while (sql_rdr.Read())
            {
                ipList.Add((string)sql_rdr["RemoteIP"]);
            }
            sql_rdr.Close();
            sql_rdr.Dispose();

            return ipList.ToArray();
        }

        #endregion

        private void dropTables()
        {
#if DEBUG1
            System.Diagnostics.Debug.WriteLine("DEBUG: dropTables()");
            string dropTable = "DROP TABLE [Processes]";
            executeNonQuery(dropTable);
            dropTable = "DROP TABLE [Threads]";
            executeNonQuery(dropTable);
#else
            System.Diagnostics.Debug.WriteLine("No DEBUG: no dropTables()");
#endif
        }
        private void createTablesSQL()
        {
            dropTables();
            // Define the SQL Create table statement, IF NOT EXISTS 
            string createAppUserTableSQL = "CREATE TABLE IF NOT EXISTS [VMusage] (" +
                "[RemoteIP] TEXT NOT NULL, " +
                "[Name] TEXT NOT NULL, " +
                "[MemUsage] INTEGER NOT NULL, " +
                "[Slot] INTEGER NOT NULL, " +
                "[ProcID] INTEGER NOT NULL, " +
                "[Time] INTEGER NOT NULL, " +
                "[idx] INTEGER PRIMARY KEY AUTOINCREMENT " +
                ")";
            executeNonQuery(createAppUserTableSQL);

            string SqlIndex = "CREATE INDEX [Name] on VMUsage (Name ASC);";
            executeNonQuery(SqlIndex);

            ////a view
            //string createView = "CREATE VIEW IF NOT EXISTS ProcessView AS " +
            //    "SELECT " +
            //    "Processes.RemoteIP, " +
            //    "Processes.ProcID, Processes.Process, Processes.[User] * 1.0 / Processes.[Time] * 1.0 * 100.0 AS Usage, Processes.Duration, strftime('%H:%M:%f', " +
            //    "datetime(Processes.[Time] / 10000000 - 62135596800, 'unixepoch')) AS theTime " +
            //    "FROM " +
            //    "Processes INNER JOIN " +
            //    "Threads ON Processes.idx = Threads.idx " +
            //    "ORDER BY theTime";
            //executeNonQuery(createView);

        }

        static StringBuilder FieldsProcessTable = new StringBuilder();
        //see also http://www.techcoil.com/blog/my-experience-with-system-data-sqlite-in-c/
        public void addSqlData(VMusage.procVMinfo procVMStats)
        {
            //System.Diagnostics.Debug.WriteLine(procStats.dumpStatistics());

            long rowID = 0; //last inserted row
            #region Process_data
            //build a list of field names of process table
            if (FieldsProcessTable.Length == 0)
            {
                //StringBuilder 
                //FieldsProcessTable = new StringBuilder();
                for (int ix = 0; ix < _fieldsProcess.Length; ix++)
                {
                    FieldsProcessTable.Append(_fieldsProcess[ix].FieldName);
                    if (ix < _fieldsProcess.Length - 1)
                        FieldsProcessTable.Append(", ");
                }
            }

            StringBuilder FieldsProcessValues = new StringBuilder();
            FieldsProcessValues.Append("'" + procVMStats.remoteIP + "', ");
            FieldsProcessValues.Append("'" + procVMStats.name.ToString()+"', ");
            FieldsProcessValues.Append("'" + procVMStats.memusage.ToString() + "', ");
            FieldsProcessValues.Append("'" + procVMStats.slot.ToString() + "', ");
            FieldsProcessValues.Append(procVMStats.procID.ToString() + ", ");
            FieldsProcessValues.Append(procVMStats.Time.ToString() + ", ");
            FieldsProcessValues.Append("NULL");    //add an idx although it is autoincrement

            string sqlStatement = "INSERT INTO VMUsage " +
                "(" + 
                FieldsProcessTable +
                ")" +
                " VALUES(" + 
                FieldsProcessValues.ToString() +
                ")";

            rowID = executeNonQuery(sqlStatement);
            #endregion

        }

        long executeNonQuery(string sSQL)
        {
            long rowId = 0;
            try
            {
                if (sql_con.State != ConnectionState.Open)
                {
                    sql_con.Close();
                    sql_con.Open();
                }
                using (SQLiteTransaction sqlTransaction = sql_con.BeginTransaction())
                {
                    sql_cmd.CommandText = sSQL;
                    sql_cmd.ExecuteNonQuery();
                    // Commit the changes into the database
                    sqlTransaction.Commit();

                    sql_cmd.CommandText = "SELECT last_insert_rowid()";
                    rowId = (long)sql_cmd.ExecuteScalar();
                    
                } // end using
            }
            catch (SQLiteException ex)
            {
                System.Diagnostics.Debug.WriteLine("executeNonQuery SQLiteException: " + ex.Message + "\r\n'" + sSQL +"'");
                rowId = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("executeNonQuery Exception: " + ex.Message + "\r\n'" + sSQL + "'");
                rowId = 0;
            }
            return rowId;
        }

        private void createTables()
        {
            dtVMUsage = new DataTable("VMUsage");
            dsVMUsage = new DataSet();
            bsVMUsage = new BindingSource();
            #region proc_table
            int iProcIDcolumn = -1;
            DataColumn[] dc = new DataColumn[_fieldsProcess.Length];
            //build the process table columns
            for (int i = 0; i < _fieldsProcess.Length ; i++)
            {
                dc[i] = new DataColumn();
                dc[i].Caption = _fieldsProcess[i].FieldName;     // "App";
                dc[i].ColumnName = _fieldsProcess[i].FieldName;  // "App";
                dc[i].DataType = System.Type.GetType(_fieldsProcess[i].FieldType);

                //if (dc[i].DataType = System.Type.GetType("System.DateTime"))
                //    dc[i].DateTimeMode = DataSetDateTime.Local;

                if (dc[i].DataType == System.Type.GetType("System.String"))
                    dc[i].MaxLength = 256;

                if (dc[i].Caption.Equals("Name", StringComparison.CurrentCultureIgnoreCase))
                {
                    dc[i].Unique = true;
                    iProcIDcolumn = i;
                }
                else
                    dc[i].Unique = false;

                dc[i].AllowDBNull = false;

            }
            //add header
            dtVMUsage.Columns.AddRange(dc);

            DataColumn[] dcKey = new DataColumn[1];
            dcKey[0] = dc[iProcIDcolumn];
            dtVMUsage.PrimaryKey = dcKey;

            dsVMUsage.Tables.Add(dtVMUsage);
            #endregion

            bsVMUsage.DataSource = dsVMUsage;
            bsVMUsage.DataMember = dsVMUsage.Tables[0].TableName;

            dtVMUsage.AcceptChanges();
            dsVMUsage.AcceptChanges();

            this._dataGrid.DataSource = bsVMUsage;
            this._dataGrid.ReadOnly = true;
            this._dataGrid.AllowUserToAddRows = false;
            this._dataGrid.AllowUserToDeleteRows = false;
            
            //show the timestamp with seconds
            this._dataGrid.Columns["Time"].DefaultCellStyle.Format = "HH:mm:ss";

            this._dataGrid.Refresh();
        }
        
        private void ExecuteQuery(string txtQuery)
        {
            connectDB();
            sql_con.Open();
            sql_cmd = sql_con.CreateCommand();
            sql_cmd.CommandText = txtQuery;
            sql_cmd.ExecuteNonQuery();
            sql_con.Close();
        }
        private void LoadData()
        {
            connectDB();
            sql_con.Open();
            sql_cmd = sql_con.CreateCommand();
            string CommandText = "select * from VMUsage";
            sql_dap = new SQLiteDataAdapter(CommandText, sql_con);
            dsVMUsage.Reset();
            sql_dap.Fill(dsVMUsage);
            dtVMUsage = dsVMUsage.Tables[0];
            this._dataGrid.DataSource = dtVMUsage;
            sql_con.Close();
        }
    }
}
