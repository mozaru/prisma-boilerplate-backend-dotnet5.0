using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
//using MySql.Data.MySqlClient;
//using Newtonsoft.Json;
using System.Data;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Net;
using System.Text;
using System.IO;
//using System.Json;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using System.Dynamic;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace prisma.core
{
    public class Utils
    {
        public static string ObterToken(HttpRequest request)
        {
            if (!request.Headers.ContainsKey("Authorization"))
            {
                return null;
            }

            string token = request.Headers["Authorization"].ToString();

            return ObterToken(token);
        }

        public static string ObterToken(string token)
        {
            return token.Replace("Bearer", "").Trim();
        }
        private static string CriarToken(DateTime Expires, params Claim[] claims)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = Expires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Constantes._OAUTH_CHAVE_)), SecurityAlgorithms.HmacSha256Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
            return token;
        }

        public static TokenValidationParameters CriarTokenValidation()
        {
            var key = Encoding.ASCII.GetBytes(Constantes._OAUTH_CHAVE_);
            var validacao = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = System.TimeSpan.Zero
            };
            return validacao;
        }
        public static dynamic ObterPayloadToken(object token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken = null;
            tokenHandler.ValidateToken(token.ToString(), CriarTokenValidation(), out securityToken);
            if (securityToken == null)
                throw new ExceptionToken("Token inválido", 401);
            return DicToDynamic(((JwtSecurityToken)securityToken).Payload);
        }

        private static bool TentaObterPayloadToken(HttpRequest request, out dynamic payload)
        {
            payload = null;
            try
            {
                string token = ObterToken(request);
                if (token == null)
                    return false;
                payload = ObterPayloadToken(token);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static Random rnd = new Random();

        private static long Time()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
        private static string DataToStrJson<T>(T data)
        {
            MemoryStream stream = new MemoryStream();
            if (data == null)
                return "";
            DataContractJsonSerializer serialiser = new DataContractJsonSerializer(
                data.GetType(),
                new DataContractJsonSerializerSettings()
                {
                    UseSimpleDictionaryFormat = true
                });

            serialiser.WriteObject(stream, data);

            return Encoding.UTF8.GetString(stream.ToArray());
        }
        private static Stream StrToStream(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static DateTime StrToDate(string strData)
        {
            strData = strData.Trim().Replace(':', '|').Replace('/', '|').Replace(' ', '|').Replace('-', '|').Replace('\\', '|') + "|0|0|0|0|0|0";
            string[] v = strData.Split('|');
            if (int.Parse(v[0]) > 31 && int.Parse(v[2]) < 31)
                return new DateTime(int.Parse(v[0]), int.Parse(v[1]), int.Parse(v[2]));
            else
                return new DateTime(int.Parse(v[2]), int.Parse(v[1]), int.Parse(v[0]));
        }

        public static dynamic StrJsonToData(string str)
        {
            DataContractJsonSerializer serialiser = new DataContractJsonSerializer(
                typeof(Dictionary<string, object>),
                new DataContractJsonSerializerSettings()
                {
                    UseSimpleDictionaryFormat = true
                });

            using (Stream sr = StrToStream(str))
            {
                object x = serialiser.ReadObject(sr);
                return x==null?null:(dynamic)x;
            }

            //return JsonSerializer.Deserialize<T>(str);
        }
        public static string Md5(object input, bool isLowercase = false)
        {
            byte[] bytes;
            Type vType = input.GetType();
            if (input is string)
                bytes = Encoding.UTF8.GetBytes((string)input);
            else if (vType.IsArray && typeof(byte).IsAssignableFrom(vType.GetElementType()))
                bytes = (byte[])input;
            else
            {
                string txt = DataToStrJson(input).ToString();
                bytes = Encoding.UTF8.GetBytes(txt);
            }

            using (var md5 = MD5.Create())
            {
                var byteHash = md5.ComputeHash(bytes);
                var hash = BitConverter.ToString(byteHash).Replace("-", "");
                return (isLowercase) ? hash.ToLower() : hash;
            }
        }

        public static string[] JArrayToStringArray(object json)
        {
            return JsonSerializer.Deserialize<string[]>(((JsonElement)json).GetRawText()) ?? new string[0];
        }
        public static string JArrayToString(object json)
        {
            string[] vet = JsonSerializer.Deserialize<string[]>(((JsonElement)json).GetRawText());
            return vet==null?"":string.Join(",", vet);
        }
        public static string JArrayToInt(object json)
        {
            int[] vet = JsonSerializer.Deserialize<int[]>(((JsonElement)json).GetRawText());
            return vet == null ? "" : string.Join(",", vet);
        }
        public static string JArrayToDouble(object json)
        {
            double[] vet = System.Text.Json.JsonSerializer.Deserialize<double[]>(((JsonElement)json).GetRawText());
            return vet==null?"":string.Join(",", vet);
        }
        public static string GetServerIp()
        {
            string IPAddress = "";
            IPHostEntry Host = new();
            string Hostname = "";
            Hostname = System.Environment.MachineName;
            Host = Dns.GetHostEntry(Hostname);
            foreach (IPAddress IP in Host.AddressList)
            {
                if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    IPAddress = Convert.ToString(IP)??"";
                }
            }
            return IPAddress;
        }

        public static string GetClientIp(HttpRequest request)
        {
            return ""+request.HttpContext.Connection.RemoteIpAddress;
        }

        public static IEnumerable<Dictionary<string, object>> GetJsonValues(JsonElement json)
        {
            var r = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>[]>(json.GetRawText());
            if (r==null)
                return new List<Dictionary<string, object>>();
            return r;
        }

        public static dynamic DicToDynamic(Dictionary<string, object> dic)
        {
            var result = new ExpandoObject() as IDictionary<string, object>;
            foreach (var v in dic)
                result.Add(v.Key, v.Value);
            return result;
        }

        public static bool PropertyExist(dynamic obj, string name)
        {
            if (obj is ExpandoObject)
                return ((IDictionary<string, object>)obj).ContainsKey(name);

            return obj.GetType().GetProperty(name) != null;
        }

        public static IEnumerable<dynamic> GetValues(JsonElement json)
        {
            var r = JsonSerializer.Deserialize<Dictionary<string, object>[]>(json.GetRawText());
            List<dynamic> list = new List<dynamic>();
            if (r != null)
                foreach (var v in r)
                    list.Add(DicToDynamic(v));
            return list;
        }

        private static byte[] GenerateIV(int size)
        {
            #if NET5_0
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] iv = new byte[size];
                rng.GetBytes(iv);
                return iv;
            }
            #else
            return RandomNumberGenerator.GetBytes(size);
            #endif
        }

        public static string GerarChave(string cpf, string motivo)
        {
            DateTime dt = DateTime.UtcNow.AddHours(Constantes._OAUTH_CHAVES_VALIDADE_);
            return CriarToken(dt,
                            new Claim("cpf", cpf),
                            new Claim("motivo", motivo),
                            new Claim("data", dt.Ticks.ToString()));
        }

        public static string GerarSenha(int qtd = 8)
        {
            string LETRAS = "abcdefghijklmnopqrstuvwxyz1234567890";
            int len = LETRAS.Length;
            char[] senha = new char[qtd];
            for (int i = 0; i < qtd; i++)
            {
                senha[i] = LETRAS[rnd.Next(1, len + 1) - 1];
            }

            for (int i = 0; i < 3 * qtd; i++)
            {
                int p1 = rnd.Next(0, qtd);
                int p2 = (rnd.Next(1, qtd) + p1) % qtd;
                char aux = senha[p1];
                senha[p1] = senha[p2];
                senha[p2] = aux;
            }
            return new string(senha);
        }

        public static string ToBase64(object valor)
        {
            if (valor == null)
                return "";
            Type vType = valor.GetType();
            if (valor is string)
                return Convert.ToBase64String(Encoding.UTF8.GetBytes((string)valor)).Trim('=');
            else if (vType.IsArray && typeof(byte).IsAssignableFrom(vType.GetElementType()))
                return Convert.ToBase64String((byte[])valor);
            else
            {
                string txt = DataToStrJson(valor).ToString();
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(txt)).Trim('=');
            }
        }

        public static byte[] FromBase64(object valor)
        {
            string texto = valor is string ? (string)valor : ""+valor;
            if (texto.Length % 4 != 0)
                texto += "===".Substring(texto.Length % 4 - 1);
            return Convert.FromBase64String(texto);
        }

        /*public static string JWTEncoder(object dados)
        {
            string header = "{\n\"typ\":\"JWT\",\n\"alg\":\"HS256\"\n}";
            string texto = ToBase64(header) + "." + ToBase64(dados);
            texto = texto + "." + HS256(texto);
            return texto;
        }

        public static dynamic JWTDecoder(string token)
        {
            try
            {
                string[] vet = token.Split('.');
                dynamic header = StrJsonToData(Encoding.UTF8.GetString(FromBase64(vet[0])));
                dynamic payload = StrJsonToData(Encoding.UTF8.GetString(FromBase64(vet[1])));
                string assinatura = HS256(vet[0] + "." + vet[1]);
                if (assinatura != vet[2])
                    throw new Exception("token invalido!");
                return payload;
            }
            catch (Exception)
            {
                throw new Exception("token em formato invalido");
            }
        }*/

        public static string CriarAccessToken(int id, string login, string perfil)
        {
            return CriarToken(DateTime.UtcNow.AddMinutes(Constantes._OAUTH_ACCESS_TOKEN_VALIDADE_),
                              new Claim("id", id.ToString()),
                              new Claim("login", login),
                              new Claim("perfil", perfil));
        }
        public static string CriarRefreshToken(int id, string login, string perfil)
        {
            return CriarToken(DateTime.UtcNow.AddHours(Constantes._OAUTH_REFRESH_TOKEN_VALIDADE_),
                              new Claim("id", id.ToString()),
                              new Claim("login", login),
                              new Claim("perfil", perfil));
        }
        /*public static dynamic GerarPayloadJWT(dynamic usuario, string iporigem, bool AccessToken = true)
        {
            int delta = (AccessToken ? Constantes._OAUTH_ACCESS_TOKEN_VALIDADE_ : Constantes._OAUTH_REFRESH_TOKEN_VALIDADE_) * 60;//converter de minutos para segundos
            long tempo = Time();
            string payload = string.Format("{{\"iss\":\"{0}\",\"iat\": {1},\"exp\": {2},\"sub\": \"{3}\",\"tipo\":\"{4}\", \"duracao\":{5}, \"email\":\"{6}\", \"perfil\":\"{7}\",\"tipo_usuario\":\"{8}\", \"id\":\"{9}\"}}",
                iporigem,
                tempo,
                tempo + delta,
                usuario.id,
                AccessToken ? Constantes._OAUTH_TIPO_TOKEN_ACESSO_ : Constantes._OAUTH_TIPO_TOKEN_REFRESH_,
                delta,
                usuario.login,
                Constantes._OAUTH_PERFIL_PADRAO_, //todo: pegar o perfil real no banco
                usuario.perfil,
                usuario.id);
            return StrJsonToData(payload);
        }

        public static string GerarToken(HttpRequest request, dynamic usuario, bool AccessToken = true)
        {
            dynamic payload = GerarPayloadJWT(usuario, GetClientIp(request), AccessToken);
            return JWTEncoder(payload);
        }
        */
        public static dynamic ChecarToken(HttpRequest request, string iporigem, string tipo = Constantes._OAUTH_TIPO_TOKEN_DEFAULT_, string perfil = Constantes._OAUTH_PERFIL_PADRAO_)
        {
            try
            {
                dynamic obj;
                if (!TentaObterPayloadToken(request, out obj))
                    throw new ExceptionToken("Token não validado!", 401);
                Console.WriteLine("tipo={0}\n{1}", tipo, DataToStrJson(obj));
                if (obj==null)
                    throw new ExceptionToken("Token não validado!", 401);
                else if (iporigem != ""+obj["iss"])
                    throw new ExceptionToken("Origem invalida!", 400);
                else if (obj.ContainsKey("tipo") && obj["tipo"].ToString() != tipo)
                    throw new ExceptionToken("Tipo de token incompativel", 400);
                else if (obj.ContainsKey("perfil") && obj["perfil"].ToString() != perfil && tipo == Constantes._OAUTH_TIPO_TOKEN_ACESSO_)
                    throw new ExceptionToken("Este usuario nao possui autorizacao para usar este recurso", 400);
                else if (!obj.ContainsKey("exp"))
                    throw new ExceptionToken("Token não possui o tempo de expiracao 'exp'", 401);
                else if ((long)obj["exp"] < Time())
                    throw new Exception("Token Expirado");
                return obj;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public static void ControlaAcesso(HttpRequest request, string perfil = Constantes._OAUTH_PERFIL_PADRAO_)
        {
            if (!request.Headers.ContainsKey("Authorization"))
                throw new Exception("acesso nao permitido\nAuthorization nao encontrado");
            string x = request.Headers["Authorization"];
            string[] vet = x.Split(' ');
            if (vet[0].ToLower() != "bearer")
                throw new Exception("Autorization deve ser do tipo bearer");
            else
                ChecarToken(request, GetClientIp(request), Constantes._OAUTH_TIPO_TOKEN_DEFAULT_, perfil);
        }

        public static dynamic GetPayloadToken(HttpRequest request, string perfil = Constantes._OAUTH_PERFIL_PADRAO_)
        {
            if (!request.Headers.ContainsKey("Authorization"))
                throw new Exception("acesso nao permitido\nAuthorization nao encontrado");
            string x = request.Headers["Authorization"];
            string[] vet = x.Split(' ');
            if (vet[0].ToLower() != "bearer")
                throw new Exception("Autorization deve ser do tipo bearer");
            else
            {
                dynamic payload;
                if (!TentaObterPayloadToken(request, out payload))
                    throw new ExceptionToken("Token não validado!", 401);
                return payload;
            }
        }

        public static int GetIdCurrentUser(HttpRequest request, string perfil = Constantes._OAUTH_PERFIL_PADRAO_)
        {
            dynamic payload = GetPayloadToken(request);
            return payload == null ? -1 : int.Parse(payload.id);
        }

        private static int GetHexVal(char hex)
        {
            if (hex < 'A')
            {
                return (byte)(hex - '0');
            }
            else if (hex < 'a')
            {
                return (byte)(hex - 'A') + 10;
            }
            else
            {
                return (byte)(hex - 'a') + 10;
            }
        }

        private static byte[] HexToBytes(string strhex)
        {
            if (strhex.Length % 2 == 1)
            {
                return new byte[] { };
            }

            byte[] arr = new byte[strhex.Length / 2];

            for (int i = 0; i < arr.Length; ++i)
            {
                arr[i] = (byte)((GetHexVal(strhex[i * 2]) << 4) |
                                (GetHexVal(strhex[i * 2 + 1])));
            }

            return arr;
        }

        public static string HS256(string texto)
        {
            var hash = new HMACSHA256(Encoding.UTF8.GetBytes(Constantes._OAUTH_CHAVE_));
            return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(texto))).Trim('=');
        }

        public static JsonResult Success(object valor)
        {
            if (valor is string)
                return new JsonResult(new Dictionary<String, Object>{
                        {"status",200},
                        {"message",valor}
                        })
                {
                    StatusCode = 200
                };
            else
                return new JsonResult(valor)
                {
                    StatusCode = 200 // Status code here 
                };
        }

        public static JsonResult Fail(object valor, int status)
        {
            if (valor is string)
                return new JsonResult(new Dictionary<String, Object>{
                        {"status",status},
                        {"message",valor}
                        })
                {
                    StatusCode = status
                };
            else
                return new JsonResult(valor)
                {
                    StatusCode = status // Status code here 
                };
        }

        public static string GetValueOrDefault(Dictionary<string, object> obj, string campo, string valor)
        {
            if (obj == null || !obj.ContainsKey(campo) || obj[campo] == null || string.IsNullOrEmpty(obj[campo].ToString()))
                return valor;
            else
                return ""+obj[campo];
        }
        public static int GetValueOrDefault(Dictionary<string, object> obj, string campo, int valor)
        {
            if (obj == null || !obj.ContainsKey(campo) || obj[campo] == null || string.IsNullOrEmpty(obj[campo].ToString()))
                return valor;
            else
                return int.Parse("0"+obj[campo]);
        }
        public static double GetValueOrDefault(Dictionary<string, object> obj, string campo, double valor)
        {
            if (obj == null || !obj.ContainsKey(campo) || obj[campo] == null || string.IsNullOrEmpty(obj[campo].ToString()))
                return valor;
            else
                return double.Parse("0"+obj[campo]);
        }
    }
}