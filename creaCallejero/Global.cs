using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.Carto;

namespace creaCallejero
{
    class Global
    {
        public static ILayer distritoLayer = null;
        public static ILayer mallaVialLayer = null;
        public static ILayer cruceMallaVialLayer = null;
        public static String nombreMaquina = Environment.MachineName;
        public static String nombreUsuario;

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
