using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.AnalysisTools;
using ESRI.ArcGIS.DataManagementTools;

namespace creaCallejero
{
    class Global
    {
        public static IFeature viaACalcular = null;
        public static ILayer distritoLayer = null;
        public static ILayer mallaVialLayer = null;
        public static ILayer cruceMallaVialLayer = null;
        public static String nombreMaquina = Environment.MachineName;
        public static String nombreUsuario = Environment.UserName;
        public static DateTime fechaCreacionLocal;
        public static DateTime fechaCreacionUTC;
        public static DateTime fechaModificacionLocal;
        public static DateTime fechaModificacionUTC;
        public static bool creationStatus = false;
        public static bool extensionActiva = false;

        // Capas strings

        // Prod
        //public static String mallavialName = "CONTUGAS.MALLAVIALDF";
        //public static String cruceMallaVialName = "CONTUGAS.CRUCEMALLAVIAL";
        //public static String distritoName = "CONTUGAS.DISTRITO";

        // Dev
        public static String mallavialName = "CONTUGAS_MALLAVIALDF";
        public static String cruceMallaVialName = "CONTUGAS_CRUCEMALLAVIAL";
        public static String distritoName = "CONTUGAS_DISTRITO";

        // Base de datos
        public static String connectionString = 
@"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)
(HOST=localhost)(PORT=1521)) 
(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = lingonet))); User Id = { 0 }; Password={1};"
.Replace(Environment.NewLine, "");
        public static Boolean dbStatusConnected = false;
        public static String dbTableName = "BLOGTABLE";
    }
}
