﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
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

    /// <summary>
    /// A construction tool for ArcMap Editor, using shape constructors
    /// </summary>
    public partial class creaCallejero : ESRI.ArcGIS.Desktop.AddIns.Tool, IShapeConstructorTool, ISketchTool
    {
        private IEditor3 m_editor;
        private IEditEvents_Event m_editEvents;
        private IEditEvents5_Event m_editEvents5;
        private IEditSketch3 m_edSketch;
        private IShapeConstructor m_csc;
        /** Variables adicionales mapa para seleccionar la via editada y su capa; 
         * geoprocessor para aplicar selectbySpatialLocation (AnalysisTools, DataManagementTools)
         * */
        IMap mapa;

        public creaCallejero()
        {
            // Get the editor
            m_editor = ArcMap.Editor as IEditor3;
            m_editEvents = m_editor as IEditEvents_Event;
            m_editEvents5 = m_editor as IEditEvents5_Event;
        }

        protected override void OnUpdate()
        {
            // Enabled = ArcMap.Application != null;
            if (m_editor.CurrentTemplate != null)
            {
                ILayer templateLayer =  m_editor.CurrentTemplate.Layer as ILayer;
                IFeatureLayer templateFeatureLayer = templateLayer as IFeatureLayer;
                IFeatureClass templateFeatureClass = templateFeatureLayer.FeatureClass;
                String templateShape = (templateFeatureClass.ShapeType).ToString();
                Enabled = templateShape == "esriGeometryPolyline" && templateFeatureLayer.Name.Contains("MALLAVIAL");
                //// Debug.WriteLine(templateShape);
            }
            else
            {
                Enabled = false;
            }
            //Enabled = m_editor.EditState == esriEditState.esriStateEditing;
        }

        protected override void OnActivate()
        {

            m_edSketch = m_editor as IEditSketch3;
            //It is important that you set the edit task.
            //For example, If you want the editor to create a feature from your sketch geometry, set the current task to createfeature.
            //If you want to handle feature creation yourself based on the sketch geometry, such as point at end of line, then
            //set the current task to null and create your feature in the finish sketch event.
            //IEditTaskSearch editTaskSearch = m_editor as IEditTaskSearch;
            //IEditTask editTask = editTaskSearch.get_TaskByUniqueName("GarciaUI_CreateNewFeatureTask");
            //m_editor.CurrentTask = editTask;

            // Activate a shape constructor based on the current sketch geometry
            if (m_edSketch.GeometryType == esriGeometryType.esriGeometryPoint | m_edSketch.GeometryType == esriGeometryType.esriGeometryMultipoint)
                m_csc = new PointConstructorClass();
            else
                m_csc = new StraightConstructorClass();

            m_csc.Initialize(m_editor);
            m_edSketch.ShapeConstructor = m_csc;
            m_csc.Activate();

            // Setup events
            m_editEvents.OnSketchModified += OnSketchModified;
            m_editEvents5.OnShapeConstructorChanged += OnShapeConstructorChanged;
            m_editEvents.OnSketchFinished += OnSketchFinished;

            // Llenar variables globales
            mapa = ArcMap.Document.FocusMap;
            IActiveView m_focusMap = mapa as IActiveView;
            List<string> listaCapasNombre = new List<string>();
            List<ILayer> capas = new List<ILayer>();
            for (int i = 0; i < mapa.LayerCount; i++)
            {
                if (mapa.get_Layer(i).Name == "MALLAVIAL" || mapa.get_Layer(i).Name == "DISTRITO" || mapa.get_Layer(i).Name == "CRUCEMALLAVIAL")
                {
                    capas.Add(mapa.get_Layer(i));
                    listaCapasNombre.Add(mapa.get_Layer(i).Name);
                }
            }            
            int indexDistrito = listaCapasNombre.FindIndex(a => a.Contains("DISTRITO"));
            int indexCapaVial = listaCapasNombre.FindIndex(a => a.Equals("MALLAVIAL"));
            int indexCruceMallavial = listaCapasNombre.FindIndex(a => a.Contains("CRUCEMALLAVIAL"));
            if (indexDistrito == -1 || indexCapaVial == -1 || indexCruceMallavial == -1)
            {
                MessageBox.Show("NO ES POSIBLE AGREGAR CALLEJERO: verifique que se encuentren cargadas las capas MALLAVIAL, CRUCEMALLAVIAL y DISTRITO en el mapa");
                m_focusMap.Refresh();
                ArcMap.Application.CurrentTool = null;
                return;
            }
            Global.distritoLayer = capas[indexDistrito];
            Global.mallaVialLayer = capas[indexCapaVial];
            Global.cruceMallaVialLayer = capas[indexCruceMallavial];
        }

        protected override bool OnDeactivate()
        {
            m_editEvents.OnSketchModified -= OnSketchModified;
            m_editEvents5.OnShapeConstructorChanged -= OnShapeConstructorChanged;
            m_editEvents.OnSketchFinished -= OnSketchFinished;
            return true;
        }

        protected override void OnDoubleClick()
        {
            if (m_edSketch.Geometry == null)
                return;

            if (Control.ModifierKeys == Keys.Shift)
            {
                // Finish part
                ISketchOperation pso = new SketchOperation();
                pso.MenuString_2 = "Finish Sketch Part";
                pso.Start(m_editor);
                m_edSketch.FinishSketchPart();
                pso.Finish(null);
            }
            else
                m_edSketch.FinishSketch();
        }

        private void OnSketchModified()
        {
            if (IsShapeConstructorOkay(m_csc))
                m_csc.SketchModified();
        }

        private void OnShapeConstructorChanged()
        {
            // Activate a new constructor
            if (m_csc != null)
                m_csc.Deactivate();
            m_csc = null;
            m_csc = m_edSketch.ShapeConstructor;
            if (m_csc != null)
                m_csc.Activate();
        }

        private void OnSketchFinished()
        {
            //TODO: Custom code
            //ESRI.ArcGIS.Geodatabase.IEnumFeature enumFeature=editor.EditSelection;
            //enumFeature.Reset();
            //Objeto mapa que contiene las capas y lista de capas a utilizar

            WinFormNombreTipoVia formNombreTipo = new WinFormNombreTipoVia();
            formNombreTipo.Show();            

        }

        public void validateLayers()
        {
            mapa = ArcMap.Document.FocusMap;
            IActiveView m_focusMap = mapa as IActiveView;
            List<string> listaCapasNombre = new List<string>();
            List<ILayer> capas = new List<ILayer>();
            for (int i = 0; i < mapa.LayerCount; i++)
            {
                if (mapa.get_Layer(i).Name == "MALLAVIAL" || mapa.get_Layer(i).Name == "DISTRITO" || mapa.get_Layer(i).Name == "CRUCEMALLAVIAL")
                {
                    capas.Add(mapa.get_Layer(i));
                    listaCapasNombre.Add(mapa.get_Layer(i).Name);
                }
            }
            int indexDistrito = listaCapasNombre.FindIndex(a => a.Contains("DISTRITO"));
            int indexCapaVial = listaCapasNombre.FindIndex(a => a.Equals("MALLAVIAL"));
            int indexCruceMallavial = listaCapasNombre.FindIndex(a => a.Contains("CRUCEMALLAVIAL"));
            if (indexDistrito == -1 || indexCapaVial == -1 || indexCruceMallavial == -1)
            {
                MessageBox.Show("NO ES POSIBLE AGREGAR CALLEJERO: verifique que se encuentren cargadas las capas MALLAVIAL, CRUCEMALLAVIAL y DISTRITO en el mapa");
                m_focusMap.Refresh();
                ArcMap.Application.CurrentTool = null;
                return;
            }
        }

        public void deleteFeature()
        {
            mapa = ArcMap.Document.FocusMap;
            IActiveView activeView = (IActiveView)mapa; // Vista del mapa
            IFeature viaEnCreacion = m_editor.EditSelection.Next();
            viaEnCreacion.Delete(); // Eliminar via creada
            m_editor.EditSelection.Reset(); // Quitar seleccion
            m_editor.AbortOperation(); // Abortar la operacion de edicion
            activeView.Refresh(); // Refrescar la vista del mapa
        }
        
// ###############################################################################################################
// ###-------------------------------------------- CALCULAR CAMPOS --------------------------------------------###
// ###############################################################################################################
        // Calcular campos en capa MALLAVIAL
        public void calculateAllFields(string nombreCallejero, string tipoCallejero)
        {
            //TODO: Custom code
            
            mapa = ArcMap.Document.FocusMap;
            IActiveView m_focusMap = mapa as IActiveView;
            List<string> listaCapasNombre = new List<string>();
            List<ILayer> capas = new List<ILayer>();
            string mensaje;

            Func<IFeature> getCapas = () =>
            {
                IEnumFeature enumFeature = m_editor.EditSelection;
                enumFeature.Reset();
                IFeature viaEnCreacion = enumFeature.Next();
                string layerName = ((IDataset)viaEnCreacion.Class).Name;
                for (int i = 0; i < mapa.LayerCount; i++)
                {
                    if (mapa.get_Layer(i).Name == layerName || mapa.get_Layer(i).Name == "DISTRITO" || mapa.get_Layer(i).Name == "CRUCEMALLAVIAL")
                    {
                        capas.Add(mapa.get_Layer(i));
                        listaCapasNombre.Add(mapa.get_Layer(i).Name);
                    }
                }
                return viaEnCreacion;
            };
            // Alojar la via en la variable nuevaVia
            IFeature nuevaVia = getCapas();
            int indexDistrito = listaCapasNombre.FindIndex(a => a.Contains("DISTRITO"));
            int indexCapaVial = listaCapasNombre.FindIndex(a => a.Equals(((IDataset)nuevaVia.Class).Name));
            int indexCruceMallavial = listaCapasNombre.FindIndex(a => a.Contains("CRUCEMALLAVIAL"));
            mensaje = String.Join(", ", listaCapasNombre.ToArray());
            // Verificar que las capas esten en el mapa
            if (indexDistrito == -1 || indexCapaVial == -1 || indexCruceMallavial == -1)
            {
                MessageBox.Show("NO ES POSIBLE AGREGAR CALLEJERO: verifique que se encuentren cargadas las capas MALLAVIAL, CRUCEMALLAVIAL y DISTRITO en el mapa");
                nuevaVia.Delete();
                ArcMap.Application.CurrentTool = null;
                m_focusMap.Refresh();
                return;
            }

            // Seleccionar capas y campos de la capa distrito y la capa en edicion (e.g. MALLAVIAL)
            ILayer capaDistrito = capas[indexDistrito];
            IFeatureLayer capaDistritoFeatureLayer = (IFeatureLayer)capaDistrito;
            IFeatureClass capaDistritoFeatureClass = capaDistritoFeatureLayer.FeatureClass;
            IFields capaDistritoFields = capaDistritoFeatureClass.Fields;

            ILayer capaVial = capas[indexCapaVial];
            IFeatureLayer capaVialFeatureLayer = (IFeatureLayer)capaVial;
            IFeatureClass capaVialFeatureClass = capaVialFeatureLayer.FeatureClass;
            IFields capaVialFields = capaVialFeatureClass.Fields;

            //########## Ventana dockable en caso de necesitar
            //UID dockWindowID = new UIDClass();
            //dockWindowID.Value = ThisAddIn.IDs.dockWin;

            //IDockableWindow dockWinReference = ArcMap.DockableWindowManager.GetDockableWindow(dockWindowID);
            //dockWinReference.Dock(esriDockFlags.esriDockFloat);
            //dockWinReference.Show(true);

// ################################## IDENTIFICAR NOMBREDEVIA Y TIPODEVIA ######################################
            string nombreVia = nombreCallejero.ToUpper();
            string tipoVia = tipoCallejero.ToUpper();
            var siglasVias = new Dictionary<string, string>
            {
                {"S/N", "00"},
                {"AL", "01"},
                {"AV", "02"},
                {"BA", "03"},
                {"CR", "04"},
                {"CJ", "05"},
                {"CA", "06"},
                {"GA", "07"},
                {"JR", "08"},
                {"ML", "09"},
                {"OV", "10"},
                {"PJ", "11"},
                {"PL", "12"},
                {"PQ", "13"},
                {"PR", "14"},
                {"PZ", "16"},
                {"PS", "15"},
                {"CM", "17"},
                {"CU", "18"},
                {"SE", "19"},
                {"PU", "20"},
                {"BO", "21"},
                {"RI", "22"},
                {"AU", "23"},
                {"CI", "24"},
                {"VI", "25"}
            };
            string dominioTipoVia;
            siglasVias.TryGetValue(tipoVia, out dominioTipoVia);

            int indexNombreVia = nuevaVia.Fields.FindField("NOMBREVIA");
            nuevaVia.Value[indexNombreVia] = nombreVia;

            int indexTipoVia = nuevaVia.Fields.FindField("TIPOVIA");
            nuevaVia.Value[indexTipoVia] = (dominioTipoVia == null) ? "00" : dominioTipoVia;

// ################################## SELECCIONAR DISTRITOS Y EXTRAER UBIGEOS ##################################

            // En caso de necesitar selectByLocation --> SelectByLocation(capaVialFeatureClass, capaDistrito);

            IActiveView activeView = (IActiveView)mapa; // Vista del mapa
            IPolyline polyline = (IPolyline)nuevaVia.Shape; // Convertir en geometria linea la nuevaVia creada
            IPoint startPointVia = new PointClass();
            polyline.QueryPoint(esriSegmentExtension.esriExtendAtTo, 0, false, startPointVia);
            IPoint endPointVia = new PointClass();
            polyline.QueryPoint(esriSegmentExtension.esriNoExtension, polyline.Length, false, endPointVia);
            IPoint puntoIzquierda = new PointClass();
            IPoint puntoDerecha = new PointClass();

            // Dejar en puntoIzquierda siempre el punto mas a la izquierda del mapa y en puntoDerecha el mas a la derecha
            if (startPointVia.X > endPointVia.X)
            {
                puntoIzquierda = endPointVia;
                puntoDerecha = startPointVia;
            }
            else
            {
                puntoIzquierda = startPointVia;
                puntoDerecha = endPointVia;
            }

            // Crear seleccion
            ISpatialFilter spatialfilter = new SpatialFilter(); // Crear filtro espacial
            spatialfilter.Geometry = puntoIzquierda;  // Asignar tipo de geometria al filtro espacial
            spatialfilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects; // Establecer tipo de relacion (intersects)

            // Calcular UBIGEOIZQUIERDA
            IFeatureSelection featureSelectionDistrito = capaDistritoFeatureLayer as IFeatureSelection; // Instancia de seleccion
                                                                                                        // Agregar a seleccion existente
            featureSelectionDistrito.SelectFeatures(spatialfilter, esriSelectionResultEnum.esriSelectionResultNew, false);
            ISelectionSet2 selectionSetDistrito = featureSelectionDistrito.SelectionSet as ISelectionSet2; // Seleccion actual de distritos
            ICursor cursor = null; // Crear cursor
            selectionSetDistrito.Search(null, true, out cursor); // Asignar SearchCursor a la seleccion actual de la capa Distrito
            IFeatureCursor featureCursorDistrito = (IFeatureCursor)cursor; // Establecer cursor para recorrer lista de efatures
            IFeature featureSelectedDistrito = featureCursorDistrito.NextFeature(); // Recorrer features
            int indexUbigeo = featureCursorDistrito.Fields.FindField("UBIGEO"); // Encontrar la posicion del campo UBIGEO
            string ubigeoizquierda = featureSelectedDistrito.Value[indexUbigeo] as string;

            // Calcular UBIGEODERECHA
            spatialfilter.Geometry = puntoDerecha;  // Asignar tipo de geometria al filtro espacial (endpoint)
            featureSelectionDistrito.SelectFeatures(spatialfilter, esriSelectionResultEnum.esriSelectionResultNew, false);
            selectionSetDistrito = featureSelectionDistrito.SelectionSet as ISelectionSet2; // Seleccion actual de distritos
            cursor = null;
            selectionSetDistrito.Search(null, true, out cursor); // Asignar SearchCursor a la seleccion actual de la capa Distrito
            featureCursorDistrito = (IFeatureCursor)cursor; // Establecer cursor para recorrer lista de efatures
            featureSelectedDistrito = featureCursorDistrito.NextFeature(); // Recorrer features
            string ubigeoderecha = featureSelectedDistrito.Value[indexUbigeo] as string;

            // Dejar seleccionados distritos
            spatialfilter.Geometry = puntoIzquierda;  // Asignar tipo de geometria al filtro espacial
            featureSelectionDistrito.SelectFeatures(spatialfilter, esriSelectionResultEnum.esriSelectionResultNew, false);
            spatialfilter.Geometry = puntoDerecha;  // Asignar tipo de geometria al filtro espacial
            featureSelectionDistrito.SelectFeatures(spatialfilter, esriSelectionResultEnum.esriSelectionResultAdd, false);
            featureSelectionDistrito.Clear();
            activeView.Refresh(); // Refrescar vista del mapa

            // Asignar UBIGEOIZQUIERDA y UBIGEODERECHA a nuevaVia
            int indexUbigeoIzquierda = nuevaVia.Fields.FindField("UBIGEOIZQUIERDA");
            nuevaVia.Value[indexUbigeoIzquierda] = ubigeoizquierda;
            int indexUbigeoDerecha = nuevaVia.Fields.FindField("UBIGEODERECHA");
            nuevaVia.Value[indexUbigeoDerecha] = ubigeoderecha;


// ################################## CALCULAR CODIGOSEGMENTOVIA (MAX registro y sumarle 1) ##################################

            // capaVialFeatureClass feature class de la capa en edicion
            IQueryFilter queryCodigo = new QueryFilter();
            string inicioCodigo = ubigeoderecha.Substring(0, 4);
            queryCodigo.WhereClause = $"CODIGOVIA LIKE '{inicioCodigo}%'";
            int indexCodigoSegmento = nuevaVia.Fields.FindField("CODIGOSEGMENTOVIA");
            IFeatureCursor cursorCodigoSegmento = capaVialFeatureClass.Search(queryCodigo, true);
            IFeature rowViaCodigo = cursorCodigoSegmento.NextFeature();
            // Debug.WriteLine("CODIGOSEGMENTOVIA calculado: " + (Convert.ToInt64(rowViaCodigo.Value[indexCodigoSegmento])));
            Int64 maxCodigoSegmento = Convert.ToInt64(rowViaCodigo.Value[indexCodigoSegmento]);
            Int64 codigoSegmentoActual = Convert.ToInt64(rowViaCodigo.Value[indexCodigoSegmento]);

            while (rowViaCodigo != null)
            {
                if (rowViaCodigo.Value[indexCodigoSegmento] != null)
                {
                    codigoSegmentoActual = Convert.ToInt64(rowViaCodigo.Value[indexCodigoSegmento]);
                    if (maxCodigoSegmento <= codigoSegmentoActual)
                    {
                        maxCodigoSegmento = codigoSegmentoActual;
                    }
                }
                rowViaCodigo = cursorCodigoSegmento.NextFeature();
            }

            nuevaVia.Value[indexCodigoSegmento] = maxCodigoSegmento + 1;


// ################################## CALCULAR CODIGOVIA ##########################################

            // variable nombre de via es tipo string: nombreVia
            // Crear seleccion
            ISpatialFilter spatialfilterVias = new SpatialFilter(); // Crear filtro espacial
            spatialfilterVias.Geometry = polyline;  // Asignar tipo de geometria al filtro espacial
            spatialfilterVias.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects; // Establecer tipo de relacion (intersects)

            // Calcular CODIGOVIA
            IFeatureSelection featureSelectionVias = capaVialFeatureLayer as IFeatureSelection; // Instancia de seleccion
                                                                                                // Agregar a seleccion existente
            featureSelectionVias.SelectFeatures(spatialfilterVias, esriSelectionResultEnum.esriSelectionResultNew, false);
            ISelectionSet2 selectionSetVias = featureSelectionVias.SelectionSet as ISelectionSet2; // Seleccion actual de vias
            ICursor cursorVias = null; // Crear cursor
            selectionSetVias.Search(null, true, out cursorVias); // Asignar SearchCursor a la seleccion actual de la capa Vias
            IFeatureCursor featureCursorVias = (IFeatureCursor)cursorVias; // Establecer cursor para recorrer lista de features           
            IFeature featureSelectedVia = featureCursorVias.NextFeature(); // Recorrer features
            int indexCodigoVia = nuevaVia.Fields.FindField("CODIGOVIA");

            while (featureSelectedVia != null)
            {
                if (!DBNull.Value.Equals(featureSelectedVia.Value[indexNombreVia]))
                {
                    if (Convert.ToString(featureSelectedVia.Value[indexNombreVia]) == nombreVia && Convert.ToString(featureSelectedVia.Value[indexNombreVia]) != "SN")
                    {
                        nuevaVia.Value[indexCodigoVia] = featureSelectedVia.Value[indexCodigoVia];
                        break;
                    }
                }
                featureSelectedVia = featureCursorVias.NextFeature();
            }

            // En caso de que no encuentre CODIGOVIA en la tabla actual
            if (DBNull.Value.Equals(nuevaVia.Value[indexCodigoVia]))
            {
                IQueryFilter queryCodigoVia = new QueryFilter();
                queryCodigoVia.WhereClause = $"CODIGOVIA LIKE '{inicioCodigo}%'";
                IFeatureCursor cursorCodigoVia = capaVialFeatureClass.Search(queryCodigo, true);
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

            // Filtro para seleccionar nueva via creada
            IQueryFilter queryFilterNuevaVia = new QueryFilter();
            queryFilterNuevaVia.WhereClause = $"CODIGOSEGMENTOVIA = {maxCodigoSegmento + 1}";
            
            // Debug.WriteLine("CODIGOVIA calculado: " + codigoCalculado);


// ################################## CALCULAR NOMBRE DE MAQUINA ##################################
            int indexCodigoMaquina = nuevaVia.Fields.FindField("MAQUINA");
            nuevaVia.Value[indexCodigoMaquina] = Global.nombreMaquina;

            // Guardar la nueva via en el featureClass
            nuevaVia.Store();

// ################################## INSERTAR EN CRUCEMALLAVIAL ##################################
            insertarCruceMallaVial(nuevaVia, Global.cruceMallaVialLayer);

            // Seleccionar nuevaVia
            featureSelectionVias.SelectFeatures(queryFilterNuevaVia, esriSelectionResultEnum.esriSelectionResultNew, false);
            activeView.Refresh(); // Refrescar vista del mapa

            // Debug.WriteLine("CODIGOSEGMENTOVIA calculado: " + (maxCodigoSegmento + 1));
            // Debug.WriteLine("Coordenada X Start: " + startPointVia.X);
            // Debug.WriteLine("Coordenada X End: " + endPointVia.X);
            // Debug.WriteLine("Ubigeo derecha: " + ubigeoderecha.ToString());
            // Debug.WriteLine("Ubigeo izquierda: " + ubigeoizquierda.ToString());
            
        }

// ###############################################################################################################
// ###---------------------------------------- INSERTAR CRUCEMALLAVIAL ----------------------------------------###
// ###############################################################################################################
        // Insertar en nueva via en CRUCEMALLAVIAL
        public void insertarCruceMallaVial(IFeature nuevaVia, ILayer cruceLayer)
        {
            // Definir mapa y multipart feature (CRUCEMALLAVIAL ES MULTIPARTE
            mapa = ArcMap.Document.FocusMap;
            IActiveView m_focusMap = mapa as IActiveView;

            // Definir capa y feature class de CRUCEMALLAVIAL
            IFeatureLayer cruceFeatureLayer = cruceLayer as IFeatureLayer;
            IFeatureClass cruceFeatureClass = cruceFeatureLayer.FeatureClass;

            // Crear query de busqueda por inicio de codigo
            int indexCodigoVia = Convert.ToInt32((Global.mallaVialLayer as IFeatureLayer).FeatureClass.Fields.FindField("CODIGOVIA"));
            String codigoVia = Convert.ToString(nuevaVia.Value[indexCodigoVia]);
            string inicioCodigo = codigoVia.Substring(0, 4);
            IQueryFilter queryCodigoVia = new QueryFilter();
            queryCodigoVia.WhereClause = $"CODIGOVIA LIKE '{inicioCodigo}%'";

            // Crear cursor
            IFeatureCursor cursorCruce = cruceFeatureClass.Update(queryCodigoVia, false);
            IFeature cruceFeature = cursorCruce.NextFeature();
            int indexCruceCodigoVia = Convert.ToInt32(cruceFeature.Fields.FindField("CODIGOVIA"));
            String cruceCodigoVia = Convert.ToString(cruceFeature.Value[indexCruceCodigoVia]);

            // Encontrar codigo
            IGeometryCollection segmentosNuevaVia = (nuevaVia.ShapeCopy as IPolyline) as IGeometryCollection;
            IGeometryCollection newShape = new PolylineClass();
            while (cruceFeature != null)
            {
                IGeometryCollection polylineCollection = cruceFeature.ShapeCopy as IGeometryCollection;
                cruceCodigoVia = Convert.ToString(cruceFeature.Value[indexCruceCodigoVia]);
                if (codigoVia == cruceCodigoVia)
                {
                    // Debug.WriteLine((cruceFeature.Shape as IGeometryCollection).GeometryCount);
                    IGeometry currentGeometry;
                    for (int i = 0; i < polylineCollection.GeometryCount; i++)
                    {
                        currentGeometry = polylineCollection.get_Geometry(i);
                        newShape.AddGeometry(currentGeometry);
                    }
                    for (int i = 0; i < segmentosNuevaVia.GeometryCount; i++)
                    {
                        currentGeometry = segmentosNuevaVia.get_Geometry(i);
                        newShape.AddGeometry(currentGeometry);
                        (cruceFeature.Shape as IGeometryCollection).AddGeometry(currentGeometry); /////////////////////////////
                    }
                    // Debug.WriteLine(newShape.GeometryCount);
                    break;
                }
                cruceFeature = cursorCruce.NextFeature();
            }
            if (cruceFeature != null)
            {
                // Debug.WriteLine(cruceFeature.Value[(cruceFeature.Fields.FindField("NOMBREVIA"))]);
                // Debug.WriteLine((cruceFeature.Shape as IGeometryCollection).GeometryCount);
                cruceFeature.Shape = (newShape as IPolyline);
                cruceFeature.Value[cruceFeature.Fields.FindField("MAQUINA")] = Global.nombreMaquina;
                cruceFeature.Store();
            }
            else
            {
                IFeatureCursor insertCursor = cruceFeatureClass.Insert(true);
                IFeatureBuffer newRow = cruceFeatureClass.CreateFeatureBuffer();
                newRow.Value[newRow.Fields.FindField("NOMBREVIA")] = nuevaVia.Value[nuevaVia.Fields.FindField("NOMBREVIA")];
                newRow.Value[newRow.Fields.FindField("TIPOVIA")] = nuevaVia.Value[nuevaVia.Fields.FindField("TIPOVIA")];
                newRow.Value[newRow.Fields.FindField("CODIGOVIA")] = nuevaVia.Value[nuevaVia.Fields.FindField("CODIGOVIA")];
                newRow.Value[newRow.Fields.FindField("MAQUINA")] = Global.nombreMaquina;
                newRow.Shape = nuevaVia.Shape;
                insertCursor.InsertFeature(newRow);
            }

            // Si sale del ciclo no encontro codigo existente
        }

        public static void SelectByLocation(IFeatureClass sourceFC, ILayer selectLayer)
        {
            Geoprocessor GP = new Geoprocessor();
            SelectLayerByLocation selectLoc = new SelectLayerByLocation();
            selectLoc.in_layer = selectLayer;
            selectLoc.select_features = sourceFC;
            selectLoc.selection_type = "NEW_SELECTION";
            RunTool(GP, selectLoc, null);
            ArcMap.Document.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
        }

        private static void RunTool(Geoprocessor geoprocessor, IGPProcess process, ITrackCancel TC)
        {

            // Set the overwrite output option to true
            geoprocessor.OverwriteOutput = true;

            // Execute the tool            
            try
            {
                geoprocessor.Execute(process, null);
                ReturnMessages(geoprocessor);

            }
            catch (Exception err)
            {
                // Debug.WriteLine(err.Message);
                ReturnMessages(geoprocessor);
            }
        }

        // Function for returning the tool messages.
        private static void ReturnMessages(Geoprocessor gp)
        {
            if (gp.MessageCount > 0)
            {
                for (int Count = 0; Count <= gp.MessageCount - 1; Count++)
                {
                    Console.WriteLine(gp.GetMessage(Count));
                }
            }

        }


    }

}