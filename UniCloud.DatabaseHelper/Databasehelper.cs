using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading;

namespace UniCloud.DatabaseHelper
{
    public class Databasehelper
    {
        private const string KillSPID = "p_Killspid";

        private SqlConnection _Conn;
        public Databasehelper()
        {
            _Conn = new SqlConnection(this.ConnectionString); 
        }

        private string ConnectionString
        {
            get { return ConfigurationSettings.AppSettings["ConnenctionString"]; }
        }

        /// <summary>
        /// 打开数据库
        /// </summary>
        /// <param name="DatabaseName">数据库名称</param>
        /// <returns></returns>
        private string GetUseDataBaseStr(string DatabaseName)
        {
            return "use " + DatabaseName;
        }

        /// <summary>
        /// 删除存储过程
        /// </summary>
        /// <param name="ProcName">存储过程名称</param>
        /// <returns></returns>
        private string GetDropProcStr(string ProcName) //可删除其他存储过程
        {
            return "if exists (select * from dbo.sysobjects where " +
                    " id = object_id(N'[dbo].[" + ProcName +"]')  and " +
                    " OBJECTPROPERTY(id,   N'IsProcedure')   =  1) " +
                    " drop procedure [dbo].[" +ProcName +"]";
        }

        /// <summary>
        /// 获取创建断开数据库连接进程的存储过程字符串
        /// </summary>
        /// <returns></returns>
        private string GetCreateKillSPIDProcStr()
        {
            return "create   proc   [dbo].[" + KillSPID +"] " + System.Environment.NewLine
                    +" @dbname   sysname --要关闭进程的数据库名 " + System.Environment.NewLine
                    +"as       " + System.Environment.NewLine
                    +"declare   @s   nvarchar(1000)" + System.Environment.NewLine
                    +"declare   tb   cursor   local   for" + System.Environment.NewLine
            +" select   s='kill   '+cast(spid   as   varchar) " + System.Environment.NewLine
            +" from   master..sysprocesses   " + System.Environment.NewLine
            +" where   dbid=db_id(@dbname) " + System.Environment.NewLine
            +" open   tb" + System.Environment.NewLine
            +" fetch   next   from   tb   into   @s  " + System.Environment.NewLine
            +" while   @@fetch_status=0  " + System.Environment.NewLine
            +" begin " + System.Environment.NewLine
            +"  exec(@s)" + System.Environment.NewLine
            +"  fetch next from tb into @s  " + System.Environment.NewLine
            +"end" + System.Environment.NewLine
            +"close tb " + System.Environment.NewLine
            +"deallocate tb  " + System.Environment.NewLine;
        }

        /// <summary>
        /// 获取执行断开数据库连接进程的存储过程字符串
        /// </summary>
        /// <param name="DatabaseName">数据库名称</param>
        /// <returns></returns>
        private string GetExecuteKillSPIDProcStr(string DatabaseName)
        {
            return "exec " + KillSPID +" '" + DatabaseName + "'";
        }

        /// <summary>
        /// 获取创建断开数据库连接进程的存储过程相关的SQL语句列表
        /// </summary>
        /// <param name="DatabaseName"></param>
        /// <returns></returns>
        public List<string> GetCreateKillSPIDSqlList(string DatabaseName)
        {
            List<string> SqlList = new List<string>();
            SqlList.Add(this.GetUseDataBaseStr("master"));
            SqlList.Add(this.GetDropProcStr(KillSPID));
            SqlList.Add(this.GetCreateKillSPIDProcStr());
            return SqlList;
        }

        /// <summary>
        /// 获取备份数据库字符串
        /// </summary>
        /// <param name="DatabaseName"></param>
        /// <param name="FilePath"></param>
        /// <param name="FileName"></param>
        /// <returns></returns>
        private string GetBackupDatabaseStr(string DatabaseName, string FilePath, string FileName)
        {
            return String.Format("backup database {0} to disk = '{1}\\{2}'", DatabaseName, FilePath, FileName);
        }

        /// <summary>
        /// 获取恢复数据库字符串
        /// </summary>
        /// <param name="DatabaseName"></param>
        /// <param name="FilePath"></param>
        /// <param name="FileName"></param>
        /// <returns></returns>
        private string GetRestoreDatabaseStr(string DatabaseName, string FilePath, string FileName)
        {
            return String.Format("restore database {0} from disk = '{1}\\{2}' with replace", DatabaseName, FilePath, FileName);
        }

        /// <summary>
        /// 用事务批量执行查询语句
        /// </summary>
        /// <param name="SqlList"></param>
        /// <returns></returns>
        private bool executeTransaction(List<string> SqlList)
        {
            bool flag = false;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = _Conn;//命令对象  
            SqlTransaction myTransaction;　//声明一个SQL事物类型  
            _Conn.Open();//打开连接
            myTransaction = _Conn.BeginTransaction();//基于一个连接初始化事物  
            try
            {
                for (int i = 0; i < SqlList.Count(); i++)
                {
                    cmd.Transaction = myTransaction;//指定SQL命令语句　的事物  
                    cmd.CommandText = SqlList[i];//给定命令语句  
                    cmd.ExecuteNonQuery();//执行SQL语句  
                }
                myTransaction.Commit(); //提交事务
                flag = true;
            }
            catch (Exception e)
            {
                myTransaction.Rollback();
                flag = false;
            }
            finally
            {
                _Conn.Close();
            }
            return flag;
        }  
  
        /// <summary>
        /// 创建断开数据库连接进程的存储过程
        /// </summary>
        /// <param name="DatabaseName">数据库</param>
        /// <returns></returns>
        private bool CreateKillSpIdProc(string DatabaseName)
        {
            List<string> SqlList = GetCreateKillSPIDSqlList(DatabaseName);
            try
            {
               this.executeTransaction(SqlList);
            }
            catch
            {
            }
            finally
            {
                SqlList.Clear();
            }
            return true;
        }

        /// <summary>
        /// 断开数据库连接进程
        /// </summary>
        /// <param name="DatabaseName">数据库</param>
        /// <returns></returns>
        public bool KillSpId(string DatabaseName)
        {
            if (this.CreateKillSpIdProc(DatabaseName)) // 创建存储过程
            {
                string strSql = this.GetExecuteKillSPIDProcStr(DatabaseName);
                try
                {
                    SqlHelper.ExecuteNonQuery(_Conn, CommandType.Text, strSql);
                }
                catch
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 备份数据库
        /// </summary>
        /// <param name="DatabaseName"></param>
        /// <param name="FilePath"></param>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public bool BackupDataBase(string DatabaseName, string FilePath, string FileName)
        {
            string strSql = this.GetBackupDatabaseStr(DatabaseName, FilePath, FileName);
            try
            {
                SqlHelper.ExecuteNonQuery(_Conn, CommandType.Text, strSql);
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 还原数据库
        /// </summary>
        /// <param name="DatabaseName"></param>
        /// <param name="FilePath"></param>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public bool RestoreDataBase(string DatabaseName, string FilePath, string FileName)
        {
            if (this.KillSpId(DatabaseName))
            {
                Thread.Sleep(100);
                
                string strSql = this.GetRestoreDatabaseStr(DatabaseName, FilePath, FileName);
                try
                {
                    SqlHelper.ExecuteNonQuery(_Conn, CommandType.Text, strSql);
                }
                catch
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        ///分离数据库
        /// </summary>
        /// <returns></returns>
        public bool SpDatabase(String DatabaseName)
        {
            Thread.Sleep(100);
            string strSql = String.Format("sp_detach_db {0}", DatabaseName);
            try
            {
                SqlHelper.ExecuteNonQuery(_Conn, CommandType.Text, strSql);
            }
            catch
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// 附加数据库
        /// </summary>
        /// <returns></returns>
        public bool AddDataBase(string DatabaseName,string FileName)
        {
            Thread.Sleep(100);
            string strSql = String.Format("exec sp_attach_db {0},'{1}'", DatabaseName, FileName);
            try
            {
                SqlHelper.ExecuteNonQuery(_Conn, CommandType.Text, strSql);
            }
            catch
            {
                return false;
            }
            return true;
        }

    }
}
