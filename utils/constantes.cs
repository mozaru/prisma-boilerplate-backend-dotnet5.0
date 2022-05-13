

namespace prisma.core
{
    public static class Constantes
    {
        // acesso ao banco
        public const string _BD_HOST_ = "#DB_HOST#";
        public const string _BD_PORT_ = "#DB_PORT#";
        public const string _BD_DATABASE_ = "#DB_DATABASE#";
        public const string _BD_LOGIN_ = "#DB_LOGIN#";
        public const string _BD_PASSWORD_ = "#DB_PASSWORD#";

        //servidor de email
        public const string _EMAIL_HOST_ = "#EMAIL_HOST#";
        public const int    _EMAIL_PORT_ = #EMAIL_PORT#;
        public const string _EMAIL_PROTOCOL_ = "#EMAIL_PROTOCOL#";
        public const string _EMAIL_USER_ = "#EMAIL_USER#";
        public const string _EMAIL_PASSWORD_ = "#EMAIL_PASSWORD#";
        public const string _SERVER_HOST_ = "#SERVER_HOST#";

        //#OAuth 2.0
        public const string _OAUTH_CHAVE_ = "#OAUTH_CHAVE#"; //tem que ter comprimento de  8, 16, 32, 64  
        public const int _OAUTH_CHAVES_VALIDADE_ = #OAUTH_CHAVES_VALIDADE#; //em horas
        public const int _OAUTH_ACCESS_TOKEN_VALIDADE_ = #OAUTH_ACCESS_TOKEN_VALIDADE#;  //em minutos
        public const int _OAUTH_REFRESH_TOKEN_VALIDADE_ = #OAUTH_REFRESH_TOKEN_VALIDADE#; //em horas
        public const string _OAUTH_TIPO_TOKEN_ACESSO_ = "#OAUTH_TIPO_TOKEN_ACESSO#";
        public const string _OAUTH_TIPO_TOKEN_REFRESH_ = "#OAUTH_TIPO_TOKEN_REFRESH#";
        public const string _OAUTH_TIPO_TOKEN_DEFAULT_ = #OAUTH_TIPO_TOKEN_DEFAULT#;
        public const string _OAUTH_PERFIL_PADRAO_ = "#OAUTH_PERFIL_PADRAO#";

        //AD
        public const string _AD_PATH_ = "#AD_PATH#";
        public const string _AD_USUARIO_ = "#AD_USUARIO#";
        public const string _AD_SENHA_ = "#AD_SENHA#";
    }
}
