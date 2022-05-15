using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
#if MYSQL
using MySql.Data.MySqlClient;
#elif SQLITE
using System.Data.SQLite;
#elif SQSERVER
using System.Data.SqlClient;
#elif POSTGRESQL
using Npgsql;
#endif

namespace prisma.core
{
    public class Banco : IDisposable
    {
        private string _host = Constantes._BD_HOST_;
        private string _port = Constantes._BD_PORT_;
        private string _db_name = Constantes._BD_DATABASE_;
        private string _username = Constantes._BD_LOGIN_;
        private string _password = Constantes._BD_PASSWORD_;
        private IDbConnection? _conn=null;
        private IDbCommand? _stmt=null;
        private string _tipo = "QUERY";
        private string _erro = "";
        private long _rowcount = 0;
        private int _lastid = 0;

        public Banco()
        {
            _tipo = "QUERY";
        }

        public void Conectar()
        {
            _erro = "";
            try
            {
                if (_conn != null)
                    return;
#if MYSQL
                string ConnectionString = $"server={_host};user={_username};password={_password};port=3306;database={_db_name}";
                _conn = new MySqlConnection(ConnectionString);
#elif SQLITE
                string ConnectionString = $"Data Source={_db_name}.sqlite;Version=3;";
                _conn = new SQLiteConnection(ConnectionString);   
                if (!System.IO.File.Exists(_db_name+".sqlite"))
                {
                    string[] paths = new string[]{ ".", "..\\database", ".\\database" };
                    bool found = false;
                    foreach (string sqlPath in paths)
                    {
                        string filename = System.IO.Path.Combine(sqlPath, "create-database.sqlite.sql");
                        if (!found && System.IO.File.Exists(filename))
                        {
                            CreateSQLiteDataBase(System.IO.File.ReadAllText(filename));
                            found = true;
                        }
                    }
                    if (!found)
                        throw new Exception("Database file not found!");
                }
#elif SQSERVER
                string ConnectionString = string.Format("workstation id={0};packet size=4096;user id={1};pwd={2};data source={0};persist security info=False;initial catalog={3}", _host, _username, _password, _db_name);
                _conn = new SqlConnection(ConnectionString);
#elif POSTGRESQL
                string ConnectionString = string.Format("Host={0};Port={1};Username={2};Password={3};Database={4}", _host, _port, _username, _password, _db_name);
                _conn = new NpgsqlConnection(ConnectionString);
#endif
                _conn?.Open();
                //_conn->exec("set names utf8");
            }
            catch (Exception e)
            {
                _erro = "Não foi possivel conectar com o Banco de Dados\n" + e.Message;
            }
        }

#if SQLITE
        public void CreateSQLiteDataBase(string sql)
        {
            _conn.Open();
            using (_stmt = _conn?.CreateCommand())
            {
                _stmt.CommandText = sql;
                _stmt.ExecuteNonQuery();
            }
            _conn.Close();
        }
#endif

        private void LiberarStmt()
        {
            try
            {
                if (_stmt != null)
                {
                    _stmt.Dispose();
                    _stmt = null;
                }
            }
            catch (Exception e)
            {
                _stmt = null;
                Console.WriteLine("ERRO: [liberar command] {0}", e.Message);
            }
        }
        public void Desconectar()
        {
            LiberarStmt();
            try
            {
                if (_conn != null)
                {
                    _conn.Close();
                    _conn.Dispose();
                    _conn = null;
                }
            }
            catch (Exception e)
            {
                _conn = null;
                Console.WriteLine("ERRO: [desconectar] {0}", e.Message);
            }
        }

        public bool TemErro() { return _erro != ""; }

        public string GetErro() { return _erro; }

        public void Prepara(string query)
        {
            try
            {
                _rowcount = -1;
                _lastid = -1;
                Conectar();
                if (TemErro()) return;
                _stmt = _conn?.CreateCommand();
                if (_stmt == null)
                    return;
                string querylower = query.ToLower();
                if (querylower.Contains("declare"))
                {
                    _tipo = "QUERY";
                }
                else if (querylower.Contains("insert"))
                {
                    _tipo = "INSERT";
                    if (query[0] == '*')
                    {
                        querylower = querylower.Substring(1);
                        query = query.Substring(1);
                        _tipo = "*INSERT";
                    }
#if SQSERVER
                    else
                    {
                        int pos = querylower.IndexOf("values");
                        query = query.Insert(pos, "OUTPUT INSERTED.ID ");
                    }
#elif POSTGRESQL
                    else
                    {
                        query = query + " RETURNING id";
                        //query = query + "; select currval('table_sequence')"
                    }
#endif
                }
                else if (querylower.Contains("update"))
                    _tipo = "UPDATE";
                else if (querylower.Contains("delete"))
                    _tipo = "DELETE";
                else
                    _tipo = "QUERY";
                _stmt.CommandText = query;
#if MYSQL
#elif SQLITE
                _stmt.Prepare();
#elif SQSERVER
                _stmt.Prepare();
#elif POSTGRESQL
#endif
            }
            catch (Exception e)
            {
                _erro = "Erro na preparação da consulta ao banco de dados\n" + e.Message;
            }
        }

        public void PreparaLimitado(string query, int inicio, int qtd)
        {
            try
            {
                query = query + string.Format(" limit {0}, {1}", inicio, qtd);
                _rowcount = -1;
                _lastid = -1;
                Conectar();
                if (TemErro()) return;
                _stmt = _conn?.CreateCommand();
                if (_stmt == null)
                    return;
                string querylower = query.ToLower();
                if (querylower.Contains("insert"))
                {
                    _tipo = "INSERT";
#if SQSERVER
                    int pos = querylower.IndexOf("values");
                    query = query.Insert(pos, "OUTPUT INSERTED.ID ");
#elif POSTGRESQL
                    query = query + " RETURNING id";
                    //query = query + "; select currval('table_sequence')"
#endif
                }
                else if (querylower.Contains("update"))
                    _tipo = "UPDATE";
                else if (querylower.Contains("delete"))
                    _tipo = "DELETE";
                else
                    _tipo = "QUERY";
                _stmt.CommandText = query;
#if MYSQL
#elif SQLITE
                _stmt.Prepare();
#elif SQSERVER
                _stmt.Prepare();
#elif POSTGRESQL
#endif
            }
            catch (Exception e)
            {
                _erro = "Erro na preparação da consulta ao banco de dados\n" + e.Message;
            }
        }

        public void Parametro(string nome, object? valor)
        {
            try
            {
                if (TemErro() || _stmt==null) return;
                if (_stmt.Parameters.Contains(nome))
                    return;
                else if (valor is System.Text.Json.JsonElement)
                    valor = valor.ToString();
                IDbDataParameter p = _stmt.CreateParameter();
                p.ParameterName = nome;
                if (valor==null)
                    p.Value = System.Data.SqlTypes.SqlString.Null;
                else if (valor is string && (""+valor.ToString()).Contains("null"))
                    p.Value = System.Data.SqlTypes.SqlString.Null;
                else
                    p.Value = valor;
                _stmt.Parameters.Add(p);
            }
            catch (Exception e)
            {
                _erro = "Falha na configuração dos parametros da consulta ao banco de dados\n" + e.Message;
            }
        }

        public long Count()
        {
            try
            {
                if (TemErro()) return -1;
                return _rowcount;
            }
            catch (Exception e)
            {
                _erro = "Erro ao tentar verificar a quantidade de elementos da consulta ao banco de dados\n" + e.Message;
                return -1;
            }
        }

        public int LastID()
        {
            try
            {
                if (TemErro()) return -1;
                return _lastid;
            }
            catch (Exception e)
            {
                _erro = "Erro ao tentar verificar o ultimo id inserido no banco de dados\n" + e.Message;
                return -1;
            }
        }
        private List<dynamic> FetchAll(IDataReader reader)
        {
            List<dynamic> results = new List<dynamic>();
            List<string> cols = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
                if (!cols.Contains(reader.GetName(i)))
                    cols.Add(reader.GetName(i));

            while (reader.Read())
                results.Add(Fetch(cols, reader));

            return results;
        }
        private dynamic Fetch(IEnumerable<string> cols,
                              IDataReader reader)
        {
            var result = new ExpandoObject() as IDictionary<string, object>;
            foreach (string col in cols)
                result.Add(col, reader[col]);
            return result;
        }
        private dynamic CreateDynamicObject(string campo, object valor)
        {
            var result = new ExpandoObject() as IDictionary<string, object>;
            result.Add(campo, valor);
            return result;
        }

        public List<dynamic> Executar()
        {
            try
            {
                if (TemErro() || _stmt==null) return new();
#if MYSQL
                _stmt.Prepare();
#elif SQLITE
#elif SQSERVER
#elif POSTGRESQL
                _stmt.Prepare();
#endif
                List<dynamic>? data = null;
                if (_tipo == "QUERY")
                {
                    using (IDataReader r = _stmt.ExecuteReader())
                        data = (List<object>)FetchAll(r);
                    _rowcount = data.Count;
                }
                else if (_tipo == "*INSERT")
                {
                    _stmt.ExecuteNonQuery();
                    _lastid = -1;
                    _rowcount = 1;
                    data = new List<object>() { CreateDynamicObject("id", _lastid) };
                }
                else if (_tipo == "INSERT")
                {

#if MYSQL
                    _stmt.ExecuteNonQuery();
                    _lastid = (int)((MySqlCommand)_stmt).LastInsertedId;
#elif SQLITE
                    _stmt.ExecuteNonQuery();
                    SQLiteConnection? conn = _conn as SQLiteConnection;
                    _lastid = (int)((conn == null) ? -1 : conn.LastInsertRowId);                    
#elif SQSERVER
                    _lastid = (int)_stmt.ExecuteScalar();
#elif POSTGRESQL
                    _lastid = (int)_stmt.ExecuteScalar();
#endif
                    _rowcount = 1;
                    data = new List<object>() { CreateDynamicObject("id", _lastid) };
                }
                else if (_tipo == "UPDATE")
                {
                    _rowcount = _stmt.ExecuteNonQuery();
                    data = new List<object>() { CreateDynamicObject("rowcount", _rowcount) };
                }
                else if (_tipo == "DELETE")
                {
                    _rowcount = _stmt.ExecuteNonQuery();
                    data = new List<object>() { CreateDynamicObject("rowcount", _rowcount) };
                }
                else
                    throw new Exception("deu problema");
                return data;
            }
            catch (Exception e)
            {
                _erro = "Erro na execução da consulta ao banco de dados\n" + e.Message;
                return new();
            }
            finally
            {
                LiberarStmt();
            }
        }

        public List<dynamic>? Listar(string query)
        {
            try
            {
                Prepara(query);
                return Executar();
            }
            catch (Exception e)
            {
                _erro = "Erro ao tentar listar os elementos do banco de dados\n" + e.Message;
                return null;
            }
            finally
            {
                LiberarStmt();
            }
        }

        public object? ObterPeloId(string query, int id)
        {
            try
            {
                Prepara(query);
                Parametro("id", id);
                var vet = Executar();
                if (TemErro()) return -1;
                return vet[0];
            }
            catch (Exception e)
            {
                _erro = "Erro ao obter um elemento do banco de dados\n" + e.Message;
                return null;
            }
            finally
            {
                LiberarStmt();
            }
        }

        public void Dispose()
        {
            Desconectar();
        }
    }
}