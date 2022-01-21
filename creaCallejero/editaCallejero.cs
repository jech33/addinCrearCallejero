using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Diagnostics;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Desktop.AddIns;
using ESRI.ArcGIS.Framework;

namespace creaCallejero
{
    public class editaCallejero : ESRI.ArcGIS.Desktop.AddIns.Extension
    {
        private static editaCallejero s_editaCallejero;
        private IMap map;
        private IEditor3 m_editor;
        private IEditEvents_Event m_editEvents;
        private IEditEvents5_Event m_editEvents5;
        // Atributos adicionales
        creaCallejero callejero;
        IGeometryCollection cruceGeometryToEdit = new PolylineClass();
        IFeature viaEnEdicionCruceMallavial;

        public editaCallejero()
        {
            
        }

        public void InitializeExtension()
        {
            if (this.State != ExtensionState.Enabled || Global.extensionActiva != true)
            { return; }

            // Inicializar variables
            IMxDocument mxdoc = ArcMap.Document as IMxDocument;
            map = mxdoc.FocusMap;
            m_editor = ArcMap.Editor as IEditor3;
            m_editEvents = m_editor as IEditEvents_Event;
            m_editEvents5 = m_editor as IEditEvents5_Event;
            callejero = new creaCallejero();
            m_editEvents.OnChangeFeature += OnChangeFeature;
            m_editEvents.OnSelectionChanged += OnSelectionChanged;
        }

        public void UninitalizeExtension()
        {
            if (s_editaCallejero == null)
            { return; }

            // Detach event handlers
            m_editEvents.OnChangeFeature -= OnChangeFeature;
            m_editEvents.OnSelectionChanged -= OnSelectionChanged;
        }

        protected override void OnStartup()
        {
            s_editaCallejero = this;
            Global.extensionActiva = true;
            InitializeExtension();
            if (this.State == ExtensionState.Enabled)
            {
                UninitalizeExtension();
            }
        }

        protected override void OnShutdown()
        {
            Global.extensionActiva = false;
            UninitalizeExtension();
            s_editaCallejero = null;
            map = null;
            base.OnShutdown();
        }

        protected override bool OnSetState(ExtensionState state)
        {
            this.State = state;
            if (state == ExtensionState.Enabled)
            {
                InitializeExtension();
            }
            else
            {
                UninitalizeExtension();
            }
            return true;
        }

        protected override ExtensionState OnGetState()
        {
            return this.State;
        }

        // Static methods
        internal static bool IsExtensionEnabled()
        {
            if (s_editaCallejero == null)
            {
                GetTheExtension();
            }

            if (s_editaCallejero == null)
            {
                return false;
            }

            if (s_editaCallejero.State == ExtensionState.Enabled)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static editaCallejero GetTheExtension()
        {
            UID extensionID = new UIDClass();
            extensionID.Value = ThisAddIn.IDs.editaCallejero;
            ArcMap.Application.FindExtensionByCLSID(extensionID);
            return s_editaCallejero;
        }

        private void OnChangeFeature(IObject obj)
        {
            IFeature viaEditada = obj as IFeature;
            string layerName = ((IDataset)viaEditada.Class).Name;
            resetSeleccion();

            // Llenar variables globales
            IMap mapa = ArcMap.Document.FocusMap;
            IActiveView m_focusMap = mapa as IActiveView;
            List<string> listaCapasNombre = new List<string>();
            List<ILayer> capas = new List<ILayer>();
            for (int i = 0; i < mapa.LayerCount; i++)
            {
                if (string.Equals(mapa.get_Layer(i).Name, Global.mallavialName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(mapa.get_Layer(i).Name, Global.distritoName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(mapa.get_Layer(i).Name, Global.cruceMallaVialName, StringComparison.OrdinalIgnoreCase))
                {
                    capas.Add(mapa.get_Layer(i));
                    listaCapasNombre.Add(mapa.get_Layer(i).Name);
                }
            }
            int indexDistrito = listaCapasNombre.FindIndex(a => string.Equals(a, Global.distritoName, StringComparison.OrdinalIgnoreCase));
            int indexCapaVial = listaCapasNombre.FindIndex(a => string.Equals(a, Global.mallavialName, StringComparison.OrdinalIgnoreCase));
            int indexCruceMallavial = listaCapasNombre.FindIndex(a => string.Equals(a, Global.cruceMallaVialName, StringComparison.OrdinalIgnoreCase));
            if (indexDistrito == -1 || indexCapaVial == -1 || indexCruceMallavial == -1)
            {
                MessageBox.Show("NO ES POSIBLE AGREGAR CALLEJERO: verifique que se encuentren cargadas las capas MALLAVIAL, CRUCEMALLAVIAL y DISTRITO en el mapa");                
                m_editor.AbortOperation();
                m_focusMap.Refresh();
                return;
            }
            Global.distritoLayer = capas[indexDistrito];
            Global.mallaVialLayer = capas[indexCapaVial];
            Global.cruceMallaVialLayer = capas[indexCruceMallavial];
            Debug.WriteLine(layerName);

            if (string.Equals(layerName, Global.mallavialName, StringComparison.OrdinalIgnoreCase) && !Global.creationStatus)
            {

                // Asignar valores a via editada
                Global.fechaModificacionLocal = DateTime.Now;
                Global.fechaModificacionUTC = DateTime.UtcNow;
                viaEditada.Value[viaEditada.Fields.FindField("NOMBREVIA")] = (Convert.ToString((viaEditada.Value[viaEditada.Fields.FindField("NOMBREVIA")]))).ToUpper();
                viaEditada.Value[viaEditada.Fields.FindField("USUARIOMODIFICACION")] = Global.nombreUsuario.ToUpper();
                viaEditada.Value[viaEditada.Fields.FindField("FECHAMODIFICACION")] = Global.fechaModificacionLocal;
                //viaEditada.Value[viaEditada.Fields.FindField("MAQUINA")] = Global.nombreMaquina;
                calculateChanges(viaEditada); 
                               
                UID pUID = new UID();
                // pUID.Value = "{59D2AFD2-9EA2-11D1-9165-0080C718DF97}";
                // or you can use the ProgID
                // pUID.Value = Editor.SaveEditCommand;
                ICommandItem pCmdItem = ArcMap.Application.Document.CommandBars.Find(pUID);
                pUID.SubType = 3;
                pCmdItem.Execute();
            }

        }

        private void OnSelectionChanged() { }

        // Probar con queryfilter de codigovia para seleccionar todas las vias que corresponden al codigovia editado
        // Eliminar via de crucemallavial
        // Insertar vias en crucemallavial por segmento

        // Metodos creados
        public void seleccionarVia(IFeature via, IFeatureLayer capaCruceVial)
        {
            string codigoViaMalla = Convert.ToString(via.Value[via.Fields.FindField("CODIGOVIA")]);
            IQueryFilter queryCodigoVia = new QueryFilter();
            queryCodigoVia.WhereClause = $"CODIGOVIA = '{codigoViaMalla}'";

            IFeatureClass cruceFeatureClass = capaCruceVial.FeatureClass;
            IFeatureCursor cursorCruce = cruceFeatureClass.Update(queryCodigoVia, false);
            viaEnEdicionCruceMallavial = cursorCruce.NextFeature();
        }

        public void resetSeleccion()
        {
            map = ArcMap.Document.FocusMap;

            map.ClearSelection();        

        }

        public void actualizarSegmentosSplit(IFeature viaEditada)
        {
            int codigoSegmentoViaEditada = Convert.ToInt32(viaEditada.Value[viaEditada.Fields.FindField("CODIGOSEGMENTOVIA")]);
            String codigoViaMalla = Convert.ToString(viaEditada.Value[viaEditada.Fields.FindField("CODIGOVIA")]);
            List<int> listCodigosSegmentos = new List<int>();
            listCodigosSegmentos.Add(codigoSegmentoViaEditada);

            // Crear query de consulta con codigoSegmento y codigoVia
            IQueryFilter queryCodigoSegmentoVia = new QueryFilter();
            queryCodigoSegmentoVia.WhereClause = $"CODIGOSEGMENTOVIA = {codigoSegmentoViaEditada}";
            IQueryFilter queryCodigoVia = new QueryFilter();
            queryCodigoVia.WhereClause = $"CODIGOVIA = '{codigoViaMalla}'";

            // Crear cursor mallavial
            IFeatureClass mallaVialFeatureClass = (Global.mallaVialLayer as IFeatureLayer).FeatureClass;
            IFeatureCursor cursorMalla = mallaVialFeatureClass.Update(queryCodigoSegmentoVia, false);
            IFeature cursorMallaFeature = cursorMalla.NextFeature();
            bool firstFeature = true;

            // Crear cursor crucemallavial
            IFeatureClass cruceMallaVialFeatureClass = (Global.cruceMallaVialLayer as IFeatureLayer).FeatureClass;
            IFeatureCursor cursorCruce = cruceMallaVialFeatureClass.Update(queryCodigoVia, false);
            IFeature cursorCruceFeature = cursorCruce.NextFeature();
            if (cursorCruceFeature != null)
            {
                cursorCruceFeature.Delete();
                cursorCruceFeature.Store();
            }

            while (cursorMallaFeature != null)
            {
                // Verificar sin nombre
                String nombreVia = Convert.ToString(cursorMallaFeature.Value[cursorMallaFeature.Fields.FindField("NOMBREVIA")]);

                if (!firstFeature)
                {
                    cursorMallaFeature.Value[viaEditada.Fields.FindField("CODIGOSEGMENTOVIA")] = null;
                    callejero.calcularCodigoSegmentoVia(cursorMallaFeature, Global.mallaVialLayer as IFeatureLayer);
                } else
                {
                    firstFeature = false;
                }

                if (nombreVia == "SIN NOMBRE")
                {
                    cursorMallaFeature.Value[cursorMallaFeature.Fields.FindField("CODIGOVIA")] = null;
                    calcularCodigoVia(cursorMallaFeature, Global.mallaVialLayer as IFeatureLayer);                    
                }
                else
                {
                    cursorMallaFeature.Value[cursorMallaFeature.Fields.FindField("CODIGOVIA")] = codigoViaMalla;
                    calcularCodigoVia(cursorMallaFeature, Global.mallaVialLayer as IFeatureLayer);
                }
                                
                callejero.calcularUbigeos(cursorMallaFeature, Global.distritoLayer as IFeatureLayer);
                cursorMallaFeature.Value[viaEditada.Fields.FindField("USUARIOMODIFICACION")] = Global.nombreUsuario.ToUpper();
                cursorMallaFeature.Value[viaEditada.Fields.FindField("FECHAMODIFICACION")] = Global.fechaModificacionLocal;
                Global.creationStatus = true;
                cursorMallaFeature.Store();
                Global.creationStatus = false;
                callejero.insertarCruceMallaVial(cursorMallaFeature, Global.cruceMallaVialLayer);

                cursorMallaFeature = cursorMalla.NextFeature();
            }
        }

        public void calculateChanges(IFeature via) // Probar eliminar cruce e insertar nuevos cruces
        {
            string layerName = (via.Class as IDataset).Name;
            if (string.Equals(layerName, Global.cruceMallaVialName, StringComparison.OrdinalIgnoreCase)) { return; }
            IFeature viaEditada = via;
            string tool = (Convert.ToString(ArcMap.Application.CurrentTool.Name)).ToUpper();
            Debug.WriteLine(tool);

            if (tool == "EDITOR_SPLITTOOL")
            {
                actualizarSegmentosSplit(via);
            }
            else
            {
                int codigoSegmentoViaEditada = Convert.ToInt32(viaEditada.Value[viaEditada.Fields.FindField("CODIGOSEGMENTOVIA")]);
                //calcularCodigoVia(viaEditada, Global.mallaVialLayer as IFeatureLayer);
                String codigoViaMalla = Convert.ToString(viaEditada.Value[viaEditada.Fields.FindField("CODIGOVIA")]);

                IQueryFilter queryCodigoVia = new QueryFilter();
                queryCodigoVia.WhereClause = $"CODIGOVIA = '{codigoViaMalla}'";

                // Crear cursor mallavial
                IFeatureClass mallaVialFeatureClass = (Global.mallaVialLayer as IFeatureLayer).FeatureClass;
                IFeatureCursor cursorMalla = mallaVialFeatureClass.Update(queryCodigoVia, false);
                IFeature cursorMallaFeature = cursorMalla.NextFeature();
                int cursorMallaCount = mallaVialFeatureClass.FeatureCount(queryCodigoVia);

                // Crear cursor crucemallavial
                IFeatureClass cruceMallaVialFeatureClass = (Global.cruceMallaVialLayer as IFeatureLayer).FeatureClass;
                IFeatureCursor cursorCruce = cruceMallaVialFeatureClass.Update(queryCodigoVia, false);
                IFeature cursorCruceFeature = cursorCruce.NextFeature();
                if (cursorCruceFeature != null)
                {
                    cursorCruceFeature.Delete();
                    cursorCruceFeature.Store();
                }
                
                //2.Calcular codigo e insertar
                while (cursorMallaFeature != null)
                {
                    if (codigoSegmentoViaEditada == Convert.ToInt32(cursorMallaFeature.Value[cursorMallaFeature.Fields.FindField("CODIGOSEGMENTOVIA")]))
                    {
                        cursorMallaFeature.Value[cursorMallaFeature.Fields.FindField("USUARIOMODIFICACION")] = Global.nombreUsuario.ToUpper();
                        cursorMallaFeature.Value[cursorMallaFeature.Fields.FindField("FECHAMODIFICACION")] = Global.fechaModificacionLocal;
                        //cursorMallaFeature.Value[cursorMallaFeature.Fields.FindField("MAQUINA")] = Global.nombreMaquina;
                        //cursorMallaFeature.Value[cursorMallaFeature.Fields.FindField("CODIGOVIA")] = null;
                        callejero.calcularUbigeos(cursorMallaFeature, Global.distritoLayer as IFeatureLayer);
                        //calcularCodigoVia(viaEditada, Global.mallaVialLayer as IFeatureLayer);
                        cursorMallaFeature.Value[cursorMallaFeature.Fields.FindField("CODIGOVIA")] = codigoViaMalla;
                    }
                    else
                    {
                        cursorMallaFeature.Value[cursorMallaFeature.Fields.FindField("USUARIOMODIFICACION")] = Global.nombreUsuario.ToUpper();
                        cursorMallaFeature.Value[cursorMallaFeature.Fields.FindField("FECHAMODIFICACION")] = Global.fechaModificacionLocal;
                        cursorMallaFeature.Value[cursorMallaFeature.Fields.FindField("CODIGOVIA")] = codigoViaMalla;
                        callejero.insertarCruceMallaVial(cursorMallaFeature, Global.cruceMallaVialLayer);
                    }
                    Debug.WriteLine(cursorMallaFeature.Value[viaEditada.Fields.FindField("CODIGOVIA")]);
                    cursorMallaFeature = cursorMalla.NextFeature();
                }

                //3 Insertar via editada
                viaEditada.Value[viaEditada.Fields.FindField("CODIGOVIA")] = null;
                calcularCodigoVia(viaEditada, Global.mallaVialLayer as IFeatureLayer);
                callejero.insertarCruceMallaVial(viaEditada, Global.cruceMallaVialLayer);

            }
            
        }

        public void calcularCodigoVia(IFeature nuevaVia, IFeatureLayer capaVialFeatureLayer)
        {
            // variable nombre de via es tipo string: nombreVia
            // Crear seleccion
            ISpatialFilter spatialfilterVias = new SpatialFilter(); // Crear filtro espacial
            spatialfilterVias.Geometry = nuevaVia.Shape as IPolyline;  // Asignar tipo de geometria al filtro espacial
            spatialfilterVias.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects; // Establecer tipo de relacion (intersects)

            // Calcular CODIGOVIA
            IFeatureClass capaVialFeatureClass = capaVialFeatureLayer.FeatureClass;
            IFeatureSelection featureSelectionVias = capaVialFeatureLayer as IFeatureSelection; // Instancia de seleccion
                                                                                                // Agregar a seleccion existente
            featureSelectionVias.SelectFeatures(spatialfilterVias, esriSelectionResultEnum.esriSelectionResultNew, false);
            ISelectionSet2 selectionSetVias = featureSelectionVias.SelectionSet as ISelectionSet2; // Seleccion actual de vias
            ICursor cursorVias = null; // Crear cursor
            selectionSetVias.Search(null, true, out cursorVias); // Asignar SearchCursor a la seleccion actual de la capa Vias
            IFeatureCursor featureCursorVias = (IFeatureCursor)cursorVias; // Establecer cursor para recorrer lista de features           
            IFeature featureSelectedVia = featureCursorVias.NextFeature(); // Recorrer features
            int indexCodigoVia = nuevaVia.Fields.FindField("CODIGOVIA");
            int indexNombreVia = nuevaVia.Fields.FindField("NOMBREVIA");
            string nombreVia = Convert.ToString(nuevaVia.Value[indexNombreVia]);

            while (featureSelectedVia != null)
            {
                if (!DBNull.Value.Equals(featureSelectedVia.Value[indexNombreVia]))
                {
                    if (Convert.ToString(featureSelectedVia.Value[indexNombreVia]) == nombreVia &&
                        Convert.ToString(featureSelectedVia.Value[indexNombreVia]) != "SN" &&
                        Convert.ToString(featureSelectedVia.Value[indexNombreVia]) != "SIN NOMBRE")
                    {
                        nuevaVia.Value[indexCodigoVia] = featureSelectedVia.Value[indexCodigoVia];
                        break;
                    }
                }
                featureSelectedVia = featureCursorVias.NextFeature();
            }

            // En caso de que no encuentre CODIGOVIA en la tabla actual
            if (DBNull.Value.Equals(nuevaVia.Value[indexCodigoVia]) || Convert.ToString(nuevaVia.Value[indexCodigoVia]) == "")
            {
                IQueryFilter queryCodigoVia = new QueryFilter();
                string ubigeoderecha = Convert.ToString(nuevaVia.Value[(nuevaVia.Fields.FindField("UBIGEODERECHA"))]);
                string inicioCodigo = ubigeoderecha.Substring(0, 4);
                queryCodigoVia.WhereClause = $"CODIGOVIA LIKE '{inicioCodigo}%'";
                IFeatureCursor cursorCodigoVia = capaVialFeatureClass.Search(queryCodigoVia, true);
                IFeature featureCodigoVia = cursorCodigoVia.NextFeature();
                int maxCodigoVia = Convert.ToInt32(featureCodigoVia.Value[indexCodigoVia]);
                int codigoViaActual = Convert.ToInt32(featureCodigoVia.Value[indexCodigoVia]);

                while (featureCodigoVia != null)
                {
                    if (featureCodigoVia.Value[indexCodigoVia] != null)
                    {
                        codigoViaActual = Convert.ToInt32(featureCodigoVia.Value[indexCodigoVia]);
                        if (maxCodigoVia <= codigoViaActual)
                        {
                            maxCodigoVia = codigoViaActual;
                        }
                    }
                    featureCodigoVia = cursorCodigoVia.NextFeature();
                }
                int codigoAsignado = maxCodigoVia + 1;
                nuevaVia.Value[indexCodigoVia] = codigoAsignado.ToString();
            }
            string codigoCalculado = Convert.ToString(nuevaVia.Value[indexCodigoVia]);
            featureSelectionVias.Clear();

            // Debug.WriteLine("CODIGOVIA calculado: " + codigoCalculado);
        }

    }

}
