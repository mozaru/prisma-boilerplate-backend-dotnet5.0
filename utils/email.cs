using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Net.Mail;

namespace prisma.core
{
    class Email
    {
        private string usuario = Constantes._EMAIL_USER_;
        private string senha = Constantes._EMAIL_PASSWORD_;
        private string host = Constantes._EMAIL_HOST_;
        private int porta = Constantes._EMAIL_PORT_;
        private string protocolo = Constantes._EMAIL_PROTOCOL_;
        private string erro = "";

        public string GetErro()
        {
            return erro;
        }

        public bool Enviar(string para, string assunto, string texto)
        {
            try
            {
                MailMessage message = new MailMessage(usuario, para);
                message.SubjectEncoding = System.Text.Encoding.UTF8;
                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.Subject = assunto;
                message.Body = texto.Replace("\n", "<br/>");
                message.IsBodyHtml = true;
                //message.From = new MailAddress(usuario, "Quickly");
                SmtpClient client = new SmtpClient(host, porta);
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(usuario, senha);
                client.EnableSsl = protocolo.ToLower() == "ssl";
                client.Send(message);
                return true;
            }
            catch (Exception e)
            {
                erro = e.Message;
                return false;
            }
        }
    }
}