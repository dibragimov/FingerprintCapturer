using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace HorioFingerprintCapturer
{
    public class SqlDBContext
    {
        private string SELECT_EMPLOYEE = "SELECT [Id],[Lastname],[Firstname],[IsActive],[Code] FROM [dbo].[Employees] WHERE [Firstname]+' '+[Lastname] LIKE '%{0}%' AND IsActive=1";
        private string SELECT_TEMPLATE = "SELECT [Template] FROM [dbo].[fingerprints] WHERE [PersonId]=@PersonId";
        private string UPDATE_TEMPLATE = "UPDATE [dbo].[fingerprints] SET [Template]=@Template, [Status]=1  WHERE [PersonId]=@PersonId";
        private string INSERT_TEMPLATE = "INSERT INTO [dbo].[fingerprints] ([PersonId],[Template], [Status]) VALUES (@PersonId, @Template, 1)";

        public List<Person> GetEmployees(string pattern)
        {
            List<Person> list = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(Settings.ConnectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SELECT_EMPLOYEE, conn);
                    //cmd.Parameters.Add("@SearchPattern", System.Data.SqlDbType.VarChar, 50).Value = pattern;
                    cmd.CommandText = string.Format(SELECT_EMPLOYEE, pattern);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        list = new List<Person>();
                        while (reader.Read())
                        {
                            Person p = new Person() { Id = Int32.Parse(reader[0].ToString()), LastName = reader[1].ToString(), FirstName = reader[2].ToString() };
                            if (!reader.IsDBNull(4))
                                p.Code = reader[4].ToString();
                            list.Add(p);
                        }
                    }
                    reader.Close();
                    reader.Dispose();

                    foreach (Person pers in list)
                    {
                        cmd.CommandText = SELECT_TEMPLATE;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@PersonId", System.Data.SqlDbType.Int).Value = pers.Id;
                        object obj = cmd.ExecuteScalar();
                        if (obj != null)
                        {
                            pers.Template = obj.ToString();
                        }
                    }

                    conn.Close();
                }
            }
            catch { }
            return list;
        }

        public bool SaveTemplate(Person p)
        {
            bool success = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(Settings.ConnectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(SELECT_TEMPLATE, conn);
                    cmd.Parameters.Add("@PersonId", System.Data.SqlDbType.Int).Value = p.Id;
                    object obj = cmd.ExecuteScalar();
                    if (obj != null)
                    {
                        cmd.CommandText = UPDATE_TEMPLATE;
                        cmd.Parameters.Add("@Template", System.Data.SqlDbType.VarChar, 2500).Value = p.Template;
                        int i = cmd.ExecuteNonQuery();
                        if (i > 0)
                            success = true;
                    }
                    else
                    {
                        cmd.CommandText = INSERT_TEMPLATE;
                        cmd.Parameters.Add("@Template", System.Data.SqlDbType.VarChar, 2500).Value = p.Template;
                        int i = cmd.ExecuteNonQuery();
                        if (i > 0)
                            success = true;
                    }

                    conn.Close();
                }
            }
            catch {
                success = false;
            }
            return success;
        }
    }
}
