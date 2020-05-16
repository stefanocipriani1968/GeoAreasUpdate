using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using Npgsql;
using System.Configuration;
using Npgsql.TypeHandlers.DateTimeHandlers;
using System.Text.RegularExpressions;
using System.Text;
using System.Globalization;

namespace GeoAreasUpdate
{
    public partial class FormMain : Form
    {
        private static string Host = ConfigurationManager.AppSettings["Host"];
        private static string User = ConfigurationManager.AppSettings["User"];
        private static string DBname = ConfigurationManager.AppSettings["DataBase"];
        private static string Password = ConfigurationManager.AppSettings["Psw"];
        private static string Port = ConfigurationManager.AppSettings["Port"];
        private static string Schema = ConfigurationManager.AppSettings["Schema"];
        private static string Table = ConfigurationManager.AppSettings["Table"];
        
        private String connString;

        private String filePath = string.Empty;

        public TimeSpan time;
        public DateTime start;


        //public StreamWriter log;

        public FormMain()
        {
            InitializeComponent();
            if(Host is null || DBname is null || User is null || Password is null || Schema is null || Table is null || Port is null)
            {
                MessageBox.Show ("Una o più Key del file GeoAreaUpdate.exe.config non sono definite \n" + "Le key da definire sono :  Host,User,DBname,Port,Password,Schema,Table","ATTENZIONE");
                Environment.Exit(Environment.ExitCode);
            }
            connString =
               String.Format(
                   "Server={0};Username={1};Database={2};Port={3};Password={4};SSLMode=Prefer",
                   Host,
                   User,
                   DBname,
                   Port,
                   Password);
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "zip files (*.zip)|*.zip";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    txtFileZip.Text = filePath;

                }
            }


        }

        private void btnElabora_Click(object sender, EventArgs e)
        {

            if (txtFileZip.Text == "")
            {
                return;
            }

            
            start = DateTime.Now; 
            timer1.Enabled = true;

            try
            {
                Directory.Delete(AppContext.BaseDirectory + "Tempfile", true);
            }
            catch (Exception ex)
            {

            }

            Cursor = Cursors.WaitCursor;

            txtLog.Text += "0. Scompattamento file zip in directory temporanea;"+ Environment.NewLine;
            try
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + "Tempfile");
            }
            catch(Exception ex)
            {

            }
            
            progressBar1.Minimum = 0;
            progressBar1.Value = 0;

            ZipArchive zip = ZipFile.OpenRead(filePath);
            progressBar1.Maximum = zip.Entries.Count;

            foreach (ZipArchiveEntry entry in zip.Entries)
            {
                entry.ExtractToFile(AppContext.BaseDirectory + "Tempfile" + "\\" + entry.Name,true);
                progressBar1.Value++;
                Application.DoEvents();
            }

            //ZipFile.ExtractToDirectory(filePath, AppContext.BaseDirectory + "Tempfile");
           
            //progressBar1.Value++;
            //Application.DoEvents();

            // svuoto la tabella import_Zone e importo file csv in import_Zone

            string[] fileEntriesCsv = Directory.GetFiles(AppContext.BaseDirectory + "Tempfile", "*ZONE*.csv");

            if (adjust_import_zone(fileEntriesCsv[0]))
            {
                String timestamp = DateTime.Now.ToString("yyyyMMddhhmmss");
                // faccio una copia di sicurezza della tabella geo_areas
                // in una table di appoggio geo_areas_import_YYYMMDDHHIISS

                Boolean saveTable = SalvaGeoAreas(timestamp);

                if (saveTable)
                {
                    // aggiorno la tabella geo_areas con il nuovi file kml
                    if (deletetype())
                    {

                        string path = AppContext.BaseDirectory + "\\logFile.txt";
                        try
                        {
                            File.Delete(AppContext.BaseDirectory + "\\logFile.txt");
                        }
                        catch (Exception ex)
                        {

                        }
                        //log = File.CreateText(path);
                        //log.WriteLine("");
                        //log.WriteLine("Lista File kml non caricati");
                        //log.WriteLine("");
                        string[] fileEntries = Directory.GetFiles(AppContext.BaseDirectory + "Tempfile", "*.kml");

                        // ciclo su tutti i file
                        txtLog.Text += "5. Inserimento kml in  tabella Geo_Areas;" + Environment.NewLine;

                        progressBar1.Minimum = 0;
                        progressBar1.Value = 0;
                        progressBar1.Maximum = fileEntries.Count();
                        foreach (string fileName in fileEntries)
                        {
                            progressBar1.Value += 1;
                            ProcessFileKml(fileName);
                            Application.DoEvents();
                        }

                        //log.Close();
                    }
                    else
                    {
                        Cursor = Cursors.Default;
                        return;
                    }

                    // Eseguo la query di update fra geo areas e import_table
                    if (!Update_GeoAreas_Zone())
                    {
                        txtLog.Text += "X. Aggiornamento tabella Geo_Areas non RIUSCITO;"  + Environment.NewLine;
                        MessageBox.Show("Processo Abortito", "Aggiornamento non eseguito", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Cursor = Cursors.Default;
                        return;

                    }

                }

                // Elimino la tabella di appoggio geo_areas_import_YYYMMDDHHIISS
                try
                {
                    DropTableSave(timestamp);
                }
                catch (Exception ex)
                {
                    txtLog.Text += "X. Eliminazione tabella temporanea: " + timestamp +" non riuscita;" + Environment.NewLine;

                }

                timer1.Enabled = false;
                Cursor = Cursors.Default;
                MessageBox.Show("Processo Terminato", "Information", MessageBoxButtons.OK);

            }
            else
            {
                timer1.Enabled = false;
                Cursor = Cursors.Default;
                MessageBox.Show("Processo Abortito", "Error", MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }



        }

        public  void ProcessFileKml(string path)
        {
            // leggo il file xml/kml
            string filename = path;
            Update newElementGeo = new Update();


            XDocument doc =null;

            try
            {
                //XDocument xmlDoc = null;

                using (StreamReader oReader = new StreamReader(path, Encoding.GetEncoding("ISO-8859-1")))
                {
                    doc = XDocument.Load(oReader);
                }
                
                
                
                ///////doc = XDocument.Load(path);
            }
            catch (Exception ex)
            {
                //log.WriteLine(path + "File corrotto");
                return;
            }
            XElement root = doc.Root;
            XNamespace ns = root.GetDefaultNamespace();

            String descrDocument = null;
            var descrizione = doc.Element(ns + "kml").Element(ns + "Document").Element(ns + "name");
            descrDocument = descrizione.Value;

            var stili = doc.Element(ns + "kml").Element(ns + "Document").Elements(ns + "Style");



            var query = root
               .Element(ns + "Document")
               .Elements(ns + "Placemark")
               .Select(x => new Placemark
               {
                   Nome = x.Element(ns + "name").Value,
                   Descrizione = x.Element(ns + "description").Value,
                   ExtendedData = x.Element(ns + "ExtendedData").Elements(ns + "Data"),
                   Polygon = x.Descendants(ns + "Polygon"),
                   Style = x.Element(ns + "styleUrl")
                  
               });

            foreach (Placemark place in query)
            {

                String CodiceComune = null;
                String CodiceZona = null;
                String CodiceDescrizione = null;
                String ListaCoordinate = null;
                String CodiceStyle = null;
                var codComune = from codici in place.ExtendedData
                                where codici.Attribute("name").Value == "CODCOM"
                                select codici;

                foreach (XElement el in codComune)
                {
                    CodiceComune = (String)el.Element(ns + "value");
                    CodiceDescrizione = "Comune:" + (String)el.Element(ns + "value");
                }


                var codZona = from codici in place.ExtendedData
                              where codici.Attribute("name").Value == "CODZONA"
                              select codici;

                foreach (XElement el in codZona)
                {
                    CodiceZona = (String)el.Element(ns + "value");
                    CodiceDescrizione += " Zona:" + (String)el.Element(ns + "value");

                }

                CodiceDescrizione += " " + descrDocument;

                foreach (var coord in place.Polygon.Descendants(ns + "outerBoundaryIs").Descendants(ns + "coordinates"))
                {
                    ListaCoordinate += coord.Value;
                }

                var aStyle = from st in stili
                             where st.Attribute("id").Value == place.Style.Value.Replace("#", "")
                             select st;
                foreach (XElement el in aStyle)
                {
                    CodiceStyle = el.Element(ns + "PolyStyle").Element(ns + "color").Value.Substring(2);

                }

                DateTime adesso = DateTime.Now;
                newElementGeo.Record_creation_time = adesso;
                newElementGeo.Record_update_time = adesso;
                newElementGeo.Code = CodiceComune + CodiceZona;
                newElementGeo.Description = CodiceDescrizione;
                
                String[] pezzi = ListaCoordinate.Replace("\n", "").Replace(",0 ", " ").TrimEnd().Split(' ');
                List<String> longitudine = new List<String>();
                List<String> latitudine= new List<String>();
                for (int i=0;i<pezzi.Count();i++)
                {
                    String[] appo = pezzi[i].Split(',');
                    longitudine.Add(appo[0]);
                    latitudine.Add(appo[1]);
                }


                newElementGeo.Point_number = pezzi.Count();
                newElementGeo.Polygon = ListaCoordinate.Replace("\n", "").Replace(",0 ", " ");
                newElementGeo.Last_update_time = adesso;
                newElementGeo.Color = "#" + CodiceStyle;
                newElementGeo.Longitude_min = longitudine.Min();
                newElementGeo.Longitude_max = longitudine.Max();
                newElementGeo.Latitude_min = latitudine.Min();
                newElementGeo.Latitude_max = latitudine.Max();

                if (!Inserttype(newElementGeo))
                {
                    txtLog.Text += "     Inserimento placemark per code:" + newElementGeo.Code + " fallito;\n";
                    Application.DoEvents();

                    //log.WriteLine(path + "Nessun Inserimento per code :" + newElementGeo.Code);
                }
               
            }

        }

        private bool deletetype()
        {
            using (var conn = new NpgsqlConnection(connString))

            {
                conn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                if (Schema == "")
                {
                    cmd.CommandText = "delete  from " + Table + " Where type_id=1";
                }
                else
                {
                    cmd.CommandText = "delete  from " + "\"" + Schema + "\"." + Table + " Where type_id=1";
                }
                cmd.CommandType = CommandType.Text;
                txtLog.Text += "4. Eliminazione righe  tabella Geo_Areas con type_id=1;" + Environment.NewLine;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = 1;
                progressBar1.Value = 0;
                Application.DoEvents();

                try
                {
                    cmd.ExecuteNonQuery();
                    progressBar1.Value++;
                    Application.DoEvents();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Attenzione Query eliminazione Type1 non riuscita ", "Delete not Complited", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cmd.Dispose();
                    conn.Close();
                    return false;
                }
                
                cmd.Dispose();
                conn.Close();

                return true;
            }
        }
        public  bool Inserttype(Update list)
        {

            
            using (var conn = new NpgsqlConnection(connString))

            {
                conn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand();
                cmd.Connection = conn;

               
                if (Schema == "")
                {
                    cmd.CommandText = "Insert Into " + Table + "(record_creation_time,record_update_time,code,description,type_id,latitude_min,longitude_min,latitude_max,longitude_max,point_number,polygon,last_update_time,external_code,color) Values(@record_creation_time,@record_update_time,@code,@description,@type_id,@latitude_min,@longitude_min,@latitude_max,@longitude_max,@point_number,@polygon,@last_update_time,@external_code,@color)";
                }
                else
                {
                    cmd.CommandText = "Insert Into " + "\"" + Schema + "\"." + Table + "(record_creation_time,record_update_time,code,description,type_id,latitude_min,longitude_min,latitude_max,longitude_max,point_number,polygon,last_update_time,external_code,color) Values(@record_creation_time,@record_update_time,@code,@description,@type_id,@latitude_min,@longitude_min,@latitude_max,@longitude_max,@point_number,@polygon,@last_update_time,@external_code,@color)";
                }
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new NpgsqlParameter("@record_creation_time", list.Record_creation_time));
                cmd.Parameters.Add(new NpgsqlParameter("@record_update_time", list.Record_update_time));
                cmd.Parameters.Add(new NpgsqlParameter("@code", list.Code));
                cmd.Parameters.Add(new NpgsqlParameter("@description", list.Description));
                cmd.Parameters.Add(new NpgsqlParameter("@type_id", list.Type_id));
                cmd.Parameters.Add(new NpgsqlParameter("@latitude_min", Convert.ToDouble(list.Latitude_min.Replace(".",","))));
                cmd.Parameters.Add(new NpgsqlParameter("@longitude_min", Convert.ToDouble(list.Longitude_min.Replace(".", ","))));
                cmd.Parameters.Add(new NpgsqlParameter("@latitude_max", Convert.ToDouble(list.Latitude_max.Replace(".", ","))));
                cmd.Parameters.Add(new NpgsqlParameter("@longitude_max", Convert.ToDouble(list.Longitude_max.Replace(".", ","))));
                cmd.Parameters.Add(new NpgsqlParameter("@point_number", list.Point_number));
                cmd.Parameters.Add(new NpgsqlParameter("@polygon", list.Polygon));
                cmd.Parameters.Add(new NpgsqlParameter("@last_update_time", list.Last_update_time));
                cmd.Parameters.Add(new NpgsqlParameter("@external_code", list.External_code));
                cmd.Parameters.Add(new NpgsqlParameter("@color", list.Color));

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Attenzione Query inserimento dati Type1 non riuscita ", "Insert not Complited", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cmd.Dispose();
                    conn.Close();
                    return false;
                }

                cmd.Dispose();
                conn.Close();
                return true;
            }
        }
        private bool adjust_import_zone(string csvfile)
        {
            //Cursor = Cursors.WaitCursor;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                if (Schema == "")
                {
                    cmd.CommandText = "delete  from import_zone ";
                }
                else
                {
                    cmd.CommandText = "delete  from " + "\"" + Schema + "\".import_zone";
                }
                cmd.CommandType = CommandType.Text;

                txtLog.Text += "1. Svuotamento tabella Import_Zone;" + Environment.NewLine;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = 1;
                progressBar1.Value = 0;
                Application.DoEvents();
                try
                {
                    cmd.ExecuteNonQuery();
                    progressBar1.Value++;
                    Application.DoEvents();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Attenzione Query eliminazione table import_zone non riuscita ", "Delete not Complited", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cmd.Dispose();
                    conn.Close();
                    return false;
                }
                
                //importo nuovo file csv in import_Zone

                cmd = new NpgsqlCommand();
                cmd.Connection = conn;

                Int32 numRighe = (Int32)File.ReadAllLines(csvfile).LongLength;
                txtLog.Text += "2. Import file csv in tabella Import_Zone;" + Environment.NewLine;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = numRighe;
                progressBar1.Value = 0;
                Application.DoEvents();

                using (var reader = new StreamReader(csvfile))
                {
                   
                    int countline = 0;
                    while (!reader.EndOfStream)
                    {
                      
                        var line = reader.ReadLine();
                        countline++;
 
                        progressBar1.Value++;
                        Application.DoEvents();


                        if (countline > 2)
                        {
                            var data = line.Split(';');
                            // data[0] data[1]

                            if (Schema == "")
                            {
                                cmd.CommandText = "Insert into import_zone " + " values ('" + toClean(data[0]) + "','" + toClean(data[1]) + "','" + toClean(data[2]) + "','" + toClean(data[3]) + "','" + toClean(data[4]) + "','" + toClean(data[5]) + "','" + toClean(data[6]) + "','" + toClean(data[7]) + "','" + toClean(data[8]) + "','" + toClean(data[9]) + "','" + toClean(data[10]) + "','" + toClean(data[11]) + "','" + toClean(data[12]) + "','" + toClean(data[13]) + "','" + toClean(data[14]) + "','" + toClean(data[15]) + "')";
                            }
                            else
                            {
                                cmd.CommandText = "Insert into " + "\"" + Schema + "\".import_zone" + " values ('" + toClean(data[0]) + "','" + toClean(data[1]) + "','" + toClean(data[2]) + "','" + toClean(data[3]) + "','" + toClean(data[4]) + "','" + toClean(data[5]) + "','" + toClean(data[6]) + "','" + toClean(data[7]) + "','" + toClean(data[8]) + "','" + toClean(data[9]) + "','" + toClean(data[10]) + "','" + toClean(data[11]) + "','" + toClean(data[12]) + "','" + toClean(data[13]) + "','" + toClean(data[14]) + "','" + toClean(data[15]) + "')";
                            }
                            cmd.CommandType = CommandType.Text;
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Attenzione insert csv non riuscito ", "inserimento iesima riga csv", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                cmd.Dispose();
                                conn.Close();
                                return false;
                            }
                        }

                    }
                }
                cmd.Dispose();
                conn.Close();
              
            }
            //Cursor = Cursors.Default;
            return true;
        }

        private Boolean SalvaGeoAreas(String extention)
        {
            using (var conn = new NpgsqlConnection(connString))

            {
                conn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                if (Schema == "")
                {
                    cmd.CommandText = "create table "+ Table + "_import_" + extention + " as select * from " + Table;
                }
                else
                {
                    cmd.CommandText = "create table " + Table + "_import_" + extention + " as select * from " + "\"" + Schema + "\"." + Table;
                }
                cmd.CommandType = CommandType.Text;
                txtLog.Text += "3. Creazione copia tabella Geo_Areas;" + Environment.NewLine;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = 1;
                progressBar1.Value = 0;
                Application.DoEvents();

                try
                {
                    cmd.ExecuteNonQuery();
                    progressBar1.Value++;
                    Application.DoEvents();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Attenzione Copia di sicurezza tabella geo_areas non riuscita", "Save not Complited", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cmd.Dispose();
                    conn.Close();
                    return false;
                }

                cmd.Dispose();
                conn.Close();


                return true;
            }
        }


        private Boolean Update_GeoAreas_Zone()
        {
            using (var conn = new NpgsqlConnection(connString))

            {
                conn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand();
                cmd.Connection = conn;

                txtLog.Text += "6. Aggiornamento tabella Geo_Areas con tabella Import_Zona;" + Environment.NewLine;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = 1;
                progressBar1.Value = 0;
                Application.DoEvents();


                if (Schema == "")
                {
                    cmd.CommandText = "update " + Table;
                    cmd.CommandText += " set external_code = import_zone.linkzona,description = LEFT(CONCAT(" + Table + ".description, ' Descrizione:', import_zone.zona_descr), 200)";
                    cmd.CommandText += " from import_zone ";
                    cmd.CommandText += " where " + Table + ".code = concat(import_zone.comune_amm, import_zone.zona) and " + Table + ".type_id = 1";


                }
                else
                {
                    cmd.CommandText = "update " + "\"" + Schema + "\"." + Table;
                    cmd.CommandText += " set external_code = " + "\"" + Schema + "\"." + "import_zone.linkzona,description = LEFT(CONCAT(" + "\"" + Schema + "\"." + Table + ".description, ' Descrizione:'," + "\"" + Schema + "\"." + "import_zone.zona_descr), 200)";
                    cmd.CommandText += " from " + "\"" + Schema + "\"." + "import_zone ";
                    cmd.CommandText += " where " + "\"" + Schema + "\"." + Table + ".code = concat(" + "\"" + Schema + "\"." + "import_zone.comune_amm," + "\"" + Schema + "\"." + "import_zone.zona) and " + "\"" + Schema + "\"." + Table + ".type_id = 1";
                }
                cmd.CommandType = CommandType.Text;
                try
                {
                    cmd.ExecuteNonQuery();
                    progressBar1.Value++;
                    Application.DoEvents();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Attenzione Update GEO_ARES<-->IMPORT_ZONE non riuscita", "Update not Complited", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cmd.Dispose();
                    conn.Close();
                    return false;
                }

                cmd.Dispose();
                conn.Close();


                return true;
            }

        }
        private Boolean DropTableSave(String extention)
        {
            using (var conn = new NpgsqlConnection(connString))

            {
                conn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                txtLog.Text += "7. Drop Tabella di appoggio " + Table + "_import_" +  extention + ";" + Environment.NewLine;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = 1;
                progressBar1.Value = 0;
                Application.DoEvents();

                if (Schema == "")
                {
                    cmd.CommandText = "DROP Table " + Table + "_import_" + extention ;
                 }
                else
                {
                    cmd.CommandText = "DROP Table " + "\"" + Schema + "\"." + Table + "_import_" + extention;
                }
                cmd.CommandType = CommandType.Text;
                try
                {
                    cmd.ExecuteNonQuery();
                    progressBar1.Value++;
                    Application.DoEvents();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Attenzione Drop tabella di appoggio " + Table + "_import_" + extention + "  non riuscita", "Drop not Complited", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                cmd.Dispose();
                conn.Close();
                return true;
            }
        }

        string toClean(string pTesto)
        {
            pTesto = Regex.Replace(pTesto, "[']", "''");
            pTesto = Regex.Replace(pTesto, "[åàáâãäæ]", "a");
            pTesto = Regex.Replace(pTesto, "[ÁÀÄÃÅÂ]", "A");
            pTesto = Regex.Replace(pTesto, "[èéêë]", "e");
            pTesto = Regex.Replace(pTesto, "[ÈÉËÊ]", "E");
            pTesto = Regex.Replace(pTesto, "[ìíîï]", "i");
            pTesto = Regex.Replace(pTesto, "[ÍÌÏÎ]", "I");
            pTesto = Regex.Replace(pTesto, "[òóôõö]", "o");
            pTesto = Regex.Replace(pTesto, "[ÓÒÖÔÕ]", "O");
            pTesto = Regex.Replace(pTesto, "[ùúûü]", "u");
            pTesto = Regex.Replace(pTesto, "[ÚÙÛÜ]", "U");
            pTesto = Regex.Replace(pTesto, "[¥]", "N");
            pTesto = Regex.Replace(pTesto, "[ý]", "y");
            pTesto = Regex.Replace(pTesto, "[Š]", "S");
            pTesto = Regex.Replace(pTesto, "[š]", "s");
            pTesto = Regex.Replace(pTesto, "[ç]", "c");
            pTesto = Regex.Replace(pTesto, "[ñ]", "n");
            pTesto = Regex.Replace(pTesto, "[Ñ]", "N");
            pTesto = Regex.Replace(pTesto, "[ž]", "z");
            pTesto = Regex.Replace(pTesto, "[[]", "(");
            pTesto = Regex.Replace(pTesto, "[]]", ")");
            pTesto = Regex.Replace(pTesto, "[@]", " ");
            pTesto = Regex.Replace(pTesto, "[#]", " ");
            pTesto = Regex.Replace(pTesto, "[ø]", " ");
            pTesto = Regex.Replace(pTesto, @"[^\u0000-\u007F]", " ");
            return pTesto;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            time = DateTime.Now - start;
            lblTime.Text = time.ToString(@"hh\:mm\:ss");
            Application.DoEvents();

        }
    }
}
