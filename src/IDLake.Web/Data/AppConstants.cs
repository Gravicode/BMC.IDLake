using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDLake.Web.Data
{
    public class AppConstants
    {
        public const int FACE_WIDTH = 180;
        public const int FACE_HEIGHT = 135;
        public const string AppName = "IDLake.Web DAK";
        public const string FACE_SUBSCRIPTION_KEY = "a068e60df8254cc5a187e3e8c644f316";
        public const string FACE_ENDPOINT = "https://southeastasia.api.cognitive.microsoft.com/";
        public static string UploadUrlPrefix = "https://storagemurahaje.blob.core.windows.net/simada/";

        public static string BEARER_TOKEN_EDESK = "";
        
        public static string SQLConn = "";
        public const string GemLic = "EDWG-SKFA-D7J1-LDQ5";
        public static string RedisCon { set; get; }

        public static string GMapApiKey { get; set; }
        public static string BlobConn { get; set; }
        //public static string? Report_SertifikatSaham { get; set; }
        //public static string? Report_KetuaUB { get; set; }
        //public static string? Report_WakilUB { get; set; }
        //public static string? KodePengurus { get; set; }
        //public static string? SKUB { get; set; }
        //public static string? Kota { get; set; }
        //public static string? RekeningUB { get; set; }
        //public static string? CheckQRUrl { get; set; }
        public static string? DefaultPass { get; set; } = "123qweasd";
        public static string? LaporanStatistikUrl { get; set; }

        public static string[] KategoriData = new string[] { "General", "Science", "Automotive", "Government", 
        "Biology", "Chemistry","Physics","Fun","Machine Learning", "Office", "Entertainment","Games","Movies","Others","Language","Geography"}; 
       
        
    }
}
