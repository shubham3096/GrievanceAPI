using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Library
{
    public class SQLGateway
    {
        private string _conn_string;

        private SqlConnection GetConnection()
        {
            return new SqlConnection(this._conn_string);
        }

        private void CloseConnection(SqlConnection conn)
        {
            conn.Close();
        }

        public SQLGateway(string conn_string)
        {
            this._conn_string = conn_string;
        }

        //Function for Convert Date format in to SQL servar Date format
        public static string date(string txtdate)
        {
            string[] dateformate = txtdate.Split('/');
            string day = dateformate[0].ToString();
            string month = dateformate[1].ToString();
            string year = dateformate[2].ToString();
            string formateddate = year + '-' + month + '-' + day;
            return formateddate;
        }

        //Function for  Return dataset with procedure name and parameters
        public DataSet ExecuteProcedureWithDataset(String storedprocname, List<SqlParameter> _params)
        {
            SqlConnection conn = GetConnection();
            DataSet _ds = new DataSet();
            SqlCommand cmd = new SqlCommand();

            try
            {
                cmd.CommandText = storedprocname;
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;

                for (int i = 0; i < _params.Count; i++)
                {
                    cmd.Parameters.Add(_params[i]);
                }

                SqlDataAdapter _da = new SqlDataAdapter(cmd); ;

                _da.Fill(_ds, "DATASET");

                cmd.Dispose();

                CloseConnection(conn);

            }
            catch (Exception ex)
            {
                cmd.Dispose();
                CloseConnection(conn);
                conn.Dispose();
                return _ds;
            }
            finally
            {
                CloseConnection(conn);
                conn.Dispose();
            }
            return _ds;
        }

        //Function for  Return dataset with procedure name and parameters
        public DataTable ExecuteProcedure(String storedprocname, List<SqlParameter> _params)
        {
            SqlConnection conn = GetConnection();
            DataSet _ds = new DataSet();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = storedprocname;
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;

            for (int i = 0; i < _params.Count; i++)
            {
                cmd.Parameters.Add(_params[i]);
            }

            SqlDataAdapter _da = new SqlDataAdapter(cmd);

            _da.Fill(_ds, "DATATABLE");

            cmd.Dispose();
            cmd.Parameters.Clear();

            CloseConnection(conn);
            //try
            //{

            //}
            //catch(Exception ex)
            //{
            //    cmd.Parameters.Clear();

            //    CloseConnection(conn);
            //    conn.Dispose();
            //    return null;
            //}
            //finally
            //{
            //    CloseConnection(conn);
            //    conn.Dispose();
            //}
            return _ds.Tables["DATATABLE"];
        }
    }
}
